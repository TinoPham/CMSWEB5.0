using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CMSSVR
{
	public static class Utils
	{
		public const string TokenKey_Config = "TokenKey";
		public const string STR_controller = "controller";
		public const string STR_action = "action";
		public const string STR_Token = "Token";
		public const string PACDM_CONNECTION_KEY = "PACDMDB";
		public const string LOGDB_CONNECTION_KEY = "LogContext";
		public static string  EncryptKey { get{ return ConfigurationManager.AppSettings.Get(TokenKey_Config);}}
		public static string PACDMConnection { get { return ConfigurationManager.ConnectionStrings[PACDM_CONNECTION_KEY].ConnectionString; } }
	}
}