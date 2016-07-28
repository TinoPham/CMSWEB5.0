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
using CMSWebApi.BusinessServices.ReportBusiness.IOPC;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportService
{

    public class TableReportConversionScenario
    {
        public string PACID_Name { get; set; }
        public String DVRDate { get; set; }
        public int CountIn { get; set; }
        public int CountOut { get; set; }
        public int CountTrans { get; set; }
        public double ConvRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int PACID { get; set; }
        public string DayOfWeek { get; set; }
    }

    public class ConversionScenario : ReportBase, IReportService
	{

        public ConversionScenario(UserContext userContext, IResposity dbModel)
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
            //var ssrp = (ISaleReportsService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISaleReportsService));
            var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));

            var pos = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
			var iopc = InitBusiness<IOPCBusinessService, IUsersService>() as IOPCBusinessService;

            int sitekey = sites == null ? 0 : int.Parse(sites);

            IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.siteKey.Value == sitekey && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0 && it.siteKey.Value == sitekey).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
            IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
			bool refasync = false;
            

            //string pacIds = string.Join(",", site_pac.Select(si => si.PACID).Distinct().ToList());

            //var conv = ssrp.GetPOSConversionHourly(pacIds, new DateTime(2014, 8, 6), new DateTime(2014, 8, 6));

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);
           // tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(report.UserID, new DateTime(2014, 8, 6));
            List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, datenow, fyInfo.FYDateStart.Value);

            FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            int weekno = 0;

            foreach (FiscalPeriod period in fyPeriods)
            {
                if (period.EndDate < fyWeekInfo.StartDate)
                {
                    weekno += period.Weeks.Count;
                }
            }

            weekno += fyWeekInfo.WeekIndex;

			var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync); ;
			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
			{
				List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
				return msg;
			});
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

            var conv = pos.GetConversionAsync(fyWeekInfo.StartDate, EndTimeOfDate(fyWeekInfo.EndDate), pacIds, lsEnableChannels, ref refasync);

            Task<List<Proc_DashBoard_Conversion_Result>> sCode = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await conv;
                return msg;
            });


            List<Proc_DashBoard_Conversion_Result> Data = sCode.Result;

            if (Data == null || Data.Count == 0) {
                Data = new List<Proc_DashBoard_Conversion_Result>();
            }
            var conv_NC = Data.GroupBy(x => x.DVRDate).Select(group =>
                new
                {
                    DVRDate = group.Key,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                });

            var formatCulture = new CultureInfo(languageId.ToLower());
            var reesult = conv_NC.OrderBy(od => od.DVRDate).Select(t => new TableReportConversionScenario()
            { 
                PACID_Name = "",
                DVRDate = t.DVRDate.HasValue ? t.DVRDate.Value.ToString("ddd", formatCulture) : string.Empty,
                CountIn = t.TrafficIn,
                CountOut = t.TrafficOut,
                CountTrans = t.CountTrans,
                ConvRate = 0,
                TotalAmount = t.TotalAmount,
                PACID = 0,
                DayOfWeek = t.DVRDate.HasValue ? t.DVRDate.Value.ToString("MMM dd, yy", formatCulture) : string.Empty
            
            }).ToList();


            DataTable dt = ReportFactory.ToDataTable<TableReportConversionScenario>(reesult);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


            DataTable parmTable = GetParamsResources(report, requestParms);

            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
            parmTable.Rows.Add("SiteName", siteName);
            //parmTable.Rows.Add("SiteName", "Tha'nh Luan");
            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
            parmTable.Rows.Add("imgLogopath", imgLogopath);

            parmTable.Rows.Add("header_week", weekno.ToString());
            DateTime year = fyWeekInfo.StartDate != null ? fyWeekInfo.StartDate : new DateTime();
            parmTable.Rows.Add("YearNumber", "(" +  year.Year.ToString() + ")");

            // Create a DataSet and put both tables in it.
            DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
            set.Tables.Add(dt);
            set.Tables.Add(parmTable);
            return set;
        }
	}
}
