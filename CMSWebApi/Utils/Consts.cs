using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Utils
{
	public class Consts
	{
		public class CacheStatus_Defines
		{
			public const string CACHE_READY ="Ready";
			public const string CACHE_REBUILD = "Cache is rebuilding";
			public const string CACHE_LOADING = "Cache is loading";
			public const string CACHE_NOT_READY = "Cache is not ready to query.";
			//public const string CACHE_NODATAFILE = "Cache is loading";
			
		}
		public const string STR_JSON = "json";
		public const string STR_attachment = "attachment";
		public const string XSRF_TOKEN_KEY = "XSRF-TOKEN";
		public const string DEFAULT_CULTURE = "en-US";
		public const int PAGESIZE_DEFAULT = 10;
		public const string Application_Json = "application/json";
		public const string Application_Xml = "application/xml";
		//public const string Path_Image = "../../App_Data/Users";
		public const string ImagesFolder = "Content/Images";
        public const string ImagesHeatMap = "HeatMap";
        public const string ImagesHeatMap_Manual = @"HeatMap\Manual";
		public const string ImagesHeatMap_Schedules = @"DVR\HeatMap\Schedules";
		public const string FORM_DATA = "form-data";
		public const char STAR_SIGN =  '*';
		public const char EQUAL_SIGN = '=';
		public const char AT_SIGN = '@';
		public const char PLUS_SIGN = '+';
		public const char DECIMAL_SIGN = ',';
		public const string App_Data = "DataDirectory";
		public const int JOBTITLE_DEFAULT_COLOR = 0xFFFFFF;
		public const string LOG_DATE_FORMAT = "yyyyMMdd HH:ss:tt";
		public const string CHART_DATE_FORMAT = "yyyy/M/d";
		public const string CHART_LEGEND_DATE_FORMAT = "MM/dd/yy";
		public const string CHART_LEGEND_HOUR_FORMAT = "{0}:00 - {1}:00";
		public const string CHART_LEGEND_MONTH_FORMAT = "Period {0:00}";
		public const string Image_Default = "img_user_blank.png";
		public const string ControllerKey = "controller";
		public const string ActionKey = "action";

		public const int ALERT_RECORDING_LESSDAYS = 90; //days - will move to config file
		public const int ALERT_OFFLINE_HOURS = 2;
		public const string ALERT_RECORDING_LESS_NAME = "DVR recorded less than {0} days";
		public const string ALERT_OFFLINE_NAME = "DVR Lost connections more than {0} hours";
		public const string ALERT_HAS_NORMALIZE_NAME = "Number of DVR Normalized";
		public const string CONFIGURATIONS = "Configurations";
		public const string PATH_RAW_DVRIMAGE = @"DVR\RawImages";
		public const int PERIOD_PER_YEAR = 12;
		public const byte VIDEOLOSS_ALERTTYPE = 5;
		public const string RECORDING_DAY = "RecordingDay";
		public const string STATE_ID_PARAM = "SID";
		public const string NUM_CONV_SITE_PARAM = "Top";
		public const int NUM_CONV_SITE_DEF = 50;
		public const short CMS_MODE_STANDARD = 255;
		public const string QUERY_STRING_DATE_FORMAT = "yyyyMMdd";
		public const string QUERY_STRING_DATE_FORMAT_SS = "yyyyMMddHHmmss";
        public const string QUERY_STRING_DATE_FORMAT_HH = "yyyyMMddHH";
		public const string THUMBNAIL_FOLDER = "thumbnail";
		public const string THUMBNAIL_Icon = "no_image.jpg";
		public const int THUMNAIL_WIDTH = 380;
		public const int THUMNAIL_HEIGHT = 275;
		public const string MAP_IMAGES_FOLDER = "Maps";
		public const string Virtualchanel = "Virtual Chanel 0";
		public const string VirtualDVR = "Virtual DVR";
		public const string MACADDRESS = "MACADDRESS";
		public const int MAX_MAC_LENGTH = 12;
		public const string REC_DATE_FORMAT = "M/d/yyyy h:mm:ss tt";
		public const string RES_DATE_FORMAT = "MM/dd/yyyy";
		public const string IMAGE_SITE_FIELD = "ImageSite";
		public const string FIXTURE_PLAN_FIELD = "FixturePlan";
		public const string ACTUAL_BUDGET_FIELD = "ActualBudget";
		public const string Config_FileName = "AlertConfigs.xml";
		public const string str_alt = "alt";

		public const int BAM_RPT_DASHBOARD = 1;
		public const int MIN_PER_HOUR = 60;
		public const int MIN_PER_DATE = 1440;
		public const int MIN_PER_WEEK = 10080;
		public const int DAY_PER_WEEK = 7;

		public abstract class DateDefines
		{
			public const string str_year = "year";
			/// <summary>
			/// year define
			/// </summary>
			public const string str_y = "y";
			public const string str_lastyear = "lastyear";
			/// <summary>
			/// Last year define
			/// </summary>
			public const string str_ly = "ly";

			public const string str_month = "month";
			/// <summary>
			/// month define
			/// </summary>
			public const string str_m = "m";
			public const string str_lastmonth = "lastmonth";
			/// <summary>
			/// Last Month define
			/// </summary>
			public const string str_lm = "lm";

			public const string str_week= "week";
			/// <summary>
			/// Week define
			/// </summary>
			public const string str_w = "w";
			public const string str_lastweek = "lastweek";
			/// <summary>
			/// Last week define
			/// </summary>
			public const string str_lw = "lw";


			/// <summary>
			/// day define
			/// </summary>
			public const string str_day = "day";
			/// <summary>
			/// day define
			/// </summary>
			public const string str_d = "d";

			/// <summary>
			/// Yesterday define
			/// </summary>
			public const string str_yesterday = "yesterday";
			/// <summary>
			/// Today define
			/// </summary>
			public const  string str_today = "today";

			public const string str_hour = "hour";
			public const string str_h = "h";
			/// <summary>
			/// minute define
			/// </summary>
			public const string str_minute = "minute";
			/// <minute>
			/// Second define
			/// </summary>
			public const string str_min = "min";

			/// <summary>
			/// Second define
			/// </summary>
			public const string str_second = "second";
			/// <summary>
			/// Second define
			/// </summary>
			public const string str_s = "s";
			public const string str_now = "now";
			public const string str_n = "n";


			public static readonly string[] ValidPeriods = { str_year, str_y, str_lastyear, str_ly
													, str_month, str_m, str_lastmonth, str_lm
													, str_week, str_lw, str_w, str_lastweek
													, str_day, str_d, str_yesterday, str_today
													, str_hour, str_h
													, str_minute, str_min, str_second, str_s, str_now, str_n};
			
			public static int ValidDefineIndex( bool ignorecase = false, params string[] values)
			{
				int index = -1;
				for( int i = 0; i< values.Length; i++)
				{
					if( isValidDefine( values[i], ignorecase))
					{
						index = i;
						break;
					}
				}
					
				return index;
			}

			public static bool isValidDefine( string value, bool ignorecase = false)
			{
				if( string.IsNullOrEmpty(value))
					return false; 
				return ValidPeriods.Any( item => string.Compare(item,value, ignorecase) == 0 ); 
			}
		}
	}
	public class FiscalCalendarConst
	{

		#region CalendarStyle

		public const string CStyle_454 = "454";
		public const string CStyle_544 = "544";
		public const string CStyle_445 = "445";

		#endregion


		#region NumberConst
		public const int NumDayOf52Weeks = 364;
		public const int NumDayOf53Weeks = 371;  
		#endregion

	}


	public class ALert
	{
		public const string KDVR = "KDVR";
		public const string KAlertType = "KAlertType";
		public const string Time = "Time";
		public const string TotalAlert = "TotalAlert";
		public const string TimeZone = "TimeZone";
		public const string BeginDate = "BeginDate";
		public const string EndDate = "EndDate";

	}

	public class Boxgadget
	{
		public const string NUMBER_ALERTS="NUMBER_ALERTS";
		public const string NUMBER_DVROFF="NUMBER_DVROFF";
		public const string NUMBER_SENSOR = "NUMBER_SENSOR";
		public const string NUMBER_URGENT = "NUMBER_URGENT";
		public const string NUMBER_DVRRECORDLESS = "NUMBER_DVRRECORDLESS";
		public const string NUMBER_VASENSOR = "NUMBER_VASENSOR";
		public const string NUMBER_CONVERSION = "NUMBER_CONVERSION";
		public const string NUMBER_TRAFFIC = "NUMBER_TRAFFIC";
		public const string NUMBER_SALE = "NUMBER_SALE";
		public const string NUMBER_TRANSACTION = "NUMBER_TRANSACTION";
		public const string NUMBER_POSEXCEPTION = "NUMBER_POSEXCEPTION";
	}

	public class SiteConts
	{
		public const string REGION_CANNOT_NULL= "RegionModel can not be null.";
		public const string REGION_UPDATE_ERROR = "Update Region had error";
		public const string REGION_DELETE_ERROR = "Delete Region had error";
		public const string REGION_KEY_0 = "Region Key can not be 0.";
	}
	
	public class ExportConst
	{
		public class CMYKColors
		{
			public double Cyan { get; set; }
			public double Magenta { get; set; }
			public double Yellow { get; set; }
			public double Black { get; set; }
		}

		public const string SHEET_NAME = "Report";
		public const string PDF_EXTENSION = ".pdf";
		public const string EXCEL_EXTENSION = ".xlsx";
		public const string CSV_EXTENSION = ".csv";

		public static Dictionary<int, string> exceldataformat = new Dictionary<int, string>() { { 1, "General" }, { 2, "0.00%" }, { 3, "\"$\"#,##0.00" } };

		public const string COLOR_GREEN = "FF009617"; //Color Hex
		public const string COLOR_YELLOW = "FFD4D000";
		public const string COLOR_RED = "FFE30000";
		public const string COLOR_LESS_BLUE = "FF89a9c2";
		public const string COLOR_BLUE = "FF426d8f";
		public const string COLOR_BLACK = "FF646464";
		public const string COLOR_WHITE = "FFFFFFFF";
		public const string COLOR_ORANGE = "FFED7D31";
		public const string COLOR_GRAY = "FFEDEDED";

		//Convert Hex to CMYK: http://www.ginifab.com/feeds/pms/cmyk_to_rgb.php
		public static readonly CMYKColors CMYK_GREEN = new CMYKColors() { Cyan = 100, Magenta = 0, Yellow = 85, Black = 41 };
		public static readonly CMYKColors CMYK_YELLOW = new CMYKColors() { Cyan = 0, Magenta = 2, Yellow = 100, Black = 17 };
		public static readonly CMYKColors CMYK_RED = new CMYKColors() { Cyan = 0, Magenta = 100, Yellow = 100, Black = 11 };
		public static readonly CMYKColors CMYK_LESS_BLUE = new CMYKColors() { Cyan = 29, Magenta = 13, Yellow = 0, Black = 24 };
		public static readonly CMYKColors CMYK_BLUE = new CMYKColors() { Cyan = 54, Magenta = 24, Yellow = 0, Black = 44 };
		public static readonly CMYKColors CMYK_BLACK = new CMYKColors() { Cyan = 0, Magenta = 0, Yellow = 0, Black = 61 };
		public static readonly CMYKColors CMYK_WHITE = new CMYKColors() { Cyan = 0, Magenta = 0, Yellow = 0, Black = 0 };

		public enum CellFontFormat : int
		{
			Default,
			ReportTitle,
			ChartTitle,
			GridHeaderCell,
			GridHeaderFirstCell,
			GridHeaderEndCell,
			TextGreaterGoal,
			TextInGoal,
			TextLessGoal,
			GreaterGoalCell,
			InGoalCell,
			LessGoalCell,
			SumCell,
			ForecastCell,
			SubHeaderCell,
			NormalCell,
			RegionCell,
			SiteCell,
			RiskFactorNumberCell,
			GridHeaderListGroup,
			GridGroupFirstCell,
			GridGroupCell
		}

		public enum ChartType : int
		{
			LineChart= 1,
			BarChart= 2,
			ColumnChart= 3,
			PieChart= 4
		}

		public enum ChartColor: int
		{
			Default = 0,
			Red,
			Green,
			Yellow,
			Blue,
			Orange
		}

		public enum RowDataType : int
		{
			Header = 1,
			Body,
			Footer
		}

		public enum GridNameList
		{
			DashboardMetric,
			DashboardMetricDetail
		}

		public enum ReportType
		{
			Default = 1,
			RebarDashBoard
		}
	}
	
}
