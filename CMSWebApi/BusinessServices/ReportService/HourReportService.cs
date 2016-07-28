using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportService
{
	public class HourReportService : ReportBase, IReportService
	{
		public HourReportService(UserContext userContext, IResposity dbModel)
			: base(userContext, dbModel)
		{

		}

		public DataSet GetReportData()
		{
			DataTable table1 = new DataTable("CustomReportTest");
			table1.Columns.Add("name");
			table1.Columns.Add("id");
			table1.Rows.Add("sam", 1);
			table1.Rows.Add("mark", 2);

			DataTable table2 = new DataTable("resource");
			table2.Columns.Add("id");
			table2.Columns.Add("medication");
			table2.Rows.Add(1, "atenolol");
			table2.Rows.Add(2, "amoxicillin");

			// Create a DataSet and put both tables in it.
			DataSet set = new DataSet("CustomReportTest");
			set.Tables.Add(table1);
			set.Tables.Add(table2);
			return set;
		}

		public DataSet GetReportData(tbl_BAM_Metric_ReportUser report, NameValueCollection requestParms = null)
		{
			DataTable table1 = new DataTable("CustomReportTest");
			table1.Columns.Add("name");
			table1.Columns.Add("id");
			table1.Rows.Add("sam", 1);
			table1.Rows.Add("mark", 2);

			DataTable table2 = new DataTable("resource");
			table2.Columns.Add("id");
			table2.Columns.Add("medication");
			table2.Rows.Add(1, "atenolol");
			table2.Rows.Add(2, "amoxicillin");

			// Create a DataSet and put both tables in it.
			DataSet set = new DataSet("CustomReportTest");
			set.Tables.Add(table1);
			set.Tables.Add(table2);
			return set;
		}
	}
}
