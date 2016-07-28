using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using PACDMModel.Model;
using CMSWebApi.DataModels;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ISaleReportsService
	{
		Task<List<Proc_DashBoard_Conversion_Result>> GetPOSConversion(string strPACID, DateTime From, DateTime To);
		Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetPOSConversionHourly(string strPACID, DateTime From, DateTime To);
	}
}
