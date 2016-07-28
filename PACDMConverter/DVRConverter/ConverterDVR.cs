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
using System.IO;
using System.Net.Sockets;
namespace PACDMConverter.DVRConverter
{
	internal class ConverterDVR : IDisposable
	{
		const string XPath_MessageID = "/message/header/id";
		const string configuration_id = "<configuration_id>6</configuration_id>";
		
		enum EnumControls:int
		{
			EVT_DVR_TOKEN_EXPIRED = 0,
			EVT_DVR_ACCEPTED,
			EVT_DVR_CONNECT,
			EVT_DVR_DISCONNECT,
			EVT_EXIT,
			EVT_DVR_PENDING_MESSAGE,
			EVT_DVR_XML_CONFIG_MESSAGE,
			EVT_DVR_SEND_MESSAGE,
			EVT_OFFLINE_SEND_MESSAGE,
			
			EVT_COUNT
		}

		ConvertDB localDB = null;
		DVRSocketServer DVRManager;
		
		ApiService serviceapi;
		//readonly MediaTypeFormatter DataFormatter = new JsonMediaTypeFormatter();
		ConcurrentQueue<MessageData> DVRMessages;
		MessageData DVRConnectMessage = null;

		CancellationToken CancelToken;

		Task TaskMessage;
		readonly DVRDataMessage DVRMsg_pending;
		readonly DVRDataMessage DVRMsg_Config_change;
		readonly DVRDataMessage DVRMsg_Network_Connect;
		readonly MessageData DVRMsg_Disconnect;
		volatile string TokenID;
		TimeSpan KeepOfflineTimeout;
		volatile int _keepofflinetimeout;
		WaitHandle[] WHandles;
		public event Events.ApitokenExpired OnApiTokenExpired;
		public ConverterDVR( ConvertDB localDb, CancellationToken CancelToken, string TokenID, int keepofflinetimeout)
		{
			string xmlMsg = string.Format( DVRUtils.MSG_CONNECT_RESPONSE, (int)CMSMsg.MSG_DVR_CONNECT_PENDING,  CMSMsg.MSG_DVR_CONNECT_PENDING.ToString());
			DVRMsg_pending = StringMessage2DVRMessage( xmlMsg);
			xmlMsg = string.Format(DVRUtils.MSG_CONNECT_RESPONSE, (int)CMSMsg.MSG_DVR_XML_CONFIG_CHANGED, CMSMsg.MSG_DVR_XML_CONFIG_CHANGED.ToString());
			DVRMsg_Config_change = StringMessage2DVRMessage(xmlMsg);
			xmlMsg = string.Format(DVRUtils.MSG_CONNECT_RESPONSE, (int)CMSMsg.MSG_DVR_NETWORK_CONNECTED, CMSMsg.MSG_DVR_XML_CONFIG_CHANGED.ToString());
			DVRMsg_Network_Connect = StringMessage2DVRMessage(xmlMsg);
			xmlMsg = string.Format(DVRUtils.MSG_DVR_DISCONNECT, (int)CMSMsg.MSG_DVR_DISCONNECT, CMSMsg.MSG_DVR_DISCONNECT.ToString());
			DVRMsg_Disconnect = new MessageData { Programset = Commons.Programset.DVR, Data = xmlMsg, Mapping = ((int)CMSMsg.MSG_DVR_DISCONNECT).ToString()};

			DVRMessages = new ConcurrentQueue<MessageData>();
			this.CancelToken = CancelToken;
			this.TokenID = TokenID;
			localDB = localDb;
			if( !string.IsNullOrEmpty(TokenID))
				serviceapi = new ApiService(localDb.ServiceConfig, TokenID);
			DVRManager = new DVRSocketServer(localDB.DvrConverter, Utils.Instance.DVRSocketAllow);
			DVRManager.OnSocketEvent += DVRManager_OnSocketEvent;
			DVRManager.OnSocketReceivedData += DVRManager_OnSocketReceivedData;
			KeepOfflineTimeout = new TimeSpan(0, keepofflinetimeout, 0);

		}
		
		public void Dispose()
		{
			if (serviceapi != null)
				serviceapi.Dispose();
		}

		public void StopDVRConvert()
		{
			StopTaskMessage();
			if( DVRManager != null)
			{
				DVRManager.OnSocketEvent -= DVRManager_OnSocketEvent;
				DVRManager.OnSocketReceivedData -= DVRManager_OnSocketReceivedData;
				
				DVRManager.Dispose();
				DVRManager = null;
			}
		}

		public void StartDVRConverter()
		{
			try
			{
				StartTaskMessage();
				if (!DVRManager.StartDVRListner())
					return;
			}
			catch (Exception ex)
			{
				localDB.AddLog(new Log { LogID = (byte)Commons.ERROR_CODE.CMS_SOCKET_ERROR, DVRDate = DateTime.Now, Message = ex.Message, Owner = true, ProgramSet = (byte)Commons.Programset.DVR });
			}
		}

		public void DVRXMLConfigChange()
		{
			TriggerEvent(EnumControls.EVT_DVR_XML_CONFIG_MESSAGE);
		}

		public void DVRInfoChange( string token, int keepofflinetimeout)
		{
			this.TokenID = token;
			_keepofflinetimeout = keepofflinetimeout;
			TriggerEvent(EnumControls.EVT_DVR_ACCEPTED);
		}

		private void StartTaskMessage()
		{
			InitTaskMessageEvents();
			TaskMessage = Task.Factory.StartNew( () => TaskMessageProc(WHandles), TaskCreationOptions.LongRunning);
		}

		private void StopTaskMessage()
		{
			if( TaskMessage != null)
			{
				TriggerEvent( EnumControls.EVT_EXIT);
				TaskMessage.Wait();
			}
		}

		private void InitTaskMessageEvents()
		{
			WHandles = new WaitHandle[(int)EnumControls.EVT_COUNT];
			int count = WHandles.Length;
			for( int i = 0; i < count; i++)
			WHandles[i] = new AutoResetEvent(false);
		}

		private void TaskMessageProc(WaitHandle[] Handles)
		{
			bool m_stop = false;
			int index = -1;
			string ResponseData = null;
			MessageData DVRMessageData = null;
			MessageResult ResponseResult = null;
			TimeSpan timeout_event = new TimeSpan(0,1, 0);
			DateTime Last_offline_trigger = DateTime.UtcNow;
			if(OfflineMessages.Instance.OfflineMessageCount > 0)
				TriggerEvent( EnumControls.EVT_OFFLINE_SEND_MESSAGE);

			bool network_pending = false;
			while(!m_stop && !CancelToken.IsCancellationRequested)
			{
				index = WaitHandle.WaitAny(Handles, timeout_event);
				switch(index)
				{
					case(int) EnumControls.EVT_DVR_TOKEN_EXPIRED:
							TokenID = null;
							serviceapi = null;
						break;

					case (int)EnumControls.EVT_DVR_ACCEPTED:
							localDB.Refresh<ServiceConfig>();
							if(!string.IsNullOrEmpty(TokenID))
							{
								serviceapi = new ApiService(localDB.ServiceConfig, TokenID);
								KeepOfflineTimeout = new TimeSpan(0, _keepofflinetimeout, 0);
							}
							else 
								serviceapi = null;

							TriggerEvent(EnumControls.EVT_DVR_CONNECT);
						break;

					case (int)EnumControls.EVT_DVR_CONNECT:
							if( DVRConnectMessage == null)
								break;
							OfflineMessages.Instance.CleanUpMessage(KeepOfflineTimeout.TotalMinutes, CancelToken);
							if (string.IsNullOrEmpty(TokenID) || serviceapi == null)
							{
								TriggerEvent(EnumControls.EVT_DVR_PENDING_MESSAGE);
								break;
							}
							DVRMessageData = DVRConnectMessage.Clone() as MessageData;
							ResponseResult = serviceapi.PostData(DVRMessageData, typeof(MessageData), CancelToken, Utils.Instance.DVRFormatter);
							if( ResponseResult.ErrorID == Commons.ERROR_CODE.OK)
							{
								ResponseData = ResponseResult == null? null : ResponseResult.Data;
								ResponseMessageToDVR(ResponseData,CancelToken);
							}
							else
							{
								if (ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_INVALID)
									TriggerEvent(EnumControls.EVT_DVR_ACCEPTED);
							}

					break;
					case (int) EnumControls.EVT_DVR_DISCONNECT:
						if (string.IsNullOrEmpty(TokenID) || serviceapi == null)
							break;
						serviceapi.PostData(DVRMsg_Disconnect, typeof(MessageData), CancelToken, Utils.Instance.DVRFormatter);
					break;
					case (int)EnumControls.EVT_DVR_PENDING_MESSAGE:
								network_pending = true;
							 DVRManager.SendToAllDVRs(DVRMsg_pending);
					break;

					case (int)EnumControls.EVT_DVR_XML_CONFIG_MESSAGE:
							DVRManager.SendToAllDVRs(DVRMsg_Config_change);
					break;

					case (int)EnumControls.EVT_OFFLINE_SEND_MESSAGE:
						#region
						Last_offline_trigger = DateTime.UtcNow;
					
						long key = 0;
						OfflineMessages.OfflineMessage offMsg = OfflineMessages.Instance.GetOfflineMessage(out key);
						bool continue_msg = true;
						if(offMsg == null)
							goto Label_Next;
						if (key > 0 && key < DateTime.UtcNow.Ticks - KeepOfflineTimeout.Ticks)
						{
							OfflineMessages.Instance.CleanUpMessage(KeepOfflineTimeout.TotalMinutes, CancelToken);
							goto Label_Next;
						}

						if (string.IsNullOrEmpty(TokenID) || serviceapi == null)
							break;


						if( offMsg.Direction == true)//to api
						{
							DVRMessageData = new MessageData{ Data = offMsg.Data, Mapping = offMsg.Mapping, Programset= Commons.Programset.DVR};
							ResponseResult = serviceapi.PostData(DVRMessageData, typeof(MessageData), CancelToken, Utils.Instance.DVRFormatter);
							if (ResponseResult != null && (ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_INVALID) )
							{
								if (OnApiTokenExpired != null)
									OnApiTokenExpired(this, ResponseResult.ErrorID);
								TriggerEvent(EnumControls.EVT_DVR_TOKEN_EXPIRED);
								break;
							}

							if (ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_ERR_XML_LOAD_ERR || ResponseResult.ErrorID == Commons.ERROR_CODE.INVALID_MAPPING 
							|| ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_ERR_XML_DESERIALIZE || ResponseResult.ErrorID == Commons.ERROR_CODE.OK)
							{
								OfflineMessages.Instance.RemoveOfflineMessage(key);
								ResponseData = ResponseResult == null ? null : ResponseResult.Data;
								ResponseMessageToDVR(ResponseData,CancelToken);
								if( network_pending)
								{
									network_pending = false;
									DVRManager.SendToAllDVRs(DVRMsg_Network_Connect);
								}
							}
							else
							{
								continue_msg = false;
								if (network_pending == false)
								{
									network_pending = true;
									TriggerEvent(EnumControls.EVT_DVR_PENDING_MESSAGE);
								}
							}
						}
						else
						{
							DVRDataMessage msg = StringMessage2DVRMessage(Commons.Utils.Base64toString(offMsg.Data));
							if( DVRManager.SendToAllDVRs(msg) == System.Net.Sockets.SocketError.Success)
								OfflineMessages.Instance.RemoveOfflineMessage(key);
							else
								continue_msg = false;
						}

						goto Label_Next;

						Label_Next:
						{
							if( OfflineMessages.Instance.OfflineMessageCount > 0 && continue_msg)
								TriggerEvent(EnumControls.EVT_OFFLINE_SEND_MESSAGE);
							else
								{
									if( DVRMessages.Count > 0)
										TriggerEvent( EnumControls.EVT_DVR_SEND_MESSAGE);
								}
						}
						
					break;
						#endregion

					case (int)EnumControls.EVT_DVR_SEND_MESSAGE:
						{
							if (string.IsNullOrEmpty(TokenID) || serviceapi == null)
							{
								TriggerEvent(EnumControls.EVT_DVR_PENDING_MESSAGE);
								break;
							}

							if (!DVRMessages.TryDequeue(out DVRMessageData))
								break;

							if (OfflineMessages.Instance.OfflineMessageCount > 0)//send out offline msg before
							{
								//TriggerEvent(EnumControls.EVT_OFFLINE_SEND_MESSAGE);
								OfflineMessages.Instance.AddOfflineMessage( DVRMessageData, true);
								if( Last_offline_trigger.Ticks + timeout_event.Ticks < DateTime.UtcNow.Ticks )
								{
									TriggerEvent(EnumControls.EVT_OFFLINE_SEND_MESSAGE);
								}
								break;
							}

							ResponseResult = serviceapi.PostData(DVRMessageData, typeof(MessageData), CancelToken, Utils.Instance.DVRFormatter);

							if( ResponseResult == null)
								break;
							if (ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_LOCKED_BY_ADMIN || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || ResponseResult.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_INVALID)
							{
								if (OnApiTokenExpired != null)
									OnApiTokenExpired( this, ResponseResult.ErrorID);
								TriggerEvent(EnumControls.EVT_DVR_TOKEN_EXPIRED);
								break;
							}

							bool send_ok = ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_ERR_XML_LOAD_ERR || ResponseResult.ErrorID == Commons.ERROR_CODE.INVALID_MAPPING || ResponseResult.ErrorID == Commons.ERROR_CODE.DVR_ERR_XML_DESERIALIZE || ResponseResult.ErrorID == Commons.ERROR_CODE.OK;

							if (send_ok == false)
							{
								localDB.Insert<Log>(new Log { Owner = true, ProgramSet = (byte)Commons.Programset.DVR, DVRDate = DateTime.Now, Message = ResponseResult.Data, LogID = (int)ResponseResult.ErrorID });
								if (ResponseResult.httpStatus == System.Net.HttpStatusCode.NotFound || ResponseResult.httpStatus == System.Net.HttpStatusCode.RequestTimeout || ResponseResult.httpStatus == System.Net.HttpStatusCode.Forbidden)
								{
									OfflineMessages.Instance.AddOfflineMessage(DVRMessageData, true);
									if(network_pending == false)
									{
										network_pending = true;
										TriggerEvent(EnumControls.EVT_DVR_PENDING_MESSAGE);

									}
								}
								break;
							}
							if(network_pending == true)
							{
								network_pending = false;
								DVRManager.SendToAllDVRs(DVRMsg_Network_Connect);
							}

							ResponseData = ResponseResult == null? null : ResponseResult.Data;

							ResponseMessageToDVR( ResponseData, CancelToken);
							if( DVRMessages.Count > 0)
								TriggerEvent(EnumControls.EVT_DVR_SEND_MESSAGE);
						}
					break;

					case (int)EnumControls.EVT_EXIT:
					if (string.IsNullOrEmpty(TokenID) == false && serviceapi != null && network_pending == false)
						{
							serviceapi.PostData(DVRMsg_Disconnect, typeof(MessageData), CancelToken, Utils.Instance.DVRFormatter);
						}
						m_stop = true;
					break;

					default: {
								if( OfflineMessages.Instance.OfflineMessageCount > 0)
									TriggerEvent( EnumControls.EVT_OFFLINE_SEND_MESSAGE);
								else if( DVRMessages.Count > 0)
									TriggerEvent(EnumControls.EVT_DVR_SEND_MESSAGE);
							break;
					}
				}
			}
			if(DVRMessages.Count > 0)
			{
				MessageData DVRMsg = null;
				while( DVRMessages.TryDequeue(out DVRMsg) )
				{
					OfflineMessages.Instance.AddOfflineMessage(DVRMsg, true);
				}
			}
		}

		private void ResponseMessageToDVR( string apiResponse, CancellationToken calceltoken)
		{
			if( string.IsNullOrEmpty(apiResponse))
				return;

			if( calceltoken.IsCancellationRequested )
				return;

			string[] allbase64 = apiResponse.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			SocketError socket_result = SocketError.Success;
			foreach (string base64 in allbase64)
			{
				if (!calceltoken.IsCancellationRequested)
				{
					DVRDataMessage msg = StringMessage2DVRMessage(Commons.Utils.Base64toString(base64));
					if (msg == null || msg.Header.msg_id == 0)
						continue;

					socket_result = DVRManager.SendToAllDVRs(msg);
					if (socket_result != SocketError.Success && socket_result != SocketError.NoData)
						OfflineMessages.Instance.AddOfflineMessage(new MessageData { Data = base64 }, false);
				}
			}

		}

		private void TriggerEvent( EnumControls evt)
		{
			if( WHandles == null || WHandles.Length == 0)
				return;

			(WHandles[(int)evt] as AutoResetEvent).Set();
		}

		void DVRManager_OnSocketReceivedData(object sender, Events.SocketReceivedData eventargs)
		{
			if (eventargs == null || eventargs.Data == null)
			{
				return;// throw new NotImplementedException();
			}

			try
			{

				string sXML = DVRMessagetoDataString(eventargs.Data);

				if (String.IsNullOrEmpty(sXML)/*|| eventargs.Data.Header.msg_id == (int)CMSMsg.MSG_DVR_DISCONNECT*/)
					return;


				if (IsConfigMsg(eventargs.Data.Header.msg_id) && sXML.IndexOf(configuration_id, StringComparison.InvariantCultureIgnoreCase) > 0)
				{
					sXML = ProcessScheduleData(sXML);
				}
				

				MessageData msg = new MessageData { Programset = Commons.Programset.DVR, Mapping = eventargs.Data.Header.msg_id.ToString(), Data = sXML };
				
				if (eventargs.Data.Header.msg_id == (int)CMSMsg.MSG_DVR_CONNECT)
				{
					DVRConnectMessage  = msg.Clone() as MessageData;
					TriggerEvent( EnumControls.EVT_DVR_CONNECT);
				}
				else
				{
					DVRMessages.Enqueue(msg);
					TriggerEvent(EnumControls.EVT_DVR_SEND_MESSAGE);
				}
			}
			catch (Exception ex)
			{
				//throw new Exception("Communicate error", ex.InnerException);
				localDB.AddLog(new Log { LogID = (byte)Commons.ERROR_CODE.CMS_SOCKET_ERROR, DVRDate = DateTime.Now, Message = ex.Message, Owner = true, ProgramSet = (byte)Commons.Programset.DVR });
			}
			//Anh, Create msg for DVR, Mar 17, 2014
		}

		void DVRManager_OnSocketEvent(object sender, Events.SocketEvent eventargs)
		{
			if( eventargs.SockError == SocketError.ConnectionReset || eventargs.SockError == SocketError.Disconnecting || eventargs.SockError == SocketError.Fault
			|| eventargs.SockError == SocketError.HostDown || eventargs.SockError == SocketError.NetworkDown || eventargs.SockError == SocketError.NotConnected || eventargs.SockError == SocketError.Shutdown)
				TriggerEvent(EnumControls.EVT_DVR_DISCONNECT);
		}
	
		private void ResponseMessages( string base64messages)
		{
			if( string.IsNullOrEmpty(base64messages))
				return;
			string[] dataBase64 = base64messages.Split(',');
			foreach (string tBase64 in dataBase64)
			{
				DVRDataMessage msg = StringMessage2DVRMessage(Commons.Utils.Base64toString(tBase64));
				if (msg.Header.msg_id == 0)
					continue;
				DVRManager.SendToDVRs(msg, false);
			
			}

			DVRManager.SendToDVRs((DVRDataMessage)null, true);
		}

		private void SendToDVR(string xmlStr)
		{
			DVRDataMessage msg = StringMessage2DVRMessage(xmlStr);
			if( msg.Header.msg_id == 0)
				return;
			DVRManager.SendToDVRs(msg);
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
			try{
				if( string.IsNullOrEmpty(message))
					return new DVRDataMessage { Header = new MsgHeader { msg_id = 0 } };

				byte[]buff = Commons.Utils.String2Byte(message);
				//ZLibNet
				//ZLibCompressor
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
			catch(Exception)
			{
				return new DVRDataMessage{ Header = new MsgHeader{ msg_id = 0}};
			}
		}
		
		string ProcessScheduleData(string sXML)
		{
			string sStartTag = "<![CDATA[";
			string sEndTag = "]]>";
			int iStartLen = sStartTag.Length;
			int iEndLen = sEndTag.Length;

			int nOldStart = 0, nCDStart = 0, nCDEnd = 0;
			string sCData = string.Empty;
			string sValidXML = string.Empty;
			string sBase64 = string.Empty;
			nCDStart = sXML.IndexOf(sStartTag/*"<![CDATA["*/);
			if (nCDStart >= 0)
			{
				nCDEnd = sXML.IndexOf(sEndTag/*"]]>"*/, nCDStart);
				sValidXML = sXML.Substring(0, nCDStart + iStartLen);
			}
			int nCount = 0;
			try
			{
				while (nCDStart > 0 && nCDEnd > 0)
				{
					nCount++;
					sCData = sXML.Substring(nCDStart + iStartLen, nCDEnd - nCDStart - iStartLen);
					byte[] buffData = Encoding.UTF8.GetBytes(sCData);
					sBase64 = Convert.ToBase64String(buffData);
					if (buffData != null)
					{
						Array.Clear(buffData, 0, buffData.Length);
						buffData = null;
					}

					sValidXML += sBase64 + sEndTag/*"]]>"*/;
					nOldStart = nCDStart;
					nCDStart = sXML.IndexOf(sStartTag/*"<![CDATA["*/, nCDEnd + iEndLen);
					if (nCDStart > 0)
					{
						sValidXML += sXML.Substring(nCDEnd + iEndLen, nCDStart + iStartLen - nCDEnd - iEndLen);
						nCDEnd = sXML.IndexOf(sEndTag/*"]]>"*/, nCDStart);
					}
					else
					{
						sValidXML += sXML.Substring(nCDEnd + iEndLen);
						nCDEnd = -1;
					}
				}
			}
			catch (System.Exception)
			{
				sValidXML = string.Empty;
			}
			return sValidXML;
		}
		
		private bool IsConfigMsg(Int32 msgID)
		{
			switch (msgID)
			{
				case (int)CMSMsg.MSG_DVR_GET_CONFIG_RESPONSE:
				case (int)CMSMsg.MSG_DVR_CONFIG_CHANGED:
					return true;

				default:
					break;
			}
			return false;
		}
		
		

	}
}
