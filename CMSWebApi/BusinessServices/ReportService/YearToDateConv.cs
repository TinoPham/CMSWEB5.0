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

    public class TableYearToDateConv
    {
        public string PACID_Name { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public double ConvRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string RegionName { get; set; }
    }

    public class YearToDateConv : ReportBase, IReportService
	{

        public YearToDateConv(UserContext userContext, IResposity dbModel)
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
            
			DateTime datenow = EndTimeOfDate(DateTime.ParseExact(sDate, "yyyyMMdd", CultureInfo.InvariantCulture));

            tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);

			var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync); ;
			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
			{
				List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
				return msg;
			});
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

            // YTD
            var conv = pos.GetConversionAsync(fyInfo.FYDateStart.Value, datenow, pacIds, lsEnableChannels, ref refasync);
            Task<List<Proc_DashBoard_Conversion_Result>> sCodeYTD = Task.Run(async () =>
            {
                List<Proc_DashBoard_Conversion_Result> msg = await conv;
                return msg;
            });
            List<Proc_DashBoard_Conversion_Result> Data_YTD = sCodeYTD.Result;

            if (Data_YTD == null || Data_YTD.Count == 0)
            {
                Data_YTD = new List<Proc_DashBoard_Conversion_Result>();
            }


            // Calculator Now each site.
            var dataYTD_siteKey = Data_YTD.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) =>
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

            var site_conv_YTD = dataYTD_siteKey.GroupBy(x => new { x.siteKey }).Select(group =>
                new
                {
                    siteKey = group.Key.siteKey,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();

            string[] includes = { typeof(tCMSWebRegion).Name };
			IQueryable<tCMSWebSites> dbSites = siteSvc.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
			IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

			var site_region = site_pac.Join(dbSites, si => si.SiteKey, rg => rg.siteKey, (si, rg) => new { site = si, region = rg });
			var site_region_name = site_region.Join(dbRegions, si => si.region.RegionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.site.PACID, siteKey = si.site.SiteKey, siteName = si.region.ServerID, regionKey = rg.RegionKey, regionName = rg.RegionName });

            var reesult = site_conv_YTD.Select(si =>
                new TableYearToDateConv()
                {
                    PACID_Name = siteSvc.GetSite<string>(si.siteKey, t => t.ServerID, null) ?? string.Empty,
                    CountTrans = si.CountTrans,
                    CountIn = si.TrafficIn,
                    TotalAmount = si.TotalAmount,
                    ConvRate = (si.TrafficIn == 0 || si.CountTrans == 0) ? 0 : ((si.CountTrans * 100) / si.TrafficIn) > 150 ? 0 : (((Double)si.CountTrans * 100) / si.TrafficIn),
                    RegionName = site_region_name.Where(x => x.siteKey == si.siteKey).FirstOrDefault().regionName ?? string.Empty
                }).OrderBy(od => od.RegionName).ToList();

            DataTable dt = ReportFactory.ToDataTable<TableYearToDateConv>(reesult);
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
            parmTable.Rows.Add("rpt_Period", fyInfo.FYDateStart.Value.ToString("MMM dd, yyyy", formatCulture) + " - " + datenow.ToString("MMM dd, yyyy", formatCulture));

            // Create a DataSet and put both tables in it.
            DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
            set.Tables.Add(dt);
            set.Tables.Add(parmTable);
            return set;
        }
	}
}
