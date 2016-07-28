using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.IOPC
{
	[Serializable]
	[XmlRoot(ConstEnums.CountingSetup)]
	public class CountingSetup
	{
		[XmlElement(ConstEnums.C_CameraNumber)]
		public int C_CameraNumber { get; set;}

		[XmlElement(ConstEnums.C_AreaName)]
		public string C_AreaName { get; set;}

		[XmlElement(ConstEnums.C_ObjectType)]
		public string C_ObjectType { get; set;}

		[XmlElement(ConstEnums.C_Count)]
		public int C_Count { get; set;}

		[XmlElement(ConstEnums.C_SetupTime)]
		public string C_SetupTime { get; set;}

		[XmlElement(ConstEnums.C_SetupDate)]
		public string C_SetupDate { get ;set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set;}

		[XmlElement(ConstEnums.Index)]
		public int Index { get ;set;}
	}

	[Serializable]
	[XmlRoot(ConstEnums.TrafficCountRegion)]
	public class TrafficCountRegion
	{
		[XmlElement(ConstEnums.RegionID)]
		public int RegionID{ get; set;}

		[XmlElement(ConstEnums.RegionName)]
		public  string RegionName { get; set;}

		[XmlElement(ConstEnums.InternalChannel)]
		public int InternalChannel{ get; set;}

		[XmlElement(ConstEnums.ExternalChannel)]
		public int ExternalChannel { get; set; }

		[XmlElement(ConstEnums.DateModified)]
		public string DateModified{ get ;set;}

		[XmlElement(ConstEnums.TimeModified)]
		public string TimeModified{ get ;set;}

		public DateTime DVRDateTime
		{
			get{ return AccessTransBase.toDVRDateTime( DateModified, TimeModified);}
		}
	}
}
