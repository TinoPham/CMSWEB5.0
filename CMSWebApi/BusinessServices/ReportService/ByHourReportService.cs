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

    public class TableReport {
        public string PACID_Name { get; set; }
        public DateTime? DVRDate { get; set; }
        public int DVRHour { get; set; }
        public int CountIn { get; set; }
        public int CountTrans { get; set; }
        public int ConvRate { get; set; }
        public decimal TotalAmount { get; set; }
        public double TotalLaborHours { get; set; }
        public double TotalLaborHoursDaily { get; set; }
    }

    public class ByHourReportService : ReportBase, IReportService
	{

		public ByHourReportService(UserContext userContext, IResposity dbModel)
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
            //var base = InitBusiness<POSBusinessService, IUsersService>() as POSBusinessService;

            DateTime datenow = DateTime.ParseExact(sDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

            int sitekey = sites == null ? 0 : int.Parse(sites);
            IEnumerable<UserSiteDvrChannel> uSites =
                                                svc.GetDvrbyUser<UserSiteDvrChannel>
                                                (
                                                  report.UserID
                                                , item => new UserSiteDvrChannel
                                                        {
                                                            KChannel = item.KChannel
                                                            ,
                                                            KDVR = item.KDVR
                                                            ,
                                                            PACID = item.PACID
                                                            ,
                                                            siteKey = item.siteKey
                                                            ,
                                                            UserID = item.UserID
                                                        });

            IEnumerable<SitePACID> site_pac = 
                                            uSites
                                            .Where(
                                                       it => it.siteKey.HasValue 
                                                    && it.PACID.HasValue
                                                    && it.siteKey.Value == sitekey
                                                    && it.siteKey.Value > 0
                                                    && it.PACID.Value > 0
                                                  )
                                            .Select
                                                    (si => new SitePACID() { PACID = si.PACID.Value, SiteKey = si.siteKey.Value }).Distinct();
            
			bool refasync = false;
         
            string pacIds = site_pac.Any() ? string.Join(",", site_pac.Select(si => si.PACID).Distinct().ToList()) : string.Empty;

            IEnumerable<int> pacIDS = site_pac.Select(si => si.PACID).Distinct();

            // Get Labor Hourly
            bool b_hideWH = Convert.ToBoolean(AppSettings.AppSettings.Instance.BHideWH);
            List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result> DataLabor = new List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>();
            if (b_hideWH == false)
            {
                var labor = iopc.GetLaborHourlyMinSecsWorkingHour(StartTimeOfDate(datenow), EndTimeOfDate(datenow), pacIDS);
                Task<List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>> Listlabor = Task.Run(async () =>
                {
                    List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result> msg = await labor;
                    return msg;
                });

                DataLabor = Listlabor.Result;

                if (DataLabor == null || DataLabor.Count == 0)
                {
                    DataLabor = new List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>();
                }
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

            //var DataLabor_siteKey = DataLabor.GroupBy(t => new { t.DVRDate }).Select(group =>
            //    new
            //    {
            //        dvrHour = group.Key,
            //        dvrDate = group.Any() ? group.FirstOrDefault().DVRDate.Value : DateTime.MinValue,
            //        LaborHours = group.Any() ? group.Sum(x => x.LaborHours.HasValue ? x.LaborHours.Value : 0) : 0,
            //        LaborHoursDaily = group.Any() ? group.Sum(x => x.LaborHoursDaily.HasValue ? x.LaborHoursDaily.Value : 0) : 0
            //    }).ToList();


            var conv = ssrp.GetPOSConversionHourly(pacIds, StartTimeOfDate(datenow), EndTimeOfDate(datenow));


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

            var reesult = Data.OrderBy(ob => ob.tHour).GroupBy(g => g.tHour).Select(t => new TableReport()
            {
                PACID_Name = "",
                DVRDate = t.First().DVRDateKey,
                DVRHour = t.Key ?? 0,
                CountIn = t.Any() ? t.Sum(x => x.TrafficIn.Value) : 0,
                CountTrans = t.Any() ? t.Sum(x => x.CountTrans.Value) : 0,
                ConvRate = 0,
                TotalAmount = t.Any() ? t.Sum(x => x.TotalAmount.Value) : 0,
                TotalLaborHours = b_hideWH == false && DataLabor.Any() ? 
                        DataLabor.Where
                                    (w => 
                                           w.InHour.Value <= t.Key.Value 
                                        && t.Key.Value <= w.OutHour.Value
                                    )
                                 .GroupBy
                                    (subg => 
                                            subg.DVRDate.Value
                                    )
                                 .Select(s => 
                                            s.Sum(
                                                (a => 
                                                    ((a.OutHour.Value < (t.Key.Value + 1) ? (a.OutHour.Value * 3600 + a.OutMin.Value * 60 + a.OutSec.Value) : ((double)t.Key.Value + 1) * 3600) 
                                                    - 
                                                    (a.InHour.Value < t.Key.Value ? ((double)t.Key.Value * 3600) : (a.InHour.Value * 3600 + a.InMin.Value * 60 + a.InSec.Value))) / 3600))).FirstOrDefault() 
                                  : 0,
                TotalLaborHoursDaily = b_hideWH == false && DataLabor.Any() ? 
                        DataLabor.GroupBy(g => 
                                            g.DVRDate
                                         )
                                 .Select(group =>
                                                group.Sum(s => ((s.OutHour.Value * 3600) + (s.OutMin.Value * 60) + s.OutSec.Value) - ((s.InHour.Value * 3600) + (s.InMin.Value * 60) + s.InSec.Value) < 0 ? 0 :
                    (((s.OutHour.Value * 3600) + (s.OutMin.Value * 60) + s.OutSec.Value) - ((s.InHour.Value * 3600) + (s.InMin.Value * 60) + s.InSec.Value)) / 3600
                    )).FirstOrDefault() : 0

            }).ToList();

			

            DataTable dt = ReportFactory.ToDataTable<TableReport>(reesult);
            dt.TableName = report.tbl_BAM_Metric_ReportList.ReportResourceName;


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
            
            parmTable.Rows.Add("b_hideWH", b_hideWH);

            DataSet set = new DataSet("tblData");
            set.Tables.Add(dt);
			set.Tables.Add(parmTable);
            return set;
        }
	}
}
