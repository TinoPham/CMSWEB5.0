using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{
	public class MetricModel
	{
		public int MListID {get; set;}
		public string MetricName {get; set;}
		public DateTime MListEditedDate {get; set;}
		public int? ParentID {get; set;}
		public string MetricMeasure {get; set;}
		public int? CreateBy {get; set;}
		public bool? isDefault {get; set;}
		public string UUsername { get; set; }

		public List<MetricSiteListModel> MetricSiteList { get; set; }
	}

	public class MetricListModel : MetricModel
	{
		public List<MetricModel> MetricList { get; set; }
	}

	public class MetricSiteListModel
	{
		public int MListID { get; set; }
		public int SiteID { get; set; }
		public DateTime CreatedDate { get; set; }
	}

	//public class MetricData : TransactionalInformation
	//{
	//	public MetricSiteModel[] MetricSite { get; set; }
	//}
}
