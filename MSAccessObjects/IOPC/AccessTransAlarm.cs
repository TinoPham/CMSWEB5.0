using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.Alarm)]
	public class AccessTransAlarm : AccessTransBase
	{
		[XmlElement(ConstEnums.A_CameraNumber)]
		public int A_CameraNumber { get; set;}

		[XmlElement(ConstEnums.A_AreaName)]
		public string A_AreaName { get; set;}

		[XmlElement(ConstEnums.DVRDate)]
		public string DVRDate { get; set;}

		[XmlElement(ConstEnums.DVRTime)]
		public string DVRTime { get; set;}

		[XmlElement(ConstEnums.A_ObjectType)]
		public string A_ObjectType { get; set;}

		[XmlElement(ConstEnums.A_AlarmType)]
		public string A_AlarmType { get; set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID{ get; set;}

		[XmlIgnore]
		public DateTime DVRDateTime{ get { return AccessTransBase.toDVRDateTime(DVRDate, DVRTime); }}
	}
}
