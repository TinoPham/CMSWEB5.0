using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.ConversionRate;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class SaleReportsController : ApiControllerBase<IUsersService, SaleReportsBusiness>
	{
		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> GetReportData([ModelBinder(typeof(BAMRptModelBinderProvider))] BAMRptParam param)
		{
			return await base.ExecuteBusinessAcsyn<SaleReportDataAll>(() =>
			{
				Task<SaleReportDataAll> ret = BusinessService.GetDataReport(usercontext, param);
				return ret;
			}).ConfigureAwait(false);
		}
	}
}
