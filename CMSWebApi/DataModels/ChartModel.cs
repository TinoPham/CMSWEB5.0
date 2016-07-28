using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	/*
	public class ChartParam
	{
		public string siteKeys { get; set; }
		public int type { get; set; }
		public string date { get; set; }
		public int period { get; set; }
		public string severity { get; set; }
	}

	public class ChartData
	{
		public string label { get; set; }
		public string value { get; set; }
	}
	public class TrafficData : ChartData
	{
		public int countIn { get; set; }
		public int countOut { get; set; }
		public int forecast { get; set; }
	}
	public class ConversionData : ChartData
	{
		public decimal goalMin { get; set; }
		public decimal goalMax { get; set; }
	}
	public class PacIDs
	{
		public int PACID_ID { get; set; }
	}
	*/

	public class ConversionChartModel : ColumnChartModel
	{
		public double goalMin { get; set; }
		public double goalMax { get; set; }
		public int trans { get; set; }
		public int traffic { get; set; }
	}

	public class TrafficChartModel : ColumnChartModel
	{
		public int countIn { get; set; }
		public int countOut { get; set; }
		public int forecast { get; set; }
		public IEnumerable<DVRPACChannel> Channels{ get ;set;}
	}

	public class ConvMapChartModel : ConvSitesChartModel
	{
		public string Code { get; set; }
		public int StateID { get; set; }
	}
	public class ConvSitesChartModel : ColumnChartModel
	{
		public decimal LastYear { get; set; }
	}
	public class ChartWithImageModel : ColumnChartModel
	{
		public int KDVR { get; set; }
		public byte[] ImageSrc { get; set; }
	}
	public class ColumnChartModel
	{
		public string Label{ get ;set;}
		public decimal Value{ get ;set;}
	}

	//public class SitePACID
	//{
	//	public int SiteKey { get; set; }
	//	public int PACID { get; set; }
	//}
	public class SiteConversion
	{
		public int SiteKey { get; set; }
		public decimal Conv { get; set; }
	}
}
