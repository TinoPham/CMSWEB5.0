using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels.DashBoardCache;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public class POSService : ServiceBase, IPOSService
	{
		public POSService(IResposity model) : base(model){}

		public POSService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<Tout> Fact_POS_Periodic_Hourly_Transact<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> pacids,Expression<Func<Fact_POS_Periodic_Hourly_Transact, bool>> combine, Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector)
		{

			IQueryable<Fact_POS_Periodic_Hourly_Transact> query = base.QueryNoTrack<Fact_POS_Periodic_Hourly_Transact, Fact_POS_Periodic_Hourly_Transact>(it => it.DVRDateKey >= sdate.Date, item => item, null);
			var query_pac = (pacids == null || !pacids.Any()) ? query : query.Join<Fact_POS_Periodic_Hourly_Transact, int, int, Fact_POS_Periodic_Hourly_Transact>(pacids, it => it.PACID, p => p, (it, p) => it);
			var query_combine = combine == null? query_pac : query_pac.Where( combine);


			return query_combine.Select(selector);

		}
		public IQueryable<Tout>Fact_POS_Periodic_Hourly_Transact<Tout>(DateTime sdate, DateTime edate, IEnumerable<int>pacids,Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector)
		{
				return Fact_POS_Periodic_Hourly_Transact<Tout>(sdate, edate, pacids, null, selector);
			//IQueryable<Fact_POS_Periodic_Hourly_Transact> query = base.QueryNoTrack<Fact_POS_Periodic_Hourly_Transact, Fact_POS_Periodic_Hourly_Transact>(it => it.DVRDateKey >= sdate.Date && it.DVRDateKey <= edate.Date, item => item, null);
			//if( pacids != null && pacids.Any())
			//	return query.Join<Fact_POS_Periodic_Hourly_Transact,int, int, Fact_POS_Periodic_Hourly_Transact>( pacids, it => it.PACID, p=> p, (it,p) => it).Select(selector);

			//return query.Select(selector);
		}
		public IQueryable<Tout> GetFact_Transact<Tout>(Expression<Func<Fact_POS_Transact, bool>> filter, Expression<Func<Fact_POS_Transact, Tout>> selector, string [] includes)
		{
			return base.Query<Fact_POS_Transact,Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetPOSPAC<Tout>(Expression<Func<tbl_POS_PACID, bool>> filter, Expression<Func<tbl_POS_PACID, Tout>> selector, string[] includes)
		{
			return QueryNoTrack<tbl_POS_PACID, Tout>(filter, selector, includes);
		}

		public IQueryable<Tout> GetTransType<Tout>(Expression< Func<Dim_POS_TransactionType, bool>> filter, Expression<Func<Dim_POS_TransactionType, Tout>> selector, string[] includes)
		{
		  return base.Query<Dim_POS_TransactionType,Tout>(filter, selector, includes);
		}

        public IQueryable<Tout> GetNormalizeTrans<Tout>(List<int> pacids, DateTime searchDate, Expression<Func<Fact_POS_Periodic_Hourly_Transact, Tout>> selector, string[] includes) 
        {
            return base.Query<Fact_POS_Periodic_Hourly_Transact, Tout>(si => (si.DVRDateKey == searchDate && pacids.Contains(si.PACID)), selector, includes);
        }

        public string UpdateReportNormalizeTrans(IEnumerable<Fact_POS_Periodic_Hourly_Transact> Normalizes)
        {
            foreach (Fact_POS_Periodic_Hourly_Transact model in Normalizes) 
            {
                DBModel.Update<Fact_POS_Periodic_Hourly_Transact>(model);
            }
            return DBModel.Save() > 0 ? CMSWebApi.DataServices.ServiceBase.Defines.ConstNormalizes.Succecss : CMSWebApi.DataServices.ServiceBase.Defines.ConstNormalizes.Error;
        }

		public Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversion_Async(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To);
			string proc = string.Format(SQLProceduces.Proc_DashBoard_Conversion, pram.Select(p => p.ParameterName).ToArray()); //"EXEC Proc_DashBoard_Conversion @PDateFrom, @PDateTo, @PPACID_IDs";
			Task<List<Proc_DashBoard_Conversion_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Conversion_Result>(proc, pram.ToArray());
			return result;
		}
		public Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversionAsync(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To);
			string proc = string.Format(SQLProceduces.Proc_DashBoard_Conversion, pram.Select(p => p.ParameterName).ToArray()); //"EXEC Proc_DashBoard_Conversion @PDateFrom, @PDateTo, @PPACID_IDs";
			Task<List<Proc_DashBoard_Conversion_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Conversion_Result>(proc, pram.ToArray());
			return result;
		}

		public Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetPOSConversionHourly_Async(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To);
			string proc = string.Format(SQLProceduces.Proc_DashBoard_Conversion_Hourly, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Proc_DashBoard_Conversion_Hourly_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Conversion_Hourly_Result>(proc, pram.ToArray());
			return result;
		}

		public Task<List<Func_Count_Exception_Trans_Result>> Count_Exception_Transaction_async(DateTime sdate, DateTime edate, string pacids, string exceptions)
		{
			List<SqlParameter>prams = Count_Exception_Transaction_Params(sdate, edate, pacids, exceptions);
			string sql = string.Format(SQLFunctions.Func_Count_Exception_Trans, prams.Select( p=> p.ParameterName).ToArray());
			Task<List<Func_Count_Exception_Trans_Result>> result = DBModel.ExecWithStoreProcedureAsync<Func_Count_Exception_Trans_Result>(sql, prams.ToArray());
			return result;
		}
		public Task<List<Func_Count_Exception_Trans_Result>> Count_Exception_Transaction_async(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<int> exceptions)
		{
			return Count_Exception_Transaction_async(sdate, edate, string.Join<int>(",", pacids), string.Join<int>( ",",exceptions));
		}

		public Task<List<CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Result>> CMSWeb_Cache_POS_Periodic_Hourly_Traffic(DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Param(sdate, edate);
			IEnumerable<string> iefields = GetProperties<CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Result>();
			string sql = string.Format(SQLFunctions.Func_CMSWeb_Cache_ALert, base.ParameterNames(prams));
			return DBModel.ExecWithStoreProcedureAsync<CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Result>(sql, prams);

		}

		public Task<List<Func_DVR_HasNormalize_Result>> Func_DVR_HasNormalize_Async(IEnumerable<int> sites, DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = Func_DVR_HasNormalize_Param(sdate, edate, string.Join<int>(",", sites));//new List<SqlParameter>();
			string sql = Format_SqlCommand(SQLFunctions.Func_DVR_HasNormalize, prams);
			//IEnumerable<Func_DVR_HasNormalize_Result> result = DBModel.ExecWithStoreProcedureAsync<Func_DVR_HasNormalize_Result>(sql, prams);
			return DBModel.ExecWithStoreProcedureAsync<Func_DVR_HasNormalize_Result>(sql, prams);
		}

		public Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> GetPOSTransaction_Async(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSTransactionParams(strPACID, From, To);
			string proc = string.Format(SQLFunctions.Func_Fact_POS_Periodic_Daily_Transact, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Func_Fact_POS_Periodic_Daily_Transact_Result>> result = DBModel.ExecWithStoreProcedureAsync<Func_Fact_POS_Periodic_Daily_Transact_Result>(proc, pram.ToArray());
			return result;
		}

		List<SqlParameter> CMSWeb_Cache_POS_Periodic_Hourly_Traffic_Param(DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("From", System.Data.SqlDbType.Date) { Value = sdate.Date });
			prams.Add(new SqlParameter("To", System.Data.SqlDbType.Date) { Value = edate.Date });
			return prams;
		}

		List<SqlParameter> Count_Exception_Transaction_Params(DateTime sdate, DateTime edate, string pacids, string exceptions)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("sdate", SqlDbType.SmallDateTime) { Value = sdate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("edate", SqlDbType.SmallDateTime) { Value = edate, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("exceptionIDs", SqlDbType.VarChar, 0) { Value = exceptions, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.VarChar, 0) { Value = pacids, Direction = ParameterDirection.Input });
			return pram;
		}
		private List<SqlParameter> POSConversionParams(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("PDateFrom", SqlDbType.DateTime) { Value = From, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PDateTo", SqlDbType.DateTime) { Value = To, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PPACID_IDs", SqlDbType.VarChar, 0) { Value = strPACID, Direction = ParameterDirection.Input});
			return pram;
		}
		List<SqlParameter> Func_DVR_HasNormalize_Param(DateTime sdate, DateTime edate, string kdvrs)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("From", System.Data.SqlDbType.Date) { Value = sdate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("To", System.Data.SqlDbType.Date) { Value = edate.Date, Direction = ParameterDirection.Input });
			prams.Add(new SqlParameter("kdvrs", System.Data.SqlDbType.VarChar, 0) { Value = kdvrs, Direction = ParameterDirection.Input });
			return prams;
		}
		private List<SqlParameter> POSTransactionParams(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("From", SqlDbType.Date) { Value = From.Date, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("To", SqlDbType.Date) { Value = To.Date, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PACIDs", SqlDbType.VarChar, 0) { Value = strPACID, Direction = ParameterDirection.Input });
			return pram;
		}

        public Task<List<Func_BAM_LaborHourlyWorkingHour_Result>> Func_BAM_LaborHourlyWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> sites)
        {
            List<SqlParameter> sqlParams = Func_BAM_LaborHourlyWorkingHour_Params(sdate, edate, sites);
            string sql = string.Format(SQLFunctions.Func_BAM_LaborHourlyWorkingHour, sqlParams.Select(p => p.ParameterName).ToArray());
            Task<List<Func_BAM_LaborHourlyWorkingHour_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_BAM_LaborHourlyWorkingHour_Result>(sql, sqlParams.ToArray());

            return resCount;
        }

        private List<SqlParameter> Func_BAM_LaborHourlyWorkingHour_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>(3);
            sqlParams.Add(new SqlParameter("FromDate", System.Data.SqlDbType.DateTime) { Value = sdate });
            sqlParams.Add(new SqlParameter("ToDate", System.Data.SqlDbType.DateTime) { Value = edate });
            sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
            return sqlParams;
        }

        public Task<List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>> Func_BAM_LaborHourlyMinSecsWorkingHour(DateTime sdate, DateTime edate, IEnumerable<int> sites)
        {
            List<SqlParameter> sqlParams = Func_BAM_LaborHourlyMinSecsWorkingHour_Params(sdate, edate, sites);
            string sql = string.Format(SQLFunctions.Func_BAM_LaborHourlyMinSecsWorkingHour, sqlParams.Select(p => p.ParameterName).ToArray());
            Task<List<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_BAM_LaborHourlyMinSecsWorkingHour_Result>(sql, sqlParams.ToArray());

            return resCount;
        }

        private List<SqlParameter> Func_BAM_LaborHourlyMinSecsWorkingHour_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>(3);
            sqlParams.Add(new SqlParameter("FromDate", System.Data.SqlDbType.DateTime) { Value = sdate });
            sqlParams.Add(new SqlParameter("ToDate", System.Data.SqlDbType.DateTime) { Value = edate });
            sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
            return sqlParams;
        }
        
	}
}
