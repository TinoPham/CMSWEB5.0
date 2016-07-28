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

    public class DailyTableReport {
        public string PACID_Name { get; set; }
        public DateTime? DVRDate { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public decimal TotalAmount { get; set; }
		public string DayOfWeek { get; set; }
        public double TotalLaborHours { get; set; }
    }

    public class DailyReportService : ReportBase, IReportService
	{

		public DailyReportService(UserContext userContext, IResposity dbModel)
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
			
			var IFiscalYear = (IFiscalYearServices)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFiscalYearServices));

			var pos = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;
			var iopc = InitBusiness<IOPCBusinessService, IUsersService>() as IOPCBusinessService;

			int sitekey = sites == null ? 0 : int.Parse(sites);

			IEnumerable<UserSiteDvrChannel> uSites = svc.GetDvrbyUser<UserSiteDvrChannel>(report.UserID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
            IEnumerable<SitePACID> site_pac = uSites.Where(it => it.siteKey.HasValue && it.siteKey.Value == sitekey && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0 && it.siteKey.Value == sitekey).Select(si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
			IEnumerable<int> pacIds = site_pac.Select(si => si.PACID).Distinct();
			bool refasync = false;

			DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(base.UserContext, datenow);
			// tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(report.UserID, new DateTime(2014, 8, 6));

			FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, datenow, fyInfo.FYDateStart.Value);

            // Get Labor Hourly
            var labor = pos.GetLaborHourlyWorkingHour(StartTimeOfDate(fyWeekInfo.StartDate), EndTimeOfDate(fyWeekInfo.EndDate), pacIds);
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

            //var DataLabor_siteKey_PacId = DataLabor.Join(site_pac, cv => cv.PACID, si => si.PACID, (cv, si) => new
            //{
            //    siteKey = si.SiteKey,
            //    pacId = si.PACID,
            //    DvrDate = cv.DVRDate.HasValue ? cv.DVRDate.Value : DateTime.MinValue,
            //    DvrHour = cv.DVRHour.HasValue ? cv.DVRHour.Value : 0,
            //    LaborHours = cv.LaborHours.HasValue ? cv.LaborHours.Value : 0,
            //    LaborHoursDaily = cv.LaborHoursDaily.HasValue ? cv.LaborHoursDaily.Value : 0
            //}).Distinct().ToList();

            var DataLabor_Date = DataLabor.GroupBy(t => t.DVRDate).Select(group =>
                new
                {
                    dvrDate = group.Key,
                    LaborHours = group.Any() ? group.Sum(x => x.LaborHours.HasValue ? x.LaborHours.Value : 0) : 0,
                    LaborHoursDaily = group.Any() ? group.FirstOrDefault(x => x.DVRDate == group.Key).LaborHoursDaily.Value : 0
                }).ToList();

			var enTraffChannel = iopc.DashBoard_Channel_EnableTrafficCount(pacIds, ref refasync); ;
			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> taskEnChan = Task.Run(async () =>
			{
				List<Proc_DashBoard_Channel_EnableTrafficCount_Result> msg = await enTraffChannel;
				return msg;
			});
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = taskEnChan.Result;

            var conv = pos.GetConversionAsync(StartTimeOfDate(fyWeekInfo.StartDate), EndTimeOfDate(fyWeekInfo.EndDate), pacIds, lsEnableChannels, ref refasync);

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
			var conv_NC = Data.Where(x=>x.DVRDate != null).GroupBy(x => x.DVRDate).Select(group =>
			{
				var i = @group.Any() ? @group.Sum(x => x.CountTrans.HasValue ? x.CountTrans : 0) : 0;
				return i != null
					? new
					{
						DVRDate = @group.Key,
						CountTrans = (int)i,
						TrafficIn = (int)(@group.Any() ? @group.Sum(x => x.TrafficIn.HasValue ? x.TrafficIn.Value : 0) : 0),
						TrafficOut = (int)(@group.Any() ? @group.Sum(x => x.TrafficOut.HasValue ? x.TrafficOut.Value : 0) : 0),
						TotalAmount = @group.Any() ? @group.Sum(x => x.TotalAmount.HasValue ? x.TotalAmount.Value : 0) : 0
					}
					: null;
			});

			var formatCulture = new CultureInfo(languageId.ToLower());
			var reesult = conv_NC.OrderBy(od => od.DVRDate).Select(t => new DailyTableReport()
			{
				PACID_Name = "",
				DVRDate = t.DVRDate,
				CountIn = t.TrafficIn,
				CountTrans = t.CountTrans,
				TotalAmount = t.TotalAmount,
				DayOfWeek = ((DateTime)t.DVRDate).ToString("dddd", formatCulture),
                TotalLaborHours = DataLabor_Date.Any() && DataLabor_Date.FirstOrDefault(s => s.dvrDate == t.DVRDate) != null ? DataLabor_Date.FirstOrDefault(s => s.dvrDate == t.DVRDate).LaborHours : 0

			}).ToList();


			DataTable dt = ReportFactory.ToDataTable<DailyTableReport>(reesult);
			dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


			DataTable parmTable = GetParamsResources(report, requestParms);

			var siteSvc = (ISiteService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ISiteService));
			var siteName = siteSvc.GetSite<string>(sitekey, t => t.ServerID, null);
			parmTable.Rows.Add("SiteName", siteName);
			parmTable.Rows.Add("rpt_FromDate", fyWeekInfo.StartDate.ToString("MMM dd, yyyy", formatCulture));
			parmTable.Rows.Add("rpt_ToDate", fyWeekInfo.EndDate.ToString("MMM dd, yyyy", formatCulture));
			CompanyModel company = base.GetCompanyInfo();
            var imgLogopath = string.Empty;
            if (company != null && company.CompanyLogo != null)
            {
                imgLogopath = Convert.ToBase64String(company.CompanyLogo);
            }
			parmTable.Rows.Add("imgLogopath", imgLogopath);

            int b_hideWH = AppSettings.AppSettings.Instance.BHideWH;
            parmTable.Rows.Add("b_hideWH",  Convert.ToBoolean(b_hideWH));

			// Create a DataSet and put both tables in it.
			DataSet set = new DataSet(report.tbl_BAM_Metric_ReportList.ReportResourceName);
			set.Tables.Add(dt);
			set.Tables.Add(parmTable);
			return set;
        }
	}
}
