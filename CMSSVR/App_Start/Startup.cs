using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Owin;

namespace CMSSVR
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			//app.MapSignalR("/api/cmsweb/notify", new HubConfiguration());
			CMSWebApi.Hub.Startup.ConfigureSignalR("/api/cmsweb/notify",app);
		}
	}
}