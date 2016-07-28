using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConverterDB.Model;
using System.Net.Sockets;
using PACDMSimulator.Events;
using Commons.Resources;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
namespace PACDMSimulator.DVRConverter
{
	internal class DVRSocketServer : IDisposable
	{
		const int SOCKET_LISTENER_BACK_LOG = 5;
		const int SOCKET_PORT_LIMIT = 65535;
		private enum ControlEvent
		{
			SOCKET_INITITALIZE,
			SOCKET_LISTENER,
			SOCKET_CLIENT_ERROR,
			SOCKET_EXIT,
			ALL_EVENTS
		}

		
		public event DVRUtils.SocketEvent OnSocketEvent;
		public event DVRUtils.SocketReceivedData OnSocketReceivedData;

		readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
		ConverterDB.Model.DVRConverter DVRConfig = null;
		Socket socketServer;
		readonly ConcurrentDictionary<string, DVRSocketClient> DVRClients = new ConcurrentDictionary<string, DVRSocketClient>();
		AutoResetEvent[] ControlEvents;
		volatile bool m_Stop = false;
		int NumberOfDVRClient = 1;//unlimited
		Task SocketListeningTask;
		readonly CancellationTokenSource SocketListeningTokenSource = new CancellationTokenSource();
		public DVRSocketServer(ConverterDB.Model.DVRConverter DVRconfig, int NumberOfClient = DVRUtils.DVR_CONNECTION_SUPPORT )
		{
			DVRConfig = DVRconfig;
			NumberOfDVRClient = NumberOfClient;
			InitializeEvent();
		}
		
		public void Dispose()
		{
			if( SocketListeningTask != null)
				SocketListeningTokenSource.Cancel();

			m_Stop = true;
			ConnectDone.Set();
			CleanDVRClient();
			DisposeSocket();
			if(SocketListeningTask != null)
			{
				SocketListeningTask.Wait();
				SocketListeningTask.Dispose();
				SocketListeningTask = null;
			}
		}
		
		public bool StartDVRListner()
		{
			if (DVRConfig == null)
			{
				OnSocketEventArgs(new Events.SocketEvent
				{
					SockError = SocketError.DestinationAddressRequired,
					Message = ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.SOCKET_ERROR_DESTINATION_ADDRESS, Utils.CultureInfo),
					Sender = this
				});
				return false;
			}

			if (!DVRConfig.Enable)
			{
				OnSocketEventArgs(new Events.SocketEvent
				{
					SockError = SocketError.InvalidArgument,
					Message = ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CMS_SOCKET_SERVER_DISABLE, Utils.CultureInfo),
					Sender = this
				});
				return false;
			}

			SocketListeningTask = Task.Factory.StartNew(() => SocketListeningTaskProc(DVRConfig, ControlEvents, SocketListeningTokenSource.Token), TaskCreationOptions.LongRunning);
			return true;
		}

		public SocketError SendToAllDVRs(DVRDataMessage data)
		{
			if( DVRClients.Count == 0)
				return SocketError.NotConnected;
			SocketError sk_error = SocketError.Success;
			SocketError srror = SocketError.Success;
			foreach( DVRSocketClient dvrclient in DVRClients.Values)
			{
				srror =  SendToDVR( dvrclient, data);
				if( srror != SocketError.Success)
				{
					dvrclient_OnSocketEvent( dvrclient, new SocketEvent{ SockError = srror, Sender = data,  ErrorCode = Commons.ERROR_CODE.CMS_SOCKET_SEND_ERROR});
				}
			}
			return sk_error;
			
		}
		private SocketError SendToDVR(DVRSocketClient dvrClient, DVRDataMessage data)
		{
			if( dvrClient == null)
				return SocketError.NotConnected;
			return dvrClient.BeginSend(data);
		}
		private void DisposeSocket()
		{
			if (socketServer == null)
				return;

			try
			{
				socketServer.Close();
				socketServer.Dispose();
				socketServer = null;
			}
			catch(Exception)
			{
				socketServer = null;
			}
		}
		
		private void InitializeEvent()
		{
			ControlEvents = new AutoResetEvent[(int)ControlEvent.ALL_EVENTS];
			for( int i = 0; i <  ControlEvents.Length; i++)
			{
				ControlEvents[i] = new AutoResetEvent(false);
			}
		}

		private void OnSocketReceivedDataEventArgs(Events.SocketReceivedData eventargs)
		{
			if (OnSocketReceivedData == null || m_Stop == true)
				return;
				OnSocketReceivedData( this, eventargs);
		}
		
		private void OnSocketEventArgs( Events.SocketEvent eventargs)
		{
			if( OnSocketEvent == null || m_Stop == true)
				return;
			OnSocketEvent(this, eventargs);
		}
		
		private Socket InitializeSocketserver( ConverterDB.Model.DVRConverter DVRconfig)
		{
			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, DVRconfig.TCPPort);
			Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				listener.Bind(localEndPoint);
				listener.Listen(SOCKET_LISTENER_BACK_LOG);
			}
			catch(Exception ex)
			{
				OnSocketEventArgs(new Events.SocketEvent
				{
					SockError = SocketError.SocketError,
					Message = ex.Message,
					Sender = this
				});
			}
			return listener;
		}

		private void SocketListeningTaskProc(ConverterDB.Model.DVRConverter DVRconfig, AutoResetEvent[] CtrlEvents, CancellationToken cancelingtoken)
		{

			if( cancelingtoken.IsCancellationRequested)
				return;

			const int thread_Wait = 5000;//5 seconds
			int event_index = -1;
			int time_out = 0;
			bool is_listenning = false;
			CtrlEvents[(int)ControlEvent.SOCKET_INITITALIZE].Set();
			while( !m_Stop || !cancelingtoken.IsCancellationRequested) 
			{
				event_index = WaitHandle.WaitAny(CtrlEvents, thread_Wait);
				switch( event_index)
				{
					case (int)ControlEvent.SOCKET_INITITALIZE:
							DisposeSocket();
							CleanDVRClient();
							socketServer = InitializeSocketserver(DVRconfig);
							if (socketServer != null)
								CtrlEvents[(int)ControlEvent.SOCKET_LISTENER].Set();
						break;
					case (int)ControlEvent.SOCKET_CLIENT_ERROR:
							if( socketServer != null && DVRClients.Count < NumberOfDVRClient && !is_listenning)
								CtrlEvents[(int)ControlEvent.SOCKET_LISTENER].Set();
						break;
					case (int)ControlEvent.SOCKET_LISTENER:
							is_listenning = true;
							ConnectDone.Reset();
							socketServer.BeginAccept(new AsyncCallback(AcceptCallback), socketServer);
							ConnectDone.WaitOne();
							is_listenning = false;
						break;

					case (int)ControlEvent.SOCKET_EXIT:
							DisposeSocket();
							CleanDVRClient();
						break;

					default:

						time_out += thread_Wait;
						if( socketServer == null)// need to initialize socket
						{
							if(time_out >= DVRconfig.DvrSocketRetry * TimeSpan.TicksPerMinute)
							{
								time_out = 0;
								CtrlEvents[(int)ControlEvent.SOCKET_INITITALIZE].Set();
								break;
							}
						}
						else
						{
							if (DVRClients.IsEmpty)//only accept connection when DVRClients is empty.
							{
								CtrlEvents[(int)ControlEvent.SOCKET_LISTENER].Set();
								time_out = 0;
							}
							else
								time_out = time_out >= (Int32.MaxValue -thread_Wait)? 0 : time_out;
						}
						break;
				}
			}
		}

		private void CleanDVRClient()
		{
			DVRSocketClient client = null;
			while(DVRClients.Count > 0)
			{
				if(!DVRClients.TryRemove( DVRClients.First().Key, out client))
					continue;
				RemoveDVRClient( ref client);

			}
		}
		
		private void RemoveDVRClient(ref DVRSocketClient dvrclient)
		{
			if( dvrclient == null)
				return;

			dvrclient.OnSocketEvent -= dvrclient_OnSocketEvent;
			dvrclient.OnSocketReceivedData -= dvrclient_OnSocketReceivedData;
			dvrclient.Dispose();
			dvrclient = null;
		}
		
		private void AcceptCallback(IAsyncResult ar)
		{
			// Get the socket that handles the client request.
			try
			{
				Socket listener = (Socket)ar.AsyncState;
				Socket handler = listener.EndAccept(ar);
				DVRSocketClient dvrclient = new DVRSocketClient(handler);
				dvrclient.OnSocketEvent += dvrclient_OnSocketEvent;
				dvrclient.OnSocketReceivedData += dvrclient_OnSocketReceivedData;
				dvrclient.BeginReceive();
				DVRClients.TryAdd(dvrclient.SocketID, dvrclient);
			}
			catch(Exception)
			{

			}
			ConnectDone.Set();
		}

		private void dvrclient_OnSocketReceivedData( object sender, SocketReceivedData eventargs)
		{
			OnSocketReceivedDataEventArgs( eventargs);
		}

		private void dvrclient_OnSocketEvent(object sender, SocketEvent eventargs)
		{
			OnSocketEventArgs(eventargs);
			if( eventargs.SockError != SocketError.Success)
			{
				DVRSocketClient client = null;
				DVRClients.TryRemove( (sender as DVRSocketClient).SocketID, out client );
				if( client != null)
				{
					RemoveDVRClient(ref client);
					ControlEvents[(int)ControlEvent.SOCKET_CLIENT_ERROR].Set();
					
				}
			}
			
		}

	}
}
