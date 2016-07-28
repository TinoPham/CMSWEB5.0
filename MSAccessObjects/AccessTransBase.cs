using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
namespace MSAccessObjects
{
	public abstract class AccessTransBase
	{
		public const string STR_Date_Format = "MM/dd/yy";
		const string RX_Date = @"^(?<Month>\d{1,2})/(?<Day>\d{1,2})/(?<Year>\d{2,4})$";
		const string RX_Time = @"^(?<Hour>\d{1,2}):(?<Min>\d{1,2}):(?<Sec>\d{1,2}):?(?<Mili>\d{1,3})?$";
		const string RX_Time_Symbol = @"^(?<Hour>\d{1,2}):(?<Min>\d{1,2}):(?<Sec>\d{1,2})(?<Sym>((\x20AM)|(\x20PM))){0,1}$";
		public const string str_Year = "Year";
		public const string str_Month = "Month";
		public const string str_Day = "Day";
		public const string str_Hour = "Hour";
		public const string str_Min = "Min";
		public const string str_Sec = "Sec";
		public const string str_Mili = "Mili";
		public const string str_AM = "AM";
		public const string str_PM = "PM";
		const string str_Symbol = "Sym";
		const int Begin_Num_Year = 2000;
		[XmlIgnore]
		public DateTime FileDate{ get ;set;}

		protected static Match Match(string pattern, string input)
		{
			Regex rx = new Regex( pattern, RegexOptions.IgnoreCase);
			return rx.Match(input);
		}

		///<summary>
		///
		/// </summary>
		/// <param name="date">Date only with format: mm/dd/yy</param>
		/// <param name="time">Time only with format: hh:mm:ss AM|PM</param>
		/// <returns></returns>
		public static DateTime? SymbolDatetimetoDateTime( string date, string time)
		{
			Match match = Match(RX_Date, date);
			if (!match.Success)
				return null;
			int year = match.Groups[str_Year].Value.Length < Begin_Num_Year.ToString().Length ? (Begin_Num_Year + Convert.ToInt16(match.Groups[str_Year].Value)) : Convert.ToInt16(match.Groups[str_Year].Value);
			int month = Convert.ToInt16(match.Groups[str_Month].Value);
			int day = Convert.ToInt16(match.Groups[str_Day].Value);
			match = AccessTransBase.Match(RX_Time_Symbol, time);
			if (!match.Success)
				return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);

			int hour = Convert.ToInt32(match.Groups[AccessTransBase.str_Hour].Value);
			int min = Convert.ToInt32(match.Groups[AccessTransBase.str_Min].Value);
			int sec = Convert.ToInt32(match.Groups[AccessTransBase.str_Sec].Value);
			string symbol = match.Groups[str_Symbol].Value;
			if (string.Compare(symbol, AccessTransBase.str_PM, true) == 0)
				hour = Math.Min(23, hour + 12);
			return new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc); 


		}
		///<summary>
		///
		/// </summary>
		/// <param name="date">Date only with format: mm/dd/yy</param>
		/// <param name="time">Time only with format: hh:mm:ss hh:mm:ss:ff</param>
		/// <returns></returns>
		public static DateTime toDVRDateTime( string date, string time)
		{
			Match match = Match(RX_Date, date);
			if( !match.Success)
				return DateTime.MinValue;
			int year = Begin_Num_Year + Convert.ToInt16(match.Groups[str_Year].Value);

			int month = Convert.ToInt16(match.Groups[str_Month].Value);
			int day = Convert.ToInt16(match.Groups[str_Day].Value);
			match = Match(RX_Time, time);
			if(!match.Success)
				return new DateTime(year, month, day, 0,0,0, DateTimeKind.Utc);
			int hour = Convert.ToInt16(match.Groups[str_Hour].Value);
			int min = Convert.ToInt16(match.Groups[str_Min].Value);
			int sec = Convert.ToInt16(match.Groups[str_Sec].Value);
			int mili = Convert.ToInt16(string.IsNullOrEmpty(match.Groups[str_Mili].Value) ? "0" : match.Groups[str_Mili].Value);
			
			return new DateTime( year, month, day, hour, min,sec, mili, DateTimeKind.Utc);

		}
		
		public static DateTime? toDateTime( string date, string time)
		{
			DateTime ret = toDVRDateTime(date, time); 
			return ret == DateTime.MinValue? (Nullable<DateTime>)null : (Nullable<DateTime>)ret;
		}
		
		protected DateTime? GetDate(string time)
		{
			if (string.IsNullOrEmpty(time))
				return null;
			if (FileDate == DateTime.MinValue || FileDate == DateTime.MaxValue)
				return null;
			return toDVRDateTime(FileDate.ToString(AccessTransBase.STR_Date_Format), time);
		}
		
	}
}
