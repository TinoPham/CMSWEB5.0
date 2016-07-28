using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices
{
	public class SaleReportsService : ServiceBase, ISaleReportsService
	{
		public SaleReportsService(IResposity model) : base(model) { }

		public SaleReportsService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversion(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To);
			string proc = string.Format(SQLProceduces.Proc_DashBoard_Conversion, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Proc_DashBoard_Conversion_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Conversion_Result>(proc, pram.ToArray());
			return result;
		}

		public Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetPOSConversionHourly(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = POSConversionParams(strPACID, From, To);
			string proc = string.Format(SQLProceduces.Proc_DashBoard_Conversion_Hourly, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Proc_DashBoard_Conversion_Hourly_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_DashBoard_Conversion_Hourly_Result>(proc, pram.ToArray());
			return result;
		}

		private List<SqlParameter> POSConversionParams(string strPACID, DateTime From, DateTime To)
		{
			List<SqlParameter> pram = new List<SqlParameter>();
			pram.Add(new SqlParameter("PDateFrom", SqlDbType.DateTime) { Value = From, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PDateTo", SqlDbType.DateTime) { Value = To, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter("PPACID_IDs", SqlDbType.VarChar, 0) { Value = strPACID, Direction = ParameterDirection.Input });
			return pram;
		}
	}
}
