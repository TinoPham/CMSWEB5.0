using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.Count)]
	public class AccessTransCount: AccessTransBase
	{
		[XmlElement(ConstEnums.C_CameraNumber)]
		public int C_CameraNumber { get; set;}

		[XmlElement(ConstEnums.C_AreaName)]
		public string C_AreaName { get; set;}

		[XmlElement(ConstEnums.C_ObjectType)]
		public string C_ObjectType { get; set;}

		[XmlElement(ConstEnums.C_Count)]
		public int C_Count { get; set;}

		[XmlElement(ConstEnums.DVRTime)]
		public string DVRTime { get; set;}

		[XmlElement(ConstEnums.DVRDate)]
		public string DVRDate { get; set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set;}

		[XmlIgnore]
		public DateTime DVRDateTime
		{
			get{ return AccessTransBase.toDVRDateTime( DVRDate, DVRTime);}
		}
	}
}
