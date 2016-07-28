using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawVisionCountConfig : RawDVRConfig<RawVisionCountBody>
	{
		#region paramXML
		public const string STR_VisionCount = "vision_count";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Enable = "enable";
		public const string STR_VideoSource = "video_source";
		public const string STR_ResolutionWidth = "resolution_width";
		public const string STR_ResolutionHeight = "resolution_height";
		public const string STR_FunctionType = "function_type";
		public const string STR_EnvType = "env_type";
		public const string STR_BeginTime = "begin_time";
		public const string STR_EndTime = "end_time";
		public const string STR_ControlOut = "control_out";
		public const string STR_Trackers = "trackers";
		public const string STR_Areas = "areas";
		public const string STR_Area = "area";
		public const string STR_UserName = "user_name";
		public const string STR_TransactionChannel = "transaction_channel";
		public const string STR_AlarmWithoutCondition = "alarm_without_condition";
		public const string STR_AlarmConditionMinute = "alarm_condition_minute";
		public const string STR_AlarmConditionSecond = "alarm_condition_second";
		public const string STR_RegionType = "region_type";
		public const string STR_Polygon = "polygon";
		public const string STR_Doors = "doors";
		public const string STR_Door = "door";
		//public const string STR_TransactionChannel = "transaction_channel";
		//public const string STR_alarmWithoutCondition = "alarm_without_condition";
		//public const string STR_AlarmConditionMinute = "alarm_condition_minute";
		//public const string STR_AlarmConditionSecond = "alarm_condition_second";
		//public const string STR_UserName = "user_name";
		public const string STR_Polyline = "polyline";

		public const string STR_Schedules = "schedules";
		public const string STR_Schedule = "schedule";
		public const string STR_Name = "name";
		public const string STR_Time = "time";
		public const string STR_RotationType = "rotationType";
		public const string STR_Size = "size";
		public const string STR_DayRecords = "day_records";
		public const string STR_Begin = "begin";
		public const string STR_End = "end";
		public const string STR_Type = "type";

		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);
			db.Query<tDVRChannels>(item => item.KDVR == dvrinfo.KDVR).Include(t => t.tDVRVCChannels).Include(x => x.tDVRVCTrackers);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.VCData == null || msgBody.VCData.Channels == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			List<int> channels = DVRAdressBook.tDVRChannels.Select(t => t.KChannel).ToList();
			IEnumerable<tDVRVCChannels> vcchannelList = db.Query<tDVRVCChannels>(t => channels.Contains(t.KChannel));
			if (UpdateVisionCount(vcchannelList, msgBody.VCData.Channels))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_VISION_COUNT, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		private bool UpdateVisionCount(IEnumerable<tDVRVCChannels> channels, List<VCChannelInfo> channelInfoList)
		{

			//Join 2 list to 1 Object list with samme channel ID
			bool result = false;
			var updates = from dvrChannel in channels
						  from dvrChannelInfo in channelInfoList
						  where dvrChannel.tDVRChannels != null && dvrChannelInfo.id == dvrChannel.tDVRChannels.ChannelNo
						  select new { DVRItem = dvrChannel, InfoItem = dvrChannelInfo };
			
			//Update Object list above
			tDVRVCChannels tvcChannel = null;
			foreach (var item in updates)
			{
				tvcChannel = item.DVRItem;
				if (!item.InfoItem.Equal(item.DVRItem))
				{
					item.InfoItem.SetEntity(ref tvcChannel);
					db.Update<tDVRVCChannels>(tvcChannel);
					result = true;
				}
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.KChannel == item.DVRItem.KChannel);
				result |= UpdateVcTrackers(item.InfoItem, channel);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRVCChannels> deletes = channels.Except(updates.Select(item => item.DVRItem)).ToList();
			foreach (tDVRVCChannels delete in deletes)
			{
				DeleteVcChannels(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<VCChannelInfo> newitems = channelInfoList.Except(updates.Select(item => item.InfoItem));
			foreach (VCChannelInfo newitem in newitems)
			{
				tvcChannel = new tDVRVCChannels();
				newitem.SetEntity(ref tvcChannel);
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.ChannelNo == newitem.id);
				tvcChannel.KChannel = channel.KChannel;
				db.Insert<tDVRVCChannels>(tvcChannel);
				InsertVcTrackers(newitem, channel);
				result = true;
			}

			return result;
		}

		private void DeleteVcChannels(tDVRVCChannels vcChannel)
		{
			List<tDVRVCTrackers> lsTrackers = db.Query<tDVRVCTrackers>(x => x.KChannel == vcChannel.KChannel).ToList();
			//db.DeleteWhere<tDVRVCPoint>(t => lsTrackers.Contains(t.tDVRVCTracker));
			foreach (var tracker in lsTrackers)
			{
				tDVRVCTrackers vctracker = tracker;
				db.DeleteWhere<tDVRVCPoints>(t => t.KVCTracker == vctracker.KVCTracker);
			}
			db.DeleteWhere<tDVRVCTrackers>(t => t.KChannel == vcChannel.KChannel);
			db.Delete<tDVRVCChannels>(vcChannel);
		}

		private void InsertVcChannels()
		{
			foreach (var vcInfo in msgBody.VCData.Channels)
			{
				tDVRChannels channel = DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.ChannelNo == vcInfo.id);
				var vcChan = new tDVRVCChannels() {tDVRChannels = channel};
				vcInfo.SetEntity(ref vcChan);
				db.Insert<tDVRVCChannels>(vcChan);
				InsertVcTrackers(vcInfo, channel);
			}
		}

		private bool UpdateVcTrackers(VCChannelInfo vcInfo, tDVRChannels channel)
		{
			if (vcInfo.Trackers == null) return false;

			bool ret = false;
			//vcInfo.Trackers.GetAreaList();
			db.Include<tDVRChannels,tDVRVCTrackers>(channel,t=>t.tDVRVCTrackers);
			foreach (var dvrvcTracker in channel.tDVRVCTrackers.ToList())
			{
				if (dvrvcTracker.KVCTracker == 0) continue;
				tDVRVCTrackers tdvrTracker = dvrvcTracker;
				var vcArea = vcInfo.Trackers.Areas.FirstOrDefault(t => t.RegionType == dvrvcTracker.RegionType && t.id == dvrvcTracker.TrackerNo);
				if (vcArea != null)
				{
					if (!vcArea.Equal(tdvrTracker))
					{
						vcArea.SetEntity(ref tdvrTracker);
						db.Update<tDVRVCTrackers>(tdvrTracker);
						ret = true;
					}

					ret |= SetVcPoints(dvrvcTracker, vcArea.Polygon.Points);
					vcInfo.Trackers.Areas.Remove(vcArea);
				}
				else
				{
					tDVRVCTrackers deleteTracker = dvrvcTracker;
					db.DeleteWhere<tDVRVCPoints>(t => t.KVCTracker == deleteTracker.KVCTracker);
					db.Delete<tDVRVCTrackers>(deleteTracker);
					ret = true;
				}
			}

			ret |= InsertVcTrackers(vcInfo, channel);
			return ret;
		}

		private bool InsertVcTrackers(VCChannelInfo vcInfo, tDVRChannels vc)
		{
			if (vcInfo.Trackers.Areas == null) return false;

			bool ret = false;
			foreach (var vcAreaInfo in vcInfo.Trackers.Areas)
			{
				var tdvrTracker = new tDVRVCTrackers {tDVRChannels = vc};
				vcAreaInfo.SetEntity(ref tdvrTracker);
				db.Insert<tDVRVCTrackers>(tdvrTracker);
				//InsertVcPoints(tdvrTracker, lsPoints);
				InsertVcPoints(tdvrTracker, vcAreaInfo.Polygon.Points);
				ret = true;
			}
			return ret;
		}


		private bool InsertVcPoints(tDVRVCTrackers tracker, List<Point> lsPoints)
		{
			bool ret = false;
			foreach (var p in lsPoints)
			{
				var point = new tDVRVCPoints { tDVRVCTrackers = tracker, VCPointIndex = p.ID, x = p.x, y = p.y };
				db.Insert<tDVRVCPoints>(point);
				ret = true;
			}
			return ret;
		}

		private bool SetVcPoints(tDVRVCTrackers tracker, List<Point> lsPoints)
		{
			db.Include<tDVRVCTrackers, tDVRVCPoints>(tracker, t => t.tDVRVCPoints);
			Func<tDVRVCPoints, Point, bool> func_filter = (dbitem, info) => dbitem.VCPointIndex == info.ID;
			Func<tDVRVCPoints, Point, bool> compare_update = null;
			Expression<Func<tDVRVCPoints, object>> updatedata = item => item.tDVRVCTrackers;

			Expression<Func<tDVRVCPoints, int>> db_key = dbitem => dbitem.VCPointIndex;
			Expression<Func<Point, int>> info_key = info => info.ID;
			return base.UpdateDBData<tDVRVCPoints, Point, int, int>(tracker.tDVRVCPoints, lsPoints, func_filter, compare_update, updatedata, tracker, db_key, info_key);
			
			//foreach (var p in vlPoints)
			//{
			//	tDVRVCPoint point = p;
			//	Point pointInfo = lsPoints.FirstOrDefault(t => t.x == p.x && t.y == p.y && t.ID == p.VCPointIndex);
			//	if (pointInfo != null)
			//	{
			//		lsPoints.Remove(pointInfo);
			//	}
			//	else
			//	{
			//		db.Delete<tDVRVCPoint>(point);
			//		ret = true;
			//	}
			//}

			//ret |= InsertVcPoints(tracker, lsPoints);
			//return ret;
		}

		//private bool InsertVcPoints(tDVRVCTracker tracker, List<Point> lsPoints)
		//{
		//	bool ret = false;
		//	foreach (var p in lsPoints)
		//	{
		//		var point = new tDVRVCPoint {tDVRVCTracker = tracker, VCPointIndex = p.ID, x = p.x, y = p.y};
		//		db.Insert<tDVRVCPoint>(point);
		//		ret = true;
		//	}
		//	return ret;
		//}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawVisionCountBody
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
		[XmlElement(RawVisionCountConfig.STR_VisionCount)]
		public VisionCountData VCData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawVisionCountConfig.STR_VisionCount)]
	public class VisionCountData
	{
		[XmlArray(RawVisionCountConfig.STR_Channels)]
		[XmlArrayItem(RawVisionCountConfig.STR_Channel)]
		public List<VCChannelInfo> Channels { get; set; }
	}

	[XmlRoot(RawVisionCountConfig.STR_Trackers)]
	public class VCTrackers
	{
		[XmlArray(RawVisionCountConfig.STR_Areas)]
		[XmlArrayItem(RawVisionCountConfig.STR_Area)]
		public List<VCAreaInfo> Areas { get; set; }


		[XmlArray(RawVisionCountConfig.STR_Doors)]
		[XmlArrayItem(RawVisionCountConfig.STR_Door)]
		public List<VCDoorInfo> Doors{ get; set; }

		public int Count
		{
			get
			{
				return Areas.Count + Doors.Count;
			}
		}
	}

	[XmlRoot(RawVisionCountConfig.STR_Area)]
	public class VCAreaInfo  
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVisionCountConfig.STR_TransactionChannel)]
		public Int32 TransactionChannel { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmWithoutCondition)]
		public Int32 AlarmWithoutCondition { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmConditionMinute)]
		public Int32 AlarmConditionMinute { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmConditionSecond)]
		public Int32 AlarmConditionSecond { get; set; }

		[XmlElement(RawVisionCountConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawVisionCountConfig.STR_RegionType)]
		public Int32 RegionType { get; set; }

		private PolygonInfo _polygon;
		[XmlElement(RawVisionCountConfig.STR_Polygon)]
		public PolygonInfo Polygon
		{
			get { return _polygon; }
			set
			{
				_polygon = value;
				_polygon.SetPoints();
			}
		}

		public bool Equal(tDVRVCTrackers Value)
		{
			bool result =// Value.KChannel == kChan &&
						 // Value.TrackerNo == trackNo &&
						  Value.UserName == UserName &&
						  Value.TransactionChannel == TransactionChannel &&
						  Value.AlarmWithoutCondition == AlarmWithoutCondition &&
						  Value.AlarmConditionMinute == AlarmConditionMinute &&
						  Value.AlarmConditionSecond == AlarmConditionSecond &&
						  Value.RegionType == RegionType;

			if (Polygon != null)
			{
				result = result &&
				Value.NumOfPoint == Polygon.NumPoint &&
						 Value.VCPolyType == Polygon.Type;
			}

			return result;
		}

		public void SetEntity(ref tDVRVCTrackers value)
		{
			if (value == null)
				value = new tDVRVCTrackers();
			//value.KChannel = kChan;
			value.TrackerNo = id;
			value.UserName = UserName;
			value.TransactionChannel = TransactionChannel;
			value.AlarmWithoutCondition = AlarmWithoutCondition;
			value.AlarmConditionMinute = AlarmConditionMinute;
			value.AlarmConditionSecond = AlarmConditionSecond;
			value.RegionType = RegionType;
			if (Polygon != null)
			{
				value.NumOfPoint = Polygon.NumPoint;
				value.VCPolyType = Polygon.Type;
			}
		}
	}

	[XmlRoot(RawVisionCountConfig.STR_Door)]
	public class VCDoorInfo : IMessageEntity<tDVRVCTrackers>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVisionCountConfig.STR_TransactionChannel)]
		public Int32 TransactionChannel { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmWithoutCondition)]
		public Int32 AlarmWithoutCondition { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmConditionMinute)]
		public Int32 AlarmConditionMinute { get; set; }

		[XmlElement(RawVisionCountConfig.STR_AlarmConditionSecond)]
		public Int32 AlarmConditionSecond { get; set; }

		[XmlElement(RawVisionCountConfig.STR_UserName)]
		public string UserName { get; set; }

		private PolygonInfo _polyline;
		[XmlElement(RawVisionCountConfig.STR_Polyline)]
		public PolygonInfo Polyline {
			get { return _polyline; }
			set
			{
				_polyline = value;
				_polyline.SetPoints();
			}
		}

		public bool Equal(tDVRVCTrackers value)
		{
			bool result =// Value.KChannel == kChan &&
						 // Value.TrackerNo == trackNo &&
						  value.UserName == UserName &&
						  value.TransactionChannel == TransactionChannel &&
						  value.AlarmWithoutCondition == AlarmWithoutCondition &&
						  value.AlarmConditionMinute == AlarmConditionMinute &&
						  value.AlarmConditionSecond == AlarmConditionSecond;

			if (Polyline != null)
			{
				result = result &&
				value.NumOfPoint == Polyline.NumPoint &&
						 value.VCPolyType == Polyline.Type;
			}
			return result;
		}

		public void SetEntity(ref tDVRVCTrackers value)
		{
			if (value == null)
				value = new tDVRVCTrackers();
		//	value.KChannel = kChan;
			//value.TrackerNo = trackNo;
			value.UserName = UserName;
			value.TransactionChannel = TransactionChannel;
			value.AlarmWithoutCondition = AlarmWithoutCondition;
			value.AlarmConditionMinute = AlarmConditionMinute;
			value.AlarmConditionSecond = AlarmConditionSecond;
			value.RegionType = -1;
			if (Polyline != null)
			{
				value.NumOfPoint = Polyline.NumPoint;
				value.VCPolyType = Polyline.Type;
			}
		}
	}

	[XmlRoot(RawVisionCountConfig.STR_Channel)]
	public class VCChannelInfo : IMessageEntity<tDVRVCChannels>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawVisionCountConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlElement(RawVisionCountConfig.STR_ResolutionWidth)]
		public Int32 ResolutionWidth { get; set; }

		[XmlElement(RawVisionCountConfig.STR_ResolutionHeight)]
		public Int32 ResolutionHeight { get; set; }

		[XmlElement(RawVisionCountConfig.STR_FunctionType)]
		public Int32 FunctionType { get; set; }

		[XmlElement(RawVisionCountConfig.STR_EnvType)]
		public Int32 EnvType { get; set; }

		[XmlElement(RawVisionCountConfig.STR_BeginTime)]
		public TimeInfo BeginTime { get; set; }

		[XmlElement(RawVisionCountConfig.STR_EndTime)]
		public TimeInfo EndTime { get; set; }

		[XmlElement(RawVisionCountConfig.STR_ControlOut)]
		public Int32 ControlOut { get; set; }

		private VCTrackers _tracker;

		[XmlElement(RawVisionCountConfig.STR_Trackers)]
		public VCTrackers Trackers
		{
			get { return _tracker; }
			set
			{
				_tracker = value;
				GetAreaList();
			}
		}

		[XmlArray(RawVisionCountConfig.STR_Schedules)]
		[XmlArrayItem(RawVisionCountConfig.STR_Schedule)]
		public List<VCSchedule> Schedules { get; set; }

		private void GetAreaList()
		{
			int index = Trackers.Areas.Count;
			foreach (var door in Trackers.Doors)
			{
				var area = new VCAreaInfo();
				area.AlarmConditionMinute = door.AlarmConditionMinute;
				area.AlarmConditionSecond = door.AlarmConditionSecond;
				area.AlarmWithoutCondition = door.AlarmWithoutCondition;
				area.Polygon = door.Polyline;
				area.RegionType = -1;
				area.TransactionChannel = door.TransactionChannel;
				area.UserName = door.UserName;
				area.id = index;
				Trackers.Areas.Add(area);
				index++;
			}
		}

		public bool Equal(tDVRVCChannels value)
		{
			bool result =// Value.KChannel == kChan &&
						  value.ControlOut == ControlOut &&
						  value.NumOfTracker == Trackers.Areas.Count &&
						  value.Enable == Enable &&
						  value.VideoSource == VideoSource &&
						  value.FunctionType == FunctionType &&
						  value.EnvType == EnvType;
			if (BeginTime != null)
			{
				result = result &&
						 value.BeginTime == BeginTime.Value;
			}
			if (EndTime != null)
			{
				result = result &&
						 value.EndTime == EndTime.Value;
			}
			return result;
		}

		public void SetEntity(ref tDVRVCChannels value)
		{
			if (value == null)
				value = new tDVRVCChannels();
			//value.KChannel = kChan;
			value.Enable = Enable;
			value.VideoSource = VideoSource;
			value.FunctionType = FunctionType;
			value.EnvType = EnvType;
			if (BeginTime != null)
				value.BeginTime = BeginTime.Value;
			if (EndTime != null) 
				value.EndTime = EndTime.Value;
			value.ControlOut = ControlOut;
			value.NumOfTracker = Trackers.Areas.Count;
		}
	}

	[XmlRoot(RawVisionCountConfig.STR_Schedule)]
	public class VCSchedule
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Time)]
		public Int64 Time { get; set; }

		[XmlElement(RawVisionCountConfig.STR_RotationType)]
		public Int32 RotationType { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Size)]
		public Int32 Size { get; set; }

		[XmlArray(RawVisionCountConfig.STR_DayRecords)]
		[XmlArrayItem(RawVisionCountConfig.STR_Time)]
		public List<VCDayRecordSchedule> DayRecords { get; set; }
	}

	[XmlRoot(RawVisionCountConfig.STR_Time)]
	public class VCDayRecordSchedule
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 DayNo { get; set; }

		[XmlElement(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Time)]
		public Int64 Time { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Begin)]
		public Int64 Begin { get; set; }

		[XmlElement(RawVisionCountConfig.STR_End)]
		public Int64 End { get; set; }

		[XmlElement(RawVisionCountConfig.STR_Type)]
		public Int32 Type { get; set; }
	}
	#endregion
}
