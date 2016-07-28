using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using System.Data.Entity;
using CMSWebApi.Cache.Caches;
using CMSWebApi.DataModels.DashBoardCache;
using Extensions;
using System.IO;


namespace CMSWebApi.BusinessServices.ReportBusiness
{
	public partial class ReportBusinessService : BusinessBase<IUsersService>
	{
		
		#region Alert Compares
		/// <summary>
		///Compare number of alerts by 	AlertSeverity
		/// </summary>
		/// <param name="kdvrs">List KDVR</param>
		/// <param name="AlertSeverity">AlertSeverity</param>
		/// <param name="altsdate">Begin alert date time</param>
		/// <param name="altDate">End alert date time</param>
		/// <param name="cmpsdate">Begin compare date time</param>
		/// <param name="cmpDate">End compare datetime</param>
		/// <returns></returns>
		private async Task<ALertCompModel> AlertSeverityComapre(IEnumerable<int> kdvrs, CMSWebApi.Utils.AlertSeverity? AlertSeverity, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate)
		{
			ICache<AlertCacheModel> ICache_alt = base.ResolveCache<AlertCacheModel>(altsdate, altDate);
			ICache<AlertCacheModel> ICache_altcmp = base.ResolveCache<AlertCacheModel>(cmpsdate, cmpDate);
			int alt_count,cmp_count;
			Task<int> Talt_count, Tcmp_count;

			if (ICache_alt == null && ICache_altcmp == null)
			{
				Talt_count = CountAlertbyServerity(altsdate, altDate, kdvrs, AlertSeverity);
				alt_count = await Talt_count;
				Tcmp_count = CountAlertbyServerity(cmpsdate, cmpDate, kdvrs, AlertSeverity);
				cmp_count = await Tcmp_count;
			}
			else
			{
				if( ICache_alt!= null)
					Talt_count = CountAlertbyServerity(ICache_alt, altsdate, altDate, kdvrs, AlertSeverity.HasValue ? new byte [] { (byte)AlertSeverity.Value } : null);
				else
					Talt_count = CountAlertbyServerity(altsdate, altDate, kdvrs, AlertSeverity);

				if (ICache_altcmp != null)
					Tcmp_count = CountAlertbyServerity(ICache_altcmp, cmpsdate, cmpDate, kdvrs, AlertSeverity.HasValue ? new byte [] { (byte)AlertSeverity.Value } : null);
				else
					Tcmp_count = CountAlertbyServerity(cmpsdate, cmpDate, kdvrs, AlertSeverity);

				alt_count = await Talt_count;
				cmp_count = await Tcmp_count;
			}
			
			return new ALertCompModel { Value = alt_count, CmpValue = toValueCompare(alt_count, cmp_count), Increase = (cmp_count == alt_count) ? (bool?)null : (alt_count > cmp_count) };
		}
		
		/// <summary>
		///Compare #Alert by alert type
		/// </summary>
		/// <param name="kdvrs">List KDVR</param>
		/// <param name="alerttype">Alert type</param>
		/// <param name="altsdate">Begin alert date time</param>
		/// <param name="altDate">End alert date time</param>
		/// <param name="cmpsdate">Begin compare date time</param>
		/// <param name="cmpDate">End compare datetime</param>
		/// <returns></returns>
		private async Task<ALertCompModel> AlertTypeComapre(IEnumerable<int> kdvrs, List<int> alerttypes, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate)
		{
			try
			{
				IEnumerable<byte> alttypes = null;
				bool bCountVLoss = false;
				if (alerttypes != null && alerttypes.Any())
				{
					if (alerttypes.FirstOrDefault(x => x == Consts.VIDEOLOSS_ALERTTYPE) == Consts.VIDEOLOSS_ALERTTYPE)
					{
						bCountVLoss = true;
						alttypes = alerttypes.Where(x => x != Consts.VIDEOLOSS_ALERTTYPE).Select(it => (byte)it);
					}
					else
					{
						alttypes = alerttypes.Select(x=>(byte)x);
					}
				}
				ICache<AlertCacheModel> ICache_alt = base.ResolveCache<AlertCacheModel>(altsdate, altDate);
				ICache<AlertCacheModel> ICache_altcmp = base.ResolveCache<AlertCacheModel>(cmpsdate, cmpDate);
				//Task<int> Talt_count, Tcmp_count;
				int alt_count, cmp_count;
				if (ICache_alt == null && ICache_altcmp == null)
				{
					alt_count = await CountAlertbyALertTypes(altsdate, altDate, kdvrs, alttypes);
					//alt_count = await Talt_count;
					if (bCountVLoss)
					{
						IQueryable<int> vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, altsdate, altDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
						alt_count = alt_count + ((vldvrs == null) ? 0 : vldvrs.Count());
					}

					cmp_count = await CountAlertbyALertTypes(cmpsdate, cmpDate, kdvrs, alttypes);
					//cmp_count= await Tcmp_count;
					if (bCountVLoss)
					{
						IQueryable<int> cmp_vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, cmpsdate, cmpDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
						cmp_count = cmp_count + ((cmp_vldvrs == null) ? 0 : cmp_vldvrs.Count());
					}
				}
				else
				{
					int vlcount = 0;
					int cmp_vlcount = 0;

					if (ICache_alt != null)
					{
						alt_count = await CountAlertbyALertTypes(ICache_alt, altsdate, altDate, kdvrs, alttypes);
						if (bCountVLoss)
						{
							vlcount = await CountDVRbyALertTypes(ICache_alt, altsdate, altDate, kdvrs, Consts.VIDEOLOSS_ALERTTYPE);
						}
					}
					else
					{
						alt_count = await CountAlertbyALertTypes(altsdate, altDate, kdvrs, alttypes);
						if (bCountVLoss)
						{
							IQueryable<int> vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, altsdate, altDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
							vlcount = (vldvrs == null) ? 0 : vldvrs.Count();
						}
					}
					//alt_count = await Talt_count;
					alt_count += vlcount;

					if (ICache_altcmp != null)
					{
						cmp_count = await CountAlertbyALertTypes(ICache_altcmp, cmpsdate, cmpDate, kdvrs, alttypes);
						if (bCountVLoss)
						{
							cmp_vlcount = await CountDVRbyALertTypes(ICache_altcmp, cmpsdate, cmpDate, kdvrs, Consts.VIDEOLOSS_ALERTTYPE);
						}
					}
					else
					{
						cmp_count = await CountAlertbyALertTypes(cmpsdate, cmpDate, kdvrs, alttypes);
						if (bCountVLoss)
						{
							IQueryable<int> cmp_vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, cmpsdate, cmpDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
							cmp_vlcount = (cmp_vldvrs == null) ? 0 : cmp_vldvrs.Count();
						}
					}
					//cmp_count = await Tcmp_count;
					cmp_count += cmp_vlcount;
				}
				return new ALertCompModel { Value = alt_count, CmpValue = toValueCompare(alt_count, cmp_count), Increase = (cmp_count == alt_count) ? (bool?)null : (alt_count > cmp_count) };
			}
			catch(Exception)
			{
				return null;
			}
		}
		private async Task<ALertCompModel> AlertTypeComapreByTZ(IEnumerable<int> kdvrs, List<int> alerttypes, DateTime altsdate, DateTime altDate, DateTime cmpsdate, DateTime cmpDate)
		{
			try
			{
				IEnumerable<byte> alttypes = null;
				bool bCountVLoss = false;
				if (alerttypes != null && alerttypes.Any())
				{
					if (alerttypes.FirstOrDefault(x => x == Consts.VIDEOLOSS_ALERTTYPE) == Consts.VIDEOLOSS_ALERTTYPE)
					{
						bCountVLoss = true;
						alttypes = alerttypes.Where(x => x != Consts.VIDEOLOSS_ALERTTYPE).Select(it => (byte)it);
					}
					else
					{
						alttypes = alerttypes.Select(x => (byte)x);
					}
				}
				//ICache<AlertCacheModel> ICache_alt = base.ResolveCache<AlertCacheModel>(altsdate, altDate);
				//ICache<AlertCacheModel> ICache_altcmp = base.ResolveCache<AlertCacheModel>(cmpsdate, cmpDate);
				int alt_count, cmp_count;
				//if (ICache_alt == null && ICache_altcmp == null)
				//{
					alt_count = await CountAlertbyTimeZone(altsdate, altDate, kdvrs, alttypes);
					if (bCountVLoss)
					{
						IQueryable<int> vldvrs = IAlertService.GetAlertsByTimeZone(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, altsdate, altDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
						alt_count = alt_count + ((vldvrs == null) ? 0 : vldvrs.Count());
					}

					cmp_count = await CountAlertbyTimeZone(cmpsdate, cmpDate, kdvrs, alttypes);
					if (bCountVLoss)
					{
						IQueryable<int> cmp_vldvrs = IAlertService.GetAlertsByTimeZone(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, cmpsdate, cmpDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
						cmp_count = cmp_count + ((cmp_vldvrs == null) ? 0 : cmp_vldvrs.Count());
					}
				//}
				//else
				//{
				//	int vlcount = 0;
				//	int cmp_vlcount = 0;

				//	if (ICache_alt != null)
				//	{
				//		alt_count = await CountAlertbyALertTypes(ICache_alt, altsdate, altDate, kdvrs, alttypes);
				//		if (bCountVLoss)
				//		{
				//			vlcount = await CountDVRbyALertTypes(ICache_alt, altsdate, altDate, kdvrs, Consts.VIDEOLOSS_ALERTTYPE);
				//		}
				//	}
				//	else
				//	{
				//		alt_count = await CountAlertbyTimeZone(altsdate, altDate, kdvrs, alttypes);
				//		if (bCountVLoss)
				//		{
				//			IQueryable<int> vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, altsdate, altDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
				//			vlcount = (vldvrs == null) ? 0 : vldvrs.Count();
				//		}
				//	}
				//	alt_count += vlcount;

				//	if (ICache_altcmp != null)
				//	{
				//		cmp_count = await CountAlertbyALertTypes(ICache_altcmp, cmpsdate, cmpDate, kdvrs, alttypes);
				//		if (bCountVLoss)
				//		{
				//			cmp_vlcount = await CountDVRbyALertTypes(ICache_altcmp, cmpsdate, cmpDate, kdvrs, Consts.VIDEOLOSS_ALERTTYPE);
				//		}
				//	}
				//	else
				//	{
				//		cmp_count = await CountAlertbyTimeZone(cmpsdate, cmpDate, kdvrs, alttypes);
				//		if (bCountVLoss)
				//		{
				//			IQueryable<int> cmp_vldvrs = IAlertService.GetAlerts(kdvrs, null, Consts.VIDEOLOSS_ALERTTYPE, cmpsdate, cmpDate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
				//			cmp_vlcount = (cmp_vldvrs == null) ? 0 : cmp_vldvrs.Count();
				//		}
				//	}
				//	cmp_count += cmp_vlcount;
				//}
				return new ALertCompModel { Value = alt_count, CmpValue = toValueCompare(alt_count, cmp_count), Increase = (cmp_count == alt_count) ? (bool?)null : (alt_count > cmp_count) };
			}
			catch (Exception)
			{
				return null;
			}
		}

		private async Task<ALertCompModel> DVR_On_Offline(IEnumerable<int> kdvrs, DateTime pram, bool isoffline, int keepaliveint)
		{
			List<Func_DVR_Offline_Result> offline = await IAlertService.GetDVR_On_Offline_Async( kdvrs, pram, isoffline, 0, keepaliveint); 
			return new ALertCompModel{ Value = offline.Any()? offline.Count() : 0, CmpValue = 0, Increase = false};
		}
		#endregion
		
		#region Alert Charts
		async Task<IQueryable<ChartWithImageModel>> AlertChartMostDVR(DateTime sdate, DateTime edate, IEnumerable<int> sites, int top)
		{
			ICache<AlertCacheModel>ICache = ResolveCache<AlertCacheModel>( sdate, edate);
			if( ICache == null )
			{
				IQueryable<byte> atypes = IAlertService.GetAlertTypes(x => x.KAlertType != Consts.VIDEOLOSS_ALERTTYPE, x => x.KAlertType, null);
				IQueryable<tAlertEvent> alts = IAlertService.GetAlertsbyTypes<tAlertEvent>(sites, atypes, sdate, edate, item => item);
				IQueryable<IGrouping<int, tAlertEvent>> group = alts.GroupBy( alt => alt.KDVR.Value, alt => alt);
				//IQueryable<ChartWithImageModel> icount = group.Select(it => new ChartWithImageModel { Label = it.FirstOrDefault().tDVRAddressBook.ServerID, Value = it.Count(x => x.KAlertType != Consts.VIDEOLOSS_ALERTTYPE) + (it.Any(x => x.KAlertType == Consts.VIDEOLOSS_ALERTTYPE) ? 1 : 0), KDVR = (int)it.Key });
				IQueryable<ChartWithImageModel> icount = group.Select(it => new ChartWithImageModel { Label = it.FirstOrDefault().tDVRAddressBook.ServerID, Value = it.Count(), KDVR = (int)it.Key });
				//return icount.OrderBy( it => it.Value).Take(top);
				List<ChartWithImageModel> lsResult = icount.OrderByDescending( it => it.Value).Take(top).ToList();

				IQueryable<int> vldvrs = IAlertService.GetAlerts(null, Consts.VIDEOLOSS_ALERTTYPE, sdate, edate, x => x.KDVR.HasValue ? x.KDVR.Value : 0, null).Distinct();
				foreach (ChartWithImageModel data in lsResult)
				{
					data.ImageSrc = ReadImageData(data.KDVR);
					if (vldvrs.Contains(data.KDVR))
						data.Value += 1;
				}
				return lsResult.AsQueryable();
			}
			else
			{
				Task<IEnumerable<AlertCacheModel>> T_alts = GetAlertsbyTypes(ICache, sdate, edate, sites, null);
				var dvr_Addressbooks = IDVRService.GetDVRs( it=> sites.Contains(it.KDVR), it => new{ KDVR = it.KDVR, ServerID = it.ServerID}, null);
				IEnumerable<AlertCacheModel> alts = await T_alts;
				var groups_count = alts.GroupBy(alt => alt.KDVR, alt => alt).Select(galt => new { Count = galt.Count(x => x.KAlertType != Consts.VIDEOLOSS_ALERTTYPE) + (galt.Any(x => x.KAlertType == Consts.VIDEOLOSS_ALERTTYPE) ? 1 : 0), KDVR = galt.Key });
				var ret = groups_count.OrderByDescending( g=> g.Count).Take(top);
				//return ret.Join( dvr_Addressbooks, alt => alt.KDVR, dvr => dvr.KDVR, (alt,kdvr) => new ChartWithImageModel{ Value = alt.Count, Label = kdvr.ServerID, KDVR = kdvr.KDVR } ).AsQueryable<ChartWithImageModel>();
				List<ChartWithImageModel> lsResult = ret.Join(dvr_Addressbooks, alt => alt.KDVR, dvr => dvr.KDVR, (alt, kdvr) => new ChartWithImageModel { Value = alt.Count, Label = kdvr.ServerID, KDVR = kdvr.KDVR }).ToList();
				foreach (ChartWithImageModel data in lsResult)
				{
					data.ImageSrc = ReadImageData(data.KDVR);
				}
				return lsResult.AsQueryable();
			}
		}

		async Task<IQueryable<ChartWithImageModel>> AlertChartbyDVR(DateTime sdate, DateTime edate, IEnumerable<int> sites, IEnumerable<byte> altTypes)
		{
			ICache<AlertCacheModel> ICache = ResolveCache<AlertCacheModel>(sdate, edate);
			if( ICache == null )
			{
				IQueryable<tAlertEvent> alts = IAlertService.GetAlertsbyTypes<tAlertEvent>(sites, altTypes, sdate, edate, item => item);
				IQueryable<IGrouping<byte, tAlertEvent>> group = alts.GroupBy(alt => alt.KAlertType, alt => alt);
				return group.Select(item => new ChartWithImageModel { Value = item.GroupBy(alt => alt.KDVR.Value).Count(), Label = item.FirstOrDefault().tAlertType.AlertType });
			}
			else
			{
				Task<IEnumerable<AlertCacheModel>> T_alts = GetAlertsbyTypes(ICache, sdate, edate, sites, altTypes);
				var alt_types = IAlertService.GetAlertTypes(null, it => new { KAlertType = it.KAlertType, AlertType = it.AlertType }, null);
				IEnumerable<AlertCacheModel> alts = await T_alts;
				var groups = alts.GroupBy(alt => alt.KAlertType, alt => alt); //.KDVR
				return groups.Join(alt_types, alt => alt.Key, ty => ty.KAlertType, (alt, ty) => new ChartWithImageModel { Value = alt.GroupBy(x=>x.KDVR).Count(), Label = ty.AlertType }).AsQueryable<ChartWithImageModel>();
			}
		}

		async Task<IQueryable<ChartWithImageModel>> AlertChartbyAlerttype(DateTime sdate, DateTime edate, IEnumerable<byte> alttypes, IEnumerable<int> sites)
		{
			ICache<AlertCacheModel> icache = base.ResolveCache<AlertCacheModel>( sdate, edate);
			if (icache == null)
			{
				return AlertChartbyAlerttype(sites, sdate, edate, alttypes);
			}
			else
			{
				Task<IEnumerable<AlertCacheModel>> T_alts = GetAlertsbyTypes(icache, sdate, edate, sites, alttypes);
				var alt_typeName = IAlertService.GetAlertTypes(it => alttypes.Contains(it.KAlertType), it => new { KAlertType = it.KAlertType, AlertType = it.AlertType }, null);
				IEnumerable<AlertCacheModel> alts = await T_alts;
				IEnumerable<IGrouping<byte, int>> groups = alts.GroupBy(it => it.KAlertType, it => it.KAlertEvent);
				return groups.Join(alt_typeName, alt => alt.Key, type => type.KAlertType, (alt, type) => new ChartWithImageModel { Label = type.AlertType, Value = alt.Count() }).AsQueryable<ChartWithImageModel>();

			}
		}
		async Task<IQueryable<ChartWithImageModel>> AlertChartbyServerity(DateTime sdate, DateTime edate, IEnumerable<byte> altsever, IEnumerable<int> sites)
		{
			ICache<AlertCacheModel> icache = base.ResolveCache<AlertCacheModel>(sdate, edate);
			List<byte> lsSeverity = altsever.ToList();
			List<byte> alttypes = IAlertService.GetAlertTypes<byte>(x => lsSeverity.Contains(x.KAlertSeverity), x => (byte)x.KAlertType, null).ToList();
			if (icache == null)
			{
				return AlertChartbyAlerttype(sites, sdate, edate, alttypes);
			}
			else
			{
				Task<IEnumerable<AlertCacheModel>> T_alts = GetAlertsbyTypes(icache, sdate, edate, sites, alttypes);
				var alt_typeName = IAlertService.GetAlertTypes(it => alttypes.Contains(it.KAlertType), it => new { KAlertType = it.KAlertType, AlertType = it.AlertType }, null);
				IEnumerable<AlertCacheModel> alts = await T_alts;
				IEnumerable<IGrouping<byte, int>> groups = alts.GroupBy(it => it.KAlertType, it => it.KAlertEvent);
				return groups.Join(alt_typeName, alt => alt.Key, type => type.KAlertType, (alt, type) => new ChartWithImageModel { Label = type.AlertType, Value = alt.Count() }).AsQueryable<ChartWithImageModel>();
			}
		}

		IQueryable<ChartWithImageModel> AlertChartbyAlerttype(IEnumerable<int> sites, DateTime sDate, DateTime eDate, IEnumerable<byte> altTypes)
		{
			var alts = IAlertService.GetAlertsbyTypes<tAlertEvent>(sites, altTypes, sDate, eDate, item => item);
			IQueryable<IGrouping<byte, tAlertEvent>> group = alts.GroupBy(alt => alt.KAlertType, alt => alt);
			return GroupAlertType(group);
		}

		IQueryable<ChartWithImageModel> GroupAlertType(IQueryable<IGrouping<byte, tAlertEvent>> altgroups)
		{
			return altgroups.Select(item => new ChartWithImageModel { Value = item.Count(), Label = !item.Any() ? null : item.FirstOrDefault().tAlertType.AlertType });
		}

		async Task<IQueryable<ChartWithImageModel>> OverallStatisticChart(DateTime sdate, DateTime edate, IEnumerable<byte> altTypes, IEnumerable<int> sites, UserContext user, int keepaliveint)
		{
			//DateTime sdate = new DateTime(2015, 4, 1);
			//DateTime edate = new DateTime(2015, 6, 1);
			//IQueryable<tAlertEvent> alts = IAlertService.GetAlertsbyTypes<tAlertEvent>(sites, altTypes, sdate, edate, item => item);
			//IQueryable<IGrouping<byte, tAlertEvent>> group = alts.GroupBy(alt => alt.KAlertType, alt => alt);
			//IQueryable<ColumnChartModel> ResultByAlertType = group.Select(item => new ColumnChartModel { Value = item.GroupBy(alt => alt.KDVR.Value).Count(), Label = item.FirstOrDefault().tAlertType.AlertType });

			Task<IQueryable<ChartWithImageModel>> T_column_chart_alts = AlertChartbyDVR(sdate, edate, sites, altTypes);

			List<ChartWithImageModel> lsResultCustom = new List<ChartWithImageModel>();

			tCMSSystemConfig sysConfig = user.CompanyID > 0 ? ICompanyService.SelectRecordingDay(user.CompanyID) : null;
			int recodingday = sysConfig != null ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;

			Task<int> TRecordLess_count = CountDVRRecordingLess(recodingday, sites); //Consts.ALERT_RECORDING_LESSDAYS
			Task<int> TCountOffline = CountDVROfflineByHour(Consts.ALERT_OFFLINE_HOURS, sites, keepaliveint);
			Task<int> TCountNormalize = CountDVRHasNormalize(sites, TimeZone.CurrentTimeZone.ToLocalTime(sdate), TimeZone.CurrentTimeZone.ToLocalTime(edate));

			int countRecLess = await TRecordLess_count;
			if(countRecLess > 0)
				lsResultCustom.Add(new ChartWithImageModel() { Label = String.Format(Consts.ALERT_RECORDING_LESS_NAME, recodingday), Value = countRecLess });

			int countOffline = await TCountOffline;
			if(countOffline > 0)
				lsResultCustom.Add(new ChartWithImageModel() { Label = String.Format(Consts.ALERT_OFFLINE_NAME, Consts.ALERT_OFFLINE_HOURS), Value = countOffline });

			int countNormalize = await TCountNormalize;
			if(countNormalize > 0)
				lsResultCustom.Add(new ChartWithImageModel() { Label = Consts.ALERT_HAS_NORMALIZE_NAME, Value = countNormalize });

			IQueryable<ChartWithImageModel> ResultByAlertType = await T_column_chart_alts;
			if (ResultByAlertType == null || !ResultByAlertType.Any())
				return lsResultCustom.AsQueryable<ChartWithImageModel>();

			return lsResultCustom.Union<ChartWithImageModel>(ResultByAlertType).AsQueryable<ChartWithImageModel>();
		}
		#endregion

		#region SQL Data queries
		private Task<int> CountALertbyDate( DateTime sdate,DateTime edate, IEnumerable<int> kdvrs)
		{
			IQueryable<int> alerts = IAlertService.GetAlerts<int>(kdvrs, null, null, sdate, edate, item => item.KAlertEvent, null);
			return alerts.CountAsync();
		}

		private Task<int> CountAlertbyALertType(DateTime sdate,DateTime edate, IEnumerable<int> kdvrs, AlertType? alerttype)
		{
			IQueryable<int> alerts = IAlertService.GetAlerts<int>(kdvrs, null, (byte?)alerttype, sdate, edate, item => item.KAlertEvent, null);
			return alerts.CountAsync();
		}

		private Task<int> CountAlertbyALertTypes(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> alerttypes)
		{
			IQueryable<int> alerts = IAlertService.GetAlertsbyTypes<int>(kdvrs, alerttypes, sdate, edate, item => item.KAlertEvent);
			return alerts.CountAsync();
		}

		private Task<int> CountAlertbyServerity(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, AlertSeverity? Serverity)
		{
			IQueryable<int> alerts = IAlertService.GetAlerts<int>(kdvrs, (byte?)Serverity, null, sdate, edate, item => item.KAlertEvent, null);
			return alerts.CountAsync();
		}

		private Task<int> CountAlertbyTimeZone(DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> alerttypes)
		{
			IQueryable<tAlertEvent> alerts = IAlertService.GetAlertsByTimeZone(kdvrs, sdate, edate, alerttypes);
			return alerts.CountAsync();
		}

		private Task<int> CountDVRRecordingLess( int limitday, IEnumerable<int> kdvrs)
		{
			IQueryable<int> rets = kdvrs == null? IDVRService.GetDVRs<int>( item => item.RecordingDay < limitday, item => item.KDVR, null) : IDVRService.GetDVRs<int>( item => item.RecordingDay < limitday && kdvrs.Contains( item.KDVR), item => item.KDVR, null);
			return rets.CountAsync();
		}

		private Task<int> CountDVROfflineByHour(int numHours, IEnumerable<int> kdvrs, int keepaliveint)
		{
			Task<List<Func_DVR_Offline_Result>> T_result =  IAlertService.GetDVR_On_Offline_Async(kdvrs, DateTime.Now, true, numHours, keepaliveint);//UtcNow
			return T_result.ContinueWith<int>( tsk => tsk.Result.Count);

		}
		private Task<int> CountDVRHasNormalize(IEnumerable<int> kdvrs, DateTime sdate, DateTime edate)
		{
			Task <List<Func_DVR_HasNormalize_Result>> normal = IPOSService.Func_DVR_HasNormalize_Async(kdvrs, sdate, edate);
			return normal.ContinueWith<int>(tsk => tsk.Result.Count);
		}
		#endregion

		#region Caches
		IEnumerable<int> Include_CMSAlert( IEnumerable<int> kdvrs, bool include_cmsalert = false)
		{
			if(!include_cmsalert)
				return kdvrs;

			List<int> _kdvr = new List<int>();
			_kdvr.Add( 0);
			if( kdvrs == null || !kdvrs.Any())
				return _kdvr;
				

			return _kdvr.Concat(kdvrs);
		}
		Task<IEnumerable<AlertCacheModel>> GetAlertsbyTypes(ICache<AlertCacheModel> cache, DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> alerttypes, bool include_cmsalert = false)
		{
			int sdate_int = (int)sdate.FullDateTimeToUnixTimestamp();
			int edate_int = (int)edate.FullDateTimeToUnixTimestamp();
			return Task.Run<IEnumerable<AlertCacheModel>>(() =>
			{
				 IEnumerable<int> _kdvrs = Include_CMSAlert(kdvrs, include_cmsalert);
				IEnumerable<AlertCacheModel> alts =cache.Query<AlertCacheModel>(it => it.Time >= sdate_int && it.Time <= edate_int, it => it);
				IEnumerable<AlertCacheModel> alt_dvrs = !_kdvrs.Any() ? alts : alts.Join(_kdvrs, alt => alt.KDVR, kdvr => kdvr, (alt, kdvr) => alt);
				if (alerttypes != null && alerttypes.Any())
					return alt_dvrs.Join(alerttypes, alt => alt.KAlertType, t => t, (alt, t) => alt);

				return alt_dvrs;
			}, System.Threading.CancellationToken.None);

		}

		Task<IEnumerable<AlertCacheModel>> GetAlertsbyServerities(ICache<AlertCacheModel> cache, DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> Serverities, bool include_cmsalert = false)
		{
			int sdate_int = (int)sdate.FullDateTimeToUnixTimestamp();
			int edate_int = (int)edate.FullDateTimeToUnixTimestamp();
			return Task.Run<IEnumerable<AlertCacheModel>>(() =>
			{
				IEnumerable<int> _kdvrs = Include_CMSAlert(kdvrs, include_cmsalert);
				IEnumerable<AlertCacheModel> alts = cache.Query<AlertCacheModel>(it => it.Time >= sdate_int && it.Time <= edate_int, it => it);
				IEnumerable<AlertCacheModel> alt_dvrs = !_kdvrs.Any() ? alts : alts.Join(_kdvrs, alt => alt.KDVR, kdvr => kdvr, (alt, kdvr) => alt);
				if (Serverities != null && Serverities.Any())
					return alt_dvrs.Join(Serverities, alt => alt.KAlertSeverity, t => t, (alt, t) => alt);

				return alt_dvrs;
			}, System.Threading.CancellationToken.None);

		}

		private Task<int> CountAlertbyALertTypes(ICache<AlertCacheModel> cache, DateTime sdate, DateTime edate, IEnumerable<int> kdvrs,IEnumerable<byte> alerttypes)
		{
			Task<IEnumerable<AlertCacheModel>> alts = GetAlertsbyTypes(cache,sdate, edate,kdvrs, alerttypes);
			return alts.ContinueWith<int>( tsk => tsk.Result.Count());

			//int sdate_int = (int)sdate.ToUnixTimestamp();
			//int edate_int = (int)edate.ToUnixTimestamp();
			//return Task.Run<int>( () =>
			//					{
			//						IEnumerable<AlertCacheModel> alts = cache.Query<AlertCacheModel>(it => it.Time >= sdate_int && it.Time <= edate_int, it => it);
			//						IEnumerable<AlertCacheModel> alt_dvrs = alts.Join(kdvrs, alt => alt.KDVR, kdvr => kdvr, (alt, kdvr) => alt);
			//						if(alerttypes != null && alerttypes.Any())
			//							return alt_dvrs.Join(alerttypes, alt => alt.KAlertType, t => t, (alt, t) => alt.KAlertEvent).Count();

			//						return alt_dvrs.Count(); 
			//					}, System.Threading.CancellationToken.None);
		}

		private Task<int> CountAlertbyServerity(ICache<AlertCacheModel> cache, DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, IEnumerable<byte> Serverities)
		{
			Task<IEnumerable<AlertCacheModel>> alts = GetAlertsbyServerities(cache, sdate, edate, kdvrs,Serverities);
			return alts.ContinueWith<int>( tsk =>  tsk.Result.Count());
			//int sdate_int = (int)sdate.ToUnixTimestamp();
			//int edate_int = (int)edate.ToUnixTimestamp();

			//return Task.Run<int>(()=>{

			//						IEnumerable<AlertCacheModel> alts = cache.Query<AlertCacheModel>(it => it.Time >= sdate_int && it.Time <= edate_int, it => it);
			//						IEnumerable<AlertCacheModel> alt_dvrs = alts.Join(kdvrs, alt => alt.KDVR, kdvr => kdvr, (alt, kdvr) => alt);
			//						if( Serverity != null && Serverity.Any())
			//							return alt_dvrs.Join(Serverity, alt => alt.KAlertSeverity, t => t, (alt, t) => alt.KAlertEvent).Count();
			//						return alt_dvrs.Count();
			//}, System.Threading.CancellationToken.None);
		}
		private Task<int> CountDVRbyALertTypes(ICache<AlertCacheModel> cache, DateTime sdate, DateTime edate, IEnumerable<int> kdvrs, byte alerttype, bool include_cmsalert = false)
		{
			int sdate_int = (int)sdate.FullDateTimeToUnixTimestamp();
			int edate_int = (int)edate.FullDateTimeToUnixTimestamp();
			return Task.Run<int>(() =>
			{
				IEnumerable<int> _kdvrs = Include_CMSAlert(kdvrs, include_cmsalert);
				IEnumerable<AlertCacheModel> alts = cache.Query<AlertCacheModel>(it => it.KAlertType == alerttype && it.Time >= sdate_int && it.Time <= edate_int, it => it);
				IEnumerable<int> alt_dvrs = !_kdvrs.Any() ? alts.Select(x=>x.KDVR).Distinct() : alts.Join(_kdvrs, alt => alt.KDVR, kdvr => kdvr, (alt, kdvr) => alt.KDVR).Distinct();

				return alt_dvrs.Count();
			}, System.Threading.CancellationToken.None);

		}
		#endregion

		private byte[] ReadImageData(int kDVR)
		{
			string folder = Path.Combine(AppSettings.AppSettings.Instance.AppData, Consts.PATH_RAW_DVRIMAGE, kDVR.ToString());
			DirectoryInfo dirInf = new DirectoryInfo(folder);
			if (dirInf.Exists)
			{
				FileInfo lastFile = dirInf.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
				if (lastFile != null && lastFile.Exists)
				{
					return File.ReadAllBytes(lastFile.FullName);
				}
			}
			return null;
		}

	}
}
