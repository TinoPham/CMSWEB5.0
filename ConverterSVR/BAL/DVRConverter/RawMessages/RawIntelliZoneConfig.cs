using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawIntelliZoneConfig : RawDVRConfig<RawIntelliZoneBody>
	{
		#region Parameter
		public const string STR_IntelliZone = "intelli_zone";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Enable = "enable";
		public const string STR_FirstPresetPosition = "first_preset_position";
		public const string STR_Sensitivity = "sensitivity";
		public const string STR_FreshTime = "fresh_time";
		public const string STR_IsAdjustAllCamera = "is_adjust_all_camera";
		public const string STR_Is24Hours = "is_24hour";
		public const string STR_DwellTimeMovePreset = "dwell_time_move_preset";
		public const string STR_ActTimeStart = "activate_time_start";
		public const string STR_ActTimeEnd = "activate_time_end";
		public const string STR_Trackers = "trackers";
		public const string STR_Tracker = "tracker";
		public const string STR_PercentChange = "percent_of_changes";
		public const string STR_PresetName = "preset_name";
		public const string STR_PosX = "position_x";
		public const string STR_PosY = "position_y";
		public const string STR_Width = "width";
		public const string STR_Height = "height";
		public const string STR_PresetID = "preset_id";
		public const string STR_DwellTime = "dwell_time";
		public const string STR_Priority = "priority";
		public const string STR_ControlOutput = "control_output";
		public const string STR_RelativeCamID = "relative_camera_id";
		public const string STR_RelativePresetID = "relative_preset_id";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawIntelliZoneBody msgBody { get; set; }

		//List<tDVRIZChannel> _lsIzChannels;
		//List<tDVRIZTracker> _izTrackerlst;
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
			if (DVRAdressBook == null || msgBody.IZData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateIntelliZoneConfigs(DVRAdressBook, msgBody.IZData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_INTELLI_ZONE, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_INTELLI_ZONE, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateIntelliZoneConfigs(tDVRAddressBook dvrAdressBook, IntelliZoneData izData)
		{
			if (izData.Channels == null) return false;

			//Join 2 list to 1 Object list with samme channel ID
			List<int> lsChannels = dvrAdressBook.tDVRChannels.Select(ch => ch.KChannel).ToList();
			var dvripCameras = db.Query<tDVRIZChannels>(x => lsChannels.Contains(x.KChannel)).ToList();
			var ipCameras = izData.Channels;
			bool result = false;
			var updates = from dvripCamera in dvripCameras
						  from cameraInfo in ipCameras
						  where dvripCamera.tDVRChannels != null && cameraInfo.ID == dvripCamera.tDVRChannels.ChannelNo
						  select new { Item = dvripCamera, InfoItem = cameraInfo };

			//Update Object list above
			tDVRIZChannels tDvripCamera;
			foreach (var item in updates)
			{
				tDvripCamera = item.Item;
				if (!item.InfoItem.Equal(item.Item))
				{
					item.InfoItem.SetEntity(ref tDvripCamera);
					db.Update<tDVRIZChannels>(tDvripCamera);
					result = true;
				}
				result |= UpdateIzTrackers(item.InfoItem, item.Item);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRIZChannels> deletes = dvripCameras.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRIZChannels delete in deletes)
			{
				db.DeleteWhere<tDVRIZTrackers>(t => t.KChannel == delete.KChannel);
				db.Delete<tDVRIZChannels>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<IZChannelInfo> newitems = ipCameras.Except(updates.Select(item => item.InfoItem)).ToList();
			foreach (IZChannelInfo newitem in newitems)
			{
				var kChannel = dvrAdressBook.tDVRChannels.Where(t => t.ChannelNo == newitem.ID).Select(t=>t.KChannel).FirstOrDefault();
				tDvripCamera = new tDVRIZChannels() { KChannel = kChannel};
				newitem.SetEntity(ref tDvripCamera);
				db.Insert<tDVRIZChannels>(tDvripCamera);
				result |= UpdateIzTrackers(newitem, tDvripCamera);
				result = true;
			}

			return result;
		}

		private bool UpdateIzTrackers(IZChannelInfo infoItem, tDVRIZChannels izItem)
		{
			if (infoItem.Trackers == null) return false;

			//Join 2 list to 1 Object list with samme channel ID
			var dvripCameras = db.Query<tDVRIZTrackers>(x => x.KChannel == izItem.KChannel).ToList();
			var ipCameras = infoItem.Trackers;
			bool result = false;
			var updates = from dvripCamera in dvripCameras
						  from cameraInfo in ipCameras
						  where cameraInfo.PosX == dvripCamera.StartX
																			  && cameraInfo.PosY == dvripCamera.StartY
																			  && cameraInfo.Width == dvripCamera.Width
																			  && cameraInfo.Height == dvripCamera.Height
						  select new { Item = dvripCamera, InfoItem = cameraInfo };

			//Update Object list above
			tDVRIZTrackers tDvripCamera;
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
					db.Update<tDVRIZTrackers>(tDvripCamera);
					result = true;
				}
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRIZTrackers> deletes = dvripCameras.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRIZTrackers delete in deletes)
			{
				db.Delete<tDVRIZTrackers>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<IZTrackerInfo> newitems = ipCameras.Except(updates.Select(item => item.InfoItem));
			foreach (IZTrackerInfo newitem in newitems)
			{
				var kChannel = DVRAdressBook.tDVRChannels.Where(t => t.ChannelNo == infoItem.ID).Select(t => t.KChannel).FirstOrDefault();
				tDvripCamera = new tDVRIZTrackers() { KChannel = kChannel };
				newitem.SetEntity(ref tDvripCamera);
				db.Insert<tDVRIZTrackers>(tDvripCamera);
				result = true;
			}

			return result;
		}

		#region Unused

		//public async Task<Commons.ERROR_CODE> UpdateToDB1()
		//{
		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	SetIzChannels(DVRAdressBook.KDVR);
		//	return await Task.FromResult<Commons.ERROR_CODE>(db.Save() == -1? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
		//}

		//private List<IZChannelInfo> GetChannelList()
		//{
		//	List<IZChannelInfo> izChannelInfoList = IsChannels() ? msgBody.IZData.Channels.ToList() : new List<IZChannelInfo>();
		//	return izChannelInfoList;
		//}

		//private void SetIzChannels(Int32 kDvr)
		//{
		//	List<tDVRChannel> channellst = db.Query<tDVRChannel>(x => x.KDVR == kDvr).OrderBy(x=>x.ChannelNo).ToList();
		//	if (CheckDvrSync(channellst)) return;
		//	List<int> lsChannels = channellst.Where(x => x.KDVR == kDvr).Select(ch => ch.KChannel).ToList();
		//	if (lsChannels.Count == 0)
		//		return;

		//	_lsIzChannels = db.Query<tDVRIZChannel>(x => lsChannels.Contains(x.KChannel)).OrderBy(x=>x.KChannel).ToList();
		//	_izTrackerlst = db.Query<tDVRIZTracker>(x => lsChannels.Contains(x.KChannel)).ToList();

		//	List<IZChannelInfo> izChannelInfoList = GetChannelList();
		//	foreach (var ls in _lsIzChannels)
		//	{
		//		tDVRIZChannel izchanel = ls;
		//		tDVRChannel cn = channellst.FirstOrDefault(t => t.KChannel == izchanel.KChannel);
		//		IZChannelInfo izInfo = izChannelInfoList.FirstOrDefault(t => cn != null && t.ID == cn.ChannelNo);
		//		if (izInfo != null)
		//		{
		//			if (cn != null && cn.Enable != 0)
		//			{
		//				if (!CompareIzChannelInfo(kDvr, izInfo, cn.KChannel, izchanel))
		//				{
		//					SetIzChannelInfo(kDvr, izInfo, cn.KChannel, ref izchanel);
		//					db.Update<tDVRIZChannel>(izchanel);
		//				}
		//				SetIzTrackers(cn, izInfo);
		//			}
		//			izChannelInfoList.Remove(izInfo);
		//		}
		//		else
		//		{
		//			List<tDVRIZTracker> izTrackers = _izTrackerlst.Where(x=>cn != null && x.KChannel == cn.KChannel).ToList();
		//			foreach (var iz in izTrackers)
		//			{
		//				db.Delete<tDVRIZTracker>(iz);
		//			}
		//			db.Delete<tDVRIZChannel>(izchanel);
		//		}
		//	}

		//	InsertNewIzChannels(kDvr, izChannelInfoList, channellst);
		//}

		//private void InsertNewIzChannels(int kDvr, List<IZChannelInfo> izChannelInfoList, List<tDVRChannel> channellst)
		//{
		//	foreach (var ls in izChannelInfoList)
		//	{
		//		tDVRChannel cn = channellst.FirstOrDefault(x => x.ChannelNo == ls.ID);
		//		if (cn != null)
		//		{
		//			var izChan = new tDVRIZChannel();
		//			SetIzChannelInfo(kDvr, ls, cn.KChannel, ref izChan);
		//			db.Insert<tDVRIZChannel>(izChan);
		//			SetIzTrackers(cn, ls);
		//		}
		//	}
		//}

		//private bool CheckDvrSync(List<tDVRChannel> channellst)
		//{
		//	if (channellst.Count == 0)
		//	{
		//		SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
		//		return true;
		//	}
		//	return false;
		//}

		//private bool IsBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IsIzData()
		//{
		//	if (!IsBodyData()) return false;
		//	return msgBody.IZData != null;
		//}

		//private bool IsChannels()
		//{
		//	if (!IsIzData()) return false;
		//	return msgBody.IZData.Channels != null;
		//}

		//private bool IsTrackersInfo(IZChannelInfo izInfo)
		//{
		//	if (izInfo == null)
		//	{
		//		return false; 
		//	}
		//	return izInfo.Trackers != null;
		//}

		//private List<IZTrackerInfo> GetTrackerList(IZChannelInfo izInfo)
		//{
		//	List<IZTrackerInfo> izTrackerlist = IsTrackersInfo(izInfo) ? izInfo.Trackers.ToList() : new List<IZTrackerInfo>();

		//	return izTrackerlist;
		//}

		//private void SetIzTrackers(tDVRChannel channel, IZChannelInfo izInfo)
		//{
		//	List<IZTrackerInfo> izTrackerlist = GetTrackerList(izInfo);

		//	List<tDVRIZTracker> lsTrackers = _izTrackerlst.Where(x => x.KChannel == channel.KChannel).ToList();

		//	foreach (var track in lsTrackers)
		//	{
		//		tDVRIZTracker izTracker = track;
		//		IZTrackerInfo izTrackInfo = izTrackerlist.FirstOrDefault(x => x.PosX == izTracker.StartX
		//																	  && x.PosY == izTracker.StartY
		//																	  && x.Width == izTracker.Width
		//																	  && x.Height == izTracker.Height);
		//		if (izTrackInfo != null)
		//		{
		//			if (!CompareIzTracker(izTrackInfo, channel.KChannel, izTracker))
		//			{
		//				SetIzTracker(izTrackInfo, channel.KChannel, ref izTracker);
		//				db.Update<tDVRIZTracker>(izTracker);
		//			}
		//			izTrackerlist.Remove(izTrackInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRIZTracker>(izTracker);
		//		}
		//	}

		//	InsertNewIzTrackers(channel, izTrackerlist);
		//}

		//private void InsertNewIzTrackers(tDVRChannel channel, List<IZTrackerInfo> izTrackerlist)
		//{
		//	foreach (var track in izTrackerlist)
		//	{
		//		var izTracker = new tDVRIZTracker();
		//		SetIzTracker(track, channel.KChannel, ref izTracker);
		//		db.Insert<tDVRIZTracker>(izTracker);
		//	}
		//}

		//private void SetIzTracker(IZTrackerInfo izTracks, int kchan, ref tDVRIZTracker izTracker)
		//{
		//	izTracker.KChannel = kchan;
		//	izTracker.PercentOfChanges = izTracks.PercentChange;
		//	izTracker.AreaName = izTracks.PresetName;
		//	izTracker.StartX = izTracks.PosX;
		//	izTracker.StartY = izTracks.PosY;
		//	izTracker.Width = izTracks.Width;
		//	izTracker.Height = izTracks.Height;
		//	izTracker.PresetID = izTracks.PresetID;
		//	izTracker.DwellTime = izTracks.DwellTime;
		//	izTracker.Priority = izTracks.Priority;
		//	izTracker.ControlOutput = izTracks.ControlOutput;
		//	izTracker.RelativeCameraID = izTracks.RelativeCamID;
		//	izTracker.RelativeResetID = izTracks.RelativePresetID;
		//}

		//private bool CompareIzTracker(IZTrackerInfo izTracks, int kchan, tDVRIZTracker izTracker)
		//{
		//	bool result = izTracker.KChannel == kchan &&
		//				  izTracker.PercentOfChanges == izTracks.PercentChange &&
		//				  izTracker.AreaName == izTracks.PresetName &&
		//				  izTracker.StartX == izTracks.PosX &&
		//				  izTracker.StartY == izTracks.PosY &&
		//				  izTracker.Width == izTracks.Width &&
		//				  izTracker.Height == izTracks.Height &&
		//				  izTracker.PresetID == izTracks.PresetID &&
		//				  izTracker.DwellTime == izTracks.DwellTime &&
		//				  izTracker.Priority == izTracks.Priority &&
		//				  izTracker.ControlOutput == izTracks.ControlOutput &&
		//				  izTracker.RelativeCameraID == izTracks.RelativeCamID &&
		//				  izTracker.RelativeResetID == izTracks.RelativePresetID;
		//	return result;
		//}

		//private void SetIzChannelInfo(Int32 kDvr, IZChannelInfo iz, int kchan, ref tDVRIZChannel chanInf)
		//{
		//	chanInf.KChannel = kchan;
		//	chanInf.Enable = iz.Enable != 0;
		//	chanInf.StartPresetPosition = iz.FirstPresetPosition;
		//	chanInf.Sensitivity = iz.Sensitivity;
		//	chanInf.FreshTime = iz.FreshTime;
		//	chanInf.IsAdjustAllCamera = iz.IsAdjustAllCamera != 0;
		//	chanInf.Is24Hour = iz.Is24Hours != 0;
		//	chanInf.DwellTimeMovePreset = iz.DwellTimeMovePreset;
		//	if (iz.ActTimeStart != null)
		//	{
		//		chanInf.StartHour = iz.ActTimeStart.Hour;
		//		chanInf.StartMinute = iz.ActTimeStart.Minute;
		//		chanInf.StartSecond = iz.ActTimeStart.Second;
		//	}

		//	if (iz.ActTimeEnd != null)
		//	{
		//		chanInf.EndHour = iz.ActTimeEnd.Hour;
		//		chanInf.EndMinute = iz.ActTimeEnd.Minute;
		//		chanInf.EndSecond = iz.ActTimeEnd.Second;
		//	}
		//}

		//private bool CompareIzChannelInfo(Int32 kDvr, IZChannelInfo iz, int kchan, tDVRIZChannel chanInf)
		//{
		//	bool result = chanInf.KChannel == kchan &&
		//				  chanInf.Enable == (iz.Enable != 0) &&
		//				  chanInf.IsAdjustAllCamera == (iz.IsAdjustAllCamera != 0) &&
		//				  chanInf.Is24Hour == (iz.Is24Hours != 0) &&
		//				  chanInf.StartPresetPosition == iz.FirstPresetPosition &&
		//				  chanInf.Sensitivity == iz.Sensitivity &&
		//				  chanInf.FreshTime == iz.FreshTime &&
		//				  chanInf.DwellTimeMovePreset == iz.DwellTimeMovePreset;
		//	if (iz.ActTimeStart != null)
		//	{
		//		result = result &&
		//				 chanInf.StartHour == iz.ActTimeStart.Hour &&
		//				 chanInf.StartMinute == iz.ActTimeStart.Minute &&
		//				 chanInf.StartSecond == iz.ActTimeStart.Second;
		//	}

		//	if (iz.ActTimeEnd != null)
		//	{
		//		result = result &&
		//				 chanInf.EndHour == iz.ActTimeEnd.Hour &&
		//				 chanInf.EndMinute == iz.ActTimeEnd.Minute &&
		//				 chanInf.EndSecond == iz.ActTimeEnd.Second;
		//	}
		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawIntelliZoneBody
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
		[XmlElement(RawIntelliZoneConfig.STR_IntelliZone)]
		public IntelliZoneData IZData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawIntelliZoneConfig.STR_IntelliZone)]
	public class IntelliZoneData
	{
		[XmlArray(RawIntelliZoneConfig.STR_Channels)]
		[XmlArrayItem(RawIntelliZoneConfig.STR_Channel)]
		public List<IZChannelInfo> Channels = new List<IZChannelInfo>();
	}

	[XmlRoot(RawIntelliZoneConfig.STR_Channel)]
	public class IZChannelInfo : IMessageEntity<tDVRIZChannels>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 ID { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_FirstPresetPosition)]
		public Int32 FirstPresetPosition { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Sensitivity)]
		public Int32 Sensitivity { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_FreshTime)]
		public Int32 FreshTime { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_IsAdjustAllCamera)]
		public Int32 IsAdjustAllCamera { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Is24Hours)]
		public Int32 Is24Hours { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_DwellTimeMovePreset)]
		public Int32 DwellTimeMovePreset { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_ActTimeStart)]
		public TimeInfo ActTimeStart { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_ActTimeEnd)]
		public TimeInfo ActTimeEnd { get; set; }

		[XmlArray(RawIntelliZoneConfig.STR_Trackers)]
		[XmlArrayItem(RawIntelliZoneConfig.STR_Tracker)]
		public List<IZTrackerInfo> Trackers = new List<IZTrackerInfo>();

		public bool Equal(tDVRIZChannels value)
		{
			bool result = //value.KChannel == kchan &&
						  value.Enable == (Enable != 0) &&
						  value.IsAdjustAllCamera == (IsAdjustAllCamera != 0) &&
						  value.Is24Hour == (Is24Hours != 0) &&
						  value.StartPresetPosition == FirstPresetPosition &&
						  value.Sensitivity == Sensitivity &&
						  value.FreshTime == FreshTime &&
						  value.DwellTimeMovePreset == DwellTimeMovePreset;
			if (ActTimeStart != null)
			{
				result = result &&
						 value.StartHour == ActTimeStart.Hour &&
						 value.StartMinute == ActTimeStart.Minute &&
						 value.StartSecond == ActTimeStart.Second;
			}

			if (ActTimeEnd != null)
			{
				result = result &&
						 value.EndHour == ActTimeEnd.Hour &&
						 value.EndMinute == ActTimeEnd.Minute &&
						 value.EndSecond == ActTimeEnd.Second;
			}
			return result;
		}

		public void SetEntity(ref tDVRIZChannels value)
		{
			if (value == null)
				value = new tDVRIZChannels();
			//value.KChannel = kchan;
			value.Enable = Enable != 0;
			value.StartPresetPosition = FirstPresetPosition;
			value.Sensitivity = Sensitivity;
			value.FreshTime = FreshTime;
			value.IsAdjustAllCamera = IsAdjustAllCamera != 0;
			value.Is24Hour = Is24Hours != 0;
			value.DwellTimeMovePreset = DwellTimeMovePreset;
			if (ActTimeStart != null)
			{
				value.StartHour = ActTimeStart.Hour;
				value.StartMinute = ActTimeStart.Minute;
				value.StartSecond = ActTimeStart.Second;
			}

			if (ActTimeEnd != null)
			{
				value.EndHour = ActTimeEnd.Hour;
				value.EndMinute = ActTimeEnd.Minute;
				value.EndSecond = ActTimeEnd.Second;
			}
		}
	}

	[XmlRoot(RawIntelliZoneConfig.STR_Tracker)]
	public class IZTrackerInfo : IMessageEntity<tDVRIZTrackers>
	{
		[XmlElement(RawIntelliZoneConfig.STR_PercentChange)]
		public Int32 PercentChange { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_PresetName)]
		public string PresetName { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_PosX)]
		public Int32 PosX { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_PosY)]
		public Int32 PosY { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Width)]
		public Int16 Width { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Height)]
		public Int16 Height { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_PresetID)]
		public Int16 PresetID { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_DwellTime)]
		public Int32 DwellTime { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_Priority)]
		public Int32 Priority { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_ControlOutput)]
		public Int16 ControlOutput { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_RelativeCamID)]
		public Int16 RelativeCamID { get; set; }

		[XmlElement(RawIntelliZoneConfig.STR_RelativePresetID)]
		public Int16 RelativePresetID { get; set; }

		public bool Equal(tDVRIZTrackers value)
		{
			bool result =// izTracker.KChannel == kchan &&
					  value.PercentOfChanges == PercentChange &&
					  value.AreaName == PresetName &&
					  value.StartX == PosX &&
					  value.StartY == PosY &&
					  value.Width == Width &&
					  value.Height == Height &&
					  value.PresetID == PresetID &&
					  value.DwellTime == DwellTime &&
					  value.Priority == Priority &&
					  value.ControlOutput == ControlOutput &&
					  value.RelativeCameraID == RelativeCamID &&
					  value.RelativeResetID == RelativePresetID;
			return result;
		}

		public void SetEntity(ref tDVRIZTrackers value)
		{
			if (value == null)
				value = new tDVRIZTrackers();
			value.PercentOfChanges = PercentChange;
			value.AreaName = PresetName;
			value.StartX = PosX;
			value.StartY = PosY;
			value.Width = Width;
			value.Height = Height;
			value.PresetID = PresetID;
			value.DwellTime = DwellTime;
			value.Priority = Priority;
			value.ControlOutput = ControlOutput;
			value.RelativeCameraID = RelativeCamID;
			value.RelativeResetID = RelativePresetID;
		}
	}
	#endregion
}
