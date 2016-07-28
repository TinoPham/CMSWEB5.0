using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Query;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Rebar;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;


namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class RebarController : ApiControllerBase<IRebarDataService, RebarService>
	{

		[HttpGet]
		public HttpResponseMessage GetTransactionInfo(long tranId, int PacId =0, int? registerId = null, int nextOrPrev = 0)
		{
			return
				ExecuteBusiness<Transaction>(() => BusinessService.GetPosTransactionInfo(usercontext, tranId, PacId, registerId, nextOrPrev));
		}

		[HttpGet]
		public HttpResponseMessage GetTransactionTypes()
		{
			return ExecuteBusiness<IQueryable<TranExceptionType>>(() => BusinessService.GetTransactionTypes(usercontext));
		}
		
		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<TransactionDetailViewerModel> GetAhocViewer([FromUri] string keys = "")
		{
			List<int> sitekeys = null;
			if (keys != "")
			{
				sitekeys = keys.Split(',').Select(Int32.Parse).ToList();
			}
			return BusinessService.GetAhocViewer(usercontext, Request, sitekeys);
		}

		[HttpGet]
		public HttpResponseMessage GetTaxsList()
		{
			return ExecuteBusiness<IQueryable<TranTax>>(() => BusinessService.GetTaxsList(usercontext));
		}

		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<TransactionViewerModel> GetTransactionViewer([FromUri] bool isPOSTransac = false, [FromUri] string keys = "", [FromUri] string flags = "", [FromUri] string pays = "", [FromUri] string taxs = "")
		{
			List<int> sitekeys = null, flaglist = null, taxlist = null, paymentlist = null;
			if (keys != "")
			{
				sitekeys = keys.Split(',').Select(Int32.Parse).ToList();
			}

			if (flags != "")
			{
				flaglist = flags.Split(',').Select(Int32.Parse).ToList();
			}

			if (taxs != "")
			{
				taxlist = taxs.Split(',').Select(Int32.Parse).ToList();
			}

			if (pays != "")
			{
				paymentlist = pays.Split(',').Select(Int32.Parse).ToList();
			}

			if (isPOSTransac)
			{
				return BusinessService.GetPOSTransactionViewer(Request, usercontext, sitekeys, flaglist, paymentlist, taxlist);
			}
			else
			{
				return BusinessService.GetTransactionViewer(usercontext, sitekeys);
			}

		}


		[HttpPost]
		public HttpResponseMessage SaveTransactionNotes([FromBody]ExceptionModel param)
		{
			return ExecuteBusiness(() =>
			{
				var result = BusinessService.SaveTransactionNotes(usercontext, param);
				return Request.CreateResponse(HttpStatusCode.OK, result);
			});
		}

		[HttpPost]
		public HttpResponseMessage AddTransactionFlagType([FromBody]TranExceptionType param)
		{
			return ExecuteBusiness(() =>
			{
				var result = BusinessService.AddTransactionFlagType(usercontext, param);
				return Request.CreateResponse(HttpStatusCode.OK, result);
			});
		}

		[HttpPost]
		public HttpResponseMessage UpdateTransactionFlagType([FromBody]TranExceptionType param)
		{
			return ExecuteBusiness(() =>
			{
				var result = BusinessService.UpdateTransactionFlagType(usercontext, param);
				return Request.CreateResponse(HttpStatusCode.OK, result);
			});
		}

		[HttpPost]
		public HttpResponseMessage DelTransactionFlagType([FromBody]TranExceptionType param)
		{
			return ExecuteBusiness(() =>
			{
				var result =  BusinessService.DelTransactionFlagType(usercontext, param);
				return Request.CreateResponse(HttpStatusCode.OK, result);
			});
		}

		[HttpPost]
		public HttpResponseMessage SaveTransactionFlagTypes([FromBody]List<TranExceptionType> param)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.SaveTransactionFlagTypes(usercontext, param);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		public HttpResponseMessage GetEmployeeRisks([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<EmployeeRiskSummaryPagings>(() => BusinessService.GetEmployeeRisks(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetSitesRisks([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<SiteRiskSummaryPagings>(() => BusinessService.GetSitesRisks(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetTransWOCust([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<SiteExceptionTransPagings>(() => BusinessService.GetTransWOCust(usercontext, param));
		}
		[HttpPost]
		public HttpResponseMessage GetCustsWOTran([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<SiteExceptionCustsPagings>(() => BusinessService.GetCustsWOTran(usercontext, param));
		}
		[HttpPost]
		public HttpResponseMessage GetCarsWOTran([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<SiteExceptionCustsPagings>(() => BusinessService.GetCarsWOTran(usercontext, param));
		}
		[HttpPost]
		public HttpResponseMessage GetIOPCCustomer([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<ExceptionCustomerPagings>(() => BusinessService.GetIOPCCustomerViewer(usercontext, param));
		}
		[HttpPost]
		public HttpResponseMessage GetIOPCCar([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness<ExceptionCarPagings>(() => BusinessService.GetIOPCCarViewer(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GeTransactionFilterPagings([FromBody]TransactionFilterParam param)
		{
			return ExecuteBusiness<TransactionFilterPagings>(() => BusinessService.GeTransactionFilterPagings(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetBoxWidgets([FromBody]BoxRebarParamModel param)
		{
			return ExecuteBusiness<List<BoxRebarModel>>(() => BusinessService.GetMetricBox(usercontext, param));
		}

		[HttpPost]

		public HttpResponseMessage GetEmployerInfoBySite([FromBody]EmployerParamModel param)
		{
			return ExecuteBusiness<EmployerModel>(() => BusinessService.GetEmployerRebarForChart(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetTransacByEmployee([FromBody]EmplTransacParam param)
		{
			return ExecuteBusiness<EmplTranctionPagings>(() => BusinessService.GetTransacByEmployee(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetTransactPaymentTypes([FromBody]TranPaymentParam param)
		{
			return ExecuteBusiness<IQueryable<TranPaymentChartModel>>(() => BusinessService.GetTransactPaymentTypes(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetTransacDetailByPaymentType([FromBody]TranPaymentDetailParam param)
		{
			return ExecuteBusiness<PaymTranctionPagings>(() => BusinessService.GetTransacDetailByPaymentType(usercontext, param));
		}

		[HttpPost]
		public HttpResponseMessage GetWeekAtGlanceSummary([FromBody]RebarWeekAtAGlanceParam param)
		{
			return ExecuteBusiness < IQueryable<SummaryWeekAtGlanceModel>>(() => BusinessService.GetWeekAtGlanceSummary(usercontext, param));
		}

		[HttpGet]
		public HttpResponseMessage GetPaymentList([FromUri]PagingParam param)
		{
			return ExecuteBusiness<ModelPaging>(() => BusinessService.GetPaymentList(param));
		}

		[HttpGet]
		public HttpResponseMessage GetRegisterList([FromUri]PagingParam param)
		{
			return ExecuteBusiness<ModelPaging>(() => BusinessService.GetRegisterList(param));
		}

		[HttpGet]
		public HttpResponseMessage GetOperatorList([FromUri]PagingParam param)
		{
			return ExecuteBusiness<ModelPaging>(() => BusinessService.GetOperatorList(param));
		}

		[HttpGet]
		public HttpResponseMessage GetDescriptionList([FromUri]PagingParam param)
		{
			return ExecuteBusiness<ModelPaging> (() => BusinessService.GetDescriptionList(param));
		}

		[HttpGet]
		public HttpResponseMessage FilterPayment(string filter)
		{
			return ExecuteBusiness<IQueryable<PaymentModel>>(() => BusinessService.FilterPayment(filter));
		}

		[HttpGet]
		public HttpResponseMessage FilterRegister(string filter)
		{
			return ExecuteBusiness<IQueryable<RegisterModel>>(() => BusinessService.FilterRegister(filter));
		}

		[HttpGet]
		public HttpResponseMessage FilterOperator(string filter)
		{
			return ExecuteBusiness<IQueryable<OperatorModel>>(() => BusinessService.FilterOperator(filter));
		}
		[HttpGet]
		public HttpResponseMessage FilterDescription(string filter)
		{
			return ExecuteBusiness<IQueryable<DescriptionModel>>(() => BusinessService.FilterDescription(filter));
		}

		[HttpGet]
		public HttpResponseMessage GetDescriptionById(string keys)
		{
			return ExecuteBusiness<IQueryable<DescriptionTransModel>>(() => BusinessService.GetDescriptionById(keys));
		}

		[HttpPost]
		public HttpResponseMessage GetColumnOpion([FromBody]List<ColumnOptionParams> param)
		{
			return ExecuteBusiness<List<ColumnOptionModel>>(() => BusinessService.GetColumnOption(param));
		}

		[HttpGet]
		public HttpResponseMessage GetTerminal()
		{
			return ExecuteBusiness<IQueryable<ColumnModel>>(() => BusinessService.GetTerminal());
		}

		[HttpGet]
		public HttpResponseMessage GetStore()
		{
			return ExecuteBusiness<IQueryable<ColumnModel>>(() => BusinessService.GetStore());
		}

		[HttpGet]
		public HttpResponseMessage GetCheckID()
		{
			return ExecuteBusiness<IQueryable<ColumnModel>>(() => BusinessService.GetCheckID());
		}
	}
}
