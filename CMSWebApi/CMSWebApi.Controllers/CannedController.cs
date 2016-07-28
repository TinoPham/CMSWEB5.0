using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Rebar;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class CannedController: ApiControllerBase<ICannedService, CannedBusinessSevice>
	{
		[HttpPost]
		public async Task<HttpResponseMessage> GetCannedReport([FromBody] CannedRptParam param)
		{
			return await ExecuteBusinessAcsyn<List<Proc_Exception_GetReport_Result>>(() =>
			{
				return BusinessService.GetCannedReport(usercontext, param);
			}).ConfigureAwait(false);
		}
	}
}
