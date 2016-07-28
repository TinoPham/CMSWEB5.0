using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
using Extensions;

namespace CMSWebApi.Hub
{
	public class Startup
	{
		public static void ConfigureSignalR(string url, IAppBuilder app)
		{
			app.Use(typeof(ClaimsMiddleware));
			GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new CustomUserIdProvider());
			HubConfiguration config = new HubConfiguration();
			config.EnableJSONP = false;
			config.EnableJavaScriptProxies = true;
			config.EnableDetailedErrors = true;
			app.MapSignalR(url, config);
		}
		

	}

	public class ClaimsMiddleware : OwinMiddleware
	{
		public ClaimsMiddleware(OwinMiddleware next)
			: base(next)
		{
		}

		public override Task Invoke(IOwinContext context)
		{
			LoginModel model = GetLoginModel(context.Request.Cookies);
			if( model != null && model.ID> 0)
			{
				var identity = new GenericPrincipal(new UserContext(model), null);
				context.Request.User = identity;
			}

			return Next.Invoke(context);
		}
		private static LoginModel GetLoginModel(RequestCookieCollection Cookies)
		{
			if (Cookies == null)
				return null;
			KeyValuePair<string, string> token = Cookies.FirstOrDefault(it => string.Compare(it.Key, Consts.XSRF_TOKEN_KEY, true) == 0);
			if (string.IsNullOrEmpty(token.Key) || string.IsNullOrEmpty(token.Value))
				return null;
			LoginModel model = WebUserToken.GetModel(token.Value);
			return model;
		}
	}

	public class CustomUserIdProvider : IUserIdProvider
	{
		public string GetUserId(IRequest request)
		{
			UserContext ucontext =request.User == null? null : request.User.Identity as UserContext;
			if( ucontext == null)
				return string.Empty;
			return  ucontext.ID.ToString();
		}
	}

	public class HubConnection : PersistentConnection
	{
		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			// Broadcast data to all clients
			//return Connection.Broadcast(data);
			return base.OnReceived( request, connectionId, data);
		}
	}
}
