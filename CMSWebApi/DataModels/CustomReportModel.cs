using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class CustomReportModel
	{
		public int ReportId { get; set; }
		public int UserId { get; set; }
		public string ReportName { get; set; }
		public string ReportResourceName { get; set; }
		public string ReportLocation { get; set; }
		public string ServiceName { get; set; }
		public DataSet Data { get; set; }
	}
}
