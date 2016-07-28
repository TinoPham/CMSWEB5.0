using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class BamModel
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<int> PacId { get; set; }
	}

	public class MetricParam
	{
		public DateTime SetDate { get; set; }
		public DateTime WeekStartDate { get; set; }
		public DateTime WeekEndDate { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<int> SitesKey { get; set; }
		public int ReportId { get; set; }
		public int ReportType { get; set; }
	}

	public class MetricDetail
	{
		public List<MetricDetailBase> Details { get; set; }
		public string Name { get; set; }
		public int ID { get; set; }
        public int UnitType { get; set; }
        public bool IsPrefix { get; set; }
        public decimal TotalWeek { get; set; }
        public string UnitName { get; set; }
        public int UnitRound { get; set; }
        public int Order { get; set; }
        public string ResourceKey { get; set; }
        public decimal Goal { get; set; }
        public decimal MaxGoal { get; set; }
        public decimal MinGoal { get; set; }
	}

	public sealed class MetricDetailBase
	{
		public DateTime Date { get; set; }
		public decimal Value { get; set; }
	}

	public class MetricSumamry
	{
		public int SiteKey { get; set; }
		public int MetricId { get; set; }
		public string Name { get; set; }
		public string ResourceKey { get; set; }
		public decimal Forcecast { get; set; }
		public decimal Actualy { get; set; }
		public decimal TotalWeekToDate { get; set; }
		public decimal TotalPeridToDate { get; set; }
		public decimal Goal { get; set; }
		public decimal TotalSales { get; set; }
		public decimal MaxGoal { get; set; }
		public decimal MinGoal { get; set; }
		public int TotalTrans { get; set; }
		public int TotalTraffic { get; set; }
		public string UnitName { get; set; }
		public int UnitType { get; set; }
		public bool IsPrefix { get; set; }
		public int Order { get; set; }
		public int UnitRound { get; set; }
	}
	public class MetricSumamryAll
	{
		public IEnumerable<MetricSumamry> DataTableSumary { get; set; }
	}
	public class ChartWeekAtAGlanceAll
	{
		public IEnumerable<ChartWeekAtAGlance> DataChartSumamry { get; set; }
		public ALertCompModel AVTData { get; set; }
		public ConvCompModel ConvData { get; set; }
	}

	public class MetricReportListModel
	{
		public int MetricId { get; set; }
		public string Name { get; set; }
		public string ResourceKey { get; set; }
		public int? Order { get; set; }
		public bool Active { get; set; }
	}

	public class ReportListModel
	{
		public int ReportId { get; set; }
		public int MetricId { get; set; }
		public string Name { get; set; }
		public string ResourceKey { get; set; }
		public int? Order { get; set; }
		public bool Active { get; set; }
	}

	public class MetricReportUpdateModel
	{
		public int ReportId { get; set; }
		public List<MetricReportListModel>  Metrics { get; set; }
	}

	public class MetricSumamryDetail
	{
		public IEnumerable<MetricDetail> DataTableSumaryDetail;
		//public IEnumerable<GraphMetricSumamryDetail> DataGraphSumamryDetail;
		public Goal_MetricModel GoalMetricConversion;
		public Goal_MetricModel GoalMetricTraffic;
		public ChartWeekAtAGlanceAll WAAGCharts { get; set; }
		public BAMDashboardCharts DashboardCharts { get; set; }
	}

    public class TableMetricSumamryDetail
    {
        public Nullable<System.DateTime> Date { get; set; }
        public int SiteKey { get; set; }
        public int MetricId { get; set; }
        public string Name { get; set; }
        public decimal? Dpo { get; set; }
        public decimal? TotalSales { get; set; }
        public decimal? TotalTrans { get; set; }
        public decimal? TotalTraffic { get; set; }
        public decimal? Conversion { get; set; }
        public decimal? Avt { get; set; }
        public decimal? Upd { get; set; }
    }

    public class GraphMetricSumamryDetail
    {
        public int SiteKey { get; set; }
        public int MetricId { get; set; }
        public decimal? Dpo { get; set; }
        public decimal? Conversion { get; set; }
    }
	public class BAMDashboardCharts
	{
		public IEnumerable<TrafficChartModel> TrafficChart { get; set; }
		public IEnumerable<GraphMetricSumamryDetail> DataGraphSumamryDetail { get; set; }
	}

    public class Goal_MetricModel
    {
        public string Name { get; set; }
        public decimal Goal { get; set; }
        public decimal MaxGoal { get; set; }
        public decimal MinGoal { get; set; }
        public string UnitName { get; set; }
        public int UnitType { get; set; }
        public bool IsPrefix { get; set; }
        public int Order { get; set; }
        public int UnitRound { get; set; }
    }

	public class ChartWeekAtAGlance
	{
		public DateTime DVRDate { get; set; }
		public string Label { get; set; }
		public decimal? Conversion { get; set; }
		public decimal? ConvForecast { get; set; }
		public decimal? Avt { get; set; }
	}

	public class ConvCompModel : ALertCompModel
	{
		public int Traffic { get; set; }
		public int Transaction { get; set; }
	}
	public class SitePACID 
	{
		public int SiteKey { get; set; }
		public int PACID { get; set; }
	}

	public class BamDataBySite : BamDataBase
	{
		public DateTime DVRDate { get; set; }
		public int SiteKey { get; set; }
		public string SiteName { get; set; }
		public int RegionKey { get; set; }
		public string RegionName { get; set; }
		public int CountSite { get; set; }
		//public decimal CountTrans { get; set; }
		//public decimal TrafficIn { get; set; }
		//public decimal TrafficOut { get; set; }
		//public decimal TotalAmount { get; set; }
		//public decimal Conversion { get; set; }
		public decimal Dpo { get; set; }
		public decimal Avt { get; set; }
		public decimal Upd { get; set; }
	}

	public class DriveThroughTotalSumBase
	{
		public decimal TotalCount { get; set; }
		public decimal TotalDwell { get; set; }
	}

	public class DriveThroughTotalSum
	{
		public DriveThroughTotalSumBase TotalSumRegion { get; set; }
		public DriveThroughTotalSumBase TotalSumSite { get; set; }
	}


	public class DriveThroughDataAll
	{
		public IEnumerable<DriveThroughDataSumary> DTData { get; set; }
		public DriveThroughTotalSum TotalSum { get; set; }
		//public IEnumerable<CountDwellChart> ChartData { get; set; }
		public CountDwellChartChartAll ChartData { get; set; }
	}
	public class DriveThroughBase
	{
		public DateTime Date { get; set; }
		public int Count { get; set; }
		public int Dwell { get; set; }
	}
	public class DriveThroughData : DriveThroughBase
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public bool isRegion { get; set; }
		public int ParentKey { get; set; }
		public int TimeIndex { get; set; }
		public string Title { get; set; }
	}
	public class DriveThroughDataSite : DriveThroughBase
	{
		public int SiteKey { get; set; }
		public string SiteName { get; set; }
		public int RegionKey { get; set; }
		public string RegionName { get; set; }
		public int CountSite { get; set; }
	}
	public class DriveThroughDataSumary : DriveThroughData
	{
		public IEnumerable<DriveThroughData> DetailData { get; set; }
		public IEnumerable<DriveThroughDataSumary> Sites { get; set; }
	}
	public class CountDwellChart : DriveThroughBase
	{
		public int ID { get; set; }
		public int TimeIndex { get; set; }
		public string Name { get; set; }

		public IEnumerable<CountDwellChart> Details { get; set; }
		//public IEnumerable<CountDwellChart> Regions { get; set; }
		//public IEnumerable<CountDwellChart> Sites { get; set; }
	}
	public class CountDwellChartChartAll
	{
		public IEnumerable<CountDwellChart> Regions { get; set; }
		public IEnumerable<CountDwellChart> Sites { get; set; }
	}

    #region Normalize

    public class NormalizeCountBase
    {
        public int Hour { get; set; }
        public int TotalNormalize { get; set; }
        public int TotalActualize { get; set; }
        public bool flag { get; set; }
    }

    public class Normalizes
    {
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public IEnumerable<NormalizeCountBase> NormalizeTime { get; set; }
    }

    public class NormalizeParam
    {
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public List<int> SitesKey { get; set; }
        public bool ReportNormalize { get; set; }
        public int Hour { get; set; }
    }

    #endregion
}
