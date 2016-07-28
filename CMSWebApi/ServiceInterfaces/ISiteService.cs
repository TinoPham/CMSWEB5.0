using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using System.Threading.Tasks;
namespace CMSWebApi.ServiceInterfaces
{
	public interface ISiteService
	{
		void ModifyMetric( tCMSWebSites dbsite,IEnumerable<int> metrickeys);
		IQueryable<Tout> GetSites<Tout>(int userID, bool isAdmin, Expression<Func<tCMSWebSites, Tout>> selector, string [] includes, bool? isdelete = false) where Tout : class;
		Tout GetSite<Tout>(int siteID, Expression<Func<tCMSWebSites, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> GetSites<Tout>(IEnumerable<int> sites,Expression<Func<tCMSWebSites, Tout>> selector, string [] includes);
		IQueryable<Tout> GetSites<Tout>(Expression<Func<tCMSWebSites, bool>> filter, Expression<Func<tCMSWebSites, Tout>> selector, string[] includes);
		tCMSWebSites GetSites(int UserId, int siteID, string[] includes);
		IQueryable<Tout> GetSites<Tout>(Expression<Func<tCMSWebSites, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetRegions<Tout>(int userId, Expression<Func<tCMSWebRegion, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> GetRegions<Tout>(Expression<Func<tCMSWebRegion, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<tCMSWebRegion> GetRegions(UserContext user);
		int UpdateRegion(tCMSWebRegion region, bool isCommit = true);
		int UpdateSite(tCMSWebSites sites, bool isCommit = true);
		int AddRegion(tCMSWebRegion region, bool isCommit = true);
		int AddSite(tCMSWebSites sites, bool isCommit = true);
		bool DeleteRegion(int userKey, int regionKey, bool isCommit = true);
		bool DeleteRegion(tCMSWebRegion model, bool isCommit = true);
		void DeleteSite(int userKey, int siteKey);
		void DeleteSite(tCMSWebSites site);
		void Remove_Calendars_fromSite(tCMSWebSites site, params tCMSWeb_CalendarEvents[] calendars);
		void Add_Calendars_fromSite(tCMSWebSites site, params tCMSWeb_CalendarEvents[] calendars);
		void Remove_Chanels_fromSite(tCMSWebSites site, params tDVRChannels[] chanels);
		void Remove_Users_fromSite(tCMSWebSites site, params tCMSWeb_UserList[] users);
		void Add_Chanels_fromSite(tCMSWebSites site, params tDVRChannels[] chanels);
		void Add_Users_fromSite(tCMSWebSites site, params tCMSWeb_UserList[] users);
		void DeleteWorkingHour(int key);
		void UpdateWorkingHourSite(tCMSWeb_WorkingHours workingHourSite);
		void AddWorkingHourSite(tCMSWeb_WorkingHours workingHourSite);
		void DeleteMetricSite(int key);
		void UpdateMetricSite(tCMSWeb_MetricSiteList metricSite);
		void AddMetricSite(tCMSWeb_MetricSiteList metricSite);
		int Save();
		//IQueryable<Tout> GetChannelDetail<Tout>(int KDVR, Expression<Func<View_Channels_Resolutions, Tout>> selector, string[] includes) where Tout : class;
        Task<List<Func_ChannelsByKDVR_Result>> GetChannelDetails(int KDVR);
		IQueryable<Tout> GetWorkingHours<Tout>(int siteKey, Expression<Func<tCMSWeb_WorkingHours, Tout>> selector, string [] includes) where Tout : class;
		IQueryable<Tout> GetMetricSites<Tout>(int siteKey, Expression<Func<tCMSWeb_MetricSiteList, Tout>> selector, string [] includes) where Tout : class;
		void Modifyrelation<T,TK>(tCMSWebSites dbsite, IEnumerable<T> current, IEnumerable<T>news, Func<T,TK>key, Expression<Func<tCMSWebSites, object>> properties) where T:class;
		IQueryable<Tout> FilterZipCode<Tout>(string stringFilter, Expression<Func<tbl_ZipCode, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetZipCode<Tout>(string zipcode, Expression<Func<tbl_ZipCode, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetAllHaspLicense<Tout>(Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class;
		int AddZipCode(tbl_ZipCode dbZipCode);
		int UpdateZipCode(tbl_ZipCode dbZipCode);
		//int GetMaxZipCodeID();
		IQueryable<Tout> GetDVRInfo<Tout>(int kDVR, Expression<Func<tDVRAddressBook, Tout>> selector, string[] includes) where Tout : class;
        IEnumerable<tDVRAddressBook> GetAllDVR();
	}
}
