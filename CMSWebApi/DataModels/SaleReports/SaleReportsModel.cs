using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class BamDataBase
	{
		public decimal Conversion { get; set; }
		public decimal CountTrans { get; set; }
		public decimal TrafficIn { get; set; }
		public decimal TrafficOut { get; set; }
		public decimal TotalAmount { get; set; }
	}

	public class SaleReportData : BamDataBase
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public bool isRegion { get; set; }
		public DateTime Date { get; set; }
		public int Hour { get; set; }
		public int ParentKey { get; set; }
		public string Title { get; set; }
		public int TimeIndex { get; set; }
	}

	public class SaleReportSummary : SaleReportData
	{
		private IEnumerable<SaleReportData> _dataDetail = null;
		private IEnumerable<SaleReportSummary> _siteData = null;
		
		public IEnumerable<SaleReportData> DataDetail 
		{
			get { return _dataDetail; }
			set { _dataDetail = value; }
		}
		public IEnumerable<SaleReportSummary> Sites
		{
			get { return _siteData; }
			set { _siteData = value; }
		}
	}

	public class SaleReportChart : BamDataBase
	{
		public DateTime Date { get; set; }
		public int ID { get; set; }
		public int TimeIndex { get; set; }
		public string Name { get; set; }

		public IEnumerable<SaleReportChart> Details { get; set; }
		//public IEnumerable<SaleReportChart> Sites { get; set; }
	}
	public class SaleReportChartAll
	{
		public IEnumerable<SaleReportChart> Regions { get; set; }
		public IEnumerable<SaleReportChart> Sites { get; set; }
	}

	public class SaleRptTotalSumBase
	{
		public decimal TotalConv { get; set; } 
		public decimal TotalTrans { get; set; }
		public decimal TotalOut { get; set; }
		public decimal TotalIn { get; set; }
		public decimal TotalAmout { get; set; }
	}

	public class SaleRptTotalSum
	{
		public SaleRptTotalSumBase TotalRegion { get; set; }
		public SaleRptTotalSumBase TotalSite { get; set; }
	}

	

	public class SaleReportDataAll
	{
		public IEnumerable<SaleReportSummary> SummaryData { get; set; }

		public SaleRptTotalSum TotalSum { get; set; }
		public SaleReportChartAll ChartData { get; set; } //public IEnumerable<SaleReportChart> ChartData { get; set; }
	}
}
