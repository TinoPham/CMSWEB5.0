using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CMSWebApi.BusinessServices.ReportBusiness.Interfaces;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using Extensions;
using Extensions.Linq;
using CMSWebApi.BusinessServices.ReportBusiness.IOPC;

namespace CMSWebApi.BusinessServices.ReportService
{

    public class TableDataConvCmpr
    {
        public string PACID_Name { get; set; }
        public string RegionName { get; set; }
        public int YTD_ThisYear_CountIn { get; set; }
        public int YTD_ThisYear_CountTrans { get; set; }
        public int YTD_LastYear_CountIn { get; set; }
        public int YTD_LastYear_CountTrans { get; set; }
        public int MTD_ThisYear_CountIn { get; set; }
        public int MTD_ThisYear_CountTrans { get; set; }
        public int MTD_LastYear_CountIn { get; set; }
        public int MTD_LastYear_CountTrans { get; set; }
    }

    public class ConversionComparision : ReportBase, IReportService
	{

        public ConversionComparision(UserContext userContext, IResposity dbModel)
			: base(userContext, dbModel)
		{

		}

		public DataSet GetReportData()
		{
            throw new NotImplementedException();
		}

        public DataSet GetReportData(tbl_BAM_Metric_ReportUser report, NameValueCollection requestParms = null)
        {
            var sDate = requestParms["sdate"] ?? string.Empty;
            var eDate = requestParms["edate"] ?? string.Empty;
            //var sites = requestParms["sites"] ?? string.Empty;
            var languageId = requestParms["languageId"] ?? string.Empty;

            if (sDate == string.Empty || eDate == string.Empty || languageId == string.Empty)
            {
                throw new NotImplementedException();
            }

            var svc = (IUsersService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUsersService));
            var ssrp = (ISaleReportsService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISaleReportsService));
            var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));
            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));

            var pos = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
			var iopc = InitBusiness<IOPCBusinessService, IUsersService>() as IOPCBusinessService;

            //int sitekey = sites == null ? 0 : int.Parse(sites);

            IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
            IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
            IEnumerable<int> siteKeys = site_pac.Select(x => x.SiteKey).Distinct();
			bool refasync = false;
            
            DateTime thisdatenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime lastdatenow = thisdatenow.AddYears(-1);

            tCMSWeb_FiscalYear thisfyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, thisdatenow, true);
            List<FiscalPeriod> ListthisfyPeriod = IFiscalYear.GetFiscalPeriods(thisfyInfo, thisdatenow, thisfyInfo.FYDateStart.Value);
            FiscalPeriod thisfyPeriod = ListthisfyPeriod.FirstOrDefault(x => x.StartDate <= thisdatenow && x.EndDate >= thisdatenow);
           
            tCMSWeb_FiscalYear lastfyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, lastdatenow, true);
            List<FiscalPeriod> ListlastfyPeriod = IFiscalYear.GetFiscalPeriods(lastfyInfo, lastdatenow, lastfyInfo.FYDateStart.Value);
            FiscalPeriod lastfyPeriod = ListlastfyPeriod.FirstOrDefault(x => x.StartDate <= lastdatenow && x.EndDate >= lastdatenow);

#region THIS_MTD
			//List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);
            var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync);;
            Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
            {
                List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
                return msg;
            });
            List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

            var thisMTDconv = pos.GetConversionAsync(thisfyPeriod.StartDate, EndTimeOfDate(thisdatenow), pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeThisMTD = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await thisMTDconv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> DataThis_MTD = sCodeThisMTD.Result;

            if (DataThis_MTD == null || DataThis_MTD.Count == 0)
            {
                DataThis_MTD = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Calculator MTD Now each site.
            var dataThisMTD_siteKey = DataThis_MTD.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
                new
                {
                    siteKey = si.SiteKey,
                    pacid = si.PACID,
                    dvrdate = cv.DVRDate,
                    TrafficIn = cv.TrafficIn,
                    TrafficOut = cv.TrafficOut,
                    CountTrans = cv.CountTrans,
                    TotalAmount = cv.TotalAmount
                }).Distinct().ToList();

            var site_conv_This_MTD = dataThisMTD_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();

#endregion

#region THIS_YTD
			var thisYTDconv = pos.GetConversionAsync(thisfyInfo.FYDateStart.Value, EndTimeOfDate(thisdatenow), pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeThisYTD = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await thisYTDconv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> DataThis_YTD = sCodeThisYTD.Result;

            if (DataThis_YTD == null || DataThis_YTD.Count == 0)
            {
                DataThis_YTD = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Calculator YTD Now each site.
            var dataThisYTD_siteKey = DataThis_YTD.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
                new
                {
                    siteKey = si.SiteKey,
                    pacid = si.PACID,
                    dvrdate = cv.DVRDate,
                    TrafficIn = cv.TrafficIn,
                    TrafficOut = cv.TrafficOut,
                    CountTrans = cv.CountTrans,
                    TotalAmount = cv.TotalAmount
                }).Distinct().ToList();

            var site_conv_This_YTD = dataThisYTD_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();
#endregion

#region LAST_MTD
			var lastMTDconv = pos.GetConversionAsync(lastfyPeriod.StartDate, EndTimeOfDate(lastdatenow), pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeLastMTD = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await lastMTDconv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> DataLast_MTD = sCodeLastMTD.Result;

            if (DataLast_MTD == null || DataLast_MTD.Count == 0)
            {
                DataLast_MTD = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Calculator MTD Last each site.
            var dataLastMTD_siteKey = DataLast_MTD.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
                new
                {
                    siteKey = si.SiteKey,
                    pacid = si.PACID,
                    dvrdate = cv.DVRDate,
                    TrafficIn = cv.TrafficIn,
                    TrafficOut = cv.TrafficOut,
                    CountTrans = cv.CountTrans,
                    TotalAmount = cv.TotalAmount
                }).Distinct().ToList();

            var site_conv_Last_MTD = dataLastMTD_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();
#endregion
#region LAST_YTD
			var lastYTDconv = pos.GetConversionAsync(lastfyInfo.FYDateStart.Value, EndTimeOfDate(lastdatenow), pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeLastYTD = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await lastYTDconv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> DataLast_YTD = sCodeLastYTD.Result;

            if (DataLast_YTD == null || DataLast_YTD.Count == 0)
            {
                DataLast_YTD = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Calculator YTD Last each site.
            var dataLastYTD_siteKey = DataLast_YTD.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
                new
                {
                    siteKey = si.SiteKey,
                    pacid = si.PACID,
                    dvrdate = cv.DVRDate,
                    TrafficIn = cv.TrafficIn,
                    TrafficOut = cv.TrafficOut,
                    CountTrans = cv.CountTrans,
                    TotalAmount = cv.TotalAmount
                }).Distinct().ToList();

            var site_conv_Last_YTD = dataLastYTD_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();
#endregion

#region JOIN_THISMTD_THISYTD_LASTMTD_LASTYTD

            string[] includes = { typeof(tCMSWebRegion).Name };
            IQueryable<tCMSWebSites> dbSites = siteSvc.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
            IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

            var site_region = site_pac.Join(dbSites, si => si.SiteKey, rg => rg.siteKey, (si, rg) => new { site = si, region = rg });
            var site_region_name = site_region.Join(dbRegions, si => si.region.RegionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.site.PACID, siteKey = si.site.SiteKey, siteName = si.region.ServerID, regionKey = rg.RegionKey, regionName = rg.RegionName });
            

            var Data_This_MTD_YTD = site_conv_This_MTD.FullOuterJoin(site_conv_This_YTD, si => si.siteKey, s => s.siteKey, (si, s, k) =>
                new 
                {
                    siteKey = si == null ? s == null ? 0 : s.siteKey : si.siteKey,
                    ThisMTD_CountTrans = si == null || si.CountTrans == null ? 0 : si.CountTrans,
                    ThisMTD_TrafficIn = si == null || si.TrafficIn == null ? 0 : si.TrafficIn,
                    ThisYTD_CountTrans = s == null || s.CountTrans == null ? 0 : s.CountTrans,
                    ThisYTD_TrafficIn = s == null || s.TrafficIn == null ? 0 : s.TrafficIn
                }).Distinct().ToList();

            var Data_Last_MTD_YTD = site_conv_Last_MTD.FullOuterJoin(site_conv_Last_YTD, si => si.siteKey, s => s.siteKey, (si, s, k) =>
                new
                {
                    siteKey = si == null ? s == null ? 0 : s.siteKey : si.siteKey,
                    LastMTD_CountTrans = si == null || si.CountTrans == null ? 0 : si.CountTrans,
                    LastMTD_TrafficIn = si == null || si.TrafficIn == null ? 0 : si.TrafficIn,
                    LastYTD_CountTrans = s == null || s.CountTrans == null ? 0 : s.CountTrans,
                    LastYTD_TrafficIn = s == null || s.TrafficIn == null ? 0 : s.TrafficIn
                }).Distinct().ToList();

            var Data_THISMTD_THISYTD_LASTMTD_LASTYTD = Data_This_MTD_YTD.FullOuterJoin(Data_Last_MTD_YTD, si => si.siteKey, s => s.siteKey, (si, s, k) => new TableDataConvCmpr()
                {
                    PACID_Name = si == null ? s == null ? string.Empty : siteSvc.GetSite<string>(s.siteKey, t => t.ServerID, null) : siteSvc.GetSite<string>(si.siteKey, t => t.ServerID, null),
                    RegionName = si == null ? s == null ? string.Empty : site_region_name.Where(x => x.siteKey == s.siteKey).FirstOrDefault().regionName : site_region_name.Where(x => x.siteKey == si.siteKey).FirstOrDefault().regionName,
                    YTD_ThisYear_CountIn = si == null ? 0 : si.ThisYTD_TrafficIn,
                    YTD_ThisYear_CountTrans = si == null ? 0 : si.ThisYTD_CountTrans,
                    YTD_LastYear_CountIn = s == null ? 0 : s.LastYTD_TrafficIn,
                    YTD_LastYear_CountTrans = s == null ? 0 : s.LastYTD_CountTrans,
                    MTD_ThisYear_CountIn = si == null ? 0 : si.ThisMTD_TrafficIn,
                    MTD_ThisYear_CountTrans = si == null ? 0 : si.ThisMTD_CountTrans,
                    MTD_LastYear_CountIn = s == null ? 0 : s.LastMTD_TrafficIn,
                    MTD_LastYear_CountTrans = s == null ? 0 : s.LastMTD_CountTrans
                }).Distinct().ToList();
            
#endregion


            DataTable dt = ReportFactory.ToDataTable<TableDataConvCmpr>(Data_THISMTD_THISYTD_LASTMTD_LASTYTD);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;

            DataTable parmTable = GetParamsResources(report, requestParms);

            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
            parmTable.Rows.Add("imgLogopath", imgLogopath);

            var formatCulture = new CultureInfo(languageId.ToLower());
            parmTable.Rows.Add("rpt_Period", "MTD " + thisfyPeriod.StartDate.ToString("MMM dd, yyyy", formatCulture) + " - " + thisdatenow.ToString("MMM dd, yyyy", formatCulture));

            // Create a DataSet and put both tables in it.
            DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
            set.Tables.Add(dt);
            set.Tables.Add(parmTable);
            return set;
        }
	}
}
