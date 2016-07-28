using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public static class Utils
	{
		static readonly DateTime Unix_Date_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); 
		public static DateTime CurrentDatetimeUtc()
		{
			return new DateTime( DateTime.Now.Ticks, DateTimeKind.Utc);
		}
		public static DateTime unixTime_ToDateTime(this Int64 unixTime)
		{
			//DateTime dtDate = new DateTime(1970, 1, 1, 0, 0, 0);
			return Unix_Date_time.AddSeconds(unixTime);
		}
		public static DateTime unixTime_ToDateTime(this Int32 unixTime)
		{
			//DateTime dtDate = new DateTime(1970, 1, 1, 0, 0, 0);
			return ((Int64)unixTime).unixTime_ToDateTime();
		}

		public static DateTime unixTime_ToDateTime(this Int32 unixTime, int hour)
		{
			//DateTime dtDate = new DateTime(1970, 1, 1, 0, 0, 0);
			DateTime ret = ((Int64)unixTime).unixTime_ToDateTime();
			return ret.AddHours(hour);
		}


		public static long DateToUnixTimestamp(this DateTime dateTime)
		{
			//TimeSpan unixTimeSpan = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0));
			TimeSpan unixTimeSpan = (dateTime.Date - Unix_Date_time);
			return (long)unixTimeSpan.TotalSeconds;
		}

		public static long FullDateTimeToUnixTimestamp(this DateTime dateTime)
		{
			//TimeSpan unixTimeSpan = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0));
			TimeSpan unixTimeSpan = (dateTime - Unix_Date_time);
			return (long)unixTimeSpan.TotalSeconds;
		}

		public static int ToUnixTimestamp(this DateTime val, int hour)
		{
			return (int)val.Date.AddHours(hour).FullDateTimeToUnixTimestamp();
		}

		public static int WeekOfYear(this DateTime val, CultureInfo cultureinfo = null)
		{
			CultureInfo myCI = cultureinfo == null? new CultureInfo("en-US"): cultureinfo;
			Calendar myCal = myCI.Calendar;

			// Gets the DTFI properties required by GetWeekOfYear.
			CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
			DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
			 return myCal.GetWeekOfYear(val, myCWR, myFirstDOW);

		}

		public static string LongMonthName(this DateTime val)
		{
			 return val.ToString("MMMM");
		}
		public static string ShortMonthName(this DateTime val)
		{
			return val.ToString("MMM");
		}
		public static int Quater(this DateTime val)
		{
			return (val.Month - 1) / 3 + 1;
		}

	}
}
