using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PACDMSimulator;
using System.Xml;
using ConvertMessage;
using ConverterDB.Model;
using Timer = System.Windows.Forms.Timer;

namespace CMSTest
{
	public enum DVRStates
	{
		Started,
		Starting,
		Connecting,
		Connected,
		Stoped,
		Stopping,
		Disconnected,
		Disconnecting,
		Runing
	}

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

	public partial class DVRSimulator : UserControl
	{
		#region CONST
		public const string STR_GUID = "STR_GUID";
		public const string STR_TIME = "DVT_TIME";
		public const string STR_DVR_CMS_MESSAGE = "DVR_CMS_Msg";
		public const string STR_CMS_DVR_MESSAGE = "CMS_DVR_Msg";
		public const string STR_DVR_CMS_XML_MESSAGE = "DVR_CMS_XML_Message";
		public const string STR_STATUS = "MSG_STATUS";
		public const string STR_PENDING = "Pending!";
		public const string STR_SENT = "Sent!";
		public const string STR_OK = "OK!";
		private const string STR_START = "Start";
		public const string STR_STOP = "Stop";
		public const string STR_DISCONNECT = "Disconnect";
        public const string STR_CONNECT = "Connect";
		const string xPath_configuration_id = "/message/body/configuration_id";
		const string xPath_id = "/message/header/id";
		private const int MESSAGE_MINUTE_DELAY = 60;
		#endregion

		#region Contructor
		private ConverterDVRSimulator converter;
		private Timer DVR;
		private bool dvrStandar;
		private TimerSetting timers;
		private TimerSetting Temptimers;
		private Utils.DVR_Info dvrinfo;
		private string cmsIP;
		private int cmsPort;
		private string GuiId;
		private DVRTabPages tab;
		private CMSMessage m_cmsMsgManager;
		private int m_dvr_mode = 254;
		private List<string> ReceivedData = new List<string>();
		private int ReceiedTotal;
		private DataTable m_tableResult;
		private int m_total_row;
		private bool ConfigRequest = false;
		private bool IsSync = false;
		public DVRStates DVRState { get; set; }
		private bool killTasks = false;
		private static object lockThread = new object();
		List<DVR_Msg> lstconfig;
		DVR_Msg msg;
		readonly XmlDocument xmlDoc = new XmlDocument();
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		#endregion
		#region Control functions
		public DVRSimulator()
		{
			InitializeComponent();
		}

		public DVRSimulator(TimerSetting timersetting,Utils.DVR_Info dvrinfo, int iToTalRow, bool bKeepXml, DVRTabPages tab)
		{
			InitializeComponent();
			this.dvrStandar = dvrinfo.Server_standard;
			this.timers = timersetting;
			this.dvrinfo = dvrinfo;
			this.cmsIP = dvrinfo.ServerUrl;
			this.cmsPort = dvrinfo.DVRPort;
			this.tab = tab;
			GuiId = dvrinfo.Mac;
			DVR = new Timer();
			DVR.Interval = 1000;
			DVR.Enabled = true;
			m_total_row = iToTalRow > 0 ? iToTalRow : 300;
			DVR.Tick += DVR_Tick;
			DVR.Stop();
			Temptimers = new TimerSetting();
			converter = new ConverterDVRSimulator();
			//converter.InfoDVR = tab.Wconverter.InfoDVR;
			converter.InfoDVR = GetMsgDVRInfo();
			converter.ServiceConfig = NewServerConfig();
			converter.setWebConnect(_cancellationTokenSource);
			DVRState = DVRStates.Stoped;
			SetValueDVR();
			SetButtonDVR();
			InitDGResult(cbViewXMLMessage.Checked);
			ApplyTimer2GUI(timers, cbDVRMode.Checked);
		}

		private void cmdStart_Click(object sender, EventArgs e)
		{

			cmdStart.Enabled = false;
			//InitDGResult(cbViewXMLMessage.Checked);
			if (cmdStart.Text == STR_START)
			{
				killTasks = false;
				if (!CheckData())
					return;
				this.DVRState = DVRStates.Starting;
				StartClient();
				DVRConnect();
				cmdStart.Text = STR_STOP;
				cmdStart.Enabled = true;
				//DVRState = DVRStates.Connected;
				SetButtonDVR();
			}
			else
			{
				this.DVRState = DVRStates.Stopping;
				DVR.Stop();
				cmdStart.Text = STR_START;
				cmdStart.Enabled = true;
				this.DVRState = DVRStates.Stoped;
				this.cmsClose.Enabled = true;
				StopClient();
				SetButtonDVR();
			}
		}

		private bool CheckData()
		{
			if (txtCMSIP.Text == null || !Utils.IsValidUri(txtCMSIP.Text))
			{
				MessageBox.Show("CMS Server link input wrong format.");
				return false;
			}

			if (txtServerID.Text.Trim().Length == 0)
			{
				MessageBox.Show("Please input DVR ID.");
				return false;
			}
			if (!Utils.IsValidDomainName(txtCMSIP.Text) && !Utils.IsValidIpAddressRegEx(txtCMSIP.Text))
			{
				MessageBox.Show("Invalid CMS server IP address.");
				return false;
			}
			try
			{
				int mport = Convert.ToInt32(txtCMSPort.Text.Trim());
				if (mport <= 0)
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

		private void cmsClose_Click(object sender, EventArgs e)
		{
			lock (this)
			{
				killTasks = true;
				DVRState = DVRStates.Stoped;
				StopClient();
				DVR.Stop();
				DVR.Tick -= DVR_Tick;
				if (_cancellationTokenSource != null)
					_cancellationTokenSource.Dispose();
				DVR.Dispose();
				DVR = null;
				if (converter != null)
					converter.Dispose();
				converter = null;
				if (m_tableResult != null)
					m_tableResult.Dispose();
				m_tableResult = null;
				tab.Dispose();
				tab.RiseDVRAction(0, 0);
				tab = null;
				this.Dispose();
			}
		}

		async void DVR_Tick(object sender, EventArgs e)
		{
			DVR.Stop();
			SetButtonDVR();
			if (DVRState == DVRStates.Connected)
			{
				CheckReceived();
				if (ReceiedTotal > 0 && IsSync == true)
				{					
					//SendDataToWeb();
					await SendDataToWebSync();
				}

				if (IsSync == true)
				{
					//Task_channel_status();
					Task_msg_alert();
					Task_keep_alive();
				}
			}
			else
			{
				if (IsSync == true)
				{
					Task_dvr_reconnect();
				}
			}

			if (_cancellationTokenSource.IsCancellationRequested)
				return;
			SetButtonDVR();
			UpdateLableStatus(DVRState.ToString());
			ApplyTimer2GUI(timers, cbDVRMode.Checked);
			DVR.Start();
		}

		private void cmdDisconnect_Click(object sender, EventArgs e)
		{
			cmdDisconnect.Enabled = false;
			if (cmdDisconnect.Text == STR_DISCONNECT)
			{
				DVRState = DVRStates.Disconnecting;
				cmdDisconnect.Text = STR_CONNECT;
				cmdDisconnect.Enabled = true;
				DVRState = DVRStates.Disconnected;
				if(_cancellationTokenSource != null)
					_cancellationTokenSource.Cancel();
				SetButtonDVR();
			}
			else
			{
				DVRState = DVRStates.Connecting;
				StartClient();
				DVRConnect();
				cmdDisconnect.Text = STR_DISCONNECT;
				cmdDisconnect.Enabled = true;
				DVRState = DVRStates.Connected;
				SetButtonDVR();
			}
		}
		#endregion

		#region Process

		private async void DVRConnect()
		{
			//DVR_Msg msg = new DVR_Msg();
			lock (lockThread)
			{
				msg.msg_id = Guid.NewGuid().ToString();
			}
			msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_CONNECT;
			msg.m_wait_response = true;

			if (_cancellationTokenSource.IsCancellationRequested)
				return;

			var getMessage = GetMessageDataToSend(tab.Startpath, this.dvrinfo.Server_ID, m_dvr_mode, msg, 0);

			if (getMessage == null)
			{
				AddNewResult(msg, msg.dvr_conf.ToString(), "Get Message Failed", string.Empty);
				DVRState = DVRStates.Disconnected;
			}

			try
			{
				if (_cancellationTokenSource.IsCancellationRequested)
					return;

				var received = await SendDataMessage(msg, getMessage);

				if (_cancellationTokenSource.IsCancellationRequested)
					return;

				string strsatus = msg.m_wait_response == true ? STR_PENDING : string.Empty;
				if (received != null)
				{
					List<string> temp = new List<string>();
					temp.AddRange(received);
					foreach (var str in temp)
					{

						if (!String.IsNullOrEmpty(str))
						{
							//XmlDocument xmlDoc = new XmlDocument();
							xmlDoc.LoadXml(str);
							XmlNode xNodeConfig = xmlDoc.SelectSingleNode(xPath_id);
							int msgID = xNodeConfig == null ? 10000 : Convert.ToInt32(xNodeConfig.InnerText);
							string streventname = CMSMessage.GetDVRMessageText((CMSTest.CMSMessage.DVR_CMS_MESSAGE_ID) msgID);
							AddNewResult(msg, streventname, "OK", string.Empty);
						}
					}

					ReceivedData.AddRange(received);
					ReceiedTotal += received.Count;

					if (ReceiedTotal > 0)
					{
						SyncConverter();
					}

					DVRState = DVRStates.Connected;
				}
				else
				{
					AddNewResult(msg, msg.dvr_conf.ToString(), "Failed", string.Empty);
					//UpdateResult(msg.msg_id, msg.dvr_conf.ToString(), "Failed");
					DVRState = DVRStates.Disconnected;
				}
			}
			catch (Exception)
			{
			}

		}

		private void CheckReceived()
		{ 
			
		}

		private void SetMessageToSend(string message)
		{
			int msg_index = 0;
			int msgID = 0;
			int ConfigId = 0;
			if (!String.IsNullOrEmpty(message))
			{
				//XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(message);
				XmlNode xNodeConfig = xmlDoc.SelectSingleNode(xPath_configuration_id);
				ConfigId = xNodeConfig == null ? 0 : Convert.ToInt32(xNodeConfig.InnerText);
				xNodeConfig = xmlDoc.SelectSingleNode(xPath_id);
				msgID = xNodeConfig == null ? 0 : Convert.ToInt32(xNodeConfig.InnerText);
			}

			if (msgID == (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG && ConfigId == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE)
			{
				msg_index = 0;
			}
			else
			{
				var msgdvr = lstconfig.FirstOrDefault(t => t.msg_index == ConfigId);
				if (chkRandomMessage.Checked || msgdvr.msg_index == 0)
					msg_index = Utils.GetRandomNumber(1, lstconfig.Count - 1);
				else
				{
					msg_index = lstconfig.IndexOf(msgdvr);
				}
			}
			msg = lstconfig[msg_index];

			if (msgID == (int)CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG && ConfigId == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE && ConfigRequest == false)
			{
				msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE;
				msg.dvr_conf = (CMSMessage.DVR_CONFIGURATION)ConfigId;
				ConfigRequest = true;
			}
			else
			{
				msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_CONFIG_RESPONSE;
				if (ConfigId == (int)CMSMessage.DVR_CONFIGURATION.EMS_CFG_HARDWARE)
					msg.dvr_conf = CMSMessage.DVR_CONFIGURATION.EMS_CFG_VISION_COUNT;
			}
		}

		private async void SyncConverter()
		{
			IsSync = false;
			List<string> received = null;
			List<string> temp = new List<string>();
			temp.AddRange(ReceivedData);
			ReceivedData = new List<string>();
			ReceiedTotal = 0;
			var context = TaskScheduler.Default;

			 await Task.Factory.StartNew(() =>
			{
				foreach (var str in temp)
				{
					if (_cancellationTokenSource.IsCancellationRequested)
						return;

					SetMessageToSend(str);
					var getMessage = GetMessageDataToSend(tab.Startpath, this.dvrinfo.Server_ID, m_dvr_mode, msg, 0);

					if (getMessage == null) continue;

					try
					{
						received = SendDataMessageSync(msg, getMessage);//.Result;
					}
					catch (Exception)
					{
					}

					if (received != null)
					{
						if (received.Count > 0)
						{
							ReceivedData.AddRange(received);
							ReceiedTotal += received.Count;
						}
						string streventname = msg.dvr_conf.ToString();
						AddNewResult(msg, streventname, "OK", string.Empty);

					}
					else
					{
						string streventname = msg.dvr_conf.ToString();
						AddNewResult(msg, streventname, "Failed", string.Empty);
						if (ConfigRequest == false)
						{
							ConfigRequest = false;
						}
					}
				}

			}, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, context);
			temp = null;
			IsSync = true;
		}

		private List<string> SendDataMessageSync(DVR_Msg msg, string Xml)
		{
			//return Task.Run(() =>
			//{
				if (!killTasks)
				{
					string id = Convert.ToString((int)msg.dvr_cms_msg_id);
					var ret = converter.DVR_SendToWebAPI(Xml, id);
					return ret;
				}
				else
				{
					return null;

				}
			//}, _cancellationTokenSource.Token);
		}

		private async Task SendDataToWebSync()
		{
			var context = TaskScheduler.Default;

			await Task.Factory.StartNew(() =>
			{
				try
				{


					List<string> received;
					List<string> temp = new List<string>();
					temp.AddRange(ReceivedData);
					ReceivedData = new List<string>();
					ReceiedTotal = 0;
					foreach (var str in temp)
					{
						if (_cancellationTokenSource.IsCancellationRequested)
							return;

						SetMessageToSend(str);

						var getMessage = GetMessageDataToSend(tab.Startpath, this.dvrinfo.Server_ID, m_dvr_mode, msg, 0);

						if (getMessage != null)
						{
							received = SendDataMessageSync(msg, getMessage); //.Result;
							if (received != null)
							{
								string streventname = msg.dvr_conf.ToString();
								AddNewResult(msg, streventname, "OK", string.Empty);

							}
							else
							{
								string streventname = msg.dvr_conf.ToString();
								AddNewResult(msg, streventname, "Failed", string.Empty);
							}
						}
					}
					temp = null;
				}
				catch (Exception)
				{
				}
			}, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, context);
		}

		private void AddNewResult(DVR_Msg msg, string cmsmsg, string strstatus, string xmlmessage)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke((Action<DVR_Msg,string,string,string>)AddNewResult, new object[] { msg, cmsmsg, strstatus, xmlmessage });
			}
			else
			{
				//m_wait_dgresult.WaitOne();
				//if (DgStatus.Columns[STR_GUID].Visible)
				//	DgStatus.Columns[0].Visible = false;

				// limit number of row to reduce memory
				if (m_tableResult == null)
					return;

				if (m_tableResult.Rows.Count > m_total_row)
				{
					//m_tableResult.Rows.Clear();
					DgStatus = new DataGridView();
					m_tableResult.Clear();
					m_tableResult.AcceptChanges();
					DgStatus.DataSource = m_tableResult;
				}

				DataRow r = m_tableResult.NewRow();
				r[STR_GUID] = msg.msg_id;
				r[STR_TIME] = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
				r[STR_DVR_CMS_MESSAGE] = CMSMessage.GetDVRMessageText(msg.dvr_cms_msg_id);
				r[STR_CMS_DVR_MESSAGE] = cmsmsg;
				r[STR_STATUS] = strstatus;
				// keep xml message make memory high
				if (cbViewXMLMessage.Checked)
					r[STR_DVR_CMS_XML_MESSAGE] = xmlmessage;

				m_tableResult.Rows.Add(r);
				m_tableResult.AcceptChanges();

				int count = DgStatus.RowCount;
				if (count > 0 && cbAutoScroll.Checked)
					DgStatus.CurrentCell = DgStatus[1, count - 1];
			}
		}
		#endregion

		#region DVR Tasks function

		public Utils.DVR_Info GetDvrInfo()
		{
			return dvrinfo;
		}

		int waiting = 0;
		private string Waiting(int waiting)
		{ 
			string wt= " | Runing: ";
			switch (waiting)
			{
				case 0: { wt = wt + "."; break; }
				case 1: { wt = wt + ".."; break; }
				case 2: { wt = wt + "..."; break; }
				case 3: { wt = wt + "...."; break; }
				case 4: { wt = wt + "....."; break; }
			}
			return wt;
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

		private void InitDGResult(bool showxmlmsg)
		{
			if (DgStatus.Columns != null)
				DgStatus.Columns.Clear();

			if (DgStatus.DataSource != null)
				DgStatus.DataSource = null;
			if (m_tableResult != null)
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

		private void UpdateLableStatus(string msg)
		{
			//DgStatus.Refresh();
			if (msg != null && msg.Length > 0)
			{
				lblStatus.Text = "Status: " + msg + Waiting(waiting);
				waiting++;
				if (waiting > 4)
					waiting = 0;
			}
			else
				lblStatus.Text = string.Empty;
		}

		private void SetButtonDVR()
		{
			switch (DVRState)
			{
				case DVRStates.Started:
					{
						cmdStart.Enabled = true;
						cmdDisconnect.Enabled = true;
						cmsSetDelay.Enabled = false;
						cmdClear.Enabled = false;
						cmsClose.Enabled = true;
						break;
					}

				case DVRStates.Stoped:
					{
						cmdStart.Enabled = true;
						cmdDisconnect.Enabled = false;
						cmsSetDelay.Enabled = true;
						cmdClear.Enabled = false;
						cmsClose.Enabled = true;
						break;
					}
				case DVRStates.Disconnected:
					{
						cmdStart.Enabled = false;
						cmdDisconnect.Enabled = true;
						cmsSetDelay.Enabled = true;
						cmdClear.Enabled = true;
						cmsClose.Enabled = true;
						break;
					}

				case DVRStates.Connected:
					{
						cmdStart.Enabled = true;
						cmdDisconnect.Enabled = true;
						cmsSetDelay.Enabled = false;
						cmdClear.Enabled = false;
						cmsClose.Enabled = false;
						break;
					}
				default:
					{
						cmdStart.Enabled = false;
						cmdDisconnect.Enabled = false;
						cmsSetDelay.Enabled = false;
						cmdClear.Enabled = false;
						cmsClose.Enabled = true;
						break;
					}
			}
		}

		private async void Task_keep_alive()
		{
			if (Temptimers.m_keep_alive == timers.m_keep_alive)
			{
				Temptimers.m_keep_alive = 0;
				Thread.Sleep(10);
				lock (lockThread)
				{
					msg.msg_id = Guid.NewGuid().ToString();
				}
				msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_KEEPALIVE;

				var getMessage = GetMessageDataToSend(tab.Startpath, this.dvrinfo.Server_ID, m_dvr_mode, msg, 0);

				if (getMessage == null)
				{
					return;
				}

				try
				{

					var received = await SendDataMessage(msg, getMessage);
					if (received != null)
					{
						ReceivedData.AddRange(received);
						ReceiedTotal += received.Count;
						AddNewResult(msg, msg.dvr_cms_msg_id.ToString(), "OK", string.Empty);
					}
					else
					{
						AddNewResult(msg, msg.dvr_cms_msg_id.ToString(), "Failed", string.Empty);
					}
				}
				catch (Exception)
				{
				}
			}
			else
			{
				Temptimers.m_keep_alive++;
			}
		}

		private void Task_dvr_reconnect()
		{
			if (Temptimers.m_dvr_reconnect == timers.m_dvr_reconnect * MESSAGE_MINUTE_DELAY)
			{
				cmdDisconnect_Click(null,null);
				Temptimers.m_dvr_reconnect = 0;
			}
			else
			{
				Temptimers.m_dvr_reconnect++;
			}
		}

		private async void Task_msg_alert()
		{
			if (Temptimers.m_msg_alert == timers.m_msg_alert * MESSAGE_MINUTE_DELAY)
			{
				Temptimers.m_msg_alert = 0;
				
				lock (lockThread)
				{
					msg.msg_id = Guid.NewGuid().ToString();
				}
				msg.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT;

				var getMessage = GetMessageDataToSend(tab.Startpath, this.dvrinfo.Server_ID, m_dvr_mode, msg, 0);

				if (getMessage == null)
				{
					return;
				}

				try
				{

					var received = await SendDataMessage(msg, getMessage);
					if (received != null)
					{
						ReceivedData.AddRange(received);
						ReceiedTotal += received.Count;
						AddNewResult(msg, msg.dvr_cms_msg_id.ToString(), "OK", string.Empty);
					}
					else
					{
						AddNewResult(msg, msg.dvr_cms_msg_id.ToString(), "Failed", string.Empty);
					}
				}
				catch (Exception)
				{
				}
			}
			else
			{
				Temptimers.m_msg_alert++;
			}

		}

		private void SetValueDVR()
		{
			lblDVRIP.Text = dvrinfo.Server_IP;
			lblGUID.Text = GuiId;
			txtCMSIP.Text = cmsIP;
			txtCMSPort.Text = cmsPort.ToString();
			txtlocation.Text = dvrinfo.Server_Location;
			cbDVRMode.Checked = dvrStandar;
			txtVersion.Text = dvrinfo.Server_Version;
			txtServerID.Text = dvrinfo.Server_ID;
			txtServerName.Text = dvrinfo.Server_Name;
			txtModel.Text = dvrinfo.Server_Model;
		}

		private string calcTime(int time, int timeshow)
		{
			int m = (time * MESSAGE_MINUTE_DELAY - timeshow) / MESSAGE_MINUTE_DELAY;
			int f = (time * MESSAGE_MINUTE_DELAY - timeshow) % MESSAGE_MINUTE_DELAY;
			return String.Format("{0}:{1}", m, f);
		}

		private void ApplyTimer2GUI(TimerSetting tmsetting, bool standardversion)
		{
			lstTimerInfo.Items.Clear();
		//	string stritem = string.Format("Message time: {0} min(s). Start after: {1}.", tmsetting.m_msg_timeout, calcTime(tmsetting.m_msg_timeout, Temptimers.m_msg_timeout));
		//	lstTimerInfo.Items.Add(stritem);

			// stritem = string.Format("Message retry: {0} min(s). Start after: {1}.", tmsetting.m_msg_retry, calcTime(tmsetting.m_msg_retry, Temptimers.m_msg_retry));
			//lstTimerInfo.Items.Add(stritem);
			string stritem = null;
			if (!standardversion)
			{
				stritem = string.Format("Reconnect time: {0} min(s). Start after: {1}.", tmsetting.m_dvr_reconnect,
					calcTime(tmsetting.m_dvr_reconnect, Temptimers.m_dvr_reconnect));
				lstTimerInfo.Items.Add(stritem);
			}

			stritem = string.Format("Keep alive: {0} sec(s). Start after: {1}.", tmsetting.m_keep_alive, tmsetting.m_keep_alive - Temptimers.m_keep_alive);
			lstTimerInfo.Items.Add(stritem);

			//stritem = string.Format("Channel status: {0} min(s). Start after: {1}.", tmsetting.m_channel_status, calcTime(tmsetting.m_channel_status, Temptimers.m_channel_status));
			//lstTimerInfo.Items.Add(stritem);

			if (standardversion)
			{
				stritem = string.Format("Alert time: {0} min(s). Start after: {1}.", tmsetting.m_msg_alert, calcTime(tmsetting.m_msg_alert, Temptimers.m_msg_alert));
				lstTimerInfo.Items.Add(stritem);

				stritem = string.Format("Configuration time: {0} min(s). Start after: {1}.", tmsetting.m_msg_configuration, calcTime(tmsetting.m_msg_configuration, Temptimers.m_msg_configuration));
				lstTimerInfo.Items.Add(stritem);

				stritem = string.Format("Video time: {0} min(s). Start after: {1}.", tmsetting.m_msg_video, calcTime(tmsetting.m_msg_video, Temptimers.m_msg_video));
				lstTimerInfo.Items.Add(stritem);
			}

		}
		#endregion

		#region DVR Message functions
		public void StartDVR()
		{
			if (DVRState == DVRStates.Disconnected || DVRState == DVRStates.Stoped)
			{
				killTasks = false;
				//InitDGResult(cbViewXMLMessage.Checked);
				this.DVRState = DVRStates.Starting;
				StartClient();
				DVRConnect();
				cmdStart.Text = STR_STOP;
				cmdStart.Enabled = true;
			}
		}
		private void StartClient()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			converter.SetCancelToken(_cancellationTokenSource);
			dvrinfo.Server_ID = txtServerID.Text;
			//dvrinfo.Server_IP = lblDVRIP.Text;
			dvrinfo.Server_Location = txtlocation.Text;
			dvrinfo.Server_Model = txtModel.Text;
			dvrinfo.Server_Name = txtServerName.Text;
			dvrinfo.Server_Version = txtVersion.Text;
			m_cmsMsgManager = new CMSMessage(dvrinfo, lblGUID.Text, txtUser.Text);
			lstconfig = GetConfigurationMessageList();
			GetVideoMessageList();
			DVR.Start();
		}

		public void StopClient()
		{
			if (DVRState != DVRStates.Disconnected || DVRState != DVRStates.Stoped)
			{
				IsSync = false;
				ConfigRequest = false;
				killTasks = true;
				this.DVRState = DVRStates.Stopping;
				DVR.Stop();
				cmdStart.Text = STR_START;
				cmdStart.Enabled = true;
				this.DVRState = DVRStates.Stoped;
				this.cmsClose.Enabled = true;
				if (_cancellationTokenSource != null)
					_cancellationTokenSource.Cancel(true);
				converter.CancelRequest();
				SetButtonDVR();
				if (m_tableResult.Rows.Count > 0)
					m_tableResult.Rows.Clear();
			}
		}

		public void CloseClient()
		{
			killTasks = true;
			if(_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();
			cmsClose_Click(null, null);
		}

		public void DisconnectClient()
		{
			if (DVRState != DVRStates.Disconnected || DVRState != DVRStates.Stoped)
			{
				killTasks = true;
				DVRState = DVRStates.Disconnecting;
				DVR.Stop();
				cmdDisconnect.Text = STR_CONNECT;
				cmdDisconnect.Enabled = true;
				DVRState = DVRStates.Disconnected;
			}
		}

		public void RemoveDVR()
		{
			Temptimers = new TimerSetting();
		}

		private string GetMessageDataToSend(string datapath, string dvrid, int dvrmode, DVR_Msg msg, int totalmsg)
		{
			byte[] sockbuff = null;
			string data = null;
			if (msg.dvr_cms_msg_id != CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_VIDEO_RESPONSE)
			{
				CMSMessage.DVREVENT dvrevent = CMSMessage.DVREVENT.EVENTBEGIN;
				if (msg.dvr_cms_msg_id == CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_SERVER_ALERT)
				{
					dvrevent = (CMSMessage.DVREVENT)Utils.GetRandomNumber((int)CMSMessage.DVREVENT.EVENTBEGIN + 1, (int)CMSMessage.DVREVENT.EVENTEND - 1);
				}
				string xmlnessage = m_cmsMsgManager.GetMessageXMLData(datapath, dvrid, dvrmode, msg.dvr_cms_msg_id, dvrevent, msg.dvr_conf, DateTime.Now, totalmsg);
				if (xmlnessage != null)
				{
					data = xmlnessage;
				}
			}
			else
			{
				string fpath = datapath += "\\VideoData\\";
				fpath += string.Format("video_input_{0}.jpg", msg.msg_index.ToString());
				sockbuff = CMSMessage.GetMessageVideoBuffer(fpath, m_cmsMsgManager.DVR_Guid, msg.msg_index);
			}

			return data;
		}

		private async Task<List<string>> SendDataMessage(DVR_Msg msg, string Xml)
		{
			return await Task.Run(()=> {
				if (!killTasks)
				{
					string id = Convert.ToString((int)msg.dvr_cms_msg_id);
					var ret = converter.DVR_SendToWebAPI(Xml, id);
					return ret;
				}
				else
				{
					return null;

				}
			}, _cancellationTokenSource.Token);
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
				lock (lockThread)
				{
					conf.msg_id = Guid.NewGuid().ToString();
				}
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
				lock (lockThread)
				{
					conf.msg_id = Guid.NewGuid().ToString();
				}
				msglist.Add(conf);
			}

			//add config message
			for (int msgID = 0; msgID < CMSMessage.ConfigurationFiles.Length; msgID++)
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
					conf.msg_index = msgID;
					conf.m_wait_response = false;
					lock (lockThread)
					{
						conf.msg_id = Guid.NewGuid().ToString();
					}
					msglist.Add(conf);
				}

			}
			return msglist;
		}

		private List<DVR_Msg> GetVideoMessageList()
		{
			List<DVR_Msg> msglist = new List<DVR_Msg>();
			int total = 16;
			for (int i = 0; i < total; i++)
			{
				DVR_Msg conf = new DVR_Msg();
				conf.dvr_cms_msg_id = CMSMessage.DVR_CMS_MESSAGE_ID.MSG_DVR_GET_VIDEO_RESPONSE;
				conf.m_timeout_message = 0;
				conf.msg_index = i + 1; ;
				conf.m_wait_response = false;
				lock (lockThread)
				{
					conf.msg_id = Guid.NewGuid().ToString();
				}
				msglist.Add(conf);
			}
			return msglist;
		}

		private ServiceConfig NewServerConfig()
		{
			return new ServiceConfig() { ID = 1, Interval = 60, LogRecycle = 1, NumDVRMsg = 1, Url = cmsIP };
		}

		private MessageDVRInfo GetMsgDVRInfo()
		{
			List<MacInfo> MacAddressInfos = new List<MacInfo>();
			MacAddressInfos.Add(new MacInfo() { Active = true, IP_Address = dvrinfo.Server_IP, IP_Version = "InterNetwork", MAC_Address = GuiId, MacOrder = 1 });
			return new MessageDVRInfo
			{
				HASPK = dvrinfo.HaspKey,
				MACs = MacAddressInfos,
				Date = DateTime.Now,
				PACinfo = new PACInfo { PACID = null, PACVersion = null }
			};
		}
		#endregion

		private void cmsSetDelay_Click(object sender, EventArgs e)
		{
			SetupTimer obj = new SetupTimer(this.timers, cbDVRMode.Checked);
			if (obj.ShowDialog(this) == DialogResult.OK)
			{
				timers = obj.Setting;
				ApplyTimer2GUI(timers, cbDVRMode.Checked);
			}
		}

		private void cmdClear_Click(object sender, EventArgs e)
		{
			if (m_tableResult.Rows.Count > 0)
				m_tableResult.Rows.Clear();
		}
	}
}
