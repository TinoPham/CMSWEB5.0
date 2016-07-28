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
namespace CMSWebApi.Cache.Caches
{
	internal class AlertsCache: CacheBase<AlertCacheModel, IAlertService>
	{
		private Dictionary<byte, byte> Alt_Serveriry;
		public AlertsCache(CancellationToken canceltoken, DashboardCacheConfig config, string datadir)
			: base(canceltoken, config, datadir)
		{
			Alt_Serveriry = new Dictionary<byte,byte>();
		}

		private void LoadAlertServerity()
		{
			var ret = dbService.GetAlertTypes(null, it => new { KAlertType = it.KAlertType, KAlertSeverity = it.KAlertSeverity }, null);
			if(ret == null || !ret.Any())
				return;
			ret.ToList().ForEach(it => Alt_Serveriry.Add(it.KAlertType, it.KAlertSeverity));
		}


		protected override CacheStatus DBLoad()
		{
			CacheStatus ret = base.DBLoad();

			DateTime Todate = DateTime.UtcNow;
			DateTime Fromdate = Config.PeriodDate(Todate);
			Task<List<CMSWeb_Cache_ALert_Result>> Tresult = dbService.CMSWeb_Cache_ALert(Fromdate, Todate);
			Task<bool> TRet = Tresult.ContinueWith(tsk =>
				{
					if (tsk.IsFaulted || tsk.IsCanceled)
						return false;

					List<CMSWeb_Cache_ALert_Result> dbCaches = tsk.Result;
					if( dbCaches == null || !dbCaches.Any())
						return false;
					
					bool add_result = AddItems<CMSWeb_Cache_ALert_Result>(dbCaches, toCachemodel<CMSWeb_Cache_ALert_Result>);
					return add_result;
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
			//List<CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Result> items = await Tresult;

			if( TRet.IsCompleted && TRet.Result == true)
				ret = CacheStatus.Ready;
			else
				ret = CacheStatus.Not_ready;
			return ret;
		}

		protected override void Update_Min_Max_Time()
		{
			if(!CacheData.Any())
				return;
			MinTime = CacheData.Min( it => it.Time);
			MaxTime = CacheData.Max( it => it.Time);
			LastCacheTime = CacheData.Max( it => it.KAlertEvent);
		}
		
		protected override DateTime MinConfigDate( DateTime datetime)
		{
			return Config.PeriodDate(datetime);
		}
		
		protected override DateTime MaxConfigDate()
		{
			return DateTime.UtcNow;
		}
		
		protected override void CleanCache( bool force = false)
		{
			int mintime = 0;
			mintime = (int)MinConfigDate(DateTime.UtcNow).DateToUnixTimestamp();
			if(!force && mintime <= MinTime)
				return;

			AddItemTask( () => {
									RemoveItems( it => it.Time < mintime );
									Update_Min_Max_Time();
								});
		}
		
		private AlertCacheModel AlerttoCachemodel(tAlertEvent alt)
		{
			AlertCacheModel ret = new AlertCacheModel();
			
			ret.KAlertEvent = alt.KAlertEvent;
			ret.KAlertSeverity = Alt_Serveriry[alt.KAlertType];
			ret.KAlertType = alt.KAlertType;
			ret.KDVR = !alt.KDVR.HasValue? 0 : alt.KDVR.Value ;
			ret.Time = (int)alt.Time.FullDateTimeToUnixTimestamp();
			return ret;
			
		}

		private void UpdateCacheFromLastAlert(int lastalt)
		{
			if (Alt_Serveriry == null || !Alt_Serveriry.Any())
				return;
			var alts = dbService.GetLastALerts(lastalt, null, null, alt => new { KAlertEvent = alt.KAlertEvent, KDVR = alt.KDVR, KAlertType = alt.KAlertType, Time = alt.Time }, null).ToList();

			IEnumerable<AlertCacheModel> altcaches = alts.Select(alt => {
																			return
																				new AlertCacheModel
																				{
																					KAlertEvent = alt.KAlertEvent,
																					KAlertSeverity = Alt_Serveriry [alt.KAlertType],
																					KAlertType = alt.KAlertType,
																					KDVR = alt.KDVR.HasValue ? alt.KDVR.Value : 0,
																					Time = (int)alt.Time.FullDateTimeToUnixTimestamp()
																				};
																		});
			if (AddItems(altcaches.ToList()))
					Update_Min_Max_Time();

		}
		
		protected override AlertCacheModel toCachemodel<Tin>(Tin model)
		{
			AlertCacheModel ret = base.toCachemodel<Tin>(model);
			CMSWeb_Cache_ALert_Result dbmodel = model as CMSWeb_Cache_ALert_Result;
			ret.KAlertEvent = dbmodel.KAlertEvent;
			ret.KAlertSeverity = dbmodel.KAlertSeverity;
			ret.KAlertType = dbmodel.KAlertType;
			ret.KDVR = dbmodel.KDVR.HasValue? dbmodel.KDVR.Value : 0;
			ret.Time = (int)dbmodel.Time.FullDateTimeToUnixTimestamp();
			return ret;
		}

		public override bool Load(bool filesupport)
		{
			bool ret = base.Load(filesupport);
			_status = (byte)CacheStatus.Loading;

			DateTime Maxdate = MaxConfigDate();
			DateTime mindate = MinConfigDate(Maxdate);

			MinTime = (int)mindate.DateToUnixTimestamp();
			MaxTime = (int)Maxdate.DateToUnixTimestamp();

			if (filesupport && LoadCacheFile() == CacheStatus.Loading)
			{
				Update_Min_Max_Time();
				UpdateCacheFromLastAlert(LastCacheTime);
				CleanCache(true);

				_status = (byte)CacheStatus.Ready;

				return true;
			}

			_status = (byte)DBLoad();
			Update_Min_Max_Time();
			return ret;
		}

		public override bool ValidData(DateTime sdate, DateTime edate)
		{
			bool ret = base.ValidData(sdate, edate);

			int min = (int)sdate.FullDateTimeToUnixTimestamp();
			int max = (int)edate.FullDateTimeToUnixTimestamp();
			
			return min >= MinTime & max <= MaxTime;

		}
		
		public override bool PreLoad()
		{
			LoadAlertServerity();
			return base.PreLoad();
		}

		//public override Task Add<Tdata>(IList<Tdata> collection)
		//{
		//	return Add(collection as IList<tAlertEvent>);
		//}
		//public override Task Add<Tdata>(Tdata value)
		//{
		//	return Add( value as tAlertEvent);
		//}

		public bool Add(tAlertEvent value)
		{
			if (!Config.Live)
				return true;

			if (!Alt_Serveriry.Any() || value == null)
				return false;
			
			int new_time = (int)value.Time.FullDateTimeToUnixTimestamp();
			if( new_time < MinTime)
				return true;

			bool ret = AddItem<tAlertEvent>(value, AlerttoCachemodel);
			if (!ret)
				return false;
			if (MaxTime < value.Time.FullDateTimeToUnixTimestamp())
				MaxTime = (int)value.Time.FullDateTimeToUnixTimestamp();
			CleanCache();
			return true;


			//Action act = () =>
			//{
			//	bool ret =  AddItem<tAlertEvent>(value, AlerttoCachemodel);
			//	if(!ret)
			//		return;
			//	if(MaxTime < value.Time.FullDateTimeToUnixTimestamp())
			//		MaxTime = (int)value.Time.FullDateTimeToUnixTimestamp();
			//	CleanCache();
			//};
			
			//return base.AddItemTask(act);
		}
		
		public bool Add(IList<tAlertEvent> values)
		{
			if (!Config.Live)
				return true;

			if (!Alt_Serveriry.Any() || values == null)
				return false;
			
			IEnumerable<tAlertEvent> valids = values.Where(it => (int)it.Time.FullDateTimeToUnixTimestamp() >= MinTime);
			if(!valids.Any())
				return true;

			bool ret = AddItems<tAlertEvent>(valids.ToList(), AlerttoCachemodel);
			if (ret)
			{
				Update_Min_Max_Time();
				CleanCache(false);
			}
			return ret;
			//Action act = () =>
			//{
			//	IEnumerable<tAlertEvent> valids = values.Where(it => (int)it.Time.FullDateTimeToUnixTimestamp() >= MinTime);

			//	bool ret = !valids.Any()? false : AddItems<tAlertEvent>(valids.ToList(), AlerttoCachemodel);
			//	if (ret)
			//	{
			//		Update_Min_Max_Time();
			//		CleanCache(false);
			//	}
				
			//};

			//return base.AddItemTask(act);
		}

		public override bool UpdateCacheFromDB(int timeUpdate)
		{
			bool ret = base.UpdateCacheFromDB( timeUpdate);
			AddItemTask(() => UpdateCacheFromLastAlert(timeUpdate));
			return ret;
		}
		
	}
}
