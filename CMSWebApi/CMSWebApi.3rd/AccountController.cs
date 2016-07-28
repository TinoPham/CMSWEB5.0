using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Account;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using Extensions;
using  AppSettings;
namespace CMSWebApi._3rd
{
	public class AccountController : ApiControllerBase<IAccountService, AccountsBusinessService>
	{
		public ApiKeyResult Get(Nullable<int> id = null)
		{
			if( !Request.Headers.Contains(AccountController.str_APPID))
				return new ApiKeyResult(HttpStatusCode.NotFound, null, base.Request); 

			IEnumerable<string> appID = Request.Headers.GetValues(AccountController.str_APPID);

			if( id.HasValue && id.Value > 0 || appID == null || !appID.Any())
			{
				return new ApiKeyResult(HttpStatusCode.NotFound, null, base.Request); 
			}
			_3rdConfig config = ClientConfig();
			if( config == null)
				return new ApiKeyResult(HttpStatusCode.NotFound, null, base.Request);

			string apikey =  base.CreateAPIKey();
			string sid = string.Format(base.AppSID());
			string key = string.Format("{0}:{1}", sid, apikey);
			MemoryCache.Default.Add(sid, apikey, DateTime.UtcNow.AddSeconds(60));
			if(config.UserID.HasValue && config.UserID.Value > 0)
			{
				MemoryCache.Default.Add(apikey, config.UserID.Value, DateTime.UtcNow.AddSeconds(60));
			}
			return new ApiKeyResult(HttpStatusCode.OK, key, base.Request); 
		}

		public ApiAuthResult Post(LoginModel model)
		{
			string key = Request.Headers.GetValues(ApiKeyResult.API_KEY).FirstOrDefault();
			if(!MemoryCache.Default.Contains(key))
				return new ApiAuthResult( HttpStatusCode.ExpectationFailed, null, base.Request);
			string apikey = MemoryCache.Default.Get(key) as string;
			if( string.IsNullOrEmpty(apikey))
				return new ApiAuthResult(HttpStatusCode.ExpectationFailed, null, base.Request);
			TransactionalModel<UserModel> trans_user = null;
			UserModel user = null;
			if(model != null )
			{
				model.SID = apikey;
				trans_user = base.BusinessService.Login( model, true);
				if (trans_user == null)
					return new ApiAuthResult(HttpStatusCode.Unauthorized, null, base.Request);
				user = trans_user.Data;
			}
			else
			{
				if (!MemoryCache.Default.Contains(apikey))
					return new ApiAuthResult(HttpStatusCode.ExpectationFailed, null, base.Request);

				int uid_cache = (int)MemoryCache.Default.Get(apikey);
				user = BusinessService.GetUserByID( uid_cache);
				model = new LoginModel{ Remember = true, UserName = user.UserName};
			}
			
			
			if (user == null ||  user.isExpired)
			{
				return new ApiAuthResult(HttpStatusCode.Unauthorized, null, base.Request);
			}
			_3rdLoginModel auth = new _3rdLoginModel{
														CompanyID = user.CompanyID.Value,
														Createdby = user.CreatedBy,
														ID = user.UserID,
														ServerID = AppSettings.AppSettings.Instance.ServerID,
														SID = apikey,
														Remember = model.Remember,
														UserName = model.UserName};
			string token = WebUserToken.GetToken(auth);
			MemoryCache.Default.Remove(key);
			MemoryCache.Default.Remove(apikey);
			return new ApiAuthResult(HttpStatusCode.OK, token, base.Request);
		}
	}
}
