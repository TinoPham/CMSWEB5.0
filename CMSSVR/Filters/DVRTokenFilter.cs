using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CMSSVR;
using CMSSVR.Infrastructure;
using Newtonsoft.Json;
namespace ApiConverter.Filters
{
	public class DVRTokenFilter : ActionFilterAttribute
	{

		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (actionContext.Request.Headers.Authorization == null)
			{
				actionContext.Response = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Unauthorized);
			}
			else
			{
				string authToken = actionContext.Request.Headers.Authorization.Parameter;
				KDVRToken dvrtoken = GetDvrToken(authToken);
				if( dvrtoken == null)
					actionContext.Response = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Unauthorized);
				else
				{
					if(string.Compare(AppSettings.AppSettings.Instance.ServerID, dvrtoken.ServerID, false) != 0 )
					{
						var request = actionContext.Request;
						actionContext.Response = request.CreateResponse<ConvertMessage.MessageResult>( HttpStatusCode.Unauthorized, new ConvertMessage.MessageResult{ ErrorID= Commons.ERROR_CODE.SERVICE_TOKEN_INVALID});
					}
					else
					{
						HttpContext.Current.User = new GenericPrincipal(dvrtoken, null);
						base.OnActionExecuting(actionContext);
					}
				}
			}
		}

		private KDVRToken GetDvrToken(string encryptToken)
		{
			string decrypttoken = Cryptography.MACSHA256.Decrypt(encryptToken, AppSettings.AppSettings.Instance.DVRTokenKey);
			if (string.IsNullOrEmpty(decrypttoken))
				return null;
			try
			{
				return JsonConvert.DeserializeObject<KDVRToken>(decrypttoken);

			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}