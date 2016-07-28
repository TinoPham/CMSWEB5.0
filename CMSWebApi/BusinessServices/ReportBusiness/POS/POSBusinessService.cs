using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Data.Entity;
using CMSWebApi.DataModels.DashBoardCache;
using CMSWebApi.Cache.Caches;
using Extensions;
using Extensions.Linq;
using CMSWebApi.BusinessServices.ReportBusiness;
using CMSWebApi.BusinessServices.ReportBusiness.Interfaces;
using System.Web.Http;

namespace CMSWebApi.BusinessServices.ReportBusiness.POS
{
	public partial class POSBusinessService : BusinessBase<IUsersService>,IPOSBusinessService
	{
		public IPOSService IPOSService{ get ;set;}
		public IGoalTypeService IGoalTypeService { get; set;}
		public ISiteService ISiteService { get ;set;}
		public ICommonInfoService ICommonInfoService{ get ;set;}
		public IIOPCService IIOPCService { get; set; }

		#region Compare
		public async Task<ALertCompModel> POSExceptionCompare(IEnumerable<int> pacids,IEnumerable<int>exceptions, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate)
		{
			IEnumerable<int> exTranTypes = (exceptions != null && exceptions.Any()) ? exceptions : IPOSService.GetTransType<int>(null, item => item.TransactionTypeID, null);

			Task<List<Func_Count_Exception_Trans_Result>> Ttrans = CountPOSExceptionTrans(sdate, date, pacids, exTranTypes);

			List<Func_Count_Exception_Trans_Result> ie_trans = await Ttrans;

			Task<List<Func_Count_Exception_Trans_Result>> Tcmp_trans = CountPOSExceptionTrans(scmpdate, cmpdate, pacids, exTranTypes);

			List<Func_Count_Exception_Trans_Result> ie_cmp_trans = await Tcmp_trans;

			Int64 trans = ie_trans.Any()? ie_trans.Sum( it => (Int64)it.Trans.Value) : 0;
			Int64 cmp_trans = ie_cmp_trans.Any()? ie_cmp_trans.Sum( it => (Int64)it.Trans.Value) : 0;
			return new ALertCompModel { Value = trans, CmpValue = trans.toValueCompare(cmp_trans), Increase = (trans == cmp_trans) ? (bool?)null : (trans > cmp_trans) };
		}

		public async Task<ALertCompModel> POSConversionCompare(IEnumerable<UserSiteDvrChannel> uSites, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate)
		{
			IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			IEnumerable<int> pacids = site_pac.Select(x => x.PACID).Distinct();
			bool isasync = false;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

			//Task<List<Proc_DashBoard_Conversion_Result>> Tconv_raw ;
			//Task<List<Proc_DashBoard_Conversion_Result>> Tcmp_conv_raw = null;
			//List<Proc_DashBoard_Conversion_Result> cmp_conv_raw;
			//List<Proc_DashBoard_Conversion_Result> conv_raw;
			//Tconv_raw = GetConversion(sdate, date, pacids, lsEnableChannels, ref isasync);
			//if( isasync)
			//{
			//	Tcmp_conv_raw = GetConversion(scmpdate, cmpdate, pacids, lsEnableChannels, ref isasync);
			//	conv_raw = await Tconv_raw;
			//	cmp_conv_raw = await Tcmp_conv_raw;
			//}
			//else
			//{
			//	conv_raw = await Tconv_raw;
			//	Tcmp_conv_raw = GetConversion(scmpdate, cmpdate, pacids, lsEnableChannels, ref isasync);
			//	cmp_conv_raw = await Tcmp_conv_raw;
			//}
			//int pacid_count = pacids.Count();
			//decimal conver = AverageConversion(conv_raw, pacid_count);//(conv == null || pacid_count == 0) ? 0 : (conv.AvgConv / pacid_count);//conv.TotalTraffic > 0 ? conv.TotalTrans * 100 / conv.TotalTraffic : 0;
			//decimal cmp_conver = AverageConversion(cmp_conv_raw, pacid_count);//(cmp_conv == null || pacid_count == 0) ? 0 : (cmp_conv.AvgConv / pacid_count);//cmp_conv.TotalTraffic > 0 ? cmp_conv.TotalTrans * 100 / cmp_conv.TotalTraffic : 0;
			IEnumerable<int> sitekeys = uSites.Where(item => item.siteKey.HasValue && item.siteKey.Value > 0).Select(it => it.siteKey.Value).Distinct();
			int siteCount = sitekeys.Count();
			//int siteCount = site_pac.Select(x=>x.SiteKey).Distinct().Count();

			var curConv = await GetSiteConversion(site_pac, pacids, sdate, date, lsEnableChannels);
			var cmdConv = await GetSiteConversion(site_pac, pacids, scmpdate, cmpdate, lsEnableChannels);
			decimal conver = curConv.Any() ? curConv.Sum(x => x.Conv) / Math.Max(1, siteCount) : 0;
			decimal cmp_conver = cmdConv.Any() ? cmdConv.Sum(x => x.Conv) / Math.Max(1, siteCount) : 0;

			return new ALertCompModel { Value = Math.Round(conver, 2), CmpValue = conver.toValueCompare(cmp_conver), Increase = (conver == cmp_conver) ? (bool?)null : (conver > cmp_conver) };
		}

		public async Task<ALertCompModel> TransactionCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate)
		{
			bool isasync = false;
			Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> Ttrans_raw;
			List<Func_Fact_POS_Periodic_Daily_Transact_Result> trans_raw;
			Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> Tcmp_trans_raw;
			List<Func_Fact_POS_Periodic_Daily_Transact_Result> cmp_trans_raw;

			Ttrans_raw = GetPOSTransaction(sdate, date, pacids, ref isasync);
			if( isasync)
			{
				Tcmp_trans_raw = GetPOSTransaction(scmpdate, cmpdate, pacids, ref isasync);
				cmp_trans_raw = await Tcmp_trans_raw;
				trans_raw = await Ttrans_raw;
			}
			else 
			{
				trans_raw = await Ttrans_raw;
				Tcmp_trans_raw = GetPOSTransaction(scmpdate, cmpdate, pacids, ref isasync);
				cmp_trans_raw = await Tcmp_trans_raw;
			}

			decimal conver = trans_raw.Any() ? trans_raw.Sum(it => it.Count_Trans.HasValue ? it.Count_Trans.Value : 0) : 0;
			decimal cmp_conver = cmp_trans_raw.Any() ? cmp_trans_raw.Sum(it => it.Count_Trans.HasValue ? it.Count_Trans.Value : 0) : 0;

			return new ALertCompModel { Value = Math.Round(conver, 2), CmpValue = conver.toValueCompare(cmp_conver), Increase = (conver == cmp_conver) ? (bool?)null : conver > cmp_conver };
		}

		public async Task<ALertCompModel> TotalSaleCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate)
		{
			bool isasync = false;
			Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> Tconv_raw;
			List<Func_Fact_POS_Periodic_Daily_Transact_Result> conv_raw;
			Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> Tcmp_conv_raw;
			List<Func_Fact_POS_Periodic_Daily_Transact_Result> cmp_conv_raw;
			Tconv_raw =  GetPOSTransaction(sdate, date, pacids, ref isasync);
			if( isasync)
			{
				Tcmp_conv_raw = GetPOSTransaction(scmpdate, cmpdate, pacids, ref isasync);
				conv_raw = await Tconv_raw;
				cmp_conv_raw = await Tcmp_conv_raw;

			}
			else
			{
				conv_raw = await Tconv_raw;
				Tcmp_conv_raw = GetPOSTransaction(scmpdate, cmpdate, pacids, ref isasync);
				cmp_conv_raw = await Tcmp_conv_raw;
			}


			decimal conver = conv_raw.Any() ? conv_raw.Sum(it => it.TotalAmount.HasValue ? it.TotalAmount.Value : 0) : 0;
			decimal cmp_conver = cmp_conv_raw.Any() ? cmp_conv_raw.Sum(it => it.TotalAmount.HasValue ? it.TotalAmount.Value : 0) : 0;
			return new ALertCompModel { Value = Math.Round(conver, 2), CmpValue = conver.toValueCompare(cmp_conver), Increase = conver == cmp_conver ? (bool?)null : conver > cmp_conver };
		}

		private Func<List<Proc_DashBoard_Conversion_Result>, POSConversionModel> F_convModel = it => new POSConversionModel
		{
			AvgConv = it.Any() ? it.Sum(x => ((!x.CountTrans.HasValue || !x.TrafficIn.HasValue || x.TrafficIn.Value == 0) ? 0 : (decimal)x.CountTrans.Value * 100 / Math.Max(1, (decimal)x.TrafficIn.Value)) > 150 ? 0 :
				  (!x.CountTrans.HasValue || !x.TrafficIn.HasValue || x.TrafficIn.Value == 0) ? 0 : (decimal)x.CountTrans.Value * 100 / Math.Max(1, (decimal)x.TrafficIn.Value)) : 0
		};

		private decimal AverageConversion(List<Proc_DashBoard_Conversion_Result> convdata, int pacidCount)
		{
			POSConversionModel conv = !convdata.Any() ? new POSConversionModel { TotalTrans = 0, TotalTraffic = 0, AvgConv = 0 } : F_convModel.Invoke(convdata);
			return (conv == null || pacidCount == 0) ? 0 : (conv.AvgConv / pacidCount);
		}
		#endregion

		#region Column charts
		public async  Task<ConversionChartModel> ConversionChartModel(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites)
		{
			
			IEnumerable<int> pacids = sites.Where(item => item.PACID.HasValue && item.PACID.Value > 0).Select(it => it.PACID.Value).Distinct();
			IEnumerable<int> sitekeys = sites.Where(item => item.siteKey.HasValue && item.siteKey.Value > 0).Select(it => it.siteKey.Value).Distinct();
			bool isasync = false;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

			isasync = false;
			Task<List<Proc_DashBoard_Conversion_Result>> Tconvdata = GetConversion(sdate, edate,  pacids, lsEnableChannels, ref isasync);
			Task<List<tCMSWeb_GoalType_Map>> TGoalMaps = GetGoalMapsbyGoalID(Utils.GoalType.Conversion);
			Task<List<int>> TGoalIds = GetGoalBySites(sitekeys);

			IEnumerable<int> GoalIds = await TGoalIds;
			IEnumerable<tCMSWeb_GoalType_Map> GoalMaps = await TGoalMaps;
			List<Proc_DashBoard_Conversion_Result> convdata = await Tconvdata;

			IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			var conv_sitekey = site_pac.Join(convdata, si => si.PACID, cv => cv.PACID, (si, cv) => new { siteKey = si.SiteKey, pacid = si.PACID, dvrdate = cv.DVRDate, trans = cv.CountTrans, traffic = cv.TrafficIn });
			var conv_site = conv_sitekey.GroupBy(x => x.siteKey).Select(gr => new 
				{
					siteKey = gr.Key,
					totalTrans = gr.Any() ? gr.Sum(x=>x.trans ?? 0) : 0,
					totalTraffic = gr.Any() ? gr.Sum(x=>x.traffic ?? 0) : 0
				});
			var convBySite = conv_site.Select(x => new { siteKey = x.siteKey, Conv = (x.totalTrans == 0 || x.totalTraffic == 0) ? 0 : (decimal)(x.totalTrans * 100) / Math.Max(1, x.totalTraffic) });

			//Average for all sites (include no data site)
			int siteCount = sitekeys.Count();
			decimal avg_conversion = convBySite.Any() ? convBySite.Sum(x => x.Conv > 150 ? 0 : x.Conv) / Math.Max(siteCount, 1) : 0;//AverageConversion(convdata, pacids.Count());//!groups.Any() ? 0: groups.Average( git => !git.Any() ? 0 : git.Average(x=>x.Conv));

			int totalTrans = convdata.Sum(x => x.CountTrans.HasValue ? x.CountTrans.Value : 0);
			int totalTraffic = convdata.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0);

			var gmaps = GoalMaps.Join(GoalIds, gmap => gmap.GoalID, gid => gid, (gmap, gid) => gmap);
			double min_goal = !gmaps.Any() ? 0: gmaps.Average( item => item.MinValue.HasValue? item.MinValue.Value : 0);
			//max goal value is 100
			double max_goal = !gmaps.Any() ? 100 : gmaps.Average(item => item.MaxValue.HasValue ? item.MaxValue.Value : 0);

			return new ConversionChartModel { Value = avg_conversion, goalMax = max_goal, goalMin = min_goal, trans = totalTrans, traffic = totalTraffic };
		}

		public async Task<IEnumerable<ConvSitesChartModel>> ConversionChartBySites(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites, int stateID, int top)
		{
			//get dvr have site& pacid
			IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			//var site_pac_count = site_pac.GroupJoin(site_pac, s => s.sitekey, sc => sc.sitekey, (s, sc) => new { pacid = s.pacid, sitekey = s.sitekey, count = sc.Count() });

			IEnumerable<int> pacids = site_pac.Select(item => item.PACID).Distinct().ToList();
			IEnumerable<int> sitekeys = site_pac.Select(sp => sp.SiteKey).Distinct();

			var QSite = ISiteService.GetSites(sk => sitekeys.Contains(sk.siteKey) && (stateID == 0 || ((sk.StateProvince.HasValue ? sk.StateProvince.Value : 0) == stateID)), it => new { siteKey = it.siteKey, siteName = it.ServerID, state = it.StateProvince.HasValue ? it.StateProvince.Value : 0 }, null).ToList();

			bool isasync = false;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

			//IEnumerable<Proc_DashBoard_Conversion_Result> conv = null;
			//Task<List<Proc_DashBoard_Conversion_Result>> TconvLY = null;
			//IEnumerable<Proc_DashBoard_Conversion_Result> convLY = null;
			//isasync = false;
			//Task<List<Proc_DashBoard_Conversion_Result>> Tconv = GetConversion(sdate, edate, pacids, lsEnableChannels, ref isasync); //IPOSService.GetPOSConversionAsync(string.Join(",", pacids), sdate, edate);
			////get data for last year
			//if (!isasync)
			//{
			//	conv = await Tconv;
			//	TconvLY = GetConversion(sdate.AddYears(-1), edate.AddYears(-1), pacids, lsEnableChannels, ref isasync);
			//}
			//else
			//{
			//	TconvLY = GetConversion(sdate.AddYears(-1), edate.AddYears(-1), pacids, lsEnableChannels, ref isasync);
			//	conv = await Tconv;
			//}
			//convLY = await TconvLY;

			//var site_data = site_pac.Join(conv, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate ?? DateTime.MinValue, countTrans = cv.CountTrans ?? 0, trafficIn = cv.TrafficIn ?? 0 });
			//var site_dataLY = site_pac.Join(convLY, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate ?? DateTime.MinValue, countTrans = cv.CountTrans ?? 0, trafficIn = cv.TrafficIn ?? 0 });

			//var site_dataSum = site_data.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(gr => new { siteKey = gr.Key.siteKey, DVRDate = gr.Key.dvrdate, totalTrans = gr.Any() ? gr.Sum(x=>x.countTrans) : 0, totalTraffic = gr.Any() ? gr.Sum(x=>x.trafficIn) : 0 });
			//var site_dataSumLY = site_dataLY.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(gr => new { siteKey = gr.Key.siteKey, DVRDate = gr.Key.dvrdate, totalTrans = gr.Any() ? gr.Sum(x => x.countTrans) : 0, totalTraffic = gr.Any() ? gr.Sum(x => x.trafficIn) : 0 });

			////_count _count
			////var site_name = site_pac.Join(QSite, sp => sp.sitekey, sn => sn.siteKey, (sp, sn) => new { siteKey = sp.sitekey, pacid = sp.pacid, sname = sn.siteName});//.ToList();
			////var site_conv = site_pac.Join(conv, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate, cval = (!cv.CountTrans.HasValue || !cv.TrafficIn.HasValue || cv.TrafficIn.Value == 0) ? 0 : ((decimal)cv.CountTrans.Value * 100) / Math.Max(1, (decimal)cv.TrafficIn.Value) });
			////var site_convLY = site_pac.Join(convLY, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate, cval = (!cv.CountTrans.HasValue || !cv.TrafficIn.HasValue || cv.TrafficIn.Value == 0) ? 0 : ((decimal)cv.CountTrans.Value * 100) / Math.Max(1, (decimal)cv.TrafficIn.Value) });
			//var siteday_conv = site_dataSum.Select(si => new { siteKey = si.siteKey, DVRDate = si.DVRDate, Conv = (si.totalTraffic == 0 || si.totalTrans == 0) ? 0 : ((decimal)si.totalTrans * 100) / Math.Max(1, si.totalTraffic) });
			//var siteday_convLY = site_dataSumLY.Select(si => new { siteKey = si.siteKey, DVRDate = si.DVRDate, Conv = (si.totalTraffic == 0 || si.totalTrans == 0) ? 0 : ((decimal)si.totalTrans * 100) / Math.Max(1, si.totalTraffic) });

			////Apply new way to calc Conversion rate
			////var siteday_conv = site_conv.GroupBy(it => new { it.siteKey, it.dvrdate }, it => it).Select(group => new
			////{
			////	siteKey = group.Key.siteKey,
			////	dvrdate = group.Key.dvrdate,
			////	cval = (group.Any() && group.FirstOrDefault() != null && group.FirstOrDefault().count > 0) ? (group.Sum(c => (c.cval > 150 ? 0 : c.cval)) / group.FirstOrDefault().count) : 0
			////});
			////var siteday_convLY = site_convLY.GroupBy(it => new { it.siteKey, it.dvrdate }, it => it).Select(group => new
			////{
			////	siteKey = group.Key.siteKey,
			////	dvrdate = group.Key.dvrdate,
			////	cval = (group.Any() && group.FirstOrDefault() != null && group.FirstOrDefault().count > 0) ? (group.Sum(c => (c.cval > 150 ? 0 : c.cval)) / group.FirstOrDefault().count) : 0
			////});

			////IEnumerable<Proc_DashBoard_Conversion_Result> conv = await Tconv;
			////var ret = site_name.GroupJoin(conv, st => st.pacid, cv => cv.PACID, (st, cv) => new { siteKey = st.siteKey, sname = st.sname, gconv = cv }).Distinct();
			////var retLY = site_name.GroupJoin(convLY, st => st.pacid, cv => cv.PACID, (st, cv) => new { siteKey = st.siteKey, gconv = cv }).Distinct();
			//var ret = QSite.GroupJoin(siteday_conv, si => si.siteKey, cv => cv.siteKey, (si, cv) => new { siteKey = si.siteKey, sname = si.siteName, Conv = !cv.Any() ? -1 : cv.Where(x => x.Conv > 0).Any() ? cv.Where(x => x.Conv > 0).Average(c => c.Conv > 150 ? 0 : c.Conv) : 0 }).Distinct();
			//var retLY = QSite.GroupJoin(siteday_convLY, si => si.siteKey, cv => cv.siteKey, (si, cv) => new { siteKey = si.siteKey, sname = si.siteName, Conv = !cv.Any() ? -1 : cv.Where(x => x.Conv > 0).Any() ? cv.Where(x => x.Conv > 0).Average(c => c.Conv > 150 ? 0 : c.Conv) : 0 }).Distinct();

			////var retData = ret.Select(it => new { key = it.siteKey, name = it.sname, value = (it.gconv.Sum(itrans => itrans.TotalTrans.HasValue ? itrans.TotalTrans.Value : 0) * 100) / Math.Max(1, it.gconv.Sum(itraff => itraff.TotalTraffic.HasValue ? (decimal)itraff.TotalTraffic.Value : 0)) });
			////var retLastYear = retLY.Select(it => new { key = it.siteKey, value = (it.gconv.Sum(itrans => itrans.TotalTrans.HasValue ? itrans.TotalTrans.Value : 0) * 100) / Math.Max(1, it.gconv.Sum(itraff => itraff.TotalTraffic.HasValue ? (decimal)itraff.TotalTraffic.Value : 0)) });
			//var retData = ret.Where(x => x.Conv >= 0).Select(it => new { key = it.siteKey, name = it.sname, value = it.Conv });
			//var retLastYear = retLY.Where(x => x.Conv >= 0).Select(it => new { key = it.siteKey, value = it.Conv });
			var siteConv = await GetSiteConversion(site_pac, pacids, sdate, edate, lsEnableChannels);
			var siteConvLY = await GetSiteConversion(site_pac, pacids, sdate.AddYears(-1), edate.AddYears(-1), lsEnableChannels);

			var retData = siteConv.Join(QSite, cv => cv.SiteKey, si => si.siteKey, (cv, si) => new { key = cv.SiteKey, name = si.siteName, value = cv.Conv });
			var retLastYear = siteConvLY.Join(QSite, cv => cv.SiteKey, si => si.siteKey, (cv, si) => new { key = cv.SiteKey, name = si.siteName, value = cv.Conv });

			return (from cy in retData
					join ly in retLastYear on cy.key equals ly.key into dat
					from it in dat.DefaultIfEmpty()
					select new ConvSitesChartModel
						{
							Label = cy.name,
							Value = cy.value,
							LastYear = (it == null) ? 0 : it.value
						}).Distinct().OrderByDescending(x=>x.Value).Take(top);
			/*
			var result = (from cy in ret
						 join ly in retLY on cy.siteKey equals ly.siteKey into dat
						 from it in dat.DefaultIfEmpty()
						 select new
						 {
							 siteKey = cy.siteKey,
							 sname = cy.sname,
							 gconv = cy.gconv,
							 gconvLY = it.gconv
						 }).Distinct();

			return result.Select(item => new ConvSitesChartModel
			{
				Label = item.sname, 
				Value = (item.gconv.Sum(itrans => itrans.TotalTrans.HasValue ? itrans.TotalTrans.Value : 0) * 100) / Math.Max(1, item.gconv.Sum( itraff => itraff.TotalTraffic.HasValue?(decimal)itraff.TotalTraffic.Value : 0)),
				LastYear = item.gconvLY == null ? 0 : (item.gconvLY.Sum(itrans => itrans.TotalTrans.HasValue ? itrans.TotalTrans.Value : 0) * 100) / Math.Max(1, item.gconvLY.Sum(itraff => itraff.TotalTraffic.HasValue ? (decimal)itraff.TotalTraffic.Value : 0))
			});
			*/
		}
		#endregion

		#region Map chart

		public async Task<IEnumerable<ConvMapChartModel>> ConversionMapchart(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites)
		 {
			//get dvr have site& pacid
			IEnumerable<SitePACID> site_pac = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			//var site_pac_count = site_pac.GroupJoin(site_pac, s => s.sitekey, sc => sc.sitekey, (s, sc) => new { pacid = s.pacid, sitekey = s.sitekey, count = sc.Count() });

			//get sites have state province
			var Qstates = ISiteService.GetSites(site_pac.Select(sp => sp.SiteKey).Distinct(), it => new { siteKey = it.siteKey, state = it.StateProvince.HasValue ? it.StateProvince.Value : 0 }, null);

			var states = Qstates.AsEnumerable().Where(item => item.state > 0).ToList().AsEnumerable();
			//filter site have pacid & state
			var site_sate = site_pac.Join(states, si => si.SiteKey, st => st.siteKey, (si, st) => new { site = si, state = st });

			IEnumerable<int> pacids = site_sate.Select( item => item.site.PACID).Distinct().ToList();
			IEnumerable<int> stateids = site_sate.Select(item => (int)item.state.state).Distinct();

			bool isasync = false;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);
			//Task<List<Proc_DashBoard_Conversion_Result>> TconvLY = null;
			//IEnumerable<Proc_DashBoard_Conversion_Result> conv = null;
			//isasync = false;
			//Task<List<Proc_DashBoard_Conversion_Result>> Tconv = GetConversion(sdate, edate, pacids, lsEnableChannels, ref isasync); //IPOSService.GetPOSConversionAsync(string.Join(",", pacids), sdate, edate);
			//if (!isasync)
			//{
			//	conv = await Tconv;
			//	TconvLY = GetConversion(sdate.AddYears(-1), edate.AddYears(-1), pacids, lsEnableChannels, ref isasync);
			//}
			//else
			//{
			//	TconvLY = GetConversion(sdate.AddYears(-1), edate.AddYears(-1), pacids, lsEnableChannels, ref isasync);
			//	conv = await Tconv;
			//}
			//IEnumerable<Proc_DashBoard_Conversion_Result> convLY = await TconvLY;

			//var site_data = site_pac.Join(conv, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate ?? DateTime.MinValue, countTrans = cv.CountTrans ?? 0, trafficIn = cv.TrafficIn ?? 0 });
			//var site_dataLY = site_pac.Join(convLY, si => si.pacid, cv => cv.PACID, (si, cv) => new { siteKey = si.sitekey, pacid = si.pacid, dvrdate = cv.DVRDate ?? DateTime.MinValue, countTrans = cv.CountTrans ?? 0, trafficIn = cv.TrafficIn ?? 0 });

			//var site_dataSum = site_data.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(gr => new { siteKey = gr.Key.siteKey, DVRDate = gr.Key.dvrdate, totalTrans = gr.Any() ? gr.Sum(x => x.countTrans) : 0, totalTraffic = gr.Any() ? gr.Sum(x => x.trafficIn) : 0 });
			//var site_dataSumLY = site_dataLY.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(gr => new { siteKey = gr.Key.siteKey, DVRDate = gr.Key.dvrdate, totalTrans = gr.Any() ? gr.Sum(x => x.countTrans) : 0, totalTraffic = gr.Any() ? gr.Sum(x => x.trafficIn) : 0 });

			//var siteday_conv = site_dataSum.Select(si => new { siteKey = si.siteKey, DVRDate = si.DVRDate, Conv = (si.totalTraffic == 0 || si.totalTrans == 0) ? 0 : ((decimal)si.totalTrans * 100) / Math.Max(1, si.totalTraffic) });
			//var siteday_convLY = site_dataSumLY.Select(si => new { siteKey = si.siteKey, DVRDate = si.DVRDate, Conv = (si.totalTraffic == 0 || si.totalTrans == 0) ? 0 : ((decimal)si.totalTrans * 100) / Math.Max(1, si.totalTraffic) });

			//var siteConvAll = siteday_conv.GroupBy(x => x.siteKey).Select(cv => new { siteKey = cv.Key, Conv = !cv.Any() ? -1 : cv.Where(x => x.Conv > 0).Any() ? cv.Where(x => x.Conv > 0).Average(c => c.Conv > 150 ? 0 : c.Conv) : 0 }).Distinct();
			//var siteConvAllLY = siteday_convLY.GroupBy(x => x.siteKey).Select(cv => new { siteKey = cv.Key, Conv = !cv.Any() ? -1 : cv.Where(x => x.Conv > 0).Any() ? cv.Where(x => x.Conv > 0).Average(c => c.Conv > 150 ? 0 : c.Conv) : 0 }).Distinct();

			//var siteConv = siteConvAll.Where(x => x.Conv >= 0).Select(it => new { siteKey = it.siteKey, Conv = it.Conv });
			//var siteConvLY = siteConvAllLY.Where(x => x.Conv >= 0).Select(it => new { siteKey = it.siteKey, Conv = it.Conv });

			var siteConv = await GetSiteConversion(site_pac, pacids, sdate, edate, lsEnableChannels);
			var siteConvLY = await GetSiteConversion(site_pac, pacids, sdate.AddYears(-1), edate.AddYears(-1), lsEnableChannels);

			Task<List<states>> Tsates = ICommonInfoService.GetStateses<states>(stateids, st => st).ToListAsync();
			List<states> lstate = await Tsates;

			var site_state_name = site_sate.Join(lstate, si => si.state.state, st => st.id, (si, st) => new { siteKey = si.site.SiteKey, stateid = st.id, state = st.name, code = st.Code } );
			var site_count = site_state_name.GroupBy(x => x.stateid).Select(gr => new { stateid = gr.Key, state = gr.Any() ? gr.FirstOrDefault().state : string.Empty, code = gr.Any() ? gr.FirstOrDefault().code : string.Empty, count = gr.Any() ? gr.Select(x=>x.siteKey).Distinct().Count() : 0 });

			var conv_state = siteConv.Join(site_sate, cv => cv.SiteKey, st => st.site.SiteKey, (cv, st) => new { conv = cv, stateid = st.state.state }).Distinct();
			var ret = site_count.GroupJoin(conv_state, st => st.stateid, cv => cv.stateid, (st, cv) => new { stateid = st.stateid, state = st.state, code = st.code, conv = (!cv.Any() || st.count == 0) ? 0 : (cv.Sum(x=>x.conv.Conv)/st.count) });

			var conv_stateLY = siteConvLY.Join(site_sate, cv => cv.SiteKey, st => st.site.SiteKey, (cv, st) => new { conv = cv, stateid = st.state.state }).Distinct();
			var retLY = site_count.GroupJoin(conv_stateLY, st => st.stateid, cv => cv.stateid, (st, cv) => new { stateid = st.stateid, state = st.state, code = st.code, conv = (!cv.Any() || st.count == 0) ? 0 : (cv.Sum(x => x.conv.Conv) / st.count) });

			/*
			//var conv_state = conv.Join(site_sate, cv => cv.PACID, st => st.site.pacid, (cv, st) => new { conv = cv, stateid = st.state.state});//.Distinct();;
			//var ret = site_sate_name.GroupJoin(conv_state, st => st.stateid, cv => cv.stateid, (st, cv) => new { state = st.state, code = st.code, gconv = cv }).Distinct();
			var conv_state = conv.Join(site_sate, cv => cv.PACID, st => st.site.pacid, (cv, st) => new { stateid = st.state.state, sitekey = st.site.sitekey, dvrdate = cv.DVRDate, paccount = st.site.count, cval = (!cv.CountTrans.HasValue || !cv.TrafficIn.HasValue || cv.TrafficIn.Value == 0) ? 0 : ((decimal)cv.CountTrans.Value * 100) / Math.Max(1, (decimal)cv.TrafficIn.Value) });
			var siteday_conv = conv_state.GroupBy(it => new { it.stateid, it.sitekey, it.dvrdate }, it => it).Select(group => new
			{
				stateid = group.Key.stateid,
				sitekey = group.Key.sitekey,
				dvrdate = group.Key.dvrdate,
				cval = (group.Any() && group.FirstOrDefault() != null && group.FirstOrDefault().paccount > 0) ? (group.Sum(c => (c.cval > 150 ? 0 : c.cval)) / group.FirstOrDefault().paccount) : 0
			});

			//var ret = site_sate_name.GroupJoin(conv_state, st => st.stateid, cv => cv.stateid, (st, cv) => new { stateid = st.stateid, state = st.state, code = st.code, cval = cv.Any() ? cv.Average(x => x.cval) : 0 }).Distinct();
			var conv_site = states.GroupJoin(siteday_conv, si => si.siteKey, cv => cv.sitekey, (si, cv) => new { siteKey = si.siteKey, stateid = si.state, cval = cv.Any() ? cv.Average(c => (c.cval > 150 ? 0 : c.cval)) : 0 });//.Distinct();
			var ret = site_count.GroupJoin(conv_site, st => st.stateid, cv => cv.stateid, (st, cv) => new {  stateid = st.stateid, state = st.state, code = st.code, cval = (st.count == 0) ? 0 : (cv.Sum(x => x.cval) / st.count) }).Distinct();

			//get data for last year
			//var convLY_state = convLY.Join(site_sate, cv => cv.PACID, st => st.site.pacid, (cv, st) => new { conv = cv, stateid = st.state.state });
			//var retLY = site_sate_name.GroupJoin(convLY_state, st => st.stateid, cv => cv.stateid, (st, cv) => new { state = st.state, code = st.code, gconv = cv }).Distinct();
			var convLY_state = convLY.Join(site_sate, cv => cv.PACID, st => st.site.pacid, (cv, st) => new { stateid = st.state.state, sitekey = st.site.sitekey, dvrdate = cv.DVRDate, paccount = st.site.count, cval = (!cv.CountTrans.HasValue || !cv.TrafficIn.HasValue || cv.TrafficIn.Value == 0) ? 0 : ((decimal)cv.CountTrans.Value * 100) / Math.Max(1, (decimal)cv.TrafficIn.Value) }); //((cv.TotalTrans.HasValue ? (decimal)cv.TotalTrans.Value : 0) * 100) / Math.Max(1, cv.TotalTraffic.HasValue ? (decimal)cv.TotalTraffic.Value : 0)
			var siteday_convLY = convLY_state.GroupBy(it => new { it.stateid, it.sitekey, it.dvrdate }, it => it).Select(group => new
			{
				stateid = group.Key.stateid,
				sitekey = group.Key.sitekey,
				dvrdate = group.Key.dvrdate,
				cval = (group.Any() && group.FirstOrDefault() != null && group.FirstOrDefault().paccount > 0) ? (group.Sum(c => (c.cval > 150 ? 0 : c.cval)) / group.FirstOrDefault().paccount) : 0
			});

			//var retLY = site_sate_name.GroupJoin(convLY_state, st => st.stateid, cv => cv.stateid, (st, cv) => new { stateid = st.stateid, state = st.state, code = st.code, cval = cv.Any() ? cv.Average(x => x.cval) : 0 }).Distinct();
			var conv_siteLY = states.GroupJoin(siteday_convLY, si => si.siteKey, cv => cv.sitekey, (si, cv) => new { siteKey = si.siteKey, stateid = si.state, cval = cv.Any() ? cv.Average(c => (c.cval > 150 ? 0 : c.cval)) : 0 });//.Distinct();
			//var site_countLY = site_sate_name.GroupJoin(states, si => si.stateid, st => st.state, (si, st) => new { stateid = si.stateid, state = si.state, code = si.code, count = st.Select(x => x.siteKey).Distinct().Count() });
			var retLY = site_count.GroupJoin(conv_siteLY, st => st.stateid, cv => cv.stateid, (st, cv) => new { stateid = st.stateid, state = st.state, code = st.code, cval = (st.count == 0) ? 0 : (cv.Sum(x => x.cval) / st.count) }).Distinct();
			*/
			var retData = ret.Select(it => new { key = it.state, code = it.code, value = it.conv, stateid = it.stateid });
												// value = (it.gconv.Sum(itrans => itrans.conv.TotalTrans.HasValue?itrans.conv.TotalTrans.Value : 0) * 100) 
												//				/ Math.Max(1, it.gconv.Sum( itraff => itraff.conv.TotalTraffic.HasValue?(decimal)itraff.conv.TotalTraffic.Value : 0))});
			var retLastYear = retLY.Select(it => new { key = it.state, value = it.conv, stateid = it.stateid });
													//   value = (it.gconv.Sum(itrans => itrans.conv.TotalTrans.HasValue?itrans.conv.TotalTrans.Value : 0) * 100) 
													//			/ Math.Max(1, it.gconv.Sum( itraff => itraff.conv.TotalTraffic.HasValue?(decimal)itraff.conv.TotalTraffic.Value : 0))});


			return (from cy in retData
					join ly in retLastYear on cy.stateid equals ly.stateid into dat
					from it in dat.DefaultIfEmpty()
					select new ConvMapChartModel
						 {
							 Label = cy.key,
							 StateID = cy.stateid,
							 Code = cy.code,
							 Value = cy.value,
							 LastYear = (it == null) ? 0 : it.value
						 }).Distinct();
			/*
			//var result = ret.Join(retLY, cy => cy.state, ly => ly.state, (cy, ly) => new { state = cy.state, code = cy.code, gconv = cy.gconv, gconvLY = ly.gconv });
			var result = (from cy in ret
						 join ly in retLY on cy.state equals ly.state into dat
						 from it in dat.DefaultIfEmpty()
						 select new
						 {
							 state = cy.state,
							 code = cy.code,
							 gconv = cy.gconv,
							 gconvLY = it.gconv
						 }).Distinct();
			return result.Select(item => new ConvMapChartModel { Code = item.code,
															Label = item.state, 
															Value = (item.gconv.Sum(itrans => itrans.conv.TotalTrans.HasValue?itrans.conv.TotalTrans.Value : 0) * 100) 
																/ Math.Max(1, item.gconv.Sum( itraff => itraff.conv.TotalTraffic.HasValue?(decimal)itraff.conv.TotalTraffic.Value : 0)),
															LastYear = item.gconvLY == null ? 0 : (item.gconvLY.Sum(itrans => itrans.conv.TotalTrans.HasValue ? itrans.conv.TotalTrans.Value : 0) * 100) / Math.Max(1, item.gconvLY.Sum(itraff => itraff.conv.TotalTraffic.HasValue ? (decimal)itraff.conv.TotalTraffic.Value : 0))
													}).Distinct();
			*/
		 }
		#endregion

		Task<List<Func_Count_Exception_Trans_Result>> CountPOSExceptionTrans(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<int> exceptions)
		{
			Task<List<Func_Count_Exception_Trans_Result>> TexTrans = IPOSService.Count_Exception_Transaction_async(sdate, edate, pacids, exceptions);

			return TexTrans;
		}
		
		public async Task<List<Proc_DashBoard_Conversion_Result>> GetConversionAsync(DateTime sdate, DateTime edate, IEnumerable<int> pacids)
		{
			bool isasync = false;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);
			return await GetConversion(sdate, edate, pacids, lsEnableChannels, ref isasync);
		}

		public Task<List<Proc_DashBoard_Conversion_Result>> GetConversionAsync(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync)
		{
			return GetConversion(sdate, edate, pacids, lsEnableChannels, ref isasync);
		}

		protected Task<List<Proc_DashBoard_Conversion_Result>> GetConversion(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync)
		{
			ICache<POSPeriodicCacheModel> POS_cache = ResolveCache<POSPeriodicCacheModel>(sdate, edate);
			ICache<IOPCCountPeriodicCacheModel> IOPCCount_cache = ResolveCache<IOPCCountPeriodicCacheModel>(sdate, edate);
			if(POS_cache != null && IOPCCount_cache != null)
			{
				isasync = true;
				int stime = sdate.ToUnixTimestamp(sdate.Hour);
				int etime = edate.ToUnixTimestamp(edate.Hour);
				Task<List<Proc_DashBoard_Conversion_Result>> tResult = Task.Run( ()=>
				{
					if (lsEnableChannels.Any())
					{
						IEnumerable<POSPeriodicCacheModel> pos = POS_cache.Query<POSPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, it => it);
						IEnumerable<IOPCCountPeriodicCacheModel> iopc = IOPCCount_cache.Query<IOPCCountPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, io => io);
						IEnumerable<POSPeriodicCacheModel> pos_dvr = pos.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it).ToList();
						IEnumerable<IOPCCountPeriodicCacheModel> iopc_dvr = iopc.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it).ToList();
						IEnumerable<IOPCCountPeriodicCacheModel> iopc_enchan = iopc_dvr.Join(lsEnableChannels, it => new { it.PACID, it.CameraID }, pid => new { pid.PACID, pid.CameraID }, (it, pid) => it).ToList();

						var fjoin = pos_dvr.FullOuterGroupJoin(iopc_enchan, itpos => new { itpos.PACID, itpos.DVRDate }, itiopc => new { itiopc.PACID, itiopc.DVRDate }, (itpos, itiopc, itkey) =>
							new Proc_DashBoard_Conversion_Result
							{
								DVRDate = ((Int64)itkey.DVRDate).unixTime_ToDateTime(),
								PACID = itkey.PACID,
								TotalAmount = !itpos.Any() ? 0 : itpos.Sum(it => !it.ReportNormalize ? it.TotalAmount : it.NTotalAmount),
								CountTrans = !itpos.Any() ? 0 : itpos.Sum(it => !it.ReportNormalize ? it.TotalTrans : it.NTotalTrans),
								TrafficIn = !itiopc.Any() ? 0 : itiopc.Sum(it => !it.ReportNormalize ? it.In : it.InN),
								TrafficOut = !itiopc.Any() ? 0 : itiopc.Sum(it => !it.ReportNormalize ? it.Out : it.OutN)
							}
						 );
						return fjoin.ToList();
					}
					else
					{
						return new List<Proc_DashBoard_Conversion_Result>();
					}
				});
				return tResult;
				
			}
			else
			{
				isasync = false;
				string strPacid = string.Join<int>(",", pacids);
				Task<List<Proc_DashBoard_Conversion_Result>> result = IPOSService.GetPOSConversion_Async(strPacid, sdate, edate);
				return result;
			}
			//var group = result.GroupBy(item => item.DVRDateKey);
			//POSConversionModel model = group.Select(item => new POSConversionModel
			//{
			//	DVRDateKey = item.Key.HasValue ? item.Key.Value : sdate
			//	,
			//	PACID = item.Count(pac => pac.PACID.HasValue)
			//	,
			//	TotalSales = item.Sum(sale => { return sale.TotalSales.HasValue ? sale.TotalSales.Value : 0; })
			//	,
			//	TotalTraffic = item.Sum(tf => { return tf.TotalTraffic.HasValue ? tf.TotalTraffic.Value : 0; })
			//	,
			//	TotalTrans = item.Sum(tt => { return tt.TotalTrans.HasValue ? tt.TotalTrans.Value : 0; })
			//}).FirstOrDefault();
			//return Task.FromResult<POSConversionModel>(model);


		}

		public async Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetConversionHourlyAsync(DateTime sdate, DateTime edate, string pacids)
		{
			bool isasync = false;
			List<int> lsPacid = pacids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(lsPacid, ref isasync);
			return await GetConversionHourly(sdate, edate, pacids, lsEnableChannels, ref isasync);
		}

		protected Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetConversionHourly(DateTime sdate, DateTime edate, string pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync)
		{
			//ICache<POSPeriodicCacheModel> POS_cache = ResolveCache<POSPeriodicCacheModel>(sdate, edate);
			//ICache<IOPCCountPeriodicCacheModel> IOPCCount_cache = ResolveCache<IOPCCountPeriodicCacheModel>(sdate, edate);
			//if(POS_cache != null && IOPCCount_cache != null)
			//{
			//	isasync = true;
			//	int stime = sdate.ToUnixTimestamp(sdate.Hour);
			//	int etime = edate.ToUnixTimestamp(edate.Hour);
			//	Task<List<Proc_DashBoard_Conversion_Hourly_Result>> tResult = Task.Run(() =>
			//	{
			//		if (lsEnableChannels.Any())
			//		{
			//			IEnumerable<POSPeriodicCacheModel> pos = POS_cache.Query<POSPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, it => it);
			//			IEnumerable<IOPCCountPeriodicCacheModel> iopc = IOPCCount_cache.Query<IOPCCountPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, io => io);
			//			IEnumerable<POSPeriodicCacheModel> pos_dvr = pos.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it).ToList();
			//			IEnumerable<IOPCCountPeriodicCacheModel> iopc_dvr = iopc.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it).ToList();
			//			IEnumerable<IOPCCountPeriodicCacheModel> iopc_enchan = iopc_dvr.Join(lsEnableChannels, it => new { it.PACID, it.CameraID }, pid => new { pid.PACID, pid.CameraID }, (it, pid) => it).ToList();

			//			var fjoin = pos_dvr.FullOuterGroupJoin(iopc_enchan, itpos => new { itpos.PACID, itpos.DVRDateHour }, itiopc => new { itiopc.PACID, itiopc.DVRDateHour }, (itpos, itiopc, itkey) =>
			//				new Proc_DashBoard_Conversion_Hourly_Result
			//				{
			//					DVRDateKey = !itpos.Any() ? DateTime.MinValue : ((Int64)itpos.FirstOrDefault().DVRDate).unixTime_ToDateTime(),
			//					tHour = itkey.DVRDateHour,
			//					PACID = itkey.PACID,
			//					TotalAmount = !itpos.Any() ? 0 : itpos.Sum(it => !it.ReportNormalize ? it.TotalAmount : it.NTotalAmount),
			//					CountTrans = !itpos.Any() ? 0 : itpos.Sum(it => !it.ReportNormalize ? it.TotalTrans : it.NTotalTrans),
			//					TrafficIn = !itiopc.Any() ? 0 : itiopc.Sum(it => !it.ReportNormalize ? it.In : it.InN)
			//				}
			//			 );
			//			return fjoin.ToList();
			//		}
			//		else
			//		{
			//			return new List<Proc_DashBoard_Conversion_Hourly_Result>();
			//		}
			//	});
			//	return tResult;
				
			//}
			//else
			{
				isasync = false;
				//string strPacid = string.Join<int>(",", pacids);
				Task<List<Proc_DashBoard_Conversion_Hourly_Result>> result = IPOSService.GetPOSConversionHourly_Async(pacids, sdate, edate);
				return result;
			}
		}

		private Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> GetPOSTransaction(DateTime sdate, DateTime edate, IEnumerable<int> pacids, ref bool isasync)
		{
			ICache<POSPeriodicCacheModel> POS_cache = ResolveCache<POSPeriodicCacheModel>(sdate, edate);
			if (POS_cache != null)
			{
				isasync = true;
				int stime = sdate.ToUnixTimestamp(sdate.Hour);
				int etime = edate.ToUnixTimestamp(edate.Hour);
				Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> tResult = Task.Run(() =>
				{
					IEnumerable<POSPeriodicCacheModel> pos = POS_cache.Query<POSPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, it => it);
					IEnumerable<POSPeriodicCacheModel> pos_dvr = pos.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it).ToList();

					var fjoin = pos_dvr.GroupBy(itpos => new { itpos.PACID, itpos.DVRDate })
						.Select(it => new
							Func_Fact_POS_Periodic_Daily_Transact_Result
							{
								DVRDateKey = ((Int64)it.Key.DVRDate).unixTime_ToDateTime(),
								PACID = it.Key.PACID,
								Count_Trans = !it.Any() ? 0 : it.Sum(x => !x.ReportNormalize ? x.TotalTrans : x.NTotalTrans),
								TotalAmount = !it.Any() ? 0 : it.Sum(x => !x.ReportNormalize ? x.TotalAmount : x.NTotalAmount)
							}
						);
					return fjoin.ToList();
				});
				return tResult;

			}
			else
			{
				isasync = false;
				string strPacid = string.Join<int>(",", pacids);
				Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> result = IPOSService.GetPOSTransaction_Async(strPacid, sdate, edate);
				return result;
			}
		}

		/// <summary>
		/// Async method is using IGoalTypeService.
		/// </summary>
		/// <param name="sites"></param>
		/// <returns></returns>
		private Task<List<tCMSWeb_GoalType_Map>> GetGoalMapsbyGoalID(Utils.GoalType goalType)
		{
			IQueryable<tCMSWeb_GoalType_Map> Gmaps = IGoalTypeService.GetGoalMaps<tCMSWeb_GoalType_Map>(item => item, null).Where(item => item.GoalTypeID == (int)goalType);
			return Gmaps.ToListAsync();
		}

		/// <summary>
		/// Async method is using ISiteService.
		/// </summary>
		/// <param name="sites"></param>
		/// <returns></returns>
		private Task<List<int>> GetGoalBySites(IEnumerable<int> sites)
		{
			var result = ISiteService.GetSites<int?>(sites, site => site.GoalID, null);
			return result.Where(it => it.HasValue && it.Value > 0).Select(sel => sel.Value).ToListAsync();
		}

		public Task<List<Func_BAM_LaborHourlyWorkingHour_Result>> GetLaborHourlyWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> pacids)
		{
			return IPOSService.Func_BAM_LaborHourlyWorkingHour(sdate, edate, pacids);
		}

		public Task<List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>> GetLaborHourlyMinSecsWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> pacids)
		{
			return IPOSService.Func_BAM_LaborHourlyMinSecsWorkingHour(sdate, edate, pacids);
		}

		Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> DashBoard_Channel_EnableTrafficCount(IEnumerable<int> pacids, ref bool isasync)
		{
			var iopcSvr = IIOPCService;
			if (iopcSvr == null)
			{
				iopcSvr = (IIOPCService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IIOPCService));
			}
			return iopcSvr.Proc_DashBoard_Channel_EnableTrafficCount_Async(pacids);
		}

		private async Task<IEnumerable<SiteConversion>> GetSiteConversion(IEnumerable<SitePACID> site_pac, IEnumerable<int> allPacids, DateTime sdate, DateTime edate, List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels)
		{
			IEnumerable<int> lsPacids = null;
			if (allPacids == null)
			{
				lsPacids = site_pac.Select(si => si.PACID).Distinct().ToList();
			}
			else
			{
				lsPacids = allPacids;
			}
			IEnumerable<Proc_DashBoard_Conversion_Result> conv = null;
			bool isasync = false;
			Task<List<Proc_DashBoard_Conversion_Result>> Tconv = GetConversion(sdate, edate, lsPacids, lsEnableChannels, ref isasync);
			conv = await Tconv;

			var site_data = site_pac.Join(conv, si => si.PACID, cv => cv.PACID, (si, cv) => new { siteKey = si.SiteKey, pacid = si.PACID, dvrdate = cv.DVRDate ?? DateTime.MinValue, countTrans = cv.CountTrans ?? 0, trafficIn = cv.TrafficIn ?? 0 });
			var site_dataSum = site_data.GroupBy(x => new { x.siteKey, x.dvrdate }).Select(gr => new { siteKey = gr.Key.siteKey, DVRDate = gr.Key.dvrdate, totalTrans = gr.Any() ? gr.Sum(x => x.countTrans) : 0, totalTraffic = gr.Any() ? gr.Sum(x => x.trafficIn) : 0 });
			var siteday_conv = site_dataSum.Select(si => new { siteKey = si.siteKey, DVRDate = si.DVRDate, Conv = (si.totalTraffic == 0 || si.totalTrans == 0) ? 0 : ((decimal)si.totalTrans * 100) / Math.Max(1, si.totalTraffic) });
			var siteConvAll = siteday_conv.GroupBy(x => x.siteKey).Select(cv => new { siteKey = cv.Key, Conv = !cv.Any() ? -1 : cv.Where(x => x.Conv > 0 && x.Conv <= 150).Any() ? cv.Where(x => x.Conv > 0 && x.Conv <= 150).Average(c => c.Conv) : 0 }).Distinct();
			IEnumerable<SiteConversion> siteConv = siteConvAll.Where(x => x.Conv >= 0).Select(it => new SiteConversion { SiteKey = it.siteKey, Conv = it.Conv });

			return siteConv;
		}
	}
}
