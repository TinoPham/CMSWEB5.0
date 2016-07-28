using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using CMSWebApi.DataModels;
using CMSWebApi.Resources;
using CMSWebApi.Utils;
using System.Security.Principal;
using CMSWebApi.Configurations;
using Extensions;


namespace CMSWebApi.APIFilters
{
	/// <summary>
	/// Authenicate User
	/// </summary>
	public class WebApiAuthenicationAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		const string XRequestWith = "X-Requested-With";
		const string XMLHttpRequest = "XMLHttpRequest";
		readonly string [] LocalAddress = new string [] { "loopback", "localhost", "127.0.0.1" };
		 
		IEnumerable<string>AllowIPAddress;
		bool is_Filter_Ipadddress = false;
		public WebApiAuthenicationAttribute()
		{
			AllowIPAddress = Enumerable.Empty<string>();
		}
		public WebApiAuthenicationAttribute(bool filteripadddress):this()
		{
			is_Filter_Ipadddress = filteripadddress;
			if (is_Filter_Ipadddress)
		    AllowIPAddress = AppSettings.AppSettings.Instance.ApiConfigs.AllowIpAddress;
		}
		
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			var request = actionContext.Request;
			if( is_Filter_Ipadddress)
			{
				if (!ValidateIP( request))
				{
					actionContext.Response = request.CreateResponse<string>(HttpStatusCode.NotImplemented, "This function is not available on your host.");
					return;
				}
			}
			var headers = request.Headers;
			var cook = headers.GetCookies(Consts.XSRF_TOKEN_KEY).FirstOrDefault();
			string token = cook == null?  null : cook[Consts.XSRF_TOKEN_KEY].Value;
			LoginModel model = WebUserToken.GetModel(token);
			if (model == null || model.ID <= 0)
			{
				TransactionalInformation transactionInformation = new TransactionalInformation();
				transactionInformation.ReturnMessage.Add(ResourceManagers.Instance.GetResourceString(Utils.CMSWebError.ACCESS_DENIED));
				transactionInformation.ReturnStatus = false;
				actionContext.Response = request.CreateResponse<TransactionalInformation>(HttpStatusCode.Unauthorized, transactionInformation);
			}
			else
			{
				HttpContext.Current.User = new GenericPrincipal(new UserContext(model), null);
				base.OnActionExecuting(actionContext);

			}
		}
		private bool ValidateIP(HttpRequestMessage request)
		{
			if(!AllowIPAddress.Any()|| request == null)
				return true;

			if( request.IsLocal())
			{
				 IEnumerable<string> local =	from all_ip in AllowIPAddress
												from lcal in LocalAddress
													where string.Compare( all_ip, lcal, true) == 0  select all_ip;
				return local.Any();
			}
			string ipadd = request.GetClientIpAddress();
			if( string.IsNullOrEmpty( ipadd))
				return false;
			return AllowIPAddress.Any( it=> string.Compare(it, ipadd, true) == 0);
		}

	}
}
