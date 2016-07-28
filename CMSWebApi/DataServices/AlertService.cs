using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Data.SqlClient;

namespace CMSWebApi.DataServices
{
	public partial class AlertService : ServiceBase, IAlertService
	{
		private const int DvrSensorTriggered = 9;

		public AlertService(PACDMModel.Model.IResposity model) : base(model) { }

		public AlertService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		#region Alert types
		public Tout GetAlertType<Tout>(byte typeID, Expression<Func<tAlertType, Tout>> selector, string [] includes)
		{
			return base.FirstOrDefault<tAlertType, Tout>( item=> item.KAlertType == typeID, selector, includes);
		}
		public IQueryable<Tout> GetAlertTypes<Tout>(Expression<Func<tAlertType, bool>> filter, Expression<Func<tAlertType, Tout>> selector, string [] includes)
		{
			return base.Query<tAlertType, Tout>(filter, selector, includes);
		}

		#endregion

		#region AlertSeverity
		public Tout GetAlertSeverity<Tout>(byte typeID, Expression<Func<tAlertSeverity, Tout>> selector, string [] includes)
		{
			return base.FirstOrDefault<tAlertSeverity, Tout>(item => item.KAlertSeverity == typeID, selector, includes);
		}
		public IQueryable<Tout> GetAlertSeverities<Tout>(Expression<Func<tAlertSeverity, Tout>> selector, string [] includes)
		{
			return base.Query<tAlertSeverity, Tout>(null, selector, includes);
		}
		#endregion

		#region ALert

		public Tout GetAlert<Tout>(int alertid, Expression<Func<tAlertEvent, Tout>> selector, string[] includes)
		{
			return base.FirstOrDefault<tAlertEvent,Tout>(item => item.KAlertEvent == alertid, selector, includes);
		}

		public IQueryable<Tout> GetAlerts<Tout>(int? KDVR, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string [] includes)
		{
			return GetAlerts(null,KDVR, KAlertType, begin,end,selector, includes);
		}

		public IQueryable<Tout> GetAlerts<Tout>(Expression<Func<tAlertEvent, bool>> filter, Expression<Func<tAlertEvent, Tout>> selector, string[] includes = null)
		{
			return base.Query<tAlertEvent, Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetLastAlertEvents<Tout>(Expression<Func<tAlertEventLast, bool>> filter,Expression<Func<tAlertEventLast, Tout>> selector, string[] includes = null)
		{
			return base.QueryNoTrack<tAlertEventLast, Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetAlertEventDetailsAsNoTrack<Tout>(Expression<Func<tAlertEventDetail, bool>> filter, Expression<Func<tAlertEventDetail, Tout>> selector, string[] includes = null)
		{
			return base.QueryNoTrack<tAlertEventDetail, Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetAlertEventDetails<Tout>(Expression<Func<tAlertEventDetail, bool>> filter, Expression<Func<tAlertEventDetail, Tout>> selector, string[] includes = null)
		{
			return base.Query<tAlertEventDetail, Tout>(filter, selector, includes);
		}

		public IQueryable<SiteMonitorModel> GetAlertEventsByKdvrs(List<int> kdvrs, DateTime begin, DateTime end)
		{
			return base.DBModel.QueryNoTrack<tAlertEvent>(t => kdvrs.Contains((int)t.KDVR) && t.Time >= begin && t.Time <= end)
				.GroupBy(c => new { KAlertType = c.KAlertType, KDVR = c.KDVR })
				.Select(t => new
				{
					DVR = t.Key,
					TotalAlert = t.Count()
				}).Select(f=> new SiteMonitorModel()
				{
					Kdvr = (int)f.DVR.KDVR,
					AlertTypeId = f.DVR.KAlertType,
					TotalAlert = f.TotalAlert
				});
		}

		public IQueryable<SiteSensorsModel> GetSensorsEventsByKdvrs(List<int> kdvrs, DateTime begin, DateTime end)
		{
			return base.DBModel.QueryNoTrack<tAlertEvent>(t => t.KAlertType == DvrSensorTriggered && kdvrs.Contains((int)t.KDVR) && t.TimeZone >= begin && t.TimeZone <= end)
				.GroupBy(c => new { KDVR = c.KDVR, TimeZone = System.Data.Entity.DbFunctions.TruncateTime(c.TimeZone) })
				.Select(t => new
				{
					DVR = t.Key,
					TotalAlert = t.Count()
				}).Select(f => new SiteSensorsModel()
				{
					TimeZone = f.DVR.TimeZone,
					Kdvr = (int)f.DVR.KDVR,
					TotalAlert = f.TotalAlert
				});
		}

		public IQueryable<Tout> GetAlerts<Tout>(byte? AlertSeverity, int? KDVR, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string [] includes, bool byTimeZone = false)
		{
			ParameterExpression pram = Expression.Parameter(typeof(tAlertEvent), "it");
			BinaryExpression expression = null;
			BinaryExpression current = null;
			expression = base.NotEqual<int?>(pram, Defines.ALert.KDVR, null);
			if( KDVR.HasValue)
			{
				current = base.Equal<Int32>(pram, Defines.ALert.KDVR, KDVR.Value);
				expression = Expression.AndAlso(expression, current);
			}
			if( KAlertType.HasValue)
			{
				current = base.Equal<byte>(pram, Defines.ALert.KAlertType, KAlertType.Value);
				expression = Expression.AndAlso(expression, current);
			}

			current = base.GreaterThanOrEqual<DateTime>(pram, byTimeZone ? Defines.ALert.TimeZone : Defines.ALert.Time, begin);
			expression = Expression.AndAlso(expression, current);

			current = base.LessThanOrEqual<DateTime>(pram, byTimeZone ? Defines.ALert.TimeZone : Defines.ALert.Time, end);
			expression = Expression.AndAlso(expression, current);

			var lambda = Expression.Lambda<Func<tAlertEvent, bool>>(expression, pram);
			if( AlertSeverity.HasValue)
			{
				IQueryable<tAlertType>  types = GetAlertTypes<tAlertType>( item => item.KAlertSeverity == AlertSeverity.Value, it => it, null);
				IQueryable<tAlertEvent> alerts = DBModel.Query<tAlertEvent>( lambda, includes);
				IQueryable<tAlertEvent> result = alerts.Join<tAlertEvent, tAlertType, byte, tAlertEvent>(types, al => al.KAlertType, t => t.KAlertType, (al, t) => al);
				return result.Select(selector);

			}
			else
				return base.Query<tAlertEvent,Tout>(lambda, selector, includes);

		}

		public IQueryable<Tout> GetAlerts<Tout>(IEnumerable<int> KDVRs, byte? AlertSeverity, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string [] includes)
		{
			IQueryable<tAlertEvent> alerts = GetAlerts<tAlertEvent>(AlertSeverity, null, KAlertType, begin, end, item => item, includes);
			return alerts.Join<tAlertEvent, int, int, tAlertEvent>(KDVRs, alert => alert.KDVR.Value, kdvr => kdvr, (alert, kdvr) => alert).Select(selector);
		}

		public IQueryable<Tout> GetAlertsbyTypes<Tout>(IEnumerable<int> KDVRs, IEnumerable<byte> KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector)
		{
			IQueryable<tAlertEvent> alerts = DBModel.Query<tAlertEvent>( alt => alt.Time >= begin && alt.Time <= end);
			IQueryable<tAlertEvent> altresult =  alerts;
			if( KDVRs != null && KDVRs.Any())
				altresult = altresult.Join(KDVRs, alt => !alt.KDVR.HasValue ? 0 : alt.KDVR.Value, kdvr => kdvr, (alt, kdvr) => alt);
			if (KAlertType != null && KAlertType.Any())
				altresult = altresult.Join(KAlertType, alt => alt.KAlertType, alttype => alttype, (alt, alttype) => alt);

			return altresult.Select(selector);
		}

		#region Get Alerts by Timezone
		public IQueryable<tAlertEvent> GetAlertsByTimeZone(IEnumerable<int> KDVRs, DateTime startDate, DateTime endDate, IEnumerable<byte> KAlertTypes)
		{
			string[] includes = new string[1];
			includes[0] = typeof(tAlertType).Name;
			if (KAlertTypes == null || !KAlertTypes.Any())
			{
				return DBModel.Query<tAlertEvent>(item => item.KDVR.HasValue, includes).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value));
			}
			else
			{
				return DBModel.Query<tAlertEvent>(item => item.KDVR.HasValue, includes).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value) && KAlertTypes.Any(item1 => item1 == item.KAlertType));
			}
		}
		public IQueryable<tAlertEvent> GetAlertsByTimeZone(List<int> KDVRs, DateTime startDate, DateTime endDate, string TypeIDs = "")
		{
			if (string.IsNullOrEmpty(TypeIDs))
			{
				return GetAlertsByTimeZone(KDVRs, startDate, endDate, (IEnumerable<byte>)null);
			}
			else
			{
				byte[] typeIDs = TypeIDs.Split(new char[] { Utils.Consts.DECIMAL_SIGN }).Select(item => byte.Parse(item)).ToArray();
				return GetAlertsByTimeZone(KDVRs, startDate, endDate, typeIDs);
			}
			/*
			string[]includes = new string[1];
			includes[0] = typeof(tAlertType).Name;
			if (string.IsNullOrEmpty(TypeIDs))
			{
				return DBModel.Query<tAlertEvent>(item => item.KDVR.HasValue, includes).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value));
			}
			else
			{
				int[] typeIDs = TypeIDs.Split(new char[] { Utils.Consts.DECIMAL_SIGN }).Select(item => int.Parse(item)).ToArray();

				return DBModel.Query<tAlertEvent>(item => item.KDVR.HasValue, includes).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value) && typeIDs.Any(item1 => item1 == item.KAlertType));
			}*/
		}
		public IQueryable<Tout> GetAlertsByTimeZone<Tout>(IEnumerable<int> KDVRs, byte? AlertSeverity, byte? KAlertType, DateTime begin, DateTime end, Expression<Func<tAlertEvent, Tout>> selector, string[] includes)
		{
			IQueryable<tAlertEvent> alerts = GetAlerts<tAlertEvent>(AlertSeverity, null, KAlertType, begin, end, item => item, includes, true);
			return alerts.Join<tAlertEvent, int, int, tAlertEvent>(KDVRs, alert => alert.KDVR.Value, kdvr => kdvr, (alert, kdvr) => alert).Select(selector);
		}
		#endregion

		public Task<List<Func_DVR_Offline_Result>> GetDVR_On_Offline_Async(IEnumerable<int> kdvrs, DateTime datetime, bool isoffline, int byHours = 0, int keepaliveint = 0)
		{
			List<SqlParameter> prams = GetDVROnOffLineParams(datetime, kdvrs, isoffline, keepaliveint);
			string sql = string.Empty;
			if (byHours == 0)
			{
				sql = Format_SqlCommand(SQLFunctions.Func_DVR_Offline, prams);//string.Format(SQLFunctions.Func_DVR_Offline, ParameterNames(prams));
			}
			else
			{
				sql = String.Format(SQLFunctions.Func_DVR_Offline_ByHours, prams[0].ParameterName, prams[1].ParameterName, prams[2].ParameterName, prams[3].ParameterName, byHours * 60);//string.Format(SQLFunctions.Func_DVR_Offline, ParameterNames(prams));
			}
			Task<List<Func_DVR_Offline_Result>> result = DBModel.ExecWithStoreProcedureAsync<Func_DVR_Offline_Result>(sql, prams);
			return result;
		}

		public IQueryable<Tout> GetLastALerts<Tout>(int tAlertEvent, IEnumerable<int> kdvrs, IEnumerable<byte> AlertTypes, Expression<Func<tAlertEvent, Tout>> selector, string [] includes)
		{
			IQueryable<tAlertEvent> query = base.QueryNoTrack<tAlertEvent, tAlertEvent>(it => it.KAlertEvent > tAlertEvent, it=> it, null);
			IQueryable<tAlertEvent> alt_dvr;
			
			alt_dvr = (kdvrs == null || !kdvrs.Any()) ? query : alt_dvr = query.Join( kdvrs, alt=> alt.KDVR, dvr => dvr, (alt, dvr)=> alt);
			if( AlertTypes == null || !AlertTypes.Any())
				return alt_dvr.Select(selector);
			return alt_dvr.Join( AlertTypes, alt => alt.KAlertType, alttype => alttype, (alt, alttype)=> alt).Select(selector);


		}
		public Task<List<CMSWeb_Cache_ALert_Result>> CMSWeb_Cache_ALert(DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = CMSWeb_Cache_ALert_Param(sdate, edate);
			IEnumerable<string>iefields = GetProperties<CMSWeb_Cache_ALert_Result>();
			
			string  sql = string.Format( SQLFunctions.Func_CMSWeb_Cache_ALert, base.ParameterNames( prams));
			return DBModel.ExecWithStoreProcedureAsync<CMSWeb_Cache_ALert_Result>(sql, prams);
		}
		#endregion

		#region privates
		List<SqlParameter> CMSWeb_Cache_ALert_Param( DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("From", System.Data.SqlDbType.DateTime) { Value = sdate});
			prams.Add(new SqlParameter("To", System.Data.SqlDbType.DateTime) { Value = edate });
			return prams;
		}
		List<SqlParameter> GetDVROnOffLineParams(DateTime date, IEnumerable<int> kdvrs, bool offline, int keepaliveint)
		{
			List<SqlParameter>prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("kdvrs", System.Data.SqlDbType.VarChar) { Value = string.Join<int>(",",kdvrs)});
			prams.Add(new SqlParameter("date", System.Data.SqlDbType.DateTime) { Value = date});
			prams.Add(new SqlParameter("keepaliveint", System.Data.SqlDbType.Int) { Value =  keepaliveint});
			prams.Add(new SqlParameter("offline", System.Data.SqlDbType.Bit) { Value =  offline ? 1 : 0 });
			return prams;
		}
		#endregion

		public Task<List<Tout>> GetsFunction<Tout>(string sql, List<SqlParameter> param) where Tout : class
		{
			return DBModel.ExecWithStoreProcedureAsync<Tout>(sql, param);
		}

		public  Task<List<AlertModelSummary>> AlertsSummary(List<int> kdvrs, DateTime startDate, DateTime endDate,string TypeIDs)
		{
			try
			{
				List<SqlParameter> prams = AlertSummaySqlParam(kdvrs, startDate, endDate, TypeIDs);
				IEnumerable<string> iefields = GetProperties<AlertModelSummary>();
				string sql = string.Format(SQLFunctions.Func_Alerts_SummaryBySites, base.ParameterNames(prams));
				return DBModel.ExecWithStoreProcedureAsync<AlertModelSummary>(sql, prams);
			}
			catch(Exception ex)
			{
				return null;
			}
		}

		public void DeleteLastAlert(Expression<Func<tAlertEventLast, bool>> filter)
		{			
			DBModel.DeleteWhere<tAlertEventLast>(filter);
		}

		public void UpdateAlertDetail(tAlertEventDetail alertDetail)
		{
			DBModel.Update<tAlertEventDetail>(alertDetail);
		}

		public int Save()
		{
			return DBModel.Save();
		}

		List<SqlParameter> AlertSummaySqlParam(List<int> kdvrs, DateTime startDate, DateTime endDate,string TypeIDs)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter(Defines.ALert.KDVR, System.Data.SqlDbType.VarChar) { Value = string.Join<int>(",", kdvrs) });
			prams.Add(new SqlParameter(Defines.ALert.BeginDate, System.Data.SqlDbType.DateTime) { Value = startDate });
			prams.Add(new SqlParameter(Defines.ALert.EndDate, System.Data.SqlDbType.DateTime) { Value = endDate });
			prams.Add(new SqlParameter(Defines.ALert.KAlertType, System.Data.SqlDbType.NVarChar) { Value = TypeIDs });
			return prams;
		}

		public IQueryable<View_Alerts_Acknowlegdement> GetAcknowlegdementAlerts(List<int> KDVRs, DateTime startDate, DateTime endDate, string TypeIDs = "")
		{
			if (string.IsNullOrEmpty(TypeIDs))
				return DBModel.Query<View_Alerts_Acknowlegdement>(item => item.KDVR.HasValue,  null).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value));
			else
			{
				int[] typeIDs = TypeIDs.Split(new char[] { Utils.Consts.DECIMAL_SIGN }).Select(item => int.Parse(item)).ToArray();
				return DBModel.Query<View_Alerts_Acknowlegdement>(item => item.KDVR.HasValue, null).Where(item => item.TimeZone >= startDate && item.TimeZone <= endDate && KDVRs.Contains(item.KDVR.Value) && typeIDs.Any(item1 => item1 == item.KAlertType));
			}
		}

	}
}
