using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
namespace ProcessException
{
	public enum DownLoadType{DATA, DAILY};
	public enum ProgramSet{POS,CA,ATM,IOPC,LPR};
	public enum ServerOEM{SRX,SRXPRO};
	public struct DBInfo
	{
		public string filename;
		public string filepath;
		public ProgramSet programset;
	}
	public struct PACServer
	{
		public string PACID;
		public string ServerIP;
		public int Port;
		public string LastKeyDownLoad_POS;
		public string LastKeyDownLoad_CA;
		public string LastKeyDownLoad_ATM;
		public string LastKeyDownLoad_IOPC_Alarm;
		public string LastKeyDownLoad_IOPC_Count;
		public string LastKeyDownLoad_IOPC_DT;
		public string LastKeyDownLoad_IOPC_QTIME;
		public string LastKeyDownLoad_IOPC_TRAFFIC_COUNT;//tram add Nov 19, 2010
		public string LastKeyDownload_POS_Sensor;
		public string LastKeyDownLoad_LPR;
		public string LastDownLoadDate;
		public DownLoadType Download;
		public string Remote_dir;
		public string I3Remote_Dir;
		public string Functions;
		public string FileList;
	}
	/// <summary>
	/// Summary description for Data.
	/// </summary>
	public class Data
	{
		#region constant
		#region Message
		public const string MSG_INVALID_COMMAND = "Invalid request from  ";
		public const string MSG_DOWNLOAD_REQUEST_FROM = "Download request received from ";
		public const string MSG_SEND_FILE_TO_REMOTE = "Send {0} file to {1}" ;
		public const string MSG_SEND_BLOCK_RECORDS_TO_REMOTE = "{0} {1} transaction(s) send to {2}";
		public const string MSG_DISCONNECTED_FROM_REMOTE = "Disconnect from remote : ";
		#endregion
		public const string Regex_SRXPRO_Version = @"^(?<V1>\d{1,3})\.(?<V2>\d{1,3})\.(?<V3>\d{1,3})(?:\.(?<V4>\d{1,3}))?";
		public const char DB_FILE_FORMAT_CHAR = '_';
		public const string LPR_IMAGE_FILE_TYPE = ".jpg";
		public const string DB_FILE_TYPE = ".mdb";
		public const string LPR_JPEG_FOLDER = "LPR_JPEG";
		public const string LPR_JEG_COLUMN_NAME = "LPR_JPEG_FILE_NAME";
		//public const int DOWNLOAD_PORT = 7575;
		public const int BUFFER_SIZE = 4096;
		public const int TIME_INTERVAL = 1000 * 60 *2;//timer to check when remote send request but can't download
		public const int MAX_BLOCK_RESPONSE = 100;//max total transactions will response to remote.
		public const int MAX_PREVIOUS_HOURS_DOWNLOAD = 24;
		public const string CLONE_POS_FILENAME = "clone.mdb";
		public const string CLONE_CA_FILENAME = "cloneCA.mdb";
		public const string CLONE_ATM_FILENAME = "CloneATM.mdb";
		public const string REPORTING_FILE_NAME = "ReportingDb.mdb";
		public const string REGION_COUNT_FILENAME = "RegionCount.mdb";
		public const string CLONE_IOPC_FILE_NAME = "objectdetection.mdb";
		public const string REPORTING_IOPC_FILENAME = "TrackingSetup.mdb";
		public const string CLONE_LPR_FILENAME = "LPR.mdb";
		public const string VIDEOLOGIX_FOLDER = "VideoLogix";
		public const string LPR_CLONE_FOLDER = "LPR";
		public const string REMOTE_READY_RECEIVE_DATA = "REMOTE_READY_RECEIVE_DATA";
		public const string REMOTE_READY_RECEIVE_FILE = "REMOTE_READY_RECEIVE_FILE";
		public const string REQUEST_DOWNLOAD_DATA = "REQUEST_DOWNLOAD_DATA";	
		public const string REQUEST_DATA_FILE = "REQUEST_DATA_FILE";
		public const string REMOTE_READY_RECEIVE = "REMOTE_READY_RECEIVE";
	
		public const string RESPONSE_FILE_INFO = "RESPONSE_FILE_INFO\a";	
		public const string RESPONSE_DATA_INFO = "RESPONSE_DATA_INFO\a";
		public const string RESPONSE_END_PROGRAMSET = "RESPONSE_END_PROGRAMSET\a";
		public const int MAX_CLIENT_CONNECT = 64;
		public const string TABLE_REQUEST = "REQUEST";
		public const string FUNCTION_COLUMN = "FUNCTION";
		public const string DOWNLOAD_TYPE_COLUMN = "DOWNLOAD_TYPE";
		public const string LAST_DOWNLOAD_DATE_COLUMN = "LAST_DOWNLOAD_DATE";
		public const string LAST_DOWNLOAD_KEY_COLUMN = "LASTKEY";
		public const string FILE_DAILY_REQUEST = "FILE_DAILY_REQUEST";
		public const string LAST_TIME_COLUMN = "LAST_TIME";
		//public const string SQL_CONNECTION = " Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}";
		public const string DATE_FORMAT = "dd/MM/yyyy";

		public const string RESPONSE_NO_DATA = "RESPONSE_NO_DATA\a"; 
		public const string RESPONSE_DOEST_NOT_EXIST_FILE = "RESPONSE_DOES_NOT_EXIST_FILE\a"; 
		public const string RESPONSE_DISCONNECT = "RESPONSE_DISCONNECT\a"; 
		public const string RESPONSE_REQUEST_COMMAND_FAIL = "RESPONSE_REQUEST_COMMAND_FAIL\a"; 
		public const string RESPONSE_DOWNLOAD_COMPLETE = "RESPONSE_DOWNLOAD_COMPLETE\a"; 
		public const string RESPONSE_IOPC_REPORTING = "RESPONSE_IOPC_REPORTING\a"; 
		public const string RESPONSE_IOPC_CLONE = "RESPONSE_IOPC_CLONE\a";
		public const string RESPONSE_ACCEPT_REQUEST = "RESPONSE_ACCEPT_REQUEST\a";
		public const string MSG_NO_NEW_DATA_COMEIN = "No new data.";
		public const string MSG_NO_NEW_DAILY_FILE = "Daily database does not exist : ";
		public const string MSG_DISCONNECT_FROM_SERVER = "Disconnect from ";
		public const string REG_I3_PATH = "Software\\DVR4\\Settings";
		public const string REG_I3_KEY = "RunningPath";

        public const string REG_IP_PRO_PATH = "Software\\I3DVR\\I3PRO\\iP-Pro_Settings";
		public const string REG_I3PRO_PATH = "Software\\I3DVR\\I3Pro\\Settings";

		public const string REG_I3PRO_KEY = "ServerAppPath";
		public const string REG_I3PRO_VERSION = "SRX-Pro Version";
		
		public static string[] TBL_DATA ={"Transact","Count","Alarm","DriveThrough","Info"};
		public const string TBL_REQUEST = "REQUEST";
		public const string TBL_SERVER_INFO = "SERVER_INFO";
		public const string TBL_REMOTE_INFO = "REMOTE_INFO";
		public const string TBL_LAST_DOWNLOAD = "LAST_DOWNLOAD";
		public const string TBL_DOWNLOAD_DATE = "DOWNLOAD_DATE";
		public const string I3_DATASET = "I3DVR_DOWNLOAD";
		public const string COL_FUNCTION = "FUNCTION";
		/// <summary>
		/// Date time when remote is sent
		/// </summary>
		public const string COL_DATE_TIME = "DATE_TIME";
		public const string COL_SERVER_IP = "SERVER_IP";
		public const string COL_PORT_NO = "PORT_NO";
		public const string COL_REMOTE_IP = "REMOTE_IP";
		public const string COL_REMOTE_PORT = "REMOTE_PORT";
		/// <summary>
		/// Date for what request remote has been sent
		/// </summary>
		public const string COL_REMOTE_DATE = "REMOTE_DATE";
		/// <summary>
		/// Date when last data was downloaded
		/// </summary>
		public const string COL_DATE = "DATE";
		/// <summary>
		/// last POS key
		/// </summary>
		public const string COL_POS_TRANS_KEY_NO = "POS_TRANS_KEY_NO";
		/// <summary>
		/// last SENSOR key
		/// </summary>
		public const string COL_POS_SENSOR_KEY_NO = "POS_SENSOR_KEY_NO";
		/// <summary>
		/// last ATM key
		/// </summary>
		public const string COL_ATM_TRANS_KEY_NO = "ATM_TRANS_KEY_NO";
		/// <summary>
		/// last CA key
		/// </summary>
		public const string COL_CA_TRANS_KEY_NO = "CA_TRANS_KEY_NO";
		/// <summary>
		/// last Alarm IOPC key
		/// </summary>
		public const string COL_IOPC_TRANS_KEY_NO_ALARM = "IOPC_TRANS_KEY_NO_ALARM";
		/// <summary>
		/// last Count IOPC key
		/// </summary>
		public const string COL_IOPC_TRANS_KEY_NO_COUNT = "IOPC_TRANS_KEY_NO_COUNT";
		/// <summary>
		/// Last Drive though key
		/// </summary>
		public const string COL_IOPC_TRANS_KEY_NO_DT = "IOPC_TRANS_KEY_NO_DT";
		/// <summary>
		/// Last QTime key
		/// </summary>
		public const string COL_IOPC_TRANS_KEY_NO_QTIME = "IOPC_TRANS_KEY_NO_QTIME";
		/// <summary>
		/// Last Traffic Count key
		/// </summary>
		public const string COL_IOPC_TRANS_KEY_NO_TRAFFIC_COUNT = "IOPC_TRANS_KEY_NO_TRAFFIC_COUNT";//tram add Nov 19, 2010
		/// <summary>
		/// last LPR key
		/// </summary>
		public const string COL_LPR_TRANS_KEY_NO = "LPR_TRANS_KEY_NO";
		/// <summary>
		/// last download date
		/// </summary>
		public const string COL_D_DATE = "D_DATE";
		private static System.Globalization.CultureInfo m_DateCulture = new System.Globalization.CultureInfo("en-US",false);
		public static System.Globalization.CultureInfo DateCulture
		{
			get
			{
				m_DateCulture.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
				return m_DateCulture;
			}
		}
		
		#endregion
		#region static functions
		public static string ReadRegistryValue (string KeyPath, string KeyName)
		{
			using (RegistryKey key = Registry.LocalMachine .OpenSubKey (KeyPath))
			{
				if (key == null)
					return string.Empty;
				try
				{
					return key.GetValue(KeyName, string.Empty).ToString ();
				}
				catch
				{
					return "";
				}
			}

		}
		public static string GetDataFilePath(DateTime datetime, ProgramSet programset, string installpath)
		{
			string path = string.Empty;
			path = installpath;
			path = path.EndsWith(@"\")?path: path + @"\";
			string subpath = string.Empty;
			subpath = string.Format(@"{0}\{0}_{1}\{0}_{1}_{2}.mdb",datetime.Year.ToString("0000"),datetime.Month.ToString("00"),datetime.Day.ToString("00") );
			if(ProgramSet.POS != programset)
				path += programset.ToString()+ @"\";
			path += subpath;
			return path;
		}
		public static string GetDataFilePath(string datetime, ProgramSet programset, string installpath)
		{
			DateTime date = Convert.ToDateTime(datetime,DateCulture);
			string path = string.Empty;
			path = installpath;
			path = path.EndsWith(@"\")?path: path + @"\";
			string subpath = string.Empty;
			subpath = string.Format(@"{0}\{0}_{1}\{0}_{1}_{2}.mdb",date.Year.ToString("0000"),date.Month.ToString("00"),date.Day.ToString("00") );
			if(ProgramSet.POS != programset)
				path += programset.ToString()+ @"\";
			path += subpath;
			return path;
		}
		
		#endregion
		public Data()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		
	}
}
