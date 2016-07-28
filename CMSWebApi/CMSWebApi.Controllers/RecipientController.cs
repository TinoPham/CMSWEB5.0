using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.Recipient;
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
	//[RoutePrefix("recipient")]
	[WebApiAuthenication]
	public class RecipientController : ApiControllerBase<IRecipientService, RecipientBusinessService>
	{
		//[HttpGet]
		//[ActivityLog]
		//public HttpResponseMessage GetRecipient()
		//{
		//	//BusinessService.IUser = DependencyResolver<IUsersService>();
		//	IQueryable<RecipientModel> recipients = base.BusinessService.GetAllRecipient(base.usercontext);
		//	return ResponseData<IQueryable<RecipientModel>>(recipients);
		//	//return Request.CreateResponse(HttpStatusCode.OK, recipients);
		//}

		//[HttpPost]
		//[ActivityLog]
		//public HttpResponseMessage AddRecipient(RecipientModel model)
		//{
		//	TransactionalModel<RecipientModel> recipient = base.BusinessService.UpdateRecipient(model);
		//	return ResponseData<TransactionalModel<RecipientModel>>(recipient);
		//	//return Request.CreateResponse(HttpStatusCode.OK, recipient);
		//}

		//[HttpPost]
		//[ActivityLog]
		//public HttpResponseMessage DeleteRecipient([FromBody] List<int> recipientIDs)
		//{
		//	TransactionalModel<RecipientModel> recipient = base.BusinessService.DeleteRecipient(recipientIDs);
		//	return ResponseData<TransactionalModel<RecipientModel>>(recipient);
		//}
		
	}
}
