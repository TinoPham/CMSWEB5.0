using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Hub.Hubs
{
	public interface IHubClient<T> where T : class
	{
		void New(T model);
		void New(List<T> model);
		void Delete(T model) ;
		void Delete(Int64 ID);
		void Update(T model) ;
	}
	
}
