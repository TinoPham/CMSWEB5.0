using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PACDMSimulator.DVRConverter
{
	public enum CMSMsg
	{
		MSG_DVR_CONNECT = 10000,
		MSG_DVR_CONNECT_RESPONSE,
		MSG_DVR_DISCONNECT,
		MSG_DVR_DISCONNECT_RESPONSE,
		MSG_DVR_KEEPALIVE,
		MSG_DVR_GET_CONFIG,						// 10005
		MSG_DVR_GET_CONFIG_RESPONSE,
		MSG_DVR_SET_CONFIG,
		MSG_DVR_SET_CONFIG_RESPONSE,
		MSG_DVR_CONFIG_CHANGED,
		MSG_DVR_SERVER_ALERT,					// 10010
		MSG_DVR_GET_VIDEO,
		MSG_DVR_GET_VIDEO_RESPONSE,
		MSG_DVR_TIME_CHANGED,
		MSG_DVR_DOWNLOAD_FILE,					// LP add to reserve for download file function
		MSG_DVR_DOWNLOAD_FILE_RESPONSE,			// 10015, LP add to reserve for download file function
		MSG_DVR_GET_SNAPSHOT,
		MSG_DVR_GET_SNAPSHOT_RESPONSE,
		MSG_DVR_PERFORMANCE_STATUS,				// Collecting performance of DVR
		MSG_DVR_SERVER_ALERT_ALL,				// all alert in a connection interval
		MSG_DVR_SENT_ALL_INFO,					// 10020, DVR Express: announce CMS done sending all information
		MSG_DVR_SENT_ALL_INFO_RESPONSE,			// CMS Server -> DVR Express: CMS receive all information
		MSG_DVR_GET_KEEP_ALIVE_INTERVAL,		// CMS Server -> DVR: CMS update keep alive interval
		MSG_DVR_GET_TECHNICAL_LOG,
		MSG_DVR_GET_TECHNICAL_LOG_RESPONSE,
		MSG_DVR_END,

		//message to & from VEO Server
		MSG_VEO_CONNECT = 15000,
		MSG_VEO_CONNECT_RESPONSE,
		MSG_VEO_DISCONNECT,
		MSG_VEO_DISCONNECT_RESPONSE,
		MSG_VEO_KEEPALIVE,
		MSG_VEO_GET_CONFIG,						//	15005
		MSG_VEO_GET_CONFIG_RESPONSE,
		MSG_VEO_SET_CONFIG,
		MSG_VEO_SET_CONFIG_RESPONSE,
		MSG_VEO_CONFIG_CHANGED,
		MSG_VEO_SERVER_ALERT,					//	15010
		MSG_VEO_GET_VIDEO,
		MSG_VEO_GET_VIDEO_RESPONSE,
		MSG_VEO_TIME_CHANGED,

		MSG_VEO_END,

		//message to CMS Monitoring
		MSG_CMS_MOR_KEEP_ALIVE = 18000,
		MSG_CMS_MOR_AUTO_EXIT,					// add for Auto Restart CMS Server function

		//	messages to & from CMS Client
		MSG_CMS_CONNECT = 20000,
		MSG_CMS_CONNECT_RESPONSE,
		MSG_CMS_DISCONNECT,
		MSG_CMS_DISCONNECT_RESPONSE,
		MSG_CMS_KEEPALIVE,
		MSG_CMS_GET_CONFIG,						//	20005
		MSG_CMS_GET_CONFIG_RESPONSE,
		MSG_CMS_SET_CONFIG,
		MSG_CMS_SET_CONFIG_RESPONSE,
		MSG_CMS_VIEW_ALERT,						//	20009
		MSG_CMS_VIEW_ALERT_RESPONSE,
		MSG_CMS_REGISTER_DVR,
		MSG_CMS_UNREGISTER_DVR,
		MSG_CMS_DEL_DVR,						//	20013
		MSG_CMS_DEL_DVR_RESPONSE,
		MSG_CMS_CENTER_VIEW_ALERT,
		MSG_CMS_CENTER_VIEW_ALERT_RESPONSE,		//	20016
		MSG_CMS_CHANGE_DVR_DETAILS,				//	20017
		MSG_CMS_STATUS_DVR,						//  20018
		MSG_CMS_SUMARY_DVR,						//	20019
		MSG_CMS_SET_OPTION,						//	20020 - LP add to save Options configure
		MSG_CMS_ADMIN_CHANGE_GROUP,				//	20021 - LP add to save User - Group 
		MSG_CMS_USER_CHANGE_GROUP,				//	20022 - LP add to save User - Group 
		MSG_CMS_END_SYNC,						//	LP add to end sync from a Client
		MSG_CMS_DOWNLOAD_FILE,					//  LP add to reserve for download file function
		MSG_CMS_DOWNLOAD_FILE_RESPONSE,			//  LP add to reserve for download file function
		MSG_CMS_REQUEST_CONFIG_CHANGE_HISTORY,
		MSG_CMS_CONFIG_CHANGE_HISTORY_RESPONSE,
		MSG_CMS_UPDATE_CONFIG_CHANGE_HISTORY,
		MSG_CMS_FULL_CONFIG_CHANGE_HISTORY,
		MSG_CMS_GET_SNAPSHOT,
		MSG_CMS_GET_SNAPSHOT_RESPONSE,
		MSG_CMS_REQUEST_DVR_STATUS,
		MSG_CMS_REQUEST_DVR_STATUS_RESPONSE,
		MSG_CMS_SAVE_SNAPSHOT,
		MSG_CMS_SAVE_SNAPSHOT_RESPONSE,
		MSG_CMS_SET_BLOCK_DVR,
		MSG_CMS_SET_BLOCK_DVR_RESPONSE,
		MSG_CMS_REQUEST_CONFIG_CHANGE_HISTORY_PREVIOUS,
		MSG_CMS_GET_ACTIVATION,					//20039 - LPhuong add to get Activation information 
		MSG_CMS_GET_ACTIVATION_RESPONSE,		//20040 - LPhuong add to get Activation information
		MSG_CMS_SET_ACTIVATION,					//20041 - LPhuong add to set Activation information
		MSG_CMS_SET_ACTIVATION_RESPONSE,		//20042 - LPhuong add to set Activation information
		MSG_CMS_GET_SNAPSHOT_LIST,
		MSG_CMS_GET_SNAPSHOT_LIST_RESPONSE,
		MSG_CMS_SAVE_SNAPSHOT_DATABASE,			//20045	- save snapshot to database
		MSG_CMS_SERVER_ALERT,					//          HASP alert
		MSG_CMS_DISCONNECT_CLIENT,				//20047 - CMS Server to disconnect a Client
		MSG_CMS_REQUEST_DVR_SUMARY, 			//          Improve Channel Status structure
		MSG_CMS_SYNC_RESULT,					//          Offline Sync result	
		MSG_CMS_UPDATE_ALERT_STATUS,			//20050 - Alert Status
		MSG_CMS_SET_BACKUP_SCHEDULE,			//          Setting backup schedule
		MSG_CMS_SET_BACKUP_SCHEDULE_RESPONSE,	//20052     Show notify when apply config schedule backup
		MSG_CMS_ACCOUNT_LIST_CHANGE,			//20053 - Response back to CMS Client when Account list in CMS Server changes
		MSG_CMS_GET_DVR_USER_LIST,				//20054
		MSG_CMS_GET_DVR_USER_LIST_RESPONSE,		//20055
		MSG_CMS_SET_USER_INFO_ALL_DVRS,			//20056
		MSG_CMS_SET_USER_INFO_SYNC_NEXTDVR,		//          Add for edit user
		MSG_CMS_GET_SERVER_CONFIG_RESPONSE,
		MSG_CMS_END,

		//	messages belong to CMS Server itself
		MSG_CMSSERVER_SYSTEM_TRAY_EVENT = 30000,
		MSG_CMSSERVER_DISCONNECT_ALL_CLIENTS,	// 30001//Kha Added Oct 29 2007
		MSG_CMSSERVER_DISCONNECT_ALL_DVRS,		// 30002 - Kha Added Oct 29 2007	
		MSG_CMSSERVER_WRITE_CONSOLE,			// 30003 Kha Added Nov 9 2007
		MSG_CMSSERVER_WRITE_LOGFILE,			// 30004 Kha Added Nov 9 2007
		MSG_CMSSERVER_UPDATE_DVR_STATUS,		// 30005 Kha Added Nov 9 2007
		MSG_CMSSERVER_CHECK_NEWDATE_COMING,		// 30006 Kha Added Nov 9 2007
		MSG_CMSSERVER_TIMER_TICK,				// 30007 Kha Added Nov 9 2007
		MSG_CMSSERVER_SAVE_DATABASE,			// 30008 LPhuong Added Feb 29 2008
		MSG_CMSSERVER_CLOSE_SOCKET,				// kqanh add to close socket
		MSG_CMSSERVER_CHECK_SOCKET,				// LPhuong add to check all socket status each 2 minutes
		MSG_CMSSERVER_EXIT_APPLICATION,			// Thong add to exit application when hard disk is full
		MSG_CMSSERVER_SAVE_SYNC_CONFIG,			//Anh Huynh, Save synch config from Client to Database if DVR is offline, Oct 14, 2009
		MSG_CMSSERVER_CHECK_OFFLINE_SYNCDATA,	//Anh Huynh, Get synch config from Database when DVR is connected, Oct 14, 2009
		MSG_CMSSERVER_SEND_SYNC_CONFIG,			//Anh Huynh, Send synch config from Database when DVR is connected, Oct 14, 2009
		MSG_CMSSERVER_DELETE_SYNC_CONFIG,		//Anh Huynh, Asynchronous update for offline DVR, Oct 19, 2009
		MSG_CMSSERVER_RESET_SYNC_CONFIG,		//Anh Huynh, Asynchronous update for offline DVR, Oct 19, 2009
		MSG_CMSSERVER_CHECK_WORKING,			//LPhuong: check whether thread is working or not, Jan 26, 2009
		MSG_CMSSERVER_WRITE_LOG,				//LPhuong: to write log to console and file, Feb 22, 2010
		MSG_CMSSERVER_UPDATE_DATABASE_CONFIG,	//Anh Huynh, Create thread for update to DB - add new Msg, Apr 08, 2010
		MSG_CMSSERVER_UPDATE_DATABASE_COMMAND,	//Anh Huynh, Create thread for update to DB - add new Msg, Apr 08, 2010
		MSG_CMSSERVER_DB_DELETE_DVR_RESPONSE,	//Anh Huynh, Create thread for update to DB - add new Msg, Apr 08, 2010
		MSG_CMSSERVER_DB_ADD_NEW_DVR_RESPONSE,	//LPhuong: Create thread for update to DB - add new Msg, Apr 21, 2010
		MSG_CMSSERVER_CHECK_EXPIRED_DVR,
		MSG_CMSSERVER_UPDATE_SYNC_STATUS_RESPONSE,	//LPhuong: Create thread to update to DB - response after set status of data to 1
		MSG_CMSSERVER_RESET_SYNC_STATUS_RESPONSE,	//LPhuong: Response when request to reset sync status from 2 --> 1, July 08 2010
		MSG_CMSSERVER_SEND_SYNC_CONFIG_CMM,			//Anh huynh, Edit user for multi DVRs, Oct 06, 2010
		MSG_CMSSERVER_DB_RESET_CONNECTION,			//Anh Huynh, Reset DB connection when new day is comming, Nov 30, 2010
		MSG_CMSSERVER_REMOVE_CC_CONNECTION,			//Anh Huynh, Remove current connection from CC list, Nov 14, 2010
		MSG_CMSSERVER_RECEIVEALL_RESPONSE,			//Anh Huynh, Response received all to DVR, Nov 14, 2010
		MSG_CMSSERVER_PERFORMANCE_STATUS,
		MSG_CMSSERVER_DB_SET_CONFIG_FAIL,
		MSG_CMSSERVER_DB_UPDATE_CONFIG_RESPONSE,
		MSG_CMSSERVER_DB_UPDATE_HASP_INFO,		//Anh Huynh, Update HASP info to DB, Aug 28, 2013
		MSG_CMSSERVER_END,

		//	messages belong to CMS Client only
		MSG_CMSCLIENT_DISCONNECT_SOCKET = 40000,
		MSG_CMSCLIENT_NETWORK_CONNECTED,
		MSG_CMSCLIENT_SOCKET_ERROR,
		MSG_CMSCLIENT_NETWORK_ERROR,
		MSG_CMSCLIENT_SAVE_CHANGES,
		MSG_CMSCLIENT_DISCARD_CHANGES,
		MSG_CMSCLIENT_SYSTEM_TRAY_EVENT,
		MSG_CMSCLIENT_SEND_DATA,

		MSG_CMSCLIENT_END,
		MSG_CMSCLIENT_CHECK_DELETED_USER
	}
	
	class DVRUtils
	{
		public delegate void SocketEvent(object sender, Events.SocketEvent eventargs);
		public delegate void SocketReceivedData(object sender, Events.SocketReceivedData eventargs);

		public enum SOCKET_BUFF_TYPE
		{
			MSG_HEADER = 0,
			MSG_BUFFSIZE,
			MSG_DATA
		}

		public const int DVR_CONNECTION_SUPPORT = 1;
		public const int MSG_HEADER_SIZE = 2;
		public const int MSG_BUFF_SIZE = 4;
		//buffer must be greater than 4
		public const int MAX_SOCKET_BUFFER = 1024;

		
	}

	public struct MsgHeader
	{
		[MarshalAs(UnmanagedType.I4)]
		public Int32 size;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
		public string msgBegin;
		[MarshalAs(UnmanagedType.I4)]
		public Int32 msg_id;
		[MarshalAs(UnmanagedType.I4)]
		public Int32 zip;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public Int32[] pads;
	}

	public struct VideoHeader
	{
			[MarshalAs(UnmanagedType.I4)]
			public Int32 param;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string guid;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 time;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string stime;
	};

	public class DVRDataMessage: IDisposable
	{
		public const string BEGIN_MSG = "B123456";
		public static readonly int HeaderSize = Marshal.SizeOf(typeof(MsgHeader));

		public MsgHeader Header{get; set;}
		public byte[] Buffer{  get; set;}

		public void Dispose()
		{
			if (Buffer != null)
				Buffer = null;

			Header = new MsgHeader();
		}

	}

}
