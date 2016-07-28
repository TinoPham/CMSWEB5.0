using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.MetricSite;
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
	//[RoutePrefix("metricsite")]
	[WebApiAuthenication]
	public class MetricSiteController : ApiControllerBase<IMetricSiteService, MetricSiteBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetAllMetric()
		{
			//BusinessService.IUser = DependencyResolver<IUsersService>();
			return base.ExecuteBusiness<IEnumerable<MetricModel>>(() => { return base.BusinessService.GetAllMetric(base.usercontext);} );
			//return base.ExecuteBusiness < IEnumerable < MetricListModel >>( () =>
			//	{
			//		IEnumerable<MetricListModel> metricList = base.BusinessService.GetAllMetric(base.usercontext);
			//		return metricList;
			//	});
			
			//return Request.CreateResponse(HttpStatusCode.OK, metricList);
		}

		[HttpGet]
		public HttpResponseMessage GetMetricChild(int parentID)
		{
			return base.ExecuteBusiness<IEnumerable<MetricModel>>(() => { return base.BusinessService.GetMetricChild(base.usercontext, parentID); });
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage AddMetricSite(List<MetricModel> metric)
		{
			//return base.ExecuteBusiness<TransactionalInformation>( () =>
			//{
			//	TransactionalInformation trans = base.BusinessService.AddMetricSite(usercontext, metric);
			//	return trans;
			//	//return Request.CreateResponse(HttpStatusCode.OK, trans);
			//});
			TransactionalModel<List<MetricModel>> response = BusinessService.AddMetricSite(usercontext, metric);
			return ResponseData<TransactionalModel<List<MetricModel>>>(response);

		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteMetricSite([FromBody] List<int> metricIDs)
		{
			TransactionalModel<List<MetricModel>> result = base.BusinessService.DeleteMetrics(usercontext, metricIDs);
			return ResponseData<TransactionalModel<List<MetricModel>>>(result);
		}
		
	}
}
