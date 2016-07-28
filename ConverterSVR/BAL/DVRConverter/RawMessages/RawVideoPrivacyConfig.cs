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
	public class RawVideoPrivacyConfig : RawDVRConfig<RawVideoPrivacyBody>
	{
		#region Parameter
		public const string STR_VideoPrivacy = "video_privacy";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Enable = "enable";
		public const string STR_VideoSource = "video_source";
		public const string STR_Trackers = "trackers";
		public const string STR_Tracker = "tracker";
		public const string STR_UserName = "user_name";
		public const string STR_MaskType = "mask_type";
		public const string STR_MaskColor = "mask_color";
		public const string STR_RepeatType = "repeat_type";
		public const string STR_Polygon = "polygon";
		public const string STR_NumPoint = "num_point";
		public const string STR_Type = "type";
		public const string STR_BeginDate = "begin_date";
		public const string STR_EndDate = "end_date";
		public const string STR_BeginTime = "begin_time";
		public const string STR_EndTime = "end_time";
		//List<tDVRVPTracker> _trackerList;
		//List<tDVRVPPoint> _vpPointList;
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);
			db.Query<tDVRChannels>(item => item.KDVR == dvrinfo.KDVR).Include(t => t.tDVRVPChannels).Include(x => x.tDVRVPTrackers);
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

			List<int> channels = DVRAdressBook.tDVRChannels.Select(t => t.KChannel).ToList();
			IEnumerable<tDVRVPChannels> vcchannelList = db.Query<tDVRVPChannels>(t => channels.Contains(t.KChannel));
			if (UpdateVideoPrivacies(vcchannelList,msgBody.VPData.Channels))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_VIDEO_PRIVACY, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateVideoPrivacies(IEnumerable<tDVRVPChannels> channels, List<VPChannelInfo> channelInfoList)
		{
			if (msgBody == null || msgBody.VPData == null || msgBody.VPData.Channels == null)
				return false;

			//Join 2 list to 1 Object list with samme channel ID
			bool result = false;
			var updates = from dvrChannel in channels
						  from dvrChannelInfo in channelInfoList
						  where dvrChannel.tDVRChannels != null && dvrChannelInfo.id == dvrChannel.tDVRChannels.ChannelNo
						  select new { DVRItem = dvrChannel, InfoItem = dvrChannelInfo };

			//Update Object list above
			tDVRVPChannels tvpChannel = null;
			foreach (var item in updates)
			{
				if (!item.InfoItem.Equal(item.DVRItem))
				{
					tvpChannel = item.DVRItem;
					item.InfoItem.SetEntity(ref tvpChannel);
					db.Update<tDVRVPChannels>(tvpChannel);
					result = true;
				}
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.KChannel == item.DVRItem.KChannel);
				result |= UpdateVpTrackers(item.InfoItem, channel);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRVPChannels> deletes = channels.Except(updates.Select(item => item.DVRItem)).ToList();
			foreach (tDVRVPChannels delete in deletes)
			{
				DeleteVpChannels(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<VPChannelInfo> newitems = channelInfoList.Except(updates.Select(item => item.InfoItem));
			foreach (VPChannelInfo newitem in newitems)
			{
				tvpChannel = new tDVRVPChannels();
				newitem.SetEntity(ref tvpChannel);
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.ChannelNo == newitem.id);
				tvpChannel.KChannel = channel.KChannel;
				db.Insert<tDVRVPChannels>(tvpChannel);
				InsertVpTrackers(newitem, channel);
				result = true;
			}

			return result;
		}

		private bool InsertVpTrackers(VPChannelInfo vpInfo, tDVRChannels channel)
		{
			bool ret = false;
			foreach (var vcAreaInfo in vpInfo.Trackers)
			{
				var tdvrTracker = new tDVRVPTrackers { KChannel = channel.KChannel };
				vcAreaInfo.SetEntity(ref tdvrTracker);
				db.Insert<tDVRVPTrackers>(tdvrTracker);
				InsertVpPoints(tdvrTracker, vcAreaInfo.Polygon.Points);
				ret = true;
			}
			return ret;
		}

		private void DeleteVpChannels(tDVRVPChannels vpChannel)
		{
			List<tDVRVPTrackers> lsTrackers = db.Query<tDVRVPTrackers>(x => x.KChannel == vpChannel.KChannel).ToList();
			//db.DeleteWhere<tDVRVPPoint>(t => lsTrackers.Contains(t.tDVRVPTracker));
			foreach (var tracker in lsTrackers)
			{
				tDVRVPTrackers vptracker = tracker;
				db.DeleteWhere<tDVRVPPoints>(t => t.KVPTracker == vptracker.KVPTracker);
			}
			db.DeleteWhere<tDVRVPTrackers>(t => t.KChannel == vpChannel.KChannel);
			db.Delete<tDVRVPChannels>(vpChannel);
		}

		private bool SetVpPoints(tDVRVPTrackers tracker, List<Point> lsPoints)
		{
			//var vlPoints = db.Query<tDVRVPPoint>(x => x.KVPTracker == tracker.KVPTracker);
			db.Include<tDVRVPTrackers, tDVRVPPoints>(tracker, t => t.tDVRVPPoints);
			Func<tDVRVPPoints, Point, bool> func_filter = (dbitem, info) => dbitem.VPPointIndex == info.ID;
			Func<tDVRVPPoints, Point, bool> compare_update = null;
			Expression<Func<tDVRVPPoints, object>> updatedata = item => item.tDVRVPTrackers;

			Expression<Func<tDVRVPPoints, int>> db_key = dbitem => dbitem.VPPointIndex;
			Expression<Func<Point, int>> info_key = info => info.ID;
			return base.UpdateDBData<tDVRVPPoints, Point, int, int>(tracker.tDVRVPPoints, lsPoints, func_filter, compare_update,
				updatedata, tracker, db_key, info_key);
		}

		private bool InsertVpPoints(tDVRVPTrackers tracker, List<Point> lsPoints)
		{
			bool ret = false;
			foreach (var p in lsPoints)
			{
				var point = new tDVRVPPoints { tDVRVPTrackers = tracker, VPPointIndex = p.ID, x = p.x, y = p.y };
				db.Insert<tDVRVPPoints>(point);
				ret = true;
			}
			return ret;
		}

		private bool UpdateVpTrackers(VPChannelInfo infoItem, tDVRChannels channel)
		{
			db.Include<tDVRChannels, tDVRVPTrackers>(channel, t => t.tDVRVPTrackers);
			var tTrackers = channel.tDVRVPTrackers;
			var tTrackerInfo = infoItem.Trackers;
			//Join 2 list to 1 Object list with samme channel ID
			bool result = false;
			var updates = from trackers in tTrackers
						  from trackerInfo in tTrackerInfo
						  where trackerInfo.id == trackers.TrackerNo
						  select new { Item = trackers, InfoItem = trackerInfo };

			//Update Object list above
			tDVRVPTrackers tvpChannel;
			foreach (var item in updates)
			{
				if (!item.InfoItem.Equal(item.Item))
				{
					tvpChannel = item.Item;
					item.InfoItem.SetEntity(ref tvpChannel);
					db.Update<tDVRVPTrackers>(tvpChannel);
					result = true;
				}

				result |= SetVpPoints(item.Item, item.InfoItem.Polygon.Points);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRVPTrackers> deletes = tTrackers.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRVPTrackers delete in deletes)
			{
				DeleteVpTrackers(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<VPTrackerInfo> newitems = tTrackerInfo.Except(updates.Select(item => item.InfoItem));
			foreach (VPTrackerInfo newitem in newitems)
			{
				tvpChannel = new tDVRVPTrackers();
				newitem.SetEntity(ref tvpChannel);
				tvpChannel.KChannel = channel.KChannel;
				db.Insert<tDVRVPTrackers>(tvpChannel);
				result |= InsertVpPoints(tvpChannel, newitem.Polygon.Points);
				result = true;
			}

			return result;
		}

		private void DeleteVpTrackers(tDVRVPTrackers delete)
		{
			db.DeleteWhere<tDVRVPPoints>(t => t.KVPTracker == delete.KVPTracker);
			db.Delete<tDVRVPTrackers>(delete);
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawVideoPrivacyBody
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
		[XmlElement(RawVideoPrivacyConfig.STR_VideoPrivacy)]
		public RawVideoPrivacyData VPData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawVideoPrivacyConfig.STR_VideoPrivacy)]
	public class RawVideoPrivacyData
	{
		[XmlArray(RawVideoPrivacyConfig.STR_Channels)]
		[XmlArrayItem(RawVideoPrivacyConfig.STR_Channel)]
		public List<VPChannelInfo> Channels { get; set; }
	}

	[XmlRoot(RawVideoPrivacyConfig.STR_Channel)]
	public class VPChannelInfo : IMessageEntity<tDVRVPChannels>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlArray(RawVideoPrivacyConfig.STR_Trackers)]
		[XmlArrayItem(RawVideoPrivacyConfig.STR_Tracker)]
		public List<VPTrackerInfo> Trackers { get; set; }

		public bool Equal(tDVRVPChannels value)
		{
			bool temp = Enable != 0;
			bool result = //value.KChannel == kChan &&
						  value.Enable == temp;
			if (Trackers != null)
			{
				result = result &&
						 value.NumberOfTracker == (short)Trackers.Count;
			}
			return result;
		}

		public void SetEntity(ref tDVRVPChannels value)
		{
			if (value == null)
				value = new tDVRVPChannels();
			//value.KChannel = kChan;
			value.Enable = Enable != 0;
			if (Trackers != null)
			{
				value.NumberOfTracker = (short)Trackers.Count;
			}
		}
	}

	[XmlRoot(RawVideoPrivacyConfig.STR_Tracker)]
	public class VPTrackerInfo : IMessageEntity<tDVRVPTrackers>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_MaskType)]
		public Int32 MaskType { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_MaskColor)]
		public Int32 MaskColor { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_RepeatType)]
		public Int32 RepeatType { get; set; }

		private PolygonInfo _polygonInfo;
		[XmlElement(RawVideoPrivacyConfig.STR_Polygon)]
		public PolygonInfo Polygon {
			get { return _polygonInfo; }
			set
			{
				_polygonInfo = value;
				_polygonInfo.SetPoints();
			}
		}

		[XmlElement(RawVideoPrivacyConfig.STR_BeginDate)]
		public DateInfo BeginDate { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_EndDate)]
		public DateInfo EndDate { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_BeginTime)]
		public TimeInfo BeginTime { get; set; }

		[XmlElement(RawVideoPrivacyConfig.STR_EndTime)]
		public TimeInfo EndTime { get; set; }

		public DateTime BeginTimeFull
		{
			get
			{
				if (BeginDate == null || BeginTime == null)
					return DateTime.MinValue;
				return new DateTime(BeginDate.Year, BeginDate.Month, BeginDate.Day, BeginTime.Hour, BeginTime.Minute, BeginTime.Second);
			}
		}

		public DateTime EndTimeFull
		{
			get
			{
				if (EndDate == null || EndTime == null)
					return DateTime.MinValue;
				return new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hour, EndTime.Minute, EndTime.Second);
			}
		}

		public bool Equal(tDVRVPTrackers value)
		{
			bool result = value.UserName == UserName &&
						  value.MaskType == MaskType &&
						  value.MaskColor == MaskColor &&
						  value.TrackerNo == (short?)id &&
						  value.RepeatType == RepeatType;
			if (BeginDate != null)
			{
				result = result &&
						 value.BeginYear == BeginDate.Year &&
						 value.BeginMonth == BeginDate.Month &&
						 value.BeginDay == BeginDate.Day &&
						 value.BeginDayOfWeek == BeginDate.DayOfWeek;
			}
			if (EndDate != null)
			{
				result = result &&
						 value.EndYear == EndDate.Year &&
						 value.EndMonth == EndDate.Month &&
						 value.EndDay == EndDate.Day &&
						 value.EndDayOfWeek == EndDate.DayOfWeek;
			}

			if(BeginTimeFull != DateTime.MinValue) //if (BeginTime != null)
			{
				result = result && value.BeginTime == BeginTimeFull;//BeginTime.Value;
			}

			if(EndTimeFull != DateTime.MinValue)//if (EndTime != null)
			{
				result = result && value.EndTime == EndTimeFull;//EndTime.Value;
			}
			if (Polygon != null)
			{
				result = result && value.TrackerType == Polygon.Type && value.NumberOfPoint == Polygon.NumPoint;
			}
			return result;
		}

		public void SetEntity(ref tDVRVPTrackers value)
		{
			if (value == null)
				value = new tDVRVPTrackers();
			value.UserName = UserName;
			value.MaskType = MaskType;
			value.MaskColor = MaskColor;
			value.RepeatType = RepeatType;
			value.TrackerNo = (short?)id;
			if (BeginDate != null)
			{
				value.BeginYear = BeginDate.Year;
				value.BeginMonth = BeginDate.Month;
				value.BeginDay = BeginDate.Day;
				value.BeginDayOfWeek = BeginDate.DayOfWeek;
			}
			if (EndDate != null)
			{
				value.EndYear = EndDate.Year;
				value.EndMonth = EndDate.Month;
				value.EndDay = EndDate.Day;
				value.EndDayOfWeek = EndDate.DayOfWeek;
			}

			value.BeginTime = (BeginTimeFull == DateTime.MinValue) ? null : (DateTime?)BeginTimeFull;
			//if (BeginTime != null)
			//{
			//	value.BeginTime = BeginTime.Value;
			//}

			value.EndTime = (EndTimeFull == DateTime.MinValue) ? null : (DateTime?)EndTimeFull;
			//if (EndTime != null)
			//{
			//	value.EndTime = EndTime.Value;
			//}
			if (Polygon != null)
			{
				value.TrackerType = Polygon.Type;
				value.NumberOfPoint = Polygon.NumPoint;
			}
		}
	}
	#endregion
}
