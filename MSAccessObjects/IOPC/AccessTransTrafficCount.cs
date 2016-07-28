using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.TrafficCount)]
	public class AccessTransTrafficCount: AccessTransBase
	{
		
		[XmlIgnore]
		public DateTime EnterDateTime { get{ return base.GetDate(RegionEnterTime).Value; } }
		[XmlIgnore]
		public DateTime ExitDateTime { get { return base.GetDate(RegionExitTime).Value; } }
		[XmlIgnore]
		public int RegionIndex{ get;set;}

		[XmlElement(ConstEnums.EventID)]
		public int EventID { get; set;}

		[XmlElement(ConstEnums.RegionID)]
		public int RegionID { get; set;}

		[XmlElement(ConstEnums.PersonID)]
		public Int64 PersonID {get; set;}

		[XmlElement(ConstEnums.RegionEnterTime)]
		public string RegionEnterTime { get; set;}

		[XmlElement(ConstEnums.RegionExitTime)]
		public string RegionExitTime { get; set;}

		
	}
}
