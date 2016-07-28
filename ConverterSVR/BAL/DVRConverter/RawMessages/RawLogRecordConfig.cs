using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SVRDatabase;
using PACDMModel.Model;
using SVRDatabase.Model;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawLogRecordConfig : RawDVRConfig<RawLogRecordBody>
	{
		#region Parameter
		public const string STR_LogRecord = "log_record";
		public const string STR_Years = "years";
		//public const string STR_Year = "year";
		public const string STR_Value = "value";
		//public const string STR_Size = "size";
		//public const string STR_id = "id";
		//public const string STR_Month = "month";
		public const string STR_Days = "days";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawLogRecordBody msgBody { get; set; }
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
			if (DVRAdressBook == null || msgBody.logData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (SetLogConfig(DVRAdressBook, msgBody.logData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_LOG_RECORD, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_LOG_RECORD, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool SetLogConfig(tDVRAddressBook dvrAddressBook, LogRecordData logRecordData)
		{
			if (logRecordData.LogYears == null) return false;
			bool ret = false;
			List<tDVRLog> logList = db.Query<tDVRLog>(x => x.KDVR == dvrAddressBook.KDVR).ToList();
			var tempLogs = CreateDvrLogList(dvrAddressBook.KDVR, msgBody.logData.LogYears);

			foreach (var log in logList)
			{
				tDVRLog tlog = log;
				tDVRLog logInfo = tempLogs.FirstOrDefault(t => t.KDVR == log.KDVR && t.Year == log.Year && t.Month == log.Month);
				if (logInfo != null)
				{
					if (!CompareLog(logInfo, tlog))
					{
						SetLog(logInfo, tlog);
						db.Update<tDVRLog>(tlog);
						ret = true;
					}
					tempLogs.Remove(logInfo);
				}
				else
				{
					db.Delete<tDVRLog>(tlog);
					ret = true;
				}
			}

			foreach (var log in tempLogs)
			{
				db.Insert<tDVRLog>(log);
				ret = true;
			}
			return ret;
		}

		private static List<tDVRLog> CreateDvrLogList(int kDvr, List<LogYearInfo> logInfolst)
		{
			var tmpLog = new List<tDVRLog>();
			foreach (var st in logInfolst)
			{
				tmpLog.AddRange(st.LogMonths.Select(im => new tDVRLog {KDVR = kDvr, Year = st.Value, Month = im.Value, Day = im.Days, DaySize = im.Size}));
			}
			return tmpLog;
		}

		private bool CompareLog(tDVRLog log1, tDVRLog log2)
		{
			bool result = log1.Day == log2.Day &&
						  log1.DaySize == log2.DaySize &&
						  log1.KDVR == log2.KDVR &&
						  log1.Month == log2.Month &&
						  log1.Year == log2.Year;
			return result;
		}

		private tDVRLog SetLog(tDVRLog sourceLog, tDVRLog dentinationLog)
		{
			dentinationLog.Day = sourceLog.Day;
			dentinationLog.DaySize = sourceLog.DaySize;
			dentinationLog.KDVR = sourceLog.KDVR;
			dentinationLog.Month = sourceLog.Month;
			dentinationLog.Year = sourceLog.Year;
			return dentinationLog;
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawLogRecordBody
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
		[XmlElement(RawLogRecordConfig.STR_LogRecord)]
		public LogRecordData logData { get; set; }
	}

	[XmlRoot(RawLogRecordConfig.STR_LogRecord)]
	public class LogRecordData
	{
		[XmlArray(RawLogRecordConfig.STR_Years)]
		[XmlArrayItem(MessageDefines.STR_Year)]
		public List<LogYearInfo> LogYears = new List<LogYearInfo>();
	}

	[XmlRoot(MessageDefines.STR_Year)]
	public class LogYearInfo
	{
		[XmlAttribute(RawLogRecordConfig.STR_Value)]
		public Int32 Value { get; set; }

		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(MessageDefines.STR_Month)]
		public List<LogMonthInfo> LogMonths { get; set; }
	}

	[XmlRoot(MessageDefines.STR_Month)]
	public class LogMonthInfo
	{
		[XmlAttribute(RawLogRecordConfig.STR_Value)]
		public Int32 Value { get; set; }

		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawLogRecordConfig.STR_Days)]
		public string Days { get; set; }
	}
	#endregion
}
