using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.POS
{
	[Serializable]
	[XmlRoot(ConstEnums.SubRetail)]
	public class SubRetail
	{
		[XmlElement(ConstEnums.RetailKey)]
		public string RetailKey { get;set;}

		[XmlElement(ConstEnums.SR_2SubItemLineNb)]
		public int SR_2SubItemLineNb { get;set;}

		[XmlElement(ConstEnums.SR_1Qty)]
		public int SR_1Qty { get;set;}

		[XmlElement(ConstEnums.SR_0Amount , IsNullable= true)]
		public double? SR_0Amount { get;set;}
		
		[XmlElement(ConstEnums.SR_Description)]
		public string SR_Description { get; set;}
	}
}
