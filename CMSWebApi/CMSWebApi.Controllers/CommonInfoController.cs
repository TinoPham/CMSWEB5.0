using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Common;
using CMSWebApi.ServiceInterfaces;
using System.Linq;
using CMSWebApi.DataModels;

namespace CMSWebApi.Controllers
{
	public class CommonInfoController : ApiControllerBase<ICommonInfoService, CommonBusinessService>
	{

		[HttpGet]
		public HttpResponseMessage GetCountries()
		{
			IQueryable<Country> Countries = base.BusinessService.GetCountries();
			return Request.CreateResponse(HttpStatusCode.OK, Countries);
		}

		[HttpGet]
		public HttpResponseMessage GetStates(string countryCode)
		{
			IQueryable<State> States = base.BusinessService.GetStateses(countryCode);
			return Request.CreateResponse(HttpStatusCode.OK, States);
		}
	}
}
