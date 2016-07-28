using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.FiscalYear;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Net;
using CMSWebApi.APIFilters;
using Newtonsoft.Json;

namespace CMSWebApi.Controllers
{
	[RoutePrefix("FiscalYear")]
	[WebApiAuthenication]
	public class FiscalYearController : ApiControllerBase<IFiscalYearServices, FiscalYearBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetFiscalYear(string date)
		{
			//int userID =   //base.usercontext.Createdby.HasValue ? base.usercontext.Createdby.Value : base.usercontext.ID;
			DateTime? _date = new Nullable<DateTime>();
			if (!string.IsNullOrEmpty(date))
				_date = DateTime.ParseExact(date, "MM/dd/yyyy", null);

			FiscalYearModel result = (_date == null || _date.HasValue == false) ? base.BusinessService.GetFiscalYear(base.usercontext) : base.BusinessService.GetCustomFiscalYear(base.usercontext, _date.Value);
			return ResponseData<FiscalYearModel>(result);
			//return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		//[HttpGet]
		//public HttpResponseMessage GetCustomFiscalYear(string date) // MM/DD/YYYY
		//{

		//	int userID = base.usercontext.Createdby.HasValue ? base.usercontext.Createdby.Value : base.usercontext.ID;
		//	DateTime _date = DateTime.ParseExact(date, "MM/dd/yyyy",null);
		//	FiscalYearData result = base.BusinessService.GetCustomFiscalYear(userID,_date);
		//	return Request.CreateResponse(HttpStatusCode.OK, result);
		//}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage Update(FiscalYearModel fyModel)
		{
			//var result = JsonConvert.DeserializeObject(fyModel.ToString());
			FiscalYearModel result = base.BusinessService.Update((FiscalYearModel)fyModel);
			return ResponseData<FiscalYearModel>(result);
			//return Request.CreateResponse(HttpStatusCode.OK, result);
		}
	}
}
