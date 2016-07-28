using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.Qtime)]
	public class AccessTransQtime : AccessTransBase
	{
		[XmlElement(ConstEnums.EventID)]
		public int EventID { get; set;}

		[XmlElement(ConstEnums.PersonID)]
		public int PersonID { get; set;}

		[XmlElement(ConstEnums.ServiceEnterTime)]
		public string ServiceEnterTime { get; set;}

		[XmlElement(ConstEnums.RegisterEnterTime)]
		public string RegisterEnterTime { get; set;}

		[XmlElement(ConstEnums.RegisterExitTime)]
		public string RegisterExitTime { get; set;}

		[XmlElement(ConstEnums.PickupEnterTime)]
		public string PickupEnterTime { get; set;}

		[XmlElement(ConstEnums.PickupExitTime)]
		public string PickupExitTime { get; set;}

		[XmlElement(ConstEnums.ServiceExitTime)]
		public string ServiceExitTime { get; set;}

		[XmlElement(ConstEnums.ExternalChannel)]
		public string ExternalChannel { get; set;}

		[XmlElement(ConstEnums.InternalChannel)]
		public string InternalChannel { get; set;}

		[XmlElement(ConstEnums.PACID)]
		public string PACID { get; set;}
	}
}
