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

    public class TablePerformanceComparison
    {
        public string PACID_Name { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public double ConvRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Compare_CountIn { get; set; }
        public int Compare_CountTrans { get; set; }
        public double Compare_ConvRate { get; set; }
        public decimal Compare_TotalAmount { get; set; }
        public int SiteKey { get; set; }
        public string SiteName { get; set; }
        public int PACID { get; set; }
    }

    public class PerformanceComparisonByPeriod : ReportBase, IReportService
	{

        public PerformanceComparisonByPeriod(UserContext userContext, IResposity dbModel)
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
			bool refasync = false;
            

            //string pacIds = string.Join(",", site_pac.Select(si => si.PACID).Distinct().ToList());

            //var conv = ssrp.GetPOSConversionHourly(pacIds, new DateTime(2014, 8, 6), new DateTime(2014, 8, 6));

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            

            tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);
            FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            // Compare Date.
            DateTime datecompare = DateTime.ParseExact(eDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            tCMSWeb_FiscalYear fyInfoCompare = IFiscalYear.GetFiscalYearInfo(base.UserContext, datecompare);
            FiscalWeek fyWeekInfoCompare = IFiscalYear.GetFiscalWeek(fyInfoCompare, datecompare, fyInfoCompare.FYDateStart.Value);

			var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync); ;
			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
			{
				List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
				return msg;
			});
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

            // Period Now
            var conv = pos.GetConversionAsync(fyWeekInfo.StartDate, EndTimeOfDate(fyWeekInfo.EndDate), pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeNow = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await conv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> Data_Now = sCodeNow.Result;

            if (Data_Now == null || Data_Now.Count == 0)
            {
                Data_Now = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Period Comapre
            var convCompare = pos.GetConversionAsync(fyWeekInfoCompare.StartDate, EndTimeOfDate(fyWeekInfoCompare.EndDate), pacIds, lsEnableChannels, ref refasync);

            Task<List<Proc_DashBoard_Conversion_Result>> sCodeCompare = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await convCompare;
                return msg;
            });

            List<Proc_DashBoard_Conversion_Result> Data_Compare = sCodeCompare.Result;

            if (Data_Compare == null || Data_Compare.Count == 0)
            {
                Data_Compare = new List<Proc_DashBoard_Conversion_Result>();
            }

            // Calculator Now each site.
            var dataNow_siteKey = Data_Now.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
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

            var site_conv_Now = dataNow_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();

            // Calculator Compare each site.
            var dataCompare_siteKey = Data_Compare.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
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

            var site_conv_Compare = dataCompare_siteKey.GroupBy(x => new { x.siteKey}).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();

            // Join Now and Compare into DataSet
            var reesult = site_conv_Now.FullOuterJoin(site_conv_Compare, cv => cv.siteKey, si => si.siteKey, (cv, si, k) => new TablePerformanceComparison()
            {
                PACID_Name = "",
                CountIn = cv == null || cv.TrafficIn == null ? 0 : cv.TrafficIn,
                CountTrans = cv == null || cv.CountTrans == null ? 0 : cv.CountTrans,
                ConvRate = 0,
                TotalAmount = cv == null || cv.TotalAmount == null ? 0 : cv.TotalAmount,
                Compare_CountIn = si == null || si.TrafficIn == null ? 0 : si.TrafficIn,
                Compare_CountTrans = si == null || si.CountTrans == null ? 0 : si.CountTrans,
                Compare_ConvRate = 0,
                Compare_TotalAmount = si == null || si.TotalAmount == null ? 0 : si.TotalAmount,
                SiteKey = cv == null ? si == null ? 0 : si.siteKey : cv.siteKey,
                SiteName = cv == null ? si == null ? string.Empty : siteSvc.GetSite<string>(si.siteKey, t => t.ServerID, null) : siteSvc.GetSite<string>(cv.siteKey, t => t.ServerID, null),
                PACID = 0
            }).OrderBy(x => x.SiteName).ToList();


            DataTable dt = ReportFactory.ToDataTable<TablePerformanceComparison>(reesult);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


            DataTable parmTable = GetParamsResources(report, requestParms);

            
            //var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
            //parmTable.Rows.Add("SiteName", siteName);
            //parmTable.Rows.Add("SiteName", "Tha'nh Luan");
            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
            parmTable.Rows.Add("imgLogopath", imgLogopath);

            var formatCulture = new CultureInfo(languageId.ToLower());
            parmTable.Rows.Add("header_ReportPeriod", fyWeekInfo.StartDate.ToString("MMM dd, yyyy", formatCulture));
            parmTable.Rows.Add("header_ComparePeriod", fyWeekInfoCompare.StartDate.ToString("MMM dd, yyyy", formatCulture));

            // Create a DataSet and put both tables in it.
            DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
            set.Tables.Add(dt);
            set.Tables.Add(parmTable);
            return set;
        }
	}
}
