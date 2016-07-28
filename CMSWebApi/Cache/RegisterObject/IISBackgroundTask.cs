using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace CMSWebApi.Cache.RegisterObject
{
	internal class IISBackgroundTask : IRegisteredObject
	{
		public IISBackgroundTask()
		{
			HostingEnvironment.RegisterObject(this);
		}

		public void Stop(bool immediate)
		{
			if( immediate)
				HostingEnvironment.UnregisterObject(this);
		}

		public bool SaveCache<T>() where T:class
		{
			return true;
		}
		public bool AddCache<T>( T item) where T:class
		{
			return true;
		}

		public bool DeleteCache<T>(Expression<Func<T, bool>> filter) where T : class
		{
			return true;
		}

		public bool DeleteCaches<T>() where T : class
		{
			return true;
		}

		public bool CheckCache<T>(Expression<Func<T, bool>> filter) where T : class
		{
			IEnumerable<T> search = GetCache<T>(filter);
			return search == null? false : search.Count() > 0;
		}

		public IEnumerable<T> GetCache<T>( Expression<Func<T, bool>> filter) where T:class
		{
			return null;
		}
	}

}
