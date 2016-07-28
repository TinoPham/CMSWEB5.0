using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.ATM
{
	[Serializable]
	[XmlRoot(ConstEnums.Transact)]
	public class AccessTransATM : AccessTransBase
	{
		[XmlElement(ConstEnums.TransactKey)]
		public string TransactKey { get; set;}

		[XmlElement(ConstEnums.T_0TransNB)]
		public double T_0TransNB { get; set;}

		[XmlElement(ConstEnums.T_TransCode)]
		public string T_TransCode { get; set;}

		[XmlElement(ConstEnums.T_TransAmount)]
		public double T_TransAmount { get; set;}

		[XmlElement(ConstEnums.T_TransTermFee)]
		public double T_TransTermFee { get; set;}

		[XmlElement(ConstEnums.T_TransType)]
		public string T_TransType { get; set;}

		[XmlElement(ConstEnums.T_TransTotal)]
		public double T_TransTotal { get; set;}

		[XmlElement(ConstEnums.TransDate)]
		public string TransDate { get; set;}

		[XmlElement(ConstEnums.TransTime)]
		public string TransTime { get; set;}

		[XmlElement(ConstEnums.BusinessDate)]
		public string BusinessDate { get; set;}

		[XmlElement(ConstEnums.DVRDate)]
		public string DVRDate { get; set;}

		[XmlElement(ConstEnums.DVRTime)]
		public string DVRTime { get; set;}

		[XmlElement(ConstEnums.T_CameraNB)]
		public string T_CameraNB { get; set;}

		[XmlElement(ConstEnums.T_CardNB)]
		public string T_CardNB { get; set;}

		[XmlElement(ConstEnums.T_AcctBalance)]
		public double T_AcctBalance { get; set;}

		[XmlElement(ConstEnums.T_TransXString1)]
		public string T_TransXString1 { get; set;}

		[XmlElement(ConstEnums.T_TransXString2)]
		public string T_TransXString2 { get; set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set; }

		[XmlIgnore]
		public DateTime DVRDateTime{ get{ return  AccessTransBase.toDVRDateTime(DVRDate, DVRTime); }}

		[XmlIgnore]
		public DateTime TransDateTime{ get{ return AccessTransBase.toDVRDateTime(TransDate, TransTime); }}
	}
}
