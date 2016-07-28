using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Dynamic;
using Commons;
using CMSWebApi.APIFilters;

namespace CMSWebApi.Hub
{
	[HubName("Alert1")]
	[CMSWebHubAuthorize]
	public class AlertHub : Microsoft.AspNet.SignalR.Hub
	{
		public override Task OnConnected()
		{
			string connectionID = Context.ConnectionId;
			UserConnection.Instance.AddConnection( connectionID);
			return base.OnConnected();
		}
		public override Task OnDisconnected(bool stopCalled)
		{
			string connectionID = Context.ConnectionId;
			UserConnection.Instance.RemoveConnection(connectionID);
			return base.OnDisconnected(stopCalled);
		}
		public override Task OnReconnected()
		{
			string connectionID = Context.ConnectionId;
			UserConnection.Instance.AddConnection(connectionID);
			return base.OnReconnected();
		}
		public void ClientMessage( string value)
		{
			System.Diagnostics.Debug.WriteLine(value);
			string msg =  Context.ConnectionId + ": " + value;
			Clients.All.OnMessage(msg);
		}
	}

	internal class UserConnection : Commons.SingletonClassBase<UserConnection>, IDisposable
	{
		readonly object locker = new object();
		List<string> connections;
		public IEnumerable<string> Connections{ get { return connections;}}
		private UserConnection(){ connections = new List<string>();}
		public void AddConnection(string id)
		{
			lock(locker)
			{
				connections.Add(id);
			}

		}
		public void RemoveConnection(string id)
		{
			lock (locker)
			{
				connections.Remove(id);
			}

		}
		public void Dispose()
		{
			lock (locker)
			{
				connections.Clear();
				connections = null;
			}
		}
	}
}
