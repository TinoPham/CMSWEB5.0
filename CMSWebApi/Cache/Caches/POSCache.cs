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
using System.Data.Entity;
using CMSWebApi.Cache.EntityCaches;
using System.Linq.Expressions;
namespace CMSWebApi.Cache.Caches
{
	internal class POSCache : CacheBase<POSPeriodicCacheModel, IPOSService>
	{

		public POSCache(CancellationToken canceltoken, DashboardCacheConfig config, string datadir)
			: base(canceltoken, config, datadir)
		{
			EntityCache<tCMSWeb_Caches>();
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
			bool ret = base.Load(filesupport);
			DateTime Maxdate = MaxConfigDate();
			DateTime Mindate = MinConfigDate(Maxdate);
			MinTime = (int)Mindate.DateToUnixTimestamp();
			MaxTime = (int)Maxdate.DateToUnixTimestamp();
			_status = (byte)CacheStatus.Loading;
			if (filesupport && LoadCacheFile() == CacheStatus.Loading)
			{
				IEnumerable<tCMSWeb_Caches> cahces = EntityCache<tCMSWeb_Caches>();
				if (cahces != null && cahces.Any())
				{
					tCMSWeb_Caches item = cahces.FirstOrDefault(it => string.Compare(it.CacheName, CMSWebApi.Cache.Defines.POS_Cache_Name, true) == 0);
					if (item != null && item.UpdateTimeInt.HasValue && item.UpdateTimeInt.Value > LastCacheTime)
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

		public override bool UpdateCacheFromDB(int timeUpdate)
		{
			bool ret = base.UpdateCacheFromDB(timeUpdate);

			Action act = () =>
			{
				CachebyUpdatetime(it=> it.UpdateTimeInt == timeUpdate, true);
				CleanCache();
			};

			AddItemTask(act);
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

		protected override CacheStatus DBLoad()
		{
			CacheStatus ret = base.DBLoad();

			DateTime Todate = MaxConfigDate();
			DateTime Fromdate = MinConfigDate(Todate);
			IQueryable<Fact_POS_Periodic_Hourly_Transact> result = dbService.Fact_POS_Periodic_Hourly_Transact<Fact_POS_Periodic_Hourly_Transact>(Fromdate, Todate, null, it => it); //dbService.CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(Fromdate, Todate);
			Task<List<Fact_POS_Periodic_Hourly_Transact>> Tresult = result.ToListAsync(CancelToken);
			ret = UpdatetoCache(Tresult, false);
			if (ret == CacheStatus.Ready && Tresult.IsCompleted && Tresult.Result != null && Tresult.Result.Any())
			{
				LastCacheTime = Tresult.Result.Max(it => it.UpdateTimeInt.HasValue ? it.UpdateTimeInt.Value : 0);
				Update_Min_Max_Time();
			}
			return ret;

		}

		protected override POSPeriodicCacheModel toCachemodel<Tin>(Tin model)
		{
			POSPeriodicCacheModel ret = base.toCachemodel<Tin>(model);
			Fact_POS_Periodic_Hourly_Transact dbmodel = model as Fact_POS_Periodic_Hourly_Transact;
			ret.TotalTrans = (ushort?)dbmodel.Count_Trans;
			ret.TotalAmount = dbmodel.TotalAmount;
			ret.NTotalTrans =(ushort?) dbmodel.Count_Trans_N;
			ret.NTotalAmount = dbmodel.TotalAmount_N;
			ret.PACID = dbmodel.PACID;
			ret.Hour = (byte)dbmodel.TransHour;
			ret.DVRDate = (int)dbmodel.DVRDateKey.DateToUnixTimestamp();
			ret.SetNormalize( dbmodel.Normalize.HasValue? dbmodel.Normalize.Value : false, dbmodel.ReportNormalize.HasValue? dbmodel.ReportNormalize.Value : false); 
			return ret;
		}

		private void CachebyUpdatetime(Expression<Func<Fact_POS_Periodic_Hourly_Transact, bool>> combine, bool removeduplicate = false)
		{
			DateTime Todate = MaxConfigDate(); //DateTime.Now;
			DateTime Fromdate = MinConfigDate(Todate); //Config.PeriodDate(Todate);

			IQueryable<Fact_POS_Periodic_Hourly_Transact> result = dbService.Fact_POS_Periodic_Hourly_Transact<Fact_POS_Periodic_Hourly_Transact>(Fromdate, Todate, null,combine, it => it); //dbService.CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(Fromdate, Todate);

			Task<List<Fact_POS_Periodic_Hourly_Transact>> Tresult = result.ToListAsync(CancelToken);
			UpdatetoCache(Tresult, removeduplicate);
		}

		private void RemoveDupplicate(List<Fact_POS_Periodic_Hourly_Transact> newitems)
		{
			if (CacheData.Count <= POSCache.Segment)
			{
				var del_items = CacheData.Join(newitems, cit => new { PACID = cit.PACID, DVRDate = cit.DVRDate }
													, dbit => new { PACID = dbit.PACID, DVRDate = (int)dbit.DVRDateKey.DateToUnixTimestamp() }
													, (cit, dbit) => cit);
				while (del_items.Any())
					RemoveItem(del_items.First());
					
			}
			else
			{
				var del_items = CacheData.AsParallel().Join(newitems.AsParallel(), cit => new { PACID = cit.PACID, DVRDate = cit.DVRDate }
																, dbit => new { PACID = dbit.PACID, DVRDate = (int)dbit.DVRDateKey.DateToUnixTimestamp() }
																, (cit, dbit) => cit);
				if( del_items.Any())
					del_items.ForAll( it => RemoveItem(it));
			}

		}
		private void RemoveDupplicate(List<POSPeriodicCacheModel> newitems)
		{
			if (newitems.Count <= POSCache.Segment)
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

		private CacheStatus UpdatetoCache(Task<List<Fact_POS_Periodic_Hourly_Transact>> TList, bool removeduplicate = false)
		{
			CacheStatus ret = CacheStatus.Not_ready;

			Task<bool> TRet = TList.ContinueWith(tsk =>
			{
				if (tsk.IsFaulted || tsk.IsCanceled)
					return false;
				List<Fact_POS_Periodic_Hourly_Transact> items = tsk.Result;
				if (removeduplicate)
				{
					RemoveDupplicate( items);
					Update_Min_Max_Time();
				}

				if (AddItems<Fact_POS_Periodic_Hourly_Transact>(items, toCachemodel<Fact_POS_Periodic_Hourly_Transact>))
				{
					return true;
				}
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
			if( fact == null || !( fact is Fact_POS_Periodic_Hourly_Transact))
				return false;
			Fact_POS_Periodic_Hourly_Transact tfact = fact as Fact_POS_Periodic_Hourly_Transact;
			if( (int)tfact.DVRDateKey.DateToUnixTimestamp()< MinTime)
				return true;
			bool ret = false;
			IEnumerable<POSPeriodicCacheModel> valids = Query<POSPeriodicCacheModel>( it => it.DVRDate == tfact.DVRDateKey.DateToUnixTimestamp() && it.PACID == tfact.PACID, it => it);
			POSPeriodicCacheModel item = null;
			if(!valids.Any())// new data
				goto ADD;
			
			else
			{
				int count = valids.Count( it => it.Normalize == true && it.ReportNormalize == false);
				if( count == valids.Count())// all data is normalize => delete all normalize before adding new
					goto DELETE;
				else
				{
					item = valids.FirstOrDefault( it => it.Hour == (byte)tfact.TransHour );
					if( item == null )// not valid item in HOUR
						goto ADD;
					else
						goto UPDATE;
				}

			}

			DELETE:
				RemoveDupplicate( valids.ToList());
				goto ADD;

			UPDATE:
				item.TotalTrans = (ushort?)tfact.Count_Trans;
				item.TotalAmount = tfact.TotalAmount;
				item.NTotalTrans = (ushort?)tfact.Count_Trans_N;
				item.NTotalAmount = tfact.TotalAmount_N;
				item.SetNormalize( tfact.Normalize ?? false , tfact.ReportNormalize?? false);
				ret = true;
				goto EXIT;

			ADD:
					item = new POSPeriodicCacheModel
					{
						PACID = tfact.PACID,
						TotalTrans = 1,
						TotalAmount = tfact.TotalAmount,
						Hour = (byte)tfact.TransHour,
						DVRDate = (int)tfact.DVRDateKey.DateToUnixTimestamp()
					};
					item.SetNormalize(false, false);
					ret = AddItem(item);
					goto EXIT;

			EXIT:
			return ret;
		}
		
		//private POSPeriodicCacheModel FromPOSTransact( tbl_POS_Transact transact)
		//{
		//	if( transact == null )
		//		return null;
		//	POSPeriodicCacheModel cacheItem = new POSPeriodicCacheModel();
		//	cacheItem.DVRDate = (int)transact.DVRDate.Value.DateToUnixTimestamp();
		//	cacheItem.Hour = (byte)transact.DVRDate.Value.Hour;
		//	cacheItem.SetNormalize(false, false);
		//	cacheItem.TotalAmount = transact.T_6TotalAmount;
		//	cacheItem.TotalTrans = 1;
		//}
		//private void UnpdatePACID(tbl_POS_Transact trans, POSPeriodicCacheModel cacheItem)
		//{
		//	IEnumerable<Dim_POS_PACID> pacids = ResigterEntityCache
		//}
		

		
	}
}
