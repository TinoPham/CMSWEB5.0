using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
//using System.Web.OData;
//using System.Web.OData.Query;
using CMSWebApi.BusinessServices.Account;
using PACDMModel;
using System.Net.Http;
using System.Net;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using CMSWebApi.APIFilters;
using System.Net.Http.Headers;
using System.Web.Security;
using CMSWebApi.Utils;
using Extensions;
using System.Web.Http.ModelBinding;
using System.Configuration;
using CMSWebApi.Configurations;
using AppSettings;


namespace CMSWebApi.Controllers
{
	[RoutePrefix("account")]
	public class AccountController : ApiControllerBase<IAccountService, AccountsBusinessService>
	{
		//[Route("Account")]
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage Account()
		{
			//var config = ConfigurationManager.GetSection("DashboardCache") as DashBoardsSection;

			string token = GetCookie(Consts.XSRF_TOKEN_KEY);
			HttpResponseMessage response;
			if (string.IsNullOrEmpty(token))
			{
				response = UnAuthorizeMessage();
				response.SetHeaderValue(SID_KEY, base.InitSID());
				return response;

			}
			LoginModel model = WebUserToken.GetModel(token);

			if (model == null || !model.Remember)
			{
				response = UnAuthorizeMessage();
				SetCookie(Consts.XSRF_TOKEN_KEY, string.Empty, 0);
				//SetCookie(response,	Consts.XSRF_TOKEN_KEY, null, 0);
				response.SetHeaderValue(SID_KEY, InitSID());
				return response;
			}

			TransactionalModel<DataModels.UserModel> user = base.BusinessService.Login(model);
			if (user == null || user.IsAuthenicated == false)
			{
				response = Request.CreateResponse(HttpStatusCode.Unauthorized);
				SetCookie(Consts.XSRF_TOKEN_KEY, null, 0);
				response.SetHeaderValue(SID_KEY, InitSID());
				return response;
			}

			string old_sid = model.SID;
			model.SID = InitSID();

			token = WebUserToken.GetToken(model);
			response = base.ResponseData<TransactionalModel<DataModels.UserModel>>(user, HttpStatusCode.OK);
			//response = base.Request.CreateResponse<DataModels.UserModel>(HttpStatusCode.OK, user);
			SetCookie(Consts.XSRF_TOKEN_KEY, token, AppSettings.AppSettings.Instance.CookieExpired);

			response.SetHeaderValue(SID_KEY, model.SID);
			return response;
		}

		////[Route("Users")]
		//[HttpGet]
		//public HttpResponseMessage Users(ODataQueryOptions<UserModel> options)
		//{
		//	return ExecuteBusiness(() =>
		//	{
		//		var result = BusinessService.GetUsers();
		//		if (result == null)
		//		{
		//			throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
		//		}
		//	result = options.ApplyTo(result) as IQueryable<UserModel>;
		//		var response = base.Request.CreateResponse(HttpStatusCode.OK, new ODataMetadata<object>(result, result.Count()));
		//		return response;
		//	});
		//}

		//[Route("Account")]
		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage Account(LoginModel model)
		{
			if (model == null)
				return UnAuthorizeMessage();
			if (string.IsNullOrEmpty(model.SID))
				model.SID = base.SID;

			TransactionalModel<DataModels.UserModel> user = base.BusinessService.Login(model, true);
			if (user == null || user.IsAuthenicated == false)
				return Request.CreateResponse(HttpStatusCode.Unauthorized);

			if (user.Data.isExpired)
			{
				return base.ResponseData<TransactionalModel<UserModel>>(user, HttpStatusCode.Unauthorized);
			}

			model.ID = user.Data.UserID;
			model.CompanyID = (int)user.Data.CompanyID;
			string token = WebUserToken.GetToken(model);
			HttpResponseMessage res = base.ResponseData<TransactionalModel<UserModel>>(user, HttpStatusCode.OK); //base.Request.CreateResponse<DataModels.UserModel>(HttpStatusCode.OK, user);
			SetCookie(Consts.XSRF_TOKEN_KEY, token, AppSettings.AppSettings.Instance.CookieExpired);
			//SetCookie(res, Consts.XSRF_TOKEN_KEY, token, (int)Cookie_Age);
			//CMSWebApi.Hub.HubManager.Instance.Add<CMSWebApi.DataModels.Alert.AlertModel>(
			// new CMSWebApi.DataModels.Alert.AlertModel{
			//  Channel = 1, Description = "test"
			// }, 70);
			return res;
		}

		[HttpPost]
		[WebApiAuthenication]
		[ActivityLog]
		public HttpResponseMessage ChangePassword(ChangePasswordModel passwordModel)
		{
			TransactionalModel<ChangePasswordModel> model = BusinessService.ChangePassword(usercontext, passwordModel, base.SID);
			return ResponseData<TransactionalModel<ChangePasswordModel>>(model);
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage ResetPassword([FromUri]ForgotPasswordModel lostPassModel)
		{

			CMSWebApi.Cache.BackgroundTaskManager.Instance.Run(async () => await BusinessService.ResetPassword(lostPassModel).ConfigureAwait(false));

			//bool ret = await BusinessService.ResetPassword(lostPassModel).ConfigureAwait(false);
			return Request.CreateResponse(HttpStatusCode.OK);
			//return ExecuteBusiness(() =>
			//{
			//	BusinessService.ResetPassword(lostPassModel);
			//	return Request.CreateResponse(HttpStatusCode.OK);
			//});
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage LogOut()
		{
			return UnAuthorizeMessage();
		}

	}
}
