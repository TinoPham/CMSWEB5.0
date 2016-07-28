using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.ActiveInfo)]
	public class AccessTransActiveInfo : AccessTransBase
	{
		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set;}

		[XmlElement(ConstEnums.AttendDay)]
		public string AttendDay { get; set;}

		[XmlElement(ConstEnums.AttendTime)]
		public string AttendTime { get; set;}

		[XmlElement(ConstEnums.ID)]
		public int ID { get; set;}

		[XmlElement(ConstEnums.Obj_Label)]
		public string Obj_Label { get; set;}

		[XmlElement(ConstEnums.Mark)]
		public string Mark {get; set;}

		[XmlElement(ConstEnums.ActiveTime)]
		public string ActiveTime { get; set;}

		[XmlElement(ConstEnums.ActiveDay)]
		public string ActiveDay{ get; set;}

		[XmlElement(ConstEnums.A_ObjectType)]
		public string A_ObjectType{ get; set;}

		[XmlElement(ConstEnums.A_AreaName)]
		public string A_AreaName { get; set;}

	}
}
