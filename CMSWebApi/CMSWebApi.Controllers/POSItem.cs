using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.CommonPOS;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class POSItemController : ApiControllerBase<IUsersService, POSCommonBusinessService>
	{
		[HttpGet]
		public HttpResponseMessage Get(string id)
		{
			IQueryable<DataModels.ListModel> data = base.BusinessService.Listmodels(id);
			return base.ResponseData<IQueryable<DataModels.ListModel>>( data, System.Net.HttpStatusCode.OK);
		}
	}
}
