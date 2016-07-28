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
using System.Text.RegularExpressions;

namespace CMSWebApi.Controllers
{
	public class QSearchController: ApiControllerBase<IQuickSearchService, QSearchBusinessSevice>
	{
		[HttpGet]
		public HttpResponseMessage Get(string id)
		{
			if( string.IsNullOrEmpty(id))
				return base.ResponseData<IQueryable<TransactionDetailModel>>( Enumerable.Empty<TransactionDetailModel>().AsQueryable(), System.Net.HttpStatusCode.OK);

			return ExecuteBusiness<IQueryable<DataModels.TransactionDetailModel>>(() =>
			{
				string[]trans = id.Split(',');
				List<long> tranids = new List<long>();
				long current = 0;
				foreach(string item in trans)
				{
					if( !Int64.TryParse(item, out current) || current <= 0)
						continue;
					if(tranids.Count > 1000)
						break;

					tranids.Add(current);
				}

				return BusinessService.TransactionDetail(tranids);
			});
		}
		[HttpPost]
		public HttpResponseMessage QuickSearchReport([FromBody] QuickSearchParam param)
		{
			return ExecuteBusiness<IQueryable<DataModels.QSSummaryModel>>(() =>
			{
				return BusinessService.QuickSearchReport(usercontext, param);
			});
		}
	}
}
