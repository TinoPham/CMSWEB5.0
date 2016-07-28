using CMSWebApi.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using CMSWebApi.DataServices;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.Alert;
using CMSWebApi.BusinessServices.InternalBusinessService;
using Extensions.Linq;
using CMSWebApi.BusinessServices.Company;
using CMSWebApi.Configurations;
using CMSWebApi.Email;
using CMSWebApi.Email.SMTP;

namespace CMSWebApi.BusinessServices.Emailsetting
{
public class EmailsettingBusinessService : BusinessBase<IAlertService>
{
	const int Limit_period_time_report = 2;
	internal IQueryable<tCMSWebReport>GetReportByUser(int UserKey)
	{
		IServiceBase<tCMSWebReport> svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
		IEnumerable<string>includes = ServiceBase.ChildProperties(typeof(tCMSWebReport));
		IQueryable<tCMSWebReport> result = svr.Gets<tCMSWebReport>(filter => filter.UserKey == UserKey, includes.ToArray(), selector => selector);
		return result;
	}

	internal tCMSWebReport GetReportbyID(int reportID)
	{
		IEnumerable<string> includes = ServiceBase.ChildProperties(typeof(tCMSWebReport));
		IServiceBase<tCMSWebReport> svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
		tCMSWebReport mod = svr.Get<tCMSWebReport>(filter => filter.ReportKey == reportID, includes.ToArray(), selector => selector);
		return mod;
	}

	public IEnumerable<AlertReportActive> AlertReportActive()
	{
		
		DateTime now = DateTime.Now;
		//TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
		//now = TimeZoneInfo.ConvertTime(DateTime.Now, easternZone);
		IEmailSettingService dbservice = new CMSWebApi.DataServices.EmailSettingService(ServiceBase);
		var dbresult = dbservice.CurrentReportSchedule(now);
		IEnumerable<AlertReportActive> schedules = dbresult.Select( it => new AlertReportActive{ FreqTypeID = it.FreqTypeID.Value, LastRunDate = it.LastRunDate.Value, NextRunDate = it.NextRunDate.Value, ReportKey = it.ReportKey, StartRunDate = it.StartRunDate.Value});
		return schedules.Where( it => it.NextRunDate <= now);

	}

	private DateTime CorrectRunDate( int type, DateTime StartDate, DateTime LastRun , DateTime NextRun)
	{
		DateTime min_report_date = type == (int)CMSWebApi.Utils.BAMReportType.Hourly ? NextRun.AddHours(-1 * Limit_period_time_report) : NextRun.AddDays(-1 * Limit_period_time_report) ;
		return min_report_date < LastRun ? LastRun : min_report_date;
	}

	public async Task<AlertReportResult> EmailingAlert(int DVRKeepAliveInterval, AlertReportActive reportmodel, string urlimage)
	{
		tCMSWebReport report = GetReportbyID(reportmodel.ReportKey);
		if( report == null)
			return null;

		CompanyBusinessService CompanyService = new Company.CompanyBusinessService{ DataService = new CompanyService(ServiceBase)};
		int recordingday = CompanyService.CMSSystemRecordingDayConfig((int)report.tCMSWeb_UserList.CompanyID);

		DateTime LastRunDate = CorrectRunDate(reportmodel.FreqTypeID, reportmodel.StartRunDate, reportmodel.LastRunDate, reportmodel.NextRunDate);  //report.LastRunDate.HasValue? report.LastRunDate.Value : report.NextRunDate.Value;

		IEnumerable<byte> reportAlertType = report.tAlertType.Select(it => it.KAlertType);
		IEnumerable<tCMSWebSites> Sites = report.tCMSWebSites;
		IEnumerable<UserSiteDvrChannel> Allsitedvrs = await base.UserSitesAsync(report.UserKey);

		IEnumerable<UserSiteDvrChannel> Reportsitedvrs = Allsitedvrs.Join(Sites, a => a.siteKey, b=> b.siteKey, (a,b)=> a);

		IEnumerable<int> TotalDVRs = Allsitedvrs.Where(it => it.KDVR.HasValue).Select(it => it.KDVR.Value).Distinct();

		var AlertTypes_svr = new SiteAlerts.SiteAlertsBusiness();

		AlertTypes_svr.DataService = new AlertService(ServiceBase);

		AlertTypes_svr.CompanyService = new CompanyService(ServiceBase);

		var all_alert_types = AlertTypes_svr.GetAllAlertTypes((int)report.tCMSWeb_UserList.CompanyID);

		IEnumerable<AlertEventType> ReportAltTypes = all_alert_types.Join( reportAlertType, t=> t.Id, r=> r, (t,r) => t).ToList();

		List<Func_DVR_Offline_Result> Func_DVROffline = await DataService.GetDVR_On_Offline_Async(TotalDVRs, LastRunDate, true, 0, DVRKeepAliveInterval);
		

		SiteAlerts.SiteAlertsBusiness svr = new SiteAlerts.SiteAlertsBusiness { DVRService = new DVRService(base.ServiceBase) };

		IQueryable<tDVRAddressBook> Total_DVRAddressBooks = svr.GetDVRs<tDVRAddressBook>(TotalDVRs, it => it);
		var dvr_keepalive = Total_DVRAddressBooks.Where( it => it.tDVRKeepAlife != null).Select( it => it.KDVR);

		IEnumerable<Func_DVR_Offline_Result> _DVROffline = Func_DVROffline.Where(it => it.Minutes > 0);
		var DVROffline = _DVROffline.LeftOuterJoin(dvr_keepalive, o => o.KDVR, k => k, (o, k, dvr) => new Func_DVR_Offline_Result { CMSMode = o.CMSMode, Status = o.Status, KDVR = o.KDVR, Minutes = k == 0 ? null : o.Minutes });

		IQueryable<EmailAlertModel> alerts = GetEmailingAlertData(DVROffline, ReportAltTypes, Sites, LastRunDate, Reportsitedvrs, urlimage);

		
		
		string report_user = string.Format("{0} {1}", report.tCMSWeb_UserList.UFirstName, report.tCMSWeb_UserList.ULastName);
		tCMSWeb_Company tCompany = new CompanyService(ServiceBase).SelectCompanyInfo((int)report.tCMSWeb_UserList.CompanyID);

		string companyname = tCompany == null ? string.Empty : tCompany.CompanyName;
		string frequency = CMSWebApi.Utils.Utilities.FrequencytoText( Commons.Utils.GetEnum<CMSWebApi.Utils.BAMReportType>( report.FreqTypeID) );
		int CountDVR = TotalDVRs.Count();
		int countDVRConnect = CountDVR - DVROffline.Count( t => t.KDVR.HasValue);
		int total_recording_less = Total_DVRAddressBooks.Count( it => it.RecordingDay.Value < recordingday);
		int total_recording_great = Total_DVRAddressBooks.Count(it => it.RecordingDay.Value > recordingday);
		StringBuilder email_format = DVRAlertFormat.Instance.EmailDataFormat(alerts, LastRunDate, report.ReportName, companyname, report_user, frequency, CountDVR, countDVRConnect, total_recording_less, total_recording_great, recordingday, recordingday);
		//System.IO.File.WriteAllText(@"d:\email.html", email_format.ToString());
		//return new AlertReportResult();

		EmailSettingSection EmailSetting = AppSettings.AppSettings.Instance.EmailSetting;
		EmailService email = CMSWebApi.Email.EmailService.Create(EmailSetting);
		email.AddReceipt(report.tCMSWeb_UserList.UEmail, report_user);
		SendMailResult result = await email.SendMessageAsync(report.ReportName, email_format.ToString(), true, System.Threading.CancellationToken.None).ConfigureAwait(false);
		AlertReportResult rptReault = new AlertReportResult{ FreqTypeID = reportmodel.FreqTypeID, LastRunDate = reportmodel.LastRunDate, NextRunDate = reportmodel.NextRunDate, ReportKey = reportmodel.ReportKey, StartRunDate = reportmodel.StartRunDate};
		rptReault.Result = new Dictionary<int,string>();
		rptReault.Result.Add(report.tCMSWeb_UserList.UserID, result.Message);
		email.Dispose();
		if( result.SmtpStatusCode == SmtpStatusCode.Ok)
		{
			string recpt_name;
			IEnumerable<int>userSites;
			IQueryable<EmailAlertModel> user_Alerts;
			foreach( tCMSWeb_UserList recpt in report.ReportRecipients )
			{
				if(recpt.UserID == report.UserKey)
					continue;

				Allsitedvrs = await base.UserSitesAsync(recpt.UserID);
				userSites = Allsitedvrs.Join(Sites, a => a.siteKey, b => b.siteKey, (a, b) => a.siteKey.Value).Distinct();
				TotalDVRs = Allsitedvrs.Where(it => it.KDVR.HasValue).Select(it => it.KDVR.Value).Distinct();
				user_Alerts = alerts.Where(it => userSites.Contains(it.SiteKey) && it.DVRAlerts.Any(a => TotalDVRs.Contains(a.KDVR)));
				email_format = DVRAlertFormat.Instance.EmailDataFormat(user_Alerts, LastRunDate, report.ReportName, companyname, report_user, frequency, CountDVR, countDVRConnect, total_recording_less, total_recording_great, recordingday, recordingday);

				recpt_name = string.Format("{0} {1}", recpt.UFirstName, recpt.ULastName);
				email = CMSWebApi.Email.EmailService.Create(EmailSetting);
				email.AddReceipt(recpt.UEmail, report_user);
				result = await email.SendMessageAsync(report.ReportName, email_format.ToString(), true, System.Threading.CancellationToken.None).ConfigureAwait(false);
				rptReault.Result.Add(recpt.UserID, result.Message);
			}
		}
		email.Dispose();

		IServiceBase<tCMSWebReport> tCMSWebReport_svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
		report.LastRunDate = reportmodel.NextRunDate;
		tCMSWebReport_svr.Edit(report, true);
		
		return rptReault;
	}
	

	internal IQueryable<EmailAlertModel> GetEmailingAlertData(IEnumerable<Func_DVR_Offline_Result> DVROffline, IEnumerable<AlertEventType> ReportAlertTypes, IEnumerable<tCMSWebSites> Sites, DateTime LastRundate, IEnumerable<UserSiteDvrChannel> sitedvrs, string urlImage)
	{
		IEnumerable<byte> tAlerttypes = ReportAlertTypes.Select( it => it.Id);

		DateTime Lastrun = LastRundate;

		IEnumerable<int> sitekeys = Sites.Select(it => it.siteKey);

		//IEnumerable<UserSiteDvrChannel> sitedvrs = await base.UserSitesBySiteIDsAsync( new CMSWebApi.DataServices.UsersService(base.ServiceBase), reportsetting.UserKey, sitekeys);


		Commons.TEqualityComparer<UserSiteDvrChannel> icomparer = new Commons.TEqualityComparer<UserSiteDvrChannel>((a, b) => { return a.KDVR == b.KDVR & a.siteKey == b.siteKey; });
		var Kdvrs = sitedvrs.Where(it => it.KDVR.HasValue == true && it.siteKey.HasValue).Distinct( icomparer).Select( it => new {KDVR = it.KDVR.Value, SiteKey = it.siteKey.Value});


		IQueryable<tAlertEvent> alerts = GetAlertData(tAlerttypes, Lastrun, Kdvrs.Select(it => it.KDVR), DVROffline, urlImage);

		var group_alert = alerts.GroupBy( it => it.KDVR.Value);
		var siteAlert = Kdvrs.Join(group_alert, s => s.KDVR, a => a.Key, (s, a) => new { Site = s, DVRALerts = a });
		var groupsite = siteAlert.GroupBy( it => it.Site.SiteKey);
		

		var dvr_service = new ModelDataService<tDVRAddressBook>(ServiceBase);
		var dvr_address_book = dvr_service.Gets(null, null).Join(group_alert.Select(it => it.Key).ToList(), d => d.KDVR, s => s, (d, s) => d).ToList();
		var result = groupsite.Select(it => new EmailAlertModel
		{
			SiteKey = it.Key,
			SiteName = Sites.First(s => it.Key == s.siteKey).ServerID,
			DVRAlerts = it.Select( dvralt => new EmailALertSiteDVR{
																	KDVR = dvralt.Site.KDVR,
																	ServerID = dvr_address_book.First(d => d.KDVR == dvralt.Site.KDVR).ServerID,
																	Alerts = dvralt.DVRALerts.Select(alt => new EmailAlertDVRModel{
																																	Description = alt.Description,
																																	KAlertEvent = alt.KAlertEvent,
																																	KAlertName = ReportAlertTypes.First(a => a.Id == alt.KAlertType).Name,
																																	KAlertType = alt.KAlertType,
																																	TimeZone = alt.TimeZone.Value,
																																	ImageUrl = alt.Image
																															})
																}
									)
			});
		return result.AsQueryable();
	}


	internal IQueryable<tAlertEvent> GetAlertData(IEnumerable<byte> tAlerttypes, DateTime LastRunDate, IEnumerable<int> KDVRs, IEnumerable<Func_DVR_Offline_Result> DVROffline, string urlImage)
	{
		//all alert type from setting
		//alert type was settign to apply Fixed alert rule
		IEnumerable<byte> tAlertfix = InternalBusinessService.AlertFixConfigs.Instance.AlertType;
		//get alert type that match with fixed alert
		IEnumerable<byte> tAlertLast = tAlerttypes.Intersect( tAlertfix);
		//normal alert type
		IEnumerable<byte> tALertNormal = tAlerttypes.Except(tAlertLast).Where( it => it != (byte)CMSWebApi.Utils.AlertType.DVR_is_off_line);
		IEnumerable<tAlertEvent> TotalAlert = System.Linq.Enumerable.Empty<tAlertEvent>();
		IEnumerable<EmailAlertConfig> emailAlertConfig = InternalBusinessService.AlertFixConfigs.Instance.GetAlertConfigEmail();
		if(tALertNormal.Any())
		{
			IServiceBase<tAlertEvent> tAlert_service = new CMSWebApi.DataServices.ModelDataService<tAlertEvent>(base.ServiceBase);
			IEnumerable<tAlertEvent> _talerts = tAlert_service.Gets<tAlertEvent>(it => tALertNormal.Contains(it.KAlertType) && KDVRs.Contains(it.KDVR.Value) && it.Time > LastRunDate, null, it => it).AsEnumerable();
			IEnumerable<tAlertEvent> talerts = RemoveAlertSend_1_Time( _talerts, emailAlertConfig, LastRunDate);
			var alt_image = talerts.Where( it => it.Channel.HasValue && it.Channel.Value >= 0 && (long)it.ImageTime > 0 && it.TimeZone.HasValue && ( it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Sensor_Triggered || it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Control_Activated) );
			string img_key;
			foreach( var alt in alt_image)
			{
				img_key = string.Format("{0}:{1}:{2}",alt.KDVR, alt.Channel, alt.TimeZone.Value.Ticks);
				img_key = System.Uri.EscapeUriString(Commons.Utils.String2Base64(img_key));
				alt.Image = string.Format(urlImage, img_key);
			}
			TotalAlert = talerts.AsEnumerable();
		}

		if( tAlertLast.Any())
		{
			IEnumerable<tAlertEvent> LastAlertEvent = LastAlertEvents(tAlertLast, LastRunDate,KDVRs, emailAlertConfig);
			TotalAlert = TotalAlert.Concat(LastAlertEvent);

			//IServiceBase<tAlertEventLast> tAlertLast_service = new CMSWebApi.DataServices.ModelDataService<tAlertEventLast>(base.ServiceBase);
			//IQueryable<tAlertEvent> lastalerts = tAlertLast_service.Gets<tAlertEventLast>(it => KDVRs.Contains(it.KDVR) && tAlertLast.Contains(it.KAlertType)).Select(alt => alt.tAlertEvent);
			
			//IEnumerable<int>VideoLoss = lastalerts.Where( it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).Select( alt => alt.KAlertEvent);
			//if( VideoLoss.Any())
			//{
			//	TotalAlert = TotalAlert.Concat(lastalerts.Where(it => it.KAlertType != (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).AsEnumerable());
			//	ModelServiceBase<tAlertEventDetail> altDetail = new ModelDataService<tAlertEventDetail>(ServiceBase);
			//	var VideoLossKchannel = altDetail.Gets(it => VideoLoss.Contains(it.KAlertEvent) && it.KChannel > 0, null, it => new { KChannel = it.KChannel, KAlert = it.KAlertEvent });
			//	IQueryable<tDVRChannels> tChannels = new ModelDataService<tDVRChannels>(ServiceBase).Gets(null,null);
			//	var channels = VideoLossKchannel.Join( tChannels, v=> v.KChannel, c=> c.KChannel, (v,c)=> new {KAlert = v.KAlert, KChannel = v.KChannel, Name = c.Name});
			//	var alert_channels = channels.GroupBy( it => it.KAlert);
			//	var videoloss_channel = alert_channels.Join(lastalerts, c => c.Key, a => a.KAlertEvent,(c, a)=> new { Channels = c.Select(it => it.Name), KAlert = a.KAlertEvent} ).ToList();
			//	//var videoloss_description = videoloss_channel.Select( it => new tAlertEvent{ Channel = it.Alert.Channel, CMSUser = it.Alert.CMSUser, Description = string.Join(", ", it.Channels), DVRUser = it.Alert.DVRUser, Image = it.Alert.Image, ImageTime = it.Alert.ImageTime,
			//	//												KAlertEvent = it.Alert.KAlertEvent, KAlertType = it.Alert.KAlertType, KDVR = it.Alert.KDVR, Note = it.Alert.Note, Rate = it.Alert.Rate, Status = it.Alert.Status, Time = it.Alert.Time, TimeZone = it.Alert.TimeZone });
			//	foreach (var alt in lastalerts.Where(it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss))
			//	{
			//		alt.Description = string.Join(", ", videoloss_channel.First( it => it.KAlert == alt.KAlertEvent).Channels);
			//	}
			//	TotalAlert = TotalAlert.Concat(lastalerts.Where(it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).AsEnumerable());
			//}
			//else
			//	TotalAlert = TotalAlert.Concat(lastalerts.AsEnumerable());
		}

		if( tAlerttypes.Contains( (byte)CMSWebApi.Utils.AlertType.DVR_is_off_line) && DVROffline.Any())
		{
			DateTime now = DateTime.Now;
			var dvrOfflines = DVROffline.Join(KDVRs, o => o.KDVR, d=> d, (o,d) => o);  //await DataService.GetDVR_On_Offline_Async(KDVRs, now, true, 0, DVRKeepAliveInterval);
			var offline_alert = dvrOfflines.Where( it => it.KDVR != null).Select( alt => new tAlertEvent{
			KDVR = alt.KDVR,
			KAlertType = (byte)CMSWebApi.Utils.AlertType.DVR_is_off_line,
			TimeZone = alt.Minutes != null ? now.AddMinutes(-(int)alt.Minutes) : now,
			Time = now,
			tAlertEventLast = System.Linq.Enumerable.Empty<tAlertEventLast>() as ICollection<tAlertEventLast>,
			tAlertEventDetail = System.Linq.Enumerable.Empty<tAlertEventDetail>() as ICollection<tAlertEventDetail>,
			});
			TotalAlert = TotalAlert.Union(offline_alert.AsEnumerable());
		}

		return TotalAlert.AsQueryable();
	}
	private IEnumerable<tAlertEvent> RemoveAlertSend_1_Time(IEnumerable<tAlertEvent> Alerts, IEnumerable<EmailAlertConfig> emailalertscfg, DateTime LastRunDate)
	{
		//filter last alert by setting
		var lastjoin = Alerts.LeftOuterJoin(emailalertscfg, a => a.KAlertType, cfg => cfg.AlertType, (a, c, k) => new { Alert = a, Config = c });

		IEnumerable<tAlertEvent> lastalerts = lastjoin.Where(it => it.Config == null || it.Config.Included == true || it.Alert.Time > LastRunDate).Select(a => a.Alert);

		return lastalerts.AsQueryable();
	}
	private IEnumerable<tAlertEvent> LastAlertEvents( IEnumerable<byte> LastAlertypes, DateTime LastRunDate, IEnumerable<int> KDVRs, IEnumerable<EmailAlertConfig> emailalertscfg)
	{
		IEnumerable<tAlertEvent> TotalAlert = System.Linq.Enumerable.Empty<tAlertEvent>();
		IServiceBase<tAlertEventLast> tAlertLast_service = new CMSWebApi.DataServices.ModelDataService<tAlertEventLast>(base.ServiceBase);
		IQueryable<tAlertEvent> _lastalerts = tAlertLast_service.Gets<tAlertEventLast>(it => KDVRs.Contains(it.KDVR) && LastAlertypes.Contains(it.KAlertType)).Select(alt => alt.tAlertEvent);
		//filter last alert by setting
		//var lastjoin =_lastalerts.LeftOuterJoin( emailalertscfg, a => a.KAlertType, cfg => cfg.AlertType, (a, c,k)=> new {Alert= a, Config = c});
		IEnumerable<tAlertEvent> lastalerts = RemoveAlertSend_1_Time(_lastalerts, emailalertscfg, LastRunDate);

		IEnumerable<int>VideoLoss = lastalerts.Where( it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).Select( alt => alt.KAlertEvent);
		if( VideoLoss.Any())
		{
			TotalAlert = TotalAlert.Concat(lastalerts.Where(it => it.KAlertType != (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).AsEnumerable());
			ModelServiceBase<tAlertEventDetail> altDetail = new ModelDataService<tAlertEventDetail>(ServiceBase);
			var VideoLossKchannel = altDetail.Gets(it => VideoLoss.Contains(it.KAlertEvent) && it.KChannel > 0, null, it => new { KChannel = it.KChannel, KAlert = it.KAlertEvent });
			IQueryable<tDVRChannels> tChannels = new ModelDataService<tDVRChannels>(ServiceBase).Gets(null,null);
			var channels = VideoLossKchannel.Join( tChannels, v=> v.KChannel, c=> c.KChannel, (v,c)=> new {KAlert = v.KAlert, KChannel = v.KChannel, Name = c.Name});
			var alert_channels = channels.GroupBy( it => it.KAlert).AsEnumerable();
			var videoloss_channel = alert_channels.Join(lastalerts.AsEnumerable(), c => c.Key, a => a.KAlertEvent,(c, a)=> new { Channels = c.Select(it => it.Name), KAlert = a.KAlertEvent} ).ToList();
				//var videoloss_description = videoloss_channel.Select( it => new tAlertEvent{ Channel = it.Alert.Channel, CMSUser = it.Alert.CMSUser, Description = string.Join(", ", it.Channels), DVRUser = it.Alert.DVRUser, Image = it.Alert.Image, ImageTime = it.Alert.ImageTime,
				//												KAlertEvent = it.Alert.KAlertEvent, KAlertType = it.Alert.KAlertType, KDVR = it.Alert.KDVR, Note = it.Alert.Note, Rate = it.Alert.Rate, Status = it.Alert.Status, Time = it.Alert.Time, TimeZone = it.Alert.TimeZone });
			foreach (var alt in lastalerts.Where(it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss))
			{
				alt.Description = string.Join(", ", videoloss_channel.First( it => it.KAlert == alt.KAlertEvent).Channels);
			}
			TotalAlert = TotalAlert.Concat(lastalerts.Where(it => it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss).AsEnumerable());
		}
		else
			TotalAlert = TotalAlert.Concat(lastalerts.AsEnumerable());
		return TotalAlert;
	}

	public EmailSettingModel GetReportByID(UserContext user,int ReportId)
	{
		IEnumerable<string> includes = ServiceBase.ChildProperties(typeof(tCMSWebReport));
		IServiceBase<tCMSWebReport> svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
		tCMSWebReport mod = GetReportbyID(ReportId);//svr.Get<tCMSWebReport>(filter => filter.ReportKey == ReportId && filter.UserKey == user.ID , Includes, selector => selector);
		if(mod == null || mod.UserKey != user.ID)
			return null;

		EmailSettingModel result = new EmailSettingModel
							{
								ReportKey = mod.ReportKey,
								BAMReportXML = mod.BAMReportXML,
								EmailSubject = mod.EmailSubject,
								EnableEmailReporting = mod.EnableEmailReporting.Value,
								FreqCount = mod.FreqCount.Value,
								FreqTypeID = mod.FreqTypeID.Value,
								LastRunDate = mod.LastRunDate.Value,
								NextRunDate = mod.NextRunDate.Value,
								Recipients = mod.ReportRecipients.Select(item => item.UserID).ToList(),
								ReportName = mod.ReportName,
								ReportType = mod.ReportType.Value,
								RunTime = mod.RunTime,
								Alerts = mod.tAlertType.Select(item=> new AlertEventType(){
									Id = item.KAlertType,
									Name = item.AlertType,
									KAlertSeverity = item.KAlertSeverity,
									CmsWebType = item.CMSWebType,
									CmsWebGroup = item.CMSWebGroup
								}).ToList(),
								SiteList = mod.tCMSWebSites.Select(item => item.siteKey).ToList(),
								StartRunDate = mod.StartRunDate.Value,
								UserKey = mod.tCMSWeb_UserList.UserID,
								EmailList = mod.ReportRecipients.Select(item => item.UEmail).ToList()
							};
		return result;
	}

	public List<EmailSettingModel> GetReportByUser(UserContext user)
	{
		IQueryable<tCMSWebReport> data = GetReportByUser(user.ID);
		if(data == null || !data.Any())
			return new List<EmailSettingModel>();

		List<EmailSettingModel> rs =
		data.Select(mod =>  new EmailSettingModel
							{
								ReportKey = mod.ReportKey,
								BAMReportXML = mod.BAMReportXML,
								EmailSubject = mod.EmailSubject,
								EnableEmailReporting = mod.EnableEmailReporting.Value,
								FreqCount = mod.FreqCount.Value,
								FreqTypeID = mod.FreqTypeID.Value,
								LastRunDate = mod.LastRunDate.Value,
								NextRunDate = mod.NextRunDate.Value,
								Recipients = mod.ReportRecipients.Select(item => item.UserID).ToList(),
								ReportName = mod.ReportName,
								ReportType = mod.ReportType.Value,
								RunTime = mod.RunTime,
								Alerts = mod.tAlertType.Select(item => new AlertEventType()
								{
									Id = item.KAlertType,
									Name = item.AlertType,
									KAlertSeverity = item.KAlertSeverity,
									CmsWebType = item.CMSWebType,
									CmsWebGroup = item.CMSWebGroup
								}).ToList(),
								SiteList = mod.tCMSWebSites.Select(item => item.siteKey).ToList(),
								StartRunDate = mod.StartRunDate.Value,
								UserKey = mod.tCMSWeb_UserList.UserID,
								EmailList = mod.ReportRecipients.Select(item => item.UEmail).ToList()
							}).ToList();
		if(rs.Any())
		{
			return rs.ToList(); 
		}
		return new List<EmailSettingModel>();

	}

	public bool SaveEmailSetting(UserContext user,EmailSettingModel mod)
	{
			IServiceBase<tCMSWebReport> svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
			IServiceBase<tCMSWeb_UserList> usr_svr = new ModelDataService<tCMSWeb_UserList>(base.ServiceBase);
			IServiceBase<tCMSWebSites> site_svr = new ModelDataService<tCMSWebSites>(base.ServiceBase);
			IServiceBase<tAlertType> alert = new ModelDataService<tAlertType>(base.ServiceBase);
			tCMSWebReport report = new tCMSWebReport();

			try
			{
				List<int> Ai = mod.Alerts.Select(i => (int)i.Id).ToList();
				if(!mod.Recipients.Contains(user.ID))
					mod.Recipients.Add(user.ID);
				if (mod.ReportKey > 0)
				{
                        IEnumerable<string> Includes = ServiceBase.ChildProperties(typeof(tCMSWebReport));
						report = svr.Get<tCMSWebReport>(filter => filter.ReportKey == mod.ReportKey,Includes.ToArray(),selector=>selector);
						report.ReportKey = mod.ReportKey;
						report.BAMReportXML = mod.BAMReportXML;
						report.EmailSubject = mod.EmailSubject;
						report.EnableEmailReporting = mod.EnableEmailReporting;
						report.FreqCount = mod.FreqCount;
						report.FreqTypeID = mod.FreqTypeID;
                        report.LastRunDate = mod.FreqTypeID == 1? mod.StartRunDate.AddHours(-1) :  mod.StartRunDate.AddDays(-1);
						report.NextRunDate = mod.StartRunDate;
						report.ReportName = mod.ReportName;
						report.ReportType = mod.ReportType;
						report.RunTime = mod.RunTime;
						report.StartRunDate = mod.StartRunDate;
						report.UserKey = user.ID;
						report.tAlertType = alert.Gets<tAlertType>(filter => Ai.Contains(filter.KAlertType), null, selector => selector).ToList();
						report.tCMSWeb_UserList = usr_svr.Get<tCMSWeb_UserList>(filter => filter.UserID == mod.UserKey, null, selector => selector);
						report.ReportRecipients = usr_svr.Gets<tCMSWeb_UserList>(filter => mod.Recipients.Contains(filter.UserID), null, selector => selector).ToList();
						report.tCMSWebSites = site_svr.Gets<tCMSWebSites>(filter => mod.SiteList.Contains(filter.siteKey), null, selector => selector).ToList();
						report = svr.Edit(report, true);
				}
				else
				{
					report = new tCMSWebReport()
					{
						ReportKey = mod.ReportKey,
						BAMReportXML = mod.BAMReportXML,
						EmailSubject = mod.EmailSubject,
						EnableEmailReporting = mod.EnableEmailReporting,
						FreqCount = mod.FreqCount,
						FreqTypeID = mod.FreqTypeID,
                        LastRunDate =  mod.FreqTypeID == 1? mod.StartRunDate.AddHours(-1) :  mod.StartRunDate.AddDays(-1),
                        NextRunDate = mod.StartRunDate,
						ReportName = mod.ReportName,
						ReportType = mod.ReportType,
						RunTime = mod.RunTime,
						StartRunDate = mod.StartRunDate,
						UserKey = user.ID,
						tAlertType = alert.Gets<tAlertType>(filter => Ai.Contains(filter.KAlertType), null, selector => selector).ToList(),
						tCMSWeb_UserList = usr_svr.Get<tCMSWeb_UserList>(filter => filter.UserID == mod.UserKey, null, selector => selector),
						ReportRecipients = usr_svr.Gets<tCMSWeb_UserList>(filter => mod.Recipients.Contains(filter.UserID), null, selector => selector).ToList(),
						tCMSWebSites = site_svr.Gets<tCMSWebSites>(filter => mod.SiteList.Contains(filter.siteKey), null, selector => selector).ToList(),
					};
					report = svr.Add(report, true);
				}
				if (report.ReportKey > 0)
					return true;
				else
					return false;
			}
			catch (Exception e)
			{
				return false;
			}

	}

	public bool DeleteEmailSetting(UserContext user, int ReportKey)
	{

		IServiceBase<tCMSWebReport> svr = new ModelDataService<tCMSWebReport>(base.ServiceBase);
        IEnumerable<string> Includes = ServiceBase.ChildProperties(typeof(tCMSWebReport));
		tCMSWebReport report = svr.Get<tCMSWebReport>(filter => filter.ReportKey == ReportKey && filter.UserKey == user.ID, Includes.ToArray(), selector => selector);
		return svr.Delete(report, true);
 
	}

}
}
