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

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawAlertEvent : RawDVRConfig<RawAlertEventBody>
	{
		#region Parameter
		public const string STR_Event = "event";
		//public const string STR_id = "id";
		//public const string STR_Name = "name";
		//public const string STR_Time = "time";
		public const string STR_StringTime = "string_time";
		public const string STR_ChannelID = "channel_id";
		//public const string STR_User = "user";
		public const string STR_Infomation = "information";
		private const byte ALERTTYPE_SENSOR = 9;
		private const byte ALERTTYPE_CONTROL = 10;
		private const byte ALERTTYPE_VL = 36;
		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawAlertEventBody msgBody { get; set; }
		#endregion

		public RawAlertEvent()
		{
		}

		public RawAlertEvent(string strMsg)
		{
			RawAlertEvent rw = Commons.ObjectUtils.DeSerialize(typeof(RawAlertEvent), strMsg) as RawAlertEvent;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<string> GetResponseMsg()
		{
			//Anh Huynh, Check & get image here because of DVR time, need improve later...
			if (NeedRequestImage()) //if (msgBody.evtData.ChannelID >= 0)
			{
				//DateTime dvrTime = ToDateTime(msgBody.evtData.StringTime);
				//long unixTime = dvrTime.FullDateTimeToUnixTimestamp();

				List<string> seqMessage = CheckAndGetImages(DVRAdressBook, msgBody.evtData.Time - AppSettings.AppSettings.Instance.ImageAlertOffset, msgBody.evtData.ChannelID, true);
				if (seqMessage != null && seqMessage.Count > 0)
				{
					string combined = string.Join(", ", seqMessage);
					return await Task.FromResult<string>(combined);
				}
			}
			return await Task.FromResult<string>(null);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			// 1. Convert mesage to Alert Event Object
			// 2. Save to Database
			if ((byte)msgBody.evtData.id == ALERTTYPE_VL)
			{
				return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.OK);
			}
			var alertInfo = new tAlertEvent
			{
				KAlertType = (byte) msgBody.evtData.id,
				Time = DateTime.UtcNow,//Time = ToDateTime(msgBody.evtData.Time),
				TimeZone = ToDateTime(msgBody.evtData.StringTime),
				Channel = msgBody.evtData.ChannelID,
				DVRUser = msgBody.evtData.User,
				Description = msgBody.evtData.Infomation,
				//DVRGuid = DVRAdressBook.DVRGuid,
				KDVR = DVRAdressBook.KDVR,
				ImageTime = msgBody.evtData.Time
			};
			db.Insert<tAlertEvent>(alertInfo);
			int ret = db.Save();
			if( ret >= 0)
			{
				//CMSWebApi.Cache.BackgroundTaskManager.Instance.Add( alertInfo);
				CheckAndFixAlert(db, alertInfo);
				CMSWebApi.Wrappers.Wrapper.Instance.DVRAlertEvent.Add(alertInfo);
			}

			return await Task.FromResult<Commons.ERROR_CODE>( ret == -1? Commons.ERROR_CODE.DB_INSERT_DATA_FAILED : Commons.ERROR_CODE.OK);
		}
		private void CheckAndFixAlert(IResposity db, tAlertEvent alert)
		{
			IEnumerable<AlertFixConfig> configs = AlertFixConfigs.Instance.GetConfig( alert.KAlertType);
			AlertHandlerBase handler = null;
			foreach(AlertFixConfig config in configs)
			{
				handler = AlertHandlerBase.AlertHandler(config.Handler);
				if( handler == null)
					continue;
				if( handler.HandleFixAlert<tAlertEvent>(db, base.DVRAdressBook, config, alert))
					break;
			}
		}
		private bool NeedRequestImage()
		{
			if (msgBody.evtData.ChannelID >= 0 && (msgBody.evtData.id == ALERTTYPE_CONTROL || msgBody.evtData.id == ALERTTYPE_SENSOR || msgBody.evtData.id == ALERTTYPE_VL))
			{
				return true;
			}
			return false;
		}
	}

	[XmlRoot(MessageDefines.STR_Body)]
	public class RawAlertEventBody
	{
		[XmlElement(RawAlertEvent.STR_Event)]
		public EventData evtData { get; set; }
	}

	[XmlRoot(RawAlertEvent.STR_Event)]
	public class EventData
	{
		[XmlElement(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(MessageDefines.STR_Time)]
		public Int64 Time { get; set; }

		[XmlElement(RawAlertEvent.STR_StringTime)]
		public string StringTime { get; set; }

		[XmlElement(RawAlertEvent.STR_ChannelID)]
		public Int32 ChannelID { get; set; }

		[XmlElement(MessageDefines.STR_User)]
		public string User { get; set; }

		[XmlElement(RawAlertEvent.STR_Infomation)]
		public string Infomation { get; set; }
	}
}
