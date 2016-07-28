using CMSWebApi.DataModels.ModelBinderProvider;
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
	public class CannedService: ServiceBase, ICannedService
	{
		public CannedService(IResposity model) : base(model) { }
		public CannedService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public Task<List<Proc_Exception_GetReport_Result>> GetCannedReports(CannedRptParam param)
		{
			List<SqlParameter> pram = ConvertCannedParams(param);
			string proc = string.Format(SQLProceduces.Proc_Exception_GetReport, pram.Select(p => p.ParameterName).ToArray());
			Task<List<Proc_Exception_GetReport_Result>> result = DBModel.ExecWithStoreProcedureAsync<Proc_Exception_GetReport_Result>(proc, pram);
			return result;
		}
		private List<SqlParameter> ConvertCannedParams(CannedRptParam param)
		{
			#region mark
			//@ReportID int,
			//@DateFrom datetime,
			//@DateTo datetime,
			//@PACIDs varchar(max),

			//@PaymentIDs varchar(max) = '',
			//@PaymentIDs_AND bit = 1,

			//@RegIDs varchar(max) = '',
			//@RegIDs_AND bit = 1,

			//@EmpIDs varchar(max) = '',
			//@EmpIDs_AND bit = 1,

			//@TransNB bigint = NULL,
			//@TransNB_OP char(2) = NULL,
			//@TransNB_AND bit = 1,

			//@TransAmount money = NULL,
			//@TransAmount_OP char(2) = NULL,
			//@TransAmount_AND bit = 1,

			//@MaxRows int = 100000-- Max number of rows return
			#endregion
			List<SqlParameter> pram = new List<SqlParameter>();
			string pacids = string.Join(",", param.PACIDs);
			string paymentids = string.Join(",", param.PaymentIDs);
			string regids = string.Join(",", param.RegIDs);
			string empids = string.Join(",", param.EmpIDs);
			bool typeMatch = true; //default filter data by TransDate
			decimal transNB = param.TransNB_OP.Contains("Any") ? 0 : param.TransNB;
			decimal transAmount = param.TransAmount_OP.Contains("Any") ? 0 : param.TransAmount;

			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<int>(() => param.ReportID), SqlDbType.Int) { Value = param.ReportID, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<DateTime>(() => param.DateFrom), SqlDbType.DateTime) { Value = param.DateFrom, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<DateTime>(() => param.DateTo), SqlDbType.DateTime) { Value = param.DateTo, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<List<int>>(() => param.PACIDs), SqlDbType.VarChar, 0) { Value = pacids, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<List<int>>(() => param.PaymentIDs), SqlDbType.VarChar, 0) { Value = paymentids, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.PaymentIDs_AND), SqlDbType.Bit, 0) { Value = param.PaymentIDs_AND, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<List<int>>(() => param.RegIDs), SqlDbType.VarChar, 0) { Value = regids, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.RegIDs_AND), SqlDbType.Bit, 0) { Value = param.RegIDs_AND, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<List<int>>(() => param.EmpIDs), SqlDbType.VarChar, 0) { Value = empids, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.EmpIDs_AND), SqlDbType.Bit, 0) { Value = param.EmpIDs_AND, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<decimal>(() => param.TransNB), SqlDbType.Decimal, 0) { Value = transNB, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<string>(() => param.TransNB_OP), SqlDbType.Char, 2) { Value = param.TransNB_OP, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.TransNB_AND), SqlDbType.Bit, 0) { Value = param.TransNB_AND, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<decimal>(() => param.TransAmount), SqlDbType.Money, 0) { Value = transAmount, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<string>(() => param.TransAmount_OP), SqlDbType.Char, 2) { Value = param.TransAmount_OP, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.TransAmount_AND), SqlDbType.Bit, 0) { Value = param.TransAmount_AND, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<bool>(() => param.TypeMatch), SqlDbType.Bit, 0) { Value = typeMatch, Direction = ParameterDirection.Input });
			pram.Add(new SqlParameter(Commons.Utils.GetPropertyName<int>(() => param.MaxRows), SqlDbType.Int, 0) { Value = param.MaxRows, Direction = ParameterDirection.Input });

			return pram;
		}
	}
}
