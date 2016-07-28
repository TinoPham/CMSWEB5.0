using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
namespace CMSTest
{
	public class CMSMessage
	{
		public const int NUMBER_ALERT_ALL = 10;
		public const string BEGIN_MSG = "B123456";
		public const int MAX_MSG_BUFFER = 1024 * 256;
		public readonly static string[] ConfigurationFiles =
		{
			"",
			"user_management.txt",
			"ip_camera.txt",
			"hw.txt",
			"system.txt",
			"siteinfo.txt",
			"record_schedule.txt",
			"video_logix.txt",
			"recording_display.txt",
			"video_input.txt",
			"motion.txt",
			"intellizone.txt",
			"intelliguard.txt",
			"text_overlay.txt",
			"virtual_ruler.txt",
			"email.txt",
			"communication.txt",
			"storage.txt",
			"log_record.txt",
			"log_content.txt",
			"",
			"",
			"book_mark.txt",
			"vision_count.txt",
            "video_privacy.txt",
		};
		public enum DVR_CONFIGURATION:int
		{
			EMS_CFG_START = 0,

			EMS_CFG_MANAGE_USERS,						// 1, the start of group synchronize message
			EMS_CFG_IP_CAMERA,
			EMS_CFG_HARDWARE,
			EMS_CFG_SYSTEM_INFO,
			EMS_CFG_SERVER_INFO,						// 5
			EMS_CFG_RECORD_SCHEDULE,
			EMS_CFG_VIDEO_LOGIX,						// 7
			EMS_CFG_RECORDING,
			EMS_CFG_VIDEO,
			EMS_CFG_MOTION,								// 10
			EMS_CFG_INTELLI_ZONE,						// 11
			EMS_CFG_INTELLI_GUARD,
			EMS_CFG_TEXT_OVERLAY,
			EMS_CFG_VIRTUAL_RULER,
			EMS_CFG_EMAIL,								//15
			EMS_CFG_NETWORK,							// 16, the end of group synchronize message
			EMS_CFG_STORAGE,
			EMS_CFG_LOG_RECORD,
			EMS_CFG_LOG_CONTENT,
			EMS_CFG_ALERTS,								// 20
			EMS_CFG_DVR_INFO,							// 21	
			EMS_CFG_BOOK_MARK,							// 22
			EMS_CFG_VISION_COUNT,						// 23
            CMS_CFG_VIDEO_PRIVACY,

			EMS_CFG_END,


			//	server's option configuration type
			EMS_CFG_SERVER_OPTION_START = 100,

			EMS_CFG_SERVER_OPTION_MAIN,
			EMS_CFG_SERVER_OPTION_COMMUNICATION,
			EMS_CFG_SERVER_OPTION_USERS,
			EMS_CFG_SERVER_OPTION_LOG,

			EMS_CFG_SERVER_OPTIONS_END
		}
		public enum DVR_CMS_MESSAGE_ID:int
		{
			EMSBEGIN					= 9999,

			//	message to/from DVR
			MSG_DVR_CONNECT = 10000,
			MSG_DVR_CONNECT_RESPONSE,
			MSG_DVR_DISCONNECT,
			MSG_DVR_DISCONNECT_RESPONSE,
			MSG_DVR_KEEPALIVE,
			MSG_DVR_GET_CONFIG,						//	10005
			MSG_DVR_GET_CONFIG_RESPONSE,
			MSG_DVR_SET_CONFIG,
			MSG_DVR_SET_CONFIG_RESPONSE,
			MSG_DVR_CONFIG_CHANGED,
			MSG_DVR_SERVER_ALERT,					//	10010
			MSG_DVR_GET_VIDEO,
			MSG_DVR_GET_VIDEO_RESPONSE,
			MSG_DVR_TIME_CHANGED,
			MSG_DVR_DOWNLOAD_FILE,					//  LP add to reserve for download file function
			MSG_DVR_DOWNLOAD_FILE_RESPONSE,			//  10015 // LP add to reserve for download file function
			MSG_DVR_GET_SNAPSHOT,
			MSG_DVR_GET_SNAPSHOT_RESPONSE,
			MSG_DVR_PERFORMANCE_STATUS,				//Anh Huynh, Add on Nov 28, 2008
			MSG_DVR_SERVER_ALERT_ALL,
			MSG_DVR_SENT_ALL_INFO,
			MSG_DVR_SENT_ALL_INFO_RESPONSE,
			EMSEND
		}
		public enum DVREVENT
					{
						EVENTBEGIN					= 0,

						EVENT_SYSTEM_STARTED,
						EVENT_SYSTEM_SHUT_DOWN,
						EVENT_DISK_SPACE_LOW,
						EVENT_CPU_TEMPERATURE,
						EVENT_VIDEO_LOST,						//5
						EVENT_BACKUP_STARTED,	
						EVENT_BACKUP_COMPLETED,
						EVENT_BACKUP_STOPPED,
						EVENT_SENSOR_TRIGGERED,
						EVENT_CONTROL_TRIGGERED,				//10
						EVENT_FORMAT_REQUESTED,
						EVENT_FORMAT_COMPLETED,
						EVENT_USER_ADDED,
						EVENT_USER_REMOVED,
						EVENT_USER_LOGGED_IN,					//15
						EVENT_USER_LOGGED_OUT,
						EVENT_SERVER_UNREACHABLE,				// not used
						EVENT_STORAGE_SETUP_CHANGE,
						EVENT_RECORDING_OVERWRITE_START,
						EVENT_NOT_RECORDING,					//20
						EVENT_CHANGE_CONFIGURATION,
						EVENT_PARTITION_DROPPED,
						EMS_ALERT_REGISTRATION_EXPIRE,			// 23, LPhuong add for activation control feature - alert for day 30-10
						EMS_ALERT_REGISTRATION_EXPIRE_URGENT,	// 24, LPhuong add for activation control feature - alert for day 10-0
						EVENT_OTHER_TYPE,						//25 //Anh Huynh add for checking Monitor.xml alert
						EVENT_PARTITION_ADDED,					//26  Khoi add for partition added 1.7.09

						EVENTEND
					} ;
		//[DVR_CMS_MESSAGE_ID.EMSEND - DVR_CMS_MESSAGE_ID.EMSBEGIN - 1] 
		private static string []g_csEMSMsgName = new string[]
								{	
									"MSG_DVR_CONNECT",
									"MSG_DVR_CONNECT_RESPONSE",
									"MSG_DVR_DISCONNECT",
									"MSG_DVR_DISCONNECT_RESPONSE",	
									"MSG_DVR_KEEPALIVE",
									"MSG_DVR_GET_CONFIG",
									"MSG_DVR_GET_CONFIG_RESPONSE",
									"MSG_DVR_SET_CONFIG",
									"MSG_DVR_SET_CONFIG_RESPONSE",
									"MSG_DVR_CONFIG_CHANGED",
									"MSG_DVR_SERVER_ALERT",
									"MSG_DVR_GET_VIDEO",	
									"MSG_DVR_GET_VIDEO_RESPONSE",
									"MSG_DVR_TIME_CHANGED",						//Anh Huynh, Update for correct position of the message
									"MSG_DVR_DOWNLOAD_FILE",					//  LP add to reserve for download file function
									"MSG_DVR_DOWNLOAD_FILE_RESPONSE",			//  LP add to reserve for download file function
									"MSG_DVR_GET_SNAPSHOT",
									"MSG_DVR_GET_SNAPSHOT_RESPONSE",
									"MSG_DVR_PERFORMANCE_STATUS",				//Anh Huynh, Add for collecting performance, Nov 28, 2008
									"MSG_DVR_SERVER_ALERT_ALL",
									"MSG_DVR_SENT_ALL_INFO",
									"MSG_DVR_SENT_ALL_INFO_RESPONSE"
								};
		private static string [] g_csEventName = new string[]
								{
									"system started",
									"system shut down",
									"disk space low",
									"CPU temperature critical",
									"video loss",
									"backup started",
									"backup completed",
									"backup stopped",
									"sensor triggered",
									"control triggered",
									"format requested",
									"format completed",
									"user added",
									"user removed",
									"user logged in",
									"user logged out",	
									"server unreachable",
									"recording stopped",
									"recording overwrite began",
									"recording is not working",
									"DVR changes configuration",
									"partition dropped",
									"Registration Expiry",
									"Registration Expiry",
									"Other type",	//Anh Huynh add for checking Monitor.xml alert
									"partition added",		//Khoi add for partition added 1.7.09
								};
		public struct VideoHeader
		{
			[MarshalAs(UnmanagedType.I4)]
			public int param;   // video source
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string guid;
			[MarshalAs(UnmanagedType.I4)]
			public int time;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string stime;  // store time under string
		} ;

		public struct MsgHeader
		{
			[MarshalAs(UnmanagedType.I4)]
			public int size;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
			public string msgBegin;
			[MarshalAs(UnmanagedType.I4)]
			public int msg_id;
			[MarshalAs(UnmanagedType.I4)]
			public int zip;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
			public int[] pads;
		} ;
		public string DVR_Guid
		{
			get { return m_DVR_Guid; }
		}
		private string m_DVR_Guid = string.Empty;
		Utils.DVR_Info m_DVR_Info;
		string m_user;
        public bool bChanelStatus = false;
		public CMSMessage(Utils.DVR_Info dvrinfo, string dvr_Guid, string struser)
		{
			m_DVR_Info = dvrinfo;
			m_DVR_Guid = dvr_Guid;
			m_user = struser;
		}
		private string GetServerInfo(bool isserverinfo,Utils.DVR_Info dvrinfo)
		{
			string tag_start = string.Empty;
			string tag_end = string.Empty;
 
            if (!isserverinfo && bChanelStatus)
            {
                tag_end = "<dvr_summary>";
                tag_end += string.Format("<server_id>{0}</server_id>", dvrinfo.Server_ID);
                tag_end += string.Format("<server_ip>{0}</server_ip>", dvrinfo.Server_IP);
                tag_end += string.Format("<server_port>17221</server_port>");
                tag_end += string.Format("<password_user_i3dvr>i3dvr</password_user_i3dvr>");
                tag_end += string.Format("<version>{0}</version>", dvrinfo.Server_Version);
                tag_end += string.Format("<location>{0}</location>", dvrinfo.Server_Location);
                tag_end += string.Format("<os>Windows XP Professional </os>");
                tag_end += string.Format("<total_size>50006</total_size>");
                tag_end += string.Format("<free_size>109</free_size>");
                tag_end += string.Format("<recording_days>8</recording_days>");

                tag_end += "<channels size=\"32\" num_video_input=\"16\">";
                tag_end += "<video_source>1718030405060708091011121314151600000000000000000000000000000000</video_source>";
                tag_end += "<channel_status>0202030303030303030303030202030301010101010101010101010101010101</channel_status>";
                tag_end += "</channels>";
                tag_end += "</dvr_summary>";

                bChanelStatus = false;
            }

            string ret = tag_start;
            if (isserverinfo)
            {
                ret += "<server_info>";
                ret += string.Format("<server_id>{0}</server_id>", dvrinfo.Server_ID);
                ret += string.Format("<server_ip>{0}</server_ip>", dvrinfo.Server_IP);
                ret += string.Format("<version>{0}</version>", dvrinfo.Server_Version);
                ret += "</server_info>";
            }
            else
            {
                ret += string.Format("<status>0</status>");
                ret += string.Format("<alias></alias>");
            } 

            ret += tag_end;

			return ret;
		}
		private string GetMessageHeader(DVR_CMS_MESSAGE_ID messageID, string dvrGuid)
		{
			string ret =	"<header>";
					ret +=		string.Format("<id>{0}</id>", (int)messageID);
					ret += string.Format("<name>{0}</name>", g_csEMSMsgName[(int)messageID - (int)DVR_CMS_MESSAGE_ID.EMSBEGIN -1]);
					ret +=		"<version>1.00</version>";
					ret += string.Format("<dvr_guid>{0}</dvr_guid>", dvrGuid);
					ret +=	"</header>";
			return ret;
		}
		private string GetMessageEvent(DVREVENT eventID, DateTime eventDate, string struser)
		{
			string strdatetime = string.Format("{0}/{1}/{2} - {3}:{4}:{5}",eventDate.Year.ToString("0000"), eventDate.Month.ToString("00"), eventDate.Day.ToString("00"), eventDate.Hour.ToString("00"), eventDate.Minute.ToString("00"),eventDate.Second.ToString("00") );
			string ret = "<event>";
					ret += string.Format("<id>{0}</id>", (int)eventID);
					ret += string.Format("<name>{0}</name>", g_csEventName[eventID - DVREVENT.EVENTBEGIN - 1]);
					ret += string.Format("<time>{0}</time>", Utils.To_time_t(eventDate));
					ret += string.Format("<string_time>{0}</string_time>", strdatetime);
					if(eventID == DVREVENT.EVENT_CONTROL_TRIGGERED || eventID == DVREVENT.EVENT_SENSOR_TRIGGERED)
						ret += string.Format("<channel_id>{0}</channel_id>",Utils.GetRandomNumber(Utils.MAX_DVR_CHANNEL) );
					else
						ret += "<channel_id>-1</channel_id>";
					ret += string.Format("<user>{0}</user>", struser);
					ret += string.Format("<information></information>", g_csEventName[eventID - DVREVENT.EVENTBEGIN - 1]);
				ret += "</event>";
			return ret;
		}
		private string GetMessageConfigHW( string dvrid, Utils.DVR_Info dvrinfo, DVR_CONFIGURATION conid, string datapath )
		{
			DateTime mdate = DateTime.Now;
			string	whois = string.Format("<whois><time>{0}</time>", Utils.To_time_t(mdate));
					whois += "<user>System</user>";
					whois += "<from>2</from> ";
					whois += string.Format("<ip>{0}</ip>", dvrinfo.Server_IP);
					whois += "<dvr_time>";
					whois += string.Format("<year>{0}</year>", mdate.Year > 1900 ? (mdate.Year - 1900) : mdate.Year);
					whois += string.Format("<month>{0}</month>", mdate.Month-1);
					whois += string.Format("<day>{0}</day>", mdate.Day);
					whois += string.Format("<hour>{0}</hour>", mdate.Hour);
					whois += string.Format("<minute>{0}</minute>", mdate.Minute);
					whois += string.Format("<second>{0}</second>", mdate.Second);
					whois += "</dvr_time>";
					whois += "</whois>";
			string fname = ConfigurationFiles[Convert.ToInt32(conid)];
            string xmlResouecePath = datapath + "\\XMLData\\" + GetConfigDir(dvrinfo.Server_Version) + "\\" + fname;
			byte[] buff = Utils.ReadFile(xmlResouecePath);
			if (buff == null)
				return string.Empty;

			string msgdata = System.Text.Encoding.Default.GetString( buff);
			switch( conid)
			{
				case DVR_CONFIGURATION.EMS_CFG_MANAGE_USERS:
					msgdata = string.Format(msgdata, dvrinfo.Server_Name);
					break;
				case DVR_CONFIGURATION.EMS_CFG_SERVER_INFO:
					msgdata = string.Format(msgdata, dvrinfo.Server_IP, dvrid);
					break;
				case DVR_CONFIGURATION.EMS_CFG_NETWORK:
					msgdata = string.Format(msgdata, dvrinfo.Server_IP);
					break;

			}
			buff = null;
			msgdata += whois;
			return msgdata;
		}
		public string GetMessageXMLData(string xmldatapath, string dvrID,int dvrmode,  DVR_CMS_MESSAGE_ID msgID,DVREVENT dvrEvent,DVR_CONFIGURATION conf_ID, DateTime eventdate, int totalmsg)
		{
			string msgbody = string.Empty;
			string msgheader = GetMessageHeader(msgID, m_DVR_Guid);
			switch (msgID)
			{
				case DVR_CMS_MESSAGE_ID.MSG_DVR_KEEPALIVE:
						msgbody = GetServerInfo(false, m_DVR_Info);
					break;
				case DVR_CMS_MESSAGE_ID.MSG_DVR_DISCONNECT:
				case DVR_CMS_MESSAGE_ID.MSG_DVR_CONNECT:
					msgbody = GetServerInfo(true, m_DVR_Info);
					msgbody += string.Format("<cms_mode>{0}</cms_mode>", dvrmode);
					break;
				case DVR_CMS_MESSAGE_ID.MSG_DVR_CONFIG_CHANGED:
				case DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE:
					msgbody = GetMessageConfigHW(dvrID, m_DVR_Info, conf_ID, xmldatapath);
					break;
				case DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT_ALL:
					msgbody += string.Format("<event_list size=\"{0}\">", NUMBER_ALERT_ALL);
					for (int i = 0; i < NUMBER_ALERT_ALL;i++ )
					{
						dvrEvent = (CMSMessage.DVREVENT)Utils.GetRandomNumber((int)CMSMessage.DVREVENT.EVENTBEGIN + 1, (int)CMSMessage.DVREVENT.EVENTEND - 1);
						msgbody += GetMessageEvent(dvrEvent, eventdate, m_user);
					}
					msgbody += "</event_list>";
					break;
				case DVR_CMS_MESSAGE_ID.MSG_DVR_SENT_ALL_INFO:
					msgbody = string.Format("<number_message>{0}</number_message>", totalmsg);
					break;
				default:
					msgbody = GetMessageEvent(dvrEvent, eventdate, m_user);
					break;
			}
			if (msgbody.Trim().Length == 0 || msgheader.Trim().Length == 0)
				return null;
			string msgdata = string.Format("<message>{0} <body>{1}</body></message>", msgheader,msgbody);
			return msgdata;
		}
		public static string GetDVRMessageText(DVR_CMS_MESSAGE_ID msgID)
		{
			return g_csEMSMsgName[msgID - DVR_CMS_MESSAGE_ID.EMSBEGIN - 1];
		}
		public static string GetDVREventText( DVREVENT dvrevent)
		{
			return g_csEventName[dvrevent - DVREVENT.EVENTBEGIN - 1];
		}
		public static byte[] GetMessageBuffer(string strxmldata, DVR_CMS_MESSAGE_ID msgID)
		{
			if (strxmldata == null || strxmldata.Length == 0)
				return null;
			byte[] zipbuff = new byte[MAX_MSG_BUFFER];
			byte[] orgbuff = System.Text.Encoding.Default.GetBytes(strxmldata);
			int orglen = orgbuff.Length;
			int zlen = MAX_MSG_BUFFER;
			CMSTest.ZLibError ret = CMSTest.ZLibWin32.compress(zipbuff, ref zlen, orgbuff, orglen);
			if( ret != ZLibError.Z_OK)
				return null;
			MsgHeader msghead = new MsgHeader();
			msghead.size = zlen + Marshal.SizeOf(typeof(MsgHeader));
			msghead.msg_id = (int)msgID;
			msghead.msgBegin = BEGIN_MSG;
			byte[] msgheader = Utils.StructureToByteArray(msghead);
			byte[] sendbuff = new byte[msgheader.Length + zlen];
			Array.Copy(msgheader, sendbuff, msgheader.Length);
			Array.Copy(zipbuff, 0, sendbuff, msgheader.Length, zlen);
            Array.Clear(zipbuff, 0, zipbuff.Length);
			    orgbuff = null;
			msgheader = null;
			zipbuff = null;
            if (zipbuff != null)
            {
                Array.Clear(zipbuff, 0, zipbuff.Length);
                zipbuff = null;
            }
			return sendbuff;
		}
		public static byte[] GetMessageVideoBuffer(string videoimgfilepath, string dvrguid, int input  )
		{
			byte[] videobuff = Utils.ReadFile(videoimgfilepath);
			if (videobuff == null)
				return null;
			DateTime eventDate = DateTime.Now;
			string strdatetime = string.Format("{0}/{1}/{2} - {3}:{4}:{5}",eventDate.Year.ToString("0000"), eventDate.Month.ToString("00"), eventDate.Day.ToString("00"), eventDate.Hour.ToString("00"), eventDate.Minute.ToString("00"),eventDate.Second.ToString("00") );

			byte[]bvheader = new byte [Marshal.SizeOf(typeof(VideoHeader)) ];
			Array.Copy(videobuff, bvheader, bvheader.Length);
			VideoHeader vheader = new VideoHeader();
		
			GCHandle handle = GCHandle.Alloc(bvheader, GCHandleType.Pinned);
			vheader = (VideoHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(VideoHeader));
			handle.Free();

			vheader.param = input;
			vheader.guid = dvrguid;
			vheader.time = Utils.To_time_t(eventDate);
			vheader.stime = strdatetime;

			MsgHeader msghead = new MsgHeader();
			msghead.size = videobuff.Length + Marshal.SizeOf(typeof(MsgHeader)) ;

			msghead.msg_id = (int)DVR_CMS_MESSAGE_ID.MSG_DVR_GET_VIDEO_RESPONSE;

			msghead.msgBegin = BEGIN_MSG;

			byte[] msgheader = Utils.StructureToByteArray(msghead);
			byte[] msgvideoheader = Utils.StructureToByteArray(vheader);

			byte[] sendbuff = new byte[msgheader.Length + videobuff.Length];
			int index = 0;
			Array.Copy(msgheader, sendbuff, msgheader.Length);
			index += msgheader.Length;

			Array.Copy(msgvideoheader, 0, sendbuff, index, msgvideoheader.Length);
			index += msgvideoheader.Length;

			Array.Copy(videobuff, Marshal.SizeOf(typeof(VideoHeader)), sendbuff, index, videobuff.Length - Marshal.SizeOf(typeof(VideoHeader)));
			videobuff = null;
			msgheader = null;
			return sendbuff;
		}

        public static string GetConfigDir(string sVer)
        {
            string sDir;
            switch (sVer)
            {
				case "3.2.0 (Simulator)":
					sDir = "3.2.0";
					break;
                case "2.3.0 (Simulator)":
                    sDir = "2.3.0";
                    break;
                case "2.2.0 (Simulator)":
                    sDir = "2.2.0";
                    break;
                case "2.1.3 (Simulator)":
                    sDir = "2.1.3";
                    break;
                case "2.0.0 (Simulator)":
                    sDir = "2.0.0";
                    break;
                case "1.600 (Simulator)":
                default:
                    sDir = "1.600";
                    break;
            }
            return sDir;
        }
    } //class
}
