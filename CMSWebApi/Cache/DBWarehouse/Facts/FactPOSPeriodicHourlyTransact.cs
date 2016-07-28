using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using Extensions;
using CMSWebApi.Cache.Caches;
using CMSWebApi.DataModels.DashBoardCache;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
	internal class FactPOSPeriodicHourlyTransact: FactBase<Fact_POS_Periodic_Hourly_Transact, Fact_POS_Transact>
	{
		protected override Fact_POS_Periodic_Hourly_Transact UpdateFact(IResposity pacdb, Fact_POS_Transact raw)
		{
			ICache<POSPeriodicCacheModel> icache = BackgroungTask.CacheMgr.Resolve<POSPeriodicCacheModel>();
			POSCache poscache = icache as POSCache;
			if( poscache == null || !poscache.Config.Enable || !poscache.Config.Live)
				return null;

			Fact_POS_Periodic_Hourly_Transact fact = base.UpdateFact(pacdb, raw);
			if (fact != null )
			{
				poscache.UpdateCacheFromWareHouse<Fact_POS_Periodic_Hourly_Transact>( fact);
			}
			return fact;
		}

		protected override Fact_POS_Periodic_Hourly_Transact ToFactModel(IResposity pacdb, Fact_POS_Transact raw, ref bool isnewmodel)
		{
			//Dim_POS_PACID pacid = GetPACIDbyRaw( raw.T_PACID);
			int pacid = raw.T_PACID;
			DateTime datekey = raw.T_DVRDateKey.Value;
			int hour = raw.T_DVRDate.Value.Hour;
			isnewmodel = false;
			Fact_POS_Periodic_Hourly_Transact fact;
			IQueryable<Fact_POS_Periodic_Hourly_Transact> facts = CurrentFacts(pacdb, pacid, datekey);
			if(!facts.Any())//add new Item
				goto ADD;
			else
			{
				bool is_any = facts.Any( it => it.TransHour < hour && it.Normalize == true && it.ReportNormalize == false);
				if (is_any)
					pacdb.DeleteWhere<Fact_POS_Periodic_Hourly_Transact>( it => it.DVRDateKey == datekey && it.PACID == pacid && it.TransHour < hour && it.Normalize == true && it.ReportNormalize == false);
				fact = facts.FirstOrDefault(it => it.TransHour == hour);
				if( fact == null)
					goto ADD;
				else
					goto UPDATE;
			}

			UPDATE:
				UpdateCurrentFact(raw, fact);
				isnewmodel = false;
				goto EXIT;

			ADD:
				fact = ToNewFact(raw, pacid);
				isnewmodel = true;
				goto EXIT;

			EXIT:
			return fact;
		}

		private Fact_POS_Periodic_Hourly_Transact ToNewFact(Fact_POS_Transact trans, int dim_pacid)
		{
			Fact_POS_Periodic_Hourly_Transact newfact = new Fact_POS_Periodic_Hourly_Transact();
			newfact.PACID = dim_pacid;
			newfact.Normalize = false;
			newfact.TotalAmount = !trans.T_6TotalAmount.HasValue ? 0 : trans.T_6TotalAmount.Value;
			newfact.Count_Trans = 1;
			newfact.DVRDateKey = trans.T_DVRDate.Value.Date;
			newfact.TransHour = trans.T_DVRDate.Value.Hour;
			newfact.UpdateTimeInt = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();
			newfact.ReportNormalize = false;
			return newfact;

		}

		private void UpdateCurrentFact(Fact_POS_Transact trans, Fact_POS_Periodic_Hourly_Transact fact)
		{
			if( !fact.Normalize.HasValue || fact.Normalize.Value  == false)
			{
				if( fact.TotalAmount.HasValue)
					fact.TotalAmount += !trans.T_6TotalAmount.HasValue ? 0 : trans.T_6TotalAmount.Value;
				else
					fact.TotalAmount = trans.T_6TotalAmount;
	
				
				if (fact.Count_Trans.HasValue)
					fact.Count_Trans += 1;
				else
					
					fact.Count_Trans = 1;
			}
			else
			{
				fact.Normalize = false;
				fact.Count_Trans = 1;
				fact.TotalAmount =  trans.T_6TotalAmount;

			}
			
			fact.UpdateTimeInt = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();
		}
		
		IQueryable<Fact_POS_Periodic_Hourly_Transact> CurrentFacts(IResposity pacdb, int pacid, DateTime dvrdatekey)
		{
			return pacdb.QueryNoTrack<Fact_POS_Periodic_Hourly_Transact>( it => it.PACID == pacid && it.DVRDateKey == dvrdatekey);
		}

		Fact_POS_Periodic_Hourly_Transact CurrentFact(IResposity pacdb, int pacid, DateTime dvrdatekey, int transhour)
		{
			return pacdb.FirstOrDefault<Fact_POS_Periodic_Hourly_Transact>( it =>it.DVRDateKey == dvrdatekey.Date && it.PACID == pacid && it.TransHour == transhour);
		}

		public Fact_POS_Periodic_Hourly_Transact SummaryInfo(IResposity pacDB, int kdvr, DateTime TransDate, int TransHour, int? Count_Trans, decimal TotalAmount)
		{
			Dim_POS_PACID dim_pacid = base.GetPACIDbyRaw( kdvr);
			if(dim_pacid == null)
				return null;

			DateTime date = TransDate.Date;
			
			Dim_POS_Date dim_date = pacDB.FirstOrDefault< Dim_POS_Date>( it => it.dateKey == date);
			if( dim_date == null)
			{
				dim_date = pacDB.Insert<Dim_POS_Date>(new Dim_POS_Date
				{
					dateKey = date,
					day = date.Day,
					dayOfWeek = date.DayOfWeek.ToString(),
					month = date.Month, monthName = date.LongMonthName(), quarter = date.Quater(), week = date.WeekOfYear(), year = date.Year});
			}
			Fact_POS_Periodic_Hourly_Transact fact = CurrentFact(pacDB, dim_pacid.PACID_ID, date, TransHour); 
			if( fact == null)
			{
				fact = new Fact_POS_Periodic_Hourly_Transact{ DVRDateKey = date, PACID = dim_pacid.PACID_ID,
																 TransHour = TransHour, Count_Trans = Count_Trans, TotalAmount = TotalAmount, Normalize = false, ReportNormalize = false,
																 UpdateTimeInt = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp()};
				pacDB.Insert<Fact_POS_Periodic_Hourly_Transact>(fact);
			}
			else
			{
				fact.Count_Trans = Count_Trans;
				fact.TotalAmount = TotalAmount;
				pacDB.Update<Fact_POS_Periodic_Hourly_Transact>(fact);
			}
			if( pacDB.Save() <= 0)
				return fact;
			try
			{
				ICache<POSPeriodicCacheModel> icache = BackgroungTask.CacheMgr.Resolve<POSPeriodicCacheModel>();
				POSCache poscache = icache as POSCache;
				if( poscache == null || !poscache.Config.Enable || !poscache.Config.Live)
					return null;
				poscache.Delete( it => it.PACID == fact.PACID && it.DVRDate == (int)fact.DVRDateKey.DateToUnixTimestamp() && it.Hour == (byte)fact.TransHour);
			
				poscache.Add( new POSPeriodicCacheModel{ PACID = dim_pacid.PACID_ID, TotalTrans = (ushort?)Count_Trans, TotalAmount = TotalAmount, Hour = (byte)TransHour, DVRDate = (int)dim_date.dateKey.DateToUnixTimestamp() } );
			}
			catch(Exception){ return null;}
			return fact;
		}
	}
}
