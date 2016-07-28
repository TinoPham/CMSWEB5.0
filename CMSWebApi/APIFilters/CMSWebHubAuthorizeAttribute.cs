using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin.Security;

namespace CMSWebApi.APIFilters
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class CMSWebHubAuthorizeAttribute : AuthorizeAttribute
	{
		public override bool AuthorizeHubConnection(Microsoft.AspNet.SignalR.Hubs.HubDescriptor hubDescriptor, IRequest request)
		{
			if(!UserAuthorized( request.User) )
				return false;

			return base.AuthorizeHubConnection(hubDescriptor, request);
		}
		protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
		{
			if (user == null)
				return false;
			UserContext ucontext = user.Identity as UserContext; 
			return ucontext == null? false : ucontext.ID > 0;
		}
	}
}
