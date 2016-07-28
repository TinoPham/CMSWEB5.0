using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CMSWebApi.APIFilters.ErrorHandler;
using CMSWebApi.BusinessServices.Sites;
using CMSWebApi.ServiceInterfaces;
using System.Web.Http;
using CMSWebApi.DataModels;
using CMSWebApi.APIFilters;
using CMSWebApi.Utils;
using System.IO;
using SVRDatabase;
using System.Web;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class SiteController : ApiControllerBase<ISiteService, SitesBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage Sites(bool hasChannel = false, bool allUsers = false)
		{
			SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return ExecuteBusiness<CMSWebSiteModel>(() =>
			{
				CMSWebSiteModel model = BusinessService.GetTreeSites(base.usercontext, true, interval, hasChannel, allUsers);
				return model;
			});
			//CMSWebSiteModel model = BusinessService.GetTreeSites(base.usercontext, true, userId);
			//return base.ResponseData<CMSWebSiteModel>(model);
		}

		[HttpGet]
		public HttpResponseMessage TreeSites(bool allUsers = false)
		{
			SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			CMSWebSiteModel model = BusinessService.GetTreeSites(base.usercontext, false, interval, false, allUsers);
			return ResponseData<CMSWebSiteModel>(model);
		}


		[HttpGet]
		public HttpResponseMessage SiteById(int siteId, bool hasChannels = false)
		{
			SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return ExecuteBusiness<CMSWebSiteModel>(() =>
			{
				CMSWebSiteModel model = BusinessService.GetSiteById(base.usercontext, siteId, hasChannels, interval);
				return model;
			});
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage SitesByPACID()
		{
			SVRManager DBModel = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SVRManager)) as SVRManager;
			int interval = (DBModel != null && DBModel.SVRConfig != null) ? DBModel.SVRConfig.KeepAliveInterval : 0;
			return ExecuteBusiness<CMSWebSiteModel>(() =>
			{
				CMSWebSiteModel model = BusinessService.GetSitePosPacId(base.usercontext, interval);
				return model;
			});
		}

		//[HttpPost]
		//public async Task<HttpResponseMessage> MoveDvrToSite(CMSWebSiteModel dvr, int toSiteKey)
		//{
		//	return await ExecuteBusinessTask(async () => await base.BusinessService.MoveDvrToSite(base.usercontext, dvr,toSiteKey));
		//}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage SaveTreeNode(CMSWebSiteModel treeSite)
		{
			return ExecuteBusiness(() =>
			{
				base.BusinessService.SaveTreeNode(treeSite, base.usercontext);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpGet]
		public HttpResponseMessage GetRegion(int regionkey)
		{
			return ExecuteBusiness<RegionModel>(() =>
			{
				RegionModel region = base.BusinessService.GetRegion(base.usercontext, regionkey);
				return region;
			});
		}

		[HttpGet]
		public HttpResponseMessage GetTreeMetrics()
		{
			return ExecuteBusiness<ICollection<TreeMetric>>(() =>
			{
				List<TreeMetric> metrictree = base.BusinessService.GetTreeMetrics(base.usercontext).ToList();
				return metrictree;
			});
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage AddRegion(RegionModel region)
		{
			//return ExecuteBusiness<RegionModel>(() =>
			//{
			//	RegionModel regionsave = base.BusinessService.AddRegion(base.usercontext, region);
			//	return regionsave;
			//});
			TransactionalModel<RegionModel> regionData = BusinessService.AddRegion(usercontext, region);
			return ResponseData<TransactionalModel<RegionModel>>(regionData);
		}

		[HttpPost]		
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage EditRegion(RegionModel region)
		{
			//return ExecuteBusiness(() =>
			//{
			//	base.BusinessService.UpdateRegion(base.usercontext, region);
			//	return Request.CreateResponse(HttpStatusCode.OK);
			//});
			TransactionalModel<RegionModel> regionData = BusinessService.UpdateRegion(usercontext, region);
			return ResponseData<TransactionalModel<RegionModel>>(regionData);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteRegion([FromBody]int regionKey)
		{
			TransactionalModel<RegionModel> response = BusinessService.DeleteRegion(usercontext, regionKey);
			return ResponseData<TransactionalModel<RegionModel>>(response);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteMultiRegion(CMSWebSiteModel regions)
		{
			TransactionalModel<RegionModel> response = BusinessService.DeleteMultiRegion(base.usercontext, regions);
			return ResponseData<TransactionalModel<RegionModel>>(response);
		}

		[HttpGet]
		public HttpResponseMessage GetSite(int siteKey)
		{
			return ExecuteBusiness<CmsSites>(() =>
			{
				CmsSites site = base.BusinessService.GetSites(base.usercontext, siteKey);
				return site;
			});
		}

		[HttpGet]
		public HttpResponseMessage GetMacFiles(int KDVR, int siteKey)
		{
			return ExecuteBusiness<IEnumerable<FileSiteModel>>(() => base.BusinessService.GetMacFiles(base.usercontext, KDVR, siteKey));
		}

		[HttpGet]
		public HttpResponseMessage GetAllMacFiles([FromUri] AllMacFilesModel model)
		{
		//	List<int> listdvrs = HttpUtility.UrlDecode(listKDVR).Split(new char[] { Utils.Consts.DECIMAL_SIGN }).Select(item => int.Parse(item)).ToList();
		//	return ExecuteBusiness<List<FileSiteModel>>(() => base.BusinessService.GetAllMacFiles(base.usercontext, listdvrs, siteKey));
			//char[] seperator = new char[1];
			//seperator[0] = Utils.Consts.DECIMAL_SIGN;
			return ExecuteBusiness<List<FileSiteModel>>(() => base.BusinessService.GetAllMacFiles(base.usercontext, model));
		}
        [HttpGet]
        public HttpResponseMessage GetListSite()
        {
            return ExecuteBusiness<List<CMSWebSiteModel>>(() => base.BusinessService.GetListSite(base.usercontext));
        }


		[HttpGet]
		public HttpResponseMessage GetImageChannel(string name, int kdvr) { 
			string fpath = string.Empty;
			fpath = Path.Combine(AppSettings.AppSettings.Instance.DvrPath, AppSettings.AppSettings.RawImages, kdvr.ToString(), name);
			if (!File.Exists(fpath))
			{
				fpath =Path.Combine(AppDomain.CurrentDomain.BaseDirectory,Utils.Consts.ImagesFolder,Utils.Consts.THUMBNAIL_Icon);
				
			}
			return ResponseFile( fpath);
		}

		[HttpGet]
		public HttpResponseMessage GetFirstImage(string chs, string kdvrs)
		{
			string fpath = string.Empty;
			string name = string.Empty;
			List<string> lsKDVR = kdvrs.Split(',').ToList();
			bool found = false;
			string fdir = string.Empty;
			foreach (string kdvr in lsKDVR)
			{
				if (!String.IsNullOrEmpty(chs))
				{
					List<int> lsChanIds = chs.Split(',').Select(x => String.IsNullOrEmpty(x) ? -1 : Convert.ToInt32(x.Trim())).ToList();
					for (int i = 0; i < lsChanIds.Count; i++)
					{
						name = string.Format("C_{0:00}.jpg", lsChanIds[i] + 1);
						fpath = Path.Combine(AppSettings.AppSettings.Instance.DvrPath, AppSettings.AppSettings.RawImages, kdvr, name);
						if (File.Exists(fpath))
						{
							found = true;
							break;
						}
					}
				}
				else
				{
					try
					{
						fdir = Path.Combine(AppSettings.AppSettings.Instance.DvrPath, AppSettings.AppSettings.RawImages, kdvr);
						DirectoryInfo di = new DirectoryInfo(fdir);
						fpath = di.GetFiles().Select(fi => fi.FullName).FirstOrDefault();
						if (File.Exists(fpath))
						{
							found = true;
						}
					}
					catch(Exception) {}
				}
				if (found)
					break;
			}
			if (!found)
			{
				fpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Utils.Consts.ImagesFolder, Utils.Consts.THUMBNAIL_Icon);
			}
			return ResponseFile(fpath);
		}

		[HttpGet]
		public HttpResponseMessage GetFile(int skey, string fname, string fdname)
		{
			string filePath = string.Empty;
			if (string.IsNullOrEmpty(fdname))
			{
				filePath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, skey.ToString(), fname);
				return ResponseFile(filePath);
			}

			filePath = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, skey.ToString(),fdname, fname);
			return ResponseFile(filePath);
		}

		[HttpGet]
		public HttpResponseMessage GetImageSite(int skey)
		{
			TransactionalModel<List<ImageModel>> response = BusinessService.GetImageSite(usercontext, skey);
			return ResponseData<TransactionalModel<List<ImageModel>>>(response);
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public async Task<HttpResponseMessage> AddSite(CmsSites site)
		{
			TransactionalModel<CmsSites> response = await base.BusinessService.AddSite(base.usercontext, site);
			return ResponseData<TransactionalModel<CmsSites>>(response);
			
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public async Task<HttpResponseMessage> EditSite(CmsSites site)
		{
			TransactionalModel<CmsSites> response = await base.BusinessService.EditSite(base.usercontext, site);
			return ResponseData<TransactionalModel<CmsSites>>(response);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteSite([FromBody]int siteKey)
		{
			TransactionalModel<CmsSites> response = BusinessService.DeleteSite(usercontext, siteKey);
			return ResponseData<TransactionalModel<CmsSites>>(response);
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage MacAddress(int sitekey)
		{
			return ResponseData<List<DVRModel>>(base.BusinessService.GetDVRInfo(sitekey));
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage ZipCode(string filter)
		{
			return ResponseData<List<ZipCodeModel>>(BusinessService.FilterZipCode(filter));
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage ZipCode(ZipCodeModel model)
		{
			TransactionalModel<ZipCodeModel> response = BusinessService.AddZipCode(model);
			return ResponseData<TransactionalModel<ZipCodeModel>>(response);
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetAllHaspLicense(int siteKey)
		{
			return ResponseData<IEnumerable<HaspLicense>>(BusinessService.GetAllHaspLicense(siteKey));
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetPacInfoBySites(string siteKeys)
		{
			return ResponseData<IEnumerable<CMSPACSiteModel>>(BusinessService.GetPacInfoBySites(usercontext, siteKeys));
		}

        [HttpGet]
        [ActivityLog]
        public HttpResponseMessage GetDVRInfoRebarTransact(int kdvr)
        {
            TransactionalModel<DVRInfoRebarTransact> respone = BusinessService.GetDVRInfoRebarTransact(kdvr);
            return ResponseData<TransactionalModel<DVRInfoRebarTransact>>(respone);
        }
	}
}
