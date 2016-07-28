using System.Linq;
using PACDMModel.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IBamMetricService
	{
		IQueryable<tbl_BAM_Metric_ReportUser> GetMetrics();
		IQueryable<tbl_BAM_Metric> GetMetricDefaults();
		IQueryable<tbl_BAM_Metric_ReportList> GetReportLists();
		IQueryable<tbl_BAM_Metric_ReportUser> GetMetricReportUsers();
		void InsertMetricReportUser(tbl_BAM_Metric_ReportUser usermetric);
		void UpdateMetricReportUser(tbl_BAM_Metric_ReportUser usermetric);
		void DeleteMetricReportUser(tbl_BAM_Metric_ReportUser usermetric);
		void DeleteMetricReportUser(int reportId, int userId);
		int SaveChange();
		Task<List<Func_BAM_DriveThroughMonthly_Result>> Func_BAM_DriveThroughMonthly_Async(string PACIDs, DateTime sdate, DateTime edate);
		Task<List<Func_BAM_DriveThroughHourly_Result>> Func_BAM_DriveThroughHourly_Async(string PACIDs, DateTime sdate, DateTime edate);
	}
}