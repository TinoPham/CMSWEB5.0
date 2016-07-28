using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Hub.Handler;
using CMSWebApi.Hub.Hubs;
using Commons;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace CMSWebApi.Hub
{
	public class HubManager: Commons.SingletonClassBase<HubManager>
	{
		public void Add<Thub, Tmodel>(Tmodel model, params int [] userids) where Tmodel : class where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub,Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub,IHubClient<Tmodel>>();
			if( hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).New(model);
		}

		public void Add<Thub, Tmodel>(List<Tmodel> model, params int [] userids) where Tmodel : class where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub, Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
			if (hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).New(model);
		}

		public void Delete<Thub, Tmodel>(Tmodel model, params int [] userids)
			where Tmodel : class
			where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub, Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
			if (hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).Delete(model);
		}
		public void Delete<Thub, Tmodel>(int valID, params int [] userids)
			where Tmodel : class
			where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub, Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
			if (hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).Delete(valID);
		}

		public void Delete<Thub,Tmodel>(Int64 valID, params int [] userids) where Tmodel : class where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub, Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
			if (hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).Delete(valID);
		}

		public void Edit<Thub, Tmodel>(Tmodel model, params int [] userids)
			where Tmodel : class
			where Thub : Hubbase<Tmodel>
		{
			IHubContext<IHubClient<Tmodel>> hctx = HubContext<Thub, Tmodel>();//GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
			if (hctx == null)
				return;
			hctx.Clients.Users(ToUsers(userids)).Update(model);
		}

		private IHubContext<IHubClient<Tmodel>> HubContext<Thub, Tmodel>()
			where Tmodel : class
			where Thub : Hubbase<Tmodel>
		{
			return GlobalHost.ConnectionManager.GetHubContext<Thub, IHubClient<Tmodel>>();
		}
		
		private List<string>ToUsers( params int[] userids)
		{
			if( userids == null || userids.Length == 0)
				return new List<string>();
			return userids.Select( it => it.ToString()).ToList();
		}
	}
}
