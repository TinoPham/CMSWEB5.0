using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.POS
{
	[XmlRoot(ConstEnums.Transact)]
	[Serializable]
	public class AccessTransPOS : AccessTransBase
	{
		[XmlElement(ConstEnums.TransactKey)]
		public string TransactKey { get; set;}

		[XmlElement(ConstEnums.T_0TransNB, IsNullable=true)]
		public double? T_0TransNB { get; set;}

		[XmlElement(ConstEnums.T_6TotalAmount, IsNullable = true)]
		public double? T_6TotalAmount { get; set;}

		[XmlElement(ConstEnums.T_1SubTotal, IsNullable = true)]
		public double? T_1SubTotal { get; set;}

		[XmlElement(ConstEnums.T_MethOfPaymID)]
		public string T_MethOfPaymID { get; set;}

		[XmlElement(ConstEnums.T_2Tax1Amount, IsNullable = true)]
		public double? T_2Tax1Amount { get; set;}

		[XmlElement(ConstEnums.T_3Tax2Amount, IsNullable = true)]
		public double? T_3Tax2Amount { get; set;}

		[XmlElement(ConstEnums.T_4Tax3Amount, IsNullable = true)]
		public double? T_4Tax3Amount { get; set;}

		[XmlElement(ConstEnums.T_5Tax4Amount, IsNullable = true)]
		public double? T_5Tax4Amount { get; set;}

		[XmlElement(ConstEnums.T_7PaymAmount, IsNullable = true)]
		public double? T_7PaymAmount { get; set;}

		[XmlElement(ConstEnums.T_8ChangeAmount, IsNullable = true)]
		public double? T_8ChangeAmount { get; set;}

		[XmlElement(ConstEnums.TransDate)]
		public string TransDate { get; set;}

		[XmlElement(ConstEnums.TransTime)]
		public string TransTime { get; set;}

		[XmlElement(ConstEnums.DVRDate)]
		public string DVRDate { get; set;}

		[XmlElement(ConstEnums.DVRTime)]
		public string DVRTime { get; set;}

		[XmlElement(ConstEnums.T_9RecItemCount)]
		public int T_9RecItemCount { get; set;}

		[XmlElement(ConstEnums.T_CameraNB)]
		public string T_CameraNB {get; set;}

		[XmlElement(ConstEnums.T_OperatorID)]
		public string T_OperatorID { get;set;}

		[XmlElement(ConstEnums.T_StoreID, IsNullable = true)]
		public string T_StoreID { get; set;}

		[XmlElement(ConstEnums.T_TerminalID, IsNullable = true)]
		public string T_TerminalID { get; set;}

		[XmlElement(ConstEnums.T_RegisterID, IsNullable = true)]
		public string T_RegisterID { get; set;}

		[XmlElement(ConstEnums.T_ShiftID, IsNullable = true)]
		public string T_ShiftID { get; set;}

		[XmlElement(ConstEnums.T_CheckID, IsNullable = true)]
		public string T_CheckID { get; set;}

		[XmlElement(ConstEnums.T_CardID, IsNullable = true)]
		public string T_CardID { get; set; }

		[XmlElement(ConstEnums.T_TransXString1, IsNullable = true)]
		public string T_TransXString1 { get; set;}

		[XmlElement(ConstEnums.T_TransXString2, IsNullable = true)]
		public string T_TransXString2 { get; set;}

		[XmlElement(ConstEnums.T_TransXNbInt, IsNullable = true)]
		public int? T_TransXNbInt { get; set;}

		[XmlElement(ConstEnums.T_TransXNbFloat, IsNullable = true)]
		public double? T_TransXNbFloat { get; set;}

		[XmlElement(ConstEnums.T_00TransNBText, IsNullable = true)]
		public string T_00TransNBText { get; set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set;}

		[XmlElement(ConstEnums.T_TOBox)]
		public int T_TOBox { get; set;}
		[XmlArray]
		[XmlArrayItem(ConstEnums.Retail)]
		public List<Retail> Retails{ get; set;}

		[XmlIgnore]
		public DateTime DVRDateTime { get { return AccessTransBase.toDVRDateTime(DVRDate, DVRTime); }}
		[XmlIgnore]
		public DateTime TransDateTime { get { return AccessTransBase.toDVRDateTime(TransDate, TransTime); } }
	}
}
