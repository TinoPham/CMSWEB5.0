using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSAccessObjects.CA
{
	[Serializable]
	[XmlRoot(ConstEnums.Transact)]
	public class AccessTransCA : AccessTransBase
	{
		//const string rx_date = @"^(?<Month>\d{2})/(?<Day>\d{2})/(?<Year>\d{4})$";
		//const string rx_time = @"^(?<Hour>\d{2}):(?<Min>\d{2}):(?<Sec>\d{2})[ ]{1}(?<Symbol>(AM)|(PM))$";
		//const string str_Symbol = "Symbol";

		[XmlElement(ConstEnums.DVRDate)]
		public string DVRDate { get; set;}

		[XmlElement(ConstEnums.DVRTime)]
		public string DVRTime {get; set;}

		[XmlElement(ConstEnums.TransDate)]
		public string TransDate { get; set;}

		[XmlElement(ConstEnums.TransTime)]
		public string TransTime { get; set;}

		[XmlElement(ConstEnums.T_CameraNB)]
		public string T_CameraNB { get; set;}

		[XmlElement(ConstEnums.T_PACID)]
		public string T_PACID { get; set;}

		[XmlElement(ConstEnums.T_TranType)]
		public string T_TranType { get; set;}

		[XmlElement(ConstEnums.T_UnitID)]
		public string T_UnitID { get; set;}

		[XmlElement(ConstEnums.T_SiteID)]
		public string T_SiteID { get; set;}

		[XmlElement(ConstEnums.T_DevName)]
		public string T_DevName { get; set;}

		[XmlElement(ConstEnums.T_Batch)]
		public string T_Batch { get; set;}

		[XmlElement(ConstEnums.T_Card)]
		public string T_Card { get; set;}

		[XmlElement(ConstEnums.T_FirstName)]
		public string T_FirstName { get; set;}

		[XmlElement(ConstEnums.T_LastName)]
		public string T_LastName { get; set;}

		[XmlElement(ConstEnums.T_XString1)]
		public string T_XString1 { get; set;}

		[XmlElement(ConstEnums.T_XString2)]
		public string T_XString2 { get; set;}

		[XmlIgnore]
		public DateTime DVRDateTime { get { return AccessTransBase.toDVRDateTime(DVRDate, DVRTime); } }

		[XmlIgnore] 
		public DateTime TransDateTime
		{
			get { return ToTransDateTime(TransDate, TransTime); }
		}

		private DateTime ToTransDateTime( string strDate, string strTime)
		{
			DateTime? ret = AccessTransBase.SymbolDatetimetoDateTime(TransDate, TransTime);
			return  ret == null || !ret.HasValue ? DateTime.MinValue : ret.Value;
		}
		//private DateTime ToTransDateTime( string strDate, string strTime)
		//{
		//	Match match = AccessTransBase.Match(rx_date, strDate); 
		//	if( !match.Success)
		//		return DateTime.MinValue;
		//	int year = Convert.ToInt32(match.Groups[AccessTransBase.str_Year].Value);
		//	int month = Convert.ToInt32(match.Groups[AccessTransBase.str_Month].Value);
		//	int day = Convert.ToInt32(match.Groups[AccessTransBase.str_Day].Value);
		//	match = AccessTransBase.Match(rx_time, strTime); 
		//	if( !match.Success)
		//		return new DateTime( year, month, day, 0,0,0, DateTimeKind.Utc);
		//	int hour = Convert.ToInt32(match.Groups[AccessTransBase.str_Hour].Value);
		//	int min = Convert.ToInt32(match.Groups[AccessTransBase.str_Min].Value);
		//	int sec = Convert.ToInt32(match.Groups[AccessTransBase.str_Sec].Value);
		//	string symbol = match.Groups[ str_Symbol].Value;
		//	if( string.Compare( symbol, AccessTransBase.str_PM, true) == 0)
		//		hour = Math.Min( 23, hour + 12);
		//	return new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc); 
		//}
	}
}
