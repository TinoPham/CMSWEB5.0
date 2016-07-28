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
using CMSWebApi.BusinessServices.ReportBusiness.IOPC;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportService
{
    //public static class Languageen
    //{
    //    const int 
    //}

    public class DataTrafficInsights
    {
        public int Hour { get; set; }
        public int TrafficIn { get; set; }
        public int Trans { get; set; }
        public int TrueTraffic { get; set; }
        public int LostOpportunities { get; set; }
    }

    public class SumTbl
    { 
        public string People {get; set;}
        public int Count { get; set; }
    }

    public class TrafficInsights : ReportBase, IReportService
	{

        public TrafficInsights(UserContext userContext, IResposity dbModel)
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
            var iopc = InitBusiness<IOPCBusinessService, IUsersService>() as IOPCBusinessService;
            //var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));
            var SiteService = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));

            int sitekey = sites == null ? 0 : int.Parse(sites);
            IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.siteKey.Value == sitekey && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            //tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);

            //FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            var site = SiteService.GetSites(report.UserID, sitekey, null);

			bool refasync = false;
         
            IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
            var conv = iopc.GetTrueTrafficOpportunity(StartTimeOfDate(datenow), EndTimeOfDate(datenow), pacIds);


            Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> sCode = Task.Run(async () =>
            {
                List<Func_BAM_TrueTraffic_Opportunity_Result> msg = await conv;
                return msg;
            });

            List<Func_BAM_TrueTraffic_Opportunity_Result> data = sCode.Result;

            bool flag = true;
            if (data == null || data.Count == 0)
            {
                flag = false;
                data = new List<Func_BAM_TrueTraffic_Opportunity_Result>();
            }

            var result = data.OrderBy(od => od.DVRHour).GroupBy(g => g.DVRHour).Select(group =>
                new DataTrafficInsights() { 
                    Hour = group.Key ?? 0,
                    TrafficIn = group.Any() ? group.Sum(t => t.TrafficIn.HasValue ? t.TrafficIn.Value : 0) : 0,
                    Trans = group.Any() ? group.Sum(t => t.TransCount.HasValue ? t.TransCount.Value : 0) : 0,
                    TrueTraffic = group.Any() ? group.Sum(t => t.TrueTraffic.HasValue ? t.TrueTraffic.Value : 0) : 0,
                    LostOpportunities = group.Any() ? group.Sum(t => t.LostOpportunities.HasValue ? t.LostOpportunities.Value : 0) : 0
                }).ToList();

            List<SumTbl> ChartData = new List<SumTbl>();
            if (flag)
            {
                ChartData = new List<SumTbl>(){
                    new SumTbl(){
                        People = "1",
                        Count = data.Any() ? (int)data.Sum(t => t.Children.HasValue ? t.Children.Value : 0) : 0
                    },
                    new SumTbl(){
                        People = "2",
                        Count = data.Any() ? (int)data.Sum(t => t.Couples.HasValue ? t.Couples.Value : 0) : 0
                    },
                    new SumTbl(){
                    People = "3",
                        Count = data.Any() ? (int)data.Sum(t => t.Singles.HasValue ? t.Singles.Value : 0) : 0
                    }
                };
            }
            
            DataTable dt = ReportFactory.ToDataTable<DataTrafficInsights>(result);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;

            DataTable chartdt = ReportFactory.ToDataTable<SumTbl>(ChartData);
            chartdt.TableName = "SumTbl";


			DataTable parmTable = GetParamsResources(report, requestParms);

            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
            parmTable.Rows.Add("SiteName", siteName);

            var formatCulture = new CultureInfo(languageId.ToLower());
            parmTable.Rows.Add("rpt_Period", datenow.ToString("MMM dd, yyyy", formatCulture));

            CompanyModel Company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (Company != null && Company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(Company.CompanyLogo);
            }
			parmTable.Rows.Add("imgLogopath", imgLogopath);

            DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
            set.Tables.Add(dt);
			set.Tables.Add(parmTable);
            set.Tables.Add(chartdt);
            return set;
        }
	}
}
