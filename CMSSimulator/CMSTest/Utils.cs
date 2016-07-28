using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.XPath;
using System.IO;
namespace CMSTest
{
	public class Utils
	{
		#region DVR info struct
		public struct DVR_Info
		{
			public int Id { get; set; }
			public int DVRPort { get; set; }
			public string Mac { get; set; }
			public string Server_IP { get; set; }
			public string Server_ID { get; set; }
			public string Server_Name { get; set; }
			public string Server_Location { get; set; }
			public string Server_Model { get; set; }
			public string Server_Version { get; set; }
			public string HaspKey { get; set; }
			public string ServerUrl { get; set; }
			public bool Server_standard { get; set; }

		}
		public enum DVR_Status
		{
			Socket_Disconnect,
			CMS_Disconnect,
			CMS_Connected,
			Socket_Connected,
			Socket_Connect_Failed,
			CMS_All_Info_response

		}
		public static string[] DVR_Status_Text = new string[] { "Socket disconnect", "CMS disconnect", "CMS Connected", "Socket connected", "Socket connect failed" };
		#endregion
		private static DateTime origin = System.TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
		public const int DELAY_TIME_CONNECT_DVR = 0;
		public const int MAX_DVR_CHANNEL = 32;
		public const int MAX_STACK_SIZE = 128;
		public const int DELAY_TIME_ADD_DVR = 0;
		public const int DELAY_TIME_DISCONNECT_DVR = 0;
		public const int MAX_ZIP_SIZE = 1024*1024*5;
		public const int MIN_ALERT_DELAY_TIME = 2;
#if DEBUG
        public const int MSG_TIMEOUT = 3;
        public const int MSG_RETRY = 3;
        public const int MSG_RECONNECT = 5;
        public const int MSG_KEEP_ALIVE = 60;
        public const int MSG_EVENT_ALERT = 1;
        public const int MSG_EVENT_CONFIG = 2;
        public const int MSG_EVENT_VIDEO = 3;
        public const int MSG_CHANNEL_STATUS = 2;
#else
        public const int MSG_TIMEOUT = 30;
        public const int MSG_RETRY = 30;
        public const int MSG_RECONNECT = 120;
        public const int MSG_KEEP_ALIVE = 30;
        public const int MSG_EVENT_ALERT  = 30;
        public const int MSG_EVENT_CONFIG = 10;
        public const int MSG_EVENT_VIDEO = 20;
        public const int MSG_CHANNEL_STATUS = 30;
#endif
        public const string XPATH_CONFIGURATION_ID = @"/message/body/configuration_id";
		//public static UCtrlDVR.TimerSetting DVRTimerSetting()
		//{
		//	UCtrlDVR.TimerSetting msetting = new UCtrlDVR.TimerSetting();
		//	msetting.m_msg_timeout = MSG_TIMEOUT;
		//	msetting.m_msg_retry = MSG_RETRY;
		//	msetting.m_dvr_reconnect = MSG_RECONNECT;
		//	msetting.m_msg_alert = MSG_EVENT_ALERT;
		//	msetting.m_msg_video = MSG_EVENT_VIDEO;
		//	msetting.m_msg_configuration = MSG_EVENT_CONFIG;
		//	msetting.m_keep_alive = MSG_KEEP_ALIVE;
		//	msetting.m_channel_status = MSG_CHANNEL_STATUS;
		//	return msetting;
		//}
		public static TimerSetting DVRTimerSettings()
		{
			TimerSetting msetting = new TimerSetting();
			msetting.m_msg_timeout = MSG_TIMEOUT;
			msetting.m_msg_retry = MSG_RETRY;
			msetting.m_dvr_reconnect = MSG_RECONNECT;
			msetting.m_msg_alert = MSG_EVENT_ALERT;
			msetting.m_msg_video = MSG_EVENT_VIDEO;
			msetting.m_msg_configuration = MSG_EVENT_CONFIG;
			msetting.m_keep_alive = MSG_KEEP_ALIVE;
			msetting.m_channel_status = MSG_CHANNEL_STATUS;
			return msetting;
		}

		public static int GetRandomNumber(int maxvalue)
		{
			Random ran = new Random((int)DateTime.Now.Ticks);
			int value = ran.Next(maxvalue);
			return value;
		}
		public static string GetRandomMacAddress()
		{
			long mac = DateTime.Now.Ticks ;
			string strhex = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

			string macID = strhex.Substring((int)((mac >> 44) & 15) + 1, 1) + strhex.Substring((int)((mac >> 40) & 15) + 1, 1) + "-"
							+ strhex.Substring((int)((mac >> 36) & 15) + 1, 1) + strhex.Substring((int)((mac >> 32) & 15) + 1, 1) + "-"
							+ strhex.Substring((int)((mac >> 28) & 15) + 1, 1) + strhex.Substring((int)((mac >> 24) & 15) + 1, 1) + "-"
							+ strhex.Substring((int)((mac >> 20) & 15) + 1, 1) + strhex.Substring((int)((mac >> 16) & 15) + 1, 1) + "-"
							+ strhex.Substring((int)((mac >> 12) & 15) + 1, 1) + strhex.Substring((int)((mac >> 8) & 15) + 1, 1) + "-"
							+ strhex.Substring((int)((mac >> 4) & 15) + 1, 1) + strhex.Substring((int)((mac >> 0) & 15) + 1, 1);
			return macID;
		}
		public static byte[] ReadFile(string fpath)
		{
			if (!File.Exists(fpath))
				return null;
			FileStream fread = new FileStream(fpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			if (fread == null)
				return null;
			byte[] buff = null;
			if( fread.Length > 0)
			{
				buff = new byte[(int)fread.Length];
				fread.Read(buff, 0, (int)fread.Length);
			}
			if( fread != null)
			{
				fread.Close();
				fread.Dispose();
				fread = null;
			}

			return buff;
		}
		
		public static DateTime ToDateTime(int time_t)
		{
			DateTime convertedValue = origin + new TimeSpan(time_t * TimeSpan.TicksPerSecond);
			if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedValue) == true)
			{
				System.Globalization.DaylightTime daylightTime = System.TimeZone.CurrentTimeZone.GetDaylightChanges(convertedValue.Year);
				convertedValue = convertedValue + daylightTime.Delta;
			}
			return convertedValue;
		}
		public static int To_time_t(DateTime time)
		{
			DateTime convertedValue = time;
			if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedValue) == true)
			{
				System.Globalization.DaylightTime daylightTime = System.TimeZone.CurrentTimeZone.GetDaylightChanges(convertedValue.Year);
				convertedValue = convertedValue - daylightTime.Delta;
			}
			long diff = convertedValue.Ticks - origin.Ticks;
			return (int)(diff / TimeSpan.TicksPerSecond);
		}
		public static bool IsValidIpAddressRegEx(string addr)
		{
			System.Text.RegularExpressions.Regex VerExp = new System.Text.RegularExpressions.Regex("\\b(?<class_a>25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
					"(?<class_b>25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
					"(?<class_c>25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
					"(?<class_d>25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\b");
			System.Text.RegularExpressions.Match m = VerExp.Match(addr);
			bool bSuccess = false;
			if (m.Success)
			{
				int vala = int.Parse(m.Groups["class_a"].Value);
				int valb = int.Parse(m.Groups["class_b"].Value);
				int valc = int.Parse(m.Groups["class_c"].Value);
				int vald = int.Parse(m.Groups["class_d"].Value);

				bSuccess = (vala >= 1 && vala <= 223) && (vald >= 1 && vald <= 255) && (valb >= 0 && valb <= 255) && (valc >= 0 && valc <= 255);
			}
			VerExp = null;
			m = null;
			return bSuccess;
		}
		//-----
		public static bool IsValidDomainName(string domainName)
		{
			System.Text.RegularExpressions.Regex exp = new System.Text.RegularExpressions.Regex(@"[-a-zA-Z0-9][-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*");
			System.Text.RegularExpressions.Match m = exp.Match(domainName);
			bool bSuccess = m.Success;
			exp = null;
			m = null;
			return bSuccess;
		}
		public static byte[] StructureToByteArray(object obj)
		{

			int len = Marshal.SizeOf(obj);

			byte[] arr = new byte[len];

			IntPtr ptr = Marshal.AllocHGlobal(len);

			Marshal.StructureToPtr(obj, ptr, true);

			Marshal.Copy(ptr, arr, 0, len);

			Marshal.FreeHGlobal(ptr);

			return arr;

		}
		public static int GetRandomNumber(int minvalue, int maxvalue)
		{
			Random random = new Random();
			int nran = random.Next(minvalue, maxvalue +1);
			return nran;
		}
		public static string RandomIP()
		{
			long Number = DateTime.Now.Ticks;
			long w = (long) (  Number / 16777216 ) % 256;
			long x = (long) (  Number / 65536 ) % 256;
			long y = (long)(  Number / 256 ) % 256;
			long z = (long)(Number) % 256;

			string ret = string.Empty;
			if (Number % 2 == 0)
				ret = string.Format("{0}.{1}.{2}.{3}", w, x, y, z);
			else
			{
				x = (x <= 0) ?GetRandomNumber(0,254):x;
				ret = string.Format("{0}.{1}.{2}.{3}", w, w, y, z);
			}
			return ret;
		}

		public static void GetDVRInfo(bool dvrstandard, ref bool[] dvrmodes, ref string[] dvrguids, ref string[] dvrips, int total)
		{
			dvrguids = new string[total];
			dvrips = new string[total];
			dvrmodes = new bool[total];
			List<string> tmp = new List<string>();
			string tmpguid = string.Empty;
			for (int i = 0; i < total; i++)
			{
				tmpguid = Utils.GetRandomMacAddress();
				while (tmp.FindIndex(new Predicate<string>(delegate(string aa)
				{
					return (string.Compare(aa, tmpguid, true) == 0);
				})) >= 0)
				{
					tmpguid = Utils.GetRandomMacAddress();
				}
				tmp.Add(tmpguid);
				dvrguids[i] = tmpguid;
				dvrips[i] = Utils.RandomIP();
				dvrmodes[i] = dvrstandard;
				//System.Threading.Thread.Sleep(10);
			}
			tmp.Clear();
			tmp = null;
		}
		public static byte[] UnZipData(byte[]buff, int len)
		{
			if( buff == null || buff.Length == 0 || len == 0)
				return  null;
			string version = ZLibWin32.zlibVersion();
			byte[] tmpbuff = new byte[Utils.MAX_ZIP_SIZE];
			int datalen = Utils.MAX_ZIP_SIZE;
			CMSTest.ZLibError err = CMSTest.ZLibWin32.uncompress(tmpbuff, ref datalen, buff, len);
			if (err != ZLibError.Z_OK)
				return null;
			byte[] rawdata = new byte[datalen];
			Array.Copy(tmpbuff, 0,rawdata, 0, datalen);
			tmpbuff = null;
			return rawdata;
		}
		public static string GetXMLValue(string strxml, string xpath)
		{
			string ret = string.Empty;
			byte[] xmlbuff = System.Text.Encoding.Default.GetBytes(strxml);
			System.IO.MemoryStream mstream = new MemoryStream(xmlbuff);
			XPathDocument document = new System.Xml.XPath.XPathDocument(mstream);
			XPathNavigator navigator = document.CreateNavigator();
			XPathNavigator node = navigator.SelectSingleNode(xpath);
			if (node != null)
				ret = node.InnerXml;
			mstream.Close();
			mstream = null;
			document = null;
			navigator = null;
			return ret;
		}
		public static bool IsInt32(string strint32)
		{
			try
			{
				Convert.ToInt32(strint32);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		public static bool IsValidUri(string urlstr)
		{
			string myString = urlstr;
			Uri myUri;
			if (Uri.TryCreate(myString, UriKind.RelativeOrAbsolute, out myUri) && Uri.IsWellFormedUriString(myString, UriKind.RelativeOrAbsolute))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
	}
	public class DVRNode : System.Windows.Forms.TreeNode
	{
		string m_dvr_guid;
		Utils.DVR_Status m_dvrstatus = Utils.DVR_Status.Socket_Disconnect;
		Utils.DVR_Info m_dvrinfo;
		int m_cmsPort = 0;
		string m_cmsIP;
		string m_orgtext;
		int m_id = 0;
		public Utils.DVR_Status NodeStatus
		{
			get { return m_dvrstatus; }
		}
		public int NodeID
		{
			get { return m_id; }
		}
		public string DVRID
		{
			get { return m_dvr_guid; }
		}
        public Utils.DVR_Info DVRInfo
        {
            get { return m_dvrinfo; }
        }
		public DVRNode(Utils.DVR_Info dvrinfo,string cmsIP, int cmsport, int node_id, string node_text, string guid)
		{
			m_cmsIP = cmsIP;
			m_cmsPort = cmsport;
			m_dvrinfo = dvrinfo;
			m_id = node_id;
			m_orgtext = node_text;
			this.Text = node_text + "(" + Utils.DVR_Status_Text[(int)m_dvrstatus] + ")";
			this.m_dvr_guid = guid;
			UpdateTooltip(dvrinfo, m_dvr_guid, m_dvrstatus, cmsIP, cmsport);
		}
		public void UpdateDvrSatus(Utils.DVR_Info dvrinfo,string cmsIP, int cmsport, string version, Utils.DVR_Status status)
		{
			if (version != null && version.Length > 0)
			{
				m_orgtext = version;
				m_orgtext += "#" + m_id.ToString();
			}
			m_cmsPort = cmsport;
			m_cmsIP = cmsIP;
			m_dvrinfo = dvrinfo;
			m_dvrstatus = status;
			this.Text = m_orgtext + "(" + Utils.DVR_Status_Text[(int)status] + ")";
			UpdateTooltip(dvrinfo, m_dvr_guid, status, cmsIP, cmsport);
		}
		private void UpdateTooltip(Utils.DVR_Info dvrinfo,string dvrID,Utils.DVR_Status dvrstatus, string cmsip, int cmsport)
		{
			string strdata = "CMS IP: " + cmsip + "\n";
			strdata += "CMS Port:   " + cmsport.ToString() + "\n";
			strdata += "DVR MAC:    " + dvrID + "\n";
			strdata += "DVR IP:     " + dvrinfo.Server_IP + "\n";
			strdata += "DVR ID:     " + dvrinfo.Server_ID + "\n";
			strdata += "DVR Status: " + Utils.DVR_Status_Text[(int)dvrstatus];
            strdata += "\n";
            strdata += "DVR edition: " + (dvrinfo.Server_standard == true? "Standard" : "Express");
			this.ToolTipText = strdata;
		}
	}
	//public class DVRTabPage: System.Windows.Forms.TabPage
	//{
	//	public event UCtrlDVR.SocketEvents OnSocketEvents;
	//	public event UCtrlDVR.CloseDVR OnCloseDVR;
	//	string m_dvrguid;
	//	string m_cmsIP;
	//	string m_dvrIP;
	//	public string CMSIP
	//	{
	//		get { return m_cmsIP; }
	//	}
	//	public string DVRGuid
	//	{
	//		get { return m_dvrguid; }
	//	}
	//	public string DVRIP
	//	{
	//		get { return m_dvrIP; }
	//	}
	//	UCtrlDVR m_dvr;
	//	public string TabID
	//	{
	//		get { return m_dvrguid; }
	//	}
	//	public DVRTabPage( string startpath, bool dvrstandard, UCtrlDVR.TimerSetting timersetting,string tabtext, string tabid, Utils.DVR_Info dvrinfo,string cmsIP, int cmsport, int iToTalRow, bool bKeepXml)
	//	{
	//		m_dvrIP = dvrinfo.Server_IP;
	//		m_cmsIP = cmsIP;
	//		this.Text = tabtext;
	//		m_dvrguid = tabid;
	//		m_dvr = new UCtrlDVR(startpath, dvrstandard, timersetting, m_dvrguid, dvrinfo, cmsIP, cmsport, iToTalRow, bKeepXml);
	//	}
	//	public void AddControl()
	//	{
	//		if (m_dvr == null)
	//			return;
	//		m_dvr.OnSocketEvents += new UCtrlDVR.SocketEvents(m_dvr_OnSocketEvents);
	//		m_dvr.OnCloseDVR += new UCtrlDVR.CloseDVR(m_dvr_OnCloseDVR);
	//		m_dvr.Location = new System.Drawing.Point(0, 0);
	//		m_dvr.Dock = System.Windows.Forms.DockStyle.Fill;
	//		this.Controls.Add(m_dvr);
	//	}
	//	void m_dvr_OnCloseDVR(string dvrguid)
	//	{
	//		//throw new Exception("The method or operation is not implemented.");
	//		if( OnCloseDVR != null)
	//		{
	//			OnCloseDVR(dvrguid);
	//		}
	//	}

	//	void m_dvr_OnSocketEvents(Utils.DVR_Info dvrinfo, string cmsip, int cmsport, string dvrguid,string version,Utils.DVR_Status curStatus)
	//	{
	//		if (OnSocketEvents != null)
	//			OnSocketEvents(dvrinfo,cmsip,cmsport,dvrguid, version,curStatus);
	//		//throw new Exception("The method or operation is not implemented.");
	//	}
	//	public void StartDVR()
	//	{
	//		if ( m_dvr != null)
	//			m_dvr.StartClient();
	//	}
	//	public bool StopDVR()
	//	{
	//		if (m_dvr != null)
	//			return m_dvr.StopClient();
	//		return false;
	//	}
	//	public void DeleteDVR()
	//	{
	//		if(m_dvr != null)
	//		{
	//			//m_dvr.OnCloseDVR -= new UCtrlDVR.CloseDVR(m_dvr_OnCloseDVR);
	//			//m_dvr.OnSocketEvents -= new UCtrlDVR.SocketEvents(m_dvr_OnSocketEvents);
	//			m_dvr.RemoveDVR();
	//			m_dvr = null;
	//		}
	//	}
	//	public void ChangeTabCaption(string newcaption)
	//	{
	//		this.Text = newcaption;
	//	}
	//}
	public class WIN32API
	{
		public const uint WM_VSCROLL = 0x0115;
		public const uint SB_BOTTOM = 7;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		public static extern IntPtr PostMessage(IntPtr hwnd, IntPtr wMsg, Int32 wParam, Int32 lParam);
	}
}
