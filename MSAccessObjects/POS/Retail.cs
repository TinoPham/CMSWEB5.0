using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.POS
{
	
	[XmlRoot(ConstEnums.Retail)]
	[Serializable]
	public class Retail
	{
		[XmlElement(ConstEnums.RetailKey)]
		public string RetailKey{ get; set;}

		[XmlElement(ConstEnums.TransactKey)]
		public string TransactKey { get;set;}

		[XmlElement(ConstEnums.R_2ItemLineNb)]
		public string R_2ItemLineNb { get;set;}

		[XmlElement(ConstEnums.R_1Qty, IsNullable= true)]
		public float? R_1Qty { get; set; }

		[XmlElement(ConstEnums.R_0Amount, IsNullable = true)]
		public double? R_0Amount { get; set;}

		[XmlElement(ConstEnums.R_Description)]
		public string R_Description { get; set;}

		[XmlElement(ConstEnums.R_ItemCode)]
		public string R_ItemCode { get;set;}

		[XmlElement(ConstEnums.R_RetailXString1)]
		public string R_RetailXString1{ get;set;}

		[XmlElement(ConstEnums.R_RetailXString2)]
		public string R_RetailXString2 { get;set;}

		[XmlElement(ConstEnums.R_RetailXNbInt)]
		public string R_RetailXNbInt{ get; set;}

		[XmlElement(ConstEnums.R_RetailXNbFloat, IsNullable = true)]
		public float? R_RetailXNbFloat {get; set;}

		[XmlElement(ConstEnums.R_DVRDate)]
		public string R_DVRDate{ get; set;}

		[XmlElement(ConstEnums.R_DVRTime)]
		public string R_DVRTime { get; set;}

		[XmlElement(ConstEnums.R_CameraNB)]
		public string R_CameraNB { get; set;}

		[XmlElement(ConstEnums.R_TOBox)]
		public string R_TOBox { get; set;}
		[XmlArray]
		[XmlArrayItem(ConstEnums.SubRetail)]
		public List<SubRetail> SubRetails { get; set;}

		[XmlIgnore]
		public DateTime? R_DVRDateTime
		{ 
		get	{
			if( string.IsNullOrEmpty(R_DVRDate) || string.IsNullOrEmpty(R_DVRTime))
				return null;
			return AccessTransBase.toDVRDateTime(R_DVRDate, R_DVRTime);
		}
			
		}
	}
}
