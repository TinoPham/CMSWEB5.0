using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Commons.Resources;
using Commons;
using System.IO;
namespace PACDMConverter.DVRConverter
{
	internal class DVRSocketClient: IDisposable
	{
		public event DVRUtils.SocketEvent OnSocketEvent;
		public event DVRUtils.SocketReceivedData OnSocketReceivedData;
		readonly ManualResetEvent evt_sendcomplete;
		private class DVRMessageSendState : IDisposable
		{
			public SocketError SocketState{ get; set;}

			public DVRUtils.SOCKET_BUFF_TYPE bufftype{ get; set;}
			/// <summary>
			///bytes already received.
			/// </summary>
			public uint DataLength { get; set;}
			/// <summary>
			/// Total data bytes.
			/// </summary>
			public uint TotalLength {  get{ return buffer == null? 0 : (uint)buffer.Length ;}}
			// Client  socket.
			public Socket workSocket{ get; set;}
			// Size of receive buffer.
			private byte[] buffer;
			public byte[] BufferMessage{
				get{ return buffer; }
				private set{ buffer = value;}
			}

			public ManualResetEvent CompleteHandle{ get; private set;}
			
			public DVRMessageSendState(DVRDataMessage dvrmessage)
			{
				ResetData();
				buffer = new byte[dvrmessage.Header.size];
				Array.Copy(Commons.ObjectUtils.StructuretoBytes<MsgHeader>(dvrmessage.Header), 0, buffer, 0, DVRDataMessage.HeaderSize);

				Array.Copy(dvrmessage.Buffer, 0, buffer, (int)DVRDataMessage.HeaderSize, dvrmessage.Buffer.Length); 
			}
			
			public void Dispose()
			{
				if( buffer != null)
					buffer = null;
			}
			private void ResetData()
			{
			
				CompleteHandle = new ManualResetEvent(false);
				CompleteHandle.Reset();
				bufftype = DVRUtils.SOCKET_BUFF_TYPE.MSG_HEADER;
			}
		}
		
		private class DVRMessageState
		{
			public DVRUtils.SOCKET_BUFF_TYPE bufftype{ get; set;}
			/// <summary>
			/// Current buffer size
			/// </summary>
			public int CurrentLength{ get{ return MsgStream == null? 0 : (int)MsgStream.Length; }}
			/// <summary>
			/// Message length
			/// </summary>
			public int MsgLength { get{ return MsgHeader.msg_id == 0? DVRDataMessage.HeaderSize : MsgHeader.size;}}

			public int RequestLength
			{
				get{
						if( bufftype ==  DVRUtils.SOCKET_BUFF_TYPE.MSG_HEADER || MsgHeader.msg_id == 0)
							return DVRDataMessage.HeaderSize -CurrentLength > DVRUtils.MAX_SOCKET_BUFFER? DVRUtils.MAX_SOCKET_BUFFER : DVRDataMessage.HeaderSize;
						return MsgLength - CurrentLength >  DVRUtils.MAX_SOCKET_BUFFER?  DVRUtils.MAX_SOCKET_BUFFER : MsgLength - CurrentLength;
				}
			}
			
			public MsgHeader MsgHeader{ get; set;}
			// Client  socket.
			public Socket workSocket{ get; set;}
			MemoryStream MsgStream = new MemoryStream();

			// Size of receive buffer.
			public byte[] buffer = new byte[DVRUtils.MAX_SOCKET_BUFFER];


			public void ResetBuffer()
			{
				if (MsgStream != null)
				{
					MsgStream.Dispose();
					MsgStream = null;
				}

				MsgHeader = new MsgHeader();
				MsgStream = new MemoryStream();
			}

			private byte[] GetBuffer( int start = 0, int length = 0)
			{
				if( start >= (int)MsgStream.Length)
					return null;
				MsgStream.Seek(start, SeekOrigin.Begin);
				int read_length =  (length == 0 || ( start + length > MsgStream.Length) ) ? (int)MsgStream.Length - start  : length;
				byte[] ret = new byte[read_length];
				MsgStream.Read(ret, 0, read_length);
				return ret;
					
			}
			
			public byte[] GetMessageBuffer()
			{
				return GetBuffer(0, 0);
			}
			public byte[] GetHeaderBuffer()
			{
				return GetBuffer(0, DVRDataMessage.HeaderSize);
			}

			public byte[] GetDataBuffer()
			{
				return GetBuffer( DVRDataMessage.HeaderSize,0);
			}

			public void SetBufferData(byte[] buff, int start = 0, int total = Int32.MaxValue)
			{
				if (buff == null || buff.Length == 0 || start < 0)
					return;
				if (total > buff.Length - start)
					total = buff.Length - start;

				MsgStream.Seek(0, SeekOrigin.End);
				MsgStream.Write(buff, 0, total);
			}
			public DVRDataMessage ToDVRMessage()
			{
				return new DVRDataMessage{ Header = this.MsgHeader, Buffer = GetDataBuffer()};
			}

		}

		Socket dvrConnection;

		volatile bool mstop = false;

		public int RemotePort{ get{ return dvrConnection == null? 0 : ((IPEndPoint)dvrConnection.RemoteEndPoint).Port;}}

		public string RemoteIP { get { return dvrConnection == null ? string.Empty : ((IPEndPoint)dvrConnection.RemoteEndPoint).Address.ToString(); } }

		public int LocalPort { get { return dvrConnection == null ? 0 : ((IPEndPoint)dvrConnection.LocalEndPoint).Port; } }

		public string LocalIP { get { return dvrConnection == null ? string.Empty : ((IPEndPoint)dvrConnection.LocalEndPoint).Address.ToString(); } }

		public Socket DVRSocket{ get{ return dvrConnection;}}

		public string SocketID{ get; private set; }
		
		public DVRSocketClient( Socket socket)
		{
			dvrConnection = socket;
			SocketID = Guid.NewGuid().ToString();
			evt_sendcomplete = new ManualResetEvent(false);
			evt_sendcomplete.Set();
		}

		public void Dispose()
		{
			CloseSocket();
		}

		public void BeginReceive()
		{
			DVRMessageState ReceivedObject = new DVRMessageState();
			ReceivedObject.workSocket = dvrConnection;
			dvrConnection.BeginReceive(ReceivedObject.buffer, 0, (int)ReceivedObject.RequestLength, SocketFlags.None, DVR_ReceiveCallback, ReceivedObject);
		}

		public SocketError BeginSend(DVRDataMessage i3Comdata)
		{
			if ( mstop == true || dvrConnection == null || !dvrConnection.Connected)
				return SocketError.Shutdown;
			try
			{
				evt_sendcomplete.Reset();
				DVRMessageSendState SendObject = new DVRMessageSendState(i3Comdata);
				SendObject.workSocket = dvrConnection;
				int buff_len = SendObject.TotalLength > DVRUtils.MAX_SOCKET_BUFFER ? DVRUtils.MAX_SOCKET_BUFFER : (int)SendObject.TotalLength;
				dvrConnection.BeginSend(SendObject.BufferMessage , 0 , buff_len , SocketFlags.None, DVR_SendCallback, SendObject);
				SendObject.CompleteHandle.WaitOne();
				evt_sendcomplete.Set();
				return SendObject.SocketState;
			}
			catch(Exception)
			{
				return SocketError.SocketError;
			}
		}

		private void DVR_SendCallback(IAsyncResult ar)
		{
			DVRMessageSendState SendState = ar.AsyncState as DVRMessageSendState;
			Socket DVRSocket = SendState.workSocket;
			SocketError skerror = SocketError.SocketError;

			try
			{
				int length = DVRSocket.EndSend(ar, out skerror);
				
				if( skerror != SocketError.Success)
				{
					SendState.SocketState = skerror;
					SendState.CompleteHandle.Set();
					return;
				}
				SendState.DataLength += (uint)length;
				if( SendState.DataLength == SendState.TotalLength)
				{
					SendState.CompleteHandle.Set();
				}
				else
				{
					uint remain_len = (SendState.TotalLength - SendState.DataLength);
					if (mstop == false)
						DVRSocket.BeginSend(SendState.BufferMessage, (int)SendState.DataLength, remain_len > DVRUtils.MAX_SOCKET_BUFFER? DVRUtils.MAX_SOCKET_BUFFER : (int)remain_len , SocketFlags.None, DVR_SendCallback, SendState);
					else
						SendState.CompleteHandle.Set();

				}
			}
			catch(Exception)
			{
				SendState.SocketState = SocketError.SocketError;
				SendState.CompleteHandle.Set();
			}

		}
		
		private void DVR_ReceiveCallback(IAsyncResult ar)
		{
			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			DVRMessageState ReceivedObject = ar.AsyncState as DVRMessageState;
			Socket DVRSocket = ReceivedObject.workSocket;
			SocketError skerror = SocketError.SocketError;
			try
			{
				int bytesRead = DVRSocket.EndReceive(ar, out skerror);
				if( bytesRead == 0)
				{
					OnSocketEventArgs(new Events.SocketEvent { Sender = this, ErrorCode = ERROR_CODE.CMS_SOCKET_RECEIVED_ERROR, SockError = SocketError.Shutdown, Message = ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CMS_SOCKET_SHUTDOWN, Utils.Instance.CultureInfo) });
					return;
				}
				ReceivedObject.SetBufferData( ReceivedObject.buffer, 0, bytesRead);

				switch( ReceivedObject.bufftype)
				{
					case DVRUtils.SOCKET_BUFF_TYPE.MSG_HEADER:
						
						if( ReceivedObject.MsgLength == DVRDataMessage.HeaderSize)
						{
							ReceivedObject.MsgHeader = Commons.ObjectUtils.BytestoStructure<MsgHeader>( ReceivedObject.GetHeaderBuffer());
							ReceivedObject.bufftype = DVRUtils.SOCKET_BUFF_TYPE.MSG_DATA;
						}
						break;

					case DVRUtils.SOCKET_BUFF_TYPE.MSG_DATA:

						if (ReceivedObject.CurrentLength == ReceivedObject.MsgLength)
						{
							OnSocketReceivedDataEventArgs( new Events.SocketReceivedData{ Sender = this, ErrorCode = ERROR_CODE.OK, Data = ReceivedObject.ToDVRMessage() } );
							ReceivedObject.ResetBuffer();
							ReceivedObject.bufftype = DVRUtils.SOCKET_BUFF_TYPE.MSG_HEADER;
						}
						
						break;
				}
				if(!mstop)
					DVRSocket.BeginReceive(ReceivedObject.buffer, 0, (int)ReceivedObject.RequestLength, SocketFlags.None, DVR_ReceiveCallback, ReceivedObject);
			}
			catch(Exception)
			{
				OnSocketEventArgs(new Events.SocketEvent { Sender = this, ErrorCode = ERROR_CODE.CMS_SOCKET_RECEIVED_ERROR, SockError = SocketError.SocketError, Message = ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CMS_SOCKET_ERROR, Utils.Instance.CultureInfo) });
			}
		}

		private void OnSocketReceivedDataEventArgs( Events.SocketReceivedData args)
		{
			if (OnSocketReceivedData == null || mstop)
				return;
				OnSocketReceivedData(this, args);
		}
		
		private void OnSocketEventArgs(Events.SocketEvent eventargs)
		{
			if (OnSocketEvent == null || mstop)
				return;
			OnSocketEvent(this, eventargs);
		}

		private void CloseSocket()
		{
			mstop = true;
			if( dvrConnection == null)
				return;
			try
			{
				evt_sendcomplete.WaitOne(5000);
				if (dvrConnection.Connected)
					dvrConnection.Shutdown(SocketShutdown.Both);

				dvrConnection.Close();
				dvrConnection = null;
			}
			catch(Exception)
			{
				dvrConnection = null;
			}
			
		}
	}
}
