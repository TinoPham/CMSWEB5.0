using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SVRDatabase;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawVideoLogixConfig : RawDVRConfig<RawVideoLogicBody>
	{
		#region paramXML
		public const string STR_VideoLogix = "video_logix";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_VideoSource = "video_source";
		public const string STR_ResolutionWidth = "resolution_width";
		public const string STR_ResolutionHeight = "resolution_height";
		public const string STR_Enable = "enable";
		public const string STR_HideDetectBox = "is_hide_detection_box";
		public const string STR_ControlOut = "control_out";
		public const string STR_MinObjSize = "min_object_size";
		public const string STR_EnvType = "environment_type";
		public const string STR_AreaTrackers = "area_trackers";
		public const string STR_StayTrackers = "stay_trackers";
		public const string STR_StopTrackers = "stop_trackers";
		public const string STR_MissTrackers = "miss_trackers";
		public const string STR_CrossWiresTrackers = "cross_wires_trackers";
		public const string STR_DirectionTrackers = "direction_trackers";
		public const string STR_PassThroughtTrackers = "pass_through_trackers";
		public const string STR_PolygonTracker = "polygon_tracker";
		public const string STR_LineTracker = "line_tracker";
		public const string STR_Polygon = "polygon";
		public const string STR_NumPoint = "num_point";
		public const string STR_Type = "type";
		//public const string STR_Name = "name";
		public const string STR_UserName = "user_name";
		public const string STR_ObjectType = "object_type";
		public const string STR_AlarmType = "alarm_type";
		public const string STR_BeginTime = "begin_time";

		//public const string STR_Hour = "hour";
		//public const string STR_Minute = "minute";
		//public const string STR_Second = "second";

		public const string STR_EndTime = "end_time";
		public const string STR_SendMail = "is_send_mail";
		public const string STR_AudioDial = "is_audio_dial_out";
		public const string STR_WaveFile = "wave_file";
		public const string STR_SoundDuration = "sound_duration";
		public const string STR_Condition = "condition";
		public const string STR_ConditionValue = "condition_value";
		public const string STR_IdleTime = "idle_time";
		public const string STR_MissAlarmCondition = "miss_alarm_condition";

		public const string STR_Line = "line";
		public const string STR_Extra1 = "extra1";
		public const string STR_Extra2 = "extra2";
		public const string STR_StartPointX = "start_point_x";
		public const string STR_StartPointY = "start_point_y";
		public const string STR_EndPointX = "end_point_x";
		public const string STR_EndPointY = "end_point_y";

		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Query<tDVRChannels>(item => item.KDVR == dvrinfo.KDVR).Include(t => t.tDVRVLChannels).Include(x => x.tDVRVLTrackers);
			db.Include<tDVRAddressBook,tDVRChannels>(DVRAdressBook,t=>t.tDVRChannels);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.VLData == null || msgBody.VLData.Channels == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			List<int> channels = DVRAdressBook.tDVRChannels.Select(t => t.KChannel).ToList();
			IEnumerable<tDVRVLChannels> vcchannelList = db.Query<tDVRVLChannels>(t => channels.Contains(t.KChannel));
			if (UpdateVideoLogic(vcchannelList, msgBody.VLData.Channels))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_VIDEO_LOGIX, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateVideoLogic(IEnumerable<tDVRVLChannels> channels, List<VLChannelInfo> channelInfoList)
		{

			//Join 2 list to 1 Object list with samme channel ID
			bool result = false;
			var updates = from dvrChannel in channels
						  from dvrChannelInfo in channelInfoList
						  where dvrChannel.tDVRChannels != null && dvrChannelInfo.ID == dvrChannel.tDVRChannels.ChannelNo
						  select new { DVRItem = dvrChannel, InfoItem = dvrChannelInfo };

			//Update Object list above
			tDVRVLChannels tvcChannel = null;
			foreach (var item in updates)
			{
				tvcChannel = item.DVRItem;
				item.InfoItem.GetAllTrackers();
				if (!item.InfoItem.Equal(item.DVRItem))
				{
					item.InfoItem.SetEntity(ref tvcChannel);
					db.Update<tDVRVLChannels>(tvcChannel);
					result = true;
				}
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.KChannel == item.DVRItem.KChannel);
				result |= UpdateVlTrackers(item.InfoItem, channel);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRVLChannels> deletes = channels.Except(updates.Select(item => item.DVRItem)).ToList();
			foreach (tDVRVLChannels delete in deletes)
			{
				DeleteVcChannels(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<VLChannelInfo> newitems = channelInfoList.Except(updates.Select(item => item.InfoItem));
			foreach (VLChannelInfo newitem in newitems)
			{
				tvcChannel = new tDVRVLChannels();
				newitem.GetAllTrackers();
				newitem.SetEntity(ref tvcChannel);
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.ChannelNo == newitem.ID);
				tvcChannel.KChannel = channel.KChannel;
				db.Insert<tDVRVLChannels>(tvcChannel);
				InsertVlTrackers(newitem, channel);
				result = true;
			}

			return result;
		}

		private void DeleteVcChannels(tDVRVLChannels vcChannel)
		{
			List<tDVRVLTrackers> lsTrackers = db.Query<tDVRVLTrackers>(x => x.KChannel == vcChannel.KChannel).ToList();
			//db.DeleteWhere<tDVRVLPoint>(t => lsTrackers.Contains(t.tDVRVLTracker));	
			foreach (var tracker in lsTrackers)
			{
				tDVRVLTrackers vltracker = tracker;
				db.DeleteWhere<tDVRVLPoints>(t => t.KVLTracker == vltracker.KVLTracker);
			}
			db.DeleteWhere<tDVRVLTrackers>(t => t.KChannel == vcChannel.KChannel);
			db.Delete<tDVRVLChannels>(vcChannel);
		}



		//private bool UpdateVideoLogic(IEnumerable<tDVRVLChannel> channels, List<VLChannelInfo> channelInfoList)
		//{
		//	bool result = false;
		//	foreach (var vlchannel in DVRAdressBook.tDVRChannels.Where(t=>t.tDVRVLChannel != null).ToList())
		//	{
		//		tDVRVLChannel vlChannel = vlchannel.tDVRVLChannel;
		//		VLChannelInfo vlInfo = msgBody.VLData.Channels.FirstOrDefault(t => t.ID == vlchannel.ChannelNo);
		//		if (vlInfo != null)
		//		{
		//			vlInfo.GetAllTrackers();
		//			if (!vlInfo.Equal(vlChannel))
		//			{
		//				vlInfo.SetEntity(ref vlChannel);
		//				db.Update<tDVRVLChannel>(vlChannel);
		//				result = true;
		//			}
		//			result |= UpdateVlTrackers(vlInfo, vlchannel);
		//			msgBody.VLData.Channels.Remove(vlInfo);
		//		}
		//		else
		//		{
		//			result |= DeleteVlChannels(vlChannel);
		//		}
		//	}

		//	result |= InsertVlChannels();
		//	return result;
		//}

		private bool InsertVlChannels()
		{
			bool ret = false;
			foreach (var vlInfo in msgBody.VLData.Channels)
			{
				tDVRChannels channel = DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.ChannelNo == vlInfo.ID);
				var vlChan = new tDVRVLChannels() {tDVRChannels = channel};
				//vlInfo.GetAllTrackers();
				vlInfo.SetEntity(ref vlChan);
				db.Insert<tDVRVLChannels>(vlChan);
				InsertVlTrackers(vlInfo, channel);
				ret = true;
			}
			return ret;
		}

		private bool InsertVlTrackers(VLChannelInfo vlInfo, tDVRChannels vlchannel)
		{
			bool ret = false;
		
			foreach (var vlAreaInfo in vlInfo.PolygonTrackers)
			{
				var tdvrTracker = new tDVRVLTrackers { KChannel = vlchannel.KChannel };
				vlAreaInfo.SetEntity(ref tdvrTracker);
				db.Insert<tDVRVLTrackers>(tdvrTracker);
				ret = true;
				List<Point> lsPoints = vlAreaInfo.Polygon.Points;				
				//InsertVlPoints(tdvrTracker, lsPoints);
				ret |= InsertVlPoints(tdvrTracker, lsPoints);
			}
			return ret;
		}

		private bool InsertVlPoints(tDVRVLTrackers tracker, List<Point> lsPoints)
		{
			bool ret = false;
			foreach (var p in lsPoints)
			{
				var point = new tDVRVLPoints { tDVRVLTrackers = tracker, VLPointIndex = p.ID, x = p.x, y = p.y };
				db.Insert<tDVRVLPoints>(point);
				ret = true;
			}
			return ret;
		}

		//private bool DeleteVlChannels(tDVRVLChannel vlChannel)
		//{
		//	List<tDVRVLTracker> lsTrackers = db.Query<tDVRVLTracker>(x => x.KChannel == vlChannel.KChannel).ToList();
		//	db.DeleteWhere<tDVRVLPoint>(t => lsTrackers.Contains(t.tDVRVLTracker));
		//	db.DeleteWhere<tDVRVLTracker>(t => t.KChannel == vlChannel.KChannel);
		//	db.Delete<tDVRVLChannel>(vlChannel);
		//	return true;
		//}

		private bool UpdateVlTrackers(VLChannelInfo vlInfo, tDVRChannels vlChannel)
		{
			bool ret = false;
			db.Include<tDVRChannels, tDVRVLTrackers>(vlChannel, t => t.tDVRVLTrackers);
			foreach (var dvrvlTracker in vlChannel.tDVRVLTrackers.ToList())
			{
				if (dvrvlTracker.KVLTracker == 0) continue;
				tDVRVLTrackers tdvrTracker = dvrvlTracker;
				var vlArea = vlInfo.PolygonTrackers.FirstOrDefault(t => t.Name.Trim() == dvrvlTracker.DefaultName.Trim());
				if (vlArea != null)
				{
					if (!vlArea.Equal(tdvrTracker))
					{
						vlArea.SetEntity(ref tdvrTracker);
						db.Update<tDVRVLTrackers>(tdvrTracker);
						ret = true;
					}

					List<Point> lsPoints = vlArea.Polygon.Points;
					ret |= SetVlPoints(dvrvlTracker, lsPoints);
					vlInfo.PolygonTrackers.Remove(vlArea);
				}
				else
				{
					tDVRVLTrackers deleteTracker = dvrvlTracker;
					db.DeleteWhere<tDVRVLPoints>(t => t.KVLTracker == deleteTracker.KVLTracker);
					db.Delete<tDVRVLTrackers>(deleteTracker);
					ret = true;
				}
			}

			ret |= InsertVlTrackers(vlInfo, vlChannel);
			return ret;
		}

		private bool SetVlPoints(tDVRVLTrackers tracker, List<Point> lsPoints)
		{
			db.Include<tDVRVLTrackers,tDVRVLPoints>(tracker,t=>t.tDVRVLPoints);
			Func<tDVRVLPoints, Point, bool> func_filter = (dbitem, info) => dbitem.VLPointIndex == info.ID;
			Func<tDVRVLPoints, Point, bool> compare_update = null;
			Expression<Func<tDVRVLPoints, object>> updatedata = item => item.tDVRVLTrackers;

			Expression<Func<tDVRVLPoints, int>> db_key = dbitem => dbitem.VLPointIndex;
			Expression<Func<Point, int>> info_key = info => info.ID;
			return base.UpdateDBData<tDVRVLPoints, Point, int, int>(tracker.tDVRVLPoints, lsPoints, func_filter, compare_update, updatedata, tracker, db_key, info_key);

			//var vlPoints = db.Query<tDVRVLPoint>(x => x.KVLTracker == tracker.);
			//foreach (var p in vlPoints)
			//{
			//	tDVRVLPoint point = p;
			//	Point pointInfo = lsPoints.FirstOrDefault(t => t.x == p.x && t.y == p.y && t.ID == p.VLPointIndex);
			//	if (pointInfo != null)
			//	{
			//		lsPoints.Remove(pointInfo);
			//	}
			//	else
			//	{
			//		db.Delete<tDVRVLPoint>(point);
			//	}
			//}

			//InsertVlPoints(tracker, lsPoints);
		}

		//private void InsertVlPoints(tDVRVLTracker tracker, List<Point> lsPoints)
		//{
		//	foreach (var p in lsPoints)
		//	{
		//		var point = new tDVRVLPoint { tDVRVLTracker = tracker, VLPointIndex = p.ID, x = p.x, y = p.y };
		//		db.Insert<tDVRVLPoint>(point);
		//	}
		//}
	}

	#region Class for Video Logix
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawVideoLogicBody
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
		[XmlElement(RawVideoLogixConfig.STR_VideoLogix)]
		public RawVideoLogixData VLData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawVideoLogixConfig.STR_VideoLogix)]
	public class RawVideoLogixData
	{
		[XmlArray(RawVideoLogixConfig.STR_Channels)]
		[XmlArrayItem(RawVideoLogixConfig.STR_Channel)]
		public List<VLChannelInfo> Channels { get; set; }
	}

	[XmlRoot(RawVideoLogixConfig.STR_Channel)]
	public class VLChannelInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 ID { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ResolutionWidth)]
		public Int32 ResolutionWidth { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ResolutionHeight)]
		public Int32 ResolutionHeight { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_HideDetectBox)]
		public Int32 HideDetectBox { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ControlOut)]
		public Int32 ControlOut { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_MinObjSize)]
		public Int32 MinObjSize { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_EnvType)]
		public Int32 EnvType { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_AreaTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_PolygonTracker)]
		public List<PolygonTracker> AreaTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_StayTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_PolygonTracker)]
		public List<PolygonTracker> StayTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_StopTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_PolygonTracker)]
		public List<PolygonTracker> StopTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_MissTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_PolygonTracker)]
		public List<PolygonTracker> MissTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_CrossWiresTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_LineTracker)]
		public List<LineTracker> CrossWiresTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_DirectionTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_LineTracker)]
		public List<LineTracker> DirectionTrackers { get; set; }

		[XmlArray(RawVideoLogixConfig.STR_PassThroughtTrackers)]
		[XmlArrayItem(RawVideoLogixConfig.STR_LineTracker)]
		public List<LineTracker> PassThroughtTrackers { get; set; }

		public List<PolygonTracker> PolygonTrackers { get; set; }

		public void GetAllTrackers()
		{
			int trackerNo = 0;
			var lsAreas = new List<PolygonTracker>();
			foreach (var polygonTracker in AllAreaTrackers())
			{
				polygonTracker.id = trackerNo;
				if (polygonTracker.Polygon != null)
				{
					polygonTracker.Polygon.SetPoints();
				}
				lsAreas.Add(polygonTracker);
				trackerNo++;
			}

			foreach (var lineTracker in AllLineTrackers())
			{
				var polygonTracker = new PolygonTracker()
				{
					AlarmType = lineTracker.AlarmType,
					AudioDial = lineTracker.AudioDial,
					BeginTime = lineTracker.BeginTime,
					ConditionValue = 0,
					Condition = null,
					EndTime = lineTracker.EndTime,
					IdleTime = null,
					MissAlarmCondition = 0,
					Name = lineTracker.Name,
					ObjectType = lineTracker.ObjectType,
					Polygon = lineTracker.Line == null ? null : lineTracker.Line.GetPolygonInfo(),
					SendMail = lineTracker.SendMail,
					SoundDuration = lineTracker.SoundDuration,
					UserName = lineTracker.UserName,
					WaveFile = lineTracker.WaveFile,
					id = trackerNo//lineTracker.id
				};
				lsAreas.Add(polygonTracker);
				trackerNo++;
			}
			PolygonTrackers = lsAreas;
		}

		private List<PolygonTracker> AllAreaTrackers()
		{
			var lsAreas = new List<PolygonTracker>();
			if (AreaTrackers != null)
			{
				lsAreas.AddRange(AreaTrackers);
			}
			if (StayTrackers != null)
			{
				lsAreas.AddRange(StayTrackers);
			}
			if (StopTrackers != null)
			{
				lsAreas.AddRange(StopTrackers);
			}
			if (MissTrackers != null)
			{
				lsAreas.AddRange(MissTrackers);
			}

			return lsAreas;
		}

		private List<LineTracker> AllLineTrackers()
		{
			var lsLines = new List<LineTracker>();
			if (CrossWiresTrackers != null)
			{
				lsLines.AddRange(CrossWiresTrackers);
			}
			if (DirectionTrackers != null)
			{
				lsLines.AddRange(DirectionTrackers);
			}
			if (PassThroughtTrackers != null)
			{
				lsLines.AddRange(PassThroughtTrackers);
			}
			return lsLines;
		}

		public bool Equal(tDVRVLChannels value)
		{
			bool result = //value.KChannel == kChan &&
				value.NumberOfTracker == PolygonTrackers.Count &&
				value.EnableVL == Enable &&
				value.HideDetection == HideDetectBox &&
				value.ControlNumber == ControlOut &&
				value.MinObjectSize == MinObjSize &&
				value.OverheadCamera == EnvType &&
				value.Crowded == EnvType;
			return result;
		}

		public void SetEntity(ref tDVRVLChannels value)
		{
			if (value == null)
				value = new tDVRVLChannels();
			//value.KChannel = kChan;
			value.NumberOfTracker = PolygonTrackers.Count;
			value.EnableVL = Enable;
			value.HideDetection = HideDetectBox;
			value.ControlNumber = ControlOut;
			value.MinObjectSize = MinObjSize;
			//Chinh 11/9/2014 
			value.OverheadCamera = EnvType == 0 ? 1: 0;
			value.Crowded = EnvType == 1 ? 1 : 0;
		}
	}

	[XmlRoot(RawVideoLogixConfig.STR_PolygonTracker)]
	public class PolygonTracker
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Polygon)]
		public PolygonInfo Polygon { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ObjectType)]
		public Int32 ObjectType { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_AlarmType)]
		public Int32 AlarmType { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_BeginTime)]
		public TimeInfo BeginTime { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_EndTime)]
		public TimeInfo EndTime { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_SendMail)]
		public Int32 SendMail { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_AudioDial)]
		public Int32 AudioDial { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_WaveFile)]
		public string WaveFile { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_SoundDuration)]
		public Int32 SoundDuration { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Condition)]
		public string Condition { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ConditionValue)]
		public Int32 ConditionValue { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_IdleTime)]
		public IdleTime IdleTime { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_MissAlarmCondition)]
		public Int32 MissAlarmCondition { get; set; }

		public bool Equal(tDVRVLTrackers value)
		{
			bool result = //Value.KChannel == kChan &&
						  value.TrackerNo == Convert.ToInt16(id);
			if (Polygon != null)
			{
				result = result &&
				         value.NumberOfPoint == Polygon.NumPoint &&
				         value.VLPolyType == Polygon.Type &&
				         value.Extra1 == Polygon.Extra1 && 
						 value.Extra2 == Polygon.Extra2;
			}

			if (BeginTime != null)
			{
				result = result &&
						 value.BeginActiveTime == BeginTime.Value;
			}
			if (EndTime != null)
			{
				result = result &&
						 value.EndActiveTime == EndTime.Value;
			}
			if (IdleTime != null)
			{
				result = result &&
						 value.IdleHour == IdleTime.Hour &&
						 value.IdleMin == IdleTime.Minute &&
						 value.IdleSec == IdleTime.Second;
			}
			result = result &&
			         value.ObjectType == ObjectType &&
			         value.AlarmType == AlarmType &&
			         value.EnSendMail == SendMail &&
			         value.MissAlarm == MissAlarmCondition &&
			         value.DefaultName == Name &&
			         value.CustomizedName == UserName &&
			         value.EnAudio == AudioDial &&
			         value.SoundDuration == SoundDuration &&
			         value.SoundFile == WaveFile &&
			         value.ConditionNum == ConditionValue &&
			         value.Condition == Condition;

			return result;
		}

		public void SetEntity(ref tDVRVLTrackers value)
		{
			if (value == null)
				value = new tDVRVLTrackers();
			//value.KChannel = kChan;
			value.TrackerNo = Convert.ToInt16(id);
			if (Polygon != null)
			{
				value.NumberOfPoint = Polygon.NumPoint;
				value.VLPolyType = Polygon.Type;
				value.Extra1 = Polygon.Extra1;
				value.Extra2 = Polygon.Extra2;
			}
			value.ObjectType = ObjectType;
			value.AlarmType = AlarmType;
			if (BeginTime != null)
			{
				value.BeginActiveTime = BeginTime.Value;
			}
			if (EndTime != null)
			{
				value.EndActiveTime = EndTime.Value;
			}
			if (IdleTime != null)
			{
				value.IdleHour = IdleTime.Hour;
				value.IdleMin = IdleTime.Minute;
				value.IdleSec = IdleTime.Second;
			}
			value.EnSendMail = SendMail;
			value.MissAlarm = MissAlarmCondition;
			value.DefaultName = Name;
			value.CustomizedName = UserName;
			value.EnAudio = AudioDial;
			value.SoundDuration = SoundDuration;
			value.SoundFile = WaveFile;
			value.ConditionNum = ConditionValue;
			value.Condition = Condition;
		}
	}

	[XmlRoot(RawVideoLogixConfig.STR_Polygon)]
	public class PolygonInfo
	{
		[XmlAttribute(RawVideoLogixConfig.STR_NumPoint)]
		public Int32 NumPoint { get; set; }

		[XmlAttribute(RawVideoLogixConfig.STR_Type)]
		public Int16 Type { get; set; }

		public Int32 Extra1 { get; set; }

		public Int32 Extra2 { get; set; }

		public List<Point> Points { get; set; }

		[XmlText]
		public string Data { get; set; }

		public List<Point> GetPoints()
		{
			var lsPoints = new List<Point>();
			RawVideoLogixConfig.GetPoints(lsPoints, Data, NumPoint);
			return lsPoints;
		}

		public void SetPoints()
		{
			var lsPoints = new List<Point>();
			 RawVideoLogixConfig.GetPoints(lsPoints, Data, NumPoint);
			Points = lsPoints;
		}
	}

	[XmlRoot(RawVideoLogixConfig.STR_LineTracker)]
	public class LineTracker
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Line)]
		public LineInfo Line { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_ObjectType)]
		public Int32 ObjectType { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_AlarmType)]
		public Int32 AlarmType { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_BeginTime)]
		public TimeInfo BeginTime { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_EndTime)]
		public TimeInfo EndTime { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_SendMail)]
		public Int32 SendMail { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_AudioDial)]
		public Int32 AudioDial { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_WaveFile)]
		public string WaveFile { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_SoundDuration)]
		public Int32 SoundDuration { get; set; }
	}

	[XmlRoot(RawVideoLogixConfig.STR_Line)]
	public class LineInfo
	{
		[XmlAttribute(RawVideoLogixConfig.STR_Type)]
		public Int16 Type { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Extra1)]
		public Int32 Extra1 { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_Extra2)]
		public Int32 Extra2 { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_StartPointX)]
		public Int32 StartPointX { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_StartPointY)]
		public Int32 StartPointY { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_EndPointX)]
		public Int32 EndPointX { get; set; }

		[XmlElement(RawVideoLogixConfig.STR_EndPointY)]
		public Int32 EndPointY { get; set; }

		private List<Point> GetPointList()
		{
			var lsPoints = new List<Point>();
			lsPoints.Add(new Point(StartPointX, StartPointY,0));
			lsPoints.Add(new Point(EndPointX, EndPointY,1));
			return lsPoints;
		}

		public PolygonInfo GetPolygonInfo()
		{
			var polygon = new PolygonInfo()
			{
				Type = this.Type,
				Extra1 = this.Extra1,
				Extra2 = this.Extra2,
				NumPoint = 2,
				Points = GetPointList()
			};
			return polygon;
		}
	}
	#endregion
}
