using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Query;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Rebar;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class AdhocController : ApiControllerBase<IAdhocDataService, AdhocService>
	{
		[HttpGet]
		public HttpResponseMessage GetAdhocs(int? id = 0)
		{
			return ExecuteBusiness<List<AdhocModel>>(() =>
			{
				return base.BusinessService.GetAdhocs(base.usercontext, id);
			});
		}

		[HttpGet]
		public HttpResponseMessage GetAhocReportColumn()
		{
			return ExecuteBusiness<IQueryable<AdhocColDefindModel>>(() =>
			{
				return base.BusinessService.GetAhocReportColumn(base.usercontext);
			});
		}

		[HttpGet]
		public HttpResponseMessage GetAdhocReportById(int reportId)
		{
			return ExecuteBusiness<AdhocReportModel>(() =>
			{
				return base.BusinessService.GetReport(base.usercontext, reportId);
			});
		}

		[HttpGet]
		public HttpResponseMessage GetAdhocFoldersById(int folderId)
		{
			return ExecuteBusiness<AdhocReportFolderModel>(() =>
			{
				return base.BusinessService.GetAdhocReportFoldersById(base.usercontext, folderId);
			});
		}

		[HttpGet]
		public HttpResponseMessage DeleteAdhocReport(int reportId)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.DeleteAdhocReport(usercontext, reportId);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpGet]
		public HttpResponseMessage DeleteAdhocReportFolder(int folderId)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.DeleteAdhocReportFolder(usercontext, folderId);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		public HttpResponseMessage UpdateAdhocReport(AdhocReportModel report)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.UpdateAdhocReport(usercontext, report);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		public HttpResponseMessage AddAdhocReport(AdhocReportModel report)
		{
			return ExecuteBusiness <AdhocReportModel>(() =>
			{
				return BusinessService.SettingAhocReport(base.usercontext, report);
			});
		}

		[HttpPost]
		public HttpResponseMessage UpdateAdhocReportFolder(AdhocReportFolderModel report)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.UpdateAdhocReportFolder(usercontext, report);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		public HttpResponseMessage AddAdhocReportFolder(AdhocReportFolderModel report)
		{
			return ExecuteBusiness<AdhocReportFolderModel>(() =>
			{
				return BusinessService.SettingAdhocReportFolder(usercontext, report);
			});
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetCardList()
		{
			return BusinessService.GetCardList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetCamList()
		{
			return BusinessService.GetCamList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetShiftList()
		{
			return BusinessService.GetShiftList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetStoreList()
		{
			return  BusinessService.GetStoreList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetCheckList()
		{
			return BusinessService.GetCheckList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetTerminalList()
		{
			return  BusinessService.GetTerminalList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetDescList()
		{
			return BusinessService.GetDescList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetItemList()
		{
			return BusinessService.GetItemList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetTaxtLists()
		{
			return BusinessService.GetTaxtLists();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetPaymentList()
		{
			return BusinessService.GetPaymentList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetRegisterList()
		{
			return BusinessService.GetRegisterList();
		}

		[HttpGet]
		[CmsQueryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 1000)]
		public IQueryable<ListModel> GetOperatorList()
		{
			return BusinessService.GetOperatorList();
		}
	}
}
