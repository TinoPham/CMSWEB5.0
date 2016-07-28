using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipes.Interfaces;
using Pipes.Utilities;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pipes.Server
{
	public class PipeServer : ICommunicationServer
	{
		#region private fields

		private readonly string _pipeName;
		private readonly SynchronizationContext _synchronizationContext;
		private readonly IDictionary<string, ICommunicationServer> _servers; // ConcurrentDictionary is thread safe
		private const int MaxNumberOfServerInstances = 1;

		#endregion

		#region c'tor

		public PipeServer( string pipename)
		{
			_pipeName = pipename;// Guid.NewGuid().ToString();
			_synchronizationContext = AsyncOperationManager.SynchronizationContext;
			_servers = new ConcurrentDictionary<string, ICommunicationServer>();
		}

		#endregion

		#region events

		public event EventHandler<MessageReceivedModelEventArgs> MessageReceivedModelEvent;
		public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
		public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
		public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;

		#endregion

		#region ICommunicationServer implementation

		public string ServerId
		{
			get { return _pipeName; }
		}

		public bool Start()
		{
			return StartNamedPipeServer();
		}

		public void Stop()
		{
			foreach (var server in _servers.Values)
			{
				try
				{
					UnregisterFromServerEvents(server);
					server.Stop();
				}
				catch (Exception)
				{
					//Logger.Error("Fialed to stop server");
				}
			}

			_servers.Clear();
		}

		#endregion

		#region private methods

		/// <summary>
		/// Starts a new NamedPipeServerStream that waits for connection
		/// </summary>
		private bool StartNamedPipeServer()
		{
			if( _servers.Count ==  MaxNumberOfServerInstances)
				return false;

			var server = new InternalPipeServer(_pipeName, MaxNumberOfServerInstances);
			_servers[server.Id] = server;

			server.ClientConnectedEvent += ClientConnectedHandler;
			server.ClientDisconnectedEvent += ClientDisconnectedHandler;
			server.MessageReceivedEvent += MessageReceivedHandler;

			return server.Start();
		}

		/// <summary>
		/// Stops the server that belongs to the given id
		/// </summary>
		/// <param name="id"></param>
		private void StopNamedPipeServer(string id)
		{
			UnregisterFromServerEvents(_servers[id]);
			_servers[id].Stop();
			_servers.Remove(id);
		}

		/// <summary>
		/// Unregisters from the given server's events
		/// </summary>
		/// <param name="server"></param>
		private void UnregisterFromServerEvents(ICommunicationServer server)
		{
			server.ClientConnectedEvent -= ClientConnectedHandler;
			server.ClientDisconnectedEvent -= ClientDisconnectedHandler;
			server.MessageReceivedEvent -= MessageReceivedHandler;
		}

		/// <summary>
		/// Fires MessageReceivedEvent in the current thread
		/// </summary>
		/// <param name="eventArgs"></param>
		private void OnMessageReceived(MessageReceivedModelEventArgs eventArgs)
		{
			_synchronizationContext.Post(e => MessageReceivedModelEvent.SafeInvoke(this, (MessageReceivedModelEventArgs)e), eventArgs);
		}

		/// <summary>
		/// Fires ClientConnectedEvent in the current thread
		/// </summary>
		/// <param name="eventArgs"></param>
		private void OnClientConnected(ClientConnectedEventArgs eventArgs)
		{
			_synchronizationContext.Post(e => ClientConnectedEvent.SafeInvoke(this, (ClientConnectedEventArgs)e),
				eventArgs);
		}

		/// <summary>
		/// Fires ClientDisconnectedEvent in the current thread
		/// </summary>
		/// <param name="eventArgs"></param>
		private void OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
		{
			_synchronizationContext.Post(
				e => ClientDisconnectedEvent.SafeInvoke(this, (ClientDisconnectedEventArgs)e), eventArgs);
		}

		/// <summary>
		/// Handles a client connection. Fires the relevant event and prepares for new connection.
		/// </summary>
		private void ClientConnectedHandler(object sender, ClientConnectedEventArgs eventArgs)
		{
			OnClientConnected(eventArgs);

			StartNamedPipeServer(); // Create a additional server as a preparation for new connection
		}

		/// <summary>
		/// Hanldes a client disconnection. Fires the relevant event ans removes its server from the pool
		/// </summary>
		private void ClientDisconnectedHandler(object sender, ClientDisconnectedEventArgs eventArgs)
		{
			OnClientDisconnected(eventArgs);

			StopNamedPipeServer(eventArgs.ClientId);
			StartNamedPipeServer(); // Create a additional server as a preparation for new connection
		}

		/// <summary>
		/// Handles a message that is received from the client. Fires the relevant event.
		/// </summary>
		private void MessageReceivedHandler(object sender, MessageReceivedEventArgs eventArgs)
		{
			
			string msg_string = eventArgs.Message;
			PipeModels.MessageModel msgmodel = JsonConvert.DeserializeObject<PipeModels.MessageModel>(msg_string, PipeModels.JsonSettings.Instance.Settings);
			if( msgmodel == null)
				return;
			try
			{
				Type mtype = Type.GetType(  string.Format("{0},{1}", msgmodel.ClassModel, msgmodel.GetType().Assembly.FullName));
				if(mtype != null)
				{
					object model = JsonConvert.DeserializeObject(msgmodel.Data, mtype, PipeModels.JsonSettings.Instance.Settings);
					OnMessageReceived( new MessageReceivedModelEventArgs{ Message = model});
				}
			}
			catch(Exception){}
		}

		#endregion

		public Task<TaskResult> SendMessage(string message)
		{
			var taskCompletionSource = new TaskCompletionSource<TaskResult>();
			return (_servers.First().Value as InternalPipeServer).SendMessage(message);

		}

	}
}
