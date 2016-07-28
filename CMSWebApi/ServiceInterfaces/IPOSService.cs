using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using CMSWebApi.DataModels.DashBoardCache;
namespace CMSWebApi.ServiceInterfaces
{
	public interface IPOSService
	{
		IQueryable<Tout> GetFact_Transact<Tout>( Expression<Func<Fact_POS_Transact, bool>> filter, Expression<Func<Fact_POS_Transact, Tout>> selector, string[] includes );
		IQueryable<Tout> GetTransType<Tout>(Expression< Func<Dim_POS_TransactionType, bool>> filter, Expression<Func<Dim_POS_TransactionType, Tout>> selector, string[] includes);
		IQueryable<Tout> GetPOSPAC<Tout>(Expression<Func<tbl_POS_PACID, bool>> filter,Expression<Func<tbl_POS_PACID, Tout>> selector, string[] includes);
		Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversion_Async(string strPACID, DateTime From, DateTime To);
		Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetPOSConversionHourly_Async(string strPACID, DateTime From, DateTime To);
		Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversionAsync(string strPACID, DateTime From, DateTime To);
		Task<List<Func_Count_Exception_Trans_Result>>Count_Exception_Transaction_async(DateTime sdate, DateTime edate, IEnumerable<int>pacids, IEnumerable<int> exceptions);
		Task<List<Func_Count_Exception_Trans_Result>> Count_Exception_Transaction_async(DateTime sdate, DateTime edate, string pacids, string exceptions);
		IQueryable<Tout>Fact_POS_Periodic_Hourly_Transact<Tout>(DateTime sdate, DateTime edate, IEnumerable<int>pacids,Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector);
		IQueryable<Tout> Fact_POS_Periodic_Hourly_Transact<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> pacids,Expression<Func<Fact_POS_Periodic_Hourly_Transact, bool>> combine, Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector);
        IQueryable<Tout> GetNormalizeTrans<Tout>(List<int> lsPacids, DateTime searchDate, Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector, string[] includes);
        string UpdateReportNormalizeTrans(IEnumerable<Fact_POS_Periodic_Hourly_Transact> Normalizes);

		Task<List<CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Result>>CMSWeb_Cache_POS_Periodic_Hourly_Traffic( DateTime sdate, DateTime edate);
        Task<List<Func_BAM_LaborHourlyWorkingHour_Result>> Func_BAM_LaborHourlyWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> sites);
        Task<List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>> Func_BAM_LaborHourlyMinSecsWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> sites);

		Task<List<Func_DVR_HasNormalize_Result>> Func_DVR_HasNormalize_Async(IEnumerable<int> sites, DateTime sdate, DateTime edate);
		Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> GetPOSTransaction_Async(string strPACID, DateTime From, DateTime To);
	}
}
