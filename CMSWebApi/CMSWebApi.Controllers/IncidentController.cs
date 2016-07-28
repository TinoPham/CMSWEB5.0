using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.Incident;
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
	[WebApiAuthenication]
	public class IncidentController : ApiControllerBase<IIncidentService, IncidentBusinessService>
	{
		[HttpGet]
		public HttpResponseMessage GetCaseType()
		{
			IQueryable<CaseTypeModel> models = base.BusinessService.GetCaseType();
			return ResponseData<IQueryable<CaseTypeModel>>(models);
			//return Request.CreateResponse(HttpStatusCode.OK, models);
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetIncidentManagent(int caseType)
		{
			IncidentManagementModel model = base.BusinessService.GetIncidentManagent(caseType);
			return ResponseData<IncidentManagementModel>(model);
			//return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage UpdateIncidentField(IncidentFieldModel[] models)
		{
			TransactionalModel<List<IncidentFieldModel>> data = base.BusinessService.UpdateIncidentField(models);
			return ResponseData<TransactionalModel<List<IncidentFieldModel>>>(data);
			//return Request.CreateResponse(HttpStatusCode.OK, data);
		}	
	}
}
