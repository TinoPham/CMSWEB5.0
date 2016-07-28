using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawMotionConfig : RawDVRConfig<RawMotionBody>
	{
		#region Parameter
		public const string STR_MotionConfig = "motion_config";
		public const string STR_RotateDwell = "rotate_dwell_time";
		public const string STR_IsGreen = "is_green";
		public const string STR_IsDisplayMotion = "is_display_motion";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Motions = "motions";
		public const string STR_Motion = "motion";
		public const string STR_Left = "left";
		public const string STR_Top = "top";
		public const string STR_Width = "width";
		public const string STR_Height = "height";
		public const string STR_Sensitivity = "sensitivity";
		public const string STR_ControlNo = "control_number";
		public const string STR_EnAlarm = "enable_alarm";
		public const string STR_EnControl = "enable_control";
		public const string STR_DwellTime = "dwell_time";
		public const string STR_AlarmEndHour = "alarm_end_hour";
		public const string STR_AlarmEndMinute = "alarm_end_minute";
		public const string STR_EnFullScreen = "enable_full_screen";
		public const string STR_FullScreenChannelNo = "full_screen_channel_no";
		public const string STR_ControlHour = "control_hour";
		public const string STR_ControlMinute = "control_minute";
		public const string STR_ControlSecond = "control_second";
		public const string STR_AlarmStartHour = "alarm_start_hour";
		public const string STR_AlarmStartMinute = "alarm_start_minute";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawMotionBody msgBody { get; set; }

		//List<tDVRMotionTracker> _lsMoTrackerList;
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
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateMotionConfigs(DVRAdressBook, msgBody.motionData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_MOTION, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_MOTION, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateMotionConfigs(tDVRAddressBook dvrAdressBook, MotionData motionData)
		{
			bool ret = false;
			ret = SetMotionConfig(dvrAdressBook,motionData);

			db.Include<tDVRAddressBook,tDVRChannels>(dvrAdressBook,t=>t.tDVRChannels);
			foreach (tDVRChannels channel in dvrAdressBook.tDVRChannels)
			{
				ret |= UpdateMotionTrackers(dvrAdressBook,channel,motionData);
			}
			return ret;
		}

		private bool UpdateMotionTrackers(tDVRAddressBook dvrAdressBook, tDVRChannels channel, MotionData motionData)
		{
			//Join 2 list to 1 Object list with samme channel ID
			IEnumerable<tDVRMotionTrackers> dvrMotionTrackers = db.Query<tDVRMotionTrackers>(x => x.KChannel == channel.KChannel).ToList();
			var infoChannels = motionData.Channels.FirstOrDefault(x => x.id == channel.ChannelNo);
			if (infoChannels != null)
			{
				var motions = infoChannels.Motions;
				bool result = false;
				var updates = from motionTracker in dvrMotionTrackers
					from motionInfo in motions
					where motionInfo.Left == motionTracker.StartX
					      && motionInfo.Top == motionTracker.StartY
					      && motionInfo.Width == motionTracker.Width
					      && motionInfo.Height == motionTracker.Height
					select new {Item = motionTracker, InfoItem = motionInfo};

				//Update Object list above
				tDVRMotionTrackers dvrRecordSchedule;
				foreach (var item in updates)
				{
					if (item.InfoItem.Equal(item.Item))
					{
						continue;
					}
					else
					{
						dvrRecordSchedule = item.Item;
						item.InfoItem.SetEntity(ref dvrRecordSchedule);
						db.Update<tDVRMotionTrackers>(dvrRecordSchedule);
						result = true;
					}
				}

				//Use channels list and except item have updates list for deleting.
				IEnumerable<tDVRMotionTrackers> deletes = dvrMotionTrackers.Except(updates.Select(item => item.Item)).ToList();
				foreach (tDVRMotionTrackers delete in deletes)
				{
					db.Delete<tDVRMotionTrackers>(delete);
					System.Threading.Thread.Sleep(Time_Loop_Delay);
					result = true;
				}

				//Use channels Info list and except item have updates list for inserting.
				IEnumerable<MotionInfo> newitems = motions.Except(updates.Select(item => item.InfoItem));
				foreach (MotionInfo newitem in newitems)
				{
					dvrRecordSchedule = new tDVRMotionTrackers() {KChannel = channel.KChannel};
					newitem.SetEntity(ref dvrRecordSchedule);
					db.Insert<tDVRMotionTrackers>(dvrRecordSchedule);
					result = true;
				}

				return result;
			}
			else
			{
				if (!dvrMotionTrackers.Any()) return false;
				db.DeleteWhere<tDVRMotionTrackers>(t => t.KChannel == channel.KChannel);
				return true;
			}
		}

		private bool SetMotionConfig(tDVRAddressBook dvrAdressBook, MotionData motionData)
		{
			bool ret = false;
			tDVRMotionConfig motionCfg = db.FirstOrDefault<tDVRMotionConfig>(item => item.KDVR == DVRAdressBook.KDVR);
			if (motionCfg == null)
			{
				motionCfg = new tDVRMotionConfig() {KDVR = dvrAdressBook.KDVR};
				motionData.SetEntity(ref motionCfg);
				db.Insert<tDVRMotionConfig>(motionCfg);
				ret = true;
			}
			else
			{
				if (!motionData.Equal(motionCfg))
				{
					motionData.SetEntity(ref motionCfg);
					db.Update<tDVRMotionConfig>(motionCfg);
					ret = true;
				}
			}
			return ret;
		}

		#region Unused

		//public async Task<Commons.ERROR_CODE> UpdateToDB1()
		//{
		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	SetMotionConfig();

		//	List<tDVRChannel> lsKChannels = db.Query<tDVRChannel>( item => item.KDVR == DVRAdressBook.KDVR).ToList();
		//	if (lsKChannels.Count == 0)
		//	{
		//		SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
		//		return await base.UpdateToDB();
		//	}
		//	List<int> kChannelList = lsKChannels.Select(t => t.KChannel).ToList();
		//	_lsMoTrackerList = db.Query<tDVRMotionTracker>(x => kChannelList.Contains(x.KChannel)).ToList();
		//	foreach (tDVRChannel cn in lsKChannels)
		//	{
		//		if (cn != null && cn.Enable != 0)
		//			SetTrackers(cn);
		//	}
		//	return await base.UpdateToDB();
		//}

		//private void SetMotionInfo(Int32 kDvr, ref tDVRMotionConfig motion)
		//{
		//	motion.KDVR = kDvr;
		//	motion.RotateDwellTime = msgBody.motionData.RotateDwell;
		//	motion.Green = msgBody.motionData.IsGreen;
		//	motion.DisplayMotion = msgBody.motionData.IsDisplayMotion;
		//}

		//private bool CompareMotionInfo(Int32 kDvr, tDVRMotionConfig motion)
		//{
		//	bool result = motion.KDVR == kDvr &&
		//				  motion.RotateDwellTime == msgBody.motionData.RotateDwell &&
		//				  motion.Green == msgBody.motionData.IsGreen &&
		//				  motion.DisplayMotion == msgBody.motionData.IsDisplayMotion;
		//	return result;
		//}

		//private bool IsBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IsMotionData()
		//{
		//	if (!IsBodyData()) return false;
		//	return msgBody.motionData != null;
		//}

		//private bool IsChannelData()
		//{
		//	if (!IsMotionData()) return false;
		//	return msgBody.motionData.Channels != null;
		//}

		//private List<MotionChannel> GetChannelList()
		//{
		//	List<MotionChannel> channels = IsChannelData() ? msgBody.motionData.Channels.ToList() : new List<MotionChannel>();
		//	return channels;
		//}

		//private bool IsMotions(MotionChannel motionchannel)
		//{
		//	if (motionchannel == null) return false;
		//	else
		//	{
		//		if (motionchannel.Motions == null) return false;
		//		else return true;
		//	}
		//}

		//private List<MotionInfo> GetMotionList(MotionChannel motionchannel)
		//{
		//	List<MotionInfo> motionList = IsMotions(motionchannel) ? motionchannel.Motions.ToList() : new List<MotionInfo>();
		//	return motionList;
		//}

		//private void SetTrackers(tDVRChannel kChan)
		//{
		//	List<tDVRMotionTracker> lsMoTrackers = _lsMoTrackerList.Where(x => x.KChannel == kChan.KChannel).ToList();

		//	List<MotionChannel> channelList = GetChannelList();

		//	MotionChannel motionChannel = channelList.FirstOrDefault(x => x.id == kChan.ChannelNo);
		//	List<MotionInfo> motionList = GetMotionList(motionChannel);

		//	foreach (var motrack in lsMoTrackers)
		//	{
		//		tDVRMotionTracker moTracker = motrack;
		//		MotionInfo moInfo = motionList.FirstOrDefault(m => m.Left == moTracker.StartX
		//														   && m.Top == moTracker.StartY
		//														   && m.Width == moTracker.Width
		//														   && m.Height == moTracker.Height);
		//		if (moInfo != null)
		//		{
		//			if (!CompareTrackerInfo(moInfo, kChan, moTracker))
		//			{
		//				SetTrackerInfo(moInfo, kChan, ref moTracker);
		//				db.Update<tDVRMotionTracker>(moTracker);
		//			}
		//			motionList.Remove(moInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRMotionTracker>(moTracker);
		//		}
		//	}

		//	InsertNewMotions(kChan, motionList);

		//	db.Save();
		//}

		//private void InsertNewMotions(tDVRChannel kChan, List<MotionInfo> motionList)
		//{
		//	foreach (var motrack in motionList)
		//	{
		//		var moTracker = new tDVRMotionTracker();
		//		SetTrackerInfo(motrack, kChan, ref moTracker);
		//		db.Insert<tDVRMotionTracker>(moTracker);
		//	}
		//}

		//private void SetTrackerInfo(MotionInfo trackInfo, tDVRChannel kChan, ref tDVRMotionTracker moTrack)
		//{
		//	moTrack.KChannel = kChan.KChannel;
		//	moTrack.StartX = trackInfo.Left;
		//	moTrack.StartY = trackInfo.Top;
		//	moTrack.Width = trackInfo.Width;
		//	moTrack.Height = trackInfo.Height;
		//	moTrack.Sensitivity = trackInfo.Sensitivity;
		//	moTrack.ControlNo = trackInfo.ControlNo;
		//	moTrack.EnableAlarm = trackInfo.EnAlarm;
		//	moTrack.EnableControl = trackInfo.EnControl;
		//	moTrack.DwellTime = trackInfo.DwellTime;
		//	moTrack.AlarmEndHour = trackInfo.AlarmEndHour;
		//	moTrack.AlarmEndMinute = trackInfo.AlarmEndMinute;
		//	moTrack.EnableFullScreen = trackInfo.EnFullScreen;
		//	moTrack.FullScreenChannelNo = trackInfo.FullScreenChannelNo;
		//	moTrack.ControlHour = trackInfo.ControlHour;
		//	moTrack.ControlMinute = trackInfo.ControlMinute;
		//	moTrack.ControlSecond = trackInfo.ControlSecond;
		//	moTrack.AlarmStartHour = trackInfo.AlarmStartHour;
		//	moTrack.AlarmStartMinute = trackInfo.AlarmStartMinute;
		//}

		//private bool CompareTrackerInfo(MotionInfo trackInfo, tDVRChannel kChan, tDVRMotionTracker moTrack)
		//{
		//	bool result = moTrack.KChannel == kChan.KChannel &&
		//				  moTrack.StartX == trackInfo.Left &&
		//				  moTrack.StartY == trackInfo.Top &&
		//				  moTrack.Width == trackInfo.Width &&
		//				  moTrack.Height == trackInfo.Height &&
		//				  moTrack.Sensitivity == trackInfo.Sensitivity &&
		//				  moTrack.ControlNo == trackInfo.ControlNo &&
		//				  moTrack.EnableAlarm == trackInfo.EnAlarm &&
		//				  moTrack.EnableControl == trackInfo.EnControl &&
		//				  moTrack.DwellTime == trackInfo.DwellTime &&
		//				  moTrack.AlarmEndHour == trackInfo.AlarmEndHour &&
		//				  moTrack.AlarmEndMinute == trackInfo.AlarmEndMinute &&
		//				  moTrack.EnableFullScreen == trackInfo.EnFullScreen &&
		//				  moTrack.FullScreenChannelNo == trackInfo.FullScreenChannelNo &&
		//				  moTrack.ControlHour == trackInfo.ControlHour &&
		//				  moTrack.ControlMinute == trackInfo.ControlMinute &&
		//				  moTrack.ControlSecond == trackInfo.ControlSecond &&
		//				  moTrack.AlarmStartHour == trackInfo.AlarmStartHour &&
		//				  moTrack.AlarmStartMinute == trackInfo.AlarmStartMinute;
		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawMotionBody
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
		[XmlElement(RawMotionConfig.STR_MotionConfig)]
		public MotionData motionData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawMotionConfig.STR_MotionConfig)]
	public class MotionData : IMessageEntity<tDVRMotionConfig>
	{
		[XmlElement(RawMotionConfig.STR_RotateDwell)]
		public Int32 RotateDwell {get; set;}

		[XmlElement(RawMotionConfig.STR_IsGreen)]
		public Int32 IsGreen {get; set;}

		[XmlElement(RawMotionConfig.STR_IsDisplayMotion)]
		public Int32 IsDisplayMotion {get; set;}

		[XmlArray(RawMotionConfig.STR_Channels)]
		[XmlArrayItem(RawMotionConfig.STR_Channel)]
		public List<MotionChannel> Channels = new List<MotionChannel>();

		public bool Equal(tDVRMotionConfig value)
		{
			bool result = value.RotateDwellTime == RotateDwell &&
			  value.Green == IsGreen &&
			  value.DisplayMotion == IsDisplayMotion;
			return result;
		}

		public void SetEntity(ref tDVRMotionConfig value)
		{
			if (value == null)
				value = new tDVRMotionConfig();
			value.RotateDwellTime = RotateDwell;
			value.Green = IsGreen;
			value.DisplayMotion = IsDisplayMotion;
		}
	}

	public class MotionChannel
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlArray(RawMotionConfig.STR_Motions)]
		[XmlArrayItem(RawMotionConfig.STR_Motion)]
		public List<MotionInfo> Motions = new List<MotionInfo>();
	}

	public class MotionInfo : IMessageEntity<tDVRMotionTrackers>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawMotionConfig.STR_Left)]
		public Int32 Left { get; set; }

		[XmlElement(RawMotionConfig.STR_Top)]
		public Int32 Top { get; set; }

		[XmlElement(RawMotionConfig.STR_Width)]
		public Int32 Width { get; set; }

		[XmlElement(RawMotionConfig.STR_Height)]
		public Int32 Height { get; set; }

		[XmlElement(RawMotionConfig.STR_Sensitivity)]
		public Int32 Sensitivity { get; set; }

		[XmlElement(RawMotionConfig.STR_ControlNo)]
		public Int32 ControlNo { get; set; }

		[XmlElement(RawMotionConfig.STR_EnAlarm)]
		public Int32 EnAlarm { get; set; }

		[XmlElement(RawMotionConfig.STR_EnControl)]
		public Int32 EnControl { get; set; }

		[XmlElement(RawMotionConfig.STR_DwellTime)]
		public Int32 DwellTime { get; set; }

		[XmlElement(RawMotionConfig.STR_AlarmEndHour)]
		public Int32 AlarmEndHour { get; set; }

		[XmlElement(RawMotionConfig.STR_AlarmEndMinute)]
		public Int32 AlarmEndMinute { get; set; }

		[XmlElement(RawMotionConfig.STR_EnFullScreen)]
		public Int32 EnFullScreen { get; set; }

		[XmlElement(RawMotionConfig.STR_FullScreenChannelNo)]
		public Int32 FullScreenChannelNo { get; set; }

		[XmlElement(RawMotionConfig.STR_ControlHour)]
		public Int32 ControlHour { get; set; }

		[XmlElement(RawMotionConfig.STR_ControlMinute)]
		public Int32 ControlMinute { get; set; }

		[XmlElement(RawMotionConfig.STR_ControlSecond)]
		public Int32 ControlSecond { get; set; }

		[XmlElement(RawMotionConfig.STR_AlarmStartHour)]
		public Int32 AlarmStartHour { get; set; }

		[XmlElement(RawMotionConfig.STR_AlarmStartMinute)]
		public Int32 AlarmStartMinute { get; set; }

		public bool Equal(tDVRMotionTrackers value)
		{
			bool result = value.StartX == Left &&
						  value.StartY == Top &&
						  value.Width == Width &&
						  value.Height == Height &&
						  value.Sensitivity == Sensitivity &&
						  value.ControlNo == ControlNo &&
						  value.EnableAlarm == EnAlarm &&
						  value.EnableControl == EnControl &&
						  value.DwellTime == DwellTime &&
						  value.AlarmEndHour == AlarmEndHour &&
						  value.AlarmEndMinute == AlarmEndMinute &&
						  value.EnableFullScreen == EnFullScreen &&
						  value.FullScreenChannelNo == FullScreenChannelNo &&
						  value.ControlHour == ControlHour &&
						  value.ControlMinute == ControlMinute &&
						  value.ControlSecond == ControlSecond &&
						  value.AlarmStartHour == AlarmStartHour &&
						  value.AlarmStartMinute == AlarmStartMinute;
			return result;
		}

		public void SetEntity(ref tDVRMotionTrackers value)
		{
			if (value == null)
				value = new tDVRMotionTrackers();
			value.StartX = Left;
			value.StartY = Top;
			value.Width = Width;
			value.Height = Height;
			value.Sensitivity = Sensitivity;
			value.ControlNo = ControlNo;
			value.EnableAlarm = EnAlarm;
			value.EnableControl = EnControl;
			value.DwellTime = DwellTime;
			value.AlarmEndHour = AlarmEndHour;
			value.AlarmEndMinute = AlarmEndMinute;
			value.EnableFullScreen = EnFullScreen;
			value.FullScreenChannelNo = FullScreenChannelNo;
			value.ControlHour = ControlHour;
			value.ControlMinute = ControlMinute;
			value.ControlSecond = ControlSecond;
			value.AlarmStartHour = AlarmStartHour;
			value.AlarmStartMinute = AlarmStartMinute;
		}
	}
	#endregion
}
