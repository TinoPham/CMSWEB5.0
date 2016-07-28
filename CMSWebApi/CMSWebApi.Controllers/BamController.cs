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
using CMSWebApi.Utils;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
    public class BamController : ApiControllerBase<IUsersService, MetricService>
	{
        [HttpPost]
		public async Task<HttpResponseMessage> GetMetricSumary(MetricParam request)
		{
			return await base.ExecuteBusinessAcsyn<MetricSumamryAll>(() =>
			{
				return BusinessService.GetMetricSumary(usercontext ,request);
			}).ConfigureAwait(false);
		}

        [HttpGet]
        [ActivityLog]
		public HttpResponseMessage GetMetricReport(int reportId)
        {
	        return base.ExecuteBusiness<List<MetricReportListModel>>(() =>
	        {
		        return BusinessService.GetMetricReport(usercontext, reportId);
	        });
        }

		[HttpGet]
		public HttpResponseMessage GetCustomReports(int groupId)
		{
			return base.ExecuteBusiness<List<ReportListModel>>(() =>
			{
				return BusinessService.GetCustomReports(usercontext, groupId);
			});
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage UpdateMetricReport(MetricReportUpdateModel metric)
		{
			return base.ExecuteBusiness(() =>
			{
				if (metric.Metrics == null || metric.Metrics.Count == 0)
				{
					throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
				}
				BusinessService.UpdateMetric(usercontext, metric);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

        [HttpGet]
        [ActivityLog]
		//public async Task<HttpResponseMessage> GetMetricDetail(MetricParam request)
        public async Task<HttpResponseMessage> GetMetricDetail([ModelBinder(typeof(DashboardBamModelBinderProvider))] MetricParam request)
        {
            return await base.ExecuteBusinessAcsyn<MetricSumamryDetail>(() =>
			{
				Task<MetricSumamryDetail> ret = BusinessService.GetMetricSumaryDetail(usercontext, request);
				return ret;
			}).ConfigureAwait(false);
		}

		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> GetReportDriveThrough([ModelBinder(typeof(DashboardBamModelBinderProvider))] MetricParam param)
		{
			return await base.ExecuteBusinessAcsyn<DriveThroughDataAll>(() =>
			{
				Task<DriveThroughDataAll> ret = BusinessService.GetReportDriveThrough(usercontext, param);
				return ret;
			}).ConfigureAwait(false);
           
		}
        [HttpGet]
        [ActivityLog]
        public  HttpResponseMessage DownloadExcelFile(string filepath)
        {
           // FileStreamContent filestream = new FileStreamContent();
            
            
           
           return  base.ResponseStream(System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppSettings.AppSettings.Instance.ExportFolder,filepath)) ,".xlsx",true,"Dashboard.xlsx");
		}

        [HttpGet]
        [ActivityLog]
        public async Task<HttpResponseMessage> GetNormalizeBySite([ModelBinder(typeof(DashboardBamModelBinderProvider))] MetricParam param)
        {
            return await base.ExecuteBusinessAcsyn<List<Normalizes>>(() =>
            {
                param.SetDate = param.WeekStartDate;
                Task<List<Normalizes>> ret = BusinessService.GetNormalizeBySite(usercontext, param);
                return ret;
            }).ConfigureAwait(false);
        }


        //[HttpPost]
        //[ActivityLog]
        //public async Task<HttpResponseMessage> Export(CMSWebApi.Utils.ExportTemplate.DashboardExportData DashboardExportData)
        //{

        //     return await base.ExecuteBusinessAcsyn<string>(() =>
        //    {

        //        Task<string> ret = new Task<string>();//BusinessService.Export(DashboardExportData);
        //        return ret;

        //    }).ConfigureAwait(false);
        // }
        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> UpdateNormalize(NormalizeParam request)
        {
            return await base.ExecuteBusinessAcsyn<string>(() =>
            {

                Task<string> ret = BusinessService.UpdateNormalize(usercontext, request);
                return ret;

            }).ConfigureAwait(false);
        }
	}
}
