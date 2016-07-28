using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Configurations;
using CMSWebApi.DataModels.DashBoardCache;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Threading;
using Extensions;
using System.Collections.Concurrent;
using System.Data.Entity;
using CMSWebApi.Cache.EntityCaches;
using System.Linq.Expressions;

namespace CMSWebApi.Cache.Caches
{
	internal class IOPCCountCache : CacheBase<IOPCCountPeriodicCacheModel, IIOPCService>
	{

		public IOPCCountCache(CancellationToken canceltoken, DashboardCacheConfig config, string datadir)
			: base(canceltoken, config, datadir)
		{
			//ResigterEntityCache<Dim_IOPC_Monitored_Location, PACDMDB>( false);
			//ResigterEntityCache<Dim_IOPC_ObjectType, PACDMDB>(false);
			//EntityCache<tCMSWeb_Caches>();
		}

		protected override void Update_Min_Max_Time()
		{
			if (!CacheData.Any())
				return;
			MinTime = CacheData.Min(it => it.DVRDate);
			MaxTime = CacheData.Max(it => it.DVRDate);
		}
		protected override DateTime MinConfigDate(DateTime datetime)
		{
			return Config.PeriodDate(datetime);
		}

		protected override DateTime MaxConfigDate()
		{
			return DateTime.Now;
		}

		protected override void CleanCache(bool force = false)
		{
			int mintime = 0;
			mintime = (int)MinConfigDate(DateTime.Now).DateToUnixTimestamp();
			if (!force && mintime <= MinTime)
				return;

			AddItemTask(() =>
			{
				RemoveItems(it => it.DVRDate < mintime);
				Update_Min_Max_Time();
			});
		}

		public override bool Load(bool filesupport)
		{
			bool ret = base.Load( filesupport);
			DateTime Maxdate = MaxConfigDate(); //DateTime.Now;
			DateTime Mindate = MinConfigDate(Maxdate); //Config.PeriodDate(Todate);
			MinTime = (int)Mindate.DateToUnixTimestamp();
			MaxTime = (int)Maxdate.DateToUnixTimestamp();
			_status = (byte)CacheStatus.Loading;
			if (filesupport && LoadCacheFile() == CacheStatus.Loading)
			{
				IEnumerable<tCMSWeb_Caches> cahces = EntityCache<tCMSWeb_Caches>();
				if( cahces != null && cahces.Any() )
				{
					tCMSWeb_Caches item = cahces.FirstOrDefault( it => string.Compare(it.CacheName, CMSWebApi.Cache.Defines.IOPCCount_Cache_Name, true) == 0);
					if( item != null && item.UpdateTimeInt.HasValue && item.UpdateTimeInt.Value > LastCacheTime)
					{
						LastCacheTime = item.UpdateTimeInt.Value;
						CachebyUpdatetime(it => it.UpdateTimeInt > LastCacheTime, true);
						
					}
				}
				Update_Min_Max_Time();
				_status = (byte)CacheStatus.Ready;
				CleanCache();
				return true;
			}

			_status = (byte)DBLoad();

			//LastCacheTime = CacheData.Max(it => it.DVRDate);
			return ret;

		}
		public override bool ValidData(DateTime sdate, DateTime edate)
		{
			bool ret = base.ValidData(sdate, edate);
			int min = (int)sdate.DateToUnixTimestamp();
			int max = (int)edate.DateToUnixTimestamp();
			return min >= MinTime & max <= MaxTime;
			//if (CacheData != null && CacheData.Any())
			//{
			//	int min = (int)sdate.DateToUnixTimestamp();
			//	int max = (int)sdate.DateToUnixTimestamp();
			//	int cmin = CacheData.Min(it => it.DVRDate);
			//	int cmax = CacheData.Max(it => it.DVRDate);
			//	return min >= cmin & max <= cmax;

			//}
			//return ret;
		}
		public override bool UpdateCacheFromDB(int timeUpdate)
		{
			bool ret = base.UpdateCacheFromDB(timeUpdate);
			Action act = () => {
								CachebyUpdatetime(it => it.UpdateTimeInt == timeUpdate, true);
								CleanCache();
								};

			AddItemTask(act);
			return ret;
		}
		
		
		protected override CacheStatus DBLoad()
		{
			CacheStatus ret = base.DBLoad();

			DateTime Todate = MaxConfigDate(); //DateTime.Now;
			DateTime Fromdate = MinConfigDate(Todate); //Config.PeriodDate(Todate);
			IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> result = dbService.Fact_IOPC_Periodic_Hourly_Traffic<Fact_IOPC_Periodic_Hourly_Traffic>( Fromdate, Todate, null, it=> it); //dbService.CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(Fromdate, Todate);
			Task<List<Fact_IOPC_Periodic_Hourly_Traffic>> Tresult = result.ToListAsync(base.CancelToken);
			ret =  UpdatetoCache( Tresult, false);
			if( ret == CacheStatus.Ready && Tresult.IsCompleted && Tresult.Result != null && Tresult.Result.Any())
			{
				LastCacheTime = Tresult.Result.Max(it => it.UpdateTimeInt.HasValue ? it.UpdateTimeInt.Value : 0);
				Update_Min_Max_Time();
			}
			return ret;
			
		}
		
		protected override IOPCCountPeriodicCacheModel toCachemodel<Tin>(Tin model)
		{
			IOPCCountPeriodicCacheModel ret = base.toCachemodel<Tin>(model);
			Fact_IOPC_Periodic_Hourly_Traffic dbmodel = model as Fact_IOPC_Periodic_Hourly_Traffic;
			ret.PACID = dbmodel.PACID;
			ret.DVRDate = (int)dbmodel.DVRDateKey.DateToUnixTimestamp();
			ret.C_Hour = (byte)dbmodel.C_Hour;
			ret.CameraID = dbmodel.CameraID;
			ret.In = dbmodel.Count_IN.HasValue? (short)dbmodel.Count_IN.Value : (short?)null;
			ret.Out = dbmodel.Count_OUT.HasValue ? (short)dbmodel.Count_OUT.Value : (short?)null;

			ret.InN = dbmodel.Count_IN_N.HasValue ? (short)dbmodel.Count_IN_N.Value : (short?)null;
			ret.OutN = dbmodel.Count_OUT_N.HasValue ? (short)dbmodel.Count_OUT_N.Value : (short?)null;
			ret.SetNormalize(dbmodel.Normalize.HasValue? dbmodel.Normalize.Value : false , dbmodel.ReportNormalize.HasValue? dbmodel.ReportNormalize.Value : false); 
			//ret.DVRDateHour = (int)dbmodel.DVRDateKey.ToUnixTimestamp(dbmodel.C_Hour);
			return ret;
		}

		private void CachebyUpdatetime(Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, bool>> combine, bool removeduplicate = false)
		{
			DateTime Todate = DateTime.Now;
			DateTime Fromdate = Config.PeriodDate(Todate);

			IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> result = dbService.Fact_IOPC_Periodic_Hourly_Traffic<Fact_IOPC_Periodic_Hourly_Traffic>(Fromdate, Todate, null, combine, it => it); //dbService.CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(Fromdate, Todate);

			Task<List<Fact_IOPC_Periodic_Hourly_Traffic>> Tresult = result.ToListAsync(CancelToken);
			UpdatetoCache(Tresult, removeduplicate);
		}

		private void RemoveDupplicate(List<Fact_IOPC_Periodic_Hourly_Traffic> newitems)
		{
			if( CacheData.Count <=IOPCCountCache.Segment)
			{
				IEnumerable<IOPCCountPeriodicCacheModel> del_items = CacheData.Join(newitems
																						  , cit => new { PACID = cit.PACID, CameraID = cit.CameraID, DVRDateHour = cit.DVRDateHour }
																						 , dbit => new { PACID = dbit.PACID, CameraID = dbit.CameraID, DVRDateHour = dbit.DVRDateKey.ToUnixTimestamp(dbit.C_Hour) }
																						 , (cit, dbit) => cit);
				while(del_items.Any())
					RemoveItem( del_items.First());
				
			}
			else
			{
				var del_items = CacheData.AsParallel().Join(newitems.AsParallel(), cit => new { PACID = cit.PACID, DVRDate = cit.DVRDate }
																, dbit => new { PACID = dbit.PACID, DVRDate = (int)dbit.DVRDateKey.DateToUnixTimestamp() }
																, (cit, dbit) => cit);
				if (del_items.Any())
					del_items.ForAll(it => RemoveItem(it));
			}
		}

		private void RemoveDupplicate(List<IOPCCountPeriodicCacheModel> newitems)
		{
			if (newitems.Count <= IOPCCountCache.Segment)
			{
				while (newitems.Any())
					RemoveItem(newitems.First());

			}
			else
			{
				var del_items = newitems.AsParallel();
				del_items.ForAll(it => RemoveItem(it));
			}
		}
	
		private CacheStatus UpdatetoCache( Task<List<Fact_IOPC_Periodic_Hourly_Traffic>> TList, bool removeduplicate = false) 
		{
			CacheStatus ret = CacheStatus.Not_ready;

			Task<bool> TRet = TList.ContinueWith(tsk =>
			{
				if (tsk.IsFaulted || tsk.IsCanceled)
					return false;
				List<Fact_IOPC_Periodic_Hourly_Traffic> items  = tsk.Result;
				if( removeduplicate )
				{
					RemoveDupplicate( items);
					Update_Min_Max_Time();
				}

				if (AddItems<Fact_IOPC_Periodic_Hourly_Traffic>(items, toCachemodel<Fact_IOPC_Periodic_Hourly_Traffic>))
					return true;
				
				return false;
			}
			, CancelToken);
			try
			{
				ret = CacheStatus.Loading;
				TRet.Wait(CancelToken);
			}
			catch (System.OperationCanceledException) { return CacheStatus.Not_ready; }
			catch (System.ObjectDisposedException) { return CacheStatus.Not_ready; }
			catch (System.AggregateException) { return CacheStatus.Not_ready; }
			if (TRet.IsCompleted && TRet.Result == true)
				ret = CacheStatus.Ready;
			else
				ret = CacheStatus.Not_ready;
			return ret;

		}

		public override bool UpdateCacheFromWareHouse<Tfact>(Tfact fact)
		{
			if (fact == null || !(fact is Fact_IOPC_Periodic_Hourly_Traffic))
				return false;
			Fact_IOPC_Periodic_Hourly_Traffic tfact = fact as Fact_IOPC_Periodic_Hourly_Traffic;
			if ((int)tfact.DVRDateKey.DateToUnixTimestamp() < MinTime)
				return true;
			bool ret = false;
			IEnumerable<IOPCCountPeriodicCacheModel> valids = Query<IOPCCountPeriodicCacheModel>(it => it.DVRDate == tfact.DVRDateKey.DateToUnixTimestamp() && it.PACID == tfact.PACID, it => it);
			IOPCCountPeriodicCacheModel item = null;
			if (!valids.Any())// new data
				goto ADD;
			else
			{
				int count = valids.Count(it => it.Normalize == true && it.ReportNormalize == false);
				if (count == valids.Count())// all data is normalize => delete all normalize before adding new
					goto DELETE;
				else
				{
					item = valids.FirstOrDefault(it => it.C_Hour == (byte)tfact.C_Hour);
					if (item == null)// not valid item in HOUR
						goto ADD;
					else
						goto UPDATE;
				}

			}
		DELETE:
			RemoveDupplicate(valids.ToList());
			goto ADD;

		UPDATE:
			item.In = (short?)tfact.Count_IN;
			item.Out = (short?)tfact.Count_OUT;
			item.InN = (short?)tfact.Count_IN_N;
			item.OutN = (short?)tfact.Count_OUT_N;
			item.SetNormalize(tfact.Normalize ?? false, tfact.ReportNormalize ?? false);
			ret = true;
			goto EXIT;

		ADD:
			item = new IOPCCountPeriodicCacheModel
			{
				PACID = tfact.PACID,
				C_Hour = (byte)tfact.C_Hour,
				CameraID = tfact.CameraID,
				DVRDate = (int)tfact.DVRDateKey.DateToUnixTimestamp(),
				In = (short?)tfact.Count_IN,
				Out = (short?)tfact.Count_OUT,
				InN =(short?)tfact.Count_IN_N,
				OutN = (short?)tfact.Count_OUT_N
			};
			item.SetNormalize(false, false);
			ret = AddItem(item);
			goto EXIT;

			EXIT:
			return ret;

		}

		
	}
}
