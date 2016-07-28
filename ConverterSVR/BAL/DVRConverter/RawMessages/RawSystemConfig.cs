using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawSystemConfig : RawDVRConfig<RawSystemBody>
	{
		#region Parameter
		public const string STR_SystemInfo = "system_info";
		public const string STR_DateFormat = "date_format";
		public const string STR_TimeFormat = "time_format";
		public const string STR_VLossAlarm = "video_loss_alarm";
		public const string STR_EnControl = "enable_control";
		public const string STR_EnAlarm = "enable_alarm";
		public const string STR_Control = "control";
		//public const string STR_Hour = "hour";
		//public const string STR_Minute = "minute";
		//public const string STR_Second = "second";
		public const string STR_AutoLogon = "auto_logon";
		public const string STR_UserName = "user_name";
		public const string STR_Password = "password";
		public const string STR_WatermarkEnable = "watermark_enable";
		public const string STR_WatermarkDisplay = "watermark_display";
		public const string STR_BaudratePtzwd = "baudrate_ptzwd";
		public const string STR_PortPtzwdName = "port_ptzwd_name";
		public const string STR_RestartSchedule = "restart_schedule";
		public const string STR_Monday = "monday";
		public const string STR_Tuesday = "tuesday";
		public const string STR_Wednesday = "wednesday";
		public const string STR_Thursday = "thursday";
		public const string STR_Friday = "friday";
		public const string STR_Saturday = "saturday";
		public const string STR_Sunday = "sunday";
		public const string STR_Enable = "enable";
		//public const string STR_Hour = "hour";
		//public const string STR_Minute = "minute";
		public const string STR_RSMode = "rs_mode";
		public const string STR_ActiveteSensor = "activate_sensor";
		public const string STR_WaitTime = "wait_time";
		public const string STR_Sensors = "sensors";
		public const string STR_Sensor = "sensor";
		public const string STR_Enabled = "enabled";
		public const string STR_Used = "used";
		public const string STR_SynchTime = "synch_time";
		public const string STR_ExportType = "export_type";
		public const string STR_DestName = "dest_name";
		public const string STR_SendToEmail = "sendto_email";
		public const string STR_NumberCopies = "number_copies";
		public const string STR_VideoFormat = "video_format";
		public const string STR_ChannelMask = "channel_mask";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawSystemBody msgBody { get; set; }
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;

			db.Include<tDVRAddressBook,tDVRChannels>(DVRAdressBook,t=>t.tDVRChannels);
			db.Include<tDVRAddressBook, tDVRRestartTime>(DVRAdressBook, t => t.tDVRRestartTimes);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateSystemConfig(DVRAdressBook))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_SYSTEM_INFO, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_SYSTEM_INFO, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateSystemConfig(tDVRAddressBook dvrAdressBook)
		{
			if (msgBody.sysData == null)
			{
				return false;
			}

			bool ret = false;
			tDVRRS232Ports rsPort = InsertRs232Port(ref ret);
			
			int controlNo = GetControlNo();

			ret |= SetSystemInfos(rsPort, controlNo, msgBody.sysData);

			ret |= SetTimeAttribute();

			if (msgBody.sysData!= null && msgBody.sysData.actSensors != null && msgBody.sysData.actSensors.Sensors != null)
				ret |= SetActiveSensors(DVRAdressBook.KDVR, msgBody.sysData.actSensors.Sensors);

			if (msgBody != null && msgBody.sysData != null && msgBody.sysData.restartSchedules!= null)
				ret |= SetRestartTime(DVRAdressBook.KDVR, msgBody.sysData.restartSchedules.RestartTimes());

			return ret;
		}

		private bool SetTimeAttribute()
		{
			bool ret = false;
			tDVRTimeAttributes timeAtt = db.FirstOrDefault<tDVRTimeAttributes>(item => item.KDVR == DVRAdressBook.KDVR);
			if (timeAtt == null)
			{
				timeAtt = new tDVRTimeAttributes
				{
					KDVR = DVRAdressBook.KDVR,
					KDateFormat = msgBody.sysData.DateFormat,
					KTimeFormat = msgBody.sysData.TimeFormat
				};
				db.Insert<tDVRTimeAttributes>(timeAtt);
				ret = true;
			}
			else
			{
				if (timeAtt.KDateFormat != msgBody.sysData.DateFormat || timeAtt.KTimeFormat != msgBody.sysData.TimeFormat)
				{
					timeAtt.KDateFormat = msgBody.sysData.DateFormat;
					timeAtt.KTimeFormat = msgBody.sysData.TimeFormat;
					db.Update<tDVRTimeAttributes>(timeAtt);
					ret = true;
				}
			}
			return ret;
		}

		private bool SetSystemInfos(tDVRRS232Ports rsPort, int controlNo, SystemData sysData)
		{
			bool ret = false;
			tDVRSystemInfo sysInfo = db.FirstOrDefault<tDVRSystemInfo>(item => item.KDVR == DVRAdressBook.KDVR);
			if (sysInfo == null)
			{
				sysInfo = new tDVRSystemInfo(){KDVR = rsPort.KDVR, KPort = rsPort.KPort, ControlVLoss = controlNo};
				sysData.SetEntity(ref sysInfo);
				db.Insert<tDVRSystemInfo>(sysInfo);
				ret = true;
			}
			else
			{
				if (sysData.Equal(sysInfo) && sysInfo.KPort == rsPort.KPort && sysInfo.ControlVLoss == controlNo)
				{
					ret = false;
				}
				else
				{
					sysInfo.KPort = rsPort.KPort;
					sysInfo.ControlVLoss = controlNo;
					sysData.SetEntity(ref sysInfo);
					db.Update<tDVRSystemInfo>(sysInfo);
					ret = true;
				}
			}
			return ret;
		}

		private tDVRRS232Ports InsertRs232Port(ref bool checkSave)
		{
			string sPortName = msgBody.sysData.PortPtzwdName;
			tDVRRS232Ports rsPort =	db.FirstOrDefault<tDVRRS232Ports>( x => x.KDVR == DVRAdressBook.KDVR && String.Compare(x.PortName, sPortName, true) == 0);
			if (rsPort == null)
			{
				rsPort = new tDVRRS232Ports {KDVR = DVRAdressBook.KDVR, PortName = sPortName};
				db.Insert<tDVRRS232Ports>(rsPort);
				checkSave = true;
			}
			return rsPort;
		}

		private int GetControlNo()
		{
			if (msgBody.sysData.VLossAlarm == null) return 0;
			int controlNo = msgBody.sysData.VLossAlarm.Control - 1;
			tDVRControl conInfo = db.FirstOrDefault<tDVRControl>(x => x.KDVR == DVRAdressBook.KDVR && x.ControlNo == controlNo);
			controlNo = conInfo == null ? 0 : conInfo.KControl;
			return controlNo;
		}

		private bool SetActiveSensors(Int32 kDvr, List<ActSensor> actInfo)
		{
			bool ret = false;
			int useRealIdx = 1;
			int? maxRealIdx = db.Query<tDVRSensors>(item => item.KDVR == kDvr).Max(s => s.RealIndex);
			if (maxRealIdx == null || maxRealIdx <= 0)
			{
				useRealIdx = 0;
			}
			List<tDVRSensors> lstDvrSensor = db.Query<tDVRSensors>(x => x.KDVR == kDvr).ToList();

			foreach (var ss in actInfo)
			{
				tDVRSensors senInfo = null;
				senInfo = useRealIdx > 0 ? lstDvrSensor.FirstOrDefault(x => x.KDVR == kDvr && x.RealIndex == ss.ID) :
					lstDvrSensor.FirstOrDefault(x => x.KDVR == kDvr && x.SensorNo == ss.ID);
				if (senInfo == null) continue;
				if (senInfo.ActivePanicBackup == ss.Enabled) continue;
				senInfo.ActivePanicBackup = ss.Enabled;
				db.Update<tDVRSensors>(senInfo);
				ret = true;
			}
			return ret;
		}


		private bool SetRestartTime(Int32 kDvr, List<RestartScheduleInfo> lsResScheds)
		{
			bool ret = false;
			List<tDVRRestartTime> lsResTimes = db.Query<tDVRRestartTime>(x => x.KDVR == kDvr).ToList();
			
			foreach (var rs in lsResTimes)
			{
				tDVRRestartTime rsTime = rs;
				RestartScheduleInfo rsInfo = lsResScheds.FirstOrDefault(t => t.ID == rs.KDay);
				if (rsInfo != null)
				{
					if (!rsInfo.Equal(rsTime))
					{
						rsInfo.SetEntity(ref rsTime);
						db.Update<tDVRRestartTime>(rsTime);
						ret = true;
					}
					lsResScheds.Remove(rsInfo);
				}
				else
				{
					db.Delete<tDVRRestartTime>(rsTime);
					ret = true;
				}
			}

			foreach (var rs in lsResScheds)
			{
				var resTime = new tDVRRestartTime(){KDVR =  kDvr};
				rs.SetEntity(ref resTime);
				db.Insert<tDVRRestartTime>(resTime);
				ret = true;
			}
			return ret;
		}

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawSystemBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawSystemConfig.STR_SystemInfo)]
		public SystemData sysData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawSystemConfig.STR_SystemInfo)]
	public class SystemData : IMessageEntity<tDVRSystemInfo>
	{
		[XmlElement(RawSystemConfig.STR_DateFormat)]
		public Int16 DateFormat { get; set; }

		[XmlElement(RawSystemConfig.STR_TimeFormat)]
		public Int16 TimeFormat { get; set; }

		[XmlElement(RawSystemConfig.STR_VLossAlarm)]
		public VLossAlarmInfo VLossAlarm { get; set; }

		[XmlElement(RawSystemConfig.STR_RestartSchedule)]
		public RestartSchedules restartSchedules { get; set; }

		[XmlElement(RawSystemConfig.STR_AutoLogon)]
		public Int32 AutoLogon { get; set; }

		[XmlElement(RawSystemConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawSystemConfig.STR_Password)]
		public string Password { get; set; }

		[XmlElement(RawSystemConfig.STR_WatermarkEnable)]
		public Int32 WatermarkEnable { get; set; }

		[XmlElement(RawSystemConfig.STR_WatermarkDisplay)]
		public Int32 WatermarkDisplay { get; set; }

		[XmlElement(RawSystemConfig.STR_BaudratePtzwd)]
		public Int32 BaudratePtzwd { get; set; }

		[XmlElement(RawSystemConfig.STR_PortPtzwdName)]
		public string PortPtzwdName { get; set; }

		[XmlElement(RawSystemConfig.STR_ActiveteSensor)]
		public ActivateSensorInfo actSensors { get; set; }

		[XmlElement(RawSystemConfig.STR_SynchTime)]
		public SynchTimeInfo syncTime { get; set; }

		public bool Equal(tDVRSystemInfo value)
		{
			bool ret = value.AutoLogonOS == AutoLogon;
			ret &= value.OSUserName == UserName;
			ret &= value.OSPassword == Password;
			ret &= value.EnWatermark == WatermarkEnable;
			ret &= value.DisplayWatermark == WatermarkDisplay;
			ret &= value.BaudratePTZ == BaudratePtzwd;
			//ret |= value.KPort == kPort;
			if (syncTime != null)
			{
				ret &= value.EnSyncNTP == syncTime.Enable;
				ret &= value.SyncNTPHour == syncTime.Hour;
				ret &= value.SyncNTPMinute == syncTime.Minute;
			}
			
			if (VLossAlarm != null)
			{
				ret &= value.EnControlVLoss == VLossAlarm.EnControl;
				ret &= value.EnAlarmVLoss == VLossAlarm.EnAlarm;
				ret &= value.AlarmHourVLoss == VLossAlarm.Hour;
				ret &= value.AlarmMinVLoss == VLossAlarm.Minute;
				ret &= value.AlarmSecVLoss == VLossAlarm.Second;
			}

			if (actSensors != null)
			{
				ret &= value.ActiveSensorWaitTime == actSensors.WaitTime;
				ret &= value.ActiveSensorExportType == actSensors.ExportType;
				ret &= String.Compare(value.ActiveSensorDestName, actSensors.DestName) == 0;
				ret &= String.Compare(value.ActiveSensorEmail, actSensors.SendToEmail) == 0;
				ret &= value.ActiveSensorCopies == actSensors.NumberCopies;
				ret &= value.ActiveSensorVideoFormat == actSensors.VideoFormat;
				ret &= value.ActiveSensorChannelMask == actSensors.ChannelMask;
			}
			if (restartSchedules != null)
			{
				var res = value.tDVRAddressBook.tDVRRestartTimes;
				if (res == null)
					ret = false;
				else
					ret &= restartSchedules.Equal(res);
			}
			return ret;
		}

		public void SetEntity(ref tDVRSystemInfo value)
		{
			if (value == null)
				value = new tDVRSystemInfo();
			//sysInf.KDVR = value.KDVR;
			if (syncTime != null)
			{
				value.EnSyncNTP = syncTime.Enable;
				value.SyncNTPHour =syncTime.Hour;
				value.SyncNTPMinute = syncTime.Minute;
			}
			//value.ControlVLoss = kControl;
			if (VLossAlarm != null)
			{
				value.EnControlVLoss = VLossAlarm.EnControl;
				value.EnAlarmVLoss = VLossAlarm.EnAlarm;
				value.AlarmHourVLoss = VLossAlarm.Hour;
				value.AlarmMinVLoss = VLossAlarm.Minute;
				value.AlarmSecVLoss = VLossAlarm.Second;
			}
			value.AutoLogonOS = AutoLogon;
			value.OSUserName = UserName;
			value.OSPassword = Password;
			value.EnWatermark = WatermarkEnable;
			value.DisplayWatermark = WatermarkDisplay;
			value.BaudratePTZ = BaudratePtzwd;
			//value.KPort = kPort;
			if (actSensors != null)
			{
				value.ActiveSensorWaitTime = actSensors.WaitTime;
				value.ActiveSensorExportType = actSensors.ExportType;
				value.ActiveSensorDestName = actSensors.DestName;
				value.ActiveSensorEmail = actSensors.SendToEmail;
				value.ActiveSensorCopies = actSensors.NumberCopies;
				value.ActiveSensorVideoFormat = actSensors.VideoFormat;
				value.ActiveSensorChannelMask = actSensors.ChannelMask;
			}
		}
	}

	[XmlRoot(RawSystemConfig.STR_VLossAlarm)]
	public class VLossAlarmInfo
	{
		[XmlElement(RawSystemConfig.STR_EnControl)]
		public Int32 EnControl { get; set; }

		[XmlElement(RawSystemConfig.STR_EnAlarm)]
		public Int32 EnAlarm { get; set; }

		[XmlElement(RawSystemConfig.STR_Control)]
		public Int32 Control { get; set; }

		[XmlElement(MessageDefines.STR_Hour)]
		public Int32 Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public Int32 Minute { get; set; }

		[XmlElement(MessageDefines.STR_Second)]
		public Int32 Second { get; set; }
	}

	[XmlRoot(RawSystemConfig.STR_RestartSchedule)]
	public class RestartSchedules
	{
		[XmlElement(RawSystemConfig.STR_Monday)]
		public RestartScheduleInfo Monday { get; set; }

		[XmlElement(RawSystemConfig.STR_Tuesday)]
		public RestartScheduleInfo Tuesday { get; set; }

		[XmlElement(RawSystemConfig.STR_Wednesday)]
		public RestartScheduleInfo Wednesday { get; set; }

		[XmlElement(RawSystemConfig.STR_Thursday)]
		public RestartScheduleInfo Thursday { get; set; }

		[XmlElement(RawSystemConfig.STR_Friday)]
		public RestartScheduleInfo Friday { get; set; }

		[XmlElement(RawSystemConfig.STR_Saturday)]
		public RestartScheduleInfo Saturday { get; set; }

		[XmlElement(RawSystemConfig.STR_Sunday)]
		public RestartScheduleInfo Sunday { get; set; }

		public List<RestartScheduleInfo> RestartTimes()
		{
			List<RestartScheduleInfo> lsResTimes = new List<RestartScheduleInfo>();
			Monday.ID = 0;
			lsResTimes.Add(Monday);
			Tuesday.ID = 1;
			lsResTimes.Add(Tuesday);
			Wednesday.ID = 2;
			lsResTimes.Add(Wednesday);
			Thursday.ID = 3;
			lsResTimes.Add(Thursday);
			Friday.ID = 4;
			lsResTimes.Add(Friday);
			Saturday.ID = 5;
			lsResTimes.Add(Saturday);
			Sunday.ID = 6;
			lsResTimes.Add(Sunday);
			return lsResTimes;
		}
		public bool Equal(IEnumerable<tDVRRestartTime>value)
		{
		
			bool ret = this.Monday.Equal(value.Where(item => (int)item.KDay == this.Monday.ID).FirstOrDefault());
			ret &= this.Tuesday.Equal(value.Where(item => (int)item.KDay == this.Tuesday.ID).FirstOrDefault());
			ret &= this.Wednesday.Equal(value.Where(item => (int)item.KDay == this.Wednesday.ID).FirstOrDefault());
			ret &= this.Thursday.Equal(value.Where(item => (int)item.KDay == this.Thursday.ID).FirstOrDefault());
			ret &= this.Friday.Equal(value.Where(item => (int)item.KDay == this.Friday.ID).FirstOrDefault());
			ret &= this.Saturday.Equal(value.Where(item => (int)item.KDay == this.Saturday.ID).FirstOrDefault());
			ret &= this.Sunday.Equal(value.Where(item => (int)item.KDay == this.Sunday.ID).FirstOrDefault());
			return ret;
		}
	}

	public class RestartScheduleInfo : IMessageEntity<tDVRRestartTime>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 ID { get; set; }

		[XmlElement(RawSystemConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(MessageDefines.STR_Hour)]
		public Int32 Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public Int32 Minute { get; set; }

		[XmlElement(RawSystemConfig.STR_RSMode)]
		public Int32 RSMode { get; set; }

		public bool Equal(tDVRRestartTime value)
		{
			bool result =
				  value.KDay == (short)ID &&
				  value.Enable == Enable &&
				  value.Hour == Hour &&
				  value.Minute == Minute &&
				  value.RestartMode == RSMode;
			return result;
		}

		public void SetEntity(ref tDVRRestartTime value)
		{
			if (value == null)
				value = new tDVRRestartTime();
			value.KDay = (short)ID;
			value.Enable = Enable;
			value.Hour = Hour;
			value.Minute = Minute;
			value.RestartMode = RSMode;
		}
	}

	[XmlRoot(RawSystemConfig.STR_ActiveteSensor)]
	public class ActivateSensorInfo
	{
		[XmlElement(RawSystemConfig.STR_WaitTime)]
		public Int32 WaitTime { get; set; }

		[XmlElement(RawSystemConfig.STR_ExportType)]
		public Int32 ExportType { get; set; }

		[XmlElement(RawSystemConfig.STR_DestName)]
		public string DestName { get; set; }

		[XmlElement(RawSystemConfig.STR_SendToEmail)]
		public string SendToEmail { get; set; }

		[XmlElement(RawSystemConfig.STR_NumberCopies)]
		public Int32 NumberCopies { get; set; }

		[XmlElement(RawSystemConfig.STR_VideoFormat)]
		public Int32 VideoFormat { get; set; }

		[XmlElement(RawSystemConfig.STR_ChannelMask)]
		public Decimal ChannelMask { get; set; }

		[XmlArray(RawSystemConfig.STR_Sensors)]
		[XmlArrayItem(RawSystemConfig.STR_Sensor)]
		public List<ActSensor> Sensors = new List<ActSensor>();
	}

	[XmlRoot(RawSystemConfig.STR_Sensor)]
	public class ActSensor
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 ID { get; set; }

		[XmlElement(RawSystemConfig.STR_Enabled)]
		public Int32 Enabled { get; set; }

		[XmlElement(RawSystemConfig.STR_Used)]
		public Int32 Used { get; set; }
	}

	[XmlRoot(RawSystemConfig.STR_SynchTime)]
	public class SynchTimeInfo
	{
		[XmlElement(RawSystemConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(MessageDefines.STR_Hour)]
		public Int32 Hour { get; set; }

		[XmlElement(MessageDefines.STR_Minute)]
		public Int32 Minute { get; set; }
	}
	#endregion
}
