using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;

namespace CMSWebApi.DataModels
{
	public class RegionSite : CMSWebSiteModel // : TransactionalInformation
	{
		//public int Type { get; set; }
		public List<RegionSite> RegionSites { get; set; }

		//public RegionSite(tCMSWebRegion region)
		//{
		//	ID = region.RegionKey;
		//	Name = region.RegionName;
		//	ServerID = string.Empty;
		//	//MACAddress = string.Empty;
		//	UserID = region.UserKey;
		//	ParentKey = region.RegionKey;
		//	Type = SiteType.REGION;

		//}
		//public RegionSite(tCMSWebSites site)
		//{
		//	ID = site.siteKey;
		//	Name = site.ServerID;
		//	ServerID = site.ServerID;
		//	//MACAddress = site.MACAddress;
		//	UserID = site.UserID;
		//	ParentKey = site.RegionKey;
		//	Type = SiteType.SITE;
		//}
	}
}