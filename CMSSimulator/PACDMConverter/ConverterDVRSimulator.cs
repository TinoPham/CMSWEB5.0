using System.Threading;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Formatting;

namespace PACDMSimulator
{
	public class ConverterDVRSimulator : IDisposable
	{
		[Flags]
		private enum CovnertTypes : uint
		{
			None = 0,
			PACDM_CONVERT = 1 << 0,
			DVR_CONVERT = PACDM_CONVERT << 1

		}
		enum EnumControl : int
		{
			EVT_DVR_SEND_MESSAGE = 0,
			EVT_API_RESPONSE_MESSAGE
		}

		const string XPath_MessageID = "/message/header/id";
		HttpClientSingleton serviceapi;
		readonly MediaTypeFormatter DataFormatter = new JsonMediaTypeFormatter();

		readonly CovnertTypes ConvertType = CovnertTypes.DVR_CONVERT; 
		public ConvertDB LocalDb;
		public MessageDVRInfo InfoDVR {get;set;}
		public ServiceConfig ServiceConfig {get;set;}
		//PACDMConverter.PACConverter pacConverter;
		private CancellationTokenSource _cancellation;
		public ConverterDVRSimulator()
		{
			
		}

		public ConverterDVRSimulator(ConvertDB localDb)
		{		
			this.LocalDb = localDb;
		}

		public void setWebConnect(CancellationTokenSource cancellation)
		{
			_cancellation = cancellation;
			serviceapi = new HttpClientSingleton(ServiceConfig, InfoDVR, cancellation);
		}

		public void setInfo()
		{
			InfoDVR = DVRInfos.Instance.MsgDVRInfo;
		}

		private bool InitDB()
		{
			var connnection = ConfigurationManager.ConnectionStrings[Consts.LogContextConnection];
			if( connnection == null || string.IsNullOrEmpty(connnection.Name) || string.IsNullOrEmpty(connnection.ConnectionString))
			{
				return false;
			}
			try
			{
				LocalDb = new ConvertDB(Consts.LogContextConnection);
				return true;
			}
			catch(Exception ex)
			{
				
				LocalDb = null;
				return false;
			}
			
		}

		public bool StartConvertTask()
		{
			if(ConvertType == CovnertTypes.None)
				return false;

			 if( !InitDB())
				return false;			 

			if( ServiceConfig == null || string.IsNullOrEmpty(ServiceConfig.Url))
			{
				//string msg = String.Format("{0}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.CONVERTER_INVALID_WEBAPI));
				return false;
			}

			serviceapi = new HttpClientSingleton(ServiceConfig, DVRInfos.Instance.MsgDVRInfo, _cancellation);

			//if( (ConvertType & CovnertTypes.PACDM_CONVERT) == CovnertTypes.PACDM_CONVERT)
			//{
			//	pacConverter = new PACDMConverter.PACConverter(this.LocalDb);
			//	pacConverter.StartConvertTask();
			//}
			if( (ConvertType & CovnertTypes.DVR_CONVERT) == CovnertTypes.DVR_CONVERT)
			{
				//this.LocalDb = 
				//DVRConverter = new ConverterDVR(this.LocalDb, SocketPort);
				//DVRConverter.StartDVRConverter();
			}
			return true;
		}

		public void Dispose()
		{
			this.serviceapi = null;
			if(_cancellation != null)
				_cancellation.Dispose();
			//if( LocalDb != null)
			//	LocalDb.Save();
		}

		public List<string> DVR_SendToWebAPI(string sXML,string msgID)
		{
			List<string> rst = null;
			try
			{
				MessageData msg = new MessageData { Programset = Commons.Programset.DVR, Mapping = msgID, Data = sXML };
				MessageResult result = serviceapi.PostData(msg, typeof(MessageData), DataFormatter);
				string alldata;
				if (result.ErrorID != Commons.ERROR_CODE.OK)
				{
					rst = null;
				}
				else
				{
					alldata = result.Data;
					rst = new List<string>();
					if (!String.IsNullOrEmpty(alldata))
					{
						var dataBase64 = alldata.Split(',');
						foreach (var tBase64 in dataBase64)
						{
							rst.Add(Commons.Utils.Base64toString(tBase64));
						}
					}
				}
			}
			catch(Exception ex)
			{
				
			}
			return rst;
		}

		public void CancelRequest()
		{
			serviceapi.CancelRequest();
		}

		public void SetCancelToken(CancellationTokenSource cancellationToken)
		{
			serviceapi.SetCancelToken(cancellationToken);
		}

		//private void SendToDVR(string xmlStr)
		//{
		//	DVRDataMessage msg = StringMessage2DVRMessage(xmlStr);
		//	if( msg.Header.msg_id == 0)
		//		return;
		//	//DVRManager.SendToAllDVRs(msg);
		//}

		//private string DVRMessagetoDataString(DVRDataMessage DVRMessage )
		//{
		//	MsgHeader Header = DVRMessage.Header;
		//	if (Header.msg_id == (int)CMSMsg.MSG_DVR_GET_SNAPSHOT_RESPONSE || Header.msg_id == (int)CMSMsg.MSG_DVR_GET_VIDEO_RESPONSE)
		//		return Commons.Utils.ToBase64String(DVRMessage.Buffer);
		//	else
		//	{
		//		byte[] unzip = Utils.UnZipData(DVRMessage.Buffer, Header.size - DVRDataMessage.HeaderSize);
		//		return unzip == null? string.Empty : Commons.Utils.ByteArr2String( unzip );
				
		//	}
		//}

		//private ushort GetMsgID(string tXml)
		//{
		//	XmlNode nodeid =  Commons.XMLUtils.SelectNode( Commons.XMLUtils.XMLDocument(tXml).DocumentElement, XPath_MessageID ); 
		//	if( nodeid == null)
		//		return 0;
		//	UInt16 ret = 0;
		//	UInt16.TryParse(nodeid.InnerText, out ret);
		//	return ret;
		//}

		//private DVRDataMessage StringMessage2DVRMessage( string  message) 
		//{
		//	byte[]buff = Commons.Utils.String2Byte(message);
		//	byte[] zipbuff = Utils.ZipData(buff, buff.Length);

		//	MsgHeader msgHeader = new MsgHeader();
		//	msgHeader.msgBegin = DVRDataMessage.BEGIN_MSG;
		//	msgHeader.msg_id = GetMsgID(message);
		//	msgHeader.size = DVRDataMessage.HeaderSize + zipbuff.Length;
		//	msgHeader.pads = new int[]{0,0,0};
		//	DVRDataMessage dvrmsg = new DVRDataMessage();
		//	dvrmsg.Header = msgHeader;
		//	dvrmsg.Buffer = zipbuff;
		//	return dvrmsg;
		//}

	}
}
