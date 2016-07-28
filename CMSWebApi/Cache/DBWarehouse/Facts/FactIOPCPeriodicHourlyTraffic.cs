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
	internal class FactIOPCPeriodicHourlyTraffic: FactBase<Fact_IOPC_Periodic_Hourly_Traffic, Fact_IOPC_Count>
	{
		protected override Fact_IOPC_Periodic_Hourly_Traffic UpdateFact(IResposity pacdb, Fact_IOPC_Count raw)
		{
			if( raw == null || raw.Dim_IOPC_Monitored_Location == null || !isTrafficCount_In_Out(raw.Dim_IOPC_Monitored_Location.Location) )
				return null;
			ICache<IOPCCountPeriodicCacheModel> icache = BackgroungTask.CacheMgr.Resolve<IOPCCountPeriodicCacheModel>();
			IOPCCountCache iopccache = icache as IOPCCountCache;
			if (iopccache == null || !iopccache.Config.Enable || !iopccache.Config.Live)
				return null;

			Fact_IOPC_Periodic_Hourly_Traffic fact = base.UpdateFact(pacdb, raw);
			if( fact != null)
				iopccache.UpdateCacheFromWareHouse<Fact_IOPC_Periodic_Hourly_Traffic>(fact);

			return fact;
		}
		protected override Fact_IOPC_Periodic_Hourly_Traffic ToFactModel(IResposity pacdb, Fact_IOPC_Count raw, ref bool isnewmodel)
		{
			Fact_IOPC_Periodic_Hourly_Traffic fact = null;
			Dim_IOPC_Monitored_Location locID = GetDimMonitored_Location(raw.MonitoredLocID.Value);
			
			DateTime datekey = raw.DVRDateKey.Value;
			int hour = raw.DVRDate.Value.Hour;
			bool isCout = isCountOUT(locID);
			int pacid = raw.T_PACID;
			IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> facts = GetCurrents( pacdb, pacid, datekey);
			if(!facts.Any())
				goto ADD;
			else
			{
				bool is_any = facts.Any( it => it.C_Hour < hour && it.Normalize == true && it.ReportNormalize == false);
				if( is_any)
				{
					pacdb.DeleteWhere<Fact_IOPC_Periodic_Hourly_Traffic>( it => it.DVRDateKey == datekey && it.PACID == pacid
																				&& it.C_Hour < hour && it.Normalize == true && it.ReportNormalize == false );
				}

				fact = facts.FirstOrDefault(it => it.C_Hour == hour && it.CameraID == raw.C_CameraNumber.Value);
				if( fact == null)
					goto ADD;
				else
					goto UPDATE;
			}
			
			UPDATE:
				isnewmodel = false;
				UpdateFact(fact, raw, isCout);
				goto EXIT;

			ADD:
				isnewmodel = true;
				fact = new Fact_IOPC_Periodic_Hourly_Traffic();
				fact.PACID = raw.T_PACID;
				fact.Normalize = false;
				fact.ReportNormalize = false;
				fact.C_Hour = hour;
				fact.DVRDateKey = datekey;
				fact.CameraID = raw.C_CameraNumber.Value;
				fact.UpdateTimeInt = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();
				if (isCout)
					fact.Count_OUT = (int)raw.C_Count;
				else
					fact.Count_IN = (int)raw.C_Count;

				goto EXIT;

			EXIT:
				return fact;
		}

		private void UpdateFact(Fact_IOPC_Periodic_Hourly_Traffic fact, Fact_IOPC_Count raw, bool isCOUT)
		{
			int cOUT = fact.Count_OUT ?? 0;
			int cIN = fact.Count_IN ?? 0;
			if(fact.Normalize == null || fact.Normalize.Value == false)
			{
				if (isCOUT)
					fact.Count_OUT = cOUT + (int)raw.C_Count;
				else
					fact.Count_IN = cIN + (int)raw.C_Count;
			}
			else
			{
				if (isCOUT)
					fact.Count_OUT = (int)raw.C_Count;
				else
					fact.Count_IN = (int)raw.C_Count;
				fact.Normalize = false;
			}
			fact.UpdateTimeInt = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();

		}
		
		Dim_IOPC_Monitored_Location GetDimMonitored_Location( int locationID)
		{
			Dim_IOPC_Monitored_Location locID = GetDim<Dim_IOPC_Monitored_Location>(it => it.LocationID == locationID && string.Compare( it.SourceName, STR_Count, true) == 0);
			return locID;
		}

		bool isCountOUT(Dim_IOPC_Monitored_Location locid)
		{
			if( locid == null || string.IsNullOrEmpty(locid.Location))
				return false;

			return locid.Location.StartsWith(STR_IOPC_OUT);

		}
	
		private Fact_IOPC_Periodic_Hourly_Traffic GetCurrent( IResposity pacdb, int pacid, DateTime datekey, int hour, int cam)
		{
			return pacdb.FirstOrDefault<Fact_IOPC_Periodic_Hourly_Traffic>( it => it.PACID == pacid && it.DVRDateKey == datekey && it.C_Hour == hour && it.CameraID == cam);
		}
		private IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> GetCurrents(IResposity pacdb, int pacid, DateTime datekey)
		{
			return pacdb.QueryNoTrack<Fact_IOPC_Periodic_Hourly_Traffic>(it => it.PACID == pacid && it.DVRDateKey == datekey);
		}
	}
}
