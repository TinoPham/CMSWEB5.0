using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace CMSTest
{
	public class ClientSocket
	{
		public delegate void SocketreceiveMessage(byte[] mbuff, int length);
		public event SocketreceiveMessage OnSocketreceiveMessage;
		public delegate void SocketClose(string cmsip, int cmsport);
		public event SocketClose OnSocketClose;

		public enum SOCKET_ERROR:int
		{
			INVALID_SOCKET = 0,
			CONNECTED_FAIL,
			CONECT_SUCCESSFUL,
			SEND_FAILED,
			RECEIVE_FAILED,
			SEND_SUCCESSFUL,

		}
		private const int NET_WORK_SLEEP_TIME = 100;
		private const int MAX_BUFFER_NETWORK = 4096;
		public const int DEFAULT_CMS_PORT = 1001;
		public const string DEFAULT_CMS_IP = "127.0.0.1";
		int m_cmsPort = DEFAULT_CMS_PORT;
		string m_cmsIP = DEFAULT_CMS_IP;
		Socket m_socket;
		ManualResetEvent m_waitevent;
		volatile bool m_stop = false;
		Thread m_ThreadReceive;
		public bool Stop
		{
			set { m_stop = value; }			
		}
		public bool Connected
		{
			get
			{
				if (m_socket == null)
					return false;
				return m_socket.Connected;
					
			}
		}
		public ClientSocket(string cmsIP, int cmsport)
		{
			m_cmsIP = cmsIP;
			m_cmsPort = cmsport;
		}
		private void CloseSocket()
		{
			if( m_socket != null)
			{
                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                }
				m_socket.Close();
				m_socket = null;
			}
		}
		private SOCKET_ERROR InitSocket(string ipadd, int port)
		{
			CloseSocket();
			try
			{
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_socket.Connect(ipadd, port);
				return SOCKET_ERROR.CONECT_SUCCESSFUL;
			}
			catch (System.Exception ex)
			{
				m_socket = null;
				return SOCKET_ERROR.CONNECTED_FAIL;
			}	
		}
		private SOCKET_ERROR SendSockData(byte[] mbuff, int length)
		{
			if (m_socket == null)
				return SOCKET_ERROR.INVALID_SOCKET;
			if (!m_socket.Connected)
				return SOCKET_ERROR.CONNECTED_FAIL;
			try
			{
				int mtotal = length;
				int status = 0;
				int curlength = MAX_BUFFER_NETWORK;
				byte[] buffsend = null;
				while ( status < mtotal && m_socket.Connected)
				{
					if (mtotal - status > MAX_BUFFER_NETWORK)
						curlength = MAX_BUFFER_NETWORK;
					else
						curlength = mtotal - status;
					buffsend = new byte[curlength];
					Array.Copy(mbuff, status, buffsend, 0, curlength);
					 int ret = m_socket.Send(mbuff, curlength, SocketFlags.None);
					status += ret;
				}
				if (status < mtotal)
					return SOCKET_ERROR.SEND_FAILED;
				else
					return  SOCKET_ERROR.SEND_SUCCESSFUL;
			}
			catch (System.Exception ex)
			{
				return SOCKET_ERROR.SEND_FAILED;
			}
		}
		private void StartReceiveData()
		{
			m_waitevent.Reset();
			int max_sixe = CMSMessage.MAX_MSG_BUFFER;
			byte[] mbuff = new byte[max_sixe];
			int rlen = 0;
			int totallen = 0;
			int rawdatalen = 0;
			int rawdatareceive = 0;
			CMSMessage.MsgHeader msgHeader;
			int msgHeaderLength = Marshal.SizeOf(typeof(CMSMessage.MsgHeader));
			bool checkHeader = false;
			try
			{
				while (!m_stop && m_socket != null)
				{
					if (m_socket.Available <= 0 && m_socket.Connected)
					{
						System.Threading.Thread.Sleep(NET_WORK_SLEEP_TIME);
						continue;
					}
					rlen = 0;
					lock (m_socket)
					{
						if (checkHeader)
						{
							rlen = m_socket.Receive(mbuff, totallen, rawdatalen - rawdatareceive, SocketFlags.None);
						}
						else
							rlen = m_socket.Receive(mbuff, totallen, 1, SocketFlags.None);
						
					}
					if (rlen == 0 || !m_socket.Connected)
					{
						if (OnSocketClose != null)
							OnSocketClose(this.m_cmsIP, this.m_cmsPort);
						break;
					}
					totallen += rlen;
					if( !checkHeader && totallen < msgHeaderLength)
					{
						continue;
					}
					if( !checkHeader)
					{
						byte[] headeruff = new byte[msgHeaderLength];
						Array.Copy(mbuff, headeruff, msgHeaderLength);
						GCHandle handle = GCHandle.Alloc(headeruff, GCHandleType.Pinned);
						msgHeader = (CMSMessage.MsgHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CMSMessage.MsgHeader));
						handle.Free();
						if (string.Compare(msgHeader.msgBegin, CMSMessage.BEGIN_MSG, false) != 0)
						{
							Array.Clear(mbuff, 0, totallen);
							rlen = totallen = rawdatalen = rawdatareceive = 0;
							checkHeader = false;
							continue;
						}
						else
						{
							//totallen += rlen;
							rawdatalen = msgHeader.size - msgHeaderLength;
							rawdatareceive = totallen - msgHeaderLength;
							checkHeader = true;
						}

						
						continue;
					}
					else
					{
						rawdatareceive += rlen;
						if (rawdatareceive >= rawdatalen)
						{
							if (OnSocketreceiveMessage != null)
								OnSocketreceiveMessage(mbuff, totallen);
							Array.Clear(mbuff, 0, totallen);
							rlen = totallen = rawdatalen = rawdatareceive = 0;
							checkHeader = false;
						}
					}

				}
			}
			catch (System.Exception )
			{
			}
			
			m_waitevent.Set();
			if (m_socket != null && !m_socket.Connected && m_stop)
			{
				if (OnSocketClose != null)
					OnSocketClose(this.m_cmsIP, this.m_cmsPort);
			}
		}
		private void StopThread()
		{
			if (m_waitevent == null)
				return;
			m_stop = true;
			m_waitevent.WaitOne();
			if( m_ThreadReceive != null)
			{
				m_ThreadReceive.Abort();
				m_ThreadReceive = null;
			}
		}
		private void StartThread()
		{
			m_stop = false;
			m_ThreadReceive = new Thread(new ThreadStart(StartReceiveData), Utils.MAX_STACK_SIZE);
			m_ThreadReceive.SetApartmentState( ApartmentState.STA);
			m_ThreadReceive.Start();
		}
		public bool StartSocket()
		{
			if (m_waitevent == null)
				m_waitevent = new ManualResetEvent(false);
			m_waitevent.Set();
			if (InitSocket(m_cmsIP, m_cmsPort) == SOCKET_ERROR.CONECT_SUCCESSFUL)
			{
				StopThread();
				StartThread();
				return true;
			}
			else
				return false;
		}		
		public SOCKET_ERROR SendData( byte[]mbuff, int length)
		{
			if (m_socket == null)
				return SOCKET_ERROR.CONNECTED_FAIL;

			SOCKET_ERROR ret =  SOCKET_ERROR.SEND_SUCCESSFUL;
			lock(m_socket)
			{
				ret = SendSockData(mbuff, length);
			}
			
			return ret;
		}
		public void StopReceive()
		{
			if (m_waitevent == null)
				return;
			m_stop = true;
			CloseSocket();
			m_waitevent.WaitOne();
			StopThread();
		}
	}
}
