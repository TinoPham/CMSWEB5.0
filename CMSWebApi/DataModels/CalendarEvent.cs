using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class CalendarEventSimple
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class CalendarEvent : CalendarEventSimple
	{
		//public int ID { get; set; }
		//public string Name { get; set; }
		//public DateTime StartDate { get; set; }
		//public DateTime EndDate { get; set; }

		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public int CreatedBy { get; set; }
		public int RemindID { get; set; }
		public int Color { get; set; }
		public byte RemindBefore { get; set; }
		public bool EventTrigger { get; set; }
		public short RelatedFunction { get; set; }
		public short ScheduleType { get; set; }
		public bool NormalizeAllSite { get; set; }
		public DateTime NormalizeTrigger { get; set; }

		public IEnumerable<int> SiteIDs { get; set; }
		public IEnumerable<int> RecipientIDs { get; set; }
	}

	//public class CalendarEventData : TransactionalInformation
	//{
	//	public CalendarEvent CalEvent { get; set; }
	//}

	
}
