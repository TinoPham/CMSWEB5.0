using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using CMSWebApi.BusinessServices.FilesManager;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using Commons;
using PACDMModel.Model;
using CMSWebApi.APIFilters;
using System.Linq.Expressions;
using Extensions.Linq;
using Extensions;
namespace CMSWebApi.BusinessServices.Sites
{
	public class SitesBusinessService : BusinessBase<ISiteService>
	{
		//private IFilesManager _fileManager;
		public IUsersService UsersService { get; set; }
		public IMetricSiteService MetricService { get; set; }
		public ICalendarService CalendarService { get; set; }
		public IPOSService PosService { get; set; }
		public IDVRService DVRService { get; set; }
		public IUsersService IUser { get; set; }

		private readonly Expression<Func<tCMSWeb_Metric_List, TreeMetric>> dbMetric2model =
			db => new TreeMetric {Id = db.MListID, Name = db.MetricName, ParentId = db.ParentID};

		private readonly Expression<Func<tCMSWebSites, CMSWebSiteModel>> dbsite2sitemodel = dbsite => new CMSWebSiteModel
		{
			Type = Utils.SiteType.SITE,
			ID = dbsite.siteKey,
			Name = dbsite.ServerID,
			//MACAddress = dbsite.MACAddress,
			UserID = dbsite.UserID,
			ParentKey = dbsite.RegionKey,
			ImageSite = dbsite.ImageSite,
			PACData = false
		};

		internal IQueryable<T> Sites<T>( Expression<Func<tCMSWebSites, bool>> filter, Expression<Func<tCMSWebSites,T>> selector)
		{
			return new CMSWebApi.DataServices.ModelDataService<tCMSWebSites>(base.ServiceBase).Gets<T>( filter, null, selector);
		}
		private readonly Expression<Func<tCMSWebRegion, CMSWebSiteModel>> dbRegion2sitemodel = dbregion => new CMSWebSiteModel
		{
			Type = Utils.SiteType.REGION,
			//UserID = dbsite.UserID,
			ID = dbregion.RegionKey,
			Name = dbregion.RegionName,
			UserID = dbregion.UserKey,
			ParentKey = dbregion.RegionParentID

		};

		public CMSWebSiteModel GetSites(UserContext context, int? userId = null)
		{
			bool is_admin = userId.HasValue ? userId.Value == context.ParentID : context.ID == context.ParentID;
			int user = userId.HasValue ? userId.Value : context.ID;
			IQueryable<CMSWebSiteModel> sites = DataService.GetSites<CMSWebSiteModel>(user, is_admin, dbsite2sitemodel, null);
			IQueryable<CMSWebSiteModel> regions = DataService.GetRegions<CMSWebSiteModel>(context.ID, dbRegion2sitemodel, null);
			return SiteModel(regions, sites);
		}
        /* Duc Get site for 3rd party*/
        public List<CMSWebSiteModel> GetListSite(UserContext context, int? userId = null)
        {
            bool is_admin = userId.HasValue ? userId.Value == context.ParentID : context.ID == context.ParentID;
            int user = userId.HasValue ? userId.Value : context.ID;
            string[] includes = new string[2];
            includes[0] = "tDVRChannels.tDVRAddressBook";
            includes[1] = "tDVRChannels";
            IQueryable<tCMSWebSites> sites = DataService.GetSites<tCMSWebSites>(user, is_admin, selector => selector, includes);
            List<CMSWebSiteModel> result = new List<CMSWebSiteModel>();
            sites.ToList().ForEach(item => result.AddRange(GetKdvrList(item, true, 10,true)));
            return result;
        }

		public List<FileSiteModel> GetMacFiles(UserContext context, int KDVR, int siteKey)
		{
			var site = DataService.GetSite(siteKey, t => new {SiteId = t.siteKey}, null);
			if (site == null)
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), "siteKey", siteKey);
			}
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, site.SiteId.ToString(), KDVR.ToString());
			IEnumerable<FileInfo> files = base.FileManager.DirGetFileInfos(path);
			return files.Select(it => new FileSiteModel {KDVR = KDVR, SiteKey = siteKey, Name = it.Name}).ToList();
		}

		public List<FileSiteModel> GetAllMacFiles(UserContext context, AllMacFilesModel model)
		{
			List<FileSiteModel> response = new List<FileSiteModel>();
			model.listKDVR.ToList().ForEach(item => response.AddRange(GetMacFiles(context, item, model.siteKey).ToList()));
			return response;
		}

		public TransactionalModel<List<ImageModel>> GetImageSite(UserContext userLogin, int siteKey)
		{
			TransactionalModel<List<ImageModel>> response = new TransactionalModel<List<ImageModel>>();
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, siteKey.ToString());
			IEnumerable<FileInfo> files = base.FileManager.DirGetFileInfos(path);
			if (!files.Any())
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DATA_NOT_FOUND.ToString());
				return response;
			}
			response.ReturnStatus = true;
			response.Data = FileManager.DirGetFileInfos(path, null, false).Select(s => new ImageModel() {Name = s.Name}).ToList();
			return response;
		}

		public IEnumerable<CMSPACSiteModel> GetPacInfoBySites(UserContext context, string siteKeys)
			{
			List<int> listSiteKeys = siteKeys.Split(new char[] { ',' }).Select(int.Parse).ToList();
			var userSiteDVRChannel = Task.Run(() => base.UserSitesBySiteIDsAsync(IUser, context, listSiteKeys))
									.Result
									.Select(s => new { siteKey = s.siteKey.Value, PACID = s.PACID.Value, KDVR = s.KDVR.Value })
									.Where(w => listSiteKeys.Contains(w.siteKey)).AsEnumerable().Distinct();
			var siteDB = DataService.GetSites<tCMSWebSites>(t => listSiteKeys.Contains(t.siteKey), t => t, null).AsEnumerable();
			var data = (from site in siteDB
						join usdc in userSiteDVRChannel on site.siteKey equals usdc.siteKey into sitedata
						from s in sitedata.DefaultIfEmpty()
						select new CMSPACSiteModel()
						{
							SiteKey = site.siteKey,
							SiteName = site.ServerID,
							PacId = s == null ? 0 : s.PACID,
							KDVR = s == null ? 0 : s.KDVR
						}).Where(w => listSiteKeys.Contains(w.SiteKey)).Distinct();

			return data;
		}

		public CMSWebSiteModel GetSitePosPacId(UserContext context, int keepaliveint)
		{
			var masterKey = UsersService.GetMasterID(context.CompanyID);
			var parentRegions = GetRegionsToModel(masterKey);

			List<int> kdvrHasPacList = PosService.GetPOSPAC(null, p => p.KDVR, null).ToList();

			var sitePermission = GetPermissionPacSites(context, kdvrHasPacList, keepaliveint);

			var regionUsers = context.ID == context.ParentID ? new List<CMSWebSiteModel>() : GetRegionsToModel(context.ID);

			var rootRegion = parentRegions.FirstOrDefault(item => item.ParentKey == null);
			if (rootRegion != null)
				BuildPermisionTree(rootRegion, parentRegions, sitePermission, regionUsers, context, false);
			return rootRegion;
		}

		public CMSWebSiteModel GetTreeSites(UserContext context, bool hasMac, int keepaliveint, bool hasChannel = false,
			bool allUsers = false)
		{

			var masterKey = UsersService.GetMasterID(context.CompanyID);
			var parentRegions = GetRegionsToModel(masterKey);

			var users =
				UsersService.GetListUser(t => t.CompanyID == context.CompanyID && t.UserID != masterKey)
					.Select(t => t.UserID)
					.ToList();

			List<CMSWebSiteModel> regionUsers = GetRegionsToModel(context.ID, users);
			//List<CMSWebSiteModel> regionUsers = allUsers ? GetRegionsToModel(context.ID, users) : context.ID == context.ParentID ? new List<CMSWebSiteModel>() : GetRegionsToModel(context.ID);

			List<CMSWebSiteModel> sitePermission;
			if (allUsers)
			{
				users.Add(context.ParentID);
				sitePermission = GetMultiUserPermissionSites(context, users, hasMac, hasChannel, keepaliveint);
			}
			else
			{
				sitePermission = GetPermissionSites(context, hasMac, hasChannel, keepaliveint);
			}

			var rootRegion = parentRegions.FirstOrDefault(item => item.ParentKey == null);
			if (rootRegion != null)
				BuildPermisionTree(rootRegion, parentRegions, sitePermission, regionUsers, context);
			return rootRegion;
		}

		private List<CMSWebSiteModel> GetPermissionPacSites(UserContext context, List<int> kdvrHasPacList, int keepaliveint)
		{
			var includes = new string[]
			{
				typeof (tCMSWebSites).Name,
				string.Format("{0}.{1}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name)
			};

			var getpermisionRegion = UsersService.Get(context.ID, t => t, includes);

			var sitePermission = getpermisionRegion.tCMSWebSites
				.Where(ws => ws.tDVRChannels.FirstOrDefault(t => kdvrHasPacList.Contains(t.KDVR)) != null)
				.Select(SelectorSiteModelFunc(false, false, keepaliveint)).ToList();

			var includesdvr = new string[]
			{
				typeof (tDVRChannels).Name,
				string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
			};

			var userSites = DataService.GetSites<tCMSWebSites>(t => t.UserID == context.ID, t => t, includesdvr).ToList();
			var usersofSite = userSites
				.Where(ws => ws.tDVRChannels.FirstOrDefault(t => kdvrHasPacList.Contains(t.KDVR)) != null)
				.Select(SelectorSiteModelFunc(false, false, keepaliveint)).ToList();

			sitePermission.AddRange(usersofSite);

			return sitePermission;
		}

		private List<CMSWebSiteModel> GetRegionsToModel(int userKey, List<int> userIds = null)
		{
			var regionQuery = DataService.GetRegions<CMSWebSiteModel>(t => new CMSWebSiteModel()
			{
				ID = t.RegionKey,
				Name = t.RegionName,
				ParentKey = t.RegionParentID,
				Type = SiteType.REGION,
				UserID = t.UserKey
			}, null);

			if (userIds != null)
			{
				regionQuery = regionQuery.Where(t => t.UserID != null && userIds.Contains((int) t.UserID));
				//regionQuery = regionQuery.Where(t => t.UserID != null && (userIds.Contains((int) t.UserID) || t.UserID == userKey)); //Include sub region added by current user
			}
			else
			{
				regionQuery = regionQuery.Where(t => t.UserID == userKey);
			}

			return regionQuery.ToList();
		}

		private List<CMSWebSiteModel> GetMultiUserPermissionSites(UserContext context, List<int> userIds, bool hasMac,
			bool hasChannel, int keepaliveint)
		{
			var includesdvr = new string[]
			{
				typeof (tDVRChannels).Name,
				string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
			};

			var userSites =
				DataService.GetSites<tCMSWebSites>(t => t.UserID != null && userIds.Contains((int) t.UserID), t => t, includesdvr)
					.ToList();
			return userSites.Select(SelectorSiteModelFunc(hasMac, hasChannel, keepaliveint)).ToList();
		}

		private List<CMSWebSiteModel> GetPermissionSites(UserContext context, bool hasMac, bool hasChannel, int keepaliveint)
		{
			var includes = new string[]
			{
				typeof (tCMSWebSites).Name,
				string.Format("{0}.{1}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name),
				string.Format("{0}.{1}.{2}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
			};

			var getpermisionRegion = UsersService.Get(context.ID, t => t, includes);
			var sitePermission =
				getpermisionRegion.tCMSWebSites.Select(SelectorSiteModelFunc(hasMac, hasChannel, keepaliveint)).ToList();

			var includesdvr = new string[]
			{
				typeof (tDVRChannels).Name,
				string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
			};
			var userSites = DataService.GetSites<tCMSWebSites>(t => t.UserID == context.ID, t => t, includesdvr).ToList();
			var usersofSite = userSites.Select(SelectorSiteModelFunc(hasMac, hasChannel, keepaliveint)).ToList();
			sitePermission.AddRange(usersofSite);
			return sitePermission;
		}

		private Func<tCMSWebSites, CMSWebSiteModel> SelectorSiteModelFunc(bool hasMac, bool hasChannel, int keepaliveint)
		{
			return ws => new CMSWebSiteModel()
			{
				ID = ws.siteKey,
				Name = ws.ServerID,
				ParentKey = ws.RegionKey,
				Type = SiteType.SITE,
				UserID = ws.UserID,
				Sites = hasMac ? GetKdvrList(ws, hasChannel, keepaliveint) : new List<CMSWebSiteModel>()
			};
		}

		private bool BuildPermisionTree(CMSWebSiteModel parent, List<CMSWebSiteModel> regions,
			List<CMSWebSiteModel> sitePermission, List<CMSWebSiteModel> regionUsers, UserContext user,
			bool includeEmptyRegion = true)
		{
			bool isChild = false;
			int pkey = parent.ID;
			var childregions = regions.Where(item => item.ParentKey == pkey && item.Type == SiteType.REGION).OrderBy(x => x.Name).ToList();
			foreach (CMSWebSiteModel child in childregions)
			{
				var haschild = BuildPermisionTree(child, regions, sitePermission, regionUsers, user, includeEmptyRegion);
				if (haschild || (user.ID == user.ParentID && includeEmptyRegion))
				{
					parent.Sites.Add(child);
					haschild = true;
				}
				isChild |= haschild;
			}

			var regionusers = regionUsers.Where(t => t.ParentKey == pkey).OrderBy(x => x.Name).ToList();
			if (regionusers.Count > 0)
			{
				regionusers.ForEach((t) =>
				{
					if (BuildTree(t, regionUsers, sitePermission, user, includeEmptyRegion))
					{
						parent.Sites.Add(t);
						isChild = true;
					}
				});
			}

			var sites = sitePermission.Where(t => t.ParentKey == pkey).OrderBy(x => x.Name).ToList();
			if (sites.Count > 0)
			{
				parent.Sites.AddRange(sites);
				isChild = true;
			}

			return isChild;
		}

		public List<CMSWebSiteModel> GetKdvrList(tCMSWebSites ws, bool hasChannels, int keepaliveint, bool customname = false)
		{
			int extraKeepAlive = (keepaliveint*3)/2;
			Int64 curTime = DateTime.Now.FullDateTimeToUnixTimestamp();
			return ws.tDVRChannels.Select(dvr => new CMSWebSiteModel()
			{
				ID = dvr.KDVR,
				Name = customname? ws.ServerID:dvr.tDVRAddressBook.ServerID,
				MACAddress = dvr.tDVRAddressBook.DVRGuid,
                HaspKey = dvr.tDVRAddressBook.HaspLicense,
				ServerID = dvr.tDVRAddressBook.ServerID,
                IsVirtual = dvr.tDVRAddressBook.ServerIP == null && dvr.tDVRAddressBook.PublicServerIP == null ? true : false,
				Checked = null,
				ParentKey = ws.siteKey,
				Type = SiteType.DVR,
				UserID = ws.UserID,
				OnlineStatus = GetStatusOfDVR(dvr.tDVRAddressBook, extraKeepAlive, curTime),
				Sites = hasChannels == true
						? ws.tDVRChannels.Where(t => t.KDVR == dvr.KDVR).OrderBy(o => o.KChannel).Select(t => new CMSWebSiteModel()
						{
							ID = t.KChannel,
							Name = t.Name,
							Checked = null,
							ParentKey = dvr.KDVR,
							Type = SiteType.CHANNEL,
							Status = t.Status,
							UserID = ws.UserID,
                            ServerID = (t.ChannelNo+1).ToString()
						}).ToList()
						: new List<CMSWebSiteModel>()
			}).GroupBy(x => new {x.ID}).Select(g => g.First()).ToList();
		}

		private int? GetStatusOfDVR(tDVRAddressBook dvr, int extraKeepAlive, Int64 curTime)
		{

			if (string.IsNullOrEmpty(dvr.DVRGuid))
			{
				return (int) OnlineStatus.OFFLINE_SCHEDULE;
			}
			dvr.tDVRKeepAlife = DataService.GetDVRInfo<tDVRKeepAlives>(dvr.KDVR, x => x.tDVRKeepAlife, null).FirstOrDefault();
			var status = (
				dvr.Online == (int) OnlineStatus.BLOCKED
				|| (dvr.Online == (int) OnlineStatus.OFFLINE && dvr.CMSMode == Consts.CMS_MODE_STANDARD)
				|| !dvr.CMSMode.HasValue
				|| dvr.CMSMode == 0)
				? dvr.Online
				: ((curTime - (dvr.tDVRKeepAlife == null ? 0 : dvr.tDVRKeepAlife.LastAccess))/60 <= extraKeepAlive)
				  && (dvr.CMSMode == Consts.CMS_MODE_STANDARD || dvr.Online == (int) OnlineStatus.ONLINE)
					? (int) OnlineStatus.ONLINE
					: ((curTime - (dvr.tDVRKeepAlife == null ? 0 : dvr.tDVRKeepAlife.LastAccess))/60 <= ((1440/dvr.CMSMode) + extraKeepAlive))
					  && dvr.CMSMode < Consts.CMS_MODE_STANDARD
						? (int) OnlineStatus.OFFLINE_SCHEDULE
						: (int) OnlineStatus.OFFLINE;

			return status;

		}

		private bool BuildTree(CMSWebSiteModel parent, List<CMSWebSiteModel> regions, List<CMSWebSiteModel> sitePermission, UserContext user, bool allowEmptyRegion)
		{
			int pkey = parent.ID;
			var hasData = false;
			var childregions = regions.Where(item => item.ParentKey == pkey && item.Type == SiteType.REGION).OrderBy(x => x.Name).ToList();
			foreach (CMSWebSiteModel child in childregions)
			{
				var hasSite = BuildTree(child, regions, sitePermission, user, allowEmptyRegion);
				if (hasSite || (child.UserID == user.ID && allowEmptyRegion))
				{
					parent.Sites.Add(child);
					hasSite = true;
				}
				hasData |= hasSite;
			}
			var sites = sitePermission.Where(t => t.ParentKey == pkey).OrderBy(x => x.Name).ToList();
			if (sites.Count > 0)
			{
				parent.Sites.AddRange(sites);
				hasData = true;
			}

			if (parent.UserID == user.ID && allowEmptyRegion)
			{
				hasData = true;
			}
			return hasData;
		}

		public CMSWebSiteModel GetSiteById(UserContext userContext, int siteId, bool hasChannels, int keepaliveint)
		{
			var includes = new string[]
				{
					 typeof (tDVRChannels).Name,
					string.Format("{0}.{1}",typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
				};

			var website = DataService.GetSite<tCMSWebSites>(siteId, t => t, includes);

			var site = new CMSWebSiteModel()
			{
				ID = website.siteKey,
				Name = website.ServerID,
				Checked = null,
				ParentKey = website.RegionKey,
				Type = SiteType.SITE,
				UserID = website.UserID,
				Sites = GetKdvrList(website, hasChannels, keepaliveint)
			};

			return site;
		}
		
		private CMSWebSiteModel SiteModel(IQueryable<CMSWebSiteModel> regions, IQueryable<CMSWebSiteModel> sites)
		{
			CMSWebSiteModel model = regions.FirstOrDefault(item => item.ParentKey.HasValue == false || item.ParentKey.Value == 0);
			if (model == null)
				return null;
			UpdateRegionrecur(model, regions, sites);
			return model;
		}

		private void UpdateRegionrecur(CMSWebSiteModel parent, IQueryable<CMSWebSiteModel> regions, IQueryable<CMSWebSiteModel> sites)
		{
			int pkey = parent.ID;
			var childsites = sites.Where(item => item.ParentKey == pkey).ToArray();
			var childregions = regions.Where(item => item.ParentKey == pkey).ToArray();
			parent.Sites = childsites.Concat(childregions).ToList();
			foreach (CMSWebSiteModel child in childregions)
			{
				UpdateRegionrecur(child, regions, sites);
			}

		}

		public ICollection<TreeMetric> GetTreeMetrics(UserContext context)
		{
			var metricTreeNode = MetricService.GetMetrics<TreeMetric>(context, true, dbMetric2model, null).ToList();

			foreach (var metric in metricTreeNode)
			{
				BuildTreeMetric(metric, context);
				metric.MetricType = MetricType.GROUP;
			}

			return metricTreeNode;
		}

		private void BuildTreeMetric(TreeMetric treeMetric, UserContext context)
		{
			var childMetric = MetricService.SelectMetricChild(treeMetric.Id, dbMetric2model, null).ToList();

			foreach (var metric in childMetric)
			{
				BuildTreeMetric(metric, context);
				metric.MetricType = MetricType.METRIC;
			}
			treeMetric.Childs = childMetric;
		}

		public void SaveTreeNode(CMSWebSiteModel treeNode, UserContext context)
		{
			if (treeNode.Type == 0)
			{
				tCMSWebRegion region = DataService.GetRegions<tCMSWebRegion>(item => item, null).FirstOrDefault(r => r.RegionKey == treeNode.ID);//context.ID, 
					//DataService.GetRegions(context).First(t => t.RegionKey == treeNode.ID);
				if (region == null)
				{
					throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), treeNode);
				}
				//Anh, don't check privilege
				//if (context.ID != treeNode.UserID)
				//{
				//	throw new CmsErrorException(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
				//}

				region.RegionParentID = treeNode.ParentKey == 0 ? null : treeNode.ParentKey;

				if (DataService.UpdateRegion(region) < 0)
				{
					throw new CmsErrorException(CMSWebError.SERVER_ERROR_MSG.ToString(), treeNode);
				}
			}
			else
			{
				tCMSWebSites site =DataService.GetSites<tCMSWebSites>(item => item, null).FirstOrDefault(si => si.siteKey == treeNode.ID);//context.ID, context.ID == context.ParentID, 
					//DataService.GetSites(context).First(t => t.siteKey == treeNode.ID);
				if (site == null)
				{
					throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), treeNode);
				}

				//Anh, don't check privilege
				//if (context.ID != site.UserID)
				//{
				//	throw new CmsErrorException(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
				//}

				site.RegionKey = treeNode.ParentKey;
				if (DataService.UpdateSite(site) < 0)
				{
					throw new CmsErrorException(CMSWebError.SERVER_ERROR_MSG.ToString(), treeNode);
				}
			}
		}

		public TransactionalModel<RegionModel> AddRegion(UserContext context, RegionModel region)
		{
			TransactionalModel<RegionModel> response = new TransactionalModel<RegionModel>();
			if (region == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}
			//var checkRegionNameExisted = DataService.GetRegions(t => t, null)
			//	.FirstOrDefault(t => (t.UserKey == context.ID || t.UserKey == context.ParentID) && t.RegionName.ToLower().Equals(region.RegionName.ToLower()));
			var checkRegionNameExisted = DataService.GetRegions(t => t, null)
				.FirstOrDefault(t => t.RegionName.ToLower().Equals(region.RegionName.ToLower()));

			if (checkRegionNameExisted != null)
			{
				//throw new CmsErrorException(CMSWebError.REGION_NAME_EXIST.ToString());
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.REGION_NAME_EXIST.ToString());
				return response;
			}

			var rules = new SitesBusinessRules(Culture);
			var dbregion = new tCMSWebRegion();
			var newReion = rules.ToDbRegion(dbregion, region);
			newReion.UserKey = context.ID;
			var newregionKey = DataService.AddRegion(newReion);
			if (newregionKey < 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			region.RegionKey = newregionKey;
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			response.Data = region;
			return response;
		}

		public TransactionalModel<RegionModel> UpdateRegion(UserContext context, RegionModel region)
		{
			TransactionalModel<RegionModel> responseData = new TransactionalModel<RegionModel>();
			if (region == null)
			{
				//throw new CmsErrorException(CMSWebError.PARAM_WRONG.ToString(), SiteConts.REGION_CANNOT_NULL);
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responseData;
			}

			var dbregion = DataService.GetRegions<tCMSWebRegion>(item => item, null).FirstOrDefault(node => node.RegionKey == region.RegionKey);//context.ID,
			//Anh Huynh, Master & admin user always have edit privilege
			//if (context.ParentID != context.ID && dbregion == null)
			//{
			//	var checkRegionExisted = DataService.GetRegions<tCMSWebRegion>(context.ParentID, item => item, null).FirstOrDefault(node => node.RegionKey == region.RegionKey);
			//	if (checkRegionExisted != null)
			//	{
			//		//throw new CmsErrorException(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString(), region);
			//		responseData.ReturnStatus = false;
			//		responseData.ReturnMessage.Add(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
			//		return responseData;
			//	}
			//}

			if (dbregion == null)
			{
				//throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), region);
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responseData;
			}

			//var checkRegionNameExisted = DataService.GetRegions(t => t, null)
			//	.FirstOrDefault(t => (t.UserKey == context.ID || t.UserKey == context.ParentID) && t.RegionName.ToLower().Equals(region.RegionName.ToLower()));
			var checkRegionNameExisted = DataService.GetRegions(t => t, null)
				.FirstOrDefault(t => t.RegionName.ToLower().Equals(region.RegionName.ToLower()));
			if (checkRegionNameExisted != null && checkRegionNameExisted.RegionKey != region.RegionKey)
			{
				//throw new CmsErrorException(CMSWebError.REGION_NAME_EXIST.ToString());
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.REGION_NAME_EXIST.ToString());
				return responseData;
			}

			var rules = new SitesBusinessRules(Culture);
			var updateReion = rules.ToDbRegion(dbregion, region);
			//updateReion.UserKey = context.ID;

			if (DataService.UpdateRegion(updateReion) < 0)
			{
				//throw new CmsErrorException(CMSWebError.SERVER_ERROR.ToString(), region, SiteConts.REGION_UPDATE_ERROR);
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responseData;
			}

			responseData.ReturnStatus = true;
			responseData.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			responseData.Data = region;
			return responseData;
		}

		public TransactionalModel<RegionModel> DeleteMultiRegion(UserContext context, CMSWebSiteModel regions)
		{
			TransactionalModel<RegionModel> response = new TransactionalModel<RegionModel>();
			if (regions.Type != SiteType.REGION)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			return CheckDeleteMultiRegion(context, regions);
		}

		public TransactionalModel<RegionModel> DeleteRegion(UserContext context, int regionKey)
		{
			TransactionalModel<RegionModel> response = new TransactionalModel<RegionModel>();
			if (regionKey == 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			response = ExecuteDeleteRegion(context, regionKey);
			return response;
		}

		private TransactionalModel<RegionModel> CheckDeleteMultiRegion(UserContext context, CMSWebSiteModel regions)
		{
			regions.Sites.ForEach(t =>
			{
				if (t.Type == SiteType.REGION)
				{
					CheckDeleteMultiRegion(context, t);
				}
			});

			return ExecuteDeleteRegion(context, regions.ID);
		}

		private TransactionalModel<RegionModel> ExecuteDeleteRegion(UserContext context, int regionKey)
		{
			TransactionalModel<RegionModel> response = new TransactionalModel<RegionModel>();
			var dbregion =DataService.GetRegions<tCMSWebRegion>(context.ID, item => item, new[] {typeof (tCMSWebSites).Name}).FirstOrDefault(node => node.RegionKey == regionKey);
			var dbsubRegion = DataService.GetRegions<tCMSWebRegion>(item => item, null).FirstOrDefault(s => s.RegionParentID.HasValue && s.RegionParentID.Value == regionKey);
			if (dbregion == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}
			else if(!dbregion.RegionParentID.HasValue)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.UNABLE_TO_DELETE_ROOT_TREE.ToString());
				return response;
			}
			else if (dbsubRegion != null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.UNABLED_DELETE_REGION_EXIST_SUB_REGION.ToString());
				return response;
			}
			else if (dbregion.tCMSWebSites != null && dbregion.tCMSWebSites.Any())
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.UNABLED_DELETE_REGION_EXIST_SITE.ToString());
				return response;
			}

			if (context.ParentID != context.ID)
			{//If User login is not Master Admin
				var checkRegionExisted =DataService.GetRegions<tCMSWebRegion>(context.ParentID, item => item, null).FirstOrDefault(node => node.RegionKey == regionKey);
				if (checkRegionExisted != null)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
					return response;
				}
				}

			if (!DataService.DeleteRegion(dbregion))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return response;
		}

		public RegionModel GetRegion(UserContext context, int regionKey)
		{
			var rules = new SitesBusinessRules(Culture);

			IQueryable<tCMSWebRegion> regions = DataService.GetRegions<tCMSWebRegion>(context.ID, item => item, null);
			var region = regions.Where(item => item.RegionKey == regionKey).Select(rules.SelectDbRegionToModel).FirstOrDefault();

			if (context.ParentID != context.ID && region == null)
			{
				var regionadmin = DataService.GetRegions<RegionModel>(context.ParentID, rules.SelectDbRegionToModel, null).FirstOrDefault(node => node.RegionKey == regionKey);
				if (regionadmin == null)
				{
					throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), regionKey);
				}
				return regionadmin;
			}

			if (region == null)
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
			}
			return region;
		}

		public CmsSites GetSites(UserContext context, int siteKey)
		{
			var rules = new SitesBusinessRules(Culture);
			var includes = IncludeEntityRelatedSite();
			CmsSites cmssite = DataService.GetSite<CmsSites>(siteKey, rules.WebSiteToModel, includes);

			if (cmssite == null)
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
			}

			var includePers = new string[]
			{
				typeof (tCMSWebSites).Name
			};
			var getpermisionRegion = UsersService.Get(context.ID, t => t, includePers);
			var sitePermission = getpermisionRegion.tCMSWebSites.FirstOrDefault(t => t.siteKey == cmssite.SiteKey);

			if (sitePermission == null && cmssite.UserId != context.ID && context.ID != context.ParentID)
			{
				throw new CmsErrorException(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString(), siteKey);
			}

			var metricSites = DataService.GetMetricSites<tCMSWeb_MetricSiteList>(siteKey, item => item, null).Select(node => node.MListID);

			cmssite.Macs = cmssite.Macs.GroupBy(t => t.Id).Select(t => t.First()).ToList();
			cmssite.DvrMetrics = metricSites; //.ToList();
			cmssite.HaspLicense = cmssite.HaspLicense.GroupBy(g => g.KDVR).Select(s => s.First()).ToList();

			return cmssite;
		}

		private static string[] IncludeEntityRelatedSite()
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_UserList).Name,
				typeof (tCMSWeb_CalendarEvents).Name,
				typeof (tCMSWeb_WorkingHours).Name,
				typeof (tDVRChannels).Name,
				typeof (tbl_ZipCode).Name,
				string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name)
			};
			return includes;
		}

		//public async Task<CmsSites> AddSite(UserContext context, CmsSites site)
		//{
		//	if (site == null)
		//	{
		//		throw new CmsErrorException(CMSWebError.PARAM_WRONG.ToString());
		//	}
		//	bool is_admin = userLogin.ID == userLogin.ParentID;
		//	IEnumerable<string> include_dvr = ServiceBase.ChildProperty(typeof(tCMSWebSites), typeof(tDVRChannels), typeof(tDVRAddressBook));
		//	string[] includes = { string.Join(".", include_dvr) };
		//	var checkSiteNameExisted = DataService.GetSites(userLogin.ID, true, t => t, includes, null).FirstOrDefault(t => (t.UserID == userLogin.ID || t.UserID == userLogin.ParentID) && t.ServerID.ToLower().Equals(site.ServerId.ToLower()));
		//	bool is_update = false;
		//	if (checkSiteNameExisted != null)
		//	{
		//		if (!checkSiteNameExisted.Deleted.HasValue || checkSiteNameExisted.Deleted.Value == false)
		//			throw new CmsErrorException(CMSWebError.SITE_NAME_EXIST.ToString());
		//		is_update = true;
		//	}

		//	var sitedb = new tCMSWebSites();
			//if( is_update)
			//{
			//	sitedb = checkSiteNameExisted;
		//		site.SiteKey = sitedb.siteKey;
			//}

		//	await ModifyCmsSite(userLogin, site, sitedb);
		//	return site;

		//	//var rul = new SitesBusinessRules(base.Culture);

		//	//sitedb = new tCMSWebSites();
		//	//if( is_update)
		//	//{
		//	//	ServiceBase.Includes<tCMSWebSites, tCMSWeb_CalendarEvents>(checkSiteNameExisted, it => it.tCMSWeb_CalendarEvents);
		//	//	ServiceBase.Includes<tCMSWebSites, tCMSWeb_UserList>(checkSiteNameExisted, it => it.tCMSWeb_UserList);
		//	//	ServiceBase.Includes<tCMSWebSites, tDVRChannels>(checkSiteNameExisted, it => it.tDVRChannels);
			
		//	//	sitedb = checkSiteNameExisted;
		//	//}

		//	//site = CorrectMacAdress(site);

		//	//var metricList = getMetricSiteList(site);
			
		//	//rul.ModelToWebSite(ref sitedb, site, metricList);

		//	//List<tCMSWeb_CalendarEvents> newCalendars =
		//	//	CalendarService.CalendarEvents<tCMSWeb_CalendarEvents>(context.ID, t => t, null)
		//	//		.Where(t => site.CalendarEvent.Contains(t.ECalID))
		//	//		.ToList();
		//	//List<tCMSWeb_CalendarEvents> oldCalendars = sitedb.tCMSWeb_CalendarEvents.ToList();
		//	//CheckCalendarEventFromSite(oldCalendars, newCalendars, sitedb);
			


		//	//List<tCMSWeb_UserList> newUsers = UsersService.GetListUser(t => site.DvrUsers.Contains(t.UserID)).ToList();
		//	//List<tCMSWeb_UserList> oldUsers = sitedb.tCMSWeb_UserList.ToList();
		//	//CheckUsersFromSite(oldUsers, newUsers, sitedb);

		//	//var newChannels = new List<tDVRChannels>();

		//	//List<tDVRChannels> newRelateChannels = CheckMacAndAddToSite(site, newChannels);
		//	//List<tDVRChannels> oldChannels = sitedb.tDVRChannels.ToList();
		//	//CheckChanelsFromSite(oldChannels, newRelateChannels, sitedb);
		//	//newChannels.ForEach(c => sitedb.tDVRChannels.Add(c));

		//	//sitedb.UserID = context.ID;
		//	//if( ! is_update)
		//	//	DataService.AddSite(sitedb);
		//	//else
		//	//	DataService.UpdateSite( sitedb);

		//	//site.SiteKey = sitedb.siteKey;

		//	//string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitedb.siteKey.ToString());

		//	//_fileManager = new FilesManager.FilesManager();
		//	//_fileManager.SetWorkingFolder(path);
		//	//if (site.ImageSiteBytes != null && site.ImageSiteBytes.Length > 0)
		//	//{
		//	//	_fileManager.Add(site.ImageSite, site.ImageSiteBytes);
		//	//}

		//	//if (site.ActualBudgetBytes != null && site.ActualBudgetBytes.Length > 0)
		//	//{
		//	//	_fileManager.Add(site.ActualBudget, site.ActualBudgetBytes);
		//	//}

		//	//if (site.FixtureBytes != null && site.FixtureBytes.Length > 0)
		//	//{
		//	//	_fileManager.Add(site.FixturePlan, site.FixtureBytes);
		//	//}

		//	//site.Macs.ToList().ForEach(mac =>
		//	//{
		//	//	if (mac.Files.Count <= 0) return;
		//	//	var macpath = Path.Combine(path, mac.MacAddress);
		//	//	_fileManager.SetWorkingFolder(macpath);
		//	//	mac.Files.ToList().ForEach(f => _fileManager.Add(f.Name, f.Data));
		//	//});

		//	//return site;
		//}

		private CmsSites CorrectMacAdress(CmsSites site)
		{
			const int chunkSize = 2;
			site.Macs.ToList().ForEach(mac =>
			{
				if (mac.MacAddress.Length == Consts.MAX_MAC_LENGTH)
				{
					IEnumerable<string> chuckMac =
						System.Linq.Enumerable.Range(0, mac.MacAddress.Length/chunkSize)
							.Select(i => mac.MacAddress.ToUpper().Substring(i*chunkSize, chunkSize));
					mac.MacAddress = string.Join("-", chuckMac);
				}
				else
				{
					mac.MacAddress = mac.MacAddress.ToUpper();
				}
			});

			return site;
		}

		private tDVRChannels CreateVirtualChannel()
		{
			tDVRChannels tchanel = new tDVRChannels()
			{
				ChannelNo = 0,
				KAudioSource = 0,
				KPTZ = 0,
				Name = Consts.Virtualchanel,
			};
			return tchanel;
		}

		private tDVRAddressBook CreateVirtualDVR( string dvrgui)
		{
			if( string.IsNullOrEmpty( dvrgui))
				return null;
			return new tDVRAddressBook
			{
					DVRGuid = dvrgui,
				ServerID = Consts.VirtualDVR,
					Online = 0,
					CMSMode = Consts.CMS_MODE_STANDARD,
					tDVRChannels = { CreateVirtualChannel()}
			};
		}

		private tDVRAddressBook CreateVirtualDVRBySerialNumber(string serialNumber)
		{
			if (string.IsNullOrEmpty(serialNumber))
				return null;
			return new tDVRAddressBook
			{
				HaspLicense = serialNumber,
				DVRGuid = string.Empty,
				ServerID = Consts.VirtualDVR,
				Online = 0,
				CMSMode = Consts.CMS_MODE_STANDARD,
				tDVRChannels = { CreateVirtualChannel() }
			};
		}

		private CMSWebError AddDVRFromSitemodel( CmsSites site, IDVRService DVRService, IDvrChanelService DvrChanelService,  out List<tDVRAddressBook> kdvrs)
		{
			CMSWebError ret = CMSWebError.OK;
			kdvrs = new List<tDVRAddressBook>();
			List<string>includes = new List<string>();
			includes.Add(ServiceBase.ChildProperty<tDVRAddressBook,tDVRChannels>());
			IEnumerable<string> arr_includes = ServiceBase.ChildProperty(typeof(tDVRAddressBook), typeof(tDVRChannels), typeof (tCMSWebSites));
			string str_include = string.Join(".", arr_includes);
			includes.Add(str_include);
			IEnumerable<string>macs = site.Macs.Select( it => it.MacAddress.ToUpper());
			IEnumerable<tDVRAddressBook> validDVRs = DvrChanelService.GetDvrAddressBookByMacs<tDVRAddressBook>(macs, it => it, includes.ToArray()).ToList();
			tDVRAddressBook dvr = null;
			bool dvrinused = false;

			foreach (string mac in macs)
			{
				dvr = validDVRs.FirstOrDefault( it => string.Compare(it.DVRGuid, mac, true)== 0);
				if(dvr == null)
				{
					dvr = CreateVirtualDVR(mac);
					DVRService.Add( dvr, false);
					kdvrs.Add(dvr);
					continue;
				}
				//Nghi add Aug, 21 2015 begin
				//When converter only convert POS data then server id & channel is empty => need to update/add virtual name
				if(string.IsNullOrEmpty(dvr.ServerID))
				{
					dvr.ServerID = Consts.VirtualDVR;
					DVRService.Update(dvr);

				}
				if(!dvr.tDVRChannels.Any())
				{
					tDVRChannels chan = CreateVirtualChannel();
					chan.KDVR = dvr.KDVR;
					DvrChanelService.AddDVRChannel(chan); 
				}
				//Nghi add Aug, 21 2015 end
				if( site.SiteKey == 0)
					dvrinused = dvr.tDVRChannels.Any( it => it.tCMSWebSites.Any());
				else
					dvrinused = dvr.tDVRChannels.Any(it => it.tCMSWebSites.FirstOrDefault(sit => sit.siteKey != site.SiteKey) != null);
			
				kdvrs.Add(dvr);
				if( dvrinused)
				{
					ret = CMSWebError.MAC_EXIST_MSG;
					break;
				}
				
				
			}
			
			return ret;
		}

        private bool CheckLiscene()
        {
            var tdvrAddressBook =  DataService.GetAllHaspLicense<tDVRAddressBook>(selector=>selector,null);
            if (tdvrAddressBook.Any())
            {
                int count = tdvrAddressBook.Count();
                return AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber > count;
            }
            return true;
        }

		private CMSWebError AddHaspLicenseFromSitemodel(CmsSites site, IDVRService DVRService, IDvrChanelService DvrChanelService, out List<tDVRAddressBook> kdvrs)
		{
			CMSWebError ret = CMSWebError.OK;
			kdvrs = new List<tDVRAddressBook>();
			List<string> includes = new List<string>();
			includes.Add(ServiceBase.ChildProperty<tDVRAddressBook, tDVRChannels>());
			IEnumerable<string> arr_includes = ServiceBase.ChildProperty(typeof(tDVRAddressBook), typeof(tDVRChannels), typeof(tCMSWebSites));
			string str_include = string.Join(".", arr_includes);
			includes.Add(str_include);
			IEnumerable<string> hasps = site.HaspLicense.Select(it => it.SerialNumber);
			IEnumerable<tDVRAddressBook> validDVRs = DvrChanelService.GetDvrAddressBookByHaspLicense<tDVRAddressBook>(hasps, it => it, includes.ToArray()).ToList();
			tDVRAddressBook dvr = null;
			bool dvrinused = false;

			foreach (string hasp in hasps)
			{
                bool AvailbletoAdd = CheckLiscene();
				dvr = validDVRs.FirstOrDefault(it => string.Compare(it.HaspLicense, hasp, true) == 0);
				if (dvr == null)
				{
					
                    if (AvailbletoAdd)
                    {
					    dvr = CreateVirtualDVRBySerialNumber(hasp);
					    DVRService.Add(dvr, false);
					    kdvrs.Add(dvr);
					    continue;
				    }
                    else
                    {
                        ret = CMSWebError.EXCEED_LICENSE;
                        return ret;
                    }
				}
				
				if (string.IsNullOrEmpty(dvr.ServerID))
				{
					dvr.ServerID = Consts.VirtualDVR;
					DVRService.Update(dvr);

				}
				if (!dvr.tDVRChannels.Any())
				{
					tDVRChannels chan = CreateVirtualChannel();
					chan.KDVR = dvr.KDVR;
					DvrChanelService.AddDVRChannel(chan);
				}
				
				if (site.SiteKey == 0)
					dvrinused = dvr.tDVRChannels.Any(it => it.tCMSWebSites.Any());
				else
					dvrinused = dvr.tDVRChannels.Any(it => it.tCMSWebSites.FirstOrDefault(sit => sit.siteKey != site.SiteKey) != null);

				kdvrs.Add(dvr);
				if (dvrinused)
				{
					ret = CMSWebError.HASP_LICENSE_EXIST_MSG;
					break;
				}
			}

			return ret;
		}
		
		//public async Task<CmsSites> EditSite(UserContext context, CmsSites site)
		//{
		//	if (site == null)
		//	{
		//		throw new CmsErrorException(CMSWebError.PARAM_WRONG.ToString());
		//	}

		//	bool is_admin = context.ID == context.ParentID;
		//	var checkSiteNameExisted = DataService.GetSites(context.ID, true, t => t, null).FirstOrDefault(t => (t.UserID == context.ID || t.UserID == context.ParentID) && t.ServerID.ToLower().Equals(site.ServerId.ToLower()));
		//	if (checkSiteNameExisted != null && checkSiteNameExisted.siteKey != site.SiteKey)
		//	{
		//		throw new CmsErrorException(CMSWebError.SITE_NAME_EXIST.ToString());
		//	}

		//	var checkPermision = DataService.GetSites(context.ParentID, is_admin, t => t, null).FirstOrDefault(t => t.siteKey == site.SiteKey);
		//	if (checkPermision != null && checkPermision.UserID != context.ID)
		//	{
		//		throw new CmsErrorException(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString(), site);
		//	}


		//	site = CorrectMacAdress(site);

		//	var includes = IncludeEntityRelatedSite();
		//	var sitedb = DataService.GetSites(context.ID, site.SiteKey, includes);

		//	if (sitedb == null)
		//	{
		//		throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
		//	}
		//	await ModifyCmsSite(context, site, sitedb);
		//	return site;

		//	#region old code
		//	/*
		//	string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitedb.siteKey.ToString());

		//	_fileManager = new FilesManager.FilesManager();
		//	_fileManager.SetWorkingFolder(path);
		//	if (site.ImageSiteBytes != null && site.ImageSiteBytes.Length > 0)
		//	{
		//		if (!string.IsNullOrEmpty(sitedb.ImageSite))
		//		{
		//			_fileManager.DeleteFile(sitedb.ImageSite);
		//		}
		//		_fileManager.Add(site.ImageSite, site.ImageSiteBytes);
		//	}

		//	if (site.ActualBudgetBytes != null && site.ActualBudgetBytes.Length > 0)
		//	{
		//		if (!string.IsNullOrEmpty(sitedb.ActualBudget))
		//		{
		//			_fileManager.DeleteFile(sitedb.ActualBudget);
		//		}
		//		_fileManager.Add(site.ActualBudget, site.ActualBudgetBytes);
		//	}

		//	if (site.FixtureBytes != null && site.FixtureBytes.Length > 0)
		//	{
		//		if (!string.IsNullOrEmpty(sitedb.FixturePlan))
		//		{
		//			_fileManager.DeleteFile(sitedb.FixturePlan);
		//		}
		//		_fileManager.Add(site.FixturePlan, site.FixtureBytes);
		//	}

		//	sitedb.tCMSWeb_MetricSiteList =
		//		DataService.GetMetricSites<tCMSWeb_MetricSiteList>(site.SiteKey, item => item, null).ToList();

		//	var rul = new SitesBusinessRules(base.Culture);

		//	var metricList = getMetricSiteList(site);

		//	DataService.DeleteMetricSite(sitedb.siteKey);
		//	DataService.DeleteWorkingHour(sitedb.siteKey);

		//	List<tCMSWeb_CalendarEvents> newCalendars =
		//		CalendarService.CalendarEvents<tCMSWeb_CalendarEvents>(context.ID, t => t, null)
		//			.Where(t => site.CalendarEvent.Contains(t.ECalID))
		//			.ToList();
		//	List<tCMSWeb_CalendarEvents> oldCalendars = sitedb.tCMSWeb_CalendarEvents.ToList();
		//	CheckCalendarEventFromSite(oldCalendars, newCalendars, sitedb);

		//	List<tCMSWeb_UserList> newUsers = UsersService.GetListUser(t => site.DvrUsers.Contains(t.UserID)).ToList();
		//	List<tCMSWeb_UserList> oldUsers = sitedb.tCMSWeb_UserList.ToList();
		//	CheckUsersFromSite(oldUsers, newUsers, sitedb);

		//	var newChannels = new List<tDVRChannels>();

		//	List<tDVRChannels> newRelateChannels = CheckMacAndAddToSite(site, newChannels);
		//	List<tDVRChannels> oldChannels = sitedb.tDVRChannels.ToList();
		//	CheckChanelsFromSite(oldChannels, newRelateChannels, sitedb);
		//	newChannels.ForEach(c => sitedb.tDVRChannels.Add(c));
		//	var deleteMac = SearchDeletetDVRChannels(oldChannels, newRelateChannels);
		//	rul.ModelToWebSite(ref sitedb, site, metricList);
		//	sitedb.UserID = context.ID;
		//	DataService.UpdateSite(sitedb);

		//	if (DataService.Save() < 0)
		//	{
		//		throw new CmsErrorException(CMSWebError.SERVER_ERROR.ToString());
		//	}

		//	if (deleteMac.Length > 0)
		//	{
		//		foreach (var mac in deleteMac.Where(mac => mac.tDVRAddressBook.DVRGuid != null))
		//		{
		//			_fileManager.SetWorkingFolder(path);
		//			_fileManager.DeleteFolder(mac.tDVRAddressBook.DVRGuid);
		//		}
		//	}

		//	site.Macs.ToList().ForEach(mac =>
		//	{
		//		if (mac.Files.Count <= 0) return;
		//		var macpath = Path.Combine(path, mac.MacAddress);
		//		_fileManager.SetWorkingFolder(macpath);

		//		var files = Task.Run(async () => await _fileManager.Get()).Result;

		//		files.ToList().ForEach(f => _fileManager.DeleteFile(f.Name));

		//		mac.Files.ToList().ForEach(f => _fileManager.Add(f.Name, f.Data));
		//	});
		//	 */
		//	#endregion
		//}

		public async Task<TransactionalModel<CmsSites>> EditSite(UserContext userLogin, CmsSites site)
		{
			TransactionalModel<CmsSites> responeData = new TransactionalModel<CmsSites>();

			// Check Licenseinfo
			IEnumerable<tDVRAddressBook> DVRs = DataService.GetAllDVR();
			int countDVR = DVRs.Distinct().Count();
			int countNew = site.HaspLicense.Where(item => item.KDVR == 0).Count();
			int total = countDVR + countNew;
			if (countDVR >= AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber)
			{
				if (countNew > 0)
				{
					responeData.ReturnStatus = false;
					responeData.ReturnMessage.Add(CMSWebError.EXCEED_LICENSE.ToString());
					return responeData;
				}
			}
			else
			{
				if (total > AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber)
				{
					responeData.ReturnStatus = false;
					responeData.ReturnMessage.Add(CMSWebError.EXCEED_LICENSE.ToString());
					return responeData;
				}
			}

			if (site == null)
			{
				responeData.ReturnStatus = false;
				responeData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responeData;
			}

			//bool is_admin = userLogin.ID == userLogin.ParentID;
			//var checkSiteNameExisted = DataService.GetSites(userLogin.ID, true, t => t, null)
			//							.FirstOrDefault(t => (t.UserID == userLogin.ID || t.UserID == userLogin.ParentID) && t.ServerID.ToLower().Equals(site.ServerId.ToLower()));
			var checkSiteNameExisted = DataService.GetSites<tCMSWebSites>(item => item, null).FirstOrDefault(s => s.ServerID.ToLower() == site.ServerId.ToLower());
			if (checkSiteNameExisted != null && checkSiteNameExisted.siteKey != site.SiteKey)
			{
				responeData.ReturnStatus = false;
				responeData.ReturnMessage.Add(CMSWebError.SITE_NAME_EXIST.ToString());
				return responeData;
			}

			//Anh Huynh, Master & admin user always have edit privilege
			//var checkPermision = DataService.GetSites(userLogin.ParentID, is_admin, t => t, null).FirstOrDefault(t => t.siteKey == site.SiteKey);
			//if (checkPermision != null && checkPermision.UserID != userLogin.ID)
			//{
			//	responeData.ReturnStatus = false;
			//	responeData.ReturnMessage.Add(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
			//	return responeData;
			//}

			site = CorrectMacAdress(site);
			var includes = IncludeEntityRelatedSite();
			var sitedb = DataService.GetSite<tCMSWebSites>(site.SiteKey, st => st, includes);//(userLogin.ID, site.SiteKey, includes);
			if (sitedb == null)
			{
				responeData.ReturnStatus = false;
				responeData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responeData;
			}

			responeData = await ModifyCmsSite(userLogin, site, sitedb);
			return responeData;
		}

		public async Task<TransactionalModel<CmsSites>> AddSite(UserContext userLogin, CmsSites site)
		{
			TransactionalModel<CmsSites> responseData = new TransactionalModel<CmsSites>();

			// Check Licenseinfo
			IEnumerable<tDVRAddressBook> DVRs = DataService.GetAllDVR();
			int countDVR = DVRs.Distinct().Count();
			int countNew = site.HaspLicense.Where(item => item.KDVR == 0).Count();
			int total = countDVR + countNew;
			if (countDVR >= AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber)
			{
				if (countNew > 0)
				{
					responseData.ReturnStatus = false;
					responseData.ReturnMessage.Add(CMSWebError.EXCEED_LICENSE.ToString());
					return responseData;
				}

			}
			else
			{
				if (total > AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber)
				{
					responseData.ReturnStatus = false;
					responseData.ReturnMessage.Add(CMSWebError.EXCEED_LICENSE.ToString());
					return responseData;
				}
			}

			
			if (site == null)
			{
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return responseData;
			}

			bool is_admin = userLogin.ID == userLogin.ParentID;
			IEnumerable<string> include_dvr = ServiceBase.ChildProperty(typeof(tCMSWebSites), typeof(tDVRChannels), typeof(tDVRAddressBook));
			string[] includes = { string.Join(".", include_dvr) };
			//var checkSiteNameExisted = DataService.GetSites(userLogin.ID, true, t => t, includes, null)
			//	.FirstOrDefault(t => (t.UserID == userLogin.ID || t.UserID == userLogin.ParentID) && t.ServerID.ToLower().Equals(site.ServerId.ToLower()));
			var checkSiteNameExisted = DataService.GetSites<tCMSWebSites>(item => item, null).FirstOrDefault(s => s.ServerID.ToLower() == site.ServerId.ToLower());
			bool is_update = false;
			if (checkSiteNameExisted != null)
			{
				if (!checkSiteNameExisted.Deleted.HasValue || checkSiteNameExisted.Deleted.Value == false)
				{
					responseData.ReturnStatus = false;
					responseData.ReturnMessage.Add(CMSWebError.SITE_NAME_EXIST.ToString());
					return responseData;
				}
				is_update = true;
			}

			var sitedb = new tCMSWebSites();
			if (is_update)
			{
				sitedb = checkSiteNameExisted;
				site.SiteKey = sitedb.siteKey;
			}

			responseData = await ModifyCmsSite(userLogin, site, sitedb);
			return responseData;
		}

		private tDVRChannels[] SearchDeletetDVRChannels(List<tDVRChannels> oldChannels, List<tDVRChannels> newChannels)
		{
			var deleteChannels = oldChannels.Where(t => newChannels.FirstOrDefault(nt => nt.KChannel == t.KChannel) == null);

			return deleteChannels as tDVRChannels[] ?? deleteChannels.ToArray();
		}

		public TransactionalModel<CmsSites> DeleteSite(UserContext context, int siteKey)
		{
			TransactionalModel<CmsSites> response = new TransactionalModel<CmsSites>();
			var includes = IncludeEntityRelatedSite();
			var sitedb = DataService.GetSites(context.ID, siteKey, includes);
			if (sitedb == null)
			{
				if (context.ParentID != context.ID)
				{
					var siteparent = DataService.GetSites(context.ParentID, siteKey, null);
					if (siteparent != null)
					{
						response.ReturnStatus = false;
						response.ReturnMessage.Add(CMSWebError.DO_NOT_HAVE_PERMISSION_MSG.ToString());
						return response;
					}
				}
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			DataService.DeleteSite(sitedb);
			if (DataService.Save() < 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			//Delete files of sites.
			string path = Path.Combine( AppSettings.AppSettings.Instance.SitesPath, siteKey.ToString());
			FileManager.DirDeleteAsync(path, true).Forget();
			 
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return response;
		}

		private Channels EntityToModel(Func_ChannelsByKDVR_Result channel,tDVRAddressBook addr)
		{
			Channels model = new Channels();
			if (channel == null) return model;
			model.ChannelNo = channel.ChannelNo;
			model.KChannel = channel.KChannel;
            model.KDVR = addr.KDVR;
			model.Name = channel.Name;
			model.Status = channel.Status;
			model.DVRName = addr.ServerID;
            int? width = channel.ResWidth;
            int? height = channel.ResHeight;
			model.Resolution = (width.HasValue && height.HasValue) ? (width.Value !=0 && height.Value!=0) ? (width.Value.ToString()+"x"+height.Value.ToString()):"" :"";
			model.FPS = channel.FPS.ToString();
			model.ModelName = channel.Model;
			return model;
		}

		private List<Channels> ModelToList(tDVRAddressBook addr)
		{
            var channelRes = DataService.GetChannelDetails(addr.KDVR).Result.ToList();
			List<Channels> chs = new List<Channels>();
			if (channelRes == null) return chs;
			if (!channelRes.Any()) return chs;
			channelRes.ForEach(item => chs.Add(EntityToModel(item, addr)));
			return chs;
		}

		public List<DVRModel> GetDVRInfo(int sitekey)
		{
			var includes = new string[]
				{
					typeof (tDVRChannels).Name,
					string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name),
					string.Format("{0}.{1}.{2}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name, "tDVRVideoSources"),
					string.Format("{0}.{1}", typeof (tDVRChannels).Name, typeof (tPTZType).Name),
					string.Format("{0}.{1}.{2}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name, typeof (tDVRIPCameras).Name),
					string.Format("{0}.{1}.{2}.{3}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name, typeof (tDVRIPCameras).Name,typeof (tIPModel).Name),
					string.Format("{0}.{1}.{2}.{3}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name, typeof (tDVRIPCameras).Name,typeof (tDVRIPCameraInputs).Name),
					string.Format("{0}.{1}.{2}", typeof (tDVRChannels).Name, typeof (tDVRAddressBook).Name,typeof (tDVRStorages).Name)
				};
			tCMSWebSites site = DataService.GetSites(filter => filter.siteKey == sitekey, selector => selector, includes).FirstOrDefault();
			if (site == null) return new List<DVRModel>();
			if (!site.tDVRChannels.Any()) return new List<DVRModel>();
			List<tDVRAddressBook> KDVRs = site.tDVRChannels.Select(selector => selector.tDVRAddressBook).Distinct().ToList();
			List<DVRModel> model = new List<DVRModel>();
			KDVRs.ForEach(item => model.Add(new DVRModel
			{
				//ActivationDate = item.ActivationDate,
				CMSMode = item.CMSMode,
				CurConnectTime = item.CurConnectTime,
				//DisConnectReason = item.DisConnectReason,
				DVRAlias = item.DVRAlias,
				DVRGuid = item.DVRGuid,
				//EnableActivation = item.EnableActivation,
				//ExpirationDate = item.ExpirationDate,
				FirstAccess = item.FirstAccess,
				FreeDiskSize = item.FreeDiskSize,
				HaspLicense = item.HaspLicense,
				KDVR = item.KDVR,
				KDVRVersion = item.KDVRVersion,
				//KGroup = item.KGroup,
				//KLocation = item.KLocation,
				LastConnectTime = item.LastConnectTime,
				Online = item.Online,
				PublicServerIP = item.PublicServerIP,
				RecordingDay = item.RecordingDay,
				ServerID = item.ServerID,
				ServerIP = item.ServerIP,
				TimeDisConnect = item.TimeDisConnect,
				TotalDiskSize = item.TotalDiskSize,
				MinDateRec = item.tDVRStorages.Any() ? item.tDVRStorages.Where(w => w.StartRecDate != null).Any() ? item.tDVRStorages.Where(w => w.StartRecDate != null).Min(d => d.StartRecDate).Value.ToString(Consts.RES_DATE_FORMAT) : "" : "",
				MaxDateRec = item.tDVRStorages.Any() ? item.tDVRStorages.Where(w => w.EndRecDate != null).Any() ? item.tDVRStorages.Where(w => w.EndRecDate != null).Max(d => d.EndRecDate).Value.ToString(Consts.RES_DATE_FORMAT) : "" : "",
				Channels = item.tDVRChannels.Any()? ModelToList(item):null
			}));

			return model;
		}
		
		private async Task<TransactionalModel<CmsSites>> ModifyCmsSite(UserContext context, CmsSites site, tCMSWebSites sitedb)
		{
			TransactionalModel<CmsSites> response = new TransactionalModel<CmsSites>();
			var rul = new SitesBusinessRules(base.Culture);
			if (sitedb.siteKey > 0)
			{
				ServiceBase.Includes<tCMSWebSites, tCMSWeb_CalendarEvents>(sitedb, it => it.tCMSWeb_CalendarEvents);
				ServiceBase.Includes<tCMSWebSites, tCMSWeb_UserList>(sitedb, it => it.tCMSWeb_UserList);
				ServiceBase.Includes<tCMSWebSites, tDVRChannels>(sitedb, it => it.tDVRChannels);
				ServiceBase.Includes<tCMSWebSites, tCMSWeb_WorkingHours>(sitedb, it => it.tCMSWeb_WorkingHours);
				ServiceBase.Includes<tCMSWebSites, tCMSWeb_MetricSiteList>(sitedb, it => it.tCMSWeb_MetricSiteList);
				ServiceBase.Includes<tCMSWebSites,tCMSWeb_WorkingHours>( sitedb, it => it.tCMSWeb_WorkingHours);
			}

			site = CorrectMacAdress(site);
			string dbImageSite = sitedb.ImageSite;
			string dbActualBudget = sitedb.ActualBudget;
			string dbFixturePlan = sitedb.FixturePlan;

			//check & update DVR
			//List<tDVRAddressBook> kdvrs = null;
			//if (AddDVRFromSitemodel(site, DataService as IDVRService, DataService as IDvrChanelService, out kdvrs) == CMSWebError.MAC_EXIST_MSG)
			//{
			//	//throw new CmsErrorException(CMSWebError.MAC_WAS_USED.ToString(), MACADDRESS, kdvrs.Last().DVRGuid);
			//	response.ReturnStatus = false;
			//	response.ReturnMessage.Add(CMSWebError.MAC_EXIST_MSG.ToString());
			//	List<MAC> macExisteds = new List<MAC>();
			//	macExisteds.Add(new MAC(){ MacAddress =  kdvrs.Last().DVRGuid });
			//	response.Data = site;
			//	response.Data.Macs = macExisteds;
			//	return response;
			//}

			//var newChannels = kdvrs.Select(it => it.tDVRChannels.ToArray());
			//IEnumerable<tDVRChannels> channels = newChannels.SelectMany(lst => lst);

			//List<tDVRChannels> oldChannels = sitedb.siteKey > 0 ? sitedb.tDVRChannels.ToList() : new List<tDVRChannels>();
			//DataService.Modifyrelation<tDVRChannels, int>(sitedb, oldChannels, channels, uit => uit.KChannel, it => it.tDVRChannels);

			//Check & Update Hasp License
			List<tDVRAddressBook> kdvrs = null;
            CMSWebError ErrorCode = AddHaspLicenseFromSitemodel(site, DataService as IDVRService, DataService as IDvrChanelService, out kdvrs);
            List<HaspLicense> haspExisteds = new List<HaspLicense>();

            if (ErrorCode != CMSWebError.OK)
			{
				response.ReturnStatus = false;
                response.ReturnMessage.Add(ErrorCode.ToString());
				haspExisteds.Add(new HaspLicense() { SerialNumber = kdvrs.Last().HaspLicense });
				response.Data = site;
				response.Data.HaspLicense = haspExisteds;
				return response;
			}

			var newChannels = kdvrs.Select(it => it.tDVRChannels.ToArray());
			IEnumerable<tDVRChannels> channels = newChannels.SelectMany(lst => lst);

			List<tDVRChannels> oldChannels = sitedb.siteKey > 0 ? sitedb.tDVRChannels.ToList() : new List<tDVRChannels>();
			DataService.Modifyrelation<tDVRChannels, int>(sitedb, oldChannels, channels, uit => uit.KChannel, it => it.tDVRChannels);

			//Add zipcode
			#region Add ZipCode

			if (!string.IsNullOrEmpty(site.PostalZipCode))
			{
				tbl_ZipCode dbZipcode = DataService.GetZipCode<tbl_ZipCode>(site.PostalZipCode, item => item, null).FirstOrDefault();
				if (dbZipcode == null)
				{
					dbZipcode = new tbl_ZipCode()
					{
							//ZipCodeID = DataService.GetMaxZipCodeID(),
						ZipCode = site.PostalZipCode
					};
			}
				sitedb.tbl_ZipCode = dbZipcode;
			}
			else sitedb.tbl_ZipCode = null;
			#endregion

			rul.ModelToWebSite(ref sitedb, site, null);
			//update calendar event
			IEnumerable<tCMSWeb_CalendarEvents> newCalendars = CalendarService.CalendarEvents<tCMSWeb_CalendarEvents>(context.ID, t => t, null).Where(t => site.CalendarEvent.Contains(t.ECalID));
			IEnumerable<tCMSWeb_CalendarEvents> oldCalendars = sitedb.siteKey > 0 ? sitedb.tCMSWeb_CalendarEvents.ToList() : null;
			DataService.Modifyrelation<tCMSWeb_CalendarEvents, int>(sitedb, oldCalendars, newCalendars, calit => calit.ECalID, it => it.tCMSWeb_CalendarEvents);
			//update user list
			if (context.ID == context.ParentID || context.ID == sitedb.UserID) //master or owner
			{
				IEnumerable<tCMSWeb_UserList> newUsers = UsersService.GetListUser(t => site.DvrUsers.Contains(t.UserID));
				IEnumerable<tCMSWeb_UserList> oldUsers = sitedb.siteKey > 0 ? sitedb.tCMSWeb_UserList : null;
				DataService.Modifyrelation<tCMSWeb_UserList, int>(sitedb, oldUsers, newUsers, uit => uit.UserID, it => it.tCMSWeb_UserList);
			}
			//update working hous
			IEnumerable<tCMSWeb_WorkingHours> newwh = site.WorkingHours.Select(it => new tCMSWeb_WorkingHours
			{
				ScheduleID = it.ScheduleId,
				CloseTime = it.CloseTime,
				OpenTime = it.OpenTime
																				});
			IEnumerable<tCMSWeb_WorkingHours> old_wh = sitedb.siteKey > 0? sitedb.tCMSWeb_WorkingHours : new List<tCMSWeb_WorkingHours>();
			IEnumerable<tCMSWeb_WorkingHours> delitems = old_wh.Where(it => newwh.Any( n=> n.ScheduleID == it.ScheduleID) == false);
			while(delitems.Any())
				sitedb.tCMSWeb_WorkingHours.Remove( delitems.First());
			tCMSWeb_WorkingHours checking = null;
			foreach(tCMSWeb_WorkingHours it in newwh)
			{
				checking = sitedb.tCMSWeb_WorkingHours.FirstOrDefault( sit => sit.ScheduleID == it.ScheduleID);
				if( checking == null)
				{
					sitedb.tCMSWeb_WorkingHours.Add( it);
					continue;
				}
				if( checking.OpenTime != it.OpenTime || checking.CloseTime != it.CloseTime)
				{
					checking.OpenTime = it.OpenTime;
					checking.CloseTime = it.CloseTime;
				}
			}
			//update metric list
			DataService.ModifyMetric(sitedb, site.DvrMetrics);

			if (sitedb.siteKey  == 0)
			{
				sitedb.UserID = context.ID; //Anh, Update UserID for add new only
				if (DataService.AddSite(sitedb) < 0)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
					return response;
				}
			}
			else
			{
				if (DataService.UpdateSite(sitedb) < 0)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					return response;
				}
			}

			//var deleteMac = SearchDeletetDVRChannels(oldChannels, channels.ToList());
			string path = Path.Combine(AppSettings.AppSettings.Instance.SitesPath, sitedb.siteKey.ToString());
			if (!FileManager.DirExist(path))
			{
				FileManager.DirCreate(path);
			}

			List<Task<bool>> Tresult = new List<Task<bool>>();
			
			#region Upload Images Site DVR
			IEnumerable<ImageModel> imageAddMoreList = site.Files.Where(w => w.Data != null);
			IEnumerable<ImageModel> imageUpdateList = site.Files.Where(w => w.Data == null && w.hasData == false);
			IEnumerable<FileInfo> imageExisted = FileManager.DirGetFileInfos(path);
			if (imageExisted.Any())
			{
				if (imageUpdateList.Any())
				{
					var delfiles = imageExisted.Where(ef => !imageUpdateList.Any(uf => string.Compare(uf.Name, ef.Name, true) == 0));
					if (delfiles.Any())
					{
						delfiles.ToList().ForEach(it => FileManager.FileDelete(it.FullName));
					}
				}
				else 
				{
					//Delete all Images Site DVR
					imageExisted.ToList().ForEach(file => FileManager.FileDeleteAsync(file.FullName).Forget());
				}
			}
			
			//Save new images
			imageAddMoreList.ToList().ForEach(file =>
			{
				var filename = FileManager.FileRandom() + FileManager.FileExtension(file.Name);
				Tresult.Add(FileManager.FileWriteAsync(Path.Combine(path, filename), file.Data, true));
			});

			#endregion

			#region Upload Images Site Doc

			if (string.IsNullOrEmpty(site.ImageSite))
			{
				if (!string.IsNullOrEmpty(dbImageSite))
				{
					FileManager.FileDelete(Path.Combine(path, dbImageSite));
				}
			}
			else
			{
			if (site.ImageSiteBytes != null && site.ImageSiteBytes.Length > 0)
			{
					if (!string.IsNullOrEmpty(dbImageSite))
				{
						FileManager.FileDelete(Path.Combine(path, dbImageSite));
				}

					string imageSitePath = Path.Combine(path, Consts.IMAGE_SITE_FIELD);
					if (!FileManager.DirExist(imageSitePath))
					{
						FileManager.DirCreate(imageSitePath);
					}
					Tresult.Add(FileManager.FileWriteAsync(Path.Combine(imageSitePath, sitedb.ImageSite), site.ImageSiteBytes, true));
			}
			}

			#endregion

			#region Upload Actual Budget Doc

			if (string.IsNullOrEmpty(site.ActualBudget))
			{
				if (!string.IsNullOrEmpty(dbActualBudget))
				{
					FileManager.FileDelete(Path.Combine(path, dbActualBudget));
				}
			}
			else
			{
			if (site.ActualBudgetBytes != null && site.ActualBudgetBytes.Length > 0)
			{
					if (!string.IsNullOrEmpty(dbActualBudget))
				{
						FileManager.FileDelete(Path.Combine(path, dbActualBudget));
				}

					string actualBudgetPath = Path.Combine(path, Consts.ACTUAL_BUDGET_FIELD);
					if (!FileManager.DirExist(actualBudgetPath))
					{
						FileManager.DirCreate(actualBudgetPath);
					}
					Tresult.Add(FileManager.FileWriteAsync(Path.Combine(actualBudgetPath, sitedb.ActualBudget), site.ActualBudgetBytes, true));
			}
			}

			#endregion

			#region Upload Fixture Plan Doc

			if (string.IsNullOrEmpty(site.FixturePlan))
			{
				if (!string.IsNullOrEmpty(dbFixturePlan))
				{
					FileManager.FileDelete(Path.Combine(path, dbFixturePlan));
				}
			}
			else
			{
			if (site.FixtureBytes != null && site.FixtureBytes.Length > 0)
			{
					if (!string.IsNullOrEmpty(dbFixturePlan))
				{
						FileManager.FileDelete(Path.Combine(path, dbFixturePlan));
				}

					string fixturePlanPath = Path.Combine(path, Consts.FIXTURE_PLAN_FIELD);
					if (!FileManager.DirExist(fixturePlanPath))
					{
						FileManager.DirCreate(fixturePlanPath);
					}
					Tresult.Add(FileManager.FileWriteAsync(Path.Combine(fixturePlanPath, sitedb.FixturePlan), site.FixtureBytes, true));
			}
			}
			#endregion

			#region Upload MAC Address Images

			//site.Macs.ToList().ForEach(mac =>
			//{
			//	var macpath = Path.Combine(path, mac.Id.ToString());
			//	if (mac.Id == 0)
			//	{ 
			//		//Get KDVR Id
			//		var channelmacs = sitedb.tDVRChannels.FirstOrDefault(t => t.tDVRAddressBook.DVRGuid == mac.MacAddress);
			//		if (channelmacs != null) 
			//		{
			//			macpath = Path.Combine(path, channelmacs.tDVRAddressBook.KDVR.ToString());
			//		}
			//	}

						//IEnumerable<FileInfo> existedfile = FileManager.DirGetFileInfos(macpath);
			//	if (existedfile.Any() && !mac.Files.Any())
			//	{
			//		//Delete all MAC files
			//		existedfile.ToList().ForEach(file => FileManager.FileDeleteAsync(file.FullName).Forget());
			//	}
			//	else 
			//	{
			//		var siteFileNew = mac.Files.Where(it => it.hasData == false && it.Data == null);
			//		if (siteFileNew != null && siteFileNew.Count() > 0)
			//		{
			//			//IEnumerable<FileInfo> existedfile = FileManager.DirGetFileInfos(macpath);
			//			var delfiles = existedfile.Where(ef => !siteFileNew.Any(it => string.Compare(it.Name, ef.Name, true) == 0));
			//			if (delfiles.Any())
			//			{
			//				delfiles.ToList().ForEach(it => FileManager.FileDelete(it.FullName));
			//			}
			//		}
			//	}
				
			//	if (!FileManager.DirExist(macpath))
			//	{
			//		FileManager.DirCreate(macpath);
			//	}

			//	//mac.Files.Where(it => it.hasData && it.Data != null).ToList().ForEach(f =>
			//	//	Tresult.Add(FileManager.FileWriteAsync(Path.Combine(macpath, f.Name), f.Data, true))
			//	//	//_fileManager.Add(f.Name, f.Data)
			//	//							);
			//	mac.Files.Select(item => item).ToList().ForEach(f =>
			//	{
			//		var filename = FileManager.FileRandom() + FileManager.FileExtension(f.Name);
			//		Tresult.Add(FileManager.FileWriteAsync(Path.Combine(macpath, filename), f.Data, true));
			//		if (f.MAC != null && f.MAC != mac.MacAddress)
			//		{//edit site
			//			string srcFile = Path.Combine(path, f.MAC, f.Name);
			//			string destFile = Path.Combine(macpath, f.Name);
			//			FileManager.FileCopy(srcFile, destFile);
			//		}
					
			//	});
			//});

			#endregion

			if( Tresult.Any())
				await Task.WhenAll(Tresult);

			#region Delete MAC Address Images

			//if (deleteMac.Length > 0)
			//{
			//	foreach (var mac in deleteMac.Where(mac => mac.tDVRAddressBook.DVRGuid != null))
			//	{
			//		//_fileManager.SetWorkingFolder(path);
			//		//_fileManager.DeleteFolder(mac.tDVRAddressBook.DVRGuid);
			//		FileManager.DirDeleteAsync(Path.Combine(path, mac.tDVRAddressBook.DVRGuid)).Forget();
			//	}
			//}

			#endregion

			response.ReturnStatus = true;
			if (site.SiteKey > 0)
			{
				response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			}
			else 
			{
				response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			}
			site.SiteKey = sitedb.siteKey;
			response.Data = site;
			return response;
				
		}

		public List<ZipCodeModel> FilterZipCode(string filter)
		{
			List<ZipCodeModel> response = new List<ZipCodeModel>();
			response = DataService.FilterZipCode(filter, item => item, null)
				.Select(s => new ZipCodeModel()
				{
					 ZipCodeID = s.ZipCodeID,
					 ZipCode = s.ZipCode
				}).ToList();
			return response;
		}

		public List<ZipCodeModel> GetZipCode(string zipcode)
		{
			List<ZipCodeModel> response = new List<ZipCodeModel>();
			response = DataService.GetZipCode(zipcode, item => item, null)
				.Select(s => new ZipCodeModel()
				{
					ZipCodeID = s.ZipCodeID,
					ZipCode = s.ZipCode
				}).ToList();
			return response;
		}

		public TransactionalModel<ZipCodeModel> AddZipCode(ZipCodeModel model)
		{
			TransactionalModel<ZipCodeModel> response = new TransactionalModel<ZipCodeModel>();
			if (model == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			tbl_ZipCode dbZipCode = new tbl_ZipCode()
			{
				ZipCode = model.ZipCode 
				//, ZipCodeID = model.ZipCodeID 
			};
			int ret = DataService.AddZipCode(dbZipCode);
			if (ret <= 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			model.ZipCodeID = ret;
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			response.Data = model;
			return response;
		}

		public IEnumerable<HaspLicense> GetAllHaspLicense(int siteKey)
		{
			List<HaspLicense> response = new List<HaspLicense>();
			var includes = new string[]
			{
				typeof (tDVRChannels).Name
			};

			var sitedetail = DataService.GetSites(site => site.siteKey != siteKey, item => item.tDVRChannels, includes);
			var kdvrs = sitedetail.SelectMany(it => it).Select(dvr => dvr.KDVR);
			IQueryable<HaspLicense> hasps = DataService.GetAllHaspLicense<HaspLicense>(item => new HaspLicense { KDVR = item.KDVR, SerialNumber = item.HaspLicense, ServerID = item.ServerID }, null);
			var haspNotUsed = hasps.Where(w => !kdvrs.Any( it => it == w.KDVR));
			return haspNotUsed;
		}
        public TransactionalModel<DVRInfoRebarTransact> GetDVRInfoRebarTransact(int kdvr)
        {
            TransactionalModel<DVRInfoRebarTransact> response = new TransactionalModel<DVRInfoRebarTransact>();
            response.IsAuthenicated = true;
            response.ReturnMessage.Add(CMSWebError.PARAM_WRONG.ToString());
            response.ReturnStatus = false;

            string [] includes = new string[1];
            includes[0] = "tDVRUsers";
            IQueryable<tDVRAddressBook> dvrs =  DataService.GetDVRInfo<tDVRAddressBook>(kdvr, selector => selector, includes);
            if(dvrs.Any())
            {
                tDVRAddressBook addr = dvrs.First();
                if (addr.tDVRUsers.Any())
                {
                    response.Data = new DVRInfoRebarTransact()
                    {
                        DVRVersion = addr.KDVRVersion.ToString(),
                        KDVR = addr.KDVR,
                        MACAddress = addr.DVRGuid,
                        Password = addr.tDVRUsers.First(item => item.KDVR == addr.KDVR && item.Usertype == "admin").Password,
                        PublicServerIP = addr.PublicServerIP,
                        ServerID = addr.ServerID,
                        ServerIP = addr.ServerIP,
                        Username = addr.tDVRUsers.First(item => item.KDVR == addr.KDVR && item.Usertype == "admin").UserName
                    };
                    response.IsAuthenicated = true;
                    response.ReturnMessage.Add(CMSWebError.OK.ToString());
                    response.ReturnStatus = true;
                }
           }
            return response;
        }

	}
}
