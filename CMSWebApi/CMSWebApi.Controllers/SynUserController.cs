using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.SynUser;
using PACDMModel;
using System.Net.Http;
using System.Net;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using CMSWebApi.APIFilters;
using System.Net.Http.Headers;
using System.Web.Security;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.Controllers
{
	[RoutePrefix("synuser")]
	[WebApiAuthenication]
	public class SynUserController : ApiControllerBase<ISynUserService, SynUserBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetAllSynUser()
		{
			//BusinessService.IUser = DependencyResolver<IUsersService>();
			IQueryable<SynUserModel> synUser = base.BusinessService.GetAllSynUser(base.usercontext);
			return ResponseData<IQueryable<SynUserModel>>( synUser);
			//return Request.CreateResponse(HttpStatusCode.OK, synUser);
		}

		[HttpGet]
		public HttpResponseMessage GetAllSynUserType()
		{
			//BusinessService.IUser = DependencyResolver<IUsersService>();
			IQueryable<SynUserTypeModel> synUserType = base.BusinessService.GetAllSynUserType();
			return ResponseData<IQueryable<SynUserTypeModel>>(synUserType);
			//return Request.CreateResponse(HttpStatusCode.OK, synUserType);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage AddSynUser(SynUserModel synUserModel)
		{
			TransactionalModel<SynUserModel> model = base.BusinessService.UpdateSynUser(synUserModel);
			return ResponseData<TransactionalModel<SynUserModel>>(model);
			//return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteSynUser([FromBody] int synID)
		{
			bool result = base.BusinessService.DeleteSynUser(synID);
			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		
	}
}
