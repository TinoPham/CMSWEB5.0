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

    public class TableMetricByHour {
        public string PACID_Name { get; set; }
        public DateTime? DVRDate { get; set; }
        public int DVRHour { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public int ConvRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalLaborHours { get; set; }
        public int TotalLaborHoursDaily { get; set; }
        public string DayOfWeek { get; set; }
    }
   
    public class MetricByHour : ReportBase, IReportService
	{

        public MetricByHour(UserContext userContext, IResposity dbModel)
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
            var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));
            var iopc = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
            //var base = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);
            FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            int sitekey = sites == null ? 0 : int.Parse(sites);
            IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.siteKey.Value == sitekey && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
            


			bool refasync = false;
         
            string pacIds = string.Join(",", site_pac.Select(si => si.PACID).Distinct().ToList());
            var conv = ssrp.GetPOSConversionHourly(pacIds, StartTimeOfDate(fyWeekInfo.StartDate), EndTimeOfDate(fyWeekInfo.EndDate));


            Task<List<Proc_DashBoard_Conversion_Hourly_Result>> sCode = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Hourly_Result> msg = await conv;
                return msg;
            });

            List<Proc_DashBoard_Conversion_Hourly_Result> Data = sCode.Result;

            if (Data == null || Data.Count == 0)
            {
                Data = new List<Proc_DashBoard_Conversion_Hourly_Result>();
            }

            var formatCulture = new CultureInfo(languageId.ToLower());
            var reesult = Data.OrderBy(ob => ob.DVRDateKey).ThenBy(tb => tb.tHour).GroupBy(g => new { g.DVRDateKey, g.tHour }).Select(t => new TableMetricByHour()
            { 
                PACID_Name = "",
                DVRDate = t.Key.DVRDateKey,
                DVRHour = t.Key.tHour.Value,
                CountIn = t.Any() ? t.Sum(x => x.TrafficIn.Value) : 0, 
                CountTrans = t.Any() ? t.Sum(x => x.CountTrans.Value) : 0,
                ConvRate = 0,
                TotalAmount = t.Any() ? t.Sum(x => x.TotalAmount.Value) : 0,
                TotalLaborHours = 0,
                TotalLaborHoursDaily = 0,
                DayOfWeek = ((DateTime)t.Key.DVRDateKey).ToString("ddd, MMM dd", formatCulture)
            
            }).ToList();

            DataTable dt = ReportFactory.ToDataTable<TableMetricByHour>(reesult);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


			DataTable parmTable = GetParamsResources(report, requestParms);

            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
            parmTable.Rows.Add("SiteName", siteName);
			//parmTable.Rows.Add("SiteName", "Tha'nh Luan");

            parmTable.Rows.Add("rpt_Period", fyWeekInfo.StartDate.ToString("MMM dd, yyyy", formatCulture) + " - " + fyWeekInfo.EndDate.ToString("MMM dd, yyyy", formatCulture));
            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
			parmTable.Rows.Add("imgLogopath", imgLogopath);


            DataSet set = new DataSet("tblData");
            set.Tables.Add(dt);
			set.Tables.Add(parmTable);
            return set;
        }
	}
}
