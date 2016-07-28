using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using PACDMSimulator;
namespace CMSTest
{
	public partial class UCtrlDVR : UserControl
	{
		private delegate void AddNewMessageResult(DVR_Msg msg, string cmsmsg, string strstatus, string xmlmessage);
		private delegate void UpdateMessageStatus(string strguid, string cmsresponse, string strstatus);
		private delegate void AddNewMessage(string dvrmsg);
		private delegate void RemoveDVRMessage();
		private delegate void DisConnectCMS();
		private delegate void LabelStatus(string msg);

		#region public delegate and events
		public delegate void SocketEvents(Utils.DVR_Info dvrinfo, string cmsIP, int cmsPort, string dvrguid,string version, Utils.DVR_Status curStatus);
		public event  SocketEvents OnSocketEvents;
		public delegate void CloseDVR(string dvrguid);
		public event CloseDVR OnCloseDVR;
		#endregion
        public struct TimerSetting
        {
            public int m_msg_timeout;
            public int m_msg_retry;
            public int m_dvr_reconnect;
            public int m_msg_configuration;
            public int m_msg_video;
            public int m_msg_alert;
            public int m_keep_alive;
            public int m_channel_status;
        }
		public struct DVR_Msg
		{
			public string msg_id;
			public CMSMessage.DVR_CMS_MESSAGE_ID dvr_cms_msg_id;
			public CMSMessage.DVR_CONFIGURATION dvr_conf;
			public bool m_wait_response;
			public int m_timeout_message;
			public int msg_index;
		}
		public const string STR_GUID = "STR_GUID";
		public const string STR_TIME = "DVT_TIME";
		public const string STR_DVR_CMS_MESSAGE = "DVR_CMS_Msg";
		public const string STR_CMS_DVR_MESSAGE = "CMS_DVR_Msg";
		public const string STR_DVR_CMS_XML_MESSAGE = "DVR_CMS_XML_Message";
		public const string STR_STATUS = "MSG_STATUS";
		public const string STR_PENDING = "Pending!";
		public const string STR_SENT = "Sent!";
		public const string STR_OK = "OK!";
		public const string STR_TIMEOUT = "Time Out";
		private const string STR_START = "Start";
		public const string STR_STOP = "Stop";
        public const string STR_DISCONNECT = "Disconnect";
        public const string STR_CONNECT = "Connect";
		public const int SLLEP_TIME_MESSAGE = 200;
		private const int MAX_DVR_CMS_MESSAGE = 5;
		private const int TIME_OUT_KEEP_ALIVE = 1000 * 30;
		public const int MAX_ITEMS_IN_LIST = 300;
		private const int MESSAGE_SECOND_DELAY = 1000;
        private const int MESSAGE_MINUTE_DELAY = 60 * MESSAGE_SECOND_DELAY;
		private const int TIME_RECONNECT_SOCKET = 30 * 1000;
		private const int COUNT_MESSAGE_CHANGE_CONFIG = 100;
		private const int TIME_OUT_COLSE_CONNECTION_TO_CMS = 15 * 1000;
		AddNewMessageResult del_AddNewMessageResult;
		UpdateMessageStatus del_UpdateMessageStatus;
		AddNewMessage del_AddNewMessage;
		RemoveDVRMessage del_RemoveDVRMessage;
		DisConnectCMS del_DisConnectCMS;
		LabelStatus del_LabelStatus;
		enum DVREventMessage:int
		{
			EVENT_CONNECT,
			EVENT_DISCONNECT,
			EVENT_GET_CONFIG,
			EVENT_ALERT,
			EVENT_KEEP_ALIVE,
			EVENT_VIDEO,
			EVENT_SEND_ALL_INFO,
			EVENT_EXIT,
            EVENT_CONNECT_SOCKET,
			EVENT_CLOSE_SOCKET,
			EVENT_SOCKET_ERROR,
			EVENT_UPDATE_TIME,
            EVENT_MSG_TIMEOUT,
            EVENT_SEND_MSG,
            EVENT_CHANGE_CONFIG,
			//Add new event here
			EVENT_COUNT
		}
		WaitHandle[] m_EventHandle;
		Utils.DVR_Info m_dvrInfo;
        UCtrlDVR.TimerSetting m_timersetting;
		string m_startpath = string.Empty;
		private string m_dvrGuid = string.Empty;
		private string m_dvrIP = string.Empty;
		DVR_Msg m_last_DVR_Msg;
		DataTable m_tableResult;
		bool m_stop = false;
		Thread m_ThreadCheckMessage;
		ManualResetEvent m_wait_send;
		CMSMessage m_cmsMsgManager;
		ClientSocket m_CMS;
		ConverterSimulator Converter;

		volatile int m_msg_timeout = 0;
		volatile int m_msg_retry = 0;
		volatile int m_dvr_nextconnect = 0;

        volatile int m_event_keepalive = 0;
        volatile int m_event_config = 0;
        volatile int m_event_video = 0;
        volatile int m_evnet_alert = 0;
		volatile int m_total_msg;
        int m_dvr_mode = 254;
        List<DVR_Msg> m_DVRMsgList;

        volatile int m_total_row;
        volatile bool m_keep_xml;
        volatile int m_event_channelstatus = 0;

		public string DVRGuid
		{
			get { return m_dvrGuid; }
		}
		public string DVRIP
		{
			get { return m_dvrIP; }
		}
		public Utils.DVR_Info DVRInfo
		{
			get { return m_dvrInfo; }
		}
		
		#region Constructors
		public UCtrlDVR()
		{
			InitializeComponent();
           	DisableInputControls(false);
			InitHandle();
			InitDelegate();
            m_timersetting = new TimerSetting();
            ApplyTimer2GUI(m_timersetting, cbDVRMode.Checked);
            InitTimer(m_timersetting);
            m_DVRMsgList = new List<DVR_Msg>();
		}
      	public UCtrlDVR(string startpath, bool dvrstandard, TimerSetting tmsetting, string guiid, Utils.DVR_Info dvrinfo, string cmsIP, int cmsPort, int iTotalRow, bool bKeepXml)
		{
			InitializeComponent();
           	m_startpath = startpath;
			m_dvrInfo = dvrinfo;
			lblDVRIP.Text = m_dvrIP = dvrinfo.Server_IP;
			lblGUID.Text = m_dvrGuid = guiid;
			txtCMSIP.Text = cmsIP;
			txtCMSPort.Text = cmsPort.ToString();
			txtlocation.Text = dvrinfo.Server_Location;
            cbDVRMode.Checked = dvrstandard;
            txtVersion.Text = dvrinfo.Server_Version;
			//lblQeue.Text = string.Format("Message queue list(Max {0} messages)", MAX_DVR_CMS_MESSAGE);
			DisableInputControls(false);
			InitHandle();
			InitDelegate();
            m_timersetting = tmsetting;
            ApplyTimer2GUI(m_timersetting, cbDVRMode.Checked);
            InitTimer(m_timersetting);
            if (dvrstandard)
                m_dvr_mode = 255;
            m_DVRMsgList = new List<DVR_Msg>();

            m_total_row = MAX_ITEMS_IN_LIST;
            m_keep_xml = false;
            if (iTotalRow > 0 && iTotalRow < MAX_ITEMS_IN_LIST)
                m_total_row = iTotalRow;
            if (bKeepXml)
                m_keep_xml = bKeepXml;
			int sk = dvrinfo.DVRPort + dvrinfo.Id;
			this.txtCMSPort.Text = sk.ToString();
			this.txtCMSPort.Enabled = false;
			Converter = new ConverterSimulator(sk);
		}
		#endregion
        private void InitTimer(TimerSetting setting)
        {
            m_msg_timeout = setting.m_msg_timeout * MESSAGE_MINUTE_DELAY;
            m_msg_retry = setting.m_msg_retry * MESSAGE_MINUTE_DELAY;
            m_dvr_nextconnect = setting.m_dvr_reconnect * MESSAGE_MINUTE_DELAY; ;
            m_event_config = setting.m_msg_configuration * MESSAGE_MINUTE_DELAY; 
            m_event_video = setting.m_msg_video * MESSAGE_MINUTE_DELAY; 
            m_evnet_alert = setting.m_msg_alert * MESSAGE_MINUTE_DELAY; 
            m_event_keepalive = setting.m_keep_alive * MESSAGE_SECOND_DELAY; // MESSAGE_MINUTE_DELAY;
            m_event_channelstatus = setting.m_channel_status * MESSAGE_MINUTE_DELAY;
        }
        private void ApplyTimer2GUI(TimerSetting tmsetting, bool standardversion)
        {
            lstTimerInfo.Items.Clear();
            string stritem = string.Format("Message time: {0} min(s)" , tmsetting.m_msg_timeout );
            lstTimerInfo.Items.Add(stritem);

            stritem = string.Format("Message retry: {0} min(s)", tmsetting.m_msg_retry);
            lstTimerInfo.Items.Add(stritem);
            if(!standardversion)
                stritem = string.Format("Reconnect time: {0} min(s)", tmsetting.m_dvr_reconnect);
            lstTimerInfo.Items.Add(stritem);

            stritem = string.Format("Keep alive: {0} sec(s)", tmsetting.m_keep_alive);
            lstTimerInfo.Items.Add(stritem);

            stritem = string.Format("Channel status: {0} min(s)", tmsetting.m_channel_status);
            lstTimerInfo.Items.Add(stritem);

            if( standardversion)
            {
                stritem = string.Format("Alert time: {0} min(s)", tmsetting.m_msg_alert);
                lstTimerInfo.Items.Add(stritem);

                stritem = string.Format("Configuration time: {0} min(s)", tmsetting.m_msg_configuration);
                lstTimerInfo.Items.Add(stritem);

                stritem = string.Format("Video time: {0} min(s)", tmsetting.m_msg_video);
                lstTimerInfo.Items.Add(stritem);
            }

        }
		private void InitHandle()
		{
			m_EventHandle = new WaitHandle[(int)DVREventMessage.EVENT_COUNT];
			for (int i = 0; i < m_EventHandle.Length; i++)
			{
				if (m_EventHandle[i] != null)
					m_EventHandle[i] = null;
				m_EventHandle[i] = new AutoResetEvent(false);
			}
		}
		private void LoadDVRInfo( Utils.DVR_Info dvrinfo) 
		{
			txtServerID.Text = dvrinfo.Server_ID;
			txtlocation.Text = dvrinfo.Server_Location;
			txtModel.Text = dvrinfo.Server_Model;
			txtVersion.Text = dvrinfo.Server_Version;
			txtServerName.Text = dvrinfo.Server_Name;
		}
		private void DisableInputControls( bool isDisable)
		{
			txtServerID.Enabled = txtServerName.Enabled = txtlocation.Enabled = txtModel.Enabled 
			= txtVersion.Enabled = txtUser.Enabled = txtCMSPort.Enabled = txtCMSIP.Enabled = !isDisable;
		}
		private DataTable InitTableResult()
		{
			DataTable tblret = new DataTable();
			DataColumn col = null;
			col = new DataColumn(STR_GUID);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);

			col = new DataColumn(STR_TIME);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);

			col = new DataColumn(STR_DVR_CMS_MESSAGE);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);

			col = new DataColumn(STR_DVR_CMS_XML_MESSAGE);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);

			col = new DataColumn(STR_CMS_DVR_MESSAGE);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);


			col = new DataColumn(STR_STATUS);
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.AllowDBNull = true;
			tblret.Columns.Add(col);
			return tblret;
		}
		private void InitDelegate()
		{
			del_AddNewMessageResult = new AddNewMessageResult(AddNewResult);
			del_UpdateMessageStatus = new UpdateMessageStatus(UpdateResult);
			del_AddNewMessage = new AddNewMessage(AddNewDVRMsg);
			del_RemoveDVRMessage = new RemoveDVRMessage(RemoveDVRMsg);
			del_DisConnectCMS = new DisConnectCMS(OnDisConnectCMS);
			del_LabelStatus = new LabelStatus(UpdateLableStatus);
			UpdateLableStatus(string.Empty);
		}
		private void UpdateLableStatus(string msg)
		{
			if( this.InvokeRequired)
			{
				this.Invoke(del_LabelStatus, new object[] { msg });
			}
			else
			{
				if (msg != null && msg.Length > 0)
					lblStatus.Text = "Status: " + msg;
				else
					lblStatus.Text = string.Empty;
			}

		}
		private void InitDGResult(bool showxmlmsg)
		{
			if( DgStatus.Columns != null)
				DgStatus.Columns.Clear();

			if (DgStatus.DataSource != null)
				DgStatus.DataSource = null;
			if( m_tableResult != null)
			{
				m_tableResult.Clear();
				m_tableResult.Dispose();
			}
			
			m_tableResult = InitTableResult();

			DgStatus.AutoGenerateColumns = false;
			DgStatus.AllowUserToAddRows = false;
			DgStatus.AllowUserToDeleteRows = false;
			DgStatus.RowHeadersVisible = false;
			DgStatus.RowTemplate.Resizable = DataGridViewTriState.False;
			DgStatus.MultiSelect = false;
			DgStatus.DataSource = m_tableResult;
			DataGridViewTextBoxColumn txtCol = null;
			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_GUID;  
			txtCol.Visible = false;
			txtCol.Width = 0;
			txtCol.Name = STR_GUID;
			txtCol.HeaderText = string.Empty;
			DgStatus.Columns.Add(txtCol);

			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_TIME;
			txtCol.Visible = true;
			txtCol.Width = 120;
			txtCol.Name = STR_TIME;
			txtCol.HeaderText = "DVR Time";
			DgStatus.Columns.Add(txtCol);


			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_DVR_CMS_MESSAGE;
			txtCol.Visible = true;
			txtCol.Width = 160;
			txtCol.Name = STR_DVR_CMS_MESSAGE;
			txtCol.HeaderText = "DVR message";
			DgStatus.Columns.Add(txtCol);

			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_DVR_CMS_XML_MESSAGE;
			txtCol.Visible = showxmlmsg;
			txtCol.Width = 200;
			txtCol.Name = STR_DVR_CMS_XML_MESSAGE;
			txtCol.HeaderText = "DVR=>CMS XML message";
			DgStatus.Columns.Add(txtCol);


			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_CMS_DVR_MESSAGE;
			txtCol.Visible = true;
			txtCol.Width = 230;
			txtCol.Name = STR_CMS_DVR_MESSAGE;
			txtCol.HeaderText = "CMS Message";
			DgStatus.Columns.Add(txtCol);

			txtCol = new DataGridViewTextBoxColumn();
			txtCol.DataPropertyName = STR_STATUS; 
			txtCol.Visible = true;
			txtCol.Width = 50;
			txtCol.Name = STR_CMS_DVR_MESSAGE;
			txtCol.HeaderText = "Status";
			DgStatus.Columns.Add(txtCol);

		}
		private void AddNewDVRMsg(string dvrmsg)
		{
			if (!this.InvokeRequired)
			{
                // limit number of row to reduce memory
                if (lstMsgQueu.Items.Count > m_total_row)
					lstMsgQueu.Items.Clear();
				lstMsgQueu.Items.Add(DateTime.Now.ToString("MM/dd/yyyy HH mm:ss") +  ": " +  dvrmsg);
				CMSTest.WIN32API.PostMessage(lstMsgQueu.Handle, (IntPtr)WIN32API.WM_VSCROLL, (int)WIN32API.SB_BOTTOM, (int)IntPtr.Zero);
			}
			else
				this.BeginInvoke(del_AddNewMessage, new object[] { dvrmsg });

		}
		private void RemoveDVRMsg()
		{
			if (!this.InvokeRequired)
			{
				if (lstMsgQueu.Items.Count > 0)
					lstMsgQueu.Items.RemoveAt(0);
			}
			else
				this.BeginInvoke(new RemoveDVRMessage(RemoveDVRMsg));
		}
		private bool CheckData()
		{
			if( txtServerID.Text.Trim().Length == 0)
			{
				MessageBox.Show("Please input DVR ID.");
				return false;
			}
			if ( !Utils.IsValidDomainName(txtCMSIP.Text) && !Utils.IsValidIpAddressRegEx(txtCMSIP.Text) )
			{
				MessageBox.Show("Invalid CMS server IP address.");
				return false;
			}
			try
			{
				int mport = Convert.ToInt32(txtCMSPort.Text.Trim());
				if( mport <= 0)
				{
					MessageBox.Show("CMS server port must be greater than zero.");
					return false;
				}
			}
			catch (System.Exception ex)
			{

				MessageBox.Show("Invalid CMS server port.");
				return false;
			}
			return true;
			

		}
		private void Start(string startpath, Utils.DVR_Info dvrinfo, string cmsip, int cmsport, bool standardversion)
		{
			if( m_CMS == null)
			{
				m_CMS = new ClientSocket(cmsip, cmsport);
				m_CMS.OnSocketClose += new ClientSocket.SocketClose(m_CMS_OnSocketClose);
				m_CMS.OnSocketreceiveMessage += new ClientSocket.SocketreceiveMessage(m_CMS_OnSocketreceiveMessage);
			}
			m_CMS.Stop = true;
			m_CMS.StopReceive();
			m_CMS.Stop = false;
			DisableInputControls(true);
			cmdStart.Text = STR_STOP;
			StopThreadCheckMessage();
			m_stop = false;
			InitHandle();
            StartThreadMessage(m_EventHandle, startpath, dvrinfo, cmsip, cmsport, standardversion);
			//}
		}

		void m_CMS_OnSocketClose(string cmsip, int cmsport)
		{
			OnSocketChangeEvent(this.m_dvrInfo, cmsip, cmsport, this.m_dvrGuid , string.Empty, Utils.DVR_Status.Socket_Connect_Failed);
			((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_SOCKET_ERROR]).Set();
		}
		private void AddNewResult(DVR_Msg msg, string cmsmsg, string strstatus, string xmlmessage)
		{
			if( this.InvokeRequired)
			{
				this.BeginInvoke(del_AddNewMessageResult, new object[] { msg, cmsmsg, strstatus, xmlmessage });
			}
			else
			{
				//m_wait_dgresult.WaitOne();
				if (DgStatus.Columns[STR_GUID].Visible)
					DgStatus.Columns[0].Visible = false;

                // limit number of row to reduce memory
                if (m_tableResult.Rows.Count > m_total_row)
                {
                    //m_tableResult.Rows.Clear();
                    m_tableResult.Clear();
                    m_tableResult.AcceptChanges();                    
                }                

				DataRow r = m_tableResult.NewRow();
				r[STR_GUID] = msg.msg_id;
				r[STR_TIME] = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
				r[STR_DVR_CMS_MESSAGE] = CMSMessage.GetDVRMessageText(msg.dvr_cms_msg_id);
				r[STR_CMS_DVR_MESSAGE] = cmsmsg;
				r[STR_STATUS] = strstatus;
                // keep xml message make memory high
               if (m_keep_xml)
				   r[STR_DVR_CMS_XML_MESSAGE] = xmlmessage;

				m_tableResult.Rows.Add(r);
				m_tableResult.AcceptChanges();
				int count = DgStatus.RowCount;
				if( count > 0 && cbAutoScroll.Checked)
					DgStatus.CurrentCell = DgStatus[1, count - 1];
				//CMSTest.WIN32API.PostMessage(DgStatus.Handle, (IntPtr)WIN32API.WM_VSCROLL, (int)WIN32API.SB_BOTTOM, (int)IntPtr.Zero);
				//m_wait_dgresult.ReleaseMutex();
			}
		}
		private void UpdateResult(string strguid, string cmsresponse, string strstatus)
		{
			if( this.InvokeRequired)
			{
				this.BeginInvoke(del_UpdateMessageStatus, new object[] { strguid, cmsresponse, strstatus });
			}
			else
			{
				//m_wait_dgresult.WaitOne();
				DataRow[] rows = m_tableResult.Select(STR_GUID + "='" + strguid + "'");
				if(rows != null && rows.Length !=0 )
				{
					rows[0][STR_CMS_DVR_MESSAGE] = cmsresponse;
					rows[0][STR_STATUS] = strstatus;
					m_tableResult.AcceptChanges();
				}
				//m_wait_dgresult.ReleaseMutex();
			}
		}
		private void ThreadStartAddMessage_V301(WaitHandle[]hnd_event, string startpath, string dvrguid, Utils.DVR_Info dvrinfo, string cmsIP, int cmsPort, bool standarversion)
		{
			m_wait_send.Set();
			DVR_Msg msg = new DVR_Msg();
			List<DVR_Msg> lstconfig = GetConfigurationMessageList();
			List<DVR_Msg> lstvideo = GetVideoMessageList();
            int  next_event_index = -1;
			int msg_index = 0;
			int event_index = 0;
			int count_total = 0;
            //bool m_all_info_send = false;
            int current_socket_time_out = m_msg_retry;
            int socket_connect = 0;
            int msg_timeout = 0;
            int msg_keepalive_time = 0;
            int event_alert_time = 0;
            int event_video_time = 0;
            int event_config_time = 0;
            bool send_disconnect = false;
            ((AutoResetEvent)hnd_event[(int)DVREventMessage.EVENT_CONNECT_SOCKET]).Set();
			while (!m_stop)
			{
				//System.Threading.Thread.Sleep(SLLEP_TIME_MESSAGE);
				event_index = WaitHandle.WaitAny(hnd_event, MESSAGE_SECOND_DELAY, false);
				next_event_index = -1;
				switch (event_index)
				{
                    case (int)DVREventMessage.EVENT_CONNECT_SOCKET:
                        #region EVENT_CONNECT_SOCKET
                        {
                            socket_connect = 0;
                            if (ConnectSocket(m_CMS))
                            {
                                OnSocketChangeEvent(dvrinfo, cmsIP, cmsPort, m_dvrGuid, string.Empty, Utils.DVR_Status.Socket_Connected);
                                next_event_index = (int)DVREventMessage.EVENT_CONNECT;
                                current_socket_time_out = m_dvr_nextconnect;//set time for next connect
                            }
                            else
                            {
                                current_socket_time_out = m_msg_retry;//set time for next connect
                                OnSocketChangeEvent(dvrinfo, cmsIP, cmsPort, m_dvrGuid, string.Empty, Utils.DVR_Status.Socket_Connect_Failed);
                            }
                        }
                        #endregion
                        break;
                    case (int)DVREventMessage.EVENT_UPDATE_TIME:
                        #region EVENT_UPDATE_TIME
                        {
                            if (socket_connect % 1000 == 0 && !m_CMS.Connected)
                            {
                                int distance = current_socket_time_out - socket_connect;
                                int min = distance / MESSAGE_MINUTE_DELAY;
                                int sec = (distance % MESSAGE_MINUTE_DELAY) / MESSAGE_SECOND_DELAY;
                                string strmsg = "Reconnecting in next " + min.ToString("00") + " min";
                                if (sec > 0)
                                {
                                    strmsg += " " + sec.ToString("00") + " s";
                                }
                                UpdateLableStatus(strmsg);
                            }
                            socket_connect += MESSAGE_SECOND_DELAY;
                            if (socket_connect >= current_socket_time_out)
                                next_event_index = (int)DVREventMessage.EVENT_CONNECT_SOCKET;
                        }
                        #endregion
                        break;
					case (int)DVREventMessage.EVENT_CONNECT:
						#region EVENT_CONNECT
                        msg_timeout = 0;
						count_total = 0;
						msg = new DVR_Msg();
						msg.msg_id = Guid.NewGuid().ToString();
						msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_CONNECT;
						msg.m_wait_response = true;
                        send_disconnect = false;
						UpdateLableStatus(string.Empty);
						#endregion
						break;
                    case (int)DVREventMessage.EVENT_ALERT:
                        #region EVENT_ALERT
                        {
                            if (!standarversion)
                            {
                                count_total++;
                                msg = GetRamdomDVRMessage();
                                msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT_ALL;
                                next_event_index = (int)DVREventMessage.EVENT_KEEP_ALIVE;
                            }
                            else
                            {
                                if (event_video_time >= m_evnet_alert)
                                    msg = GetRamdomDVRMessage();
                            }
                            msg_timeout = 0;
                            event_alert_time = 0;
                            if (send_disconnect)
                            {
                                next_event_index = -1;
                                msg.msg_id = string.Empty;
                            }
                        }
                        #endregion
                        break;
					//Get config
                    case (int)DVREventMessage.EVENT_GET_CONFIG:
                        #region EVENT_GET_CONFIG
                        {
                            if (!standarversion)
                            {
                                count_total++;
                                if (msg_index > lstconfig.Count)
                                    msg_index = 0;
                                msg = lstconfig[msg_index];
                                msg_index++;
                                if (msg_index < lstconfig.Count)
                                    next_event_index = (int)DVREventMessage.EVENT_GET_CONFIG;
                                else
                                {
                                    next_event_index = (int)DVREventMessage.EVENT_ALERT;
                                    msg_index = 0;
                                }
                            }
                            else
                            {
                                if (event_config_time >= m_event_config)
                                {
                                    msg_index = Utils.GetRandomNumber(0, lstconfig.Count - 1);
                                    msg = lstconfig[msg_index];
                                    msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_CONFIG_CHANGED;
                                }
                            }
                            msg_timeout = 0;
                            event_config_time = 0;
                            if (send_disconnect)
                            {
                                next_event_index = -1;
                                msg.msg_id = string.Empty;
                            }
                        }
                        #endregion
                        break;

					//exit
                    case (int)DVREventMessage.EVENT_EXIT:
                        {
                            msg_timeout = 0;
                            m_stop = true;
                        }
                        break;
					//keep alive
                    case (int)DVREventMessage.EVENT_KEEP_ALIVE:
                        #region EVENT_KEEP_ALIVE
                        {
                            msg_timeout = 0;
                            msg_keepalive_time = 0;
                            if (!standarversion)
                            {
                                count_total++;
                                msg = MessageKeepAlive();
                                next_event_index = (int)DVREventMessage.EVENT_VIDEO;
                                msg_index = 0;
                            }
                            else
                                msg = MessageKeepAlive();
                            if (send_disconnect)
                            {
                                next_event_index = -1;
                                msg.msg_id = string.Empty;
                            }
                        }
                        #endregion
                        break;
					//send all info
                    case (int)DVREventMessage.EVENT_SEND_ALL_INFO:
                        #region EVENT_SEND_ALL_INFO
                        {
                            msg_timeout = 0;
                            if (!standarversion)
                            {
                                count_total++;
                                msg.msg_id = Guid.NewGuid().ToString();
                                msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SENT_ALL_INFO;
                                msg.m_wait_response = false;
                                m_total_msg = count_total;

                            }
                            if (send_disconnect)
                            {
                                next_event_index = -1;
                                msg.msg_id = string.Empty;
                            }
                        }
                        #endregion
                        break;
					//send video
                    case (int)DVREventMessage.EVENT_VIDEO:
                        #region EVENT_VIDEO
                        {
                            if (!standarversion)
                            {
                                count_total++;
                                if (msg_index > lstvideo.Count)
                                    msg_index = 0;

                                msg = lstvideo[msg_index];
                                msg_index++;
                                if (msg_index < lstvideo.Count)
                                    next_event_index = (int)DVREventMessage.EVENT_VIDEO;
                                else
                                {
                                    next_event_index = (int)DVREventMessage.EVENT_SEND_ALL_INFO;
                                    msg_index = 0;
                                }
                            }
                            else
                            {
                                if (event_video_time >= m_event_video)
                                {
                                    msg_index = Utils.GetRandomNumber(0, lstvideo.Count - 1);
                                    msg = lstvideo[msg_index];
                                }
                            }
                            msg_timeout = 0;
                            event_video_time = 0;
                            if (send_disconnect)
                            {
                                next_event_index = -1;
                                msg.msg_id = string.Empty;
                            }
                        }
                        #endregion
                        break;
                    case (int)DVREventMessage.EVENT_MSG_TIMEOUT:
                        {
                            m_last_DVR_Msg.msg_id = string.Empty;
                            UpdateLableStatus("Message time out");
                            next_event_index = (int)DVREventMessage.EVENT_SOCKET_ERROR;
                        }
                        break;
                    case (int)DVREventMessage.EVENT_DISCONNECT:
                        #region EVENT_DISCONNECT
                        {
                            if (!send_disconnect)
                            {
                                send_disconnect = true;
                                msg_timeout = 0;
                                m_last_DVR_Msg.msg_id = string.Empty;
                                msg.msg_id = Guid.NewGuid().ToString();
                                msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_DISCONNECT;
                                msg.m_wait_response = false;

                                next_event_index = (int)DVREventMessage.EVENT_CLOSE_SOCKET;
                                current_socket_time_out = m_dvr_nextconnect;
                                int nWaiting = Utils.GetRandomNumber(1, 10) * 1000;
                                System.Threading.Thread.Sleep(nWaiting);
                            }
                            //System.Threading.Thread.Sleep(Utils.GetRandomNumber(1, 60) * 1000);
                            //UpdateLableStatus("Reconnecting in next " + (current_socket_time_out / MESSAGE_MINUTE_DELAY).ToString() + " min");
                        }
                        #endregion
                        break;
                    case (int)DVREventMessage.EVENT_SOCKET_ERROR:
                        #region EVENT_SOCKET_ERROR
                        {
                            msg_timeout = 0;
                            current_socket_time_out = m_dvr_nextconnect;//m_msg_retry; //AAA
                            m_CMS.Stop = true;
                            m_CMS.StopReceive();
                            m_last_DVR_Msg.msg_id = string.Empty;
                            msg.msg_id = string.Empty;
                            OnSocketChangeEvent(m_dvrInfo, cmsIP, cmsPort, dvrguid, string.Empty, Utils.DVR_Status.Socket_Connect_Failed);
                            UpdateLableStatus("Reconnecting in next " + (current_socket_time_out / MESSAGE_MINUTE_DELAY).ToString("00") + " min");
                        }
                        #endregion
                        break;
                    case (int)DVREventMessage.EVENT_CLOSE_SOCKET:
                        #region EVENT_CLOSE_SOCKET
                        {
                            m_CMS.Stop = true;
                            msg_timeout = 0;
                            m_last_DVR_Msg.msg_id = string.Empty;
                            m_CMS.StopReceive();
                            OnSocketChangeEvent(m_dvrInfo, cmsIP, cmsPort, dvrguid, string.Empty, Utils.DVR_Status.Socket_Disconnect);
                            if (m_stop == true)
                                next_event_index = (int)DVREventMessage.EVENT_EXIT;
                            current_socket_time_out = m_dvr_nextconnect;
                            UpdateLableStatus("Reconnecting in next " + (current_socket_time_out / MESSAGE_MINUTE_DELAY).ToString("00") + " min");
                        }
                        #endregion
                        break;

                    default:
                        #region default                        
                        {
                            //reconect_time += MESSAGE_SECOND_DELAY;
                            //next_connect_time += MESSAGE_SECOND_DELAY;

                            if (standarversion && m_CMS.Connected)
                            {
                                event_alert_time += MESSAGE_SECOND_DELAY;
                                event_video_time += MESSAGE_SECOND_DELAY;
                                event_config_time += MESSAGE_SECOND_DELAY;
                                msg_keepalive_time += MESSAGE_SECOND_DELAY;
                                if (event_alert_time >= m_evnet_alert)
                                    next_event_index = (int)DVREventMessage.EVENT_ALERT;
                                else if (event_config_time >= m_event_config)
                                    next_event_index = (int)DVREventMessage.EVENT_GET_CONFIG;
                                else if (event_video_time >= m_event_video)
                                    next_event_index = (int)DVREventMessage.EVENT_VIDEO;
                                else if (msg_keepalive_time >= m_event_keepalive)
                                    next_event_index = (int)DVREventMessage.EVENT_KEEP_ALIVE;

                            }
                            else
                            {
                                if (m_last_DVR_Msg.msg_id.Length > 0)
                                    msg_timeout += MESSAGE_SECOND_DELAY;

                                if (msg_timeout >= m_msg_timeout)
                                    next_event_index = (int)DVREventMessage.EVENT_MSG_TIMEOUT;
                                else
                                    next_event_index = (int)DVREventMessage.EVENT_UPDATE_TIME;

                            }
                        }
                        #endregion
                        break;
				}
				if (msg.msg_id != null && msg.msg_id.Length > 0 && SendDVRMSG(msg, startpath, dvrguid,m_dvr_mode, dvrinfo, cmsIP, cmsPort, count_total) != ClientSocket.SOCKET_ERROR.SEND_SUCCESSFUL)
				{
					AddNewDVRMsg("Send failed.");
					OnSocketChangeEvent(m_dvrInfo, cmsIP, cmsPort, dvrguid, string.Empty, Utils.DVR_Status.Socket_Disconnect);
					//((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_CLOSE_SOCKET]).Set();
                    current_socket_time_out = m_msg_retry;
					next_event_index = (int)DVREventMessage.EVENT_SOCKET_ERROR;
                    UpdateLableStatus("Reconnecting in next " + (current_socket_time_out / MESSAGE_SECOND_DELAY).ToString() + " min(s)");
				}
				else
				{
                    //if (msg.msg_id != null && msg.msg_id.Length > 0 && msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_DISCONNECT)
                    //{
                    //    //next_event_index = -1;
                    //    m_last_DVR_Msg.msg_id = string.Empty;
                    //    send_disconnect = true;
                    //    System.Threading.Thread.Sleep(Utils.GetRandomNumber(1, 60) * 1000);
                    //    UpdateLableStatus("Reconnecting in next " + (current_socket_time_out / MESSAGE_MINUTE_DELAY).ToString() + " min(s)");
                    //}
					msg.msg_id = string.Empty;
				}
             	if (next_event_index >= 0)
				{
                
					((AutoResetEvent)hnd_event[next_event_index]).Set();
					next_event_index = -1;
				}
					
				System.Threading.Thread.Sleep(SLLEP_TIME_MESSAGE );			
			}
			m_wait_send.Set();
		}
        private bool ConnectSocket(ClientSocket cmssock)
        {
            if (cmssock != null && cmssock.Connected)
                return true;
            return cmssock.StartSocket();

        }
        
		private ClientSocket.SOCKET_ERROR SendDVRMSG(DVR_Msg msg, string startpath, string dvrguid,int dvrmode,  Utils.DVR_Info dvrinfo, string cmsIP, int cmsPort, int totalmsg)
		{
			ClientSocket.SOCKET_ERROR ret = ClientSocket.SOCKET_ERROR.SEND_FAILED;
			#region send data
			if (msg.msg_id != null && msg.msg_id.Length > 0)
			{
				m_last_DVR_Msg = msg;
				string strevent = string.Empty;
				//string xml_msg = string.Empty;
				ret = SendDataMessage(startpath, dvrguid,dvrmode, msg, ref strevent, /*ref xml_msg, */totalmsg);

                if (msg.dvr_conf == CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE && msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE)
                    System.Threading.Thread.Sleep(2000);

				string strsatus = msg.m_wait_response == true ? STR_PENDING : string.Empty;
				AddNewResult(msg, strevent, strsatus, string.Empty/*xml_msg*/);
				if (msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE || msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_CONFIG_CHANGED)
				{
					UpdateResult(msg.msg_id, msg.dvr_conf.ToString(), "");
				}
				if (ret != ClientSocket.SOCKET_ERROR.SEND_SUCCESSFUL)
				{
					UpdateResult(msg.msg_id, strevent, "Send message failed");
					m_last_DVR_Msg.msg_id = string.Empty;
					if (ret == ClientSocket.SOCKET_ERROR.INVALID_SOCKET || ret == ClientSocket.SOCKET_ERROR.CONNECTED_FAIL || ret == ClientSocket.SOCKET_ERROR.SEND_FAILED)
					{
						OnSocketChangeEvent(dvrinfo, cmsIP, cmsPort, m_dvrGuid, string.Empty, Utils.DVR_Status.Socket_Disconnect);
					}
				}
			}
			#endregion
			return ret;
		}
		private void OnSocketChangeEvent(Utils.DVR_Info dvrinfo, string cmsIP,int cmsPort, string dvrGuid, string dvrversion, Utils.DVR_Status dvrstatus )
		{
			if (OnSocketEvents != null && dvrstatus != Utils.DVR_Status.CMS_All_Info_response)
				OnSocketEvents(dvrinfo, cmsIP, cmsPort, m_dvrGuid, dvrversion, dvrstatus);
			string msg = string.Empty;
			switch(dvrstatus)
			{
				case Utils.DVR_Status.Socket_Connected:
					msg = "Socket connected.";
					break;
				case Utils.DVR_Status.Socket_Disconnect:
					msg = "Socket disconnect.";
					break;
				case Utils.DVR_Status.CMS_Connected:
					msg = "CMS connected.";
					break;
				case Utils.DVR_Status.CMS_Disconnect:
					msg = "CMS disconnect.";
					break;
				case Utils.DVR_Status.Socket_Connect_Failed:
					msg = "Socket connect failed.";
					break;
				case Utils.DVR_Status.CMS_All_Info_response:
					msg = "All info response.";
					break;
			}
			AddNewDVRMsg(msg);
		}
		private ClientSocket.SOCKET_ERROR SendDataMessage(string datapath, string dvrid,int dvrmode, DVR_Msg msg, ref string streventname/*, ref string xmlmsg*/, int totalmsg)
		{
			byte[] sockbuff = null;
			if( msg.dvr_cms_msg_id != CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_VIDEO_RESPONSE)
			{
				CMSMessage.DVREVENT dvrevent = CMSMessage.DVREVENT.EVENTBEGIN;
				if (msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT)
				{
					dvrevent = (CMSMessage.DVREVENT)Utils.GetRandomNumber((int)CMSMessage.DVREVENT.EVENTBEGIN + 1, (int)CMSMessage.DVREVENT.EVENTEND - 1);
					streventname = dvrevent.ToString();
				}
				string xmlnessage = m_cmsMsgManager.GetMessageXMLData(datapath, dvrid,dvrmode, msg.dvr_cms_msg_id, dvrevent, msg.dvr_conf, DateTime.Now, totalmsg);
				if (xmlnessage != null)
				{
					//xmlmsg = xmlnessage;
					//Console.WriteLine( xmlnessage );
					sockbuff = CMSMessage.GetMessageBuffer(xmlnessage, msg.dvr_cms_msg_id);
				}
			}
			else
			{
				string fpath = datapath += "\\VideoData\\";
				fpath += string.Format("video_input_{0}.jpg", msg.msg_index.ToString());
				sockbuff = CMSMessage.GetMessageVideoBuffer(fpath, m_cmsMsgManager.DVR_Guid,  msg.msg_index);
				//xmlmsg = fpath;
			}
			
			if (sockbuff == null)
				return ClientSocket.SOCKET_ERROR.SEND_FAILED;
			ClientSocket.SOCKET_ERROR ret = m_CMS.SendData(sockbuff, sockbuff.Length);
			if (sockbuff != null)
				sockbuff = null;
			return ret ;
		}
		private void StartThreadMessage(WaitHandle[]hnd_event, string startpath, Utils.DVR_Info dvrinfo, string cmsip, int cmsport, bool standardversion)
		{
            ThreadStart threadstart = delegate { ThreadStartAddMessage_V301(hnd_event, startpath, m_dvrGuid, dvrinfo, cmsip, cmsport, standardversion); };
			m_ThreadCheckMessage = new Thread(threadstart,Utils.MAX_STACK_SIZE);
			m_ThreadCheckMessage.SetApartmentState(ApartmentState.STA);
			m_ThreadCheckMessage.Start();
		}
		private void StopThreadCheckMessage()
		{
			if (m_wait_send == null)
				return;
			m_stop = true;
			m_wait_send.WaitOne();	
			if( m_ThreadCheckMessage != null)
			{
				m_ThreadCheckMessage.Abort();
				m_ThreadCheckMessage = null;
			}
		}
		private void SendDisconnectMessage()
		{
			DVR_Msg msg = new DVR_Msg();
			msg.msg_id = Guid.NewGuid().ToString();
			msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_DISCONNECT;
			msg.m_wait_response = false;

			string strevent = string.Empty;
			//string xml_msg = string.Empty;
			ClientSocket.SOCKET_ERROR ret = SendDataMessage(null, m_dvrGuid,m_dvr_mode, msg, ref strevent, /*ref xml_msg,*/ 0);

			string strsatus = msg.m_wait_response == true ? STR_PENDING : string.Empty;
			AddNewResult(msg, strevent, strsatus, "");//xml_msg);

            if (ret != ClientSocket.SOCKET_ERROR.SEND_SUCCESSFUL)
			{
				UpdateResult(msg.msg_id, strevent, "Send message failed");
				m_last_DVR_Msg.msg_id = string.Empty;
				if (ret == ClientSocket.SOCKET_ERROR.INVALID_SOCKET || ret == ClientSocket.SOCKET_ERROR.CONNECTED_FAIL)
				{
					OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid, string.Empty, Utils.DVR_Status.Socket_Disconnect);
				}
			}
			else
			{
				OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid, string.Empty, Utils.DVR_Status.CMS_Disconnect);
				System.Threading.Thread.Sleep(1000);
			}
		}
		private DVR_Msg GetRamdomDVRMessage()
		{
			DVR_Msg msg = new DVR_Msg();
			msg.msg_id = Guid.NewGuid().ToString();
			msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT;
			return msg;
		}
		private List<DVR_Msg> GetDVRMessageList()
		{
			List<DVR_Msg> msglist = new List<DVR_Msg>();
			msglist.AddRange(GetConfigurationMessageList());
			msglist.AddRange(GetAlertList());
			msglist.Add(MessageKeepAlive());
			msglist.AddRange(GetVideoMessageList());

			return msglist;
		}
		private List<DVR_Msg> GetAlertList()
		{
			List<DVR_Msg> msglist = new List<DVR_Msg>();
			int total = 10;
			for (int i = 0; i < total; i++ )
			{
				msglist.Add(GetRamdomDVRMessage());
			}
			return msglist;
		}
		private DVR_Msg MessageKeepAlive()
		{
			DVR_Msg msg = new DVR_Msg();
			msg.msg_id = Guid.NewGuid().ToString();
			msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_KEEPALIVE;
			msg.m_wait_response = false;
			return msg;
		}
		private List<DVR_Msg> GetVideoMessageList()
		{
			List<DVR_Msg> msglist = new List<DVR_Msg>();
			int total = 16;
			for (int i = 0; i < total; i++ )
			{
				DVR_Msg conf = new DVR_Msg();
				conf.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_VIDEO_RESPONSE;
				conf.m_timeout_message = 0;
				conf.msg_index = i + 1; ;
				conf.m_wait_response = false;
				conf.msg_id = Guid.NewGuid().ToString();
				msglist.Add(conf);
			}
			return msglist;
		}
		private List<DVR_Msg> GetConfigurationMessageList()
		{
			List<DVR_Msg> msglist = new List<DVR_Msg>();

            string hwname = CMSMessage.ConfigurationFiles[(int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE];
            if (hwname.Length > 0)
            {
                DVR_Msg conf = new DVR_Msg();
                conf.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE;
                conf.dvr_conf = CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE;
                conf.m_timeout_message = 0;
                conf.msg_index = 0;
                conf.m_wait_response = false;
                conf.msg_id = Guid.NewGuid().ToString();
                msglist.Add(conf);
            }
            hwname = CMSMessage.ConfigurationFiles[(int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_VIDEO];
            if (hwname.Length > 0)
            {
                DVR_Msg conf = new DVR_Msg();
                conf.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE;
                conf.dvr_conf = CMSMessage.DVR_CONFIGURATION.EMS_CFG_VIDEO;
                conf.m_timeout_message = 0;
                conf.msg_index = 0;
                conf.m_wait_response = false;
                conf.msg_id = Guid.NewGuid().ToString();
                msglist.Add(conf);
            }

			//add config message
			for (int msgID = 0; msgID < CMSMessage.ConfigurationFiles.Length; msgID++ )
			{
                if (msgID == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE || msgID == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_VIDEO)
                    continue;
                if (msgID == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_LOG_CONTENT)
                    continue;

				string fname = CMSMessage.ConfigurationFiles[msgID];
				if (fname.Length > 0)
				{
					DVR_Msg conf = new DVR_Msg();
					conf.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE;
					conf.dvr_conf = (CMSMessage.DVR_CONFIGURATION)msgID;
					if (conf.dvr_conf == CMSMessage.DVR_CONFIGURATION.EMS_CFG_LOG_CONTENT)
						continue;
					conf.m_timeout_message = 0;
					conf.msg_index = 0;
					conf.m_wait_response = false;
					conf.msg_id = Guid.NewGuid().ToString();
					msglist.Add(conf);
				}

			}
			return msglist;
		}
		private void OnDisConnectCMS()
		{
			if( this.InvokeRequired)
			{
				this.BeginInvoke(del_DisConnectCMS);
			}
			else
			{
				StopClient();
			}

		}
		private void cmdStart_Click(object sender, EventArgs e)
		{
			lblStatus.Text = string.Empty;
            //if (m_wait_dgresult == null)
            //    m_wait_dgresult = new Mutex();
			cmdStart.Enabled = false;
			if (cmdStart.Text == STR_START)
			{
				if (!CheckData())
					return;
				m_stop = false;
				if (m_wait_send == null)
					m_wait_send = new ManualResetEvent(false);
				m_wait_send.Set();
				//Utils.DVR_Info info = new Utils.DVR_Info();
				m_dvrInfo.Server_ID = txtServerID.Text;
				m_dvrInfo.Server_IP = lblDVRIP.Text;
				m_dvrInfo.Server_Location = txtlocation.Text;
				m_dvrInfo.Server_Model = txtModel.Text;
				m_dvrInfo.Server_Name = txtServerName.Text;
				m_dvrInfo.Server_Version = txtVersion.Text;
				m_cmsMsgManager = new CMSMessage(m_dvrInfo, lblGUID.Text, txtUser.Text);
				m_last_DVR_Msg = new DVR_Msg();
				m_last_DVR_Msg.msg_id = string.Empty;
				
				lstMsgQueu.Items.Clear();

				if (Converter.IsRuning)
				{
					Converter.OnStop();
				}

				Converter.OnStart(null);

				if (!Converter.IsRuning)
				{
					return;
				}
				
				InitDGResult(cbViewXMLMessage.Checked);
				Start(m_startpath, m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), cbDVRMode.Checked);
				DisableInputControls( true );
				cmdStart.Text = STR_STOP;
			}
			else
			{
				((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_DISCONNECT]).Set();
				StopThreadCheckMessage();
				Converter.OnStop();
				if (m_CMS != null)
				{
					m_CMS.Stop = true;
					m_CMS.StopReceive();
					OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid, string.Empty, Utils.DVR_Status.Socket_Disconnect);
					m_CMS = null;
				}
				DisableInputControls(false);
				cmdStart.Text = STR_START;
			}
			cmdStart.Enabled = true;
		}
		public void StartClient()
		{
			if( cmdStart.Text == STR_START)
				cmdStart_Click(cmdStart, EventArgs.Empty);
		}
		public bool StopClient()
		{
			if( cmdStart.Text == STR_STOP)
			{
				cmdStart_Click(cmdStart, EventArgs.Empty);
				return true;
			}
			return false;
		}
		public void RemoveDVR()
		{
			cmsClose_Click(cmsClose, EventArgs.Empty);
		}
		
		void m_CMS_OnSocketreceiveMessage(byte[] mbuff, int length)
		{
			int hlen = Marshal.SizeOf(typeof(CMSMessage.MsgHeader));
			byte[] hbuff = new byte[hlen];
			byte[] buffdata = new byte[length - hlen];
			Array.Copy(mbuff, hbuff, hlen);
			Array.Copy(mbuff,hlen,buffdata,0, length - hlen);
			GCHandle handle = GCHandle.Alloc(hbuff, GCHandleType.Pinned);
			CMSMessage.MsgHeader msgheader = (CMSMessage.MsgHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CMSMessage.MsgHeader));
			handle.Free();
			mbuff = null;
			hbuff = null;
            int event_index = -1;
           	switch (msgheader.msg_id)
			{
				case (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_CONNECT_RESPONSE:
					OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid,string.Empty, Utils.DVR_Status.CMS_Connected);
                    if( cbDVRMode.Checked)
                        event_index = (int)DVREventMessage.EVENT_KEEP_ALIVE;
                   	break;
				case (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_DISCONNECT_RESPONSE:
					//((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_EXIT]).Set();
                    event_index = (int)DVREventMessage.EVENT_EXIT;
					OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid, string.Empty, Utils.DVR_Status.CMS_Disconnect);
					break;
				case (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SENT_ALL_INFO_RESPONSE:
                    
					OnSocketChangeEvent(m_dvrInfo, txtCMSIP.Text, Convert.ToInt32(txtCMSPort.Text), m_dvrGuid, string.Empty, Utils.DVR_Status.CMS_All_Info_response);
                    
                    if(!cbDVRMode.Checked)
                        event_index = (int)DVREventMessage.EVENT_DISCONNECT;
					//((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_DISCONNECT]).Set();
					break;
				case (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG:
                    event_index = (int)DVREventMessage.EVENT_GET_CONFIG;
					//((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_GET_CONFIG]).Set();
					break;
         	}
            //Console.WriteLine( CMSMessage.GetDVRMessageText((CMSMessage.DVR_CMS_MESSAGE_ID)msgheader.msg_id));
			UpdateResult(m_last_DVR_Msg.msg_id, CMSMessage.GetDVRMessageText((CMSMessage.DVR_CMS_MESSAGE_ID)msgheader.msg_id), STR_OK);
			m_last_DVR_Msg.msg_id = string.Empty;
			buffdata = null;
            if( event_index >= 0)
                ((AutoResetEvent)m_EventHandle[event_index]).Set();
			//throw new Exception("The method or operation is not implemented.");
		}

		private void cmsClose_Click(object sender, EventArgs e)
		{
			if (cmdStart.Text == STR_STOP)
			{
				cmdStart_Click(cmdStart, EventArgs.Empty);
			}
			if (OnCloseDVR != null)
			{
				OnCloseDVR(m_dvrGuid);
				m_cmsMsgManager = null;
				this.Dispose();
			}
		}
		
		private void cbViewXMLMessage_CheckedChanged(object sender, EventArgs e)
		{
			if ( DgStatus.Columns.Contains(STR_DVR_CMS_XML_MESSAGE) && ( cbViewXMLMessage.Checked != DgStatus.Columns[STR_DVR_CMS_XML_MESSAGE].Visible) )
				DgStatus.Columns[STR_DVR_CMS_XML_MESSAGE].Visible = cbViewXMLMessage.Checked;
		}
		private void cmsSetDelay_Click(object sender, EventArgs e)
		{
            frmSetupTimer obj = new frmSetupTimer(m_timersetting, cbDVRMode.Checked);
            if (obj.ShowDialog(this) == DialogResult.OK)
            {
                m_timersetting = obj.Setting;
                ApplyTimer2GUI(m_timersetting, cbDVRMode.Checked);
                InitTimer(m_timersetting);
                //((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_UPDATE_TIME_OUT]).Set();
            }
	
		}

		private void cmdDisconnect_Click(object sender, EventArgs e)
		{
			if (cmdStart.Text == STR_STOP)
			{
				Converter.OnStop();
				((AutoResetEvent)m_EventHandle[(int)DVREventMessage.EVENT_DISCONNECT]).Set();
			}
		
		}

        private void cmdClear_Click(object sender, EventArgs e)
        {
            lstMsgQueu.Items.Clear();
            if (DgStatus != null && DgStatus.DataSource != null)
                (DgStatus.DataSource as DataTable).Clear();
        }

        private void lblQeue_Click(object sender, EventArgs e)
        {

        }

	}
}
