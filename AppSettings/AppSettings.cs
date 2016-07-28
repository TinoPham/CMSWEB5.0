using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using CMSWebApi.Configurations;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using LicenseInfo.Models;

namespace AppSettings
{
	public sealed class AppSettings : Commons.SingletonClassBase<AppSettings>
	{

		internal const int Default_LoginRetry = 5;
		internal const int Default_LoginNextInter = 15;
		internal const int Default_MsgEncrypt = 2;//0: disable, 1 always enable, 2: auto
		internal const int Default_IdleTimeout = 600;
		internal const int Default_MsgDelay = 60;
		internal const int Default_RestorePassTime = 24;
		internal const int Default_SnapshotBufferSize = 16384;
		internal const int Default_UserImageSize = 71680;
		internal const int Default_RecordDayExpected = 50;
		internal const int Default_NotesPeriodDay = 1;
		
		internal const byte Default_ForecastFormular = 1;
		internal const int Default_ForecastWeeks = 5;
		internal const int Default_Image_Offset = 1; //1 second
		internal const int Default_Image_Width = 320;
		internal const int Default_Image_Height = 240;
		internal const int Default_Image_Recycle_Space = 0; //500 //MB
		internal const int Default_Image_Recycle_Days = 0;//30;
		internal const int Default_Image_Recycle_Disable = -1;
        internal const int Default_BHideWH = 1;         // Set default true
		internal const string Default_Report_Path = "Report";
		internal const int Default_DVRBackupConfig_Interval = 2;
		internal const int Default_DVRBackupConfig_Keep = 4;

		public const string RawImages = "RawImages";
		public const string AlertImages = "AlertImages";
		public const string TokenKey_Config = "TokenKey";
		internal const string Default_TokenKey = "Be8ampLYhi4f8y3pOWDLV1GzfIvH34GH6TaPHwqYS/I=";
		internal const string LoginRetry_Key = "LoginRetry";
		internal const string LoginNextInter_Key = "LoginNextInter";
		internal const string MsgEncrypt_Key = "MsgEncrypt";
		internal const string IdleTimeout_Key = "IdleTimeout";
		internal const string MsgDelay_Key = "MsgDelay";
		internal const string RestorePassTime_Key = "RestorePassTime";
		internal const string SnapshotBufferSize_Key = "SnapshotBufferSize";
		internal const string UserImageSize_Key = "UserImageSize";
		internal const string RecordDayExpected_Key = "RecordDayExpected";
		internal const string NotesPeriodDay_key = "NotesPeriodDay";
		internal const string DataPath_Key = "DataPath";
		internal const string ImagePath_Key = "ImagePath";
		internal const string ReportPath_Key = "ReportPath";

		internal const string STR_controller = "controller";
		internal const string STR_action = "action";
		internal const string STR_Token = "Token";
		internal const string PACDM_CONNECTION_KEY = "PACDMDB";
		internal const string LOGDB_CONNECTION_KEY = "LogContext";
		internal const string USERS = "Users";
		internal const string EmailFormatFile = "ForgotPasswordEmailFormat";
		internal const string DvrRecordingLessThanStr = "DVR Recording less than {0} days";
		internal const string LOGS = "Logs";
		internal const string Sites = "Sites";
		internal const string COOKIE_EXPIRE_KEY = "CookieExpired";
		internal const string Dvr = "DVR";
		internal const string FORECAST_FORMULAR_KEY = "ForecastFomular";
		internal const string FORECAST_WEEKS_KEY = "ForecastWeeks";

		internal const string IMAGEALERT_RECYCLE = "ImageAlertRecycle";
		internal const string IMAGEALERT_OFFSET = "ImageAlertOffset";
		internal const string IMAGEALERT_WIDTH = "ImageAlertWidth";
		internal const string IMAGEALERT_HEIGHT = "ImageAlerttHeight";
		internal const string str_day = "day";
		internal const string str_Mb = "Mb";
		internal const string AdvertisementKey = "Advertisement";
		public const string License_Info_File = "License.lic";
		public const string _3rd_Info_File = "3rd_app.json";
		internal const string ExportFolder_Name = "ExportTemplates";
		internal const string DBJobName_Key = "DBJobName";
		// Tri Create name flag hideWH
		internal const string B_HIDEWH = "B_HideWH";
		internal const string DVR_BACKUPCONFIG_INTERVAL = "DVRBackupConfigInterval";//1: Daily, 2: Weekly, 3: Monthly
		internal const string DVR_BACKUPCONFIG_KEEP = "DVRBackupConfigKeep";

		public const string Excel_Exstention = ".xlsx";
		
		//static string EncryptKey { get { return ConfigurationManager.AppSettings.Get(TokenKey_Config); } }
		//static string PACDMConnection { get { return ConfigurationManager.ConnectionStrings [PACDM_CONNECTION_KEY].ConnectionString; } }

		/// <summary>
		/// Passwork key that using to encrypt token for DVR Converter
		/// </summary>
		public string DVRTokenKey { get{ return AppDVRTokenKey.Instance.Value;}}
		/// <summary>
		///# of time allow user input wrong user/pass
		/// </summary>
		public int CMSWeb_LoginRetry { get{ return AppCMSWeb_LoginRetry.Instance.Value; }}
		/// <summary>
		/// after inputting wrong user/pass with #CMSWeb_LoginRetry
		/// </summary>
		public int CMSWeb_LoginNextInter { get{ return AppCMSWeb_LoginNextInter.Instance.Value;}}

		public int MessageEncrypt { get; private set; }

		public int IdleTimeout { get { return SettingIdleTimeout.Instance.Value; } }

		public int MessageDelay {get{ return AppMessageEncrypt.Instance.Value;}}

		public string PACDM_Model{ get{ return PACDM_CONNECTION_KEY;}}

		public string PACDMConnection { get; private set;}

		public string SitesPath { get; set; }

		public string DvrPath { get; set; }

		public string AlertImagesPath { get; set; }

		public string LogDB_Model { get { return LOGDB_CONNECTION_KEY; } }

		public int RestorePassTime { get { return AppRestorePassTime.Instance.Value;}}

		public int SnapShotSize { get{ return AppSnapShotSize.Instance.Value;}}

		public int UserImageSize { get{ return AppUserImageSize.Instance.Value;}}

		public int RecordDayExpected { get{ return AppRecordDayExpected.Instance.Value;}}

		public int NotesPeriodDay { get{ return AppNotesPeriodDay.Instance.Value;}}
		
		public string AppData{ get{ return AppDataPath.Instance.Value;}}

		public string UsersPath { get; private set; }

		public string EmailForgotSettingPath { get; private set; }

		public string DvrRecordingLessThan { get; private set; }

		public string LogsPath { get; private set; }

		public int CookieExpired{ get { return AppCookieExpired.Instance.Value;}}

		public byte ForecastFomular { get{ return AppForecastFomular.Instance.Value;}}

		public int ForecastWeeks { get{ return AppForecastWeeks.Instance.Value;} }

        public int BHideWH { get { return AppBHideWH.Instance.Value; } }

		public int ImageAlertOffset { get{ return AppImageAlertOffset.Instance.Value;}}

		public int ImageAlertWidth { get{ return AppImageAlertWidth.Instance.Value;}}

		public int ImageAlertHeight { get{ return AppImageAlertHeight.Instance.Value;}}

		public int ImageAlertRecycleSpace { get{ return AppImageRecycleConfig.Instance.ImageAlertRecycleSpace;}}

		public int ImageAlertRecycleDays { get { return AppImageRecycleConfig.Instance.ImageAlertRecycleDays; } }

		public string ServerID { get { return AppServerID.Instance.Value;}}

		public string JobName { get { return AppServerID.Instance.JobName; } }

		public string ReportPath { get { return global::AppSettings.ReportPath.Instance.Value; } }
		
		public string AdvertisementPath { get; private set; }

		public CachesConfig CacheConfig { get{ return AppCachesConfig.Instance.Value;}}

		public TableCacheConfigCollection TableCaches{ get { return CacheConfig == null ? null : CacheConfig.Tables;}}

		public DashboardCacheConfigCollection DashboardCaches { get { return CacheConfig == null ? null : CacheConfig.DashBoards; } }

		public EmailSettingSection EmailSetting { get{ return AppEmailSetting.Instance.Value;}}

		public CMSWebApi.Configurations.ApiConfigs ApiConfigs { get{ return AppApiConfigs.Instance.Value;}}

		public LicenseModel Licenseinfo { get { return LicenseInfo.Instance.License; } }

		public int DVRBackupConfigInterval { get { return AppDVRBackupConfigInterval.Instance.Value; } }
		public _3rdAppSettings _3rdAppSettings { get { return _3rdAppSettings.Instance; } }
		public int DVRBackupConfigKeep { get { return AppDVRBackupConfigKeep.Instance.Value; } }

        public string ExportFolder { get; set; }
       // public string Excel_File_Extension { get; set; }
		private AppSettings()
		{

			#region move to classes
			//DVRTokenKey = GetValue<string>(ConfigurationManager.AppSettings, TokenKey_Config, Default_TokenKey, item => string.IsNullOrEmpty(item));

			//CMSWeb_LoginRetry = GetValue<int>(ConfigurationManager.AppSettings, LoginRetry_Key, Default_LoginRetry, item => invalidNumber(item));

			//CMSWeb_LoginNextInter = GetValue<int>(ConfigurationManager.AppSettings, LoginNextInter_Key, Default_LoginNextInter, item => invalidNumber(item));

			//MessageEncrypt = GetValue<int>(ConfigurationManager.AppSettings, MsgEncrypt_Key, Default_MsgEncrypt, item => invalidNumber(item));

			//MessageDelay = GetValue<int>(ConfigurationManager.AppSettings, MsgDelay_Key, Default_MsgDelay, item => invalidNumber(item));

			//RestorePassTime = GetValue<int>(ConfigurationManager.AppSettings, RestorePassTime_Key, Default_RestorePassTime, item => invalidNumber(item));
			#endregion

			PACDMConnection = ConfigurationManager.ConnectionStrings [PACDM_CONNECTION_KEY].ConnectionString;

			#region move to classes
			//SnapShotSize = GetValue<int>(ConfigurationManager.AppSettings, SnapshotBufferSize_Key, Default_SnapshotBufferSize, item => invalidNumber(item));

			//UserImageSize = GetValue<int>(ConfigurationManager.AppSettings, UserImageSize_Key, Default_UserImageSize, item => invalidNumber(item));

			//RecordDayExpected = GetValue<int>(ConfigurationManager.AppSettings, RecordDayExpected_Key, Default_RecordDayExpected, item => invalidNumber(item));

			//NotesPeriodDay = GetValue<int>(ConfigurationManager.AppSettings, NotesPeriodDay_key, Default_NotesPeriodDay, item => invalidNumber(item));
			
			//AppData = GetValue<string>(ConfigurationManager.AppSettings, DataPath_Key, AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), item => invalidPath(item));

			//CookieExpired = GetValue<int>(ConfigurationManager.AppSettings, COOKIE_EXPIRE_KEY, Default_Cookie_Expire, item => invalidNumber(item));
			//if(CookieExpired <= 0)
			//	CookieExpired = Default_Cookie_Expire;

			//ForecastFomular = GetValue<byte>(ConfigurationManager.AppSettings, FORECAST_FORMULAR_KEY, Default_ForecastFormular, item => invalidByte(item));
			//if (ForecastFomular <= 0)
			//	ForecastFomular = Default_ForecastFormular;

			//ForecastWeeks = GetValue<int>(ConfigurationManager.AppSettings, FORECAST_WEEKS_KEY, Default_ForecastWeeks, item => invalidNumber(item));
			//if (ForecastWeeks <= 0)
			//	ForecastWeeks = Default_ForecastWeeks;
			#endregion

			UsersPath = Path.Combine(AppData, USERS);
			EmailForgotSettingPath = Path.Combine(AppData, EmailFormatFile);
			DvrRecordingLessThan = DvrRecordingLessThanStr;

			LogsPath = Path.Combine(AppData, LOGS);
			SitesPath = Path.Combine(AppData, Sites);
			DvrPath = Path.Combine(AppData, Dvr);
			AlertImagesPath = Path.Combine(DvrPath, AlertImages);
            ExportFolder = Path.Combine(AppData,ExportFolder_Name);
			//string advertise = GetValue<string>(ConfigurationManager.AppSettings, AdvertisementKey, string.Empty, item => string.IsNullOrEmpty(item));
			AdvertisementPath = !string.IsNullOrEmpty(AppAdvertisementPath.Instance.Value) ? Path.Combine(AppData, AppAdvertisementPath.Instance.Value) : string.Empty;
			#region move to classes
			//CacheConfig = ConfigurationManager.GetSection(CachesConfig.CachesSection_Name) as CachesConfig;

			//EmailSetting = ConfigurationManager.GetSection(EmailSettingSection.EmailSettingsSection_Name) as EmailSettingSection;

			//ApiConfigs = ConfigurationManager.GetSection(ApiConfigs.ApiConfigs_Name) as ApiConfigs;

			
			//	ImageAlertOffset = Default_Image_Offset;

			//ImageAlertWidth = GetValue<int>(ConfigurationManager.AppSettings, IMAGEALERT_WIDTH, Default_Image_Width, item => invalidNumber(item));
			//if (ImageAlertWidth <= 0)
			//	ImageAlertWidth = Default_Image_Width;

			//ImageAlertHeight = GetValue<int>(ConfigurationManager.AppSettings, IMAGEALERT_HEIGHT, Default_Image_Height, item => invalidNumber(item));
			//if (ImageAlertHeight <= 0)
			//	ImageAlertHeight = Default_Image_Height;
			//string imageRecycleCfg = GetValue<string>(ConfigurationManager.AppSettings, IMAGEALERT_RECYCLE, string.Empty, item => string.IsNullOrEmpty(item));
			//UpdateImageRecycleConfig(imageRecycleCfg);
			#endregion
		}
		#region move to new classes
		//public void UpdateImageRecycleConfig(string config)
		//{
		//	if (string.IsNullOrEmpty(config))
		//	{
		//		ImageAlertRecycleSpace = Default_Image_Recycle_Space;
		//		ImageAlertRecycleDays = Default_Image_Recycle_Days;
		//		return;
		//	}

		//	string[] configs = config.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		//	string str_config = configs.FirstOrDefault(item => item.EndsWith(str_day, StringComparison.InvariantCultureIgnoreCase));
		//	if (!string.IsNullOrEmpty(str_config))
		//	{
		//		ImageAlertRecycleDays = GetConfigValue(str_config);
		//	}
		//	else
		//	{
		//		ImageAlertRecycleDays = Default_Image_Recycle_Disable;
		//	}
		//	str_config = configs.FirstOrDefault(item => item.EndsWith(str_Mb, StringComparison.InvariantCultureIgnoreCase));
		//	if (!string.IsNullOrEmpty(str_config))
		//	{
		//		ImageAlertRecycleSpace = GetConfigValue(str_config);
		//	}
		//	else
		//	{
		//		ImageAlertRecycleSpace = Default_Image_Recycle_Disable;
		//	}
		//}
		
		//private T GetValue<T>(NameValueCollection collection ,string key, T default_value, Func<string,bool> compareinvalid )
		//{
		//	string str_value = GetValue( collection, key);
		//	if( compareinvalid.Invoke( str_value))
		//		return default_value;
		//	return Commons.Utils.ChangeSimpleType<T>(str_value);
		//}

		//private string GetValue(NameValueCollection collection ,string key)
		//{
		//	return (collection == null || !collection.AllKeys.Contains(key)) ? null : collection[key];
		//}

		//private int GetConfigValue(string config)
		//{
		//	Regex rx = new Regex(@"^(?<Num>\d+)");
		//	Match match = rx.Match(config);
		//	if (!match.Success)
		//		return 0;
		//	return Convert.ToInt32(match.Groups["Num"].Value);
		//}

		//private bool invalidString(string value)
		//{
		//	return string.IsNullOrEmpty(value);
		//}

		//private bool invalidNumber( string value)
		//{
		//	if( string.IsNullOrEmpty( value))
		//		return true;
		//	int outvalue = 0;
		//	return !Int32.TryParse(value,out outvalue);
		//}
		
		//private bool invalidByte(string value)
		//{
		//	if (string.IsNullOrEmpty(value))
		//		return true;
		//	byte outvalue = 0;
		//	return !byte.TryParse(value, out outvalue);
		//}
		
		//private bool invalidPath( string value)
		//{
		//	if( string.IsNullOrEmpty( value) || value == ".")
		//		return true;
		//	 return !System.IO.Directory.Exists(value);
		//}

		 #endregion
	}

	
	internal class AppAdvertisementPath : ConfigFieldKey<AppAdvertisementPath, String>
	{
		AppAdvertisementPath(){
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.AdvertisementKey, string.Empty, item => string.IsNullOrEmpty(item));
		}
	}
	internal class AppDataPath : ConfigFieldKey<AppDataPath, String>
	{
		AppDataPath()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.DataPath_Key, AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), item => invalidPath(item));
		}
	}
	
	internal class AppApiConfigs : ConfigFieldKey<AppApiConfigs, ApiConfigs>
	{
		AppApiConfigs()
		{
			Value = ConfigurationManager.GetSection(ApiConfigs.ApiConfigs_Name) as ApiConfigs;
		}
	}

	internal class AppEmailSetting : ConfigFieldKey<AppEmailSetting, EmailSettingSection>
	{
		AppEmailSetting()
		{
			Value = ConfigurationManager.GetSection(EmailSettingSection.EmailSettingsSection_Name) as EmailSettingSection;
		}
	}

	internal class AppCachesConfig : ConfigFieldKey<AppCachesConfig, CachesConfig>
	{
		AppCachesConfig()
		{
			Value = ConfigurationManager.GetSection(CachesConfig.CachesSection_Name) as CachesConfig;
		}
	}
	
	internal class AppImageRecycleConfig: ConfigFieldKey<AppImageRecycleConfig, string>
	{
		public int ImageAlertRecycleSpace{ get; private set;}

		public int ImageAlertRecycleDays{ get ;private set;}

		AppImageRecycleConfig()
		{
			string imageRecycleCfg = GetValue(ConfigurationManager.AppSettings, AppSettings.IMAGEALERT_RECYCLE, string.Empty, item => string.IsNullOrEmpty(item));
			UpdateImageRecycleConfig(imageRecycleCfg);
		}
		private void UpdateImageRecycleConfig(string config)
		{
			if (string.IsNullOrEmpty(config))
			{
				ImageAlertRecycleSpace = AppSettings.Default_Image_Recycle_Space;
				ImageAlertRecycleDays = AppSettings.Default_Image_Recycle_Days;
				return;
			}

			string [] configs = config.Split(new char [] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			string str_config = configs.FirstOrDefault(item => item.EndsWith(AppSettings.str_day, StringComparison.InvariantCultureIgnoreCase));
			if (!string.IsNullOrEmpty(str_config))
			{
				ImageAlertRecycleDays = GetConfigValue(str_config);
			}
			else
			{
				ImageAlertRecycleDays = AppSettings.Default_Image_Recycle_Disable;
			}
			str_config = configs.FirstOrDefault(item => item.EndsWith(AppSettings.str_Mb, StringComparison.InvariantCultureIgnoreCase));
			if (!string.IsNullOrEmpty(str_config))
			{
				ImageAlertRecycleSpace = GetConfigValue(str_config);
			}
			else
			{
				ImageAlertRecycleSpace = AppSettings.Default_Image_Recycle_Disable;
			}
		}
	}
	
	internal class AppImageAlertHeight:  ConfigFieldKey<AppImageAlertHeight, int>
	{
		AppImageAlertHeight()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.IMAGEALERT_HEIGHT, AppSettings.Default_Image_Height, item => invalidNumber(item));
			if (Value <= 0)
				Value = AppSettings.Default_Image_Height;
		}
	}
	
	internal class AppImageAlertWidth : ConfigFieldKey<AppImageAlertWidth, int>
	{
		AppImageAlertWidth()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.IMAGEALERT_WIDTH, AppSettings.Default_Image_Width, item => invalidNumber(item));
			if (Value <= 0)
				Value = AppSettings.Default_Image_Width;
		}
	}

	internal class ReportPath : ConfigFieldKey<ReportPath, string>
	{
		ReportPath()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.ReportPath_Key, AppSettings.Default_Report_Path, item => string.IsNullOrEmpty(item));
		}
	}
	
	internal class AppImageAlertOffset : ConfigFieldKey<AppImageAlertOffset, int>
	{
		AppImageAlertOffset()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.IMAGEALERT_OFFSET, AppSettings.Default_Image_Offset, item => invalidNumber(item));
			if (Value <= 0)
				Value = AppSettings.Default_Image_Offset;
		}
	}
	
	internal class AppForecastWeeks : ConfigFieldKey<AppForecastWeeks, int>
	{
		AppForecastWeeks()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.FORECAST_WEEKS_KEY, AppSettings.Default_ForecastWeeks, item => invalidNumber(item));
			if (Value <= 0)
				Value = AppSettings.Default_ForecastWeeks;
		}
	}

    internal class AppBHideWH : ConfigFieldKey<AppBHideWH, int>
    {
        AppBHideWH()
        {
            Value = GetValue(ConfigurationManager.AppSettings, AppSettings.B_HIDEWH, AppSettings.Default_BHideWH, item => invalidNumber(item));
            if (Value > 1 || Value < 0)
                Value = AppSettings.Default_BHideWH;
        }
    }
	
	internal class AppForecastFomular : ConfigFieldKey<AppForecastFomular, byte>
	{
		AppForecastFomular()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.FORECAST_FORMULAR_KEY, AppSettings.Default_ForecastFormular, item => invalidByte(item));
			if (Value <= 0)
				Value = AppSettings.Default_ForecastFormular;
		}
	}
	
	internal class AppCookieExpired : ConfigFieldKey<AppCookieExpired, int>
	{
		internal readonly int Default_Cookie_Expire = (int)new TimeSpan(1, 0, 0, 0).TotalSeconds * 7;
		AppCookieExpired(){
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.COOKIE_EXPIRE_KEY, Default_Cookie_Expire, item => invalidNumber(item));
			if (Value <= 0)
				Value = Default_Cookie_Expire;
		}
	}
	
	internal class AppNotesPeriodDay : ConfigFieldKey<AppNotesPeriodDay, int>
	{
		AppNotesPeriodDay()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.NotesPeriodDay_key, AppSettings.Default_NotesPeriodDay, item => invalidNumber(item));
		}
	}

	internal class AppRecordDayExpected : ConfigFieldKey<AppRecordDayExpected, int>
	{
		AppRecordDayExpected(){
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.RecordDayExpected_Key, AppSettings.Default_RecordDayExpected, item => invalidNumber(item));
		}
	}

	internal class AppUserImageSize : ConfigFieldKey<AppUserImageSize, int>
	{
		AppUserImageSize(){
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.UserImageSize_Key, AppSettings.Default_UserImageSize, item => invalidNumber(item));
		}
	}

	internal sealed class AppServerID : ConfigFieldKey<AppServerID, string>//Commons.SingletonClassBase<AppServerID>
	{
		public string JobName{ get; private set;}
		const string SERVERID_ENCRYPTKEY = "K2vYadJ9iK0/dj73/mekCdUQp0DeQPY0TudRH+x7fdM=";
		//private string _ServerID;
		//public string ServerID { get { return _ServerID; } }
		private AppServerID()
		{
			Value = GetServerID();
		}
		private string GetJobName( string dbanme)
		{
			string jname = GetValue(ConfigurationManager.AppSettings, AppSettings.DBJobName_Key, string.Empty, item => string.IsNullOrEmpty(item));
			if( string.IsNullOrEmpty(jname))
				return dbanme + "_PACDW_JOB";
			return jname;
		}
		private string GetServerID()
		{
			ConnectionStringSettings consettings = ConfigurationManager.ConnectionStrings [AppSettings.PACDM_CONNECTION_KEY];
			EntityConnectionStringBuilder conbuilder = new EntityConnectionStringBuilder(consettings.ConnectionString);
			SqlConnectionStringBuilder sqlconbuilder = new SqlConnectionStringBuilder(conbuilder.ProviderConnectionString);
			string hostname = System.Environment.MachineName;
			string servername = sqlconbuilder.DataSource;
			string dbname = sqlconbuilder.InitialCatalog;
			string orgtoken = hostname + servername + dbname + SERVERID_ENCRYPTKEY;
			JobName = GetJobName(dbname);
			return Cryptography.SHA.ComputeHash(orgtoken.ToLower(), SERVERID_ENCRYPTKEY);
		}
	}

	internal sealed class AppDVRTokenKey : ConfigFieldKey<AppDVRTokenKey, string>
	{

		private AppDVRTokenKey()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.TokenKey_Config, AppSettings.Default_TokenKey, item => string.IsNullOrEmpty(item));
		}
	}

	internal sealed class AppCMSWeb_LoginRetry : ConfigFieldKey<AppCMSWeb_LoginRetry, int>
	{
		private AppCMSWeb_LoginRetry()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.LoginRetry_Key, AppSettings.Default_LoginRetry, item => invalidNumber(item));
		}
	}

	internal sealed class AppCMSWeb_LoginNextInter: ConfigFieldKey<AppCMSWeb_LoginNextInter,int>
	{
		private AppCMSWeb_LoginNextInter()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.LoginNextInter_Key, AppSettings.Default_LoginNextInter, item => invalidNumber(item));
		}
	}
	
	internal sealed class AppMessageEncrypt : ConfigFieldKey<AppMessageEncrypt, int>
	{
		private AppMessageEncrypt()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.MsgEncrypt_Key, AppSettings.Default_MsgEncrypt, item => invalidNumber(item));
		}
	}

	internal sealed class SettingIdleTimeout : ConfigFieldKey<SettingIdleTimeout, int>
	{
		private SettingIdleTimeout()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.IdleTimeout_Key, AppSettings.Default_IdleTimeout, item => invalidNumber(item));
		}
	}

	internal sealed class AppRestorePassTime : ConfigFieldKey<AppRestorePassTime, int>
	{
		AppRestorePassTime()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.RestorePassTime_Key, AppSettings.Default_RestorePassTime, item => invalidNumber(item));
		}
	}

	internal sealed class AppSnapShotSize : ConfigFieldKey<AppSnapShotSize, int>
	{
		AppSnapShotSize()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.SnapshotBufferSize_Key, AppSettings.Default_SnapshotBufferSize, item => invalidNumber(item));
		}
	}

	internal class AppDVRBackupConfigInterval : ConfigFieldKey<AppDVRBackupConfigInterval, int>
	{
		AppDVRBackupConfigInterval()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.DVR_BACKUPCONFIG_INTERVAL, AppSettings.Default_DVRBackupConfig_Interval, item => invalidNumber(item));
		}
	}

	internal class AppDVRBackupConfigKeep : ConfigFieldKey<AppDVRBackupConfigKeep, int>
	{
		AppDVRBackupConfigKeep()
		{
			Value = GetValue(ConfigurationManager.AppSettings, AppSettings.DVR_BACKUPCONFIG_KEEP, AppSettings.Default_DVRBackupConfig_Keep, item => invalidNumber(item));
		}
	}

	internal abstract class ConfigFieldKey<T,Tout>: Commons.SingletonClassBase<T> where T :class
	{
		public virtual Tout Value{ get;protected set; }

		protected Tout GetValue(NameValueCollection collection, string key, Tout default_value, Func<string, bool> compareinvalid)
		{
			string str_value = GetValue(collection, key);
			if (compareinvalid.Invoke(str_value))
				return default_value;
			return Commons.Utils.ChangeSimpleType<Tout>(str_value);
		}

		protected string GetValue(NameValueCollection collection, string key)
		{
			return (collection == null || !collection.AllKeys.Contains(key)) ? null : collection [key];
		}

		protected int GetConfigValue(string config)
		{
			Regex rx = new Regex(@"^(?<Num>\d+)");
			Match match = rx.Match(config);
			if (!match.Success)
				return 0;
			return Convert.ToInt32(match.Groups ["Num"].Value);
		}

		protected bool invalidString(string value)
		{
			return string.IsNullOrEmpty(value);
		}

		protected bool invalidNumber(string value)
		{
			if (string.IsNullOrEmpty(value))
				return true;
			int outvalue = 0;
			return !Int32.TryParse(value, out outvalue);
		}

		protected bool invalidByte(string value)
		{
			if (string.IsNullOrEmpty(value))
				return true;
			byte outvalue = 0;
			return !byte.TryParse(value, out outvalue);
		}
		protected bool invalidPath(string value)
		{
			if (string.IsNullOrEmpty(value) || value == ".")
				return true;
			return !System.IO.Directory.Exists(value);
		}
	} 
}
