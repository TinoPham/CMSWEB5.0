using CMSWebApi.APIFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.BusinessServices.Distribution;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using PACDMModel.Model;
using System.Web;
using System.Net;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class DistributionController : ApiControllerBase<IDistributionService, DistributionBusiness>
	{
		[HttpGet]
		[ActivityLog]
		public async Task<HttpResponseMessage> GetReportData([ModelBinder(typeof(BAMRptModelBinderProvider))] BAMRptParam param)
		{
			return await base.ExecuteBusinessAcsyn<DistributionDataAll>(() =>
			{
				Task<DistributionDataAll> ret = BusinessService.GetDataReport(usercontext, param);
				return ret;
			}).ConfigureAwait(false);
		}

		[HttpPost]
		[ActivityLog]
		public async Task<HttpResponseMessage> GetArea(AddQueueParam param)
		{
			return await base.ExecuteBusinessAcsyn <DistributionQueueRegion>(() =>
			{
				Task<DistributionQueueRegion> ret = BusinessService.GetArea(usercontext, param);
				return ret;
			}).ConfigureAwait(false);
		}

		//[HttpPost]
		//[ActivityLog]
		//public Task<HttpResponseMessage> AddQueue(List<DistributionQueue> Queue)
		//{

		//	return base.ExecuteBusiness<List<DistributionQueue>>(() =>
		//	{
		//		List<DistributionQueue> ret = BusinessService.AddQueue(usercontext, Queue);
		//		return ret;
		//	});
		//}



		[HttpPost]
		[ActivityLog]
		public async Task<HttpResponseMessage> AddQueue(AddQueueParam Queue)
		{
			//TransactionalModel<List<DistributionQueue>> response = BusinessService.AddQueue(usercontext, Queue);
			//return ResponseData<TransactionalModel<List<DistributionQueue>>>(response);

			return await base.ExecuteBusinessAcsyn<TransactionalModel<List<DistributionQueue>>>(() =>
			{
				Task<TransactionalModel<List<DistributionQueue>>> ret = BusinessService.AddQueue(usercontext, Queue);
				return ret;
			}).ConfigureAwait(false);
		}

		[HttpPost]
		[ActivityLog]
		public async Task<HttpResponseMessage> applyStore(AddQueueParam Queue)
		{
			//TransactionalModel<List<DistributionQueue>> response = BusinessService.AddQueue(usercontext, Queue);
			//return ResponseData<TransactionalModel<List<DistributionQueue>>>(response);

			return await base.ExecuteBusinessAcsyn<TransactionalModel<List<DistributionQueue>>>(() =>
			{
				Task<TransactionalModel<List<DistributionQueue>>> ret = BusinessService.applyStore(usercontext, Queue);
				return ret;
			}).ConfigureAwait(false);
		}

		//[HttpPost]
		//[ActivityLog]
		//public HttpResponseMessage DeleteQueue(DistributionQueue Queue)
		//{
		//	TransactionalModel<DistributionQueue> response = BusinessService.DeleteQueue(usercontext, Queue);
		//	return ResponseData<TransactionalModel<DistributionQueue>>(response);

		//}

        [HttpPost]
        [ActivityLog]
        public TransactionalModel<HeatMapsImage> UploadFromDialog(int KDVR, int id, string shedule)
        {
            try
            {
                HttpFileCollection filesCollection = HttpContext.Current.Request.Files;
                return BusinessService.UploadFromDialog(KDVR, id, filesCollection, shedule);
            }
            catch (Exception e)
            {
                return null;
            }

            //return null;
        }

        [HttpGet]
        [ActivityLog]
        public async Task<HttpResponseMessage> GetDataHeatMap([ModelBinder(typeof(BAMHeatMapModelBinderProvider))] BAMHeatMapParam param)
        {
            return await base.ExecuteBusinessAcsyn<HeatMapsModel>(() =>
            {
                Task<HeatMapsModel> ret = BusinessService.GetDataHeatMap(usercontext, param);
                return ret;
            }).ConfigureAwait(false);
        }

        [HttpGet]
        [ActivityLog]
        public HttpResponseMessage Images(int img)
        {
            string path = BusinessService.GetHeatMapImages(img);
            return ResponseFile(path);
        }

        [HttpGet]
        [ActivityLog]
        public HttpResponseMessage GetListChannels(int KDVR)
        {
            return base.ExecuteBusiness<IEnumerable<tDVRChannels>>(() =>
            {
                return BusinessService.GetListChannels(usercontext, KDVR);
            });
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> InsertImage(HeatMapsModel model)
        {
            TransactionalModel<HeatMapsImage> SaveUpLoad = await base.BusinessService.InsertImage(model, base.usercontext);
            return ResponseData<TransactionalModel<HeatMapsImage>>(SaveUpLoad);
        }

        [HttpGet]
        [ActivityLog]
        public HttpResponseMessage CheckExistsImage(int KDVR, int schedule, string dateImage)
        {
            bool result = BusinessService.CheckExistImage(usercontext, KDVR, schedule, dateImage);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [ActivityLog]
        public async Task<HttpResponseMessage> GetDataScheduleTasks([ModelBinder(typeof(BAMHeatMapModelBinderProvider))] BAMHeatMapParam param)
        {
            return await base.ExecuteBusinessAcsyn<IEnumerable<ScheduleTasks>>(() =>
            {
                Task<IEnumerable<ScheduleTasks>> ret = BusinessService.GetDataScheduleTasks(usercontext, (int)param.rptDataType);
                return ret;
            }).ConfigureAwait(false);
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> InsertDataScheduleTasks(ScheduleTasks model)
        {
            TransactionalModel<ScheduleTasks> SavesShedule = await base.BusinessService.InsertDataScheduleTasks(model, base.usercontext);
            return ResponseData<TransactionalModel<ScheduleTasks>>(SavesShedule);
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> UpdateDataScheduleTasks(ScheduleTasks model)
        {
            TransactionalModel<ScheduleTasks> UpdateShedule = await base.BusinessService.UpdateDataScheduleTasks(model, base.usercontext);
            return ResponseData<TransactionalModel<ScheduleTasks>>(UpdateShedule);
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> DeleteDataScheduleTasks(ScheduleTasks idSchedule)
        {
            TransactionalModel<ScheduleTasks> UpdateShedule = await base.BusinessService.DeleteDataScheduleTasks(idSchedule, base.usercontext);
            return ResponseData<TransactionalModel<ScheduleTasks>>(UpdateShedule);
        }
	}
}
