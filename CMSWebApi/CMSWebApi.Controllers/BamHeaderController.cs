using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Bam;
using CMSWebApi.BusinessServices.ReportBusiness;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.DataServices;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class BamHeaderController : ApiControllerBase<IBamHeaderService, BamHeaderBusinessService>
	{
		[HttpGet]
		public async Task<HttpResponseMessage> GetHeaderBam([ModelBinder(typeof(DashboardBamModelBinderProvider))]MetricParam request)
		{
			request.StartDate = request.WeekStartDate;
			request.EndDate = request.WeekEndDate;
			return await base.ExecuteBusinessAcsyn<HeaderBamModel>(() =>
			{
				return BusinessService.GetHeaderBam(usercontext, request);
			}).ConfigureAwait(false);
		}

	}
}
