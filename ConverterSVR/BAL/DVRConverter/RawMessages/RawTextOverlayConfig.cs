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
	public class RawTextOverlayConfig : RawDVRConfig<RawTextOverlayBody>
	{
		#region Parameter
		public const string STR_TextOverlay = "text_overlay";
		public const string STR_EnCollector = "en_collector";
		public const string STR_EnEmail = "en_email";
		public const string STR_IntervalEmail = "interval_email";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_ScrollDelay = "scroll_delay";
		public const string STR_Delay = "text_delay";
		public const string STR_Disable = "disable";
		public const string STR_Brightness = "brightness";
		public const string STR_ShowColorMode = "show_color_mode";
		public const string STR_Trackers = "trackers";
		public const string STR_Tracker = "tracker";
		public const string STR_Left = "left";
		public const string STR_Top = "top";
		public const string STR_Width = "width";
		public const string STR_Height = "height";
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;

			DVRAdressBook.tDVRChannels = db.Query<tDVRChannels>(item => item.KDVR == dvrinfo.KDVR).Include(t => t.tDVRTextOverlayChannels).Include(t=>t.tDVRTextOverlayTrackers).ToList();
			DVRAdressBook.tDVRTextOverlayConfig = db.FirstOrDefault<tDVRTextOverlayConfig>(item => item.KDVR == dvrinfo.KDVR);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody == null || msgBody.TOData == null || msgBody.TOData.Channels == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateTextOverlayConfig())
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			List<int> channels = DVRAdressBook.tDVRChannels.Select(t => t.KChannel).ToList();
			IEnumerable<tDVRTextOverlayChannels> vcchannelList = db.Query<tDVRTextOverlayChannels>(t => channels.Contains(t.KChannel));
			if (UpdateTextOverlayConfigs(DVRAdressBook, msgBody.TOData.Channels, vcchannelList))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_TEXT_OVERLAY, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_TEXT_OVERLAY, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateTextOverlayConfigs(tDVRAddressBook dvrAdressBook, List<TOChannelInfo> channelInfoList, IEnumerable<tDVRTextOverlayChannels> vcchannelList)
		{
			var channels = vcchannelList;
			bool result = false;
			var updates = from dvrChannel in channels
						  from dvrChannelInfo in channelInfoList
						  where dvrChannel.tDVRChannels != null && dvrChannelInfo.id == dvrChannel.tDVRChannels.ChannelNo
						  select new { DVRItem = dvrChannel, InfoItem = dvrChannelInfo };

			//Update Object list above
			tDVRTextOverlayChannels textOverlayChannel = null;
			foreach (var item in updates)
			{
				textOverlayChannel = item.DVRItem;
				if (!item.InfoItem.Equal(item.DVRItem))
				{
					item.InfoItem.SetEntity(ref textOverlayChannel);
					db.Update<tDVRTextOverlayChannels>(textOverlayChannel);
					result = true;
				}
				tDVRChannels channel = item.DVRItem.tDVRChannels;
					//DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.KChannel == item.DVRItem.KChannel);
				result |= UpdateTextOverlayTrackers(channel, item.InfoItem);
				//result |= UpdateVlTracker(channel,item.InfoItem.Trackers);
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRTextOverlayChannels> deletes = channels.Except(updates.Select(item => item.DVRItem)).ToList();
			foreach (tDVRTextOverlayChannels delete in deletes)
			{
				db.DeleteWhere<tDVRTextOverlayTrackers>(t => t.tDVRChannels == delete.tDVRChannels);
				db.Delete<tDVRTextOverlayChannels>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<TOChannelInfo> newitems = channelInfoList.Except(updates.Select(item => item.InfoItem));
			foreach (TOChannelInfo newitem in newitems)
			{
				textOverlayChannel = new tDVRTextOverlayChannels();
				newitem.SetEntity(ref textOverlayChannel);
				tDVRChannels channel =
					DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.KDVR == dvrInfo.KDVR && t.ChannelNo == newitem.id);
				textOverlayChannel.KChannel = channel.KChannel;
				db.Insert<tDVRTextOverlayChannels>(textOverlayChannel);
				InsertTextOverlayTrackers(newitem, channel);
				//UpdateVlTracker(channel, newitem.Trackers);
				result = true;
			}

			return result;
		}

		//private bool UpdateVlTracker(tDVRChannel channel, List<TOTrackerInfo> lsInfo)
		//{
		//	db.Include<tDVRChannel, tDVRTextOverlayTracker>(channel, item => item.tDVRTextOverlayTrackers);
		//	Func<tDVRTextOverlayTracker, TOTrackerInfo, bool> func_filter = (dbitem, info) => info.Left == dbitem.StartX && info.Top == dbitem.StartY && info.Width == dbitem.Width && info.Height == dbitem.Height;
		//	Func<tDVRTextOverlayTracker, TOTrackerInfo, bool> compare_update = null;
		//	List<Expression<Func<tDVRTextOverlayTracker, object>>> update_expr = new List<Expression<Func<tDVRTextOverlayTracker, object>>>();
		//	List<object> update_data = new List<object>();
		//	update_expr.Add(dbitem => dbitem.tDVRChannel);
		//	update_data.Add(channel);

		//	Expression<Func<tDVRTextOverlayTracker, int>> db_key = dbitem => dbitem.KTracker;
		//	Expression<Func<TOTrackerInfo, int>> info_key = info => info.id;
		//	return base.UpdateDBData<tDVRTextOverlayTracker, TOTrackerInfo, int, int>(channel.tDVRTextOverlayTrackers, lsInfo, func_filter, compare_update, update_expr, update_data, db_key, info_key);
		//}
		
		private bool UpdateTextOverlayTrackers(tDVRChannels channel, TOChannelInfo channelInfo)
		{
			if (channelInfo.Trackers == null)
				return false;

			bool ret = false;
			foreach (var dvrvcTracker in channel.tDVRTextOverlayTrackers.ToList())
			{
				if (dvrvcTracker.KTracker == 0) continue;
				tDVRTextOverlayTrackers tdvrTracker = dvrvcTracker;

				var vcArea = channelInfo.Trackers.FirstOrDefault(t => t.Left == tdvrTracker.StartX && t.Top == tdvrTracker.StartY && t.Width == tdvrTracker.Width && t.Height == tdvrTracker.Height);
				if (vcArea != null)
				{
					channelInfo.Trackers.Remove(vcArea);
				}
				else
				{
					db.Delete<tDVRTextOverlayTrackers>(tdvrTracker);
					ret = true;
				}
			}

			ret |= InsertTextOverlayTrackers(channelInfo, channel);
			return ret;
		}

		private bool InsertTextOverlayTrackers(TOChannelInfo vcInfo, tDVRChannels channel)
		{
			bool ret = false;
			foreach (var trackerInfo in vcInfo.Trackers)
			{
				var toTracker = new tDVRTextOverlayTrackers() { tDVRChannels = channel };
				trackerInfo.SetEntity(ref toTracker);
				db.Insert<tDVRTextOverlayTrackers>(toTracker);
				ret = true;
			}
			return ret;
		}

		private bool UpdateTextOverlayConfig()
		{
			if (msgBody == null || msgBody.TOData == null)
				return false;

			bool result = false;
			TextOverlayData toInfo = msgBody.TOData;
			if (DVRAdressBook.tDVRTextOverlayConfig == null)
			{
				var toConfig = new tDVRTextOverlayConfig() { tDVRAddressBook = DVRAdressBook };
				toInfo.SetEntity(ref toConfig);
				db.Insert<tDVRTextOverlayConfig>(toConfig);
			}
			else
			{
				var toConfig = DVRAdressBook.tDVRTextOverlayConfig;
				if (!toInfo.Equal(toConfig))
				{
					toInfo.SetEntity(ref toConfig);
					db.Update<tDVRTextOverlayConfig>(toConfig);
				}
			}

			result = true;
			return result;
		}


		#region Unused

		//private bool UpdateTextOverlays()
		//{
		//	if (msgBody == null || msgBody.TOData == null || msgBody.TOData.Channels == null)
		//		return false;

		//	bool result = false;
		//	foreach (var channel in DVRAdressBook.tDVRChannels.Where(t=>t.tDVRTextOverlayChannel != null).ToList())
		//	{
		//		tDVRTextOverlayChannel toChannel = channel.tDVRTextOverlayChannel;
		//		TOChannelInfo vrInfo = msgBody.TOData.Channels.FirstOrDefault(t => t.id == channel.ChannelNo);
		//		if (vrInfo != null)
		//		{
		//			if (!vrInfo.Equal(toChannel))
		//			{
		//				vrInfo.SetEntity(ref toChannel);
		//				db.Update<tDVRTextOverlayChannel>(toChannel);
		//			}
		//			UpdateTextOverlayTrackers(channel,vrInfo);
		//			msgBody.TOData.Channels.Remove(vrInfo);
		//		}
		//		else
		//		{
		//			tDVRChannel deletechannel = channel;
		//			db.DeleteWhere<tDVRTextOverlayTracker>(t => t.tDVRChannel == deletechannel);
		//			db.Delete<tDVRTextOverlayChannel>(toChannel);
		//		}
		//	}

		//	InsertTextOVerlayChannels();
		//	result = true;
		//	return result;
		//}

		//private void InsertTextOVerlayChannels()
		//{
		//	foreach (var vcInfo in msgBody.TOData.Channels)
		//	{
		//		tDVRChannel channel = DVRAdressBook.tDVRChannels.FirstOrDefault(t => t.ChannelNo == vcInfo.id);
		//		if (channel != null)
		//		{
		//			var vcChan = new tDVRTextOverlayChannel() {tDVRChannel = channel};
		//			vcInfo.SetEntity(ref vcChan);
		//			db.Insert<tDVRTextOverlayChannel>(vcChan);
		//			InsertTextOverlayTrackers(vcInfo, channel);
		//		}
		//	}
		//}

		//private void UpdateTextOverlayTrackers(tDVRChannel channel, TOChannelInfo channelInfo)
		//{
		//	if (channelInfo.Trackers == null)
		//		return;

		//	foreach (var dvrvcTracker in channel.tDVRTextOverlayTrackers.ToList())
		//	{
		//		if (dvrvcTracker.KTracker == 0) continue;
		//		tDVRTextOverlayTracker tdvrTracker = dvrvcTracker;

		//		var vcArea = channelInfo.Trackers.FirstOrDefault(t => t.Left == tdvrTracker.StartX && t.Top == tdvrTracker.StartY && t.Width == tdvrTracker.Width && t.Height == tdvrTracker.Height);
		//		if (vcArea != null)
		//		{
		//			channelInfo.Trackers.Remove(vcArea);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRTextOverlayTracker>(tdvrTracker);
		//		}
		//	}

		//	InsertTextOverlayTrackers(channelInfo, channel);
		//}

		//private void InsertTextOverlayTrackers(TOChannelInfo vcInfo, tDVRChannel channel)
		//{
		//	foreach (var trackerInfo in vcInfo.Trackers)
		//	{
		//		var toTracker = new tDVRTextOverlayTracker() { tDVRChannel = channel };
		//		trackerInfo.SetEntity(ref toTracker);
		//		db.Insert<tDVRTextOverlayTracker>(toTracker);
		//	}
		//}

		#endregion

	}
	#region Class for Text Overlay
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawTextOverlayBody
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
		[XmlElement(RawTextOverlayConfig.STR_TextOverlay)]
		public TextOverlayData TOData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawTextOverlayConfig.STR_TextOverlay)]
	public class TextOverlayData
	{
		[XmlElement(RawTextOverlayConfig.STR_EnCollector)]
		public Int32 EnCollector { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_EnEmail)]
		public Int32 EnEmail { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_IntervalEmail)]
		public Int32 IntervalEmail { get; set; }

		[XmlArray(RawTextOverlayConfig.STR_Channels)]
		[XmlArrayItem(RawTextOverlayConfig.STR_Channel)]
		public List<TOChannelInfo> Channels = new List<TOChannelInfo>();

		public bool Equal(tDVRTextOverlayConfig value)
		{
			bool temp = EnCollector != 0;
			bool result = value.EnableCollector == temp;
			temp = EnEmail != 0;
			result = result && value.EnableEmail == temp &&
					 value.IntervalEmail == IntervalEmail;
			return result;
		}

		public void SetEntity(ref tDVRTextOverlayConfig value)
		{
			if (value == null)
				value = new tDVRTextOverlayConfig();
			value.EnableCollector = EnCollector != 0;
			value.EnableEmail = EnEmail != 0;
			value.IntervalEmail = IntervalEmail;
		}
	}

	[XmlRoot(RawTextOverlayConfig.STR_Channel)]
	public class TOChannelInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_ScrollDelay)]
		public Int32 ScrollDelay { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Delay)]
		public Int32 TextDelay { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Disable)]
		public bool Disable { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Brightness)]
		public Int32 Brightness { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_ShowColorMode)]
		public Int32 ShowColorMode { get; set; }

		[XmlArray(RawTextOverlayConfig.STR_Trackers)]
		[XmlArrayItem(RawTextOverlayConfig.STR_Tracker)]
		public List<TOTrackerInfo> Trackers = new List<TOTrackerInfo>();

		public bool Equal(tDVRTextOverlayChannels value)
		{
			bool result = //value.KChannel == kChannel &&
						  value.Brightness == Brightness &&
						  value.Disable == Disable &&
						  value.ScrollDelay == ScrollDelay &&
						  value.ShowColorMode == ShowColorMode &&
						  value.TextDelay == TextDelay;
			return result;
		}

		public void SetEntity(ref tDVRTextOverlayChannels value)
		{
			if (value == null)
				value = new tDVRTextOverlayChannels();
			//value.KChannel = kChannel;
			value.Brightness = Brightness;
			value.Disable = Disable;
			value.ScrollDelay = ScrollDelay;
			value.ShowColorMode = ShowColorMode;
			value.TextDelay = TextDelay;
		}
	}

	[XmlRoot(RawTextOverlayConfig.STR_Tracker)]
	public class TOTrackerInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Left)]
		public Int32 Left { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Top)]
		public Int32 Top { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Width)]
		public Int32 Width { get; set; }

		[XmlElement(RawTextOverlayConfig.STR_Height)]
		public Int32 Height { get; set; }

		public bool Equal(tDVRTextOverlayTrackers value)
		{
			bool result = value.Height == Height &&
						  value.StartX == Left &&
						  value.StartY == Top &&
			              value.Width == Width;
			return result;
		}

		public void SetEntity(ref tDVRTextOverlayTrackers value)
		{
			if (value == null)
				value = new tDVRTextOverlayTrackers();
			value.Height = Height;
			value.StartX = Left;
			value.StartY = Top;
			value.Width = Width;
		}
	}
#endregion
}
