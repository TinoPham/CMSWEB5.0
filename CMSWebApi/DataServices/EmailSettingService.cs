using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public class EmailSettingService : ServiceBase, IEmailSettingService
	{
		public EmailSettingService(IResposity db):base(db){}

		public EmailSettingService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }
		public IEnumerable<Func_CMSWebReport_Schedule_Result> CurrentReportSchedule(DateTime date)
		{
			SqlParameter p = base.SQLFunctionParamater("date", date);
			string sql = string.Format(SQLFunctions.Func_CMSWebReport_Schedule, p.ParameterName);// "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@UserID)";
			IEnumerable<Func_CMSWebReport_Schedule_Result> result = DBModel.ExecWithStoreProcedure<Func_CMSWebReport_Schedule_Result>(sql, p);
			return result;
		}
	}
}
