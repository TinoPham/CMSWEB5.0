using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IIOPCService
	{
		IEnumerable<Func_BAM_Normalize_IOPC_Count_Result> Func_BAM_Normalize_IOPC_Count(DateTime sdate, DateTime edate, IEnumerable<int> sites, int iopc_limit = 0, int LimitWeek = 10);
        Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> Func_BAM_TrueTraffic_Opportunity(DateTime sdate, DateTime edate, IEnumerable<int> sites);
        Task<List<Func_BAM_Normalize_IOPC_Count_Result>> Func_BAM_Normalize_IOPC_CountAsync(DateTime sdate, DateTime edate, IEnumerable<int> sites, int iopc_limit = 0, int LimitWeek = 10);
		Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>> Func_Fact_IOPC_Periodic_Hourly_Traffic_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites);
		Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> Func_Fact_IOPC_Periodic_Daily_Traffic_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites);
        
		IQueryable<Tout> Fact_IOPC_Periodic_Hourly_Traffic<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> sites, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector);
		IQueryable<Tout> Fact_IOPC_Periodic_Hourly_Traffic<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> pacids, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, bool>> combine, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector);
        IQueryable<Tout> GetNormalizeTraffics<Tout>(List<int> pacids, DateTime searchDate, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector, string[] includes);
        string UpdateReportNormalizeTraffics(IEnumerable<Fact_IOPC_Periodic_Hourly_Traffic> Normalizes);
		Task<List<CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Result>> CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(DateTime sdate, DateTime edate);

		Task<List<Proc_BAM_Get_DashBoard_ForeCast_Period_Result>> Proc_BAM_Get_DashBoard_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek);
		Task<List<Proc_DashBoard_Traffic_ForeCast_Result>> Proc_DashBoard_Traffic_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek);
		Task<List<Proc_DashBoard_Traffic_ForeCast_Hourly_Result>> Proc_DashBoard_Traffic_ForeCast_Hourly_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek);
		Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>> Func_Fact_IOPC_Periodic_Daily_Traffic_Channels(int pacID, DateTime sdate, DateTime edate);
		Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result>> Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels(int pacID, DateTime date);
		Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> Proc_DashBoard_Channel_EnableTrafficCount_Async(IEnumerable<int> pacids);
        Task<List<Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks_Result>> Proc_BAM_DashBoard_LaborHours_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek);
	}
}
