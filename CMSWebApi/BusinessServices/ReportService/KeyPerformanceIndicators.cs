using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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

    public class DataKeyPerformanceIndicators
    {
        public string PACID_Name { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public decimal TotalAmount { get; set; }
        public double ConvRate { get; set; }
        public double TotalLaborHours { get; set; }
        public string RegionName { get; set; }
        public int? RegionKey { get; set; }
    }

    public class KeyPerformanceIndicators : ReportBase, IReportService
	{

        public KeyPerformanceIndicators(UserContext userContext, IResposity dbModel)
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
            var type = requestParms["typeCustom"] ?? string.Empty;
			var languageId = requestParms["languageId"] ?? string.Empty;

            if (sDate == string.Empty || eDate == string.Empty || sites == string.Empty || languageId == string.Empty || type == string.Empty)
			{
				throw new NotImplementedException();
			}

			var svc = (IUsersService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUsersService));
            var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
            var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));

			var pos = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
			var iopc = InitBusiness<IOPCBusinessService, IUsersService>() as IOPCBusinessService;

            List<int> sitekey = sites.Split(',').Select(n => Convert.ToInt32(n)).ToList();

			IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac_all = uSites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
            IEnumerable<SitePACID> site_pac = site_pac_all.Join(sitekey, s => s.SiteKey, t => t, (s, t) => new SitePACID() {
                PACID = s.PACID,
                SiteKey = s.SiteKey
            }).Distinct();

            IEnumerable<int> siteKeys = site_pac.Select(x => x.SiteKey).Distinct();
			IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

            DateTime startDate = datenow;
            DateTime endDate = datenow;

            int Type = int.Parse(type);
            if (Type == 1) { 
                // Daily
                
            }else if(Type == 2){
                //Weekly
                tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);
                FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);
                startDate = fyWeekInfo.StartDate;
                endDate = fyWeekInfo.EndDate;
            }

			bool refasync = false;

            // Get Labor Hourly
            bool b_hideWH = Convert.ToBoolean(AppSettings.AppSettings.Instance.BHideWH);
			var labor = pos.GetLaborHourlyWorkingHour(StartTimeOfDate(startDate), EndTimeOfDate(endDate), pacIds);
            Task<List<Func_BAM_LaborHourlyWorkingHour_Result>> Listlabor = Task.Run(async () =>
            {
                List<Func_BAM_LaborHourlyWorkingHour_Result> msg = await labor;
                return msg;
            });


            List<Func_BAM_LaborHourlyWorkingHour_Result> DataLabor = Listlabor.Result;

            if (DataLabor == null || DataLabor.Count == 0)
            {
                DataLabor = new List<Func_BAM_LaborHourlyWorkingHour_Result>();
            }

            var DataLabor_siteKey_PacId = DataLabor.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) => new { 
                siteKey = si.SiteKey,
                pacId = si.PACID,
                DvrDate = cv.DVRDate.HasValue ? cv.DVRDate.Value : new DateTime(),
                DvrHour = cv.DVRHour.HasValue ? cv.DVRHour.Value : 0,
                LaborHours = cv.LaborHours.HasValue ? cv.LaborHours.Value : 0,
                LaborHoursDaily = cv.LaborHoursDaily.HasValue ? cv.LaborHoursDaily.Value : 0
            }).Distinct().ToList();

            var DataLabor_siteKey = DataLabor_siteKey_PacId.GroupBy(t => t.siteKey).Select(group =>
                new 
                { 
                    siteKey = group.Key,
                    LaborHours = group.Any() ? group.Sum(x => x.LaborHours) : 0,
                    LaborHoursDaily = group.Any() ? group.Sum(x => x.LaborHoursDaily) : 0
                }).ToList();

			var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync); ;
			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
			{
				List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
				return msg;
			});
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

			var conv = pos.GetConversionAsync(StartTimeOfDate(startDate), EndTimeOfDate(endDate), pacIds, lsEnableChannels, ref refasync);

			Task<List<Proc_DashBoard_Conversion_Result>> sCode = Task.Run(async () =>
			{
				List<Proc_DashBoard_Conversion_Result> msg = await conv;
				return msg;
			});

			List<Proc_DashBoard_Conversion_Result> Data = sCode.Result;

			if (Data == null || Data.Count == 0)
			{
				Data = new List<Proc_DashBoard_Conversion_Result>();
			}

            var Data_siteKey_PacId = Data.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) => new {
                siteKey = si.SiteKey,
                pacid = si.PACID,
                dvrdate = cv.DVRDate,
                TrafficIn = cv.TrafficIn,
                TrafficOut = cv.TrafficOut,
                CountTrans = cv.CountTrans,
                TotalAmount = cv.TotalAmount
            }).Distinct().ToList();

            var Data_SiteKey = Data_siteKey_PacId.GroupBy(g => g.siteKey).Select(group =>
                new 
                { 
                    siteKey = group.Key,
                    CountTrans = (int)(group.Any() ? group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0),
                    TrafficIn = (int)(group.Any() ? group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
                    TrafficOut = (int)(group.Any() ? group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
                    TotalAmount = group.Any() ? group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
                }).ToList();

            string[] includes = { typeof(tCMSWebRegion).Name };
            IQueryable<tCMSWebSites> dbSites = siteSvc.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
            IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

            var site_region = site_pac.Join(dbSites, si => si.SiteKey, rg => rg.siteKey, (si, rg) => new { site = si, region = rg });
            var site_region_name = site_region.Join(dbRegions, si => si.region.RegionKey, rg => rg.RegionKey, (si, rg) => 
                new { pacid = si.site.PACID, siteKey = si.site.SiteKey, siteName = si.region.ServerID, regionKey = rg.RegionKey, regionName = rg.RegionName }).Distinct().ToList();

            var reesult = Data_SiteKey.Select(si =>
                new DataKeyPerformanceIndicators()
                {
                    PACID_Name = siteSvc.GetSite<string>(si.siteKey, t => t.ServerID, null) ?? string.Empty,
                    CountTrans = si.CountTrans,
                    CountIn = si.TrafficIn,
                    TotalAmount = si.TotalAmount,
                    ConvRate = (si.TrafficIn == 0 || si.CountTrans == 0) ? 0 : ((si.CountTrans * 100) / si.TrafficIn) > 150 ? 0 : (((Double)si.CountTrans * 100) / si.TrafficIn),
                    RegionName = site_region_name.FirstOrDefault(x => x.siteKey == si.siteKey).regionName ?? string.Empty,
                    RegionKey = site_region_name.FirstOrDefault(x => x.siteKey == si.siteKey).regionKey,
                    TotalLaborHours = b_hideWH == false && DataLabor_siteKey.Any() && DataLabor_siteKey.FirstOrDefault(s => s.siteKey == si.siteKey) != null ? DataLabor_siteKey.FirstOrDefault(s => s.siteKey == si.siteKey).LaborHours : 0
                }).OrderBy(od => od.RegionName).ToList();

            DataTable dt = ReportFactory.ToDataTable<DataKeyPerformanceIndicators>(reesult);
			dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


			DataTable parmTable = GetParamsResources(report, requestParms);

            var formatCulture = new CultureInfo(languageId.ToLower());
            
            
			CompanyModel company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (company != null && company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(company.CompanyLogo);
            }
			parmTable.Rows.Add("imgLogopath", imgLogopath);

            string strTo = "To";
            for (int i = 0; i < parmTable.Rows.Count; i++)
            {
                if (parmTable.Rows[i]["NameField"].ToString() == "rpt_To")
                {
                    strTo = parmTable.Rows[i]["ValueField"].ToString();
                }
            }

            if (startDate.CompareTo(endDate) == 0)
            {
                // Daily
                parmTable.Rows.Add("period_FromTo", startDate.ToString("MMM dd, yyyy", formatCulture));
                parmTable.Rows.Add("rpt_Period", startDate.ToString("MMM dd, yyyy", formatCulture));
            }
            else { 
                // Weekly
                if (startDate.Year != endDate.Year)
                {
                    parmTable.Rows.Add("period_FromTo", startDate.ToString("MMM dd, yyyy", formatCulture) + " - " + endDate.ToString("MMM dd, yyyy", formatCulture));
                    parmTable.Rows.Add("rpt_Period", startDate.ToString("MMM dd, yyyy", formatCulture) + " " + strTo + " " + endDate.ToString("MMM dd, yyyy", formatCulture));
                }
                else {
                    parmTable.Rows.Add("period_FromTo", startDate.ToString("MMM dd ", formatCulture) + " - " + endDate.ToString(" dd, yyyy", formatCulture));
                    parmTable.Rows.Add("rpt_Period", startDate.ToString("MMM dd, yyyy", formatCulture) + " " + strTo + " " + endDate.ToString("MMM dd, yyyy", formatCulture));
                }
            }

			// Create a DataSet and put both tables in it.
			DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
			set.Tables.Add(dt);
			set.Tables.Add(parmTable);
			return set;
        }
	}
}
