using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.POS
{
	[Serializable]
	[XmlRoot(ConstEnums.Sensor)]
	public class AccessTransSensor: AccessTransBase
	{
		[XmlIgnore]
		public DateTime DVRDate{ get;set;}

		[XmlElement(ConstEnums.S_ID)]
		public string S_ID  { get; set; }

		[XmlElement(ConstEnums.OT_Start, IsNullable=true)]
		public string OT_Start { get; set;}

		[XmlIgnore]
		public DateTime? OT_StartDate
		{
			get { return GetDate(OT_Start);}
		} 

		[XmlElement(ConstEnums.OT_End, IsNullable = true)]
		public string OT_End { get; set;}
		[XmlIgnore]
		public DateTime? OT_Endate
		{
			get { return GetDate(OT_End); }
		}

		[XmlElement(ConstEnums.GT_Start, IsNullable = true)]
		public string GT_Start { get; set;}

		[XmlIgnore]
		public DateTime? GT_StartDate
		{
			get{ return GetDate(GT_Start);}
		}

		[XmlElement(ConstEnums.GT_End, IsNullable = true)]
		public string GT_End { get; set;}

		[XmlIgnore]
		public DateTime? DT_EndDate
		{
			get{ return GetDate(GT_Start);}
		}

		[XmlElement(ConstEnums.PU_Start, IsNullable = true)]
		public string PU_Start { get; set;}
		[XmlIgnore]
		public DateTime? PU_StartDate
		{
			get{ return GetDate(PU_Start);}
		}

		[XmlElement(ConstEnums.PU_End, IsNullable = true)]
		public string PU_End { get; set;}

		[XmlIgnore]
		public DateTime? PU_EndDate
		{
			get{ return GetDate(PU_End);}
		}

		[XmlElement(ConstEnums.PAC_ID, IsNullable = true)]
		public string PAC_ID { get; set;}

	}
}
