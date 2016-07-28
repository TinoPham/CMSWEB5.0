using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{

	public class AlertsParam {
		public string Dvrs { get; set; }
		public string Begin { get; set; }
		public string End { get; set; }
		public string TypeIDs { get; set; }
	}
	public class SiteMonitorParam
	{
		public int[] Dvrs { get; set; }
		public int[] AlertTypes { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
	}

	public class SiteMonitorModel
	{
		public int Kdvr { get; set; }
		public int AlertTypeId { get; set; }
		public int TotalAlert { get; set; }
		public DateTime TimeZone { get; set; }
		public int Kalert { get; set; }
	}

	public class SiteSensorsModel
	{
		public int Kdvr { get; set; }
		public DateTime? TimeZone { get; set; }
		public int TotalAlert { get; set; }
	}

	public class AlertSensorDetail
	{
		public AlertSensorDetail()
		{
			SnapShot = new List<string>();
		}

		public int Id { get; set; }
		public int ChannelNo { get; set; }
		public int KChannel { get; set; }
		public string ChannelName { get; set; }
		public string Description { get; set; }
		public string FullTime { get; set; }
		public string Time { get; set; }
		public long TimeZone { get; set; }
		public List<string> SnapShot { get; set; }
	}

	public class AlertEventType
	{
		public byte Id { get; set; }
		public byte KAlertSeverity { get; set; }
		public string Name { get; set; }
		public int? CmsWebType { get; set; }
		public int? CmsWebGroup { get; set; }
	}
}
