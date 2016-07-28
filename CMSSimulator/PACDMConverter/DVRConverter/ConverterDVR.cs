using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using System.Diagnostics;

namespace PACDMSimulator.DVRConverter
{
	internal class ConverterDVR : IDisposable
	{
		const string XPath_MessageID = "/message/header/id";

		enum EnumControl:int
		{
			EVT_DVR_SEND_MESSAGE = 0,
			EVT_API_RESPONSE_MESSAGE
		}

		ConvertDB localDB = null;
		DVRSocketServer DVRManager;
		//readonly ConcurrentBag<DVRSocketClient> DVRMessages = new ConcurrentBag<DVRSocketClient>();
		readonly HttpClientSingleton serviceapi;
		readonly MediaTypeFormatter DataFormatter = new JsonMediaTypeFormatter();

		
		Task MsgProcessTask;

		readonly CancellationTokenSource MsgProcessTaskTokenSource = new CancellationTokenSource();

		public ConverterDVR( ConvertDB localDb)
		{
			localDB = localDb;
			serviceapi = new HttpClientSingleton( localDb.ServiceConfig);
			DVRManager = new DVRSocketServer( localDB.DvrConverter);
			DVRManager.OnSocketEvent += DVRManager_OnSocketEvent;
			DVRManager.OnSocketReceivedData += DVRManager_OnSocketReceivedData;
		}

		public ConverterDVR(ConvertDB localDb,int socketPort = 0)
		{
			localDB = localDb;
			serviceapi = new HttpClientSingleton(localDb.ServiceConfig);
			localDB.DvrConverter.TCPPort = socketPort;
			DVRManager = new DVRSocketServer(localDB.DvrConverter);
			DVRManager.OnSocketEvent += DVRManager_OnSocketEvent;
			DVRManager.OnSocketReceivedData += DVRManager_OnSocketReceivedData;
		}
		
		public void Dispose()
		{
		}

		public void StopDVRConvert()
		{
			if( DVRManager != null)
			{
				DVRManager.Dispose();
				DVRManager = null;
			}
		}

		public void StartDVRConverter()
		{
			if( !DVRManager.StartDVRListner())
				return;

		}

		private void StartProcessMessage()
		{
			AutoResetEvent[]evts = new  AutoResetEvent[ Enum.GetValues(typeof(EnumControl)).Length];
			for(int i =0 ;i< evts.Length; i++)
				evts[i] = new AutoResetEvent(false);

			MsgProcessTask = Task.Factory.StartNew(() => DVRMessageProc(MsgProcessTaskTokenSource.Token, localDB, evts), TaskCreationOptions.LongRunning);
		}

		private void DVRMessageProc(CancellationToken CancelToken, ConvertDB localDB, AutoResetEvent[] evts)
		{
			while (CancelToken.IsCancellationRequested)
			{

			}
		}

		void DVRManager_OnSocketReceivedData(object sender, Events.SocketReceivedData eventargs)
		{
			if (eventargs == null || eventargs.Data == null)
			{
				Utils.WriteToLogFile("Data send to  Converter is Null", "");
				throw new NotImplementedException();
			}

			string sXML = DVRMessagetoDataString(eventargs.Data);

			if (String.IsNullOrEmpty(sXML) || eventargs.Data.Header.msg_id == (int)CMSMsg.MSG_DVR_DISCONNECT)
			{
				Utils.WriteToLogFile("XML of DVR error", "");
				return;
			}
			
			try
			{
			
				MessageData msg = new MessageData { Programset = Commons.Programset.DVR, Mapping = eventargs.Data.Header.msg_id.ToString(), Data = sXML };

				//send to WebAPI
				Utils.WriteToLogFile("WEBAPI-> Send To WEBAPI:", sXML);
				Stopwatch timer = new Stopwatch();
				timer.Start();
				MessageResult result = serviceapi.PostData(msg, typeof(MessageData), DataFormatter);
				timer.Stop();
				Utils.WriteToLogFile("Timer:", timer.ElapsedMilliseconds.ToString());
				timer.Reset();

				if (result.ErrorID !=  Commons.ERROR_CODE.OK)
				{
					Utils.WriteToLogFile("Send to Web API Error", "");
					localDB.Insert<Log>( new Log{ Owner = true, ProgramSet = (byte)Commons.Programset.DVR, DVRDate = DateTime.Now, Message = result.Data, LogID = (int)result.ErrorID});
				}

				string alldata = result.Data;
				
				if (!String.IsNullOrEmpty(alldata))
				{
					var dataBase64 = alldata.Split(',');
					foreach (var tBase64 in dataBase64)
					{
						Utils.WriteToLogFile("DVR-> Send to DVR: ", Commons.Utils.Base64toString(tBase64));
						SendToDVR(Commons.Utils.Base64toString(tBase64));
					}
				}

				
			}
			catch(Exception ex)
			{
				throw new Exception("Communicate error", ex.InnerException);
			}
			//Anh, Create msg for DVR, Mar 17, 2014
		}

		void DVRManager_OnSocketEvent(object sender, Events.SocketEvent eventargs)
		{
			var ab = sender;
			if (ab == null)
			{
				throw new NotImplementedException();
			}
		}

		private void SendToDVR(string xmlStr)
		{
			DVRDataMessage msg = StringMessage2DVRMessage(xmlStr);
			if( msg.Header.msg_id == 0)
				return;
			DVRManager.SendToAllDVRs(msg);
		}

		private string DVRMessagetoDataString(DVRDataMessage DVRMessage )
		{
			MsgHeader Header = DVRMessage.Header;
			if (Header.msg_id == (int)CMSMsg.MSG_DVR_GET_SNAPSHOT_RESPONSE || Header.msg_id == (int)CMSMsg.MSG_DVR_GET_VIDEO_RESPONSE)
				return Commons.Utils.ToBase64String(DVRMessage.Buffer);
			else
			{
				byte[] unzip = Utils.UnZipData(DVRMessage.Buffer, Header.size - DVRDataMessage.HeaderSize);
				return unzip == null? string.Empty : Commons.Utils.ByteArr2String( unzip );
				
			}
		}

		private ushort GetMsgID(string tXml)
		{
			XmlNode nodeid =  Commons.XMLUtils.SelectNode( Commons.XMLUtils.XMLDocument(tXml).DocumentElement, XPath_MessageID ); 
			if( nodeid == null)
				return 0;
			UInt16 ret = 0;
			UInt16.TryParse(nodeid.InnerText, out ret);
			return ret;
		}

		private DVRDataMessage StringMessage2DVRMessage( string  message) 
		{
			byte[]buff = Commons.Utils.String2Byte(message);
			byte[] zipbuff = Utils.ZipData(buff, buff.Length);

			MsgHeader msgHeader = new MsgHeader();
			msgHeader.msgBegin = DVRDataMessage.BEGIN_MSG;
			msgHeader.msg_id = GetMsgID(message);
			msgHeader.size = DVRDataMessage.HeaderSize + zipbuff.Length;
			msgHeader.pads = new int[]{0,0,0};
			DVRDataMessage dvrmsg = new DVRDataMessage();
			dvrmsg.Header = msgHeader;
			dvrmsg.Buffer = zipbuff;
			return dvrmsg;
		}


	}
}
