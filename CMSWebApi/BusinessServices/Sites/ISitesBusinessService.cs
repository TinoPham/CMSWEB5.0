using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;

namespace CMSWebApi.BusinessServices.Sites
{
	public interface ISitesBusinessService
	{
		CMSWebSiteModel GetSites(UserContext context, int? userId = null);
		Task<FileModel> GetFileSite(UserContext context, string fileName, int siteKey);
		CMSWebSiteModel GetTreeSites(UserContext context, bool hasMac, int? userId = null);
		ICollection<TreeMetric> GetTreeMetrics(UserContext context);
		void SaveTreeNode(CMSWebSiteModel treeNode, UserContext context);
		RegionModel AddRegion(UserContext context, RegionModel region);
		void UpdateRegion(UserContext context, RegionModel region);
		void DeleteRegion(UserContext context, int regionKey);
		RegionModel GetRegion(UserContext context, int regionKey);
		CmsSites GetSites(UserContext context, int siteKey);
		CmsSites AddSite(UserContext context, CmsSites site);
		void EditSite(UserContext context, CmsSites site);
		void DeleteSite(UserContext context, int siteKey);
		ISiteService DataService { get; set; }
		DataServices.ServiceBase ServiceBase { get; }
		CultureInfo Culture { get; set; }
		CMSWebApi.DataModels.UserContext Userctx { get; set; }
	}
}