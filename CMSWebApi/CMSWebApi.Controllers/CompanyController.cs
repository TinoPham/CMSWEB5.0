using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.Company;
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
	[RoutePrefix("company")]
	[WebApiAuthenication]
	public class CompanyController : ApiControllerBase<ICompanyService, CompanyBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetCompanyInfo()
		{
			//base.BusinessService.IUsersvr = DependencyResolver<IUsersService>();
			CompanyModel model = base.BusinessService.GetCompanyInfo(base.usercontext);
			return ResponseData<CompanyModel>( model);
			//return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage UpdateCompanyInfo(CompanyModel companyModel)
		{
			//base.BusinessService.IUsersvr = DependencyResolver<IUsersService>();
			TransactionalModel<CompanyModel> model = base.BusinessService.UpdateCompanyInfo(companyModel, usercontext.ID);
			return ResponseData<TransactionalModel<CompanyModel>>(model);
			//return Request.CreateResponse(HttpStatusCode.OK, model);
		}
	}
}
