using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConverterDB.Model;
using Commons;
using Microsoft.Win32;

namespace PACDMConverter
{
	internal sealed class Messages{
		public const string STR_START_SUCCESSFULL = "Service started successfully.";
		public const string STR_STOP_SUCCESSFULL = "Service stopped successfully.";
		public const string STR_MSG_CANNOT_CREATE_FOLDER = "Cannot create folder: '{0}'";
	}
	internal abstract class Consts
	{
		public const int Last_Month_Convert_Data_Support = 24;
		public const int DVR_SOCKET_RETRY = 10;
		public const int MIN_CONVERT_INTERVAL = 10;
		public const int DEFAULT_DVR_MSG = 10;
		public const int DEFAULT_LOG_RECYCLE = 10;
		public const int DEFAULT_CMS_TCP_PORT = 1000;
		public const int MAX_CONVERT_INTERVAL = Int16.MaxValue;

		public const string PIPE_NAME = "ServiceConfig_D37A4DC7-315C-45C4-9C40-9DEADD4F195E";
		public const string Cmd_Stop_service = "-s";
		public const string Cmd_Install = "-i";
		public const string Cmd_KillProcessID = "-k";

		public const string STR_UPGRADE = "upgrade";
		public const string STR_DEFAULT_CULTURE = "en-US";
		public const string STR_SOFTWARE = "SOFTWARE";
		public const string STR_i3DVR = "i3DVR";
		public const string STR_I3Pro = "I3Pro";
		public const string STR_Settings = "Settings";
		public const string STR_SRX_Pro_Version = "SRX-Pro Version";
		public const string STR_ServerAppPath = "ServerAppPath";
		public const string STR_DATADIR = "DATADIR";
		public const string STR_PACID = "PACID";
		public const string STR_Version = "Version";
		public const string STR_PAC = "PAC";
		public const string HEXA_FORMAT = "X2";
		public const string LogContextConnection = "LocalDb";
		public const string PACEncodeTypeKey = "PACEncodeType";
		public const string DVREncodeTypeKey = "DVREncodeType";
		public const string PACConvertMode = "PACConvertMode";
		public const string DVRSocketConnectionKey= "DVRSocketConnection";
		public const string STR_JSON = "json";
		public const string LOG_SOURCE = "PACDM Converter";
		public const string STR_APPLICATION = "Application";
		public const string STR_CONVERTER = "PACDM Converter";
		public const string RX_YEAR = @"^\d{4}?";
		public const string RX_DB_FILE_EXTENSION = @"^(?<Year>\d{4})_(?<Month>\d{2})_(?<Day>\d{2})\.mdb$";
		public const string RX_DB_FILE = @"^(?<Year>\d{4})_(?<Month>\d{2})_(?<Day>\d{2})$";
		public const string ReportingDB_FileName = "ReportingDb.mdb";
		public const string STAR_Sign = "*";
		public const string str_DomainSid = "DomainSid";
		public const string str_AskDomain = "AskDomain";
		public const string STR_HaspLicense = "hasp_license";

		public const string STR_ConverterURL = "api/converter/Converter/";
		public const string STR_Http = "http://";
		public const string STR_Https = "https://";
		public const string STR_PAC_DIR = @"D:\PAC";
		public const string STR_APPID = "AppID";
		public const string STR_APPPlatform = "Platform";
		public const string STR_Message = "Message";
		public const string APP_Upgrade = "AppUpgrade.exe";
		public const string STR_PACConverterVer = "PACConverterVer";
		public const string STR_MIN_Version = "1.0.0.0";
		
		#if DEBUG
		public const int Default_Http_TimeOut = 2*60;//2 mins
		#else
		public const int Default_Http_TimeOut = 90;//90 secs
		#endif
	}

	internal class Utils
	{
		private static readonly Lazy<Utils> sInstance = new Lazy<Utils>(() => new Utils());
		public static Utils Instance { get { return sInstance.Value; } }
		public readonly Version Min_SRXPRo_Agreement_Version = new Version(3, 3, 3, 43);
		readonly string _Platform;
		private Utils()
		{
			_Platform = IntPtr.Size == 4 ? "x86" : "x64";
			SRX_Pro_Reg_Path = Path.Combine(Consts.STR_SOFTWARE, Consts.STR_i3DVR, Consts.STR_I3Pro);
			SRX_Pro_Reg_Settings_Path = Path.Combine(SRX_Pro_Reg_Path, Consts.STR_Settings);

			PAC_Reg_Path = Path.Combine(Consts.STR_SOFTWARE, Consts.STR_PAC);
			
			
			string strver = GetRegValue(RegistryHive.LocalMachine,PAC_Reg_Path, Consts.STR_PACConverterVer); //RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, PAC_Reg_Path, Consts.STR_PACConverterVer); 
			Version = Commons.Utils.ParserVersion(strver); //Commons.Utils.String2Version(strver);
			if(Version == null)
				Version = new System.Version(Consts.STR_MIN_Version);

			_PACMediaFomatter = MediaFormatter(ConfigurationManager.AppSettings[Consts.PACEncodeTypeKey]);
			_DVRFormatter = MediaFormatter(ConfigurationManager.AppSettings[Consts.DVREncodeTypeKey]);
		}

		#region ZLib
		private const int MAX_ZIP_SIZE = 1024 * 1024 * 5;
		private const int DEFAULT_ZIP_SIZE = 1024 * 1024;
		internal enum ZLibError : int
		{
			Z_OK = 0,
			Z_STREAM_END = 1,
			Z_NEED_DICT = 2,
			Z_ERRNO = (-1),
			Z_STREAM_ERROR = (-2),
			Z_DATA_ERROR = (-3),
			Z_MEM_ERROR = (-4),
			Z_BUF_ERROR = (-5),
			Z_VERSION_ERROR = (-6),
		}

		internal enum ZLibCompressionLevel : int
		{
			Z_NO_COMPRESSION = 0,
			Z_BEST_SPEED = 1,
			Z_BEST_COMPRESSION = 9,
			Z_DEFAULT_COMPRESSION = (-1)
		}

		internal class ZLibWin32
		{
			//ZLibNet.zlib64.dll
			//[DllImport("zlib")]
			//public static extern string zlibVersion();
			#if X64
			[DllImport("ZLibNet.zlib64.dll")]
			public static extern ZLibError compress(byte[] dest, ref int destLength, byte[] source, int sourceLength);
			[DllImport("ZLibNet.zlib64.dll")]
			public static extern ZLibError compress2(byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibCompressionLevel level);
			[DllImport("ZLibNet.zlib64.dll")]
			public static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);
#else
			[DllImport("ZLibNet.zlib32.dll")]
			public static extern ZLibError compress(byte[] dest, ref int destLength, byte[] source, int sourceLength);
			[DllImport("ZLibNet.zlib32.dll")]
			public static extern ZLibError compress2(byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibCompressionLevel level);
			[DllImport("ZLibNet.zlib32.dll")]
			public static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);
#endif
		}

		public static byte[] ZipData(byte[] buff, int len)
		{
			if (buff == null || buff.Length == 0 || len == 0)
				return null;

			byte[] zipData = null;
			try
			{
				//FileStream st = new FileStream("D:\\compress.bin", FileMode.Create);
				//st.Write(buff,0, buff.Length);
				//st.Close();
				//st.Dispose();
				byte[] tmpbuff = new byte[DEFAULT_ZIP_SIZE];
				int datalen = DEFAULT_ZIP_SIZE;
				ZLibError err = ZLibWin32.compress(tmpbuff, ref datalen, buff, len);
				if (err != ZLibError.Z_OK)
				{
					Array.Clear(tmpbuff, 0, DEFAULT_ZIP_SIZE);
					tmpbuff = null;
					return null;
				}
				zipData = new byte[datalen];
				Array.Copy(tmpbuff, 0, zipData, 0, datalen);
				Array.Clear(tmpbuff, 0, DEFAULT_ZIP_SIZE);
				tmpbuff = null;
			}
			catch (Exception)
			{
				//sLastError = ex.Message;
			}
			return zipData;
		}

		public static byte[] UnZipData(byte[] buff, int len)
		{
			if (buff == null || buff.Length == 0 || len == 0)
				return null;

			byte[] rawdata = null;
			try
			{
				byte[] tmpbuff = new byte[MAX_ZIP_SIZE];
				int datalen = MAX_ZIP_SIZE;
				//FileStream st = new FileStream("D:\\uncompress.bin", FileMode.Create);
				//st.Write(buff, 0, buff.Length);
				//st.Close();
				//st.Dispose();

				ZLibError err = ZLibWin32.uncompress(tmpbuff, ref datalen, buff, len);
				if (err != ZLibError.Z_OK)
				{
					Array.Clear(tmpbuff, 0, MAX_ZIP_SIZE);
					tmpbuff = null;
					return null;
				}
				rawdata = new byte[datalen];
				Array.Copy(tmpbuff, 0, rawdata, 0, datalen);
				Array.Clear(tmpbuff, 0, MAX_ZIP_SIZE);
				tmpbuff = null;
			}
			catch (Exception)
			{
				//sLastError = ex.ToString();
			}
			return rawdata;
		}

		#endregion
		public PACDMConverter.ConvertBase.ConvertMode PACConvertMode
		{
			get{
				return Commons.Utils.GetEnum<PACDMConverter.ConvertBase.ConvertMode>(ConfigurationManager.AppSettings[Consts.PACConvertMode]);
			}
			 
		}
		 
		private MediaTypeFormatter _PACMediaFomatter;
		private MediaTypeFormatter _DVRFormatter;
		public MediaTypeFormatter DVRFormatter
		{
			get{
					return _DVRFormatter;
				}
		}
		public MediaTypeFormatter PACMediaFomatter
		{
		  get{ return _PACMediaFomatter;}
		}

		public int DVRSocketAllow
		{
			get{
				int numberdvr;
				string strnumber = ConfigurationManager.AppSettings[Consts.DVRSocketConnectionKey];
				Int32.TryParse( strnumber, out numberdvr);
				return numberdvr <= 0 || numberdvr >= DVRConverter.DVRUtils.DVR_CONNECTION_LIMIT ? DVRConverter.DVRUtils.DVR_CONNECTION_SUPPORT : numberdvr;
			}
		}

		private readonly CultureInfo _Culture = new CultureInfo(Consts.STR_DEFAULT_CULTURE);

		public CultureInfo CultureInfo{ get{ return _Culture;}}

		private readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
		
		public Version Version{ get; private set;}
		
		private readonly GuidAttribute AppGuID = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true).FirstOrDefault() as GuidAttribute;

		public string GetReg32Value(RegistryHive regHive, string Reg_Path, string regkey)
		{
			return RegistryUtils.GetRegValue(regHive, RegistryView.Registry32, Reg_Path, regkey);
		}
		public string GetReg64Value(RegistryHive regHive, string Reg_Path, string regkey)
		{
			return RegistryUtils.GetRegValue(regHive, RegistryView.Registry64, Reg_Path, regkey);
		}

		public string GetRegValue(RegistryHive regHive, string Reg_Path, string regkey)
		{
			if( regHive == RegistryHive.LocalMachine || regHive == RegistryHive.CurrentUser)
			{
#if X64
				return GetReg64Value(regHive, Reg_Path, regkey);
				//return  RegistryUtils.GetRegValue(regHive, RegistryView.Registry64, Reg_Path, regkey);
#else
				return GetReg32Value(regHive, Reg_Path, regkey);
				//return RegistryUtils.GetRegValue(regHive, RegistryView.Registry32, Reg_Path, regkey);
#endif
			}
			else
				return RegistryUtils.GetRegValue(regHive, RegistryView.Default, Reg_Path, regkey);
		}

		public bool SetRegValue(RegistryHive regHive, string Reg_Path, string regkey, string value)
		{
			if (regHive == RegistryHive.LocalMachine || regHive == RegistryHive.CurrentUser)
			{
#if X64
				return RegistryUtils.SetRegValue(regHive, RegistryView.Registry64, Reg_Path, regkey, value);
#else
				return RegistryUtils.SetRegValue(regHive, RegistryView.Registry32, Reg_Path, regkey, value);
#endif
			}
			else
				return RegistryUtils.SetRegValue(regHive, RegistryView.Default, Reg_Path, regkey, value);
		}

		public void DeleteRegValue(RegistryHive regHive, string Reg_Path, string regkey)
		{
			if (regHive == RegistryHive.LocalMachine || regHive == RegistryHive.CurrentUser)
			{
#if X64
				RegistryUtils.DeleteRegValue(regHive, RegistryView.Registry64, Reg_Path, regkey);
#else
				RegistryUtils.DeleteRegValue(regHive, RegistryView.Registry32, Reg_Path, regkey);
#endif
			}
			else
				RegistryUtils.DeleteRegValue(regHive, RegistryView.Default, Reg_Path, regkey);
		}

	
		public string Platform{ get { return _Platform;}}

		public string StartupDir
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
			 
		}

		public string CovnerterAppID{
			get{
				if( AppGuID == null)
					return string.Empty;
				return AppGuID.Value;
			}
		}

		public string CovnerterAgent{
			get{
				return string.Format("{0}. Version {1}", Consts.STR_CONVERTER, Version.ToString());
			}
		}
		
		public string SRX_Pro_Reg_Path 
		{
			get;
			private set;
		}

		public string SRX_Pro_Reg_Settings_Path { get; private set; }

		public string PAC_Reg_Path { get; private set; }
		
		public static DirectoryInfo[] DirInfo(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			if( string.IsNullOrEmpty(path))
				return  Enumerable.Empty<DirectoryInfo>().ToArray();

			DirectoryInfo dinfo = new DirectoryInfo( path);
			if( dinfo.Exists == false)
				return Enumerable.Empty<DirectoryInfo>().ToArray();

			return dinfo.GetDirectories(pattern, option);
		}
		
		public static FileInfo[] GetFileInfo(DirectoryInfo dinfo, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			if( dinfo == null || dinfo.Exists == false)
				return Enumerable.Empty<FileInfo>().ToArray();

			return dinfo.GetFiles(pattern, SearchOption.AllDirectories);
		}
		
		public static string[] ListFile(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			try
			{
			return Directory.GetFiles(path, pattern, option);
			}
			catch(Exception)
			{
				return Enumerable.Empty<string>().ToArray();
			}
		}

		public static string[] ListFolder(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			try
			{
			return Directory.GetDirectories(path, pattern, option);
			}
			catch(Exception)
			{
				return Enumerable.Empty<string>().ToArray();
			}
		}

		public static bool DeleteFile(FileInfo fInfo)
		{
			if( fInfo == null)
				return true;
			return DeleteFile(fInfo.FullName);
		}

		public static bool DeleteFile( string Filepath)
		{
			if( string.IsNullOrEmpty( Filepath) || !File.Exists(Filepath))
				return true;
			try
			{
				File.Delete(Filepath);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
			
		}

		public static bool CreateDir( string dir)
		{
			if( string.IsNullOrEmpty(dir))
				return false;
			if( Directory.Exists(dir))
				return true;
			try
			{
				DirectoryInfo dinfo = Directory.CreateDirectory(dir);
				return dinfo.Exists;
			} catch(Exception ){ return false;}
		}
		
		private static Match GetMatch(string regex, string data, RegexOptions option = RegexOptions.IgnoreCase)
		{
			Regex rx = new Regex(regex, option);
			return rx.Match(data);
		}

		public static DateTime DataFile2Date(string filename)
		{
			Match match = GetMatch(Consts.RX_DB_FILE_EXTENSION, filename);
			if( !match.Success)
				match = GetMatch(Consts.RX_DB_FILE, filename);
			if( match.Success)
			{
				return new DateTime(Convert.ToInt16(match.Groups["Year"].Value), Convert.ToInt16(match.Groups["Month"].Value), Convert.ToInt16(match.Groups["Day"].Value));
			}
			else
				return DateTime.MinValue;
		}
		
		private static MediaTypeFormatter MediaFormatter(string value)
		{
			if( string.IsNullOrEmpty( value) || string.Compare(value, Consts.STR_JSON, true ) == 0)
				return new JsonMediaTypeFormatter();
			return new XmlMediaTypeFormatter();
		}

		public static bool FileCopy( string src, string des)
		{
			if(!File.Exists(src))
				return false;
			try
			{
				File.Copy( src, des, true);
				return true;
			}
			catch(Exception){return false;}
		}
	}

	internal class Enums
	{
		[Flags]
		public enum CovnertTypes : uint
		{
			None = 0,
			PACDM_CONVERT = 1 << 0,
			DVR_CONVERT = PACDM_CONVERT << 1

		}

		public enum EventMonitor: int
		{
			Event_Exit,
			Event_DomainLocked,
			Event_DomainChange,
			Event_DomainRequest,
			Event_DomainKeepAlive,
			Event_DVRInfoChange,
			EVent_InvalidToken,
			Event_Login,
			Event_KeepAlive,
			Event_GetLastVersion,
			Event_RecycleData,
			Event_Count



		}
	}
}
