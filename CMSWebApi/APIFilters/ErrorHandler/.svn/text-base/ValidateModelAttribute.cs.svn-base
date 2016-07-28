using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.ModelBinding;
using CMSWebApi.DataModels;

namespace CMSWebApi.APIFilters.ErrorHandler
{
	public class ValidateModelAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			var request = actionContext.Request;

			if (!actionContext.ModelState.IsValid)
			{
				TransactionalInformation error = GetErrors(actionContext.ModelState, false);
				actionContext.Response = request.CreateResponse(HttpStatusCode.BadRequest, error);
			}
		}

		protected TransactionalInformation GetErrors(IEnumerable<KeyValuePair<string, ModelState>> modelState, bool includeErrorDetail)
		{
			var modelStateError = new TransactionalInformation();
			foreach (KeyValuePair<string, ModelState> keyModelStatePair in modelState)
			{
				//string key = keyModelStatePair.Key;
				ModelErrorCollection errors = keyModelStatePair.Value.Errors;
				if (errors != null && errors.Count > 0)
				{
					IEnumerable<string> errorMessages = errors.Select(error =>
					{
						if (includeErrorDetail && error.Exception != null)
						{
							return error.Exception.Message;
						}
						return String.IsNullOrEmpty(error.ErrorMessage) ? "ErrorOccurred" : error.ErrorMessage;
					}).ToArray();
					modelStateError.ReturnMessage.AddRange(errorMessages);
				}
			}

			return modelStateError;
		}
	}
}
