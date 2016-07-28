using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Dynamic;
using Commons;
using System.Collections.Concurrent;
using CMSWebApi.DataModels;
using CMSWebApi.Hub.Hubs;

namespace CMSWebApi.Hub.Hubs
{

	public abstract class Hubbase<Tmodel> : Microsoft.AspNet.SignalR.Hub<IHubClient<Tmodel>> where Tmodel : class
	{
		internal virtual void New(Tmodel model, params int[] userid)
		{
			List<string> connections = GetConnection( userid);
			if( connections != null)
				Clients.Clients(connections).New(model);
		}
		
		internal virtual void New(List<Tmodel> model, params int [] userid)
		{
			List<string> connections = GetConnection(userid);
			if (connections != null)
				Clients.Clients(connections).New(model);
		}

		internal virtual void Delete(Tmodel model, params int [] userid) 
		{
			List<string> connections = GetConnection(userid);
			if (connections != null)
				Clients.Clients(connections).Delete(model);
		}

		internal virtual void Delete(Int64 ID, params int [] userid) 
		{
			List<string> connections = GetConnection(userid);
			if (connections != null)
				Clients.Clients(connections).Delete(ID);
		}

		internal virtual void Update(Tmodel model, params int [] userid)
		{
			List<string> connections = GetConnection(userid);
			if (connections != null)
				Clients.Clients(connections).Update(model);
		 }

		private List<string> GetConnection( params int[] userid)
		{
			if( userid == null || userid.Length == 0)
				return null;
			return userid.Cast<string>().ToList();
		}
	}
}
