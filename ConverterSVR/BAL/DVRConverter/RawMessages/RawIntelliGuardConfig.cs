using System;
using System.Collections.Generic;
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
	public class RawIntelliGuardConfig : RawDVRConfig<RawIntelliGuardBody>
	{
		#region Parameter
		public const string STR_IntelliGuard = "intelli_guard";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_IntervalAlarm = "interval_alarm";
		public const string STR_Motions = "motions";
		public const string STR_Motion = "motion";
		public const string STR_Left = "left";
		public const string STR_Top = "top";
		public const string STR_Width = "width";
		public const string STR_Height = "height";
		public const string STR_SendEmail = "send_email";
		public const string STR_EnSound = "enable_sound";
		public const string STR_UpdateBackground = "update_background";
		public const string STR_AreaName = "area_name";
		public const string STR_WaveFile = "wave_file";
		public const string STR_DetectionSize = "detection_size";
		public const string STR_DetectionSense = "detection_sense";
		public const string STR_SoundDuration = "sound_duration";
		public const string STR_FreshTime = "fresh_time";
		public const string STR_ActiveTime = "active_time";
		public const string STR_UseControl = "use_control";
		public const string STR_ControlNumber = "control_number";
		public const string STR_ControlHour = "control_hour";
		public const string STR_ControlMinute = "control_minute";
		public const string STR_ControlSecond = "control_second";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawIntelliGuardBody msgBody { get; set; }

		//List<tDVRIGChannel> _lsIgChannels;
		//List<tDVRIGTracker> _lsTracker;
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
			if (DVRAdressBook == null || msgBody.IGData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateIntelligConfigs(DVRAdressBook, msgBody.IGData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_INTELLI_GUARD, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_INTELLI_GUARD, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateIntelligConfigs(tDVRAddressBook dvrAdressBook, IntelliGuardData igData)
		{
			if (igData.Channels == null) return false;

			//Join 2 list to 1 Object list with samme channel ID
			var	lsChannels = dvrAdressBook.tDVRChannels.Select(ch => ch.KChannel);
			var dvripCameras = db.Query<tDVRIGChannel>(x => lsChannels.Contains(x.KChannel)).ToList();
			var ipCameras = igData.Channels;
			bool result = false;
			var updates = from dvripCamera in dvripCameras
						  from cameraInfo in ipCameras
						  where dvripCamera.tDVRChannels != null && cameraInfo.id == dvripCamera.tDVRChannels.ChannelNo
						  select new { Item = dvripCamera, InfoItem = cameraInfo };

			//Update Object list above
			tDVRIGChannel tDvripCamera;
			foreach (var item in updates)
			{
				tDvripCamera = item.Item;
				if (!item.InfoItem.Equal(item.Item))
				{
					item.InfoItem.SetEntity(ref tDvripCamera);
					db.Update<tDVRIGChannel>(tDvripCamera);
					result = true;
				}
				result |= UpdateIgTrackers(item.InfoItem, item.Item);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRIGChannel> deletes = dvripCameras.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRIGChannel delete in deletes)
			{
				db.DeleteWhere<tDVRIGTrackers>(t => t.KChannel == delete.KChannel);
				db.Delete<tDVRIGChannel>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<IGChannel> newitems = ipCameras.Except(updates.Select(item => item.InfoItem));
			foreach (IGChannel newitem in newitems)
			{
				var kChannel = dvrAdressBook.tDVRChannels.Where(t => t.ChannelNo == newitem.id).Select(t => t.KChannel).FirstOrDefault();
				tDvripCamera = new tDVRIGChannel() { KChannel = kChannel };
				newitem.SetEntity(ref tDvripCamera);
				db.Insert<tDVRIGChannel>(tDvripCamera);
				result |= UpdateIgTrackers(newitem, tDvripCamera);
				result = true;
			}

			return result;
		}

		private bool UpdateIgTrackers(IGChannel infoItem, tDVRIGChannel igItem)
		{
			if (infoItem.Motions == null) return false;

			//Join 2 list to 1 Object list with samme channel ID
			var dvripCameras = db.Query<tDVRIGTrackers>(x => x.KChannel == igItem.KChannel).ToList();
			var ipCameras = infoItem.Motions;
			bool result = false;
			var updates = from dvripCamera in dvripCameras
						  from cameraInfo in ipCameras
						  where cameraInfo.Left == dvripCamera.StartX
																		 && cameraInfo.Top == dvripCamera.StartY
																		 && cameraInfo.Width == dvripCamera.Width
																		 && cameraInfo.Height == dvripCamera.Height
						  select new { Item = dvripCamera, InfoItem = cameraInfo };

			//Update Object list above
			tDVRIGTrackers tDvripCamera;
			foreach (var item in updates)
			{
				if (item.InfoItem.Equal(item.Item))
				{
					continue;
				}
				else
				{
					tDvripCamera = item.Item;
					item.InfoItem.SetEntity(ref tDvripCamera);
					db.Update<tDVRIGTrackers>(tDvripCamera);
					result = true;
				}
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRIGTrackers> deletes = dvripCameras.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRIGTrackers delete in deletes)
			{
				db.Delete<tDVRIGTrackers>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<IGMotionInfo> newitems = ipCameras.Except(updates.Select(item => item.InfoItem));
			foreach (IGMotionInfo newitem in newitems)
			{
				var kChannel = DVRAdressBook.tDVRChannels.Where(t => t.ChannelNo == infoItem.id).Select(t => t.KChannel).FirstOrDefault();
				tDvripCamera = new tDVRIGTrackers() { KChannel = kChannel };
				newitem.SetEntity(ref tDvripCamera);
				db.Insert<tDVRIGTrackers>(tDvripCamera);
				result = true;
			}

			return result;
		}

		#region Unused

		//public async Task<Commons.ERROR_CODE> UpdateToDB1()
		//{

		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	SetIgChannels(DVRAdressBook.KDVR);
		//	return await base.UpdateToDB();
		//}


		//private List<IGChannel> GetChannelList()
		//{
		//	List<IGChannel> igChannellist = IsChannelData() ? msgBody.IGData.Channels.ToList() : new List<IGChannel>();
		//	return igChannellist;
		//}

		//private void SetIgChannels(Int32 kDvr)
		//{
		//	List<tDVRChannel> channels = db.Query<tDVRChannel>(x => x.KDVR == kDvr).ToList();
		//	if (CheckDvrSync(channels)) return;
		//	GetGlobalList(channels);
		//	List<IGChannel> igChannellist = GetChannelList();
		//	foreach (var ig in _lsIgChannels)
		//	{
		//		tDVRIGChannel igChannel = ig;
		//		tDVRChannel cn = channels.FirstOrDefault(t => t.KChannel == ig.KChannel);
		//		IGChannel igInfo = igChannellist.FirstOrDefault(t => cn != null && t.id == cn.ChannelNo);
		//		if (igInfo != null)
		//		{
		//			if (cn != null && cn.Enable != 0)
		//			{
		//				if (!CompareIgChannelInfo(kDvr, cn.KChannel, igInfo, igChannel))
		//				{
		//					SetIgChannelInfo(kDvr, cn.KChannel, igInfo, ref igChannel);
		//					db.Update<tDVRIGChannel>(igChannel);
		//				}
		//				SetIgTrackers(cn.KChannel, cn.ChannelNo, igInfo);
		//			}
		//			igChannellist.Remove(igInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRIGChannel>(igChannel);
		//			List<tDVRIGTracker> lsTrackers = _lsTracker.Where(x => cn != null && x.KChannel == cn.KChannel).ToList();
		//			foreach (var tk in lsTrackers)
		//			{
		//				db.Delete<tDVRIGTracker>(tk);
		//			}
		//		}
		//	}

		//	InsertNewIgChannels(kDvr, igChannellist, channels);
		//	db.Save();
		//}

		//private void GetGlobalList(List<tDVRChannel> channels)
		//{
		//	List<int> lsChannels = channels.Select(ch => ch.KChannel).ToList();
		//	_lsIgChannels = db.Query<tDVRIGChannel>(x => lsChannels.Contains(x.KChannel)).ToList();
		//	_lsTracker = db.Query<tDVRIGTracker>(x => lsChannels.Contains(x.KChannel)).ToList();
		//}

		//private bool CheckDvrSync(List<tDVRChannel> channels)
		//{
		//	if (channels.Count == 0)
		//	{
		//		SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
		//		return true;
		//	}
		//	return false;
		//}

		//private void InsertNewIgChannels(int kDvr, List<IGChannel> igChannellist, List<tDVRChannel> channels)
		//{
		//	foreach (var igInfo in igChannellist)
		//	{
		//		tDVRChannel cn = channels.FirstOrDefault(t => t.KDVR == kDvr && t.ChannelNo == igInfo.id);
		//		var igChan = new tDVRIGChannel();
		//		SetIgChannelInfo(kDvr, cn.KChannel, igInfo, ref igChan);
		//		db.Insert<tDVRIGChannel>(igChan);
		//		SetIgTrackers(cn.KChannel, cn.ChannelNo, igInfo);
		//	}
		//}

		//private void SetIgChannelInfo(Int32 kDvr, int kChannel, IGChannel igInfo, ref tDVRIGChannel chanInf)
		//{
		//	chanInf.KChannel = kChannel;
		//	chanInf.IntervalAlarm = igInfo.IntervalAlarm;
		//}

		//private bool CompareIgChannelInfo(Int32 kDvr, int kChannel, IGChannel igInfo, tDVRIGChannel chanInf)
		//{
		//	bool result = chanInf.KChannel == kChannel &&
		//				  chanInf.IntervalAlarm == igInfo.IntervalAlarm;
		//	return result;
		//}

		//private bool IsIgBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IsIgData()
		//{
		//	if (!IsIgBodyData()) return false;
		//	return msgBody.IGData != null;
		//}

		//private bool IsChannelData()
		//{
		//	if (!IsIgData()) return false;
		//	return msgBody.IGData.Channels != null;
		//}

		//private bool IsMontionInfo(IGChannel igInfo)
		//{
		//	if (igInfo.Motions == null) return false;
		//	else return true;
		//}

		//private List<IGMotionInfo> GetMontionInfoList(IGChannel igInfo)
		//{
		//	List<IGMotionInfo> lsMotions = IsMontionInfo(igInfo) ? igInfo.Motions.ToList() : new List<IGMotionInfo>();
		//	return lsMotions;
		//}

		//private void SetIgTrackers(int kchan, int chanNo, IGChannel igInfo)
		//{
		//	List<IGMotionInfo> lsMotions = GetMontionInfoList(igInfo);

		//	List<tDVRIGTracker> lsTrackers = _lsTracker.Where(x => x.KChannel == kchan).ToList();
		//	foreach (var track in lsTrackers)
		//	{
		//		tDVRIGTracker igTracker = track;
		//		IGMotionInfo igTrackInfo = lsMotions.FirstOrDefault(x => x.Left == igTracker.StartX
		//																 && x.Top == igTracker.StartY
		//																 && x.Width == igTracker.Width
		//																 && x.Height == igTracker.Height);
		//		if (igTrackInfo != null)
		//		{
		//			if (!CompareIgTrackerInfo(igTrackInfo, kchan, igTracker))
		//			{
		//				SetIgTrackerInfo(igTrackInfo, kchan, ref igTracker);
		//				db.Update<tDVRIGTracker>(igTracker);
		//			}
		//			lsMotions.Remove(igTrackInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRIGTracker>(igTracker);
		//		}
		//	}

		//	InsertNewIgTrackers(kchan, lsMotions);
		//}

		//private void InsertNewIgTrackers(int kchan, List<IGMotionInfo> lsMotions)
		//{
		//	foreach (var track in lsMotions)
		//	{
		//		var igTracker = new tDVRIGTracker();
		//		SetIgTrackerInfo(track, kchan, ref igTracker);
		//		db.Insert<tDVRIGTracker>(igTracker);
		//	}
		//}

		//private void SetIgTrackerInfo(IGMotionInfo igTrackInfo, int kchan, ref tDVRIGTracker igTracker)
		//{
		//	igTracker.KChannel = kchan;
		//	igTracker.StartX = igTrackInfo.Left;
		//	igTracker.StartY = igTrackInfo.Top;
		//	igTracker.Width = igTrackInfo.Width;
		//	igTracker.Height = igTrackInfo.Height;
		//	igTracker.EnSendEmail = igTrackInfo.SendEmail != 0;
		//	igTracker.EnableSound = igTrackInfo.EnSound != 0;
		//	igTracker.WaveFile = igTrackInfo.WaveFile;
		//	igTracker.UpdateBackground = igTrackInfo.UpdateBackground != 0;
		//	igTracker.AreaName = igTrackInfo.AreaName;
		//	igTracker.DetectionSize = igTrackInfo.DetectionSize;
		//	igTracker.DetectionSense = igTrackInfo.DetectionSense;
		//	igTracker.SoundDuration = igTrackInfo.SoundDuration;
		//	igTracker.FreshTime = igTrackInfo.FreshTime;
		//	igTracker.ActiveTime = igTrackInfo.ActiveTime;
		//	igTracker.EnableControl = igTrackInfo.UseControl;
		//	igTracker.ControlNumber = igTrackInfo.ControlNumber;
		//	igTracker.ControlHour = igTrackInfo.ControlHour;
		//	igTracker.ControlMinute = igTrackInfo.ControlMinute;
		//	igTracker.ControlSecond = igTrackInfo.ControlSecond;
		//}

		//private bool CompareIgTrackerInfo(IGMotionInfo igTrackInfo, int kchan, tDVRIGTracker igTracker)
		//{
		//	bool result = igTracker.KChannel == kchan &&
		//				  igTracker.StartX == igTrackInfo.Left &&
		//				  igTracker.StartY == igTrackInfo.Top &&
		//				  igTracker.Width == igTrackInfo.Width &&
		//				  igTracker.Height == igTrackInfo.Height &&
		//				  igTracker.EnSendEmail == (igTrackInfo.SendEmail != 0) &&
		//				  igTracker.EnableSound == (igTrackInfo.EnSound != 0) &&
		//				  igTracker.WaveFile == igTrackInfo.WaveFile &&
		//				  igTracker.UpdateBackground == (igTrackInfo.UpdateBackground != 0) &&
		//				  igTracker.AreaName == igTrackInfo.AreaName &&
		//				  igTracker.DetectionSize == igTrackInfo.DetectionSize &&
		//				  igTracker.DetectionSense == igTrackInfo.DetectionSense &&
		//				  igTracker.SoundDuration == igTrackInfo.SoundDuration &&
		//				  igTracker.FreshTime == igTrackInfo.FreshTime &&
		//				  igTracker.ActiveTime == igTrackInfo.ActiveTime &&
		//				  igTracker.EnableControl == igTrackInfo.UseControl &&
		//				  igTracker.ControlNumber == igTrackInfo.ControlNumber &&
		//				  igTracker.ControlHour == igTrackInfo.ControlHour &&
		//				  igTracker.ControlMinute == igTrackInfo.ControlMinute &&
		//				  igTracker.ControlSecond == igTrackInfo.ControlSecond;
		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawIntelliGuardBody
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
		[XmlElement(RawIntelliGuardConfig.STR_IntelliGuard)]
		public IntelliGuardData IGData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawIntelliGuardConfig.STR_IntelliGuard)]
	public class IntelliGuardData
	{
		[XmlArray(RawIntelliGuardConfig.STR_Channels)]
		[XmlArrayItem(RawIntelliGuardConfig.STR_Channel)]
		public List<IGChannel> Channels = new List<IGChannel>();
	}

	[XmlRoot(RawIntelliGuardConfig.STR_Channel)]
	public class IGChannel : IMessageEntity<tDVRIGChannel>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawIntelliGuardConfig.STR_IntervalAlarm)]
		public Int16 IntervalAlarm { get; set; }

		[XmlArray(RawIntelliGuardConfig.STR_Motions)]
		[XmlArrayItem(RawIntelliGuardConfig.STR_Motion)]
		public List<IGMotionInfo> Motions = new List<IGMotionInfo>();

		public bool Equal(tDVRIGChannel value)
		{
			bool result = value.IntervalAlarm == IntervalAlarm;
			return result;
		}

		public void SetEntity(ref tDVRIGChannel value)
		{
			if (value == null)
				value = new tDVRIGChannel();
			value.IntervalAlarm = IntervalAlarm;
		}
	}

	[XmlRoot(RawIntelliGuardConfig.STR_Motion)]
	public class IGMotionInfo : IMessageEntity<tDVRIGTrackers>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_Left)]
		public Int16 Left { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_Top)]
		public Int16 Top { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_Width)]
		public Int16 Width { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_Height)]
		public Int16 Height { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_SendEmail)]
		public Int32 SendEmail { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_EnSound)]
		public Int32 EnSound { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_UpdateBackground)]
		public Int32 UpdateBackground { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_AreaName)]
		public string AreaName { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_WaveFile)]
		public string WaveFile { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_DetectionSize)]
		public Int16 DetectionSize { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_DetectionSense)]
		public Int16 DetectionSense { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_SoundDuration)]
		public Int16 SoundDuration { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_FreshTime)]
		public Int16 FreshTime { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_ActiveTime)]
		public Int32 ActiveTime { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_UseControl)]
		public Int32 UseControl { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_ControlNumber)]
		public byte ControlNumber { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_ControlHour)]
		public byte ControlHour { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_ControlMinute)]
		public byte ControlMinute { get; set; }

		[XmlElement(RawIntelliGuardConfig.STR_ControlSecond)]
		public byte ControlSecond { get; set; }

		public bool Equal(tDVRIGTrackers value)
		{
			bool result = //value.KChannel == kchan &&
						  value.StartX == Left &&
						  value.StartY == Top &&
						  value.Width == Width &&
						  value.Height == Height &&
						  value.EnSendEmail == (SendEmail != 0) &&
						  value.EnableSound == (EnSound != 0) &&
						  value.WaveFile == WaveFile &&
						  value.UpdateBackground == (UpdateBackground != 0) &&
						  value.AreaName == AreaName &&
						  value.DetectionSize == DetectionSize &&
						  value.DetectionSense == DetectionSense &&
						  value.SoundDuration == SoundDuration &&
						  value.FreshTime == FreshTime &&
						  value.ActiveTime == ActiveTime &&
						  value.EnableControl == UseControl &&
						  value.ControlNumber == ControlNumber &&
						  value.ControlHour == ControlHour &&
						  value.ControlMinute == ControlMinute &&
						  value.ControlSecond == ControlSecond;
			return result;
		}

		public void SetEntity(ref tDVRIGTrackers value)
		{
			if (value == null)
				value = new tDVRIGTrackers();
			//value.KChannel = kchan;
			value.StartX = Left;
			value.StartY = Top;
			value.Width = Width;
			value.Height = Height;
			value.EnSendEmail = SendEmail != 0;
			value.EnableSound = EnSound != 0;
			value.WaveFile = WaveFile;
			value.UpdateBackground = UpdateBackground != 0;
			value.AreaName = AreaName;
			value.DetectionSize = DetectionSize;
			value.DetectionSense = DetectionSense;
			value.SoundDuration = SoundDuration;
			value.FreshTime = FreshTime;
			value.ActiveTime = ActiveTime;
			value.EnableControl = UseControl;
			value.ControlNumber = ControlNumber;
			value.ControlHour = ControlHour;
			value.ControlMinute = ControlMinute;
			value.ControlSecond = ControlSecond;
		}
	}
	#endregion
}
