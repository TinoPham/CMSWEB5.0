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
	public class RawTechnicalLog : RawDVRConfig<RawTechnicalLogBody>
	{
		#region Parameter
		public const string STR_TechLog = "record_log";
		//public const string STR_Year = "year";
		//public const string STR_Month = "month";
		public const string STR_Data = "data";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawTechnicalLogBody msgBody { get; set; }
		#endregion

		public RawTechnicalLog() { }

		public RawTechnicalLog(string strMsg)
		{
			RawTechnicalLog rw = Commons.ObjectUtils.DeSerialize(typeof(RawTechnicalLog), strMsg) as RawTechnicalLog;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
	
			if (DVRAdressBook == null)
				return await base.UpdateToDB();

			tDVRTechnicalLog logInfo = db.FirstOrDefault<tDVRTechnicalLog>(x => x.KDVR == DVRAdressBook.KDVR && x.Year == msgBody.logData.Year && x.Month == msgBody.logData.Month);
			if (logInfo == null)
			{
				logInfo = new tDVRTechnicalLog
				{
					KDVR = DVRAdressBook.KDVR,
					Month = msgBody.logData.Month,
					Year = msgBody.logData.Year,
					LogContent = msgBody.logData.Data
				};
				db.Insert<tDVRTechnicalLog>(logInfo);
			}
			else
			{
				logInfo.LogContent = msgBody.logData.Data;
				db.Update<tDVRTechnicalLog>(logInfo);
			}
			return await Task.FromResult<Commons.ERROR_CODE>( db.Save() == -1? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
		}

		
	}

	[XmlRoot(MessageDefines.STR_Body)]
	public class RawTechnicalLogBody
	{
		[XmlElement(RawTechnicalLog.STR_TechLog)]
		public TechnicalLogData logData { get; set; }
	}

	[XmlRoot(RawTechnicalLog.STR_TechLog)]
	public class TechnicalLogData
	{
		[XmlElement(MessageDefines.STR_Year)]
		public Int32 Year { get; set; }

		[XmlElement(MessageDefines.STR_Month)]
		public Int32 Month { get; set; }

		[XmlElement(RawTechnicalLog.STR_Data)]
		public string Data { get; set; }
	}
}
