using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using PACDMModel.Model;
namespace CMSWebApi.BusinessServices.ReportBusiness.Interfaces
{
	public interface IIOPCBusinessService
	{
		Task<IEnumerable<TrafficChartModel>> ChartTraffic(DateTime sdate, DateTime edate, PeriodType ptype, IEnumerable<int> pacids, UserContext user);
		Task<ALertCompModel> TrafficCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate);
		Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> GetTrueTrafficOpportunity(DateTime sdate, DateTime edate, IEnumerable<int> pacids);
		Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> DashBoard_Channel_EnableTrafficCount(IEnumerable<int> pacids, ref bool isasync);
	}
}
