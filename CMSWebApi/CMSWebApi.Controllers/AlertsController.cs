using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.ReportBusiness;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.DataModels;
using SVRDatabase;


namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class AlertsController : ApiControllerBase<IUsersService, ReportBusinessService>
	{
		protected SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> Alerts([ModelBinder(typeof(AlertCompareModelBinder))] AlertCompareParam param)
		{
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return await base.ExecuteBusinessAcsyn<ALertCompModel>(() =>
				{
					Task<ALertCompModel> ret = BusinessService.CompareAlert(usercontext, param, interval);
					return ret;
				}).ConfigureAwait(false);
		}
	}
}
