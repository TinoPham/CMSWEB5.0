using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;
namespace CMSWebApi.BusinessServices.ReportBusiness.Interfaces
{
	public interface IPOSBusinessService
	{
		Task<IEnumerable<ConvSitesChartModel>> ConversionChartBySites(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites, int stateID, int top);
		Task<ConversionChartModel> ConversionChartModel(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites);
		Task<IEnumerable<ConvMapChartModel>> ConversionMapchart(DateTime sdate, DateTime edate, IEnumerable<UserSiteDvrChannel> sites);
		Task<ALertCompModel> POSConversionCompare(IEnumerable<UserSiteDvrChannel> uSites, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate);
		Task<ALertCompModel> POSExceptionCompare(IEnumerable<int> pacids, IEnumerable<int> exceptions, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate);
		Task<ALertCompModel> TotalSaleCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate);
		Task<ALertCompModel> TransactionCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate);
		Task<List<Proc_DashBoard_Conversion_Result>> GetConversionAsync(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync);
		Task<List<Proc_DashBoard_Conversion_Result>> GetConversionAsync(DateTime sdate, DateTime edate, IEnumerable<int> pacids);
		Task<List<Proc_DashBoard_Conversion_Hourly_Result>> GetConversionHourlyAsync(DateTime sdate, DateTime edate, string pacids);
	}
}
