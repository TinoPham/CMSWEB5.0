using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using CMSWebApi.APIFilters;
using Microsoft.AspNet.SignalR;
using CMSWebApi.Hub.Hubs;

namespace CMSWebApi.Hub.Handler
{
	[HubName("Alert")]
	[CMSWebHubAuthorize]
	public class AlertHub : Hubbase<CMSWebApi.DataModels.Alert.AlertModel>
	{
		public override Task OnConnected()
		{
			return base.OnConnected();
		}

		public override Task OnReconnected()
		{
			return base.OnReconnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			return base.OnDisconnected(stopCalled);
		}
	}
}
