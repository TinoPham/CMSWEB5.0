using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.BusinessServices.ReportBusiness;
using CMSWebApi.APIFilters;
using System.Web.Http;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.DataModels;
using System.Web.Http.ModelBinding;
using System.Net.Http;
using SVRDatabase;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class ReportsController: ApiControllerBase<IUsersService, ReportBusinessService>
	{
		protected SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
		#region Dashboards
		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshAlerts([ModelBinder(typeof(AlertChartModelBinderProvider))] RptChartParam param)
		{
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return await base.ExecuteBusinessAcsyn<IQueryable<ChartWithImageModel>>(() =>
			{
				return BusinessService.DashBoardColumnCharts(usercontext, param, interval);
			}).ConfigureAwait(false);
			
		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshConversion([ModelBinder(typeof(AlertChartModelBinderProvider))] RptChartParam param)
		{

			return await base.ExecuteBusinessAcsyn<ConversionChartModel>(() =>
			{
				return BusinessService.DashBoardConversionCharts(usercontext, param);
			}).ConfigureAwait(false);

		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshTraffic([ModelBinder(typeof(AlertChartModelBinderProvider))] RptChartParam param)
		{

			return await base.ExecuteBusinessAcsyn<IEnumerable<TrafficChartModel>>(() =>
			{
				return BusinessService.DashboardTraffic(usercontext, param);
			}).ConfigureAwait(false);

		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshAlertCMP([ModelBinder(typeof(AlertCompareModelBinder))] AlertCompareParam param)
		{
			int interval = (DBModel!= null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return await base.ExecuteBusinessAcsyn<ALertCompModel>(() =>
			{
				Task<ALertCompModel> ret = BusinessService.CompareAlert(usercontext, param, interval);
				return ret;
			}).ConfigureAwait(false);

		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshConversionMap([ModelBinder(typeof(AlertChartModelBinderProvider))] RptChartParam param)
		{
			return await base.ExecuteBusinessAcsyn<IEnumerable<ConvMapChartModel>>(() =>
			{
				Task<IEnumerable<ConvMapChartModel>> ret = BusinessService.DashBoardConverionMap(usercontext, param);
				return ret;
			}).ConfigureAwait(false);
		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> DshConversionSites([ModelBinder(typeof(AlertChartModelBinderProvider))] RptChartParam param)
		{
			return await base.ExecuteBusinessAcsyn<IEnumerable<ConvSitesChartModel>>(() =>
			{
				Task<IEnumerable<ConvSitesChartModel>> ret = BusinessService.DashBoardConverionSites(usercontext, param);
				return ret;
			}

			).ConfigureAwait(false);
		}
		#endregion Dashboards
	}
}
