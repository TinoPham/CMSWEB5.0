using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConverterDB.Model;

namespace PACDMSimulator
{
	internal abstract class Consts
	{

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
		public const string LOG_SOURCE = "PACDM Converter";
		public const string STR_APPLICATION = "Application";
		public const string STR_CONVERTER = "PACDM Converter";
		public const string RX_YEAR = @"^\d{4}?";
		public const string RX_DB_FILE_EXTENSION = @"^(?<Year>\d{4})_(?<Month>\d{2})_(?<Day>\d{2})\.mdb$";
		public const string RX_DB_FILE = @"^(?<Year>\d{4})_(?<Month>\d{2})_(?<Day>\d{2})$";
		public const string ReportingDB_FileName = "ReportingDb.mdb";
		public const string STAR_Sign = "*";
		public const string STR_START_SUCCESSFULL = "Service started successfully.";
		public const string STR_STOP_SUCCESSFULL = "Service stopped successfully.";
	}

	internal class Utils
	{
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
			//[DllImport("zlib")]
			//public static extern string zlibVersion();
			[DllImport("zlib")]
			public static extern ZLibError compress(byte[] dest, ref int destLength, byte[] source, int sourceLength);
			[DllImport("zlib")]
			public static extern ZLibError compress2(byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibCompressionLevel level);
			[DllImport("zlib")]
			public static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);
		}

		public static byte[] ZipData(byte[] buff, int len)
		{
			if (buff == null || buff.Length == 0 || len == 0)
				return null;

			byte[] zipData = null;
			try
			{
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

		private static readonly CultureInfo _Culture = new CultureInfo("en-US");

		public static CultureInfo CultureInfo{ get{ return _Culture;}}

		private static Version Version = Assembly.GetExecutingAssembly().GetName().Version;
		public static string StartupDir
		{
			get{ return Assembly.GetExecutingAssembly().Location; }
			 
		}

		public static string CovnerterAgent{
			get{
				return string.Format("{0}. Version {1}", Consts.STR_CONVERTER, Version.ToString());
			}
		}
		public static string SRX_Pro_Reg_Path 
		{
			get { return Path.Combine(Consts.STR_SOFTWARE, Consts.STR_i3DVR, Consts.STR_I3Pro); }
		}

		public static string SRX_Pro_Reg_Settings_Path
		{
			get { return Path.Combine(Utils.SRX_Pro_Reg_Path, Consts.STR_Settings); }
		}

		public static string PAC_Reg_Path
		{
			get { return Path.Combine(Consts.STR_SOFTWARE, Consts.STR_PAC); }
		}
		
		public static DirectoryInfo[] DirInfo(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			DirectoryInfo dinfo = new DirectoryInfo( path);
			return dinfo.GetDirectories(pattern, option);
		}
		public static FileInfo[] GetFileInfo(DirectoryInfo dinfo, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return dinfo.GetFiles(pattern, SearchOption.AllDirectories);
		}
		public static string[] ListFile(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return Directory.GetFiles(path, pattern, option);
		}

		public static string[] ListFolder(string path, string pattern = Consts.STAR_Sign, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return Directory.GetDirectories(path, pattern, option);
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

		public static void WriteToLogFile(string title, string logMessage)
		{
			string strLogMessage = string.Empty;
			string strLogFile = System.Configuration.ConfigurationManager.AppSettings["logFilePath"].ToString();
			StreamWriter swLog;

			strLogMessage = string.Format("{0}:{1}{2}",DateTime.Now, title,logMessage);

			if (!File.Exists(strLogFile))
			{ 
				swLog = new StreamWriter(strLogFile);
			}
			else
			{
				swLog = File.AppendText(strLogFile);
			}

			swLog.WriteLine(strLogMessage);
			swLog.Close();
		}
	}
}
