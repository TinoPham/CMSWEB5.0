using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.Alert;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using Commons;
using Extensions;
using PACDMModel.Model;
using SVRDatabase;
using AlertModel = CMSWebApi.DataModels.AlertModel;

namespace CMSWebApi.BusinessServices.SiteAlerts
{
	public class SiteAlertsBusiness : BusinessBase<IAlertService>
	{
		private const string ChannelPrefix = "channel_";
		public IDvrChanelService ChannelSvc { get; set; }
		public ICompanyService CompanyService { get; set; }
		public IDVRService DVRService { get; set; }

		public void IgnoreAlerts(UserContext context, IgnoreAlertModel ignoreAlert)
		{
			DataService.DeleteLastAlert(t => t.KAlertEvent == ignoreAlert.KAlert);
			var alertdetails = DataService.GetAlertEventDetails(t => t.KAlertEvent == ignoreAlert.KAlert, t => t).ToList();
			if (alertdetails.Count > 0)
			{
				if (ignoreAlert.Kchannel > 0)
				{
					alertdetails = alertdetails.Where(t => t.KChannel == ignoreAlert.Kchannel).ToList();
				}
				foreach (var alertdetail in alertdetails)
				{
					alertdetail.IsManual = true;
					alertdetail.Description = ignoreAlert.Description;
					DataService.UpdateAlertDetail(alertdetail);
				}

				if (DataService.Save() < 0)
				{
					throw new CmsErrorException(CMSWebError.SERVER_ERROR_MSG.ToString());
				}
			}	
		}

		public IQueryable<SiteMonitorModel> GetAlertLastByDvrs(List<int> kdvrs,int[] atypes, SVRManager DBModel, UserContext userContext)
		{

			//var filePath = Path.Combine(AppSettings.AppSettings.Instance.AppData, "ReportTemplates\\Sites\\heathmonitor.xlsx");

			//HeathMonitorReport headReport = new HeathMonitorReport(filePath, "SheetName");

			//headReport.GenerateHeathMonitorReport();
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			int dvrofflineType = (int)AlertType.DVR_is_off_line;
			var now = DateTime.Now;

			if (atypes != null)
			{
				if (atypes[0] == (int)AlertType.DVR_is_off_line)
				{
					var dvrOfflines = DataService.GetDVR_On_Offline_Async(kdvrs, now, true, 0, interval).Result;
					var dvrofflinealert = dvrOfflines.Where(t => t.KDVR != null).Select(t => new SiteMonitorModel()
					{
						Kdvr = (int)t.KDVR,
						AlertTypeId = dvrofflineType,
						TotalAlert = 1,
						TimeZone = t.Minutes != null ? now.AddMinutes(-(int)t.Minutes) : now
					});
					return dvrofflinealert.AsQueryable();

				}
				else if (atypes[0] == (int)AlertType.DVR_Record_Less_Than)
				{
					tCMSSystemConfig sysConfig = userContext.CompanyID > 0 ? CompanyService.SelectRecordingDay(userContext.CompanyID) : null;

					int recodingday = sysConfig != null ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;
					var dvrRecordLessThan = DVRService.GetDVRs<SiteMonitorModel>(item => item.RecordingDay < recodingday && kdvrs.Contains(item.KDVR), t => new SiteMonitorModel()
					{
						Kdvr = (int)t.KDVR,
						AlertTypeId = (int)AlertType.DVR_Record_Less_Than,
						TotalAlert = 1,
						TimeZone = now
					}, null).ToList();
					return dvrRecordLessThan.AsQueryable();

				}
				else
				{
					return null;
				}
			}
			else 
			{
				var alertList = DataService.GetLastAlertEvents(t => kdvrs.Contains(t.KDVR), t => new SiteMonitorModel()
				{
					AlertTypeId = t.KAlertType,
					Kdvr = t.KDVR,
					TotalAlert = 1,
					Kalert = t.KAlertEvent,
					TimeZone = t.TimeZone
				}, null).ToList();
				tCMSSystemConfig sysConfig = userContext.CompanyID > 0 ? CompanyService.SelectRecordingDay(userContext.CompanyID) : null;

				int recodingday = sysConfig != null ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;
				var dvrRecordLessThan = DVRService.GetDVRs<SiteMonitorModel>(item => item.RecordingDay < recodingday && kdvrs.Contains(item.KDVR), t => new SiteMonitorModel()
				{
					Kdvr = (int)t.KDVR,
					AlertTypeId = (int)AlertType.DVR_Record_Less_Than,
					TotalAlert = 1,
					TimeZone = now
				}, null).ToList();
				alertList.AddRange(dvrRecordLessThan);

				var dvrOfflines = DataService.GetDVR_On_Offline_Async(kdvrs, now, true, 0, interval).Result;
				var dvrofflinealert = dvrOfflines.Where(t => t.KDVR != null).Select(t => new SiteMonitorModel()
				{
					Kdvr = (int)t.KDVR,
					AlertTypeId = dvrofflineType,
					TotalAlert = 1,
					TimeZone = t.Minutes != null ? now.AddMinutes(-(int)t.Minutes) : now
				});
				alertList.AddRange(dvrofflinealert);
				return alertList.AsQueryable();
			}
		}

		public IQueryable<SiteMonitorModel> GetAlertByDvrs(List<int> kdvrs, DateTime begin, DateTime end)
		{
			return DataService.GetAlertEventsByKdvrs(kdvrs, begin, end);
		}

		public IQueryable<SiteSensorsModel> GetSensorsEventsByKdvrs(List<int> kdvrs, DateTime begin, DateTime end)
		{
			return DataService.GetSensorsEventsByKdvrs(kdvrs, begin, end);
		}

		public IQueryable<AlertSensorDetail> GetAlertSensorsDetails(int kdvr, DateTime date, int uncertaintySecond)
		{
			var alerts = DataService.GetAlerts(t => t.KDVR == kdvr && t.KAlertType == (int)AlertType.DVR_Sensor_Triggered && System.Data.Entity.DbFunctions.TruncateTime(t.TimeZone) == System.Data.Entity.DbFunctions.TruncateTime(date), t => new
			{
				Channel = t.Channel ?? -2, 
				Status = t.Status, 
				TimeZone = t.TimeZone, 
				Description = t.Description, 
				KAlertType = t.KAlertType, 
				KAlertEvent = t.KAlertEvent,
				ImageTime = t.ImageTime,
			}, null).OrderByDescending(x=>x.TimeZone).ToList();
			var channels = ChannelSvc.GetChannels(t => t.KDVR == kdvr, t => new {KDVR = t.KDVR, KChannel = t.KChannel, Name = t.Name, ChannelNo = t.ChannelNo}, null).ToList();

			return (from p in alerts
				join r in channels on p.Channel equals r.ChannelNo into g
				from x in g.DefaultIfEmpty()
				select new AlertSensorDetail
				{
					Id = p.KAlertEvent,
					ChannelNo = x != null ? x.ChannelNo : p.Channel,
					KChannel = x != null ? x.KChannel : -2,
					TimeZone = p.ImageTime != null ? (long)p.ImageTime: 0,
					FullTime = p.TimeZone.Value.ToString(),
					Time = p.TimeZone.Value.ToLongTimeString(),
					ChannelName = x != null ? x.Name : "",
					Description = p.Description,
					SnapShot = x != null ? GetImagesInfo(x.KDVR, x.ChannelNo, p.TimeZone, uncertaintySecond) : new List<string>()
				}).AsQueryable();
		}

		private List<string> GetImagesInfo(int kdvr, int channelNo, DateTime? time, int uncertaintySecond)
		{
			var images = new List<string>();
			if (time == null) return images;

			string fpath = Path.Combine(AppSettings.AppSettings.Instance.AlertImagesPath, kdvr.ToString());
			int channel = channelNo + 1;
			string channelPattern = ChannelPrefix  + channel + "*";

			images = FileManager.DirGetFileInfos(fpath, channelPattern, false)
				.Where(t => t.LastWriteTimeUtc >= time.Value.AddSeconds(-uncertaintySecond) && t.LastWriteTimeUtc <= time.Value.AddSeconds(uncertaintySecond))				
				.Select(t => t.Name).ToList();
			return images;
		}

		public IQueryable<AlertEventType> GetAllAlertTypes(UserContext userContext)
		{
			return GetAllAlertTypes( userContext == null? 0 : userContext.CompanyID);
		}
		public IQueryable<AlertEventType> GetAllAlertTypes(int companyID)
		{

			tCMSSystemConfig sysConfig = companyID > 0 ? CompanyService.SelectRecordingDay(companyID) : null;
			int recodingday = sysConfig != null ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;

			var alertTypes = DataService.GetAlertTypes(null, t => new AlertEventType()
			{
				Id = t.KAlertType,
				Name = t.AlertType,
				KAlertSeverity = t.KAlertSeverity,
				CmsWebGroup = t.CMSWebGroup,
				CmsWebType = t.CMSWebType
			}, null).ToList();

			alertTypes.Add(new AlertEventType()
			{
				Id = (int) AlertType.DVR_Record_Less_Than,
				Name = string.Format(AppSettings.AppSettings.Instance.DvrRecordingLessThan, recodingday)
			});

			//var recordLessThanType = alertTypes.FirstOrDefault(t => t.Id == (int) AlertType.DVR_Record_Less_Than);
			//if (recordLessThanType != null)
			//{
			//	recordLessThanType.Name = string.Format(recordLessThanType.Name, recodingday);
			//}
			return alertTypes.AsQueryable();
		}

        public IQueryable<AlertEventType> GetEmailAlertsType(UserContext userContext)
        {
            var result = GetAllAlertTypes(userContext);
            InternalBusinessService.AlertFixConfigs acfg = InternalBusinessService.AlertFixConfigs.Instance;
            IEnumerable<byte> AlertEmail = new List<byte>();
            AlertEmail = acfg.GetAlertConfigEmail().Select(it => it.AlertType);
            if (AlertEmail != null && AlertEmail.Any())
            {
                result = result.Where(item => AlertEmail.Contains(item.Id));
            }
            return result;
        }


		#region Alert_MAPs

		public AlertModel EntityToModel(tAlertEvent alertEvent)
		{

			AlertModel model = new AlertModel();
			if (alertEvent != null)
			{
				model.Channel = alertEvent.Channel;
				model.Description = alertEvent.Description;
				model.DVRUser = alertEvent.DVRUser;
				model.KAlertEvent = alertEvent.KAlertEvent;
				model.KAlertType = alertEvent.KAlertType;
				model.KDVR = alertEvent.KDVR;
				model.Time = alertEvent.Time;
				model.TimeZone = alertEvent.TimeZone;
				model.AlertType = alertEvent.tAlertType.AlertType;
			}
			return model;


		}


		public AlertModel EntityToModel(View_Alerts_Acknowlegdement alertEvent)
		{
			
			AlertModel model = new AlertModel();
			if (alertEvent != null)
			{
				model.Channel = alertEvent.ChannelNo;
				model.Description = alertEvent.Description;
				model.DVRUser = alertEvent.DVRUser;
				model.KAlertEvent = alertEvent.KAlertEvent;
				model.KAlertType = alertEvent.KAlertType;
				model.KDVR = alertEvent.KDVR;
				model.Time = alertEvent.Time;
				model.TimeZone = alertEvent.TimeZone;
				model.AlertType = alertEvent.AlertType;
			}
			return model;


		}

		public List<AlertModel> SiteAlerts(List<int> kdvr, DateTime begin, DateTime end,string TypeIDs)
		{
			List<tAlertEvent> talert = DataService.GetAlertsByTimeZone(kdvr, begin, end, TypeIDs).ToList();
		
			return talert.Select(item => EntityToModel(item)).OrderByDescending(item=>item.TimeZone).ToList();
		}


		public List<View_Alerts_Acknowlegdement> GetAcknowlegdementAlerts(List<int> kdvr, DateTime begin, DateTime end, string TypeIDs = "")
		{
			List<View_Alerts_Acknowlegdement> talert = DataService.GetAcknowlegdementAlerts(kdvr, begin, end, TypeIDs).ToList();

			if (talert.Any())
			{
				for (int i = 0; i < talert.Count(); i++)
				{
					if (talert[i].KAlertType == (int)AlertType.DVR_Sensor_Triggered || talert[i].KAlertType == (int)AlertType.DVR_Control_Activated)
					{
						if (talert[i].ChannelNo.HasValue)
						{
							List<string> images = new List<string>();
							images = GetImagesInfo(talert[i].KDVR.Value, talert[i].ChannelNo.Value, talert[i].TimeZone.Value, 10);
							talert[i].Image = images.Any() ? string.Join("," ,images.ToArray()) : "";
						}
						else
						{
							talert[i].Image = "";
						}
					}
					else
					{
						talert[i].Image = "";
					}
				}
			}
			return talert.OrderByDescending(item => item.TimeZone).ToList();
		}

        public List<string> GetImagesAlert(int kdvr,int ChannelNo, DateTime TimeZone)
        {
            List<string> images = new List<string>();
            images = GetImagesInfo(kdvr, ChannelNo, TimeZone, 10);
            return images;
        }

		public Task<List<AlertModelSummary>> SiteAlertsSummary(List<int> kdvr, DateTime begin, DateTime end, string TypeIDs)
		{
			return DataService.AlertsSummary(kdvr, begin, end, TypeIDs);
		} 

		//public List<string> GetAlertConfig()
		//{
		//	List<string> result = new List<string>();
		//	string path = Path.Combine(AppSettings.AppSettings.Instance.AppData, Utils.Consts.Config_FileName);
		//	XmlDocument doc = new XmlDocument();
		//	doc.Load(path);
		//	XmlNode root = doc.DocumentElement;
		//	foreach (XmlNode node in root.ChildNodes)
		//	{
		//		string att_val = XMLUtils.XMLAttributeValue(node, Utils.Consts.str_alt);
		//		result.Add(att_val);
		//	}
		//	return result;
		//}
	
        public List<EmailSettingModel> GetEmailSettingByUser(UserContext user, int reportID) {

            Emailsetting.EmailsettingBusinessService BS = new Emailsetting.EmailsettingBusinessService();
            BS.DataService = DataService;
            if (reportID != 0)
            {
                List<EmailSettingModel> rs = new List<EmailSettingModel>();
                rs.Add(BS.GetReportByID(user,reportID));
                return rs;
            }
            return BS.GetReportByUser(user);
        }

        public bool SaveEmailSetting(UserContext user,EmailSettingModel mod) 
        {
            Emailsetting.EmailsettingBusinessService BS = new Emailsetting.EmailsettingBusinessService();
            BS.DataService = DataService;
            return BS.SaveEmailSetting(user,mod);
        }

        public bool DeleteEmailSetting(UserContext user, int ID)
        {
            Emailsetting.EmailsettingBusinessService BS = new Emailsetting.EmailsettingBusinessService();
            BS.DataService = DataService;
            return BS.DeleteEmailSetting(user, ID);
        }
		#endregion
		#region
		internal IQueryable<T> GetDVRs<T>(IEnumerable<int> kdvrs,System.Linq.Expressions.Expression<Func<tDVRAddressBook,T>> selector)
		{
            var includes = new string[]
			{
				typeof (tDVRNetwork).Name, 
            };
            return DVRService.GetDVRs<T>(item => kdvrs.Contains(item.KDVR), selector, includes);
			
		}
		#endregion
	}
}
