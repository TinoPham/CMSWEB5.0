using CMSWebApi.DataModels.ModelBinderProvider;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ICannedService
	{
		Task<List<Proc_Exception_GetReport_Result>> GetCannedReports(CannedRptParam param);
	}
}
