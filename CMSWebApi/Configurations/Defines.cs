using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Configurations
{
	public enum Period : byte
	{
		None = 0,
		Day,
		Week,
		Month,
		Year
	}
	public enum MailServerType
	{
		SMTP = 0,
		EXCHANGE
	}
	public enum MailSercure
	{
		Auto = 0,
		TSL = 1,
		SSL = 2
	}
	internal static class Defines
	{
		
		public const int Default_Items = 100000;
		public const int Default_Parallelism = 0;
		public const int Max_Items = 10 * Default_Items;
		public const int Default_Interval = 1;
		#region EmailSettings
		public const int Default_Mailserver_Port = 25;
		public const string STR_EXCHANGE = "EXCHANGE";
		public const string STR_DEFAULT_MAIL_TYPE = "SMTP";
		public const string STR_DEFAULT_MAIL_SERCURE = "Auto";
		public const string STR_Host = "host";
		public const string STR_AutodiscoverUrl = "AutodiscoverUrl";
		public const string STR_Version = "Version";
		public const string STR_port = "port";
		public const string STR_tsl_ssl = "tsl";
		public const string STR_Type = "type";
		public const string STR_address = "address";
		public const string STR_userid = "userid";
		public const string STR_PWD = "pwd";
		public const string STR_password = "password";
		public const string STR_Server = "Server";
		public const string STR_Account = "Account";
		
		#endregion
		#region dashboard
		public const string STR_name = "name";
		public const string STR_items = "items";
		public const string STR_chunkzise = "chunk-size";
		public const string STR_Parallelism = "parallelism";
		
		public const string STR_save = "save";
		public const string STR_enable = "enable";
		public const string STR_live = "live";
		public const string STR_DashBoards = "DashBoards";
		public const string STR_add= "add";
		public const string STR_remove = "remove";
		public const string STR_clear = "clear";
		public const string STR_interval = "interval";
		public const string STR_Period = "period";
		public const string STR_autoupdate = "autoupdate";
		public const string STR_Tables = "Tables";
		public const string STR_Table = "Table";

		#endregion
		#region api config
		public const string Allow = "Allow";
		public const string ip = "ip";
		public const string value = "value";

		#endregion
		public static DateTime PeriodDate( DateTime date, Period period, int interval)
		{
			DateTime ret_date = date;
			switch(period)
			{
				case Period.Day:
				ret_date = date.AddDays(-interval);
				break;
				case Period.Month:
				ret_date = date.AddMonths(-interval);
				break;
				case Period.Week:
				ret_date = date.AddDays(-7 * interval);
				break;
				case Period.Year:
				ret_date = date.AddYears(- interval);
				break;
				default:
					ret_date = date.Date;
				break;
			}
			return ret_date;
		}
	}
}
