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
	public class RawRecordScheduleConfig : RawDVRConfig<RawRecordScheduleBody>
	{
		#region Parameter
		public const string STR_RecordSchedule = "record_schedule";
		public const string STR_Server = "server";
		public const string STR_SubStreamMode = "sub_stream_mode";
		public const string STR_PreRecTime = "pre_record_time";
		public const string STR_PostRecTime = "post_record_time";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Schedules = "schedules";
		public const string STR_Schedule = "schedule";
		//public const string STR_Name = "name";
		//public const string STR_Time = "time";
		public const string STR_RotationType = "rotation_type";
		//public const string STR_Size = "size";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader MsgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawRecordScheduleBody MsgBody { get; set; }

		//List<tDVRRecordSchedule> _reCheduleList;
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
			if (DVRAdressBook == null || msgBody.RSData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateRecordScheduleConfigs(DVRAdressBook.tDVRChannels, msgBody.RSData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_RECORD_SCHEDULE, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_RECORD_SCHEDULE, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateHardware(RecordScheduleData rsData, tDVRAddressBook dvrAddressBook)
		{
			bool ret = false;

			if (dvrAddressBook.KDVRVersion < VersionProSchedule)
				return false;

			tDVRHardware hardware = db.FirstOrDefault<tDVRHardware>(t => t.KDVR == dvrAddressBook.KDVR);

			if (hardware != null && (hardware.PreRecordingTime != rsData.PreRecTime || hardware.PostRecordingTime != rsData.PostRecTime))
			{
				hardware.PreRecordingTime = rsData.PreRecTime;
				hardware.PostRecordingTime = rsData.PostRecTime;
				db.Update<tDVRHardware>(hardware);
				ret = true;
			}
			return ret;
		}

		private bool UpdateRecordScheduleConfigs(IEnumerable<tDVRChannels> vcchannelList, RecordScheduleData rsData)
		{
			bool ret = false;

			ret = UpdateHardware(rsData, DVRAdressBook);

			foreach (var channel in vcchannelList)
			{
				ret |= UpdateRecordSchedule(channel, rsData);
			}
			return ret;
		}

		private bool UpdateRecordSchedule(tDVRChannels tChannel, RecordScheduleData rsData)
		{
			RSChannelInfo chanelInfo = rsData.Channels.FirstOrDefault(t => t.id == tChannel.ChannelNo);
			if (chanelInfo == null || chanelInfo.Schedules == null)
			{
				db.DeleteWhere<tDVRRecordSchedule>(t=>t.KChannel == tChannel.KChannel);
				return true;
			}

			//Join 2 list to 1 Object list with samme channel ID
			IEnumerable<tDVRRecordSchedule> recordSchedules = db.Query<tDVRRecordSchedule>(x => x.KChannel == tChannel.KChannel).ToList();
			var scheduleInfos = chanelInfo.Schedules;
			bool result = false;
			var updates = from recordSchedule in recordSchedules
						  from scheduleInfo in scheduleInfos
						  where scheduleInfo.Name.Trim().Equals(recordSchedule.Name.Trim()) && scheduleInfo.RotationType == recordSchedule.RotationType
						  select new { Item = recordSchedule, InfoItem = scheduleInfo };

			//Update Object list above
			tDVRRecordSchedule dvrRecordSchedule;
			foreach (var item in updates)
			{
				if (item.InfoItem.Equal(item.Item) && item.Item.ScheduleID == tChannel.ChannelNo && item.Item.SubStreamMode == rsData.SubStreamMode)
				{
					continue;
				}
				else
				{
					dvrRecordSchedule = item.Item;
					dvrRecordSchedule.KChannel = tChannel.KChannel;
					dvrRecordSchedule.ScheduleID = tChannel.ChannelNo;
					dvrRecordSchedule.SubStreamMode = rsData.SubStreamMode;
					item.InfoItem.SetEntity(ref dvrRecordSchedule);
					db.Update<tDVRRecordSchedule>(dvrRecordSchedule);
					result = true;
				}
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRRecordSchedule> deletes = recordSchedules.Except(updates.Select(item => item.Item)).ToList();
			foreach (tDVRRecordSchedule delete in deletes)
			{
				db.Delete<tDVRRecordSchedule>(delete);
				//System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<ScheduleInfo> newitems = scheduleInfos.Except(updates.Select(item => item.InfoItem));
			foreach (ScheduleInfo newitem in newitems)
			{
				dvrRecordSchedule = new tDVRRecordSchedule() { KChannel = tChannel.KChannel, ScheduleID = tChannel.ChannelNo, SubStreamMode = rsData.SubStreamMode};
				newitem.SetEntity(ref dvrRecordSchedule);
				db.Insert<tDVRRecordSchedule>(dvrRecordSchedule);
				result = true;
			}

			return result;
		}

		#region Unused

		//public async Task<Commons.ERROR_CODE>  UpdateToDB1()
		//{
		//	if (DVRAdressBook == null)
		//		return await base.UpdateToDB();
		//	//tDVRRecordSchedule

		//	List<tDVRChannel> channelList = db.Query<tDVRChannel>(item => item.KDVR == DVRAdressBook.KDVR).ToList();
		//	if (channelList.Count == 0)
		//	{
		//		SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
		//		return await base.UpdateToDB();
		//	}

		//	List<int> lsKChannels = channelList.Select(chan => chan.KChannel).ToList();
		//	_reCheduleList = db.Query<tDVRRecordSchedule>(x => lsKChannels.Contains(x.KChannel)).ToList();
		//	foreach (var cn in channelList)
		//	{
		//		if (cn.Enable != 0)
		//			SetRecordSchedules(cn);
		//	}
		//	return await base.UpdateToDB();
		//}

		//private bool IsBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IsRsData()
		//{
		//	if (!IsBodyData()) return false;
		//	return msgBody.RSData != null;
		//}

		//private bool IsChannelData()
		//{
		//	if (!IsRsData()) return false;
		//	return msgBody.RSData.Channels != null;
		//}

		//private List<RSChannelInfo> GetChannelList()
		//{
		//	List<RSChannelInfo> channels = IsChannelData() ? msgBody.RSData.Channels.ToList() : new List<RSChannelInfo>();
		//	return channels;
		//}

		//private bool IsScheduleInfos(RSChannelInfo rschannel)
		//{
		//	if (rschannel == null) return false;
		//	return rschannel.Schedules != null;
		//}

		//private List<ScheduleInfo> GetScheduleInfoList(RSChannelInfo rschannel)
		//{
		//	List<ScheduleInfo> rsList = IsScheduleInfos(rschannel) ? rschannel.Schedules.ToList() : new List<ScheduleInfo>();
		//	return rsList;
		//}

		//private void SetRecordSchedules(tDVRChannel channel)
		//{
		//	List<tDVRRecordSchedule> lsRsData = _reCheduleList.Where(x => x.KChannel == channel.KChannel).ToList();

		//	List<RSChannelInfo> channelList = GetChannelList();
		//	RSChannelInfo chanelInfo = channelList.FirstOrDefault(t => t.id == channel.ChannelNo);
		//	List<ScheduleInfo> scheduleList = GetScheduleInfoList(chanelInfo);

		//	foreach (var sc in lsRsData)
		//	{
		//		tDVRRecordSchedule schedule = sc;
		//		ScheduleInfo scInfo = scheduleList.FirstOrDefault(t => t.Name.Trim().ToUpper() == sc.Name.Trim().ToUpper() && t.RotationType == sc.RotationType);
		//		if (scInfo != null)
		//		{
		//			if (!CompareRecordScheduleInfo(scInfo, channel.ChannelNo, channel.KChannel, schedule))
		//			{
		//				SetRecordScheduleInfo(scInfo, channel.ChannelNo, channel.KChannel, ref schedule);
		//				db.Update<tDVRRecordSchedule>(schedule);
		//			}
		//			scheduleList.Remove(scInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRRecordSchedule>(schedule);
		//		}
		//	}

		//	foreach (var sc in scheduleList)
		//	{
		//		var rsData = new tDVRRecordSchedule();
		//		SetRecordScheduleInfo(sc, channel.ChannelNo, channel.KChannel, ref rsData);
		//		db.Insert<tDVRRecordSchedule>(rsData);
		//	}

		//	db.Save();
		//}

		//private void SetRecordScheduleInfo(ScheduleInfo schedInf, int idx, int kChan, ref tDVRRecordSchedule rsData)
		//{
		//	rsData.KChannel = kChan;
		//	rsData.ScheduleID = idx;
		//	rsData.Name = schedInf.Name;
		//	rsData.RotationType = schedInf.RotationType;
		//	rsData.Date = schedInf.Time.Value;
		//	rsData.Size = schedInf.Size;
		//	rsData.SubStreamMode = msgBody.RSData.SubStreamMode;
		//	if (schedInf.BinData != null)
		//	{
		//		rsData.Data = schedInf.BinData;
		//	}
		//}

		//private bool CompareRecordScheduleInfo(ScheduleInfo schedInf, int idx, int kChan, tDVRRecordSchedule rsData)
		//{
		//	bool result = rsData.KChannel == kChan &&
		//				  rsData.ScheduleID == idx &&
		//				  rsData.Name == schedInf.Name &&
		//				  rsData.RotationType == schedInf.RotationType &&
		//				  rsData.Date == schedInf.Time.Value &&
		//				  rsData.Size == schedInf.Size &&
		//				  rsData.SubStreamMode == msgBody.RSData.SubStreamMode;
		//	if (schedInf.BinData != null)
		//	{
		//		result = result &&
		//				 rsData.Data == schedInf.BinData;
		//	}
		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawRecordScheduleBody
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
		[XmlElement(RawRecordScheduleConfig.STR_RecordSchedule)]
		public RecordScheduleData RSData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawRecordScheduleConfig.STR_RecordSchedule)]
	public class RecordScheduleData
	{
		[XmlElement(RawRecordScheduleConfig.STR_Server)]
		public string Server { get; set; }

		[XmlElement(RawRecordScheduleConfig.STR_SubStreamMode)]
		public Int32 SubStreamMode { get; set; }

		[XmlElement(RawRecordScheduleConfig.STR_PreRecTime)]
		public Int32 PreRecTime { get; set; }

		[XmlElement(RawRecordScheduleConfig.STR_PostRecTime)]
		public Int32 PostRecTime { get; set; }

		[XmlArray(RawRecordScheduleConfig.STR_Channels)]
		[XmlArrayItem(RawRecordScheduleConfig.STR_Channel)]
		public List<RSChannelInfo> Channels = new List<RSChannelInfo>();
	}

	[XmlRoot(RawRecordScheduleConfig.STR_Channel)]
	public class RSChannelInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlArray(RawRecordScheduleConfig.STR_Schedules)]
		[XmlArrayItem(RawRecordScheduleConfig.STR_Schedule)]
		public List<ScheduleInfo> Schedules = new List<ScheduleInfo>();
	}

	[XmlRoot(RawRecordScheduleConfig.STR_Schedule)]
	public class ScheduleInfo : IMessageEntity<tDVRRecordSchedule>
	{
		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(MessageDefines.STR_Time)]
		public DateInfo Time { get; set; }

		[XmlElement(RawRecordScheduleConfig.STR_RotationType)]
		public Int32 RotationType { get; set; }

		[XmlElement(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlText]
		public string BinData { get; set; }

		public bool Equal(tDVRRecordSchedule value)
		{
			bool result = //value.KChannel == kChan &&
				//value.ScheduleID == idx &&
				//value.Name == Name &&
				//value.RotationType == RotationType &&
				value.Date == Time.Value &&
				value.Size == Size;
						 // value.SubStreamMode == msgBody.RSData.SubStreamMode;
			if (BinData != null && result)
			{
				result = value.Data == BinData;
			}
			return result;
		}

		public void SetEntity(ref tDVRRecordSchedule value)
		{
			if (value == null)
				value = new tDVRRecordSchedule();
			//value.KChannel = kChan;
			//value.ScheduleID = idx;
			value.Name = Name;
			value.RotationType = RotationType;
			value.Date = Time.Value;
			value.Size = Size;
			//value.SubStreamMode = msgBody.RSData.SubStreamMode;
			if (BinData != null)
			{
				value.Data = BinData;
			}
		}
	}
	#endregion
}
