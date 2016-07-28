using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels.Alert
{
	public class EmailAlertModel
	{
		public string SiteName { get; set; }
		public int SiteKey { get; set; }
		public IEnumerable<EmailALertSiteDVR> DVRAlerts{ get ;set;}
	}
	
	public class EmailALertSiteDVR
	{
		public int KDVR{get; set;}
		public string ServerID{ get ;set;}
		public IEnumerable<EmailAlertDVRModel> Alerts { get; set; }
	}
	public class EmailAlertDVRModel
	{
		public int KAlertEvent { get; set; }
		public byte KAlertType { get; set; }
		public string KAlertName { get; set; }
		public string Description { get; set; }
		public System.DateTime TimeZone { get; set; }
		public IEnumerable<int> KChannels{ get ;set;}
		public string ImageUrl{ get ;set;}
	}

	public class AlertModel
	{
		public int KAlertEvent { get; set; }
		public byte KAlertType { get; set; }
		public Nullable<int> KDVR { get; set; }
		public Nullable<System.DateTime> TimeZone { get; set; }
		public string DVRUser { get; set; }
		public string Description { get; set; }
		public System.DateTime Time { get; set; }
		public Nullable<int> Channel { get; set; }
		public string CMSUser { get; set; }
		public Nullable<short> Status { get; set; }
		public Nullable<int> UpdateTime { get; set; }
		public Nullable<short> Rate { get; set; }
		public string Note { get; set; }
		public string Image { get; set; }
		public Nullable<long> ImageTime { get; set; }
	}
}
