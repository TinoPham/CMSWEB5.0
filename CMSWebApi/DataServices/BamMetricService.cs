using System.Data.Entity;
using System.Linq;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Data;

namespace CMSWebApi.DataServices
{
	public class BamMetricService : ServiceBase, IBamMetricService
	{
		public BamMetricService(PACDMModel.Model.IResposity model) : base(model) { }

		public BamMetricService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<tbl_BAM_Metric_ReportUser> GetMetrics()
		{
			return DBModel.QueryNoTrack<tbl_BAM_Metric_ReportUser>().Include(t => t.tbl_BAM_Metric);
		}

		public IQueryable<tbl_BAM_Metric> GetMetricDefaults()
		{
			return DBModel.QueryNoTrack<tbl_BAM_Metric>();
		}

		public IQueryable<tbl_BAM_Metric_ReportList> GetReportLists()
		{
			return DBModel.QueryNoTrack<tbl_BAM_Metric_ReportList>();
		}

		public IQueryable<tbl_BAM_Metric_ReportUser> GetMetricReportUsers()
		{
			return DBModel.QueryNoTrack<tbl_BAM_Metric_ReportUser>().Include(t=>t.tbl_BAM_Metric_ReportList);
		}

		public void UpdateMetricReportUser(tbl_BAM_Metric_ReportUser usermetric)
		{
			DBModel.Update<tbl_BAM_Metric_ReportUser>(usermetric);
		}

		public void InsertMetricReportUser(tbl_BAM_Metric_ReportUser usermetric)
		{
			DBModel.Insert<tbl_BAM_Metric_ReportUser>(usermetric);
		}

		public void DeleteMetricReportUser(tbl_BAM_Metric_ReportUser usermetric)
		{
			DBModel.Delete<tbl_BAM_Metric_ReportUser>(usermetric);
		}

		public void DeleteMetricReportUser(int reportId, int userId)
		{
			DBModel.DeleteWhere<tbl_BAM_Metric_ReportUser>(t=> t.ReportID == reportId && t.UserID == userId);
		}

		public int SaveChange()
		{
			return DBModel.Save();
		}
		List<SqlParameter> Func_BAM_DriveThroughMonthly_Param(DateTime sdate, DateTime edate, string PAC_ID)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("StartDate", System.Data.SqlDbType.Date) { Value = sdate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("EndDate", System.Data.SqlDbType.Date) { Value = edate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar, 0) { Value = PAC_ID, Direction = ParameterDirection.Input });
			return prams;
		}
		public Task<List<Func_BAM_DriveThroughMonthly_Result>> Func_BAM_DriveThroughMonthly_Async(string PACIDs, DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = Func_BAM_DriveThroughMonthly_Param(sdate, edate, PACIDs);//new List<SqlParameter>();
			string sql = Format_SqlCommand(SQLFunctions.Func_BAM_DriveThroughMonthly, prams);
			//IEnumerable<Func_DVR_HasNormalize_Result> result = DBModel.ExecWithStoreProcedureAsync<Func_DVR_HasNormalize_Result>(sql, prams);
			return DBModel.ExecWithStoreProcedureAsync<Func_BAM_DriveThroughMonthly_Result>(sql, prams);
		}


		List<SqlParameter> Func_BAM_DriveThroughHourly_Param(DateTime sdate, DateTime edate, string PAC_ID)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("StartDate", System.Data.SqlDbType.Date) { Value = sdate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("EndDate", System.Data.SqlDbType.Date) { Value = edate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar, 0) { Value = PAC_ID, Direction = ParameterDirection.Input });
			return prams;
		}
		public Task<List<Func_BAM_DriveThroughHourly_Result>> Func_BAM_DriveThroughHourly_Async(string PACIDs, DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = Func_BAM_DriveThroughHourly_Param(sdate, edate, PACIDs);//new List<SqlParameter>();
			string sql = Format_SqlCommand(SQLFunctions.Func_BAM_DriveThroughHourly, prams);
			//IEnumerable<Func_DVR_HasNormalize_Result> result = DBModel.ExecWithStoreProcedureAsync<Func_DVR_HasNormalize_Result>(sql, prams);
			return DBModel.ExecWithStoreProcedureAsync<Func_BAM_DriveThroughHourly_Result>(sql, prams);
		}
	}
}
