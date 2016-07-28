using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using Extensions.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace CMSWebApi.DataServices
{
	public partial class SiteService: DVRService, ISiteService
	{
		public SiteService(PACDMModel.Model.IResposity model) : base(model) { }

		public SiteService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public void ModifyMetric( tCMSWebSites dbsite,IEnumerable<int> metrickeys)
		{
			string include = base.ChildProperty<tCMSWeb_Metric_List, tCMSWeb_MetricSiteList>();
			IEnumerable<tCMSWeb_Metric_List> metrics = !metrickeys.Any() ? new List<tCMSWeb_Metric_List>() : DBModel.Query<tCMSWeb_Metric_List>(it => metrickeys.Contains(it.MListID), new string [] { include }).ToList();
			IEnumerable<tCMSWeb_MetricSiteList> delitem = dbsite.tCMSWeb_MetricSiteList.Where( it => metrickeys.Contains( it.MListID) == false);
			IEnumerable<tCMSWeb_Metric_List> additem = metrics.Where(it => dbsite.tCMSWeb_MetricSiteList.FirstOrDefault(s => s.MListID == it.MListID) == null);
			while( delitem.Any())
			{
				dbsite.tCMSWeb_MetricSiteList.Remove( delitem.First());
			}
			tCMSWeb_MetricSiteList newmap = null;

			foreach(tCMSWeb_Metric_List item in additem)
			{
				newmap = new tCMSWeb_MetricSiteList();
				newmap.tCMSWebSites = dbsite;
				dbsite.tCMSWeb_MetricSiteList.Add(newmap);
				newmap.tCMSWeb_Metric_List = item;
				item.tCMSWeb_MetricSiteList.Add(newmap);
				newmap.CreateDate = DateTime.Now;
			}
			
		}
		
		public void Modifyrelation<T,TK>(tCMSWebSites dbsite, IEnumerable<T> current, IEnumerable<T>news, Func<T,TK>key, Expression<Func<tCMSWebSites, object>> properties) where T:class
		{
			ModifyDataRelation<tCMSWebSites, T, TK>(dbsite,current,news, key, properties);
		}
		
		public IQueryable<Tout> GetSites<Tout>(int userID, bool isAdmin, Expression<Func<tCMSWebSites, Tout>> selector, string [] includes, bool? isdelete = false) where Tout : class
		{
			IQueryable<tCMSWebSites> model;
			if(!isAdmin)
			{
				if( isdelete.HasValue)
					model = DBModel.Query<tCMSWebSites>(w => w.UserID == userID && w.Deleted == isdelete.Value, includes);
				else
					model = DBModel.Query<tCMSWebSites>(w => w.UserID == userID, includes);

				return model.Select( selector);
			}
			else
			{
				if (isdelete.HasValue)
					model = DBModel.Query<tCMSWebSites>(w => w.Deleted == isdelete.Value, includes);
				else
					model = DBModel.Query<tCMSWebSites>(null, includes);
			}
			//IQueryable<tCMSWeb_UserList> user_sites = DBModel.Query<tCMSWeb_UserList>(usite => usite.UserID == userID);
			//return model.Join(user_sites, s => s.siteKey, us => us.SiteID, (s, us) => s).Distinct().Select(selector);
			return model.Select(selector);
		}

		public Tout GetSite<Tout>(int siteID, Expression<Func<tCMSWebSites, Tout>> selector, string [] includes) where Tout : class
		{
			return base.FirstOrDefault<tCMSWebSites, Tout>( site => site.siteKey == siteID, selector, includes);
		}

		public tCMSWebSites GetSites(int UserId, int siteID, string[] includes)
		{
			return DBModel.FirstOrDefault<tCMSWebSites>(site => site.siteKey == siteID && site.UserID == UserId, includes);
		}

		public IQueryable<Tout> GetSites<Tout>(IEnumerable<int> sites,Expression<Func<tCMSWebSites, Tout>> selector, string [] includes)
		{
			return base.Query<tCMSWebSites, Tout>(site => sites.Contains(site.siteKey), selector, includes);
		}

		public IQueryable<Tout> GetSites<Tout>(Expression<Func<tCMSWebSites, bool>> filter, Expression<Func<tCMSWebSites, Tout>> selector, string[] includes)
		{
			return base.Query<tCMSWebSites, Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetSites<Tout>(Expression<Func<tCMSWebSites, Tout>> selector, string[] includes) where Tout: class
		{
			return Query<tCMSWebSites, Tout>(null, selector, includes);
		}

		public IQueryable<Tout> GetRegions<Tout>(int userId, Expression<Func<tCMSWebRegion, Tout>> selector, string [] includes) where Tout : class
		{
			return Query<tCMSWebRegion, Tout>(item => item.UserKey == userId, selector, includes);
		}

		public IQueryable<Tout> GetRegions<Tout>(Expression<Func<tCMSWebRegion, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWebRegion, Tout>(null, selector, includes);
		}

		public IQueryable<tCMSWebRegion> GetRegions(UserContext user)
		{
			var includes = new string[]
			{
				typeof (tCMSWebSites).Name, 
				string.Format("{0}.{1}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name),
				string.Format("{0}.{1}.{2}", typeof (tCMSWebSites).Name, typeof (tDVRChannels).Name,typeof (tDVRAddressBook).Name)
			};

			return DBModel.Query<tCMSWebRegion>(t => t.UserKey == user.ID, includes);
			//.Where(t => t.tCMSWebSites.Where(w => w.Deleted == false).Equals(t.tCMSWebSites))
		}

		public int AddRegion(tCMSWebRegion region, bool isCommit = true)
		{
			DBModel.Insert<tCMSWebRegion>(region);
			if (isCommit)
			{
				return DBModel.Save() > 0 ? region.RegionKey : -1;
			}
			else
			{
				return 0;
			}

		}

		public int UpdateRegion(tCMSWebRegion region, bool isCommit = true)
		{
			DBModel.Update<tCMSWebRegion>(region);
			if (isCommit)
			{
			return DBModel.Save() >= 0 ? region.RegionKey : -1;
				}
			else
			{
				return 0;
			}
		}

		public int UpdateSite(tCMSWebSites sites, bool isCommit = true)
		{
			DBModel.Update<tCMSWebSites>(sites);
			if (isCommit)
			{
			return DBModel.Save() >= 0 ? sites.siteKey : -1;
				}
			else
			{
				return 0;
			}
		}

		public int AddSite(tCMSWebSites sites, bool isCommit = true)
		{
			DBModel.Insert<tCMSWebSites>(sites);
			if (isCommit)
			{
			return DBModel.Save() > 0 ? sites.siteKey : -1;
				}
			else
			{
				return 0;
			}
		}

		public bool DeleteRegion(int userKey, int regionKey, bool isCommit = true)
		{
			DBModel.DeleteWhere<tCMSWebRegion>(t=>t.UserKey == userKey && t.RegionKey == regionKey);
			if (isCommit)
			{
			return DBModel.Save() > 0 ? true : false;
				}
			else
			{
				return true;
			}
		}

		public bool DeleteRegion(tCMSWebRegion model, bool isCommit = true)
		{
			DBModel.Delete<tCMSWebRegion>(model);
			if (isCommit)
			{
			return DBModel.Save() > 0 ? true : false;
			}
			else
			{
				return true;
			}
		}

		public void DeleteSite(int userKey, int siteKey)
		{
			DBModel.DeleteWhere<tCMSWebSites>(t => t.UserID == userKey && t.siteKey == siteKey);
		}

		public void DeleteSite(tCMSWebSites site)
		{
			//this.tCMSWeb_WorkingHours = new HashSet<tCMSWeb_WorkingHours>();
			//this.tCMSWebBAMHeatMapAreas = new HashSet<tCMSWebBAMHeatMapAreas>();
			//this.tCMSWeb_MetricSiteList = new HashSet<tCMSWeb_MetricSiteList>();
			//this.tDVRChannels = new HashSet<tDVRChannels>();
			//this.tCMSWeb_UserList = new HashSet<tCMSWeb_UserList>();
			//this.tCMSWeb_CalendarEvents = new HashSet<tCMSWeb_CalendarEvents>();
			//this.tCMSWebSiteImage = new HashSet<tCMSWebSiteImage>();
			DBModel.DeleteWhere<tCMSWeb_WorkingHours>( it => it.SiteID == site.siteKey);
			DBModel.DeleteWhere<tCMSWebBAMHeatMapAreas>(it => it.siteKey == site.siteKey);
			DBModel.DeleteWhere<tCMSWeb_MetricSiteList>(it => it.SiteID == site.siteKey);
			DBModel.DeleteWhere<tCMSWebSiteImage>(it => it.siteKey == site.siteKey);
			DBModel.Include<tCMSWebSites,tDVRChannels>(site, s => s.tDVRChannels);
			DBModel.DeleteItemRelation<tCMSWebSites,tDVRChannels>( site, s=> s.tDVRChannels, site.tDVRChannels.ToArray());
			DBModel.Include<tCMSWebSites, tCMSWeb_UserList>(site, s => s.tCMSWeb_UserList);
			DBModel.DeleteItemRelation<tCMSWebSites, tCMSWeb_UserList>(site, s => s.tCMSWeb_UserList, site.tCMSWeb_UserList.ToArray());
			DBModel.Include<tCMSWebSites, tCMSWeb_CalendarEvents>(site, s => s.tCMSWeb_CalendarEvents);
			DBModel.DeleteItemRelation<tCMSWebSites, tCMSWeb_CalendarEvents>(site, s => s.tCMSWeb_CalendarEvents, site.tCMSWeb_CalendarEvents.ToArray());
			DBModel.Delete(site);
		}

		public void Remove_Calendars_fromSite(tCMSWebSites site, params tCMSWeb_CalendarEvents[] calendars)
		{
			DBModel.DeleteItemRelation<tCMSWebSites, tCMSWeb_CalendarEvents>(site, u => u.tCMSWeb_CalendarEvents, calendars);
		}

		public void Add_Calendars_fromSite(tCMSWebSites site, params tCMSWeb_CalendarEvents[] calendars)
		{
			DBModel.AddItemRelation<tCMSWebSites, tCMSWeb_CalendarEvents>(site, u => u.tCMSWeb_CalendarEvents, calendars);
		}

		public void Remove_Chanels_fromSite(tCMSWebSites site, params tDVRChannels[] chanels)
		{
			DBModel.DeleteItemRelation<tCMSWebSites, tDVRChannels>(site, u => u.tDVRChannels, chanels);
		}

		public void Add_Chanels_fromSite(tCMSWebSites site, params tDVRChannels[] chanels)
		{
			DBModel.AddItemRelation<tCMSWebSites, tDVRChannels>(site, u => u.tDVRChannels, chanels);
		}

		public void Remove_Users_fromSite(tCMSWebSites site, params tCMSWeb_UserList[] users)
		{
			DBModel.DeleteItemRelation<tCMSWebSites, tCMSWeb_UserList>(site, u => u.tCMSWeb_UserList, users);
		}

		public void Add_Users_fromSite(tCMSWebSites site, params tCMSWeb_UserList[] users)
		{
			DBModel.AddItemRelation<tCMSWebSites, tCMSWeb_UserList>(site, u => u.tCMSWeb_UserList, users);
		}

		public void DeleteWorkingHour(int key)
		{
			DBModel.DeleteWhere<tCMSWeb_WorkingHours>(t => t.SiteID == key);
		}

		public void UpdateWorkingHourSite(tCMSWeb_WorkingHours workingHourSite)
		{
			DBModel.Update<tCMSWeb_WorkingHours>(workingHourSite);
		}

		public void AddWorkingHourSite(tCMSWeb_WorkingHours workingHourSite)
		{
			DBModel.Insert <tCMSWeb_WorkingHours>(workingHourSite);
		}

		//public void DeleteUserSite(int key)
		//{
		//	DBModel.DeleteWhere<tCMSWeb_User_Sites>(t => t.SiteID == key);
		//}

		//public void UpdateUserSite(tCMSWeb_User_Sites userSite)
		//{
		//	DBModel.Update<tCMSWeb_User_Sites>(userSite);
		//}

		//public void AddUserSite(tCMSWeb_User_Sites userSite)
		//{
		//	DBModel.Insert<tCMSWeb_User_Sites>(userSite);
		//}

		public void DeleteMetricSite(int key)
		{
			DBModel.DeleteWhere<tCMSWeb_MetricSiteList>(t => t.SiteID == key);
		}

		public void UpdateMetricSite(tCMSWeb_MetricSiteList metricSite)
		{
			DBModel.Update<tCMSWeb_MetricSiteList>(metricSite);
		}

		public void AddMetricSite(tCMSWeb_MetricSiteList metricSite)
		{
			DBModel.Insert<tCMSWeb_MetricSiteList>(metricSite);
		}

		public int Save()
		{
			return DBModel.Save();
		}

		//public IQueryable<Tout> GetCalendarEvents<Tout>(int siteKey, Expression<Func<tCMSWeb_CalendarEvents_Sites, Tout>> selector, string [] includes) where Tout : class
		//{
		//	return Query<tCMSWeb_CalendarEvents_Sites, Tout>(item => item.SiteID == siteKey, selector, includes);
		//}

		//public IQueryable<tCMSWeb_CalendarEvents_Sites> GetCalendarEvents(int siteKey)
		//{
		//	return DBModel.Query<tCMSWeb_CalendarEvents_Sites>(item => item.SiteID == siteKey);
		//}

		public IQueryable<Tout> GetWorkingHours<Tout>(int siteKey, Expression<Func<tCMSWeb_WorkingHours, Tout>> selector, string [] includes) where Tout : class
		{
			return Query<tCMSWeb_WorkingHours, Tout>(t=>t.SiteID == siteKey, selector, includes);
		}

		//public IQueryable<tCMSWeb_WorkingHours> GetWorkingHours(int siteKey)
		//{
		//	return DBModel.Query<tCMSWeb_WorkingHours>(t=>t.SiteID == siteKey);
		//}

		//public IQueryable<Tout> GetUserSites<Tout>(int siteKey, Expression<Func<tCMSWeb_User_Sites, Tout>> selector, string [] includes) where Tout : class
		//{
		//	return Query<tCMSWeb_User_Sites, Tout>(t => t.SiteID == siteKey, selector, includes);
		//}

		//public IQueryable<tCMSWeb_User_Sites> GetUserSites(int siteKey)
		//{
		//	return DBModel.Query<tCMSWeb_User_Sites>(t=>t.SiteID == siteKey);
		//}

		public IQueryable<Tout> GetMetricSites<Tout>(int siteKey, Expression<Func<tCMSWeb_MetricSiteList, Tout>> selector, string [] includes) where Tout : class
		{
			return Query<tCMSWeb_MetricSiteList, Tout>(t => t.SiteID == siteKey, selector, includes);
		}

		public Task<List<Func_ChannelsByKDVR_Result>> GetChannelDetails(int kdvr)
		{
            string sql = string.Format(SQLFunctions.Func_ChannelsByKDVR,kdvr.ToString());
            Task<List<Func_ChannelsByKDVR_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_ChannelsByKDVR_Result>(sql);
			return resCount;
		}

       

		public IQueryable<Tout> FilterZipCode<Tout>(string stringFilter, Expression<Func<tbl_ZipCode, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tbl_ZipCode, Tout>(t => t.ZipCode.ToLower().Contains(stringFilter.ToLower()), selector, includes);
		}

		public IQueryable<Tout> GetZipCode<Tout>(string zipcode, Expression<Func<tbl_ZipCode, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tbl_ZipCode, Tout>(t => t.ZipCode == zipcode, selector, includes);
		}

		public int AddZipCode(tbl_ZipCode dbZipCode)
		{
			DBModel.Insert<tbl_ZipCode>(dbZipCode);
			return DBModel.Save() > 0 ? dbZipCode.ZipCodeID : -1;
		}

		public int UpdateZipCode(tbl_ZipCode dbZipCode)
		{
			DBModel.Update<tbl_ZipCode>(dbZipCode);
			return DBModel.Save() >= 0 ? dbZipCode.ZipCodeID : -1;
		}

		public IQueryable<Tout> GetAllHaspLicense<Tout>(Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tDVRAddressBook, Tout>(t => !string.IsNullOrEmpty(t.HaspLicense) && t.HaspLicense != "0", selector, includes);
		}

		//public int GetMaxZipCodeID()
		//{
		//	var model = DBModel.Query<tbl_ZipCode>().Select(s => s.ZipCodeID);
		//	return (model != null && model.Any()) ? model.Max() + 1 : 1;
		//}
		public IQueryable<Tout> GetDVRInfo<Tout>(int kDVR, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tDVRAddressBook, Tout>(t => t.KDVR == kDVR, selector, includes);
		}

        public IEnumerable<tDVRAddressBook> GetAllDVR()
        {
            return DBModel.Query<tDVRAddressBook>();
        }
	}
}
