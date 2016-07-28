using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;
namespace CMSWebApi.ServiceInterfaces
{
	public interface IAlertService
	{
		#region Alert types
		Tout GetAlertType<Tout> (byte typeID, Expression< Func<tAlertType, Tout>> selector, string [] includes);
		IQueryable<Tout> GetAlertTypes<Tout>(Expression<Func<tAlertType, bool>> filter, Expression<Func<tAlertType, Tout>> selector, string [] includes);
		#endregion
		#region AlertSeverity
		Tout GetAlertSeverity<Tout>(byte typeID, Expression<Func<tAlertSeverity, Tout>> selector, string [] includes);
		IQueryable<Tout> GetAlertSeverities<Tout>(Expression<Func<tAlertSeverity, Tout>> selector, string [] includes);
		#endregion
		#region ALert
		Tout GetAlert<Tout>(int alertid, Expression<Func<tAlertEvent, Tout>> selector, string [] includes);
		IQueryable<Tout> GetAlertEventDetailsAsNoTrack<Tout>(Expression<Func<tAlertEventDetail, bool>> filter, Expression<Func<tAlertEventDetail, Tout>> selector, string[] includes = null);
		IQueryable<Tout> GetAlertEventDetails<Tout>(Expression<Func<tAlertEventDetail, bool>> filter, Expression<Func<tAlertEventDetail, Tout>> selector, string[] includes = null);
		IQueryable<Tout> GetLastAlertEvents<Tout>(Expression<Func<tAlertEventLast, bool>> filter, Expression<Func<tAlertEventLast, Tout>> selector, string[] includes = null);
		IQueryable<Tout> GetAlerts<Tout>(Expression<Func<tAlertEvent, bool>> filter,Expression<Func<tAlertEvent, Tout>> selector, string[] includes = null);
		IQueryable<SiteMonitorModel> GetAlertEventsByKdvrs(List<int> kdvrs, DateTime begin, DateTime end);
		IQueryable<SiteSensorsModel> GetSensorsEventsByKdvrs(List<int> kdvrs, DateTime begin, DateTime end);
		IQueryable<Tout> GetAlerts<Tout>(int? KDVR, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string [] includes);
		IQueryable<Tout> GetAlerts<Tout>(byte? AlertSeverity, int? KDVR, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string[] includes, bool byTimeZone = false);
		IQueryable<Tout> GetAlerts<Tout>(IEnumerable<int> KDVRs, byte? AlertSeverity, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string [] includes);
		IQueryable<Tout> GetLastALerts<Tout>(int tAlertEvent, IEnumerable<int> kdvrs, IEnumerable<byte>AlertTypes, Expression<Func<tAlertEvent, Tout>> selector, string [] includes);
		IQueryable<Tout> GetAlertsbyTypes<Tout>(IEnumerable<int> KDVRs, IEnumerable<byte> KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector);
		Task<List<Func_DVR_Offline_Result>> GetDVR_On_Offline_Async(IEnumerable<int> kdvrs, DateTime datetime, bool isoffline, int byHours = 0, int keepaliveint = 0);
		Task<List<CMSWeb_Cache_ALert_Result>>CMSWeb_Cache_ALert(DateTime sdate, DateTime edate);
		Task<List<AlertModelSummary>> AlertsSummary(List<int> kdvrs, DateTime startDate, DateTime endDate, string TypeIDs );
		Task<List<Tout>> GetsFunction<Tout>(string sql, List<SqlParameter> param) where Tout : class;
		IQueryable<tAlertEvent> GetAlertsByTimeZone(List<int> KDVRs, DateTime startDate, DateTime endDate, string TypeIDs = "");
		IQueryable<tAlertEvent> GetAlertsByTimeZone(IEnumerable<int> KDVRs, DateTime startDate, DateTime endDate, IEnumerable<byte> KAlertTypes);
		IQueryable<Tout> GetAlertsByTimeZone<Tout>(IEnumerable<int> KDVRs, byte? AlertSeverity, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string[] includes);
		IQueryable<View_Alerts_Acknowlegdement> GetAcknowlegdementAlerts(List<int> KDVRs, DateTime startDate, DateTime endDate, string TypeIDs = "");


		#endregion

		void DeleteLastAlert(Expression<Func<tAlertEventLast, bool>> filter);
		void UpdateAlertDetail(tAlertEventDetail alertDetail);
		int Save();
	}
}
