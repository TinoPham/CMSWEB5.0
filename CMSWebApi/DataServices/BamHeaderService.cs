using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
    public class BamHeaderService : ServiceBase, IBamHeaderService
	{
        public BamHeaderService(PACDMModel.Model.IResposity model) : base(model) { }

		public BamHeaderService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

        private List<SqlParameter> Proc_BAM_Get_Header_Stores_Count_Params(int UserID, DateTime sdate)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("UserID", System.Data.SqlDbType.Int) { Value = UserID });
            sqlParams.Add(new SqlParameter("Date", System.Data.SqlDbType.Date) { Value = sdate.Date });
           
            return sqlParams;
        }

        public Task<List<Proc_BAM_Get_Header_Stores_Count_Result>> GetCountDataHeader(int UserID, DateTime sdate)
        {
            List<SqlParameter> sqlparams = Proc_BAM_Get_Header_Stores_Count_Params(UserID, sdate);
            string sql = string.Empty;
            sql = string.Format(SQLProceduces.Proc_BAM_Get_Header_Stores_Count, sqlparams.Select(p => p.ParameterName).ToArray());

            Task<List<Proc_BAM_Get_Header_Stores_Count_Result>> resCount = DBModel.ExecWithStoreProcedureAsync<Proc_BAM_Get_Header_Stores_Count_Result>(sql, sqlparams.ToArray());
            return resCount;
        }

	}
}
