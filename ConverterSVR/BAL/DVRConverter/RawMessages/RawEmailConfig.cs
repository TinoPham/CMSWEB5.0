using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;
using System.Data.Entity;

namespace ConverterSVR.BAL.DVRConverter
{
	[Serializable()]
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawEmailConfig : RawDVRConfig<RawEmailBody>
	{
		#region Global Parameter
		public const string STR_Email = "email";
		public const string STR_Enable = "enable";
		public const string STR_VideoLoss = "video_loss";
		public const string STR_Threshold = "threshold";
		public const string STR_TransDelay = "transmission_delay";
		public const string STR_VLossDelay = "videoloss_dwelltime";
		public const string STR_FromAddress = "from_address";
		public const string STR_FromName = "from_name";
		public const string STR_SMTP = "smtp";
		public const string STR_ip = "ip";
		public const string STR_Port = "port";
		public const string STR_LoginMethod = "login_method";
		public const string STR_UserName = "user_name";
		public const string STR_Password = "password";
		public const string STR_ServerStatus = "server_status";
		public const string STR_SendEmail = "send_email";
		public const string STR_ChannelImage = "channel_image";
		public const string STR_SendEmailEvery = "send_email_every";
		public const string STR_EmailTitle = "email_title";
		public const string STR_Sensors = "sensors";
		public const string STR_Sensor = "sensor";
		public const string STR_ToAddress = "to_address";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawEmailBody msgBody { get; set; }
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);

		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.EmailData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateEmailConfigs(DVRAdressBook, msgBody.EmailData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_EMAIL, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_EMAIL, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateEmailConfigs(tDVRAddressBook dvrAdressBook, EmailData emailData)
		{
			bool ret = false;
			int useRealIdx = 1;
			var maxRealIdx = db.Query<tDVRSensors>(item => item.KDVR == DVRAdressBook.KDVR).Max(s => s.RealIndex);
			if (maxRealIdx == null || maxRealIdx <= 0)
			{
				useRealIdx = 0;
			}

			if (emailData.SMTP != null)
			{
				if (dvrAdressBook.KDVRVersion >= VersionForBandwidth)
				{

					var emailMethod = db.Query<tSMTPLoginMethod>(item => item.MethodName.Trim().Equals(emailData.SMTP.LoginMethod.Trim())).FirstOrDefault();

					if (emailMethod != null)
						msgBody.EmailData.SMTP.EmailLoginMethod = emailMethod.KMethod;
					else
						msgBody.EmailData.SMTP.EmailLoginMethod = 0;
				}
				else
				{
					msgBody.EmailData.SMTP.EmailLoginMethod = emailData.SMTP.LoginMethod == null ? (short)0 : Convert.ToInt16(emailData.SMTP.LoginMethod);
				}
			}

			ret |=	UpdateEmailInfo(DVRAdressBook, msgBody.EmailData);

			List<tDVRSensors> sensorList = db.Query<tDVRSensors>(s => s.KDVR == dvrAdressBook.KDVR).ToList();

			if (emailData.Sensors != null)
			{
				if (useRealIdx > 0)
				{
					foreach (EmailSensor eSen in emailData.Sensors)
					{
						tDVRSensors sensor = sensorList.FirstOrDefault(s => s.RealIndex == eSen.id);
						if (sensor == null) continue;
						if (sensor.ActiveEmail != eSen.Enable)
						{
							sensor.ActiveEmail = eSen.Enable;
							db.Update<tDVRSensors>(sensor);
							ret = true;
						}
					}
				}
				else
				{
					foreach (EmailSensor eSen in emailData.Sensors)
					{
						tDVRSensors sensor = sensorList.FirstOrDefault(s => s.SensorNo == eSen.id);
						if (sensor == null) continue;
						if (sensor.ActiveEmail != eSen.Enable)
						{
							sensor.ActiveEmail = eSen.Enable;
							db.Update<tDVRSensors>(sensor);
							ret = true;
						}
					}
				}
			}
			else
			{
				foreach (var ss in sensorList)
				{
					ss.ActiveEmail = 0;
					db.Update<tDVRSensors>(ss);
					ret = true;
				}
			}
			return ret;
		}

		private bool UpdateEmailInfo(tDVRAddressBook dvrAdressBook, EmailData emailData)
		{
			bool ret = false;
			tDVREmail email = db.FirstOrDefault<tDVREmail>(item => item.KDVR == dvrAdressBook.KDVR);
			if (email == null) //insert new
			{
				email = new tDVREmail(){KDVR =  dvrAdressBook.KDVR} ;
				emailData.SetEntity(ref email);
				db.Insert<tDVREmail>(email);
				ret = true;
			}
			else
			{
				if (emailData.Equal(email)) return false;
				emailData.SetEntity(ref email);
				db.Update<tDVREmail>(email);
				ret = true;
			}
			return ret;
		}


		#region Unused

		//private bool IsSensorData()
		//{
		//	bool result = false;
		//	if (msgBody.EmailData == null) return false;
		//	if (msgBody.EmailData != null && msgBody.EmailData.Sensors != null) result = true;
		//	return result;
		//}

		//public async Task<Commons.ERROR_CODE> UpdateToDB1()
		//{
		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	int useRealIdx = 1;
		//	int? maxRealIdx = db.Query<tDVRSensor>(item => item.KDVR == DVRAdressBook.KDVR).Max(s => s.RealIndex);
		//	if (maxRealIdx == null || maxRealIdx <= 0)
		//	{
		//		useRealIdx = 0;
		//	}

		//	SetDvrEmail();

		//	List<tDVRSensor> sensorList = db.Query<tDVRSensor>(s => s.KDVR == DVRAdressBook.KDVR).ToList();

		//	if (IsSensorData())
		//	{
		//		if (useRealIdx > 0)
		//		{
		//			SetEmailSensorRealIndex(sensorList);
		//		}
		//		else
		//		{
		//			SetEmailSensor(sensorList);
		//		}
		//	}
		//	else
		//	{
		//		foreach (var ss in sensorList)
		//		{
		//			ss.ActiveEmail = 0;
		//			db.Update<tDVRSensor>(ss);
		//		}
		//	}
		//	return await Task.FromResult<Commons.ERROR_CODE>(db.Save() == -1? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
		//}


		//private void SetEmailSensor(List<tDVRSensor> sensorList)
		//{
		//	foreach (EmailSensor eSen in msgBody.EmailData.Sensors)
		//	{
		//		tDVRSensor sensor = sensorList.FirstOrDefault(s => s.SensorNo == eSen.id);
		//		if (sensor == null) continue;
		//		if (sensor.ActiveEmail != eSen.Enable)
		//		{
		//			sensor.ActiveEmail = eSen.Enable;
		//			db.Update<tDVRSensor>(sensor);
		//		}
		//	}
		//}

		//private void SetEmailSensorRealIndex(List<tDVRSensor> sensorList)
		//{
		//	foreach (EmailSensor eSen in msgBody.EmailData.Sensors)
		//	{
		//		tDVRSensor sensor = sensorList.FirstOrDefault(s => s.RealIndex == eSen.id);
		//		if (sensor == null) continue;
		//		if (sensor.ActiveEmail != eSen.Enable)
		//		{
		//			sensor.ActiveEmail = eSen.Enable;
		//			db.Update<tDVRSensor>(sensor);
		//		}
		//	}
		//}

		//private void SetDvrEmail()
		//{
		//	tDVREmail email = db.FirstOrDefault<tDVREmail>(item => item.KDVR == DVRAdressBook.KDVR);
		//	if (email == null) //insert new
		//	{
		//		email = new tDVREmail();
		//		SetEmailInfo(DVRAdressBook.KDVR, ref email);
		//		db.Insert<tDVREmail>(email);
		//	}
		//	else
		//	{
		//		if (CompareEmailInfo(DVRAdressBook.KDVR, email)) return;
		//		SetEmailInfo(DVRAdressBook.KDVR, ref email);
		//		db.Update<tDVREmail>(email);
		//	}
		//}

		//private void SetEmailInfo(Int32 kDvr, ref tDVREmail email)
		//{
		//	email.KDVR = kDvr;
		//	email.Enable = msgBody.EmailData.Enable != 0;
		//	email.EnableVLoss = msgBody.EmailData.VideoLoss != 0;
		//	email.Threshold = msgBody.EmailData.Threshold;
		//	email.TransmissionDelay = msgBody.EmailData.TransDelay;
		//	email.VideoLossDwelltime = msgBody.EmailData.VLossDelay;
		//	email.ToAddress = msgBody.EmailData.ToAddress;
		//	email.FromAddress = msgBody.EmailData.FromAddress;
		//	email.FromName = msgBody.EmailData.FromName;

		//	if (msgBody.EmailData.SMTP != null)
		//	{
		//		email.SMTPIP = msgBody.EmailData.SMTP.IP;
		//		email.SMTPPort = msgBody.EmailData.SMTP.Port;
		//		email.KSMTPLoginMethod = msgBody.EmailData.SMTP.LoginMethod;
		//		email.SMTPUserName = msgBody.EmailData.SMTP.UserName;
		//		email.SMTPPassword = msgBody.EmailData.SMTP.Password;
		//	}
		//	if (msgBody.EmailData.ServerStatus != null)
		//	{
		//		email.ServerStatusSendEmail = msgBody.EmailData.ServerStatus.SendEmail != 0;
		//		email.ServerStatusChannelImage = msgBody.EmailData.ServerStatus.ChannelImage;
		//		email.ServerStatusSendEmailEvery = msgBody.EmailData.ServerStatus.SendEmailEvery;
		//		email.ServerStatusEmailTitle = msgBody.EmailData.ServerStatus.EmailTitle;
		//	}
		//}

		//private bool CompareEmailInfo(Int32 kDvr, tDVREmail email)
		//{
		//	bool result = email.KDVR == kDvr &&
		//				  email.Enable == (msgBody.EmailData.Enable != 0) &&
		//				  email.EnableVLoss == (msgBody.EmailData.VideoLoss != 0) &&
		//				  email.Threshold == msgBody.EmailData.Threshold &&
		//				  email.TransmissionDelay == msgBody.EmailData.TransDelay &&
		//				  email.VideoLossDwelltime == msgBody.EmailData.VLossDelay &&
		//				  email.ToAddress == msgBody.EmailData.ToAddress &&
		//				  email.FromAddress == msgBody.EmailData.FromAddress &&
		//				  email.FromName == msgBody.EmailData.FromName;

		//	if (msgBody.EmailData.SMTP != null)
		//	{
		//		result = result &&
		//				 email.SMTPIP == msgBody.EmailData.SMTP.IP &&
		//				 email.SMTPPort == msgBody.EmailData.SMTP.Port &&
		//				 email.KSMTPLoginMethod == msgBody.EmailData.SMTP.LoginMethod &&
		//				 email.SMTPUserName == msgBody.EmailData.SMTP.UserName &&
		//				 email.SMTPPassword == msgBody.EmailData.SMTP.Password;
		//	}
		//	if (msgBody.EmailData.ServerStatus != null)
		//	{
		//		result = result &&
		//				 email.ServerStatusSendEmail == (msgBody.EmailData.ServerStatus.SendEmail != 0) &&
		//				 email.ServerStatusChannelImage == msgBody.EmailData.ServerStatus.ChannelImage &&
		//				 email.ServerStatusSendEmailEvery == msgBody.EmailData.ServerStatus.SendEmailEvery &&
		//				 email.ServerStatusEmailTitle == msgBody.EmailData.ServerStatus.EmailTitle;
		//	}
		//	return result;
		//}

		#endregion

	}
	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawEmailBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public string DVRTime { get; set; }
		public DateTime dtDVRTime
		{
			get
			{
				return DateTime.ParseExact(DVRTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawEmailConfig.STR_Email)]
		public EmailData EmailData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawEmailConfig.STR_Email)]
	public class EmailData : IMessageEntity<tDVREmail>
	{
		[XmlElement(RawEmailConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawEmailConfig.STR_VideoLoss)]
		public Int32 VideoLoss { get; set; }

		[XmlElement(RawEmailConfig.STR_Threshold)]
		public Int32 Threshold { get; set; }

		[XmlElement(RawEmailConfig.STR_TransDelay)]
		public Int32 TransDelay { get; set; }

		[XmlElement(RawEmailConfig.STR_VLossDelay)]
		public Int32 VLossDelay { get; set; }

		[XmlElement(RawEmailConfig.STR_ToAddress, IsNullable = true)]
		public string ToAddress { get; set; }

		[XmlElement(RawEmailConfig.STR_FromAddress)]
		public string FromAddress { get; set; }

		[XmlElement(RawEmailConfig.STR_FromName)]
		public string FromName { get; set; }

		[XmlElement(RawEmailConfig.STR_SMTP)]
		public SMTPInfo SMTP { get; set; }

		[XmlElement(RawEmailConfig.STR_ServerStatus)]
		public ServerStatusInfo ServerStatus { get; set; }

		[XmlArray(RawEmailConfig.STR_Sensors)]
		[XmlArrayItem(RawEmailConfig.STR_Sensor)]
		public List<EmailSensor> Sensors = new List<EmailSensor>();

		public bool Equal(tDVREmail value)
		{
			bool result =
					  value.Enable == (Enable != 0) &&
					  value.EnableVLoss == (VideoLoss != 0) &&
					  value.Threshold == Threshold &&
					  value.TransmissionDelay == TransDelay &&
					  value.VideoLossDwelltime == VLossDelay &&
					  value.ToAddress == ToAddress &&
					  value.FromAddress == FromAddress &&
					  value.FromName == FromName;

			if (SMTP != null)
			{
				result = result &&
						 value.SMTPIP == SMTP.IP &&
						 value.SMTPPort == SMTP.Port &&
						 value.KSMTPLoginMethod == SMTP.EmailLoginMethod &&
						 value.SMTPUserName == SMTP.UserName &&
						 value.SMTPPassword == SMTP.Password;
			}
			if (ServerStatus != null)
			{
				result = result &&
						 value.ServerStatusSendEmail == (ServerStatus.SendEmail != 0) &&
						 value.ServerStatusChannelImage == ServerStatus.ChannelImage &&
						 value.ServerStatusSendEmailEvery == ServerStatus.SendEmailEvery &&
						 value.ServerStatusEmailTitle == ServerStatus.EmailTitle;
			}
			return result;
		}

		public void SetEntity(ref tDVREmail value)
		{
			if (value == null)
				value = new tDVREmail();
			value.Enable = Enable != 0;
			value.EnableVLoss = VideoLoss != 0;
			value.Threshold = Threshold;
			value.TransmissionDelay = TransDelay;
			value.VideoLossDwelltime = VLossDelay;
			value.ToAddress = ToAddress;
			value.FromAddress = FromAddress;
			value.FromName = FromName;

			if (SMTP != null)
			{
				value.SMTPIP = SMTP.IP;
				value.SMTPPort = SMTP.Port;
				value.KSMTPLoginMethod = SMTP.EmailLoginMethod;
				value.SMTPUserName = SMTP.UserName;
				value.SMTPPassword = SMTP.Password;
			}
			if (ServerStatus != null)
			{
				value.ServerStatusSendEmail = ServerStatus.SendEmail != 0;
				value.ServerStatusChannelImage = ServerStatus.ChannelImage;
				value.ServerStatusSendEmailEvery = ServerStatus.SendEmailEvery;
				value.ServerStatusEmailTitle = ServerStatus.EmailTitle;
			}
		}
	}

	[XmlRoot(RawEmailConfig.STR_SMTP)]
	public class SMTPInfo
	{
		[XmlElement(RawEmailConfig.STR_ip)]
		public string IP { get; set; }

		[XmlElement(RawEmailConfig.STR_Port)]
		public Int32 Port { get; set; }

		[XmlElement(RawEmailConfig.STR_LoginMethod)]
		public string LoginMethod { get; set; }

		[XmlElement(RawEmailConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawEmailConfig.STR_Password)]
		public string Password { get; set; }
		[XmlIgnore]
		public short EmailLoginMethod { get; set; }
	}

	[XmlRoot(RawEmailConfig.STR_ServerStatus)]
	public class ServerStatusInfo
	{
		[XmlElement(RawEmailConfig.STR_SendEmail)]
		public Int32 SendEmail { get; set; }

		[XmlElement(RawEmailConfig.STR_ChannelImage)]
		public Int32 ChannelImage { get; set; }

		[XmlElement(RawEmailConfig.STR_SendEmailEvery)]
		public Int32 SendEmailEvery { get; set; }

		[XmlElement(RawEmailConfig.STR_EmailTitle)]
		public string EmailTitle { get; set; }
	}

	[XmlRoot(RawEmailConfig.STR_Sensor)]
	public class EmailSensor
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawEmailConfig.STR_Enable)]
		public Int32 Enable { get; set; }
	}
	#endregion
}
