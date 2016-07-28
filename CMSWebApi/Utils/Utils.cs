using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.IO;
using System.Globalization;
namespace CMSWebApi.Utils
{
	public static class Utilities
	{
		/// <summary>
		/// Calculate Total Pages
		/// </summary>
		/// <param name="numberOfRecords"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public static int CalculateTotalPages(long numberOfRecords, Int32 pageSize)
		{
			long result;
			int totalPages;

			Math.DivRem(numberOfRecords, pageSize, out result);

			if (result > 0)
				totalPages = (int)((numberOfRecords / pageSize)) + 1;
			else
				totalPages = (int)(numberOfRecords / pageSize);

			return totalPages;

		}

		/// <summary>
		/// Check if date is a valid format
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static Boolean IsDate(string date)
		{
			DateTime dateTime;
			return DateTime.TryParse(date, out dateTime);
		}

		/// <summary>
		/// IsNumeric
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static Boolean IsNumeric(object entity)
		{
			if (entity == null)
				return false;

			int result;
			return int.TryParse(entity.ToString(), out result);
		}

		/// <summary>
		/// IsDouble
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static Boolean IsDouble(object entity)
		{
			if (entity == null)
				return false;

			string e = entity.ToString();

			// Loop through all instances of the string 'text'.
			int count = 0;
			int i = 0;
			while ((i = e.IndexOf(".", i)) != -1)
			{
				i += ".".Length;
				count++;
			}
			if (count > 1)
				return false;

			e = e.Replace(".", "");

			int result;
			return int.TryParse(e, out result);
		}

		/// <summary>
		/// Add a generic message string
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static List<String> Message(string message)
		{
			List<String> returnMessage = new List<String>();
			returnMessage.Add(message);
			return returnMessage;
		}

		/// <summary>
		/// Folder first letter of every word to uppercase
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string UppercaseFirstLetter(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

			StringBuilder output = new StringBuilder();
			string[] words = s.Split(' ');
			foreach (string word in words)
			{
				char[] a = word.ToCharArray();
				a[0] = char.ToUpper(a[0]);
				string b = new string(a);
				output.Append(b + " ");
			}

			return output.ToString().Trim();

		}

		/// <summary>
		/// Get String
		/// </summary>
		/// <param name="inValue"></param>
		/// <returns></returns>
		public static string GetString(string inValue)
		{
			return (inValue != null) ? (inValue) : String.Empty;
		}
		
		public static string FormatString( string templete, object[]param)
		{
			if( param == null || param.Length == 0)
			return templete;
			return string.Format(templete, param);
		}

		public static DateTime DateTimeParseExact( string date, string format, DateTime defaultvalue)
		{
			try
			{
				return DateTime.ParseExact( date, format, CultureInfo.InvariantCulture);
			}
			catch( System.FormatException){}
			catch(System.ArgumentNullException){}

			return defaultvalue;
		}

		public static DateTime DefaultDate(PeriodType period)
		{
			DateTime result = DateTime.Now;
			return ToPeriodDate( result, period);
		}

		public static DateTime ToPeriodDate(DateTime date, PeriodType period, int value = 0, bool startOfDate = false)
		{
			DateTime pdate = date;
			switch(period)
			{
				case PeriodType.Year:
					pdate = date.AddYears(-value);
				break;

				case PeriodType.LastYear:
					pdate = date.AddYears(-1);
				break;

				case PeriodType.Month:
					pdate = date.AddMonths(-value);
				break;

				case PeriodType.LastMonth:
					pdate = date.AddMonths(-1);
				break;

				case PeriodType.Week:
					pdate = date.AddDays(-(7 * value));
				break;

				case PeriodType.LastWeek:
					pdate = date.AddDays(-7);
				break;

				case PeriodType.Day:
					pdate = date.AddDays(-value);
				break;

				case PeriodType.Yesterday:
					pdate = date.AddDays(-1);
				break;

				case PeriodType.Hour:
					pdate = date.AddHours(-value);
				break;

				case PeriodType.Min:
					pdate = date.AddMinutes(-value);
				break;

				case PeriodType.Sec:
					pdate = date.AddSeconds(-value);
				break;
				case PeriodType.Today:
					pdate = DateTime.Now.Date;
				break;
				case PeriodType.Now:
				pdate = DateTime.Now;
				break;
				default:
					pdate = date; //DateTime.MinValue;
				break;
			}
			if (startOfDate)
				 return pdate.Date;
			return pdate;
		}

		public static CMSWebApi.Utils.PeriodType Period(string value)
		{
			CMSWebApi.Utils.PeriodType period = Utils.PeriodType.Invalid;
			switch (value.ToLower())
			{
				case Consts.DateDefines.str_year:
				case Consts.DateDefines.str_y:
					period = Utils.PeriodType.Year;
					break;
				case Consts.DateDefines.str_lastyear:
				case Consts.DateDefines.str_ly:
					period = PeriodType.LastYear;
				break;
				case Consts.DateDefines.str_month:
				case Consts.DateDefines.str_m:
					period = Utils.PeriodType.Month;
					break;
				case Consts.DateDefines.str_lastmonth:
				case Consts.DateDefines.str_lm:
					period = PeriodType.LastMonth;
				break;
				case Consts.DateDefines.str_week:
				case Consts.DateDefines.str_w:
					period = PeriodType.Week;
				break;

				case Consts.DateDefines.str_lastweek:
				case Consts.DateDefines.str_lw:
					period = PeriodType.LastWeek;
				break;
				case Consts.DateDefines.str_day:
				case Consts.DateDefines.str_d:
					period = Utils.PeriodType.Day;
					break;
				case Consts.DateDefines.str_yesterday:
					period = PeriodType.Yesterday;
				break;
				case Consts.DateDefines.str_today:
					period = PeriodType.Today;
				break;
				case Consts.DateDefines.str_hour:
				case Consts.DateDefines.str_h:
					period = PeriodType.Hour;
				break;
				case Consts.DateDefines.str_minute:
				case Consts.DateDefines.str_min:
					period = Utils.PeriodType.Min;
					break;
				case Consts.DateDefines.str_second:
				case Consts.DateDefines.str_s:
					period = Utils.PeriodType.Sec;
					break;
			}
			return period;
		}
		
		public static bool CreateDir( string path)
		{
			if( string.IsNullOrEmpty(path))
				return false;
			if( Directory.Exists(path))
				return true;
			try
			{
				DirectoryInfo dinfo = Directory.CreateDirectory( path);
				return dinfo.Exists;
			}
			catch(Exception)
			{
				return false;
			}

		}

		public static bool DeleteFile(string fpath)
		{
			if( string.IsNullOrEmpty(fpath) || !File.Exists( fpath))
				return true;
			try{ File.Delete( fpath); return true;}catch(Exception){ return false;}
		}

		public static bool DeleteFolder(string path, bool recursive = true)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				return true;
			}

			try
			{
				Directory.Delete(path, recursive);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		
		public static DateTime StartOfDay(this DateTime theDate)
		{
			return theDate.Date;
		}

		public static DateTime EndOfDay(this DateTime theDate)
		{
			return theDate.Date.AddDays(1).AddTicks(-1);
	}

		public static string FrequencytoText(BAMReportType type)
		{
			string result = "Undefined.";
			switch( type)
			{
				case BAMReportType.Daily:
					result = "Daily";
				break;
				case BAMReportType.Hourly:
					result = "Hourly";
				break;
				case BAMReportType.Weekly:
					result = "Weekly";
				break;
				case BAMReportType.PTD:
					result = "Period to Date";
				break;
				case BAMReportType.WTD:
					result = "Week to Date";
				break;
				case BAMReportType.YTD:
					result = "Year to Date";
				break;
			}

			return result;
		}
	}

	
	public static class ArrayUtilities
	{
		public static IEnumerable<int> SequenceNumber(int Start, int numberElement)
		{
			if (numberElement <= 0)
				return Enumerable.Empty<int>();

			return Enumerable.Range(Start, numberElement);
		}
		public static IEnumerable<DateTime> SequenceDate(DateTime startDate, int numberElement)
		{
			if (numberElement < 0)
				yield break;

			DateTime endDate = startDate.AddDays(numberElement);
			foreach (DateTime ret in SequenceDate(startDate, endDate))
				yield return ret;
		}

		public static IEnumerable<DateTime> SequenceDate(DateTime startDate, DateTime endDate)
		{
			if (endDate < startDate)
				yield break;

			while (startDate <= endDate)
			{
				yield return startDate;
				startDate = startDate.AddDays(1);
			}
		}
	}
}
