using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IReportService
	{
		DataSet GetReportData();
		DataSet GetReportData(tbl_BAM_Metric_ReportUser report, NameValueCollection requestParms = null);
	}
}
