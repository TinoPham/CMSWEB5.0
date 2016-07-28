using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
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

namespace CMSWebApi.BusinessServices.ReportService
{

    public class DataOpportunity
    {
        public int TimeString { get; set; }
        public int Day1Count { get; set; }
        public int Day2Count { get; set; }
        public int Day3Count { get; set; }
        public int Day4Count { get; set; }
        public int Day5Count { get; set; }
        public int Day6Count { get; set; }
        public int Day7Count { get; set; }
        public int TotalCount { get; set; }
    }

    public class OpportunityPieChartItem
    { 
        public DateTime DvrDate {get; set;}
        public int CountTraffic { get; set; }
        public string NameDayOfWeek { get; set; }
    }

    public class OpportunityReport : ReportBase, IReportService
	{

        public OpportunityReport(UserContext userContext, IResposity dbModel)
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
            var sites = requestParms["sites"] ?? string.Empty;
            var languageId = requestParms["languageId"] ?? string.Empty;

            if (sDate == string.Empty || eDate == string.Empty || sites == string.Empty || languageId == string.Empty)
            {
                throw new NotImplementedException();
            }

            var svc = (IUsersService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUsersService));
            var ssrp = (ISaleReportsService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISaleReportsService));
            var iopc = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
            var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));
            var SiteService = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var formatCulture = new CultureInfo(languageId.ToLower());

            int sitekey = sites == null ? 0 : int.Parse(sites);
            IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.siteKey.Value == sitekey && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);

            List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, datenow, fyInfo.FYDateStart.Value);
            
            FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            int weekno = 0;

            foreach (FiscalPeriod period in fyPeriods)
            {
                if (period.EndDate < fyWeekInfo.StartDate) {
                    weekno += period.Weeks.Count;
                }
            }

            weekno += fyWeekInfo.WeekIndex;

            var site = SiteService.GetSites(report.UserID, sitekey, null);

			bool refasync = false;
         
            string pacIds = string.Join(",", site_pac.Select(si => si.PACID).Distinct().ToList());
            var conv = ssrp.GetPOSConversionHourly(pacIds, StartTimeOfDate(fyWeekInfo.StartDate), EndTimeOfDate(fyWeekInfo.EndDate));


            Task<List<Proc_DashBoard_Conversion_Hourly_Result>> sCode = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Hourly_Result> msg = await conv;
                return msg;
            });

            List<Proc_DashBoard_Conversion_Hourly_Result> data = sCode.Result;

            var resultDate = data.GroupBy(t => t.DVRDateKey).Select(
                group => new OpportunityPieChartItem()
                {
                    DvrDate = group.Key.Value,
                    CountTraffic = group.Sum(t => t.TrafficIn.HasValue ? t.TrafficIn.Value : 0),
                    NameDayOfWeek = group.Key.Value.ToString("ddd", formatCulture)
                }).ToList();

            var resultTime = data.GroupBy(t => t.tHour).Select(
                group => new
                {
                    Hour = group.Key,
                    SumTrafficTime = group.Sum(t => t.TrafficIn.HasValue ? t.TrafficIn.Value : 0)
                });

            var resultDateTime = data.GroupBy(t => new { t.DVRDateKey, t.tHour }).Select(
                group => new
                {
                    Hour = group.Key.tHour,
                    DvrDate = group.Key.DVRDateKey,
                    Traffic = group.Sum(t => t.TrafficIn.HasValue ? t.TrafficIn.Value : 0)
                }).ToList();

            var reesult = resultTime.Where(x => x.SumTrafficTime > 0 ).OrderBy(ob => ob.Hour).Select(t => new DataOpportunity()
            {
                TimeString = t.Hour.HasValue ? t.Hour.Value : 0,
                Day1Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day2Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(1)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day3Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(2)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day4Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(3)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day5Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(4)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day6Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(5)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                Day7Count = resultDateTime.Where(w => DateTime.Compare(w.DvrDate.Value, fyWeekInfo.StartDate.AddDays(6)) == 0 && w.Hour == t.Hour).Select(s => s.Traffic).FirstOrDefault(),
                TotalCount = t.SumTrafficTime
            
            }).ToList();

            

            DataTable dt = ReportFactory.ToDataTable<DataOpportunity>(reesult);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;

            DataTable chartdt = ReportFactory.ToDataTable<OpportunityPieChartItem>(resultDate.Where(x => x.CountTraffic > 0).ToList());
            chartdt.TableName = "OpportunityPieChartItem";


			DataTable parmTable = GetParamsResources(report, requestParms);

            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
            parmTable.Rows.Add("SiteName", siteName);
			//parmTable.Rows.Add("SiteName", "Tha'nh Luan");

            parmTable.Rows.Add("WeekTitle", fyWeekInfo.StartDate.ToString("MMM dd, yyyy", formatCulture) + " - " + fyWeekInfo.EndDate.ToString("MMM dd, yyyy", formatCulture));
            parmTable.Rows.Add("weekno", weekno.ToString());

            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
			parmTable.Rows.Add("imgLogopath", imgLogopath);

            for (int i = 0; i < 7; i++) {
                parmTable.Rows.Add("Day" + (i + 1).ToString() + "Name", fyWeekInfo.StartDate.AddDays(i).ToString("ddd dd MMM", formatCulture));
            }

            DataSet set = new DataSet("DataOpportunity");
            set.Tables.Add(dt);
			set.Tables.Add(parmTable);
            set.Tables.Add(chartdt);
            return set;
        }
	}
}
