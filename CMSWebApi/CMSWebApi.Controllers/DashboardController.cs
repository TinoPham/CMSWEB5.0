using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Dashboard;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class DashboardController : ApiControllerBase<IDashboardService, DashboardBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetDashboard()
		{
			return ExecuteBusiness < DashboardUser>(() =>
			{
				var user = base.usercontext;
				if (user == null)
				{
					throw new CmsErrorException(CMSWebError.NO_PERMISION.ToString());
				}
				var advertisementPath = AppSettings.AppSettings.Instance.AdvertisementPath;

				return BusinessService.GetDashboarduser(user.ID, advertisementPath);
			});
		}

		[HttpGet]
		public HttpResponseMessage GetLayouts()
		{
			return ExecuteBusiness<List<Dashboard>>(() =>
			{
				var user = base.usercontext;
				if (user == null)
				{
					throw new CmsErrorException(CMSWebError.NO_PERMISION.ToString());
				}
				var advertisementPath = AppSettings.AppSettings.Instance.AdvertisementPath;
				return BusinessService.GetDashboards(advertisementPath);
				
			});
		}

		[HttpGet]
		public HttpResponseMessage GetElements()
		{
			return ExecuteBusiness<List<Element>>(() => BusinessService.GetElements().ToList());
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage EditDashboard(DashboardUser dashboardUser)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.EditDashboard(dashboardUser, usercontext.ID);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetDefinedDashboard(int level)
		{
			return ExecuteBusiness<DashboardUser>(() =>
			{
				var user = base.usercontext;
				if (user == null)
				{
					throw new CmsErrorException(CMSWebError.NO_PERMISION.ToString());
				}
				var advertisementPath = AppSettings.AppSettings.Instance.AdvertisementPath;
				return BusinessService.GetDefinedDashboard(level, user.ID, advertisementPath);
			});
		}
	}
}
