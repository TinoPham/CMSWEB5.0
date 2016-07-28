using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.DriveThrough)]
	public class AccessTransDriveThrough : AccessTransBase
	{
		[XmlElement(ConstEnums.TD_ID)]
		public int TD_ID { get; set;}

		[XmlElement(ConstEnums.StartTime)]
		public string StartTime {get; set;}

		[XmlElement(ConstEnums.StartDate)]
		public string StartDate{ get; set;}

		[XmlElement(ConstEnums.EndTime)]
		public string EndTime { get; set;}

		[XmlElement(ConstEnums.EndDate)]
		public string EndDate { get; set;}

		[XmlElement(ConstEnums.ExternalCamera)]
		public int ExternalCamera { get; set;}

		[XmlElement(ConstEnums.PACID)]
		public string PACID { get; set; }

		[XmlElement(ConstEnums.InternalCamera)]
		public int InternalCamera { get; set;}

		[XmlElement(ConstEnums.Function)]
		public int Function { get; set;}

		[XmlIgnore]
		public DateTime? StartDateTime{ get{ return AccessTransBase.toDateTime( StartDate, StartTime); }}
		[XmlIgnore]
		public DateTime? EndDateTime { get { return AccessTransBase.toDateTime(EndDate, EndTime); } }


	}
}
