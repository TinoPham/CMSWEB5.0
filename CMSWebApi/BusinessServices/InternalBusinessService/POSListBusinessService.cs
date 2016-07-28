using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataServices;
using CMSWebApi.DataServices.POS;
using System.Linq.Expressions;

namespace CMSWebApi.BusinessServices.InternalBusinessService
{
	internal class POSListBusinessService<T> where T: class
	{
		protected DataServices.ServiceBase ServiceBase { get; set; }
		POSListModelService<T> DataService;
		internal POSListBusinessService(ServiceBase ServiceBase)
		{
			DataService = new POSListModelService<T>(ServiceBase);
		}
		internal T Get( Expression<Func<T,bool>> filter)
		{
			return DataService.Get<T>(filter, null, it => it);
		}

		internal IQueryable<T> Gets(Expression<Func<T, bool>> filter)
		{
			return DataService.Gets<T>(filter, null, it => it);
		}
	}
}
