using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	
	public class EmailAlertConfig
	{
		public byte AlertType { get ;set;}
		public bool Included { get ;set;}
	}
	public class AlertReportActive
	{
		public int ReportKey { get; set; }
		public int FreqTypeID { get; set; }
		public DateTime StartRunDate { get; set; }
		public DateTime LastRunDate { get; set; }
		public DateTime NextRunDate { get; set; }
	}

	public class AlertReportResult : AlertReportActive
	{
		public Dictionary<int,string>Result{ get ;set;}
	}
	public class EmailSettingModel : AlertReportActive
    {
         public  int UserKey { get; set; }
         public List<int> Recipients { get; set; }
         public List<string> EmailList { get; set; }
         public List<int> SiteList { get; set; }
         public List<AlertEventType> Alerts  { get; set; }
         public  string ReportName { get; set; }
         public string EmailSubject { get; set; }
         public int FreqCount { get; set; }
         public bool  EnableEmailReporting { get; set; }
         public string RunTime { get; set; }
         public string  BAMReportXML { get; set; }
         public int ReportType { get; set; }
    }
}
