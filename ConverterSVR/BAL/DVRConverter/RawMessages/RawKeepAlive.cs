using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;
using Extensions;
using AppSettings;
using ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers;
using CMSWebApi.Utils;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawKeepAlive : RawDVRConfig<RawKeepAliveBody>
	{
		#region Parameter
		public const string STR_DvrSummary = "dvr_summary";
		public const string STR_TotalSize = "total_size";
		public const string STR_FreeSize = "free_size";
		public const string STR_RecordingDays = "recording_days";
		public const string STR_Channels = "channels";
		//public const string STR_Size = "size";
		public const string STR_NumVideoInput = "num_video_input";
		public const string STR_VideoSource = "video_source";
		public const string STR_ChannelStatus = "channel_status";
		public const string STR_DVRTime = "dvr_time";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawKeepAliveBody msgBody { get; set; }
		#endregion

		public RawKeepAlive() { }

		public RawKeepAlive(string strMsg)
		{
			RawKeepAlive rw = Commons.ObjectUtils.DeSerialize(typeof(RawKeepAlive), strMsg) as RawKeepAlive;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<string> GetResponseMsg()
		{
			//Anh Huynh, Check & get image in case there is no alert from DVR, need improve later...
			//tDVRConfigChangeTime cfgTime = db.FirstOrDefault<tDVRConfigChangeTime>(cfg => cfg.KDVR == DVRAdressBook.KDVR && cfg.TimeChange > 0);
			List<string> seqMessage = new List<string>();
			List<string> imgReqs = null;
			if (msgBody.tsDVRTime != null && msgBody.tsDVRTime.TimeStamp > 0)
			{

				imgReqs = CheckAndGetImages(DVRAdressBook, 0, -1, false);// msgBody.tsDVRTime.TimeStamp);
			}
			else
			{
				db.Include<tDVRAddressBook, tDVRConfigChangeTime>(DVRAdressBook, dvr => dvr.tDVRConfigChangeTimes);
				if (DVRAdressBook.tDVRConfigChangeTimes != null && DVRAdressBook.tDVRConfigChangeTimes.Count > 0)
				{
					tDVRConfigChangeTime cfgTime = DVRAdressBook.tDVRConfigChangeTimes.OrderByDescending(x => x.TimeChange).FirstOrDefault(x => x.TimeChange.HasValue && x.TimeChange > 0);
					if (cfgTime != null)
					{
						//seqMessage = CheckAndGetImages(DVRAdressBook, (Int64)cfgTime.TimeChange);
						if (cfgTime.CMSTime.HasValue && cfgTime.CMSTime > DateTime.MinValue)
						{
							TimeSpan delta = DateTime.Now - (DateTime)cfgTime.CMSTime;
							//seqMessage.AddRange(CheckAndGetImages(DVRAdressBook, (Int64)(cfgTime.TimeChange + delta.TotalSeconds - 60)));
							imgReqs = CheckAndGetImages(DVRAdressBook, (Int64)(cfgTime.TimeChange + delta.TotalSeconds - 60));
						}
					}
				}
			}
			if (imgReqs != null && imgReqs.Any())
			{
				seqMessage.AddRange(imgReqs);
			}
			//seqMessage.AddRange(CheckHeatmapSchedule());

			if (seqMessage != null && seqMessage.Count > 0)
			{
				string combined = string.Join(", ", seqMessage);
				return await Task.FromResult<string>(combined);
			}

			return await Task.FromResult<string>(null);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.csData == null)
				return await base.UpdateToDB();
			//CheckHeatmapSchedule();

			if (UpdateKeepAlive(DVRAdressBook, msgBody.csData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateLastAccess(DVRAdressBook))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			// check to fix alert not recording
			AlertFixConfig config = AlertFixConfigs.Instance.GetConfig( this.GetType().Name); 
			if( config != null)
			{
				AlertHandlerBase instance = AlertHandlerBase.AlertHandler(config.MsgHandler);
				if( instance != null)
					instance.HandleFixAlert<tDVRAddressBook>(db, base.DVRAdressBook, config, base.DVRAdressBook);
			}

			return await base.UpdateToDB();
		}

		private bool UpdateLastAccess(tDVRAddressBook dvrAdressBook)
		{
			tDVRKeepAlives keepalive = db.FirstOrDefault<tDVRKeepAlives>(item => item.KDVR == dvrAdressBook.KDVR);
			if (keepalive != null)
			{
				keepalive.LastAccess = (int)DateTime.Now.FullDateTimeToUnixTimestamp();
				db.Update<tDVRKeepAlives>(keepalive);
			}
			else
			{
				keepalive = new tDVRKeepAlives { LastAccess = (int)DateTime.Now.FullDateTimeToUnixTimestamp(), KDVR = dvrAdressBook.KDVR };
				db.Insert<tDVRKeepAlives>(keepalive);
			}
			return true;
		}

		private bool UpdateDVRAddressBook(tDVRAddressBook dvrAddressBook, KeepAliveData csData)
		{
			bool ret = false;

			if (dvrAddressBook != null)
			{
				if (dvrAddressBook.TotalDiskSize != csData.TotalSize || dvrAddressBook.FreeDiskSize != csData.FreeSize ||
				    dvrAddressBook.RecordingDay != csData.RecordingDays)
				{
					dvrAddressBook.TotalDiskSize = csData.TotalSize;
					dvrAddressBook.FreeDiskSize = csData.FreeSize;
					dvrAddressBook.RecordingDay = csData.RecordingDays;
					db.Update<tDVRAddressBook>(dvrAddressBook);
					ret = true;
				}
			}

			return ret;
		}

		private bool UpdateKeepAlive(tDVRAddressBook dvrAdressBook, KeepAliveData csData)
		{
			bool ret = false;

			if (!(csData.FreeSize == 0 && csData.RecordingDays == 0 && csData.TotalSize == 0))
				ret = UpdateDVRAddressBook(dvrAdressBook, csData);

			if (msgBody.csData.ChannelStatus == null)
				return ret;

			if (csData.ChannelStatus == null || csData.ChannelStatus.ChannelStatus == null) return ret;
			int[] arrInt = null;
			if (!StringToIntArray(ref arrInt, csData.ChannelStatus.ChannelStatus, 2)) return ret;

			db.Include<tDVRAddressBook,tDVRChannels>(dvrAdressBook,t=>t.tDVRChannels);
			for (int channelStatus = 0; channelStatus < arrInt.Length; channelStatus++)
			{
				tDVRChannels channel = dvrAdressBook.tDVRChannels.FirstOrDefault(t => t.ChannelNo == channelStatus);
				if (channel == null) continue;
				channel.Status = arrInt[channelStatus];
				db.Update<tDVRChannels>(channel);
				ret = true;
			}

			return ret;
		}

		public int[] GetStatusList(string msgChannelStatusList)
		{
			int[] arrInt = null;
			if (StringToIntArray(ref arrInt, msgChannelStatusList, 2))
			{
				return arrInt;
			}
			return arrInt;
		}

		private List<string> CheckHeatmapSchedule()
		{
			List<string> lsHMReqs = new List<string>();
			DateTime dvrNow = DateTime.Now;
			if (msgBody.tsDVRTime != null)
			{
				db.Include<tDVRAddressBook, tDVRChannels>( DVRAdressBook, item => item.tDVRChannels);
				List<int> lsChannels = DVRAdressBook.tDVRChannels.Select(x=>x.KChannel).ToList();

				dvrNow = dvrNow.Date;
				var includes = new string[]
				{
					typeof (tbl_HM_TaskChannel).Name
					//string.Format("{0}.{1}", typeof (tbl_HM_TaskChannel).Name, typeof (tDVRChannels).Name)
				};
				List<tbl_HM_ScheduledTasks> HMTasks = db.Query<tbl_HM_ScheduledTasks>(x => x.StartDate <= dvrNow && x.EndDate.Value.Hour >= dvrNow.Hour && x.tbl_HM_TaskChannel.Any(ch => lsChannels.Contains(ch.KChannel ?? 0)), includes).ToList();
				foreach (tbl_HM_ScheduledTasks task in HMTasks)
				{
					CheckHMScheduleTask(task, dvrNow, lsHMReqs);
				}
			}
			return lsHMReqs;
		}
		private void CheckHMScheduleTask(tbl_HM_ScheduledTasks task, DateTime dvrTime, List<string> lsHMReqs)
		{
			switch (task.ScheduleType)
			{
				case (int)HM_SCHEDULE_TYPE.Hourly:
					CheckHMHourlyTask(task, dvrTime, lsHMReqs);
					break;
				case (int)HM_SCHEDULE_TYPE.Weekly:
					CheckHMWeeklyTask(task, dvrTime, lsHMReqs);
					break;
				case (int)HM_SCHEDULE_TYPE.Daily:
				default:
					CheckHMDailyTask(task, dvrTime, lsHMReqs);
					break;
			}
		}
		private UInt64 GetChannelMask(tbl_HM_ScheduledTasks task)
		{
			if (task.tbl_HM_TaskChannel == null || !task.tbl_HM_TaskChannel.Any())
			{
				db.Include<tbl_HM_ScheduledTasks, tbl_HM_TaskChannel>(task, it => it.tbl_HM_TaskChannel);
			}
			List<int> lsChannel = task.tbl_HM_TaskChannel.Where(x=>x.tDVRChannels != null).Select(x => x.tDVRChannels.ChannelNo).ToList();
			UInt64 chanMask = 0;
			foreach (int ch in lsChannel)
			{
				chanMask |= (UInt64)1 << ch;
			}
			return chanMask;
		}
		private void CheckHMHourlyTask(tbl_HM_ScheduledTasks task, DateTime dvrTime, List<string> lsHMReqs)
		{
			DateTime from = DateTime.MinValue;
			DateTime to = DateTime.MinValue;
			UInt64 chanMask = GetChannelMask(task);

			string imgReq = string.Empty;
			if (!task.LastExecuted.HasValue)
			{
				DateTime lastHour = dvrTime.AddHours(-1);
				from = lastHour.Date.AddHours(lastHour.Hour);
				to = from.AddHours(1).AddMilliseconds(-1);

				imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Hourly);
				lsHMReqs.Add(imgReq);
			}
			else
			{
				DateTime lastExecuted = task.LastExecuted.Value;
				int totalMin = Convert.ToInt32(Math.Ceiling((dvrTime - lastExecuted).TotalMinutes));
				while (totalMin > CMSWebApi.Utils.Consts.MIN_PER_HOUR)
				{
					from = lastExecuted.AddHours(1);
					to = from.AddHours(1).AddMilliseconds(-1);

					imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Hourly);
					lsHMReqs.Add(imgReq);

					lastExecuted = from;
					totalMin = Convert.ToInt32(Math.Ceiling((dvrTime - lastExecuted).TotalMinutes));
				}
			}
		}
		private void CheckHMDailyTask(tbl_HM_ScheduledTasks task, DateTime dvrTime, List<string> lsHMReqs)
		{
			DateTime from = DateTime.MinValue;
			DateTime to = DateTime.MinValue;
			UInt64 chanMask = GetChannelMask(task);

			string imgReq = string.Empty;
			if (!task.LastExecuted.HasValue)
			{
				DateTime lastDate = dvrTime.AddDays(-1);
				from = lastDate.Date;
				to = from.AddDays(1).AddMilliseconds(-1);

				imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Daily);
				lsHMReqs.Add(imgReq);
			}
			else
			{
				DateTime lastExecuted = task.LastExecuted.Value;
				int totalDate = Convert.ToInt32(Math.Ceiling((dvrTime.Date - lastExecuted).TotalDays));
				while (totalDate > 0)
				{
					from = lastExecuted.AddDays(1);
					to = from.AddDays(1).AddMilliseconds(-1);

					imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Daily);
					lsHMReqs.Add(imgReq);

					lastExecuted = from;
					totalDate = Convert.ToInt32(Math.Ceiling((dvrTime.Date - lastExecuted).TotalDays));
				}
			}
		}
		private void CheckHMWeeklyTask(tbl_HM_ScheduledTasks task, DateTime dvrTime, List<string> lsHMReqs)
		{
			DateTime from = DateTime.MinValue;
			DateTime to = DateTime.MinValue;
			UInt64 chanMask = GetChannelMask(task);

			string imgReq = string.Empty;
			if (!task.LastExecuted.HasValue)
			{
				DateTime lastWeek = dvrTime.AddDays(- CMSWebApi.Utils.Consts.DAY_PER_WEEK);
				from = lastWeek.AddDays(0 - (int)lastWeek.DayOfWeek);
				to = from.AddDays(CMSWebApi.Utils.Consts.DAY_PER_WEEK).AddMilliseconds(-1);

				imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Weekly);
				lsHMReqs.Add(imgReq);
			}
			else
			{
				DateTime lastExecuted = task.LastExecuted.Value;
				int totalDay = Convert.ToInt32(Math.Ceiling((dvrTime - lastExecuted).TotalDays));
				while (totalDay > CMSWebApi.Utils.Consts.DAY_PER_WEEK)
				{
					from = lastExecuted.AddDays(CMSWebApi.Utils.Consts.DAY_PER_WEEK);
					to = from.AddDays(CMSWebApi.Utils.Consts.DAY_PER_WEEK).AddMilliseconds(-1);

					imgReq = GetHeatMapImageMsg(chanMask, from, to, (int)HM_SCHEDULE_TYPE.Weekly);
					lsHMReqs.Add(imgReq);

					lastExecuted = from;
					totalDay = Convert.ToInt32(Math.Ceiling((dvrTime - lastExecuted).TotalMinutes));
				}
			}
		}
		#region Number array convert
		//public static bool IntArrayToString(ref string sOutString, int[] arrInt)
		//{
		//	return IntArrayToString(ref sOutString, arrInt, 2);
		//}

		//public static bool IntArrayToString(ref string sOutString, int[] arrInt, int nStep)
		//{
		//	bool bRet = true;
		//	string sFormat = "{0:";
		//	for (int st = 0; st < nStep; st++)
		//	{
		//		sFormat += "0";
		//	}
		//	sFormat += "}";
		//	sOutString = string.Empty;
		//	int nSize = arrInt.Length;
		//	for (int i = 0; i < nSize; i++)
		//	{
		//		sOutString += String.Format(sFormat, arrInt[i]);
		//	}

		//	return bRet;
		//}

		public static bool StringToIntArray(ref int[] arrInt, string sInString, int nStep)
		{
			if (String.IsNullOrEmpty(sInString))
				return false;

			bool bRet = true;
			try
			{
				int nStartPos = 0;
				int nStrLen = sInString.Length;
				int idx = 0;
				int nSize = nStrLen / nStep;
				if (nStrLen % nStep > 0)
				{
					nSize += 1;
				}
				arrInt = new int[nSize];
				while (nStartPos < nStrLen && idx < nSize)
				{
					arrInt[idx] = Convert.ToInt32(sInString.Substring(nStartPos, nStep));
					nStartPos += nStep;
					idx++;
				}
			}
			catch (System.Exception)
			{
				return false;
			}
			return bRet;
		}
		#endregion
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawKeepAliveBody
	{
		[XmlElement(RawKeepAlive.STR_DVRTime)]
		public DVRTimeStamp tsDVRTime { get; set; }

		[XmlElement(RawKeepAlive.STR_DvrSummary)]
		public KeepAliveData csData { get; set; }
	}

	[XmlRoot(RawKeepAlive.STR_DvrSummary)]
	public class KeepAliveData
	{
		[XmlElement(RawKeepAlive.STR_TotalSize)]
		public Int32 TotalSize { get; set; }

		[XmlElement(RawKeepAlive.STR_FreeSize)]
		public Int32 FreeSize { get; set; }

		[XmlElement(RawKeepAlive.STR_RecordingDays)]
		public Int32 RecordingDays { get; set; }

		[XmlElement(RawKeepAlive.STR_Channels)]
		public KeepAliveChannel ChannelStatus { get; set; }
	}

	[XmlRoot(RawKeepAlive.STR_Channels)]
	public class KeepAliveChannel
	{
		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlAttribute(RawKeepAlive.STR_NumVideoInput)]
		public Int32 NumVideoInput { get; set; }

		[XmlElement(RawKeepAlive.STR_VideoSource)]
		public string VideoSource { get; set; }

		[XmlElement(RawKeepAlive.STR_ChannelStatus)]
		public string ChannelStatus { get; set; }
	}
	#endregion
}
