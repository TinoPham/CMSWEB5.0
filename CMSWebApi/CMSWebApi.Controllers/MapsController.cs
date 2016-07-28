using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.JobTitle;
using CMSWebApi.BusinessServices.Map;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using System.Drawing;
using System.Web;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class MapsController : ApiControllerBase<IMapsService, MapsBusiness>
	{
		
		[HttpGet]
		[ActivityLog]
		public  HttpResponseMessage Gets(int sitekey)
		{
			MapsModel maps = base.BusinessService.Gets(sitekey);
			return  ResponseData<MapsModel>(maps);
		}

		[HttpPost]
		[ActivityLog]
		public async Task<HttpResponseMessage> Maps(MapsModel model)
		{
			TransactionalModel<MapsModel> maps = await base.BusinessService.Sets(model,base.usercontext.ID);
			return ResponseData<TransactionalModel<MapsModel>>(maps);
		}

		[HttpGet]
		[ActivityLog]
		public  HttpResponseMessage Images(int sitekey, string filename, bool thumbnail)
		{
			string path = BusinessService.GetMapImages(sitekey, filename, thumbnail);
			return ResponseFile(path);
		}

		[HttpPost]
		[ActivityLog]
		public TransactionalModel<MapsImage> Upload(int sitekey,int id)
		{
			try
			{
				HttpFileCollection filesCollection = HttpContext.Current.Request.Files;
				return BusinessService.Upload(sitekey, id, filesCollection);
			}
			catch (Exception e)
			{
				return null;
			}
			
			//return null;
		}
		
        [HttpPost]
        [ActivityLog]
        public TransactionalModel<MapsImage> UploadFromDialog(int sitekey, int id)
        {
            try
            {
                HttpFileCollection filesCollection = HttpContext.Current.Request.Files;
                return BusinessService.UploadFromDialog(sitekey, id, filesCollection);
            }
            catch (Exception e)
            {
                return null;
            }

            //return null;
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> InsertModelFromDialog(MapsModel model)
        {
            TransactionalModel<MapsModel> maps = await base.BusinessService.InsertModelFromDialog(model, base.usercontext.ID);
            return ResponseData<TransactionalModel<MapsModel>>(maps);
        }

        [HttpPost]
        [ActivityLog]
        public async Task<HttpResponseMessage> DeleteModelFromButtonX(MapsModel model)
        {
            TransactionalModel<MapsModel> maps = await base.BusinessService.DeleteModelFromButtonX(model, base.usercontext.ID);
            return ResponseData<TransactionalModel<MapsModel>>(maps);
        }

	}
}
