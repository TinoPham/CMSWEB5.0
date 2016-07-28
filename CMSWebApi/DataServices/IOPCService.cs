using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public class IOPCService : ServiceBase, IIOPCService
	{
		public IOPCService(IResposity model):base(model){}

		public IOPCService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>> Func_Fact_IOPC_Periodic_Daily_Traffic_Channels(int pacID, DateTime sdate, DateTime edate)
		{
			List<SqlParameter> sqlparams = Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Params(pacID, sdate, edate);
			string sql = string.Format(SQLFunctions.Func_Fact_IOPC_Periodic_Daily_Traffic_Channels, sqlparams.Select(p => p.ParameterName).ToArray());
			Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>(sql, sqlparams.ToArray());
			return resCount;
		}
		private List<SqlParameter> Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Params(int pacID, DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("PacID", System.Data.SqlDbType.Int) { Value = pacID });
			prams.Add(new SqlParameter("sDate", System.Data.SqlDbType.Date) { Value = sdate });
			prams.Add(new SqlParameter("eDate", System.Data.SqlDbType.Date) { Value = edate });
			return prams;
		}
		public Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result>> Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels(int pacID, DateTime date)
		{
			List<SqlParameter> sqlparams = Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Params(pacID, date);
			string sql = string.Format(SQLFunctions.Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels, sqlparams.Select(p => p.ParameterName).ToArray());
			Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

		private List<SqlParameter> Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Params(int pacID, DateTime date)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("PacID", System.Data.SqlDbType.Int) { Value = pacID });
			prams.Add(new SqlParameter("Date", System.Data.SqlDbType.Date) { Value = date });
			
			return prams;
		}

		public IQueryable<Tout> Fact_IOPC_Periodic_Hourly_Traffic<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> pacids, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector)
		{
			return Fact_IOPC_Periodic_Hourly_Traffic<Tout>(sdate, edate,pacids,null, selector);
			//IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> query = base.QueryNoTrack<Fact_IOPC_Periodic_Hourly_Traffic, Fact_IOPC_Periodic_Hourly_Traffic>(it => it.DVRDateKey >= sdate.Date && it.DVRDateKey <= edate.Date, item => item, null);
			//if( pacids != null && pacids.Any())
			//	return query.Join<Fact_IOPC_Periodic_Hourly_Traffic, int, int, Fact_IOPC_Periodic_Hourly_Traffic>(pacids, it => it.PACID, p => p, (it, p) => it).Select(selector);
			//return query.Select( selector);
		}
		public IQueryable<Tout> Fact_IOPC_Periodic_Hourly_Traffic<Tout>(DateTime sdate, DateTime edate, IEnumerable<int> pacids, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, bool>> combine, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector)
		{
			IQueryable<Fact_IOPC_Periodic_Hourly_Traffic> query = base.QueryNoTrack<Fact_IOPC_Periodic_Hourly_Traffic, Fact_IOPC_Periodic_Hourly_Traffic>(it => it.DVRDateKey >= sdate.Date && it.DVRDateKey <= edate.Date, item => item, null);
			var query_pac = pacids == null || !pacids.Any()? query : query.Join<Fact_IOPC_Periodic_Hourly_Traffic, int, int, Fact_IOPC_Periodic_Hourly_Traffic>(pacids, it => it.PACID, p => p, (it, p) => it);

			var query_combine = combine == null ? query_pac : query_pac.Where( combine);
			return query_combine.Select(selector);
		}

        public IQueryable<Tout> GetNormalizeTraffics<Tout>(List<int> pacids, DateTime searchDate, Expression<Func<Fact_IOPC_Periodic_Hourly_Traffic, Tout>> selector, string[] includes)
        {
            return base.Query<Fact_IOPC_Periodic_Hourly_Traffic, Tout>(si => (si.DVRDateKey == searchDate && pacids.Contains(si.PACID)), selector, includes);
        }

        public string UpdateReportNormalizeTraffics(IEnumerable<Fact_IOPC_Periodic_Hourly_Traffic> Normalizes)
        {
            foreach (Fact_IOPC_Periodic_Hourly_Traffic model in Normalizes)
            {
                DBModel.Update<Fact_IOPC_Periodic_Hourly_Traffic>(model);
            }
            return DBModel.Save() > 0 ? CMSWebApi.DataServices.ServiceBase.Defines.ConstNormalizes.Succecss : CMSWebApi.DataServices.ServiceBase.Defines.ConstNormalizes.Error;
        }

		public IEnumerable<Func_BAM_Normalize_IOPC_Count_Result> Func_BAM_Normalize_IOPC_Count(DateTime sdate, DateTime edate, IEnumerable<int> sites, int iopc_limit = 0, int LimitWeek = 10)
		{
			List<SqlParameter> sqlParams = Func_BAM_Normalize_IOPC_Count_Params(sdate, edate, sites, iopc_limit, LimitWeek);
			string sql = string.Format(SQLFunctions.Func_BAM_Normalize_IOPC_Count, sqlParams.Select(p => p.ParameterName).ToArray());
			IEnumerable<Func_BAM_Normalize_IOPC_Count_Result> resCount = DBModel.ExecWithStoreProcedure<Func_BAM_Normalize_IOPC_Count_Result>(sql, sqlParams.ToArray());


			return resCount;
		}

        public Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> Func_BAM_TrueTraffic_Opportunity(DateTime sdate, DateTime edate, IEnumerable<int> sites)
        {
            List<SqlParameter> sqlParams = Func_BAM_TrueTraffic_Opportunity_Params(sdate, edate, sites);
            string sql = string.Format(SQLFunctions.Func_BAM_TrueTraffic_Opportunity, sqlParams.Select(p => p.ParameterName).ToArray());
            Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_BAM_TrueTraffic_Opportunity_Result>(sql, sqlParams.ToArray());

            return resCount;
        }

		public Task<List<Func_BAM_Normalize_IOPC_Count_Result>> Func_BAM_Normalize_IOPC_CountAsync(DateTime sdate, DateTime edate, IEnumerable<int> sites, int iopc_limit = 0, int LimitWeek = 10)
		{
			List<SqlParameter> sqlParams = Func_BAM_Normalize_IOPC_Count_Params( sdate, edate, sites, iopc_limit, LimitWeek);
			
			string sql = string .Format( SQLFunctions.Func_BAM_Normalize_IOPC_Count, sqlParams.Select( p => p.ParameterName).ToArray())  ;
			Task<List<Func_BAM_Normalize_IOPC_Count_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_BAM_Normalize_IOPC_Count_Result>(sql, sqlParams.ToArray());
			return resCount;
		}
		public Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>> Func_Fact_IOPC_Periodic_Hourly_Traffic_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites)
		{
			List<SqlParameter> sqlparams = Func_Fact_IOPC_Periodic_Hourly_Traffic_Params(sdate, edate, sites);
			string sql = string.Format(SQLFunctions.Func_Fact_IOPC_Periodic_Hourly_Traffic, sqlparams.Select(p => p.ParameterName).ToArray());
			Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

		public Task<List<CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Result>> CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic(DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Param(sdate, edate);
			IEnumerable<string> iefields = GetProperties<CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Result>();
			string sql = string.Format(SQLFunctions.Func_CMSWeb_Cache_ALert, base.ParameterNames(prams));
			return DBModel.ExecWithStoreProcedureAsync<CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Result>(sql, prams);

		}

		public Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> Func_Fact_IOPC_Periodic_Daily_Traffic_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites)
		{
			List<SqlParameter> sqlparams = Func_Fact_IOPC_Periodic_Daily_Traffic_Params(sdate, edate, sites);
			string sql = string.Format(SQLFunctions.Func_Fact_IOPC_Periodic_Daily_Traffic, sqlparams.Select(p => p.ParameterName).ToArray());
			Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

		public Task<List<Proc_BAM_Get_DashBoard_ForeCast_Period_Result>> Proc_BAM_Get_DashBoard_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek)
		{
			List<SqlParameter> sqlparams = Proc_DashBoard_Traffic_ForeCast_Params(sdate, edate, sites, formular, limitWeek);

			string sql = string.Empty;
			if (formular == (byte) ForecastFormular.FiveWeek)
			{
				sql = string.Format(SQLProceduces.Proc_BAM_Get_DashBoard_ForeCast_5Weeks, sqlparams.Select(p=>p.ParameterName).ToArray());
			}
			else
			{
				sql= string.Format(SQLProceduces.Proc_BAM_Get_DashBoard_ForeCast_Period, sqlparams.Select(p=>p.ParameterName).ToArray());
			}
			return DBModel.ExecWithStoreProcedureAsync<Proc_BAM_Get_DashBoard_ForeCast_Period_Result>(sql, sqlparams.ToArray());

		}

		public Task<List<Proc_DashBoard_Traffic_ForeCast_Result>> Proc_DashBoard_Traffic_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek)
		{
			List<SqlParameter> sqlparams = Proc_DashBoard_Traffic_ForeCast_Params(sdate, edate, sites, formular, limitWeek);
			//string sql = string.Format(SQLFunctions.Func_BAM_Get_DashBoard_ForeCast, sqlparams.Select(p => p.ParameterName).ToArray());
			string sql = string.Empty;
			if (formular == (byte)ForecastFormular.FiveWeek)
			{
				sql = string.Format(SQLProceduces.Proc_DashBoard_Traffic_ForeCast_5Weeks, sqlparams.Select(p => p.ParameterName).ToArray());
			}
			else
			{
				sql = string.Format(SQLProceduces.Proc_DashBoard_Traffic_ForeCast, sqlparams.Select(p => p.ParameterName).ToArray());
			}
			Task<List<Proc_DashBoard_Traffic_ForeCast_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Traffic_ForeCast_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

        public Task<List<Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks_Result>> Proc_BAM_DashBoard_LaborHours_ForeCast_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek)
        {
            List<SqlParameter> sqlparams = Proc_DashBoard_LaborHours_ForeCast_Params(sdate, edate, sites, formular, limitWeek);
            //string sql = string.Format(SQLFunctions.Func_BAM_Get_DashBoard_ForeCast, sqlparams.Select(p => p.ParameterName).ToArray());
            string sql = string.Empty;
            if (formular == (byte)ForecastFormular.FiveWeek)
            {
                sql = string.Format(SQLProceduces.Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks, sqlparams.Select(p => p.ParameterName).ToArray());
            }
            else
            {
                sql = string.Format(SQLProceduces.Proc_BAM_DashBoard_LaborHour_ForeCast_Period, sqlparams.Select(p => p.ParameterName).ToArray());
            }
            Task<List<Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Proc_BAM_DashBoard_LaborHour_ForeCast_5Weeks_Result>(sql, sqlparams.ToArray());
            return resCount;
        }

		public Task<List<Proc_DashBoard_Traffic_ForeCast_Hourly_Result>> Proc_DashBoard_Traffic_ForeCast_Hourly_Async(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte formular, int limitWeek)
		{
			List<SqlParameter> sqlparams = Proc_DashBoard_Traffic_ForeCast_Params(sdate, edate, sites, formular, limitWeek);
			string sql = string.Empty;
			if (formular == (byte)ForecastFormular.FiveWeek)
			{
				sql = string.Format(SQLProceduces.Proc_DashBoard_Traffic_ForeCast_5Weeks_Hourly, sqlparams.Select(p => p.ParameterName).ToArray());
			}
			else
			{
				sql = string.Format(SQLProceduces.Proc_DashBoard_Traffic_ForeCast_Hourly, sqlparams.Select(p => p.ParameterName).ToArray());
			}
			Task<List<Proc_DashBoard_Traffic_ForeCast_Hourly_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Traffic_ForeCast_Hourly_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

		public Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> Proc_DashBoard_Channel_EnableTrafficCount_Async(IEnumerable<int> pacids)
		{
			List<SqlParameter> sqlparams = new List<SqlParameter>();
			sqlparams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = pacids == null ? null : string.Join(",", pacids) });

			string sql = string.Format(SQLProceduces.Proc_DashBoard_Channel_EnableTrafficCount, sqlparams.Select(p => p.ParameterName).ToArray());

			Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Channel_EnableTrafficCount_Result>(sql, sqlparams.ToArray());
			return resCount;
		}

		List<SqlParameter> CMSWeb_Cache_IOPC_Periodic_Hourly_Traffic_Param(DateTime sdate, DateTime edate)
		{
			List<SqlParameter> prams = new List<SqlParameter>();
			prams.Add(new SqlParameter("From", System.Data.SqlDbType.Date) { Value = sdate.Date });
			prams.Add(new SqlParameter("To", System.Data.SqlDbType.Date) { Value = edate.Date });
			return prams;
		}


		private List<SqlParameter> Func_Fact_IOPC_Periodic_Hourly_Traffic_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>(5);
			sqlParams.Add(new SqlParameter("From", System.Data.SqlDbType.SmallDateTime) { Value = sdate.Date });
			sqlParams.Add(new SqlParameter("To", System.Data.SqlDbType.SmallDateTime) { Value = edate.Date });
			sqlParams.Add(new SqlParameter("sHour", System.Data.SqlDbType.TinyInt) { Value = sdate.Hour });
			sqlParams.Add(new SqlParameter("eHour", System.Data.SqlDbType.TinyInt) { Value = edate.Hour });
			sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
			
			return sqlParams;
		}

		private List<SqlParameter> Func_BAM_Normalize_IOPC_Count_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites, int iopc_limit = 0, int LimitWeek = 10)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>(5);
			sqlParams.Add(new SqlParameter("sdate", System.Data.SqlDbType.SmallDateTime) { Value = sdate });
			sqlParams.Add(new SqlParameter("edate", System.Data.SqlDbType.SmallDateTime) { Value = edate });
			sqlParams.Add(new SqlParameter("pacids", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
			sqlParams.Add(new SqlParameter("iopc_limit", System.Data.SqlDbType.Int) { Value = iopc_limit });
			sqlParams.Add(new SqlParameter("LimitWeek", System.Data.SqlDbType.Int) { Value = LimitWeek });
			return sqlParams;
		}

        private List<SqlParameter> Func_BAM_TrueTraffic_Opportunity_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>(3);
            sqlParams.Add(new SqlParameter("FromDate", System.Data.SqlDbType.DateTime) { Value = sdate });
            sqlParams.Add(new SqlParameter("ToDate", System.Data.SqlDbType.DateTime) { Value = edate });
            sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites)});
			return sqlParams;
		}
		private List<SqlParameter> Func_Fact_IOPC_Periodic_Daily_Traffic_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("From", System.Data.SqlDbType.SmallDateTime) { Value = sdate.Date });
			sqlParams.Add(new SqlParameter("To", System.Data.SqlDbType.SmallDateTime) { Value = edate.Date });
			sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });

			return sqlParams;
		}

		private List<SqlParameter> Proc_DashBoard_Traffic_ForeCast_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte fomular, int limitWk = 5)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("Form", System.Data.SqlDbType.SmallDateTime) { Value = sdate.Date });
			sqlParams.Add(new SqlParameter("To", System.Data.SqlDbType.SmallDateTime) { Value = edate.Date });
			sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
			if (fomular == (byte)ForecastFormular.FiveWeek)
			{
				sqlParams.Add(new SqlParameter("limitWeek", System.Data.SqlDbType.Int) { Value = limitWk });
			}
			return sqlParams;
		}

        private List<SqlParameter> Proc_DashBoard_LaborHours_ForeCast_Params(DateTime sdate, DateTime edate, IEnumerable<int> sites, byte fomular, int limitWk = 5)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("Form", System.Data.SqlDbType.DateTime) { Value = sdate.Date });
            sqlParams.Add(new SqlParameter("To", System.Data.SqlDbType.DateTime) { Value = edate.Date });
            sqlParams.Add(new SqlParameter("PACIDs", System.Data.SqlDbType.VarChar) { Value = sites == null ? null : string.Join(",", sites) });
            if (fomular == (byte)ForecastFormular.FiveWeek)
            {
                sqlParams.Add(new SqlParameter("limitWeek", System.Data.SqlDbType.Int) { Value = limitWk });
            }
            return sqlParams;
        }
	}
}
