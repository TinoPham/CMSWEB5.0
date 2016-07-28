using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class DistributionBase
	{
		public int PACID { get; set; }
		public DateTime Date { get; set; }
		public int ID { get; set; }
		public string Name { get; set; }
		public int Count { get; set; }
		public int DWell { get; set; }
	}

	public class DistributionModel:DistributionBase
	{
		public int TimeIndex { get; set; }
		public string Title { get; set; }
		public int ParentID { get; set; }
		public string ParentName { get; set; }
		public int CountRegion { get; set; }

		public int KDVR { get; set; }
		public int ChannelNo { get; set; }
	}

	public class DistributionSummary: DistributionModel
	{
		private IEnumerable<DistributionModel> _dataDetail = null;
		public IEnumerable<DistributionModel> DataDetail
		{
			get { return _dataDetail; }
			set { _dataDetail = value; }
		}

		public IEnumerable<DistributionModel> Regions { get; set; }
	}

	public class DistributionTotalSum
	{
		public double TotalCount { get; set; }
		public double TotalDwell { get; set; }
	}

	public class DistributionDataAll
	{
		public IEnumerable<DistributionSummary> SummaryData { get; set; }

		public DistributionTotalSum TotalSum { get; set; }
		public DistributionChartAll ChartData { get; set; }
		//public IEnumerable<CountDwellChart> ChartData { get; set; }
	}

	public class DistributionChartAll
	{
		public IEnumerable<DistChartData> Regions { get; set; }
		public IEnumerable<DistChartData> Sites { get; set; }
	}

	public class DistChartData : DriveThroughBase
	{
		public int ID { get; set; }
		public int TimeIndex { get; set; }
		public string Name { get; set; }
		public int ItemCount { get; set; }

		public int KDVR { get; set; }
		public int ChannelNo { get; set; }

		public DriveThroughBase DataYTD { get; set; }
		public IEnumerable<DistChartData> Details { get; set; }
	}

	public class DistributionQueueBase
	{
		public int id { get; set; }
		public string Name { get; set; }
	}
	public class AddQueueParam
	{
		public string siteKeys { get; set; }
		public IEnumerable<DistributionQueue> QueueData { get; set; }
	}
	public class DistributionQueue : DistributionQueueBase
	{
		public int cid { get; set; }
		public IEnumerable<DistributionQueueBase> areas { get; set; }
	}
	public class DistributionQueueRegion
	{
		public IEnumerable<DistributionQueue> QueueData { get; set; }
		public IEnumerable<DistributionQueueBase> AllRegions { get; set; }
	}
}
