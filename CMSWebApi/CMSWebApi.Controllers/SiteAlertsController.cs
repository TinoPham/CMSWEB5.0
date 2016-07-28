using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.APIFilters.ErrorHandler;
using CMSWebApi.BusinessServices.SiteAlerts;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.Alert;
using CMSWebApi.ServiceInterfaces;
using SVRDatabase;
using AlertModel = CMSWebApi.DataModels.AlertModel;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class SiteAlertsController : ApiControllerBase<IAlertService, SiteAlertsBusiness>
	{
		[HttpGet]
		public HttpResponseMessage GetSiteAlertByDvrs([FromUri] SiteMonitorParam siteMonitor)
		{
			return ExecuteBusiness<List<SiteMonitorModel>>(() =>
			{
				List<SiteMonitorModel> model = BusinessService.GetAlertByDvrs(siteMonitor.Dvrs.ToList(), siteMonitor.Begin, siteMonitor.End).ToList();
				return model;
			});
		}
		[HttpGet]
		public HttpResponseMessage GetAlertLastByDvrs([FromUri] SiteMonitorParam siteMonitor)
		{
			return ExecuteBusiness<List<SiteMonitorModel>>(() =>
			{
				SVRManager cfgDB = base.DependencyResolver <SVRManager>();
				List<SiteMonitorModel> model = BusinessService.GetAlertLastByDvrs(siteMonitor.Dvrs.ToList(), siteMonitor.AlertTypes, cfgDB, usercontext).ToList();
				return model;
			});
		}

		[HttpGet]
		public HttpResponseMessage GetSensorsAlertByDvrs([FromUri] SiteMonitorParam siteMonitor)
		{
			return ExecuteBusiness<List<SiteSensorsModel>>(() =>
			{
				List<SiteSensorsModel> model = BusinessService.GetSensorsEventsByKdvrs(siteMonitor.Dvrs.ToList(), siteMonitor.Begin, siteMonitor.End).ToList();
				return model;
			});
		}


		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage IgnoreAlerts(IgnoreAlertModel ignoreAlert)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.IgnoreAlerts(base.usercontext, ignoreAlert);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}


		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage SiteAlerts([FromUri]AlertsParam alert)
		{
			return ExecuteBusiness<List<PACDMModel.Model.View_Alerts_Acknowlegdement>>(() =>
			{
				DateTime s = CMSWebApi.Utils.Utilities.DateTimeParseExact(alert.Begin, CMSWebApi.Utils.Consts.QUERY_STRING_DATE_FORMAT_SS, DateTime.MinValue);
				DateTime e = CMSWebApi.Utils.Utilities.DateTimeParseExact(alert.End, CMSWebApi.Utils.Consts.QUERY_STRING_DATE_FORMAT_SS, DateTime.MinValue);
				char[] seperator = new char[1];
				seperator[0] = Utils.Consts.DECIMAL_SIGN;
				string dvr = System.Web.HttpUtility.UrlDecode(alert.Dvrs);
				List<PACDMModel.Model.View_Alerts_Acknowlegdement> model = BusinessService.GetAcknowlegdementAlerts(dvr.Split(seperator).Select(item => int.Parse(item)).ToList(), s, e, alert.TypeIDs);
				return model;
			});
		}

		[HttpGet]
		[ActivityLog]
		public  HttpResponseMessage SiteAlertsSummary([FromUri]AlertsParam alert)
		{
			return  ExecuteBusiness<List<AlertModelSummary>>(() =>
			{
				DateTime s = CMSWebApi.Utils.Utilities.DateTimeParseExact(alert.Begin, CMSWebApi.Utils.Consts.QUERY_STRING_DATE_FORMAT_SS, DateTime.MinValue);
				DateTime e = CMSWebApi.Utils.Utilities.DateTimeParseExact(alert.End, CMSWebApi.Utils.Consts.QUERY_STRING_DATE_FORMAT_SS, DateTime.MinValue);
				List<AlertModelSummary> model = BusinessService.SiteAlertsSummary(System.Web.HttpUtility.UrlDecode(alert.Dvrs).Split(new char[]{Utils.Consts.DECIMAL_SIGN}).Select(item => int.Parse(item)).ToList(), s, e,alert.TypeIDs).Result.OrderByDescending(t=>t.TimeZone).ToList();
				return model;
			});
		}



		[HttpGet]
		public HttpResponseMessage GetAlertSensorsDetails([FromUri]int kdvr, [FromUri] string date, [FromUri] int uncertse = 10)
		{
			DateTime rdate = CMSWebApi.Utils.Utilities.DateTimeParseExact( date,CMSWebApi.Utils.Consts.QUERY_STRING_DATE_FORMAT, DateTime.MinValue);
			if( rdate == DateTime.MinValue)
				return ResponseData<List<AlertSensorDetail>>( null, System.Net.HttpStatusCode.BadRequest);

			return ExecuteBusiness<List<AlertSensorDetail>>(() =>
			{
				List<AlertSensorDetail> model = BusinessService.GetAlertSensorsDetails(kdvr, rdate, uncertse).ToList();
				return model;
			});
		}

		[HttpGet]
		public HttpResponseMessage GetAllAlertTypes()
		{
			return ExecuteBusiness<List<AlertEventType>>(() =>
			{
				List<AlertEventType> model = BusinessService.GetAllAlertTypes(usercontext).ToList();
				return model;
			});
		}


        [HttpGet]
        public HttpResponseMessage GetAllEmailAlertTypes()
        {
            return ExecuteBusiness<List<AlertEventType>>(() =>
            {
                List<AlertEventType> model = BusinessService.GetEmailAlertsType(usercontext).ToList();
                return model;
            });
        }


		[HttpGet]
		public HttpResponseMessage GetSensorSnapshot(string filename, int kdvr)
		{
            string fpath = string.Empty;
            if (filename == null) filename = "";
			fpath = Path.Combine(AppSettings.AppSettings.Instance.AlertImagesPath, kdvr.ToString(), filename.ToString());
            if (!File.Exists(fpath))
            {
                fpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Utils.Consts.ImagesFolder, Utils.Consts.THUMBNAIL_Icon);

            }
			return ResponseFile(fpath);
		}

        [HttpGet]
        [ActivityLog]
        public HttpResponseMessage GetImagesAlert([FromUri] int kdvrs, [FromUri] int channelNo, [FromUri] string timeZone)
        {
            return ExecuteBusiness<List<string>>(() =>
            {
                List<string> images = new List<string>();

                DateTime TZ = DateTime.ParseExact(timeZone, Utils.Consts.QUERY_STRING_DATE_FORMAT_SS, null);

                images = BusinessService.GetImagesAlert(kdvrs, channelNo, TZ);
                return images;
            });
        }

        [HttpGet]
        public HttpResponseMessage GetEmailSettingByUser(int reportID)
        {
            return ExecuteBusiness<List<EmailSettingModel>>(() =>
            {
                return BusinessService.GetEmailSettingByUser(usercontext, reportID);
            });
        }
		
			
        [HttpPost]
        [ActivityLog]
        public bool SaveEmailSettings(EmailSettingModel mod)
        {
            return BusinessService.SaveEmailSetting(usercontext, mod);
        }

        [HttpPost]
        [ActivityLog]
        public bool DeleteEmailSettings(EmailSettingModel mod)
        {
            return BusinessService.DeleteEmailSetting(usercontext, mod.ReportKey);
        }
	}
}
