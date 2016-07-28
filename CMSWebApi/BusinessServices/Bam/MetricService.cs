using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.BusinessServices.GoalType;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using  Extensions;
using Extensions.Linq;
using CMSWebApi.BusinessServices.ReportBusiness;

namespace CMSWebApi.BusinessServices.Bam
{
	public class MetricService : ReportBusinessService //POSBusinessService
	{
		#region properties
		public IGoalTypeService IGoalTypeSvc { get; set; }
		public IIOPCService IIOPCService { get; set; }
		public ISiteService SiteSvc { get; set; }
		public IBamMetricService MetricReportSvc { get; set; }
		public IFiscalYearServices IFiscalYear { get; set; }
		public IPOSService POSService { get; set; }
		#endregion

		public async Task<IEnumerable<BamDataBySite>> GetBamDataBySite(IEnumerable<SitePACID> site_pac, DateTime startDate, DateTime endDate)
		{
			//var site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new { pacid = si.PACID.Value, sitekey = si.siteKey.Value }).Distinct();
			//var site_pac_count = site_pac.GroupJoin(site_pac, s => s.sitekey, sc => sc.sitekey, (s, sc) => new { pacid = s.pacid, sitekey = s.sitekey, count = sc.Count() });
			IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
			//bool refasync = false;
			//List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await base.IIOPCBusinessService.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync);

			var conv = await base.IPOSBusinessService.GetConversionAsync(StartTimeOfDate(startDate), EndTimeOfDate(endDate), pacIds);
			/*
            var pacid_conv = site_pac_count.Join(conv, si => si.pacid, cv => cv.PACID, (si, cv) =>
                new
                {
                    siteKey = si.sitekey,
                    pacid = si.pacid,
                    dvrdate = cv.DVRDate,
                    count = si.count,
                    TrafficIn = cv.TrafficIn,
                    TrafficOut = cv.TrafficOut,
                    CountTrans = cv.CountTrans,
                    TotalAmount = cv.TotalAmount,
                    Dpo = cv.TotalAmount == null || cv.TrafficIn == null || (int)cv.TrafficIn == 0 ? 0 : Math.Round(((decimal)cv.TotalAmount / (decimal)cv.TrafficIn), 2),
                    Conversion = cv.CountTrans == null || cv.TrafficIn == null || (int)cv.TrafficIn == 0 ? 0 : ((decimal)cv.CountTrans * 100) / (Math.Max(1, (decimal)cv.TrafficIn)),
                    Avt = cv.CountTrans == null || (int)cv.CountTrans <= 0 || cv.TotalAmount == null || (decimal)cv.TotalAmount == 0 ? 0 : Math.Round((decimal)cv.TotalAmount / (decimal)cv.CountTrans, 2),
                    Upd = cv.TotalAmount == null || cv.CountTrans == null || cv.TrafficIn == null || ((decimal)cv.TrafficIn - (decimal)cv.CountTrans) < 0 || (int)cv.CountTrans == 0
                            ? 0 : Math.Round(((decimal)cv.TotalAmount / (decimal)cv.CountTrans) * ((decimal)cv.TrafficIn - (decimal)cv.CountTrans), 2)
                });

            var site_conv = pacid_conv.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(group =>
                new
                {
                    DVRDate = group.Key.dvrdate,
                    siteKey = group.Key.siteKey,
                    CountTrans = group.Any() ? group.Sum(x => x.CountTrans) : 0,
                    TrafficIn = group.Any() ? group.Sum(x => x.TrafficIn) : 0,
                    TrafficOut = group.Any() ? group.Sum(x => x.TrafficOut) : 0,
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount) : 0,
                    Dpo = (decimal)group.Average(x => x.Dpo),
                    Conversion = group.Any() ? (decimal)group.Sum(x => x.Conversion) / group.FirstOrDefault().count : 0,
                    Avt = (decimal)group.Average(x => x.Avt),
                    Upd = (decimal)group.Average(x => x.Upd)
                }
                );
            */

			var pacid_conv = conv.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
				new
				{
					siteKey = si.SiteKey,
					pacid = si.PACID,
					dvrdate = cv.DVRDate,
					//count = si.count,
					TrafficIn = cv.TrafficIn,
					TrafficOut = cv.TrafficOut,
					CountTrans = cv.CountTrans,
					TotalAmount = cv.TotalAmount//,
					//Dpo = cv.TotalAmount == null || cv.TrafficIn == null || (int)cv.TrafficIn == 0 ? 0 : Math.Round(((decimal)cv.TotalAmount / (decimal)cv.TrafficIn), 2),
					//Conversion = cv.CountTrans == null || cv.TrafficIn == null || (int)cv.TrafficIn == 0 ? 0 : ((decimal)cv.CountTrans * 100) / (Math.Max(1, (decimal)cv.TrafficIn)),
					//Avt = cv.CountTrans == null || (int)cv.CountTrans <= 0 || cv.TotalAmount == null || (decimal)cv.TotalAmount == 0 ? 0 : Math.Round((decimal)cv.TotalAmount / (decimal)cv.CountTrans, 2),
					//Upd = cv.TotalAmount == null || cv.CountTrans == null || cv.TrafficIn == null || ((decimal)cv.TrafficIn - (decimal)cv.CountTrans) < 0 || (int)cv.CountTrans == 0
					//        ? 0 : Math.Round(((decimal)cv.TotalAmount / (decimal)cv.CountTrans) * ((decimal)cv.TrafficIn - (decimal)cv.CountTrans), 2)
				}).Distinct();

			var site_conv_NC = pacid_conv.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(group =>
				new
				{
					DVRDate = group.Key.dvrdate,
					siteKey = group.Key.siteKey,
					CountTrans = (decimal)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
					TrafficIn = (decimal)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
					TrafficOut = (decimal)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
					TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
				});

			var site_conv = site_conv_NC.Select(si =>
				new BamDataBySite()
				{
					DVRDate = si.DVRDate.HasValue ? si.DVRDate.Value : DateTime.MinValue,
					SiteKey = si.siteKey,
					CountTrans = si.CountTrans,
					TrafficIn = si.TrafficIn,
					TrafficOut = si.TrafficOut,
					TotalAmount = si.TotalAmount,
					Dpo = (si.TotalAmount == 0 || si.TrafficIn == 0) ? 0 : si.TotalAmount / si.TrafficIn,
					Conversion = (si.TrafficIn == 0 || si.CountTrans == 0) ? 0 : ((si.CountTrans * 100) / si.TrafficIn) > 150 ? 0 : ((si.CountTrans * 100) / si.TrafficIn),
					Avt = (si.TotalAmount == 0 || si.CountTrans == 0) ? 0 : si.TotalAmount / si.CountTrans,
					Upd = (si.TotalAmount == 0 || (si.TrafficIn - si.CountTrans) < 0 || si.CountTrans == 0 ) ? 0 : (si.TotalAmount / si.CountTrans) * (si.TrafficIn - si.CountTrans)
				});
			return site_conv;
		}
		public async Task<IEnumerable<BamDataBySite>> GetBamDataBySite(UserContext user, List<int> parKeys, DateTime startDate, DateTime endDate)
		{
			IEnumerable<UserSiteDvrChannel> sites = null;
			//sites = await base.UserSitesAsync(base.DataService, user);
			List<int> selectedSites = parKeys;
			if (selectedSites != null && selectedSites.Count > 0)
			{
				IEnumerable<UserSiteDvrChannel> allsites = await base.UserSitesAsync(base.DataService, user);
				sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
			}
			else
			{
				sites = await base.UserSitesAsync(base.DataService, user);
			}
			IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			//IEnumerable<int> pacIds = sites.Where(item => item.PACID.HasValue && item.PACID.Value > 0).Select(kdvr => kdvr.PACID.Value).Distinct();

			return await GetBamDataBySite(site_pac, startDate, endDate);
		}
		//private async Task<IEnumerable<UserSiteDvrChannel>> GetSitesByUser(UserContext user, List<int> parKeys)
		//{
		//	IEnumerable<UserSiteDvrChannel> sites = null;
		//	List<int> selectedSites = parKeys;
		//	if (selectedSites != null && selectedSites.Count > 0)
		//	{
		//		IEnumerable<UserSiteDvrChannel> allsites =  await base.UserSitesAsync(base.DataService, user);
		//		sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
		//	}
		//	else
		//	{
		//		sites = await base.UserSitesAsync(base.DataService, user);
		//	}
		//	return sites;
		//}
		public async Task<MetricSumamryAll> GetMetricSumary(UserContext user, MetricParam param)
		{
			//IEnumerable<UserSiteDvrChannel> sites = await GetSitesByUser(user, param.SitesKey);
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, user, param.SitesKey);

			var site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => si.siteKey != null ? (si.PACID != null ? new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value } : null) : null).Distinct().ToList();
			//var site_pac_count = site_pac.GroupJoin(site_pac, s => s.sitekey, sc => sc.sitekey, (s, sc) => new { pacid = s.pacid, sitekey = s.sitekey, count = sc.Count() });

			IEnumerable<int> pacIds = sites.Where(t => t.PACID != null).Select(t => t.PACID).Distinct().Cast<int>();
			int numOfSite = sites.Select(x => x.siteKey).Distinct().Count();
			if (numOfSite <= 0)
				numOfSite = 1;

			//bool refasync = false;
			// Get Labor Hourly
			bool b_hideWH = Convert.ToBoolean(AppSettings.AppSettings.Instance.BHideWH);

			DateTime sdate = new DateTime(param.SetDate.Year, param.SetDate.Month, param.SetDate.Day, 0, 0, 0);
			DateTime edate = new DateTime(param.SetDate.Year, param.SetDate.Month, param.SetDate.Day, 23, 59, 59);
			var forecastConv = await IIOPCService.Proc_BAM_Get_DashBoard_ForeCast_Async(sdate, edate, pacIds, AppSettings.AppSettings.Instance.ForecastFomular, AppSettings.AppSettings.Instance.ForecastWeeks);

			var forecastJoinPacToSite = site_pac.Join(forecastConv, si => si.PACID, cv => cv.PACID, (si, cv) =>
				new
				{
					siteKey = si.SiteKey,
					pacid = si.PACID,
					//count = si.count,
					dvrdate = cv.DVRDate,
					TrafficIn = cv.TotalTraffic,
					TrafficOut = 0,
					CountTrans = cv.TotalTrans,
					TotalAmount = cv.TotalSales,
					Conversion = cv.Conversion,
					DPO = cv.DPO,
					ATV = cv.ATV,
					UnconvertedPortential = cv.UnconvertedPortential
				}).Distinct();

			var site_forecastResult = forecastJoinPacToSite.GroupBy(x => new {x.siteKey, x.dvrdate}).Select(group =>
				new
				{
					DVRDate = group.Key.dvrdate,
					siteKey = group.Key.siteKey,
					Conversion = group.Any() ? (decimal)group.Average(x => x.Conversion) : 0,//(decimal)group.Sum(x => x.Conversion) / group.FirstOrDefault().count : 0,
					Dpo = group.Any() ? (decimal)group.Average(x => x.DPO) : 0,
					TotalTraffict = group.Any() ? (decimal)group.Sum(x => x.TrafficIn) : 0,
					CountTrans = group.Any() ? (decimal)group.Sum(x => x.CountTrans) : 0,
					TrafficIn = group.Any() ? (decimal)group.Sum(x => x.TrafficIn) : 0,
					TrafficOut = group.Any() ? (decimal)group.Sum(x => x.TrafficOut) : 0,
					TotalAmount = group.Any() ? (decimal)group.Sum(x => x.TotalAmount) : 0,
					Avt = group.Any() ? (decimal)group.Average(x => x.ATV) : 0,
					Upd = group.Any() ? (decimal)group.Sum(x => x.UnconvertedPortential) : 0
				}).ToList();

			DateTime rptFromDate = edate;
			var site_conv = await GetBamDataBySite(site_pac, StartTimeOfDate(param.StartDate), rptFromDate);//EndTimeOfDate(param.EndDate > param.WeekEndDate ? param.WeekEndDate : param.EndDate));

			var result = new List<MetricSumamry>();
			var metricList = MetricReportSvc.GetMetrics().Where(t => t.UserID == user.ID && t.ReportID == param.ReportId && t.Active == true).ToList();

			if (b_hideWH == true) {
				metricList = metricList.Where(x => !x.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
			}

			//Get BAM
			var includes = new string[]
			{
				typeof (tCMSWeb_Goals).Name,
				typeof (tCMSWeb_Goals).Name + "." + typeof(tCMSWeb_GoalType_Map).Name
			};

			//var goalBySite = SiteSvc.GetSites(t => new { SiteKey = t.siteKey, t.tCMSWeb_Goals,  t.tCMSWeb_Goals.tCMSWeb_GoalType_Map }, includes)
			//	.Where(t => param.SitesKey.Contains(t.SiteKey)).ToList();
			IEnumerable<int>sel_site = sites.Where( it => it.siteKey.HasValue).Select(s => s.siteKey.Value);
			var goalBySite = SiteSvc.GetSites(sel_site, t => new { SiteKey = t.siteKey, t.tCMSWeb_Goals, t.tCMSWeb_Goals.tCMSWeb_GoalType_Map }, includes).ToList();

			var validgoalBySite = goalBySite.Where(t => t.tCMSWeb_Goals != null && t.tCMSWeb_Goals.tCMSWeb_GoalType_Map != null);
			var goalList = validgoalBySite.ToList();

			decimal goalAverage = goalList == null ? 0 : (decimal)(goalList.Average(g =>
			{
				var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Conversion);
				return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
			}) ?? 0);

            decimal maxGoal = goalList == null ? 0 : (decimal)(goalList.Average(
					g =>
					{
						var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
							t => t.MaxValue.HasValue && t.GoalTypeID == (int) CMSWebApi.Utils.GoalType.Conversion);
						return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
					}) ?? 0);

            decimal minGoal = goalList == null ? 0 : (decimal)(goalList.Average(
					g =>
					{
						var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                            t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Conversion);
						return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
					}) ?? 0);

			#region Summary
			foreach (var metric in metricList)
			{
				var dataForDate = site_conv.Where(t => t.DVRDate.Equals(sdate.Date)).ToList();
				var dataForWeek = site_conv.Where(t => t.DVRDate >= param.WeekStartDate && t.DVRDate <= sdate.Date).ToList();

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_CONVERSION_RATE.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var convByDate = site_conv.GroupBy(it => it.DVRDate).Select(sc => new { Conversion = !sc.Any() ? 0 : sc.Sum(aa => aa.Conversion) / numOfSite });//site_conv.GroupBy(it => it.SiteKey)
					var valSiteConv = convByDate.Where(x => x.Conversion > 0);
					var conWeekByDate = dataForWeek.GroupBy(it => it.DVRDate).Select(sc => new { Conversion = !sc.Any() ? 0 : sc.Sum(aa => aa.Conversion) / numOfSite });//dataForWeek.GroupBy(it => it.SiteKey)
					var valConvWeek = conWeekByDate.Where(x => x.Conversion > 0);

					var convResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = !valSiteConv.Any() ? 0 : valSiteConv.Average(x=>x.Conversion),//!site_conv.Any() ? 0 : site_conv.GroupBy(it => it.SiteKey).Select(sc => sc.Average(aa => aa.Conversion)).Average(it => it),//site_conv.Average(x => x.Conversion) : 0,
						TotalWeekToDate = !valConvWeek.Any() ? 0 : valConvWeek.Average(x => x.Conversion),//!dataForWeek.Any() ? 0 :  dataForWeek.GroupBy( it => it.SiteKey).Select( sc => sc.Average( aa =>aa.Conversion) ).Average( it => it),
						Actualy = !dataForDate.Any() ? 0 : dataForDate.Sum(x => x.Conversion) / numOfSite,//dataForDate.Average(x => x.Conversion),
						Goal = goalAverage,
						MaxGoal = maxGoal,
						MinGoal = minGoal,
						Forcecast = !site_forecastResult.Any() ? 0 : (decimal)site_forecastResult.Sum(t => t.Conversion) / numOfSite,//(decimal)site_forecastResult.Average(t => t.Conversion),
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0
					};
					result.Add(convResult);
					continue; 
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_DPO.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var dpoByDate = site_conv.GroupBy(it => it.DVRDate).Select(sc => new { Dpo = !sc.Any() ? 0 : sc.Sum(aa => aa.Dpo)/numOfSite });//site_conv.GroupBy(it => it.SiteKey)
					var valSiteDpo = dpoByDate.Where(x => x.Dpo > 0);
					var dpoWeekByDate = dataForWeek.GroupBy(it => it.DVRDate).Select(sc => new { Dpo = !sc.Any() ? 0 : sc.Sum(aa => aa.Dpo)/numOfSite });//dataForWeek.GroupBy(it => it.SiteKey)
					var valDpoWeek = dpoWeekByDate.Where(x => x.Dpo > 0);

					var dpoResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = valSiteDpo.Any() ? valSiteDpo.Average(x => x.Dpo) : 0,//site_conv.Any() ? site_conv.Average(x => x.Dpo) : 0,
						TotalWeekToDate = valDpoWeek.Any() ? valDpoWeek.Average(x => x.Dpo) : 0,//dataForWeek.Any() ? dataForWeek.Average(x => x.Dpo) : 0,
						Actualy = !dataForDate.Any() ? 0 : dataForDate.Sum(x => x.Dpo) / numOfSite,//dataForDate.Average(x => x.Dpo),
						Forcecast = !site_forecastResult.Any() ? 0 : site_forecastResult.Sum(t => t.Dpo) / numOfSite,// site_forecastResult.Average(t => t.Dpo),
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
						MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
						MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(dpoResult);
					continue;
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = site_conv.Any() ? site_conv.Sum(x => x.TrafficIn) : 0,
						TotalWeekToDate = dataForWeek.Any() ? dataForWeek.Sum(x => x.TrafficIn) : 0,
						Actualy = dataForDate.Any() ? dataForDate.Sum(x => x.TrafficIn) : 0,
						Forcecast = (decimal)(site_forecastResult.Any() ? site_forecastResult.Sum(t => t.TotalTraffict) : 0),
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(oppResult);
					continue;
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_SALES.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = site_conv.Any() ? site_conv.Sum(x => x.TotalAmount) : 0,
						TotalWeekToDate = dataForWeek.Any() ? dataForWeek.Sum(x => x.TotalAmount) : 0,
						Actualy = dataForDate.Any() ? dataForDate.Sum(x => x.TotalAmount) : 0,
						Forcecast = site_forecastResult.Any() ? site_forecastResult.Sum(t => t.TotalAmount) : 0,
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(oppResult);
					continue;
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_TRANSACTIONS.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = site_conv.Any() ? site_conv.Sum(x => x.CountTrans) : 0,
						TotalWeekToDate = dataForWeek.Any() ? dataForWeek.Sum(x => x.CountTrans) : 0,
						Actualy = dataForDate.Any() ? dataForDate.Sum(x => x.CountTrans) : 0,
						Forcecast = (decimal)(site_forecastResult.Any() ? site_forecastResult.Sum(t => t.CountTrans) : 0),
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(oppResult);
					continue;
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_ATV.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					var atvByDate = site_conv.GroupBy(it => it.DVRDate).Select(sc => new { Avt = !sc.Any() ? 0 : sc.Sum(aa => aa.Avt) / numOfSite });//site_conv.GroupBy(it => it.SiteKey)
					var valSiteAtv = atvByDate.Where(x => x.Avt > 0);
					var atvWeekByDate = dataForWeek.GroupBy(it => it.DVRDate).Select(sc => new { Avt = !sc.Any() ? 0 : sc.Sum(aa => aa.Avt) / numOfSite });//dataForWeek.GroupBy(it => it.SiteKey)
					var valAtvWeek = atvWeekByDate.Where(x => x.Avt > 0);

					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = valSiteAtv.Any() ? valSiteAtv.Average(x => x.Avt) : 0,//site_conv.Any() ? site_conv.Average(x => x.Avt) : 0,
						TotalWeekToDate = valAtvWeek.Any() ? valAtvWeek.Average(x => x.Avt) : 0,//dataForWeek.Any() ? dataForWeek.Average(x => x.Avt) : 0,
						Actualy = !dataForDate.Any() ? 0 : dataForDate.Sum(x => x.Avt) / numOfSite,//dataForDate.Average(x => x.Avt)
						Forcecast = !site_forecastResult.Any() ? 0 : site_forecastResult.Sum(t => t.Avt) / numOfSite, //site_forecastResult.Average(t => t.Avt)
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(oppResult);
					continue;
				}

				if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_UPD.ToString(),
					StringComparison.OrdinalIgnoreCase))
				{
					var updByDate = site_conv.GroupBy(it => it.DVRDate).Select(sc => new { Upd = sc.Sum(aa => aa.Upd) });//site_conv.GroupBy(it => it.SiteKey)
					var valSiteUpd = updByDate.Where(x => x.Upd > 0);
                    var updWeekByDate = dataForWeek.GroupBy(it => it.DVRDate).Select(sc => new { Upd = sc.Sum(aa => aa.Upd) });//dataForWeek.GroupBy(it => it.SiteKey)
					var valUpdWeek = updWeekByDate.Where(x => x.Upd > 0);
					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
                        TotalPeridToDate = valSiteUpd.Any() ? valSiteUpd.Sum(x => x.Upd) : 0,//site_conv.Any() ? site_conv.Average(x => x.Upd) : 0,
                        TotalWeekToDate = valUpdWeek.Any() ? valUpdWeek.Sum(x => x.Upd) : 0,//dataForWeek.Any() ? dataForWeek.Average(x => x.Upd) : 0,
						Actualy = dataForDate.Any() ? dataForDate.Sum(x => x.Upd) : 0,
						Forcecast = site_forecastResult.Any() ? site_forecastResult.Sum(t => t.Upd) : 0,
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
						Goal = (decimal)(goalList.Average(g =>
						{
							var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
							return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
						g =>
						{
							var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
								t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
							return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
						}) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
						g =>
						{
							var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
							return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
						}) ?? 0)
					};
					result.Add(oppResult);
					continue;
				}

                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase) && b_hideWH == false)
                {
                    var forecastLaborHoursResult = await IIOPCService.Proc_BAM_DashBoard_LaborHours_ForeCast_Async(sdate, edate, pacIds, AppSettings.AppSettings.Instance.ForecastFomular, AppSettings.AppSettings.Instance.ForecastWeeks);

                    List<Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks_Result> forecastLaborHours = forecastLaborHoursResult;
                   
                    List<Func_BAM_LaborHourlyWorkingHour_Result> laborForPeriod = await IPOSService.Func_BAM_LaborHourlyWorkingHour(StartTimeOfDate(param.StartDate), EndTimeOfDate(rptFromDate), pacIds);
                    var laborForDate = laborForPeriod.Where(t => t.DVRDate.Equals(sdate.Date)).ToList();
                    var laborForWeek = laborForPeriod.Where(t => t.DVRDate >= param.WeekStartDate && t.DVRDate <= sdate.Date).ToList();

                    var laborForDayPeriod = laborForWeek.GroupBy(g => g.DVRDate).Select(group =>
                    new Func_BAM_LaborHourlyWorkingHour_Result()
                    {
                        DVRDate = group.Key,
                        LaborHours = group.Sum(t => t.LaborHours.Value)
                    }).ToList();
                    var laborForDayWeel = laborForWeek.GroupBy(g => g.DVRDate).Select(group =>
                    new Func_BAM_LaborHourlyWorkingHour_Result()
                    {
                        DVRDate = group.Key,
                        LaborHours = group.Sum(t => t.LaborHours.Value)
                    }).ToList();
                  
                    var laborResult = new MetricSumamry
                    {
                        Name = metric.tbl_BAM_Metric.MetricName,
                        TotalPeridToDate = laborForDayPeriod.Any() ? (decimal)laborForDayPeriod.Sum(x => Math.Round(x.LaborHours.Value, 0, MidpointRounding.AwayFromZero)) : 0,
                        TotalWeekToDate = laborForDayWeel.Any() ? (decimal)laborForDayWeel.Sum(x => Math.Round(x.LaborHours.Value, 0, MidpointRounding.AwayFromZero)) : 0,
                        Actualy = laborForDate.Any() ? (decimal)laborForDate.Sum(x => x.LaborHours.Value) : 0,
                        Forcecast = (decimal)(forecastLaborHours.Any() ? (decimal)forecastLaborHours.Sum(t => t.LaborHours.Value) : 0),
                        ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
                        UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
                        UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
                        IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
                        Order = metric.MetricOrder ?? 0,
                        UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
                        Goal = (decimal)(goalList.Average(g =>
                        {
                            var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                            return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                        }) ?? 0),
                        MaxGoal = (decimal)(goalList.Average(
                        g =>
                        {
                            var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                            return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                        }) ?? 0),
                        MinGoal = (decimal)(goalList.Average(
                        g =>
                        {
                            var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                            return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                        }) ?? 0)
                    };
                    result.Add(laborResult);
                    continue;
                }
				else
				{
					var oppResult = new MetricSumamry
					{
						Name = metric.tbl_BAM_Metric.MetricName,
						TotalPeridToDate = 0,
						TotalWeekToDate = 0,
						Actualy = 0,
						Forcecast = 0,
						ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
						UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
						UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
						IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
						Order = metric.MetricOrder ?? 0,
						UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0
					};
					result.Add(oppResult);
					continue;
				}
			}
			#endregion

			MetricSumamryAll retData = new MetricSumamryAll();
			retData.DataTableSumary = result;
			return retData;
		}

		private async Task<ChartWeekAtAGlanceAll> GetSumaryChartData(UserContext userLogin, IEnumerable<SitePACID> site_pac, IEnumerable<int> pacIds, IEnumerable<BamDataBySite> site_conv, DateTime startDate, DateTime endDate)
		{
			ChartWeekAtAGlanceAll retData = new ChartWeekAtAGlanceAll();

			#region Forecast chart
			var forcastAll = await IIOPCService.Proc_BAM_Get_DashBoard_ForeCast_Async(startDate, endDate, pacIds, AppSettings.AppSettings.Instance.ForecastFomular, AppSettings.AppSettings.Instance.ForecastWeeks);
			var pacid_forcast = site_pac.Join(forcastAll, si => si.PACID, cv => cv.PACID, (si, cv) =>
				new
				{
					siteKey = si.SiteKey,
					pacid = si.PACID,
					dvrdate = cv.DVRDate,
					//count = si.count,
					Conversion = cv.Conversion.HasValue ? cv.Conversion.Value : 0
				});

			var site_forcast = pacid_forcast.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(group =>
				new
				{
					DVRDate = group.Key.dvrdate,
					siteKey = group.Key.siteKey,
					Conversion = group.Any() && group.Where(x=>x.Conversion > 0).Any() ? group.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0 //group.Any() ? group.Sum(x => x.Conversion) / group.FirstOrDefault().count : 0
				});
			var forcast_day = site_forcast.GroupBy(x => x.DVRDate).Select(gr => new
			{
				DVRDate = gr.Key.HasValue ? gr.Key.Value : DateTime.MinValue,
				Conversion = gr.Any() && gr.Where(x=>x.Conversion > 0).Any() ? gr.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0
			});

			var conv_day = site_conv.GroupBy(x => x.DVRDate).Select(gr => new
			{
				DVRDate = gr.Key,
				Label = gr.Key.ToString(),
				Conversion = gr.Any() && gr.Where(x=>x.Conversion > 0).Any() ? Math.Round(gr.Where(x=>x.Conversion > 0).Average(x => x.Conversion), 2) : 0,
				TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
				CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
				Atv = gr.Any() && gr.Where(x=>x.Avt > 0).Any() ? Math.Round(gr.Where(x=>x.Avt > 0).Average(x => x.Avt), 2) : 0
			});

			var dbdata = conv_day.FullOuterJoin(forcast_day, cnt => cnt.DVRDate, fcs => fcs.DVRDate, (cnt, fcs, k) => new
			{
				DVRDate = (cnt == null) ? ((fcs == null) ? DateTime.MinValue : fcs.DVRDate) : cnt.DVRDate,
				Conversion = (cnt == null) ? 0 : cnt.Conversion,
				Atv = (cnt == null) ? 0 : cnt.Atv,
				ConvForcast = (fcs == null) ? 0 : fcs.Conversion
			}).OrderBy(x => x.DVRDate);

			IEnumerable<DateTime> lsDateTimes = new List<DateTime>();
			int iCount = Convert.ToInt32((endDate - startDate).TotalDays);
			lsDateTimes = ArrayUtilities.SequenceDate(startDate, iCount);

			var resChart = from day in lsDateTimes
						   join db in dbdata on day.Date equals db.DVRDate.Date into dat
						   from it in dat.DefaultIfEmpty()
						   select new ChartWeekAtAGlance()
						   {
							   DVRDate = day,
							   Label = day.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							   Conversion = it != null ? it.Conversion : 0,
							   ConvForecast = it != null ? (decimal)it.ConvForcast : 0,
							   Avt = it != null ? it.Atv : 0
						   };
			retData.DataChartSumamry = resChart;
			#endregion

			#region Compare data
			DateTime endLastWeek = startDate.AddDays(-1);
			DateTime startLastWeek = endLastWeek.AddDays(-6);
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, endLastWeek);
			if (fyInfo != null)
			{
				FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, endLastWeek, fyInfo.FYDateStart.Value);
				if (fyWeekInfo != null)
					startLastWeek = fyWeekInfo.StartDate;
			}

			var site_conv_last = await GetBamDataBySite(site_pac, startLastWeek, endLastWeek);
			var conv_day_last = site_conv_last.GroupBy(x => x.DVRDate).Select(gr => new
			{
				DVRDate = gr.Key,
				Label = gr.Key.ToString("MM/dd/yy"),
				Conversion = gr.Any() && gr.Where(x=>x.Conversion > 0).Any() ? gr.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0,
				TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
				CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
				Atv = gr.Any() && gr.Where(x => x.Avt > 0).Any() ? gr.Where(x => x.Avt > 0).Average(x => x.Avt) : 0
			});

			ChartWeekAtAGlance cmpData = resChart.FirstOrDefault();
			var validDataAtv = conv_day.Where(x => x.Atv > 0);
			decimal curATV = validDataAtv.Any() ? validDataAtv.Average(x => x.Atv) : 0;
			var validCmpAtv = conv_day_last.Where(x => x.Atv > 0);
			decimal cmpATV = validCmpAtv.Any() ? validCmpAtv.Average(x => x.Atv) : 0;
			ALertCompModel avtData = new ALertCompModel() { Value = Math.Round(curATV, 2, MidpointRounding.AwayFromZero), CmpValue = curATV.toValueCompare(cmpATV), Increase = (curATV == cmpATV) ? (bool?)null : curATV > cmpATV };
			retData.AVTData = avtData;

			decimal curConv = conv_day.Any() && conv_day.Where(x => x.Conversion > 0).Any() ? conv_day.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0;//curData != null ? curData.Conversion.Value : 0;
			decimal cmpConv = conv_day_last.Any() && conv_day_last.Where(x => x.Conversion > 0).Any() ? conv_day_last.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0;

			decimal curTraffic = conv_day.Any() ? conv_day.Sum(x => x.TrafficIn) : 0;
			decimal curTrans = conv_day.Any() ? conv_day.Sum(x => x.CountTrans) : 0;
			ConvCompModel convCompare = new ConvCompModel()
			{
				Traffic = Convert.ToInt32(curTraffic),
				Transaction = Convert.ToInt32(curTrans),
				Value = curConv,
				CmpValue = curConv.toValueCompare(cmpConv),
				Increase = (curConv == cmpConv) ? (bool?)null : curConv > cmpConv
			};
			retData.ConvData = convCompare;
			#endregion

			return retData;
		}

		public async Task<MetricSumamryDetail> GetMetricSumaryDetail(UserContext userLogin, MetricParam param)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, userLogin, param.SitesKey);
			var site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => si.siteKey != null ? (si.PACID != null ? new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value } : null) : null).Distinct().ToList();
			IEnumerable<int> LBpacIds = sites.Where(t => t.PACID != null).Select(t => t.PACID).Distinct().Cast<int>();

			var site_conv = await GetBamDataBySite(site_pac, param.WeekStartDate, param.WeekEndDate);
			IEnumerable<int> sitekeys = sites.Where(it => it.siteKey.HasValue).Select(s => s.siteKey.Value).Distinct();
			int numOfSite = sitekeys.Count();
			if (numOfSite <= 0)
				numOfSite = 1;

            // Get Labor Hourly
            bool b_hideWH = Convert.ToBoolean(AppSettings.AppSettings.Instance.BHideWH);
            List<Func_BAM_LaborHourlyWorkingHour_Result> laborForPeriod = new  List<Func_BAM_LaborHourlyWorkingHour_Result>();
           
            if (b_hideWH == false)
            {
                var laborForPeriodResult = await IPOSService.Func_BAM_LaborHourlyWorkingHour(StartTimeOfDate(param.WeekStartDate), EndTimeOfDate(param.WeekEndDate), LBpacIds);

                laborForPeriod = laborForPeriodResult.GroupBy(g => g.DVRDate).Select(group => 
                    new Func_BAM_LaborHourlyWorkingHour_Result()
                    {
                    DVRDate = group.Key,
                    LaborHours = group.Sum(t => t.LaborHours.Value)
                }).ToList();
            }

			var TableMetric = site_conv.GroupBy(x => x.DVRDate).Select(group =>
				new TableMetricSumamryDetail()
				{
					Date = group.Key,
					TotalTrans = group.Sum(x => x.CountTrans),
					TotalTraffic = group.Sum(x => x.TrafficIn),
					TotalSales = group.Sum(x => x.TotalAmount),
					Dpo = !group.Any() ? 0 : Math.Round(group.Sum(x=>x.Dpo) / numOfSite, 2),//Math.Round(group.Average(x => x.Dpo),2),
					Conversion = !group.Any() ? 0 : Math.Round(group.Sum(x=>x.Conversion) / numOfSite, 2),//Math.Round(group.Average(x => x.Conversion),2),
					Avt = !group.Any() ? 0 : Math.Round(group.Sum(x=>x.Avt) / numOfSite, 2),//Math.Round(group.Average(x => x.Avt),2),
					Upd = Math.Round(group.Sum(x => x.Upd),2)
				});

            #region Goal
            var includes = new string[]
			{
				typeof (tCMSWeb_Goals).Name,
				typeof (tCMSWeb_Goals).Name + "." + typeof(tCMSWeb_GoalType_Map).Name
			};


            //var goalBySite = SiteSvc.GetSites(t => new { SiteKey = t.siteKey, t.tCMSWeb_Goals, t.tCMSWeb_Goals.tCMSWeb_GoalType_Map }, includes)
            //    .Where(t => sitekeys.Contains(t.SiteKey));
            var goalBySite = SiteSvc.GetSites(t => new { SiteKey = t.siteKey, t.tCMSWeb_Goals, t.tCMSWeb_Goals.tCMSWeb_GoalType_Map }, includes)
                .Where(t => sitekeys.Contains(t.SiteKey)).ToList();

            var validgoalBySite = goalBySite.Where(t => t.tCMSWeb_Goals != null && t.tCMSWeb_Goals.tCMSWeb_GoalType_Map != null);

            var goalList = validgoalBySite.ToList();

            decimal goalAverage = goalList == null ? 0 : (decimal)(goalList.Average(g =>
            {
                var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Conversion);
                return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
            }) ?? 0);

            decimal maxGoal = goalList == null ? 0 : (decimal)(goalList.Average(
              g =>
              {
                  var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                   t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Conversion);
                  return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
              }) ?? 0);

            decimal minGoal = goalList == null ? 0 : (decimal)(goalList.Average(
              g =>
              {
                  var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                   t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Conversion);
                  return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
              }) ?? 0);
            #endregion

            #region SummaryDetail

            List<MetricDetail> table = new List<MetricDetail>();
			int dayofweek = (int)param.EndDate.Subtract(param.StartDate).TotalDays + 1;
            int countNameMetric = 0;
            var metricList = MetricReportSvc.GetMetrics().Where(t => t.UserID == userLogin.ID && t.ReportID == param.ReportId && t.Active == true).ToList();
            if (b_hideWH == true) {
                metricList = metricList.Where(x => !x.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
            foreach (var metric in metricList)
            {
                table.Add(new MetricDetail()
                {
                    Name = metric.tbl_BAM_Metric.MetricName,
                    UnitType = metric.tbl_BAM_Metric.UnitType ?? 0,
                    Details = new List<MetricDetailBase>(){
                        new MetricDetailBase()  
                        {
                                Date = param.WeekStartDate,
                                Value = 0
                        }
                    },
                    UnitName = metric.tbl_BAM_Metric.CurrencyFormat,
                    IsPrefix = metric.tbl_BAM_Metric.CF_Prefix ?? false,
                    UnitRound = metric.tbl_BAM_Metric.DecimalPlace ?? 0,
                    Order = metric.MetricOrder ?? 0,
                    ResourceKey = metric.tbl_BAM_Metric.MetricResourceName,
                    TotalWeek = 0,
                    Goal = 0,
                    MinGoal = 0,
                    MaxGoal = 0
                });
                for (int i = 1; i < dayofweek; i++)
                {
                    table[countNameMetric].Details.Add(new MetricDetailBase()
                    {
                        Date = param.WeekStartDate.AddDays(i),
                        Value = 0
                    });
                }

                #region GetGoal
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_CONVERSION_RATE.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = goalAverage;
                    table[countNameMetric].MaxGoal = maxGoal;
                    table[countNameMetric].MinGoal = minGoal;
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_DPO.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.DPO);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_SALES.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Sale);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_TRANSACTIONS.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Transaction);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_ATV.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.ATV);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_UPD.ToString(),
                     StringComparison.OrdinalIgnoreCase))
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.UPD);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase) && b_hideWH == false)
                {
                    table[countNameMetric].Goal = (decimal)(goalList.Average(g =>
                    {
                        var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                        return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                    }) ?? 0);
                    table[countNameMetric].MaxGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                         return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                     }) ?? 0);
                    table[countNameMetric].MinGoal = (decimal)(goalList.Average(
                     g =>
                     {
                         var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                             t => t.MinValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.LaborHours);
                         return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                     }) ?? 0);
                }
                #endregion

                for (int i = 0; i < table[countNameMetric].Details.Count; i++)
                {
                    foreach (var obj in TableMetric)
                    {

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_CONVERSION_RATE.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.Conversion;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.Conversion;
                                var valConversion = TableMetric.Where(t => t.Conversion != 0);
                                table[countNameMetric].TotalWeek = valConversion.Any() ? (decimal)valConversion.Average(x => x.Conversion) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_DPO.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.Dpo;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.Dpo;
                                var valDpo = TableMetric.Where(t => t.Dpo != 0);
                                table[countNameMetric].TotalWeek = valDpo.Any() ? (decimal)valDpo.Average(x => x.Dpo) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.TotalTraffic;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.TotalTraffic;
                                table[countNameMetric].TotalWeek = TableMetric.Any() ? (decimal)TableMetric.Sum(x => x.TotalTraffic) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_SALES.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.TotalSales;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.TotalSales;
                                table[countNameMetric].TotalWeek = TableMetric.Any() ? (decimal)TableMetric.Sum(x => x.TotalSales) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_TRANSACTIONS.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.TotalTrans;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.TotalTrans;
                                table[countNameMetric].TotalWeek = TableMetric.Any() ? (decimal)TableMetric.Sum(x => x.TotalTrans) : 0;
                            }
                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_ATV.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.Avt;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.Avt;
                                var valAvt = TableMetric.Where(t => t.Avt != 0);
                                table[countNameMetric].TotalWeek = valAvt.Any() ? (decimal)valAvt.Average(x => x.Avt) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_UPD.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)obj.Upd;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.Upd;
                                var valUpd = TableMetric.Where(t => t.Upd != 0);
                                table[countNameMetric].TotalWeek = valUpd.Any() ? (decimal)valUpd.Sum(x => x.Upd) : 0;
                            }

                        }

                        if (metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase) && b_hideWH == false)
                        {
                            if (table[countNameMetric].Details[i].Date == (DateTime)obj.Date)
                            {
                                table[countNameMetric].Details[i].Value = (decimal)laborForPeriod.Count > 0 ?
                                    (decimal)laborForPeriod.Where(x => DateTime.Compare(x.DVRDate.Value, table[countNameMetric].Details[i].Date) == 0).Select(t => Math.Round(t.LaborHours.Value, 0, MidpointRounding.AwayFromZero)).FirstOrDefault() : 0;
                                //table[countNameMetric].TotalWeek = table[countNameMetric].TotalWeek + (decimal)obj.Upd;

                                table[countNameMetric].TotalWeek = laborForPeriod.Count > 0 ? (decimal)laborForPeriod.Sum(x => Math.Round(x.LaborHours.Value, 0, MidpointRounding.AwayFromZero)) : 0;
                            }

                        }
                    }
                }
                countNameMetric++;
            }
            #endregion


			#region Chart Data
			ChartWeekAtAGlanceAll chartWAAGData = null;
			IEnumerable<GraphMetricSumamryDetail> GraphMetric = null;
			IEnumerable<TrafficChartModel> trafficChart = null;
			if (param.ReportType == 1)
			{
				IEnumerable<int> pacIds = sites.Where(t => t.PACID != null).Select(t => t.PACID).Distinct().Cast<int>();
				chartWAAGData = await GetSumaryChartData(userLogin, site_pac, pacIds, site_conv, param.WeekStartDate, param.WeekEndDate);
			}
			else
			{
				GraphMetric = site_conv.GroupBy(x => x.SiteKey).Select(group =>
					new GraphMetricSumamryDetail()
					{
						SiteKey = group.Key,
						Dpo = group.Any() ? group.Average(x => x.Dpo) : 0,
						Conversion = group.Any() && group.Where(x=>x.Conversion > 0).Any() ? group.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0
					});

				IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
				base.IIOPCService = IIOPCService;
				trafficChart = await base.IIOPCBusinessService.ChartTraffic(param.WeekStartDate, param.WeekEndDate, PeriodType.Day, pacIds, userLogin);
			}
			#endregion

			MetricSumamryDetail result = new MetricSumamryDetail();
			result.DataTableSumaryDetail = table;
			result.DashboardCharts = new BAMDashboardCharts();
			result.DashboardCharts.DataGraphSumamryDetail = GraphMetric;
			result.DashboardCharts.TrafficChart = trafficChart;
			result.WAAGCharts = chartWAAGData;

            #region InfoMetric
			//var metricCov_Traf = MetricReportSvc.GetMetricDefaults().Where(t => t.UserID == userLogin.ID && (t.MetricID == 5 || t.MetricID == 1)).ToList();
            var metricCov_Traf = MetricReportSvc.GetMetricDefaults().Where(t => t.UserID == userLogin.ID && (t.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_CONVERSION_RATE.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    t.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase))).ToList();

            foreach (var metric in metricCov_Traf)
            {
				if (metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_CONVERSION_RATE.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.GoalMetricConversion = new Goal_MetricModel()
                    {
                        Goal = goalAverage,
                        MinGoal = minGoal,
                        MaxGoal = maxGoal,
						Name = metric.MetricName,
						UnitType = metric.UnitType ?? 0,
						UnitName = metric.CurrencyFormat,
						IsPrefix = metric.CF_Prefix ?? false,
						UnitRound = metric.DecimalPlace ?? 0,
						Order = 0,
                    };
                }
				if (metric.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.GoalMetricTraffic = new Goal_MetricModel()
                    {
                        Goal = (decimal)(goalList.Average(g =>
                        {
                            var tCmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(t => t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                            return tCmsWebGoalTypeMap != null ? tCmsWebGoalTypeMap.MaxValue : null;
                        }) ?? 0),
                        MaxGoal = (decimal)(goalList.Max(
                         g =>
                         {
                             var cmsWebGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                 t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                             return cmsWebGoalTypeMap != null ? cmsWebGoalTypeMap.MaxValue : null;
                         }) ?? 0),
                        MinGoal = (decimal)(goalList.Min(
                         g =>
                         {
                             var webGoalTypeMap = g.tCMSWeb_Goals.tCMSWeb_GoalType_Map.FirstOrDefault(
                                 t => t.MaxValue.HasValue && t.GoalTypeID == (int)CMSWebApi.Utils.GoalType.Opportunity);
                             return webGoalTypeMap != null ? webGoalTypeMap.MinValue : null;
                         }) ?? 0),
						Name = metric.MetricName,
						UnitType = metric.UnitType ?? 0,
						UnitName = metric.CurrencyFormat,
						IsPrefix = metric.CF_Prefix ?? false,
						UnitRound = metric.DecimalPlace ?? 0,
						Order = 0,
                    };
                }
            }

            #endregion

            return result;
        }

		public List<MetricReportListModel> GetMetricReport(UserContext user, int reportId)
		{
			// Get Labor Hourly
			bool b_hideWH = Convert.ToBoolean(AppSettings.AppSettings.Instance.BHideWH);
			//metric.tbl_BAM_Metric.MetricResourceName.Equals(METRIC_DEFAULT.LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase) && b_hideWH == false

			var currentMetricList = MetricReportSvc.GetMetrics().Where(t => t.UserID == user.ID && t.ReportID == reportId && t.Active == true).ToList();
			var reportUser = MetricReportSvc.GetReportLists().FirstOrDefault(t => t.ReportID == reportId);
			var metricdefault = MetricReportSvc.GetMetricDefaults().Where(t => t.MetricGroupID == reportUser.MetricGroupID).ToList();

			if(b_hideWH == false)
			{
				return metricdefault.Select(t => new MetricReportListModel()
				{
					Name = t.MetricName,
					ResourceKey = t.MetricResourceName,
					MetricId = t.MetricID,
					Active = currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID) != null,
					Order = currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID) != null ? currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID).MetricOrder : 0
				}).ToList();
			}
			else
			{
				return metricdefault.Where(x => !x.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_LABOUR_HOURS.ToString(), StringComparison.OrdinalIgnoreCase))
					.Select(t => new MetricReportListModel()
					{
						Name = t.MetricName,
						ResourceKey = t.MetricResourceName,
						MetricId = t.MetricID,
						Active = currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID) != null,
						Order = currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID) != null ? currentMetricList.FirstOrDefault(m => m.MetricID == t.MetricID).MetricOrder : 0
					}).ToList();
			}
		}

		public List<ReportListModel> GetCustomReports(UserContext user, int groupId)
		{
			var customReports = MetricReportSvc.GetMetricReportUsers().Where(t => t.tbl_BAM_Metric_ReportList.MetricGroupID == groupId && t.Active == true).ToList();

			return customReports.Select(t => new ReportListModel()
			{
				Name = t.tbl_BAM_Metric_ReportList.ReportName,
				ResourceKey = t.tbl_BAM_Metric_ReportList.ReportResourceName,
				ReportId = t.tbl_BAM_Metric_ReportList.ReportID,
				MetricId = t.MetricID,
				Active = t.Active ?? false,
				Order = t.MetricOrder
			}).ToList();
		}

		public void UpdateMetric(UserContext user, MetricReportUpdateModel mectricUpdate)
		{
			MetricReportSvc.DeleteMetricReportUser(mectricUpdate.ReportId, user.ID);

			foreach (var metric in mectricUpdate.Metrics)
			{
				var usermetric = new tbl_BAM_Metric_ReportUser
				{
					Active = metric.Active,
					MetricID = (short)metric.MetricId,
					MetricOrder = (byte?) metric.Order,
					ReportID = (short) mectricUpdate.ReportId,
					UserID = user.ID
				};

				MetricReportSvc.InsertMetricReportUser(usermetric);
			}

			MetricReportSvc.SaveChange();

		}

		#region DriveThrough report
		public async Task<DriveThroughDataAll> GetReportDriveThrough(UserContext user, MetricParam param)
		{
			switch (param.ReportType)
			{
				case (int)Utils.BAMReportType.Hourly:
					return await GetDriveThroughHourly(user, param);
				case (int)Utils.BAMReportType.Daily:
				case (int)Utils.BAMReportType.WTD:
				case (int)Utils.BAMReportType.PTD:
					return await GetDriveThroughDaily(user, param);
				case (int)Utils.BAMReportType.Weekly:
					return await GetDriveThroughWeekly(user, param);
				case (int)Utils.BAMReportType.YTD:
					return await GetDriveThroughMonthly(user, param);
				default:
					return null;
			}
		}

		private async Task<IEnumerable<DriveThroughDataSite>> GetDriveThroughSite(IEnumerable<UserSiteDvrChannel> sites, DateTime startDate, DateTime endDate)
		{
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());
			List<Func_BAM_DriveThroughMonthly_Result> bamData = await MetricReportSvc.Func_BAM_DriveThroughMonthly_Async(pacIds, startDate, endDate);// GET FROM DATABASE

			Commons.TEqualityComparer<SitePACID> icomparer = new Commons.TEqualityComparer<SitePACID>((a, b) => { return a.PACID == b.PACID & a.SiteKey == b.SiteKey;});
			IEnumerable<int> siteKeys = sites.Select(x => x.siteKey.HasValue ? x.siteKey.Value : 0).Distinct();
			IEnumerable<SitePACID> site_pacid = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct(icomparer);

			string[] includes = { typeof(tCMSWebRegion).Name };
			IQueryable<tCMSWebSites> dbSites = SiteSvc.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
			IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

			var site_region = site_pacid.Join(dbSites, si => si.SiteKey, rg => rg.siteKey, (si, rg) => new { siteKey = si.SiteKey, PACID = si.PACID, regionKey = rg.RegionKey, serverID = rg.ServerID }).Distinct();//.ToList();//site = si, region = rg
			var site_region_name = site_region.Join(dbRegions, si => si.regionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.PACID, siteKey = si.siteKey, siteName = si.serverID, regionKey = rg.RegionKey, regionName = rg.RegionName }).Distinct().ToList();
			//var region_site_count = dbRegions.GroupJoin(site_region_name, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();
			var countSiteInRegion = site_region_name.GroupBy(x => x.regionKey).Select(gr => new { RegionKey = gr.Key, Count = gr.Where(x => siteKeys.Contains(x.siteKey)).Select(x => x.siteKey).Distinct().Count() }); //siteregionName, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();

			var site_data = bamData.Join(site_region_name, drv => drv.PACID, si => si.pacid, (drv, si) =>
				new
				{
					pacid = si.pacid,
					siteKey = si.siteKey,
					siteName = si.siteName,
					regionKey = si.regionKey,
					regionName = si.regionName,
					dvrDate = drv.DVRDate,
					//count = si.count,
					count = drv.Count,
					dwell = drv.DWellTime
				}).Distinct();

			var site_data_group = site_data.GroupBy(cs => new { cs.siteKey, cs.dvrDate })//, cs.regionKey
				.Select(s => new DriveThroughDataSite()
				{
					Date = s.Key.dvrDate.HasValue ? s.Key.dvrDate.Value : DateTime.MinValue,
					SiteKey = s.Key.siteKey,
					SiteName = s.Any() ? s.FirstOrDefault().siteName : string.Empty,
					RegionKey = s.Any() ? s.FirstOrDefault().regionKey : 0,
					RegionName = s.Any() ? s.FirstOrDefault().regionName : string.Empty,
					Count = s.Any() ? s.Sum(x => x.count.HasValue ? x.count.Value : 0) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.dwell.HasValue ? x.dwell.Value : 0) : 0)
				});

			var ret = site_data_group.Join(countSiteInRegion, s => s.RegionKey, reg => reg.RegionKey, (s, rg) => new DriveThroughDataSite()
			{
				Date = s.Date,
				SiteKey = s.SiteKey,
				SiteName = s.SiteName,
				RegionKey = s.RegionKey,
				RegionName = s.RegionName,
				Count = s.Count,
				Dwell = s.Dwell,
				CountSite = Math.Max(rg.Count, 1)
			}).Distinct();

			return ret;// site_data_group;
		}

		private async Task<DriveThroughDataAll> GetDriveThroughDaily(UserContext user, MetricParam param)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, user, param.SitesKey);
			DateTime sDate = param.StartDate;
			DateTime eDate = param.EndDate;
			/* ---------------------------------------------------------------*/
			if (param.ReportType == (int)Utils.BAMReportType.Daily)
			{
				sDate = param.StartDate;
				eDate = param.EndDate;
			}
			else
			{
				tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(user, param.EndDate);
				
				if (param.ReportType == (int)Utils.BAMReportType.WTD)
				{
					FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, param.EndDate, fyInfo.FYDateStart.Value);
					sDate = fyWeekInfo.StartDate;
					eDate = param.EndDate;
				}
				else if (param.ReportType == (int)Utils.BAMReportType.PTD)
				{
					List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, param.EndDate, fyInfo.FYDateStart.Value);
					FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x => x.StartDate <= param.EndDate && x.EndDate >= param.EndDate);
					sDate = fyPeriod != null ? fyPeriod.StartDate : param.EndDate;
					eDate = param.EndDate;
				}
			}
			/*-----------------------------------------------------------------------------*/
			var siteDataGroup = await GetDriveThroughSite(sites, sDate, eDate);
			List<DateTime> dateSearchList = siteDataGroup.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
			//var curDate = sDate;
			//while (curDate <= eDate)
			//{
			//	dateSearchList.Add(curDate);
			//	curDate = curDate.AddDays(1);
			//};

			#region Site Data
			var hasDataSites = siteDataGroup.Select(x => new { x.SiteKey, x.SiteName }).Distinct().ToList();
			var SiteWithDate = (from reg in hasDataSites
								from dd in dateSearchList
								select new { SiteKey = reg.SiteKey, Name = reg.SiteName, Date = dd }).ToList();

			IEnumerable<DriveThroughData> dataSiteDailyAll = (from si in SiteWithDate
															 join cv in siteDataGroup on new { si.SiteKey, si.Date } equals new { cv.SiteKey, cv.Date } into dat
															 from it in dat.DefaultIfEmpty()
															 select new DriveThroughData()
															 {
																 ID = si.SiteKey,
																 Name = si.Name,
																 Date = si.Date,
																 Count = it != null ? it.Count : 0,
																 Dwell = it != null ? it.Dwell : 0,
																 isRegion = false,
																 ParentKey = it != null ? it.RegionKey : 0
															 }).ToList();

			IEnumerable<DriveThroughDataSumary> dataBySiteSum = siteDataGroup.GroupBy(x => x.SiteKey)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().SiteName : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().RegionKey : 0,
					isRegion = false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = s.Any() ? Convert.ToInt32(s.Average(x => x.Dwell)) : 0,
					DetailData = dataSiteDailyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.Date)
				}).ToList();
			#endregion

			#region Region Data
			//REGION DATA
			var hasDataRegions = siteDataGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithDate = (from reg in hasDataRegions
								  from d in dateSearchList
								  select new { ID = reg.RegionKey, Name = reg.RegionName, Date = d.Date }).ToList();

			var regionGroup = siteDataGroup.GroupBy(x => new { x.RegionKey, x.Date }).
				Select(s => new
				{
					ID = s.Key.RegionKey,
					Name = s.Any() ? s.FirstOrDefault().RegionName : string.Empty,
					Date = s.Key.Date,
					Count = !s.Any() ? 0 : s.Sum(x => x.Count) / s.FirstOrDefault().CountSite,//s.Sum(x => x.Count) : 0,
					Dwell = !s.Any() ? 0 : s.Sum(x => x.Dwell) / s.FirstOrDefault().CountSite,//Convert.ToInt32(s.Average(x => x.Dwell)) : 0,
					isRegion = true,
					ParentKey = 0
				});

			IEnumerable<DriveThroughData> dataByRegionAll = (from si in RegionWithDate
															join cv in regionGroup on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
															from it in dat.DefaultIfEmpty()
															select new DriveThroughData()
															{
																ID = si.ID,
																Name = si.Name,
																Date = si.Date,
																Count = it != null ? it.Count : 0,
																Dwell = it != null ? it.Dwell : 0,
																isRegion = true,
																ParentKey = it != null ? it.ParentKey : 0
															}).ToList();

			IEnumerable<DriveThroughDataSumary> dataByRegionSum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataByRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.Date)
				}).ToList();

			foreach (DriveThroughDataSumary data in dataByRegionSum)
			{
				data.Sites = dataBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			#endregion

			#region Total Sum Data
			DriveThroughTotalSumBase totalSumSite = dataBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSumBase totalSumRegion = dataByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSum totalSum = new DriveThroughTotalSum()
			{
				TotalSumRegion = totalSumRegion,
				TotalSumSite = totalSumSite
			};

			#endregion

			#region Chart Data
			DateTime minDate = siteDataGroup.Any() ? siteDataGroup.Min(x => x.Date) : DateTime.MinValue;
			DateTime maxDate = siteDataGroup.Any() ? siteDataGroup.Max(x => x.Date) : DateTime.MinValue;
			IEnumerable<DateTime> fullDates = null;
			if (!(minDate == DateTime.MinValue && maxDate == DateTime.MinValue))
			{
				int iCount = Convert.ToInt32((maxDate - minDate).TotalDays);
				fullDates = ArrayUtilities.SequenceDate(minDate, iCount);
			}
			IEnumerable<CountDwellChart> chartByRegs = null;
			IEnumerable<CountDwellChart> chartBySites = null;
			if (fullDates != null && fullDates.Any())
			{
				var RegionFullDate = (from reg in hasDataRegions
									  from d in fullDates
									  select new { ID = reg.RegionKey, Name = reg.RegionName, Date = d.Date }).ToList();
				IEnumerable<DriveThroughData> chartByRegionAll = (from si in RegionFullDate
																  join cv in regionGroup on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																  from it in dat.DefaultIfEmpty()
																  select new DriveThroughData()
																  {
																	  ID = si.ID,
																	  Name = si.Name,
																	  Date = si.Date,
																	  Count = it != null ? it.Count : 0,
																	  Dwell = it != null ? it.Dwell : 0,
																	  isRegion = true,
																	  ParentKey = it != null ? it.ParentKey : 0
																  }).ToList();

				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartByRegionAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = 0,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});

				var SiteFullDate = (from reg in hasDataSites
									from dd in fullDates
									select new { SiteKey = reg.SiteKey, Name = reg.SiteName, Date = dd }).ToList();
				IEnumerable<DriveThroughData> chartSiteDailyAll = (from si in SiteFullDate
																   join cv in siteDataGroup on new { si.SiteKey, si.Date } equals new { cv.SiteKey, cv.Date } into dat
																   from it in dat.DefaultIfEmpty()
																   select new DriveThroughData()
																   {
																	   ID = si.SiteKey,
																	   Name = si.Name,
																	   Date = si.Date,
																	   Count = it != null ? it.Count : 0,
																	   Dwell = it != null ? it.Dwell : 0,
																	   isRegion = false,
																	   ParentKey = it != null ? it.RegionKey : 0
																   }).ToList();
				chartBySites = siteDataGroup.GroupBy(x => x.SiteKey).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().SiteName : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartSiteDailyAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = 0,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});
			}
			//var chartDataAll = siteDataGroup.GroupBy(x => x.Date).Select(
			//	gr => new CountDwellChart()
			//	{
			//		TimeIndex = 0,
			//		Date = gr.Key,
			//		Name = gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
			//		Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
			//		Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
			//		Regions = dataByRegionAll.Where(x => x.Date == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = 0,
			//			Date = x.Date,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		}),
			//		Sites = dataSiteDailyAll.Where(x => x.Date == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = 0,
			//			Date = x.Date,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		})
			//	}).OrderBy(x=>x.Date);
			#endregion

			DriveThroughDataAll retData = new DriveThroughDataAll();
			retData.DTData = dataByRegionSum;
			retData.TotalSum = totalSum;
			retData.ChartData = new CountDwellChartChartAll();
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<DriveThroughDataAll> GetDriveThroughWeekly(UserContext user, MetricParam param)
		{
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(user, param.EndDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, param.EndDate, fyInfo.FYDateStart.Value);
			FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x => x.StartDate <= param.EndDate && x.EndDate >= param.EndDate); //Get data Pos by fyPeriod start date and end date

			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, user, param.SitesKey);
			DateTime sDate = fyPeriod == null ? param.EndDate : fyPeriod.StartDate;
			DateTime eDate = (fyPeriod == null || fyPeriod.EndDate < param.EndDate) ? param.EndDate : fyPeriod.EndDate;

			var siteDataGroup = await GetDriveThroughSite(sites, sDate, eDate);

			var convSiteWeek = (from fyweek in fyPeriod.Weeks
								from datSite in siteDataGroup
								where datSite.Date >= fyweek.StartDate && datSite.Date <= fyweek.EndDate
								select new
								{
									ID = datSite.SiteKey,
									Name = datSite.SiteName,
									Date = datSite.Date,
									Count = datSite.Count,
									Dwell = datSite.Dwell,
									isRegion = false,
									ParentKey = datSite.RegionKey,
									ParentName = datSite.RegionName,
									CountSite = datSite.CountSite,
									Week = fyweek
								}).ToList();

			List<FiscalWeek> weekSearchList = convSiteWeek.Select(x => x.Week).Distinct().ToList();
			#region Site Data
			var hasDataSites = siteDataGroup.Select(x => new { x.SiteKey, x.SiteName }).Distinct().ToList();
			var SiteWithWeek = (from reg in hasDataSites
								from p in weekSearchList//fyPeriod.Weeks
								select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p.WeekIndex, week = p }).ToList();

			IEnumerable<DriveThroughData> dataSiteWeekly = convSiteWeek.GroupBy(x => new { x.ID, x.Week.WeekIndex })
				.Select(s => new DriveThroughData()
				{
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					isRegion = false,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					Title = s.Any() ? string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", s.FirstOrDefault().Week.StartDate, s.FirstOrDefault().Week.EndDate, s.FirstOrDefault().Week.WeekIndex) : string.Empty,
					TimeIndex = s.Key.WeekIndex
				}).ToList();

			IEnumerable<DriveThroughData> dataSiteWeeklyAll = (from si in SiteWithWeek
															   join cv in dataSiteWeekly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															   from it in dat.DefaultIfEmpty()
															   select new DriveThroughData()
															   {
																   ID = si.ID,
																   Name = si.Name,
																   Date = it != null ? it.Date : DateTime.MinValue,
																   Count = it != null ? it.Count : 0,
																   Dwell = it != null ? it.Dwell : 0,
																   isRegion = false,
																   ParentKey = it != null ? it.ParentKey : 0,
																   TimeIndex = si.week.WeekIndex,
																   Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.week.StartDate, si.week.EndDate, si.week.WeekIndex)
															  }).ToList();

			IEnumerable<DriveThroughDataSumary> dataBySiteSum = dataSiteWeekly.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataSiteWeeklyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();
			#endregion

			#region Region Data
			var hasDataRegions = siteDataGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithWeek = (from reg in hasDataRegions
								  from p in weekSearchList//fyPeriod.Weeks
								  select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p.WeekIndex, Week = p }).ToList();

			var regionGroup = convSiteWeek.GroupBy(x => new { x.ParentKey, x.Week.WeekIndex }).
				Select(s => new DriveThroughData()
				{
					ID = s.Key.ParentKey,
					Name = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Count = !s.Any() ? 0 : s.Sum(x => x.Count) / s.FirstOrDefault().CountSite,
					Dwell = !s.Any() ? 0 : s.Sum(x => x.Dwell) / s.FirstOrDefault().CountSite,//Convert.ToInt32(s.Average(x => x.Dwell)),
					isRegion = true,
					ParentKey = 0,
					TimeIndex = s.Key.WeekIndex
				});

			IEnumerable<DriveThroughData> dataByRegionAll = (from si in RegionWithWeek
															 join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															 from it in dat.DefaultIfEmpty()
															 select new DriveThroughData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = it != null ? it.Date : DateTime.MinValue,
																 Count = it != null ? it.Count : 0,
																 Dwell = it != null ? it.Dwell : 0,
																 isRegion = true,
																 ParentKey = it != null ? it.ParentKey : 0,
																 TimeIndex = si.TimeIndex,
																 Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.Week.StartDate, si.Week.EndDate, si.Week.WeekIndex)
															 }).ToList();

			IEnumerable<DriveThroughDataSumary> dataByRegionSum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataByRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();

			foreach (DriveThroughDataSumary data in dataByRegionSum)
			{
				data.Sites = dataBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			#endregion

			#region Total Sum Data
			DriveThroughTotalSumBase totalSumSite = dataBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSumBase totalSumRegion = dataByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSum totalSum = new DriveThroughTotalSum()
			{
				TotalSumRegion = totalSumRegion,
				TotalSumSite = totalSumSite
			};

			#endregion

			#region Chart Data
			int minTime = dataSiteWeekly.Any() ? dataSiteWeekly.Min(x => x.TimeIndex) : -1;
			int maxTime = dataSiteWeekly.Any() ? dataSiteWeekly.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullWeeks = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullWeeks = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<CountDwellChart> chartByRegs = null;
			IEnumerable<CountDwellChart> chartBySites = null;
			if (fullWeeks != null && fullWeeks.Any())
			{
				var RegionFullWeek = (from reg in hasDataRegions
									  from p in fullWeeks
									  select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p }).ToList();
				IEnumerable<DriveThroughData> chartByRegionAll = (from si in RegionFullWeek
																 join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																 from it in dat.DefaultIfEmpty()
																 select new DriveThroughData()
																 {
																	 ID = si.ID,
																	 Name = si.Name,
																	 Date = it != null ? it.Date : DateTime.MinValue,
																	 Count = it != null ? it.Count : 0,
																	 Dwell = it != null ? it.Dwell : 0,
																	 isRegion = true,
																	 ParentKey = it != null ? it.ParentKey : 0,
																	 TimeIndex = si.TimeIndex,
																	 Title = string.Format("{0}", si.TimeIndex)
																 }).ToList();

				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartByRegionAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});

				var SiteFullWeek = (from reg in hasDataSites
									from p in fullWeeks
									select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p }).ToList();
				IEnumerable<DriveThroughData> chartSiteWeeklyAll = (from si in SiteFullWeek
																   join cv in dataSiteWeekly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																   from it in dat.DefaultIfEmpty()
																   select new DriveThroughData()
																   {
																	   ID = si.ID,
																	   Name = si.Name,
																	   Date = it != null ? it.Date : DateTime.MinValue,
																	   Count = it != null ? it.Count : 0,
																	   Dwell = it != null ? it.Dwell : 0,
																	   isRegion = false,
																	   ParentKey = it != null ? it.ParentKey : 0,
																	   TimeIndex = si.TimeIndex,
																	   Title = string.Format("{0}", si.TimeIndex)
																   }).ToList();
				chartBySites = siteDataGroup.GroupBy(x => x.SiteKey).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().SiteName : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartSiteWeeklyAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});
			}
			//var chartDataAll = regionGroup.GroupBy(x => x.TimeIndex).Select(
			//	gr => new CountDwellChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = String.Format("Week {0}", gr.Key),
			//		Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
			//		Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
			//		Regions = dataByRegionAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		}),
			//		Sites = dataSiteWeeklyAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			DriveThroughDataAll retData = new DriveThroughDataAll();
			retData.DTData = dataByRegionSum;
			retData.TotalSum = totalSum;
			retData.ChartData = new CountDwellChartChartAll();
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<DriveThroughDataAll> GetDriveThroughMonthly(UserContext user, MetricParam param)
		{
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(user, param.EndDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, param.EndDate, fyInfo.FYDateStart.Value);
			fyPeriods = fyPeriods.Where(x => x.StartDate < param.EndDate).ToList();

			DateTime sDate = fyInfo.FYDateStart.HasValue ? fyInfo.FYDateStart.Value : param.EndDate;
			DateTime eDate = (fyInfo.FYDateEnd.HasValue && fyInfo.FYDateEnd.Value <= param.EndDate) ? fyInfo.FYDateEnd.Value : param.EndDate;

			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, user, param.SitesKey);
			var siteDataGroup = await GetDriveThroughSite(sites, sDate, eDate);

			var convSitePeriod = (from fyPer in fyPeriods
								from datSite in siteDataGroup
								  where datSite.Date >= fyPer.StartDate && datSite.Date <= fyPer.EndDate
								select new
								{
									ID = datSite.SiteKey,
									Name = datSite.SiteName,
									Date = datSite.Date,
									Count = datSite.Count,
									Dwell = datSite.Dwell,
									isRegion = false,
									ParentKey = datSite.RegionKey,
									ParentName = datSite.RegionName,
									CountSite = datSite.CountSite,
									Period = fyPer
								}).ToList();

			List<FiscalPeriod> periodSearchList = convSitePeriod.Select(x => x.Period).Distinct().ToList();
			#region Site Data
			var hasDataSites = siteDataGroup.Select(x => new { x.SiteKey, x.SiteName }).Distinct().ToList();
			var SiteWithPeriod = (from reg in hasDataSites
								  from p in periodSearchList//fyPeriods
								  select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p.Period, Period = p }).ToList();

			IEnumerable<DriveThroughData> dataSitePeriod = convSitePeriod.GroupBy(x => new { x.ID, x.Period.Period })
				.Select(s => new DriveThroughData()
				{
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					isRegion = false,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					Title = string.Format("{0}", s.Key.Period),
					TimeIndex = s.Key.Period
				}).ToList();

			IEnumerable<DriveThroughData> dataSitePeriodlyAll = (from si in SiteWithPeriod
															  join cv in dataSitePeriod on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															  from it in dat.DefaultIfEmpty()
															  select new DriveThroughData()
															  {
																  ID = si.ID,
																  Name = si.Name,
																  Date = it != null ? it.Date : DateTime.MinValue,
																  Count = it != null ? it.Count : 0,
																  Dwell = it != null ? it.Dwell : 0,
																  isRegion = false,
																  ParentKey = it != null ? it.ParentKey : 0,
																  TimeIndex = si.TimeIndex,
																  Title = string.Format("{0}", si.TimeIndex)
															  }).ToList();

			IEnumerable<DriveThroughDataSumary> dataBySiteSum = dataSitePeriod.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataSitePeriodlyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();
			#endregion

			#region Region Data
			//REGION DATA
			var hasDataRegions = siteDataGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithPeriod = (from reg in hasDataRegions
									from p in periodSearchList//fyPeriods
									select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p.Period, Period = p }).ToList();

			var regionGroup = convSitePeriod.GroupBy(x => new { x.ParentKey, x.Period.Period }).
				Select(s => new DriveThroughData()
				{
					ID = s.Key.ParentKey,
					Name = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Count = !s.Any() ? 0 : s.Sum(x => x.Count) / s.FirstOrDefault().CountSite,// ? s.Sum(x => x.Count) : 0,
					Dwell = !s.Any() ? 0 : s.Sum(x => x.Dwell) / s.FirstOrDefault().CountSite,//Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					isRegion = true,
					ParentKey = 0,
					TimeIndex = s.Key.Period,
					Title = string.Format("{0}", s.Key.Period)
				});

			IEnumerable<DriveThroughData> dataByRegionAll = (from si in RegionWithPeriod
															 join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															 from it in dat.DefaultIfEmpty()
															 select new DriveThroughData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = it != null ? it.Date : DateTime.MinValue,
																 Count = it != null ? it.Count : 0,
																 Dwell = it != null ? it.Dwell : 0,
																 isRegion = true,
																 ParentKey = it != null ? it.ParentKey : 0,
																 TimeIndex = si.TimeIndex,
																 Title = string.Format("{0}", si.TimeIndex)
															 }).ToList();

			IEnumerable<DriveThroughDataSumary> dataByRegionSum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataByRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();

			foreach (DriveThroughDataSumary data in dataByRegionSum)
			{
				data.Sites = dataBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			#endregion

			#region Total Sum Data
			DriveThroughTotalSumBase totalSumSite = dataBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSumBase totalSumRegion = dataByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSum totalSum = new DriveThroughTotalSum()
			{
				TotalSumRegion = totalSumRegion,
				TotalSumSite = totalSumSite
			};

			#endregion

			#region Chart Data
			int minTime = dataSitePeriod.Any() ? dataSitePeriod.Min(x => x.TimeIndex) : -1;
			int maxTime = dataSitePeriod.Any() ? dataSitePeriod.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullPeriods = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullPeriods = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<CountDwellChart> chartByRegs = null;
			IEnumerable<CountDwellChart> chartBySites = null;
			if (fullPeriods != null && fullPeriods.Any())
			{
				var RegionFullPeriod = (from reg in hasDataRegions
										from p in fullPeriods
										select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p }).ToList();
				IEnumerable<DriveThroughData> chartByRegionAll = (from si in RegionFullPeriod
																 join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																 from it in dat.DefaultIfEmpty()
																 select new DriveThroughData()
																 {
																	 ID = si.ID,
																	 Name = si.Name,
																	 Date = it != null ? it.Date : DateTime.MinValue,
																	 Count = it != null ? it.Count : 0,
																	 Dwell = it != null ? it.Dwell : 0,
																	 isRegion = true,
																	 ParentKey = it != null ? it.ParentKey : 0,
																	 TimeIndex = si.TimeIndex,
																	 Title = string.Format("{0}", si.TimeIndex)
																 }).ToList();
				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartByRegionAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});

				var SiteFullPeriod = (from reg in hasDataSites
									  from p in fullPeriods
									  select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p }).ToList();
				IEnumerable<DriveThroughData> chartSitePeriodlyAll = (from si in SiteFullPeriod
																	 join cv in dataSitePeriod on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DriveThroughData()
																	 {
																		 ID = si.ID,
																		 Name = si.Name,
																		 Date = it != null ? it.Date : DateTime.MinValue,
																		 Count = it != null ? it.Count : 0,
																		 Dwell = it != null ? it.Dwell : 0,
																		 isRegion = false,
																		 ParentKey = it != null ? it.ParentKey : 0,
																		 TimeIndex = si.TimeIndex,
																		 Title = string.Format("{0}", si.TimeIndex)
																	 }).ToList();
				chartBySites = siteDataGroup.GroupBy(x => x.SiteKey).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().SiteName : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartSitePeriodlyAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});
			}
			//var chartDataAll = dataSitePeriod.GroupBy(x => x.TimeIndex).Select(
			//	gr => new CountDwellChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = String.Format("Period {0}", gr.Key),
			//		Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
			//		Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
			//		Regions = dataByRegionAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		}),
			//		Sites = dataSitePeriodlyAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			DriveThroughDataAll retData = new DriveThroughDataAll();
			retData.DTData = dataByRegionSum;
			retData.TotalSum = totalSum;
			retData.ChartData = new CountDwellChartChartAll();
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<DriveThroughDataAll> GetDriveThroughHourly(UserContext user, MetricParam param)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, user, param.SitesKey);
			DateTime sDate = param.EndDate;
			DateTime eDate = param.EndDate;

			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());
			List<Func_BAM_DriveThroughHourly_Result> bamData = await MetricReportSvc.Func_BAM_DriveThroughHourly_Async(pacIds, sDate, eDate);// GET FROM DATABASE

			Commons.TEqualityComparer<SitePACID> icomparer = new Commons.TEqualityComparer<SitePACID>((a, b) => { return a.PACID == b.PACID & a.SiteKey == b.SiteKey; });
			IEnumerable<int> siteKeys = sites.Select(x => x.siteKey.HasValue ? x.siteKey.Value : 0).Distinct();
			IEnumerable<SitePACID> site_pacid = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct(icomparer);

			string[] includes = { typeof(tCMSWebRegion).Name };
			IQueryable<tCMSWebSites> dbSites = SiteSvc.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
			IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

			var site_region = site_pacid.Join(dbSites, si => si.SiteKey, rg => rg.siteKey, (si, rg) => new { siteKey = si.SiteKey, PACID = si.PACID, regionKey = rg.RegionKey, serverID = rg.ServerID }).Distinct().ToList();//site = si, region = rg
			var site_region_name = site_region.Join(dbRegions, si => si.regionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.PACID, siteKey = si.siteKey, siteName = si.serverID, regionKey = rg.RegionKey, regionName = rg.RegionName }).Distinct().ToList();
			//var region_site_count = dbRegions.GroupJoin(site_region_name, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();
			var countSiteInRegion = site_region_name.GroupBy(x => x.regionKey).Select(gr => new { regionKey = gr.Key, count = gr.Where(x => siteKeys.Contains(x.siteKey)).Select(x => x.siteKey).Distinct().Count() });

			var site_data = bamData.Join(site_region_name, drv => drv.PACID, si => si.pacid, (drv, si) =>
				new
				{
					pacid = si.pacid,
					siteKey = si.siteKey,
					siteName = si.siteName,
					regionKey = si.regionKey,
					regionName = si.regionName,
					dvrDate = drv.DVRDate,
					dvrHour = drv.DVRHour.HasValue ? drv.DVRHour.Value : 0,
					//count = si.count,
					count = drv.Count,
					dwell = drv.DWellTime
				}).Distinct();

			var siteDataGroupNC = site_data.GroupBy(cs => new { cs.siteKey, cs.regionKey, cs.dvrHour })
				.Select(s => new //DriveThroughDataSite()
				{
					Date = s.Any() ? s.FirstOrDefault().dvrDate.Value : DateTime.MinValue,
					SiteKey = s.Key.siteKey,
					SiteName = s.Any() ? s.FirstOrDefault().siteName : string.Empty,
					RegionKey = s.Key.regionKey,
					RegionName = s.Any() ? s.FirstOrDefault().regionName : string.Empty,
					Count = s.Any() ? s.Sum(x => x.count.HasValue ? x.count.Value : 0) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.dwell.HasValue ? x.dwell.Value : 0) : 0),
					TimeIndex = s.Key.dvrHour
				});
			//var siteDataGroup = siteDataGroupNC.GroupBy(cs => new { cs.siteKey, cs.regionKey, cs.dvrHour })
			//	.Select(s => new //DriveThroughDataSite()
			//	{
			//		Date = s.Any() ? s.FirstOrDefault().dvrDate.Value : DateTime.MinValue,
			//		SiteKey = s.Key.siteKey,
			//		SiteName = s.Any() ? s.FirstOrDefault().siteName : string.Empty,
			//		RegionKey = s.Key.regionKey,
			//		RegionName = s.Any() ? s.FirstOrDefault().regionName : string.Empty,
			//		Count = s.Any() ? s.Sum(x => x.count.HasValue ? x.count.Value : 0) : 0,
			//		Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.dwell.HasValue ? x.dwell.Value : 0) : 0),
			//		TimeIndex = s.Key.dvrHour
			//	});

			var siteDataGroup = siteDataGroupNC.Join(countSiteInRegion, s => s.RegionKey, reg => reg.regionKey, (s, rg) => new //DriveThroughDataSite()
				{
					Date = s.Date,
					SiteKey = s.SiteKey,
					SiteName = s.SiteName,
					RegionKey = s.RegionKey,
					RegionName = s.RegionName,
					Count = s.Count,
					Dwell = s.Dwell,
					CountSite = Math.Max(rg.count, 1),
					TimeIndex = s.TimeIndex
				}).Distinct();
			//var siteDataGroup = await GetDriveThroughSite(sites, sDate, eDate);

			#region Site Data
			//SITE DATA
			var Hours = siteDataGroup.Select(s => s.TimeIndex).OrderBy(x => x).Distinct();
			var SiteID = siteDataGroup.Select(s => new { ID = s.SiteKey, Name = s.SiteName, ParentKey = s.RegionKey }).Distinct();
			var SiteWithHour = (from hour in Hours
								from site in SiteID
								select new { TimeIndex = hour, SiteKey = site.ID, Name = site.Name, ParentKey = site.ParentKey }).ToList();

			IEnumerable<DriveThroughData> dataSiteHourlyAll = (from si in SiteWithHour
															   join cv in siteDataGroup on new { si.SiteKey, si.TimeIndex } equals new { cv.SiteKey, cv.TimeIndex } into dat
															   from it in dat.DefaultIfEmpty()
															   select new DriveThroughData()
															   {
																   ID = si.SiteKey,
																   Name = si.Name,
																   Date = it != null ? it.Date : DateTime.MinValue,
																   Count = it != null ? it.Count : 0,
																   Dwell = it != null ? it.Dwell : 0,
																   isRegion = false,
																   ParentKey = it != null ? it.RegionKey : 0,
																   TimeIndex = si.TimeIndex
															   }).ToList();

			IEnumerable<DriveThroughDataSumary> dataBySiteSum = siteDataGroup.GroupBy(x => x.SiteKey)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().SiteName : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().RegionKey : 0,
					isRegion = false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataSiteHourlyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();
			#endregion

			#region Region Data
			//REGION DATA
			var hasDataRegions = siteDataGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithHour = (from reg in hasDataRegions
								  from hh in Hours
								  select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = hh }).ToList();

			var regionGroup = siteDataGroup.GroupBy(x => new { x.RegionKey, x.TimeIndex }).
				Select(s => new
				{
					ID = s.Key.RegionKey,
					Name = s.Any() ? s.FirstOrDefault().RegionName : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Count = !s.Any() ? 0 : s.Sum(x => x.Count) / s.FirstOrDefault().CountSite,//s.Sum(x => x.Count) : 0,
					Dwell = !s.Any() ? 0 : s.Sum(x => x.Dwell) / s.FirstOrDefault().CountSite,//Convert.ToInt32(s.Average(x => x.Dwell)) : 0,
					isRegion = true,
					ParentKey = 0,
					TimeIndex = s.Key.TimeIndex
				});

			IEnumerable<DriveThroughData> dataByRegionAll = (from si in RegionWithHour
															 join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															 from it in dat.DefaultIfEmpty()
															 select new DriveThroughData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = it != null ? it.Date : DateTime.MinValue,
																 Count = it != null ? it.Count : 0,
																 Dwell = it != null ? it.Dwell : 0,
																 isRegion = true,
																 ParentKey = it != null ? it.ParentKey : 0,
																 TimeIndex = si.TimeIndex
															 }).ToList();

			IEnumerable<DriveThroughDataSumary> dataByRegionSum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new DriveThroughDataSumary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Count = s.Any() ? s.Sum(x => x.Count) : 0,
					Dwell = Convert.ToInt32(s.Any() ? s.Average(x => x.Dwell) : 0),
					DetailData = dataByRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.TimeIndex)
				}).ToList();

			foreach (DriveThroughDataSumary data in dataByRegionSum)
			{
				data.Sites = dataBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			#endregion

			#region Total Sum Data
			DriveThroughTotalSumBase totalSumSite = dataBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSumBase totalSumRegion = dataByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new DriveThroughTotalSumBase()
				{
					TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
					TotalDwell = s.Any() ? s.Sum(i => i.Dwell) : 0
				}).FirstOrDefault();
			DriveThroughTotalSum totalSum = new DriveThroughTotalSum()
			{
				TotalSumRegion = totalSumRegion,
				TotalSumSite = totalSumSite
			};

			#endregion

			#region Chart Data
			int minTime = siteDataGroup.Any() ? siteDataGroup.Min(x => x.TimeIndex) : -1;
			int maxTime = siteDataGroup.Any() ? siteDataGroup.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullHours = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullHours = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<CountDwellChart> chartByRegs = null;
			IEnumerable<CountDwellChart> chartBySites = null;
			if (fullHours != null && fullHours.Any())
			{
				var RegionFullHours = (from reg in hasDataRegions
									   from hh in fullHours
									   select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = hh }).ToList();
				IEnumerable<DriveThroughData> chartByRegionAll = (from si in RegionFullHours
																  join cv in regionGroup on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																  from it in dat.DefaultIfEmpty()
																  select new DriveThroughData()
																  {
																	  ID = si.ID,
																	  Name = si.Name,
																	  Date = it != null ? it.Date : DateTime.MinValue,
																	  Count = it != null ? it.Count : 0,
																	  Dwell = it != null ? it.Dwell : 0,
																	  isRegion = true,
																	  ParentKey = it != null ? it.ParentKey : 0,
																	  TimeIndex = si.TimeIndex
																  }).ToList();

				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartByRegionAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});

				var SiteFullHour = (from hour in fullHours
									from site in SiteID
									select new { TimeIndex = hour, SiteKey = site.ID, Name = site.Name, ParentKey = site.ParentKey }).ToList();
				IEnumerable<DriveThroughData> chartSiteHourlyAll = (from si in SiteFullHour
																	join cv in siteDataGroup on new { si.SiteKey, si.TimeIndex } equals new { cv.SiteKey, cv.TimeIndex } into dat
																	from it in dat.DefaultIfEmpty()
																	select new DriveThroughData()
																	{
																		ID = si.SiteKey,
																		Name = si.Name,
																		Date = it != null ? it.Date : DateTime.MinValue,
																		Count = it != null ? it.Count : 0,
																		Dwell = it != null ? it.Dwell : 0,
																		isRegion = false,
																		ParentKey = it != null ? it.RegionKey : 0,
																		TimeIndex = si.TimeIndex
																	}).ToList();
				chartBySites = siteDataGroup.GroupBy(x => x.SiteKey).Select(
						gr => new CountDwellChart()
						{
							TimeIndex = 0,
							ID = gr.Key,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							Name = gr.Any() ? gr.FirstOrDefault().SiteName : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Details = chartSiteHourlyAll.Where(x => x.ID == gr.Key).Select(x => new CountDwellChart()
							{
								ID = x.ID,
								Name = x.Name,
								TimeIndex = x.TimeIndex,
								Date = x.Date,
								Dwell = x.Dwell,
								Count = x.Count
							})
						});
			}
			//var chartDataAll = siteDataGroup.GroupBy(x => x.TimeIndex).Select(
			//	gr => new CountDwellChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = string.Format(Consts.CHART_LEGEND_HOUR_FORMAT, gr.Key, gr.Key + 1),
			//		Dwell = Convert.ToInt32(gr.Any() ? gr.Average(x => x.Dwell) : 0),
			//		Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
			//		Regions = dataByRegionAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		}),
			//		Sites = dataSiteHourlyAll.Where(x => x.TimeIndex == gr.Key).Select(x => new CountDwellChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.TimeIndex,
			//			Dwell = x.Dwell,
			//			Count = x.Count
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			DriveThroughDataAll retData = new DriveThroughDataAll();
			retData.DTData = dataByRegionSum;
			retData.TotalSum = totalSum;
			retData.ChartData = new CountDwellChartChartAll();
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}
		#endregion

        #region Normalize
        //public async void GetNormalizeBySite(UserContext user, List<int> parKeys)
        public async Task<List<Normalizes>> GetNormalizeBySite(UserContext user, MetricParam param)
        {
            //List<int> parKeys, DateTime searchDate;
            //DateTime searchDate = DateTime.ParseExact("20110109", "yyyyMMdd", null);
            //sites = await base.UserSitesAsync(base.DataService, user);
            //selectedSites.Clear();
            //selectedSites.Add(325);
            IEnumerable<UserSiteDvrChannel> sites = null;
            List<int> selectedSites = param.SitesKey;

            if (selectedSites != null && selectedSites.Count > 0)
            {
                IEnumerable<UserSiteDvrChannel> allsites = await base.UserSitesAsync(base.DataService, user);
                sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
            }
            else
            {
                sites = await base.UserSitesAsync(base.DataService, user);
            }
            IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();

           
            List<int> pacids = site_pac.Select(x => x.PACID).Distinct().ToList();
            string[] includes_POS = { typeof(Fact_POS_Periodic_Hourly_Transact).Name };
            IQueryable<Fact_POS_Periodic_Hourly_Transact> dbPOSNormal = POSService.GetNormalizeTrans<Fact_POS_Periodic_Hourly_Transact>(pacids, param.SetDate, item => item, includes_POS);
            var dbPOSNormals = dbPOSNormal.Select( t =>
                new 
                { 
                    PACID = t.PACID,
                    DVRDateKey = t.DVRDateKey,
                    Hour = t.TransHour,
                    Normalize = t.Normalize,
                    ReportNormalize = t.ReportNormalize
                }).ToList();

            //var result = dbPOSNormals.Join(site_pac, pos => pos.PACID, dvr => dvr.PACID, (pos, dvr) =>
            //    new {
            //        PACID = pos.PACID,
            //        DVRDateKey = pos.DVRDateKey,
            //        Hour = pos.Hour,
            //        Normalize = pos.Normalize
            //    }).ToList();

            //var Nor = result.Where(t => t.Normalize == true);
            //var Actual = result.Where(t => t.Normalize == false);

            var POS = dbPOSNormals.GroupBy(x => x.Hour).Select(
				t => new NormalizeCountBase()
				{
                    Hour = t.Key,
                    TotalActualize = t.Any() ? t.Count(a => a.Normalize == false) : 0,
                    TotalNormalize = t.Any() ? t.Count(a => a.Normalize == true) : 0,
                    flag = (bool)t.FirstOrDefault().ReportNormalize
                });

            string[] includes_IOPC = { typeof(Fact_IOPC_Periodic_Hourly_Traffic).Name };
            IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> dbIOPCNormal = IIOPCService.GetNormalizeTraffics<Fact_IOPC_Periodic_Hourly_Traffic>(pacids, param.SetDate, item => item, includes_IOPC);
            var dbIOPCNormals = dbIOPCNormal.Select(t =>
                new
                {
                    PACID = t.PACID,
                    DVRDateKey = t.DVRDateKey,
                    Hour = t.C_Hour,
                    Normalize = t.Normalize,
                    ReportNormalize = t.ReportNormalize
                }).ToList();

            var IOPC = dbIOPCNormals.GroupBy(x => x.Hour).Select(
                t => new NormalizeCountBase()
                {
                    Hour = t.Key,
                    TotalActualize = t.Any() ? t.Count(a => a.Normalize == false) : 0,
                    TotalNormalize = t.Any() ? t.Count(a => a.Normalize == true) : 0,
                    flag = (bool)t.FirstOrDefault().ReportNormalize
                });

            List<Normalizes> data = new List<Normalizes>(){
                new Normalizes()
                {
                    Date = param.SetDate,
                    Type = 1,
                    NormalizeTime = POS
                },
                new Normalizes()
                {
                    Date = param.SetDate,
                    Type = 2,
                    NormalizeTime = IOPC
                }
             };

            return data;
        }
        public async Task<string> UpdateNormalize(UserContext user, NormalizeParam param)
        {
            //MetricReportSvc.DeleteMetricReportUser(mectricUpdate.ReportId, user.ID);

            //foreach (var metric in mectricUpdate.Metrics)
            //{
            //    var usermetric = new tbl_BAM_Metric_ReportUser
            //    {
            //        Active = metric.Active,
            //        MetricID = (short)metric.MetricId,
            //        MetricOrder = (byte?)metric.Order,
            //        ReportID = (short)mectricUpdate.ReportId,
            //        UserID = user.ID
            //    };

            //    MetricReportSvc.InsertMetricReportUser(usermetric);
            //}

            //MetricReportSvc.SaveChange();
            string result = string.Empty;
            IEnumerable<UserSiteDvrChannel> sites = null;
            List<int> selectedSites = param.SitesKey;

            if (selectedSites != null && selectedSites.Count > 0)
            {
                Task<IEnumerable<UserSiteDvrChannel>> Tallsites = base.UserSitesAsync(base.DataService, user);//.Result;
                IEnumerable<UserSiteDvrChannel> allsites = await Tallsites;//.Result;

                sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
            }
            else
            {
                sites = base.UserSitesAsync(base.DataService, user).Result;
            }
            IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();

            List<int> pacids = site_pac.Select(x => x.PACID).Distinct().ToList();
            
            if (param.Type == 1)
            {
                string[] includes_POS = { typeof(Fact_POS_Periodic_Hourly_Transact).Name };
                IQueryable<Fact_POS_Periodic_Hourly_Transact> dbPOSNormal = POSService.GetNormalizeTrans<Fact_POS_Periodic_Hourly_Transact>(pacids, param.Date, item => item, includes_POS);
                var dbPOSNormals = dbPOSNormal.Select(
                    si => new
                    { 
                        PACID = si.PACID,
                        DVRDateKey = si.DVRDateKey,
                        TransHour = si.TransHour,
                        Count_Trans = si.Count_Trans,
                        TotalAmount = si.TotalAmount,
                        Count_Trans_N = si.Count_Trans_N,
                        TotalAmount_N = si.TotalAmount_N,
                        Normalize = si.Normalize,
                        ReportNormalize = param.ReportNormalize,
                        UpdateTimeInt = si.UpdateTimeInt
                    }).ToList();

                var model = dbPOSNormals.Where(t => t.TransHour == param.Hour).Select(
                    si => new Fact_POS_Periodic_Hourly_Transact() 
                    {
                        PACID = si.PACID,
                        DVRDateKey = si.DVRDateKey,
                        TransHour = si.TransHour,
                        Count_Trans = si.Count_Trans,
                        TotalAmount = si.TotalAmount,
                        Count_Trans_N = si.Count_Trans_N,
                        TotalAmount_N = si.TotalAmount_N,
                        Normalize = si.Normalize,
                        ReportNormalize = param.ReportNormalize,
                        UpdateTimeInt = si.UpdateTimeInt
                    });

                result = POSService.UpdateReportNormalizeTrans(model);
                
            }
            else
            {
                string[] includes_IOPC = { typeof(Fact_IOPC_Periodic_Hourly_Traffic).Name };
                IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> dbIOPCNormal = IIOPCService.GetNormalizeTraffics<Fact_IOPC_Periodic_Hourly_Traffic>(pacids, param.Date, item => item, includes_IOPC);
                var dbIOPCNormals = dbIOPCNormal.Select(
                    si => new
                    {
                        PACID = si.PACID,
                        DVRDateKey = si.DVRDateKey,
                        C_Hour = si.C_Hour,
                        CameraID = si.CameraID,
                        Count_IN = si.Count_IN,
                        Count_OUT = si.Count_OUT,
                        Count_IN_N = si.Count_IN_N,
                        Count_OUT_N = si.Count_OUT_N,
                        Normalize = si.Normalize,
                        ReportNormalize = param.ReportNormalize,
                        UpdateTimeInt = si.UpdateTimeInt
                    }).ToList();

                var model = dbIOPCNormals.Where(t => t.C_Hour == param.Hour).Select(
                    si => new Fact_IOPC_Periodic_Hourly_Traffic()
                    {
                        PACID = si.PACID,
                        DVRDateKey = si.DVRDateKey,
                        C_Hour = si.C_Hour,
                        CameraID = si.CameraID,
                        Count_IN = si.Count_IN,
                        Count_OUT = si.Count_OUT,
                        Count_IN_N = si.Count_IN_N,
                        Count_OUT_N = si.Count_OUT_N,
                        Normalize = si.Normalize,
                        ReportNormalize = param.ReportNormalize,
                        UpdateTimeInt = si.UpdateTimeInt
                    });

                result = IIOPCService.UpdateReportNormalizeTraffics(model);
            }
            return result;
        }
        #endregion
	}
}
