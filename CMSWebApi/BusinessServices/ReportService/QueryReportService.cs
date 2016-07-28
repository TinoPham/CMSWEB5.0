using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportService
{
	public class QueryReportService : ReportBase, IReportService
	{
		public QueryReportService(UserContext userContext, IResposity dbModel)
			: base(userContext, dbModel)
		{

		}

		public System.Data.DataSet GetReportData()
		{
			throw new NotImplementedException();
		}

		public System.Data.DataSet GetReportData(tbl_BAM_Metric_ReportUser report, NameValueCollection requestParms = null)
		{
			var queryStr = "select * from tCMSWeb_UserList";

			var resultData = DBModel.ExecuteQuery(queryStr,CommandType.Text, null);
			resultData.Tables[0].TableName = report.tbl_BAM_Metric_ReportList.ReportName;
			return resultData;
		}
	}
}
