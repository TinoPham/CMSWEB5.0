using System.Collections.Generic;
using System.Threading;
using ConvertMessage;
using PACDMSimulator;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMSTest
{
	public partial class ConverterSimulators : Form
	{
		public TimerSetting TimerSetting = new TimerSetting();
		public int Indextab { get; set; }
		string m_startpath = string.Empty;
	
		ImageList il;

		public ConverterSimulators()
		{
			InitializeComponent();
		}

		private void cmdLoadDVR_Click(object sender, EventArgs e)
		{
			cmdLoadDVR.Enabled = false;
			List<Utils.DVR_Info> dvrList = DvrInfoList.ReadXml(m_startpath);
			if (dvrList.Count == 0)
			{
				MessageBox.Show("No DVRs to Load.", "Simulator"); return;
			}
			CreateDVRTabs(dvrList, dvrList.Count);
			
		}

		private void cmdAddDVR_Click(object sender, EventArgs e)
		{
			
			cmdAddDVR.Enabled = false;
			string cmsip = string.Empty;

			if (txtCmsLink.Text == null || !Utils.IsValidUri(txtCmsLink.Text))
			{
				MessageBox.Show("CMS Server link input wrong format.");
				cmdAddDVR.Enabled = true;
				return;
			}

			if (cboCMSList.SelectedItem == null)
			{
				cmsip = cboCMSList.Text;
				cboCMSList.Items.Add(cboCMSList.Text);
				cboCMSList.SelectedIndex = cboCMSList.FindString(cmsip);
			}
			else
			{
				cmsip = cboCMSList.SelectedItem.ToString();
			}

			int delaytime = Utils.DELAY_TIME_DISCONNECT_DVR;
			try
			{
				delaytime = Convert.ToInt32(txtconnectDelay.Text);
			}
			catch (System.Exception ex)
			{
				delaytime = Utils.DELAY_TIME_DISCONNECT_DVR;
			}
			//string[] dvrguids = null;
			//string[] dvrips = null;
			//bool[] dvrmodes = null;
			if (!CheckDelayvalue(true))
			{
				cmdAddDVR.Enabled = true;
				return;
			}

			//Utils.GetDVRInfo(cbDVRMode.Checked, ref dvrmodes, ref dvrguids, ref dvrips, Convert.ToInt32(txtDVRNum.Text));
			List<Utils.DVR_Info> dvrList = DvrInfoList.GetDVRInfo(cbDVRMode.Checked, "100-011", Convert.ToInt32(txtCMSPort.Text),
				txtlocation.Text, "Model", "SRX-Pro Server", cboVersion.Text,txtCmsLink.Text,  Convert.ToInt32(txtDVRNum.Text));

			CreateDVRTabs(dvrList, Convert.ToInt32(txtDVRNum.Text));
		}


		private void SetPropressBar(int total)
		{
			if (InvokeRequired)
			{
				BeginInvoke((Action<int>) SetPropressBar, new object[]{total});
			}
			else
			{
				toolStripProgressBar1.Visible = true;
				toolStripProgressBar1.Minimum = 0;
				toolStripProgressBar1.Maximum = total;
				toolStripProgressBar1.Step = 1;
				toolStripProgressBar1.Value = 0;
			}
		}

		private async void CreateDVRTabs(List<Utils.DVR_Info> dvrs, int total)
		{
			bool isCanncel = false;
			ProgressBarFrm prB = new ProgressBarFrm(total);
			prB.StartPosition = FormStartPosition.Manual;
			prB.Location = new Point(this.Location.X + (this.Width / 2) - prB.Width / 2, this.Location.Y + this.Height / 2 - prB.Height / 2);
			prB.Show(this);
			prB.FormClosing += (e,s) =>
			{
				isCanncel = true;
			};

			SetPropressBar(total);
			//string cmsip = string.Empty;

			//string[] dvrguids = null;
			//string[] dvrips = null;
			//bool[] dvrmodes = null;

			//Utils.GetDVRInfo(cbDVRMode.Checked, ref dvrmodes, ref dvrguids, ref dvrips, Convert.ToInt32(txtDVRNum.Text));
			//cmsip = txtCmsLink.Text;
			//int len = dvrguids.Length;
			this.Enabled = false;
	

			foreach (var dvr in dvrs)
			{
				if(isCanncel)
					break;
				//toolStripProgressBar1.Visible = true;
				Utils.DVR_Info dvrinfo = dvr;
				dvrinfo.Id = Indextab;
				DVRTabPages dvrtab = await CreateDVRTab(m_startpath, TimerSetting, dvrinfo, true);
				dvrtab.DVRAction += dvrtab_DVRAction;
				tabDVRList.TabPages.Add(dvrtab);
				//this.PerformLayout();
				dvrtab.AutoScroll = true;
				dvrtab.AddControl();
				//AddTabTabView(dvrtab);
				if (cbAutoConnect.Checked)
				{
					tabDVRList.SelectedTab = dvrtab;
					dvrtab.StartDVR();
				}
				Indextab++;
				toolStripProgressBar1.PerformStep();
				prB.SetProgress();
				//toolStripProgressBar1.Visible = false;
				AddItemToListView(dvrtab.Text);
				toolStripStatusLabel1.Text = "Total number of DVRs:" + tabDVRList.TabPages.Count.ToString();
				Thread.Sleep(20);
				//dvrtab_DVRAction(null, null);
			}
			toolStripProgressBar1.Visible = false;
			cmdAddDVR.Enabled = true;
			this.Enabled = true;
			this.Activate();
		}

		void dvrtab_DVRAction(object sender, DVRSimulatorEventArgs e)
		{
			toolStripStatusLabel1.Text = "Total number of DVRs:" + tabDVRList.TabPages.Count.ToString();
			refeshListView();
		}

		private async Task<DVRTabPages> CreateDVRTab(string m_startpath, TimerSetting timersetting, Utils.DVR_Info dvrInfo, bool autoConnct)
		{
			return await Task.Run(() =>
			{
				int iTotalRow = Convert.ToInt32(numberRows.Text);
				bool bKeepXml = keepXml.Checked;
				DVRTabPages dvrTab = new DVRTabPages(m_startpath, timersetting, dvrInfo, iTotalRow, bKeepXml, autoConnct, this);
				return dvrTab;
			});
		}

		private bool CheckDelayvalue(bool addnewdvr)
		{
			if (addnewdvr)
			{
				if (!Utils.IsInt32(txtDVRNum.Text))
				{
					MessageBox.Show("Invalid DVR number.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
				if (Convert.ToInt32(txtDVRNum.Text) < 0)
				{
					MessageBox.Show("DVR number must be numeric.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
			}
			return true;
		}

		private void cmdSettingtimer_Click(object sender, EventArgs e)
		{
			SetupTimer obj = new SetupTimer(TimerSetting, true);
			if (obj.ShowDialog(this) == DialogResult.OK)
			{
				TimerSetting = obj.Setting;
			}
			ApplyTimer2GUI(TimerSetting);
		}

		private void ConverterSimulators_Load(object sender, EventArgs e)
		{
			m_startpath = Application.StartupPath;
			TimerSetting = Utils.DVRTimerSettings();
			ApplyTimer2GUI(TimerSetting);
			//LoadCMSList();
			//UpdateDVRTotal();

			string[] sVerList = GetVersionList();
			foreach (string sVer in sVerList)
			{
				cboVersion.Items.Add(sVer);
			}
			cboVersion.Text = sVerList[0];

			toolStripProgressBar1.Visible = false;
			loadListView();
			setContext(3);
		}

		private void loadListView()
		{
			il = new ImageList();
			il.Images.Add("logo", Image.FromFile(m_startpath + "\\VideoData\\logo.png"));

			listView1.LargeImageList = il;
			listView1.SmallImageList = il;
			listView1.View = View.Details;
			listView1.AllowColumnReorder = true;
			listView1.FullRowSelect = true;
			// Display grid lines.
			listView1.GridLines = true;
			// Sort the items in the list in ascending order.
			listView1.Sorting = SortOrder.Ascending;
			listView1.Columns.Add("DVR List", -2, HorizontalAlignment.Left);
		}

		private void AddItemToList(string dvrName)
		{
			var lvi = new ListViewItem { ImageIndex = 0, Text = dvrName };
			listView1.Items.Add(lvi);
		}

		private async void AddItemToListView(string dvrName)
		{
			await Task.Run(() =>
			{
				var lvi = new ListViewItem {ImageIndex = 0, Text = dvrName};
				AddlistView(lvi); //listView1.Items.Add(lvi);
			});
		}

		private void AddlistView(ListViewItem lvi)
		{
			if (InvokeRequired)
			{
				BeginInvoke((Action<ListViewItem>) AddlistView, new object[] {lvi});
			}
			else
			{
				listView1.Items.Add(lvi);
			}
		}

		private void ApplyTimer2GUI(TimerSetting tmsetting)
		{
			lstTimer.Items.Clear();
			//string stritem = string.Format("Message time: {0} min(s)", tmsetting.m_msg_timeout);
			//lstTimer.Items.Add(stritem);

			//stritem = string.Format("Message retry: {0} min(s)", tmsetting.m_msg_retry);
			//lstTimer.Items.Add(stritem);

			string stritem = string.Format("Reconnect time: {0} min(s)", tmsetting.m_dvr_reconnect);
			lstTimer.Items.Add(stritem);

			stritem = string.Format("Keep alive: {0} second(s)", tmsetting.m_keep_alive);
			lstTimer.Items.Add(stritem);


			stritem = string.Format("Alert time: {0} min(s)", tmsetting.m_msg_alert);
			lstTimer.Items.Add(stritem);

			stritem = string.Format("Configuration time: {0} min(s)", tmsetting.m_msg_configuration);
			lstTimer.Items.Add(stritem);

			stritem = string.Format("Video time: {0} min(s)", tmsetting.m_msg_video);
			lstTimer.Items.Add(stritem);

		}

		private string[] GetVersionList()
		{
			string[] sDirList = Directory.GetDirectories(m_startpath + "\\XMLData");
			string[] sVerList = new string[sDirList.Length];
			int i = 0;
			foreach (string sDir in sDirList)
			{
				int idx = sDir.LastIndexOf("\\");
				string sStr = sDir.Substring(idx + 1);
				sVerList[i] = GetVersionName(sStr);
				i++;
			}
			return sVerList;
		}

		private string GetVersionName(string sDirName)
		{
			string sVersion = "";
			switch (sDirName)
			{
				case "2.0.0":
					sVersion = "2.0.0 (Simulator)";
					break;
				case "2.1.3":
					sVersion = "2.1.3 (Simulator)";
					break;
				case "2.2.0":
					sVersion = "2.2.0 (Simulator)";
					break;
				case "2.3.0":
					sVersion = "2.3.0 (Simulator)";
					break;
				case "3.2.0":
					sVersion = "3.2.0 (Simulator)";
					break;
				case "1.600":
				default:
					sVersion = "1.600 (Simulator)";
					break;
			}
			return sVersion;
		}

		private void listView1_MouseClick(object sender, MouseEventArgs e)
		{
			int flag = 0;
			ListView listView = sender as ListView;
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				ListViewItem item = listView.GetItemAt(e.X, e.Y);
				flag = 0;
				if (item != null)
				{
					flag = 0;
					item.Selected = true;
					if (listView.SelectedItems.Count > 1)
					{
						flag = 1;
					}
					contextMenuStrip1.Show(listView, e.Location);
				}
			}

			setContext(flag);
		}

		private void setContext(int Items)
		{
			if (this.tabDVRList.TabCount <= 0)
			{
				Items = -1;
			}

			switch (Items)
			{
				case 0: //one Item
					{
						startedToolStripMenuItem.Visible = true;
						stopToolStripMenuItem.Visible = true;
						disconnectToolStripMenuItem.Visible = true;
						closeToolStripMenuItem.Visible = true;
						toolStripSeparator1.Visible = true;
						startSelectedDVRsToolStripMenuItem.Visible = false;
						stopSelectedDVRsToolStripMenuItem.Visible = false;
						disconnectSelectedDVRsToolStripMenuItem.Visible = false;
						closeSelectedDVRsToolStripMenuItem.Visible = false;
						toolStripSeparator2.Visible = false;
						startAllDVRsToolStripMenuItem.Enabled = true;
						stopAllDVRsToolStripMenuItem.Enabled = true;
						disconnectAllDVRsToolStripMenuItem.Enabled = true;
						closeAllDVRsToolStripMenuItem.Enabled = true;	
						break;
					}
				case 1: //ITems
					{
						startedToolStripMenuItem.Visible = false;
						stopToolStripMenuItem.Visible = false;
						disconnectToolStripMenuItem.Visible = false;
						closeToolStripMenuItem.Visible = false;
						toolStripSeparator1.Visible = false;
						startSelectedDVRsToolStripMenuItem.Visible = true;
						stopSelectedDVRsToolStripMenuItem.Visible = true;
						disconnectSelectedDVRsToolStripMenuItem.Visible = true;
						closeSelectedDVRsToolStripMenuItem.Visible = true;
						toolStripSeparator2.Visible = true;
						startAllDVRsToolStripMenuItem.Enabled = true;
						stopAllDVRsToolStripMenuItem.Enabled = true;
						disconnectAllDVRsToolStripMenuItem.Enabled = true;
						closeAllDVRsToolStripMenuItem.Enabled = true;	
						break;
					}
					
				default: //no Items
					{
						startedToolStripMenuItem.Visible = false;
						stopToolStripMenuItem.Visible = false;
						disconnectToolStripMenuItem.Visible = false;
						closeToolStripMenuItem.Visible = false;
						toolStripSeparator1.Visible = false;
						startSelectedDVRsToolStripMenuItem.Visible = false;
						stopSelectedDVRsToolStripMenuItem.Visible = false;
						disconnectSelectedDVRsToolStripMenuItem.Visible = false;
						closeSelectedDVRsToolStripMenuItem.Visible = false;
						toolStripSeparator2.Visible = false;
						startAllDVRsToolStripMenuItem.Enabled = false;
						stopAllDVRsToolStripMenuItem.Enabled = false;
						disconnectAllDVRsToolStripMenuItem.Enabled = false;
						closeAllDVRsToolStripMenuItem.Enabled = false;	
						break;
					}
			}
		}

		private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			setContext(3);
		}

		private void startedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem items in listView1.SelectedItems)
			{
				var dvrtab = SearchDVR(items.Text);
				if (dvrtab != null)
				{
					dvrtab.StartDVR();
				}
			}
		}

		private DVRTabPages SearchDVR(string DVRName)
		{
			DVRTabPages dvrTab = null;
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages tabs = tab as DVRTabPages;
				if (tabs.Text == DVRName)
				{
					dvrTab = tabs;
					break;
				}
			}
			return dvrTab;
		}

		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			stopSelectedDVRsToolStripMenuItem_Click(sender, e);
		}

		private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			disconnectSelectedDVRsToolStripMenuItem_Click(sender, e);
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			closeSelectedDVRsToolStripMenuItem_Click(sender, e);
		}

		private void startSelectedDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem items in listView1.SelectedItems)
			{
				var dvrtab = SearchDVR(items.Text);
				if (dvrtab != null)
				{
					System.Threading.Thread.Sleep(10);
					dvrtab.StartDVR();
				}
			}
			//List<ListViewItem> list = new List<ListViewItem>();
			//foreach (ListViewItem item in listView1.SelectedItems)
			//{
			//	list.Add(item);
			//}

			//Parallel.ForEach(list, items => 
			//{
			//	var dvrtab = SearchDVR(items.Text);
			//	if (dvrtab != null)
			//	{
			//		dvrtab.StartDVR();
			//	}
			//});
		}

		private void stopSelectedDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem items in listView1.SelectedItems)
			{
				var dvrtab = SearchDVR(items.Text);
				if (dvrtab != null)
				{
					System.Threading.Thread.Sleep(10);
					dvrtab.StopDVR();
				}
			}
		}

		private void disconnectSelectedDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem items in listView1.SelectedItems)
			{
				var dvrtab = SearchDVR(items.Text);
				if (dvrtab != null)
				{
					System.Threading.Thread.Sleep(10);
					dvrtab.DisconnectDVR();
				}
			}
		}

		private void closeSelectedDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool isCanncel = false;
			ProgressBarFrm prB = new ProgressBarFrm(listView1.SelectedItems.Count, listView1.SelectedItems.Count, isRemove: true);
			prB.StartPosition = FormStartPosition.Manual;
			prB.Location = new Point(this.Location.X + (this.Width / 2) - prB.Width / 2, this.Location.Y + this.Height / 2 - prB.Height / 2);
			prB.Show(this);
			prB.FormClosing += (d, s) =>
			{
				isCanncel = true;
			};
			this.Enabled = false;
			foreach (ListViewItem items in listView1.SelectedItems)
			{
				if(isCanncel) return;
				var dvrtab = SearchDVR(items.Text);
				if (dvrtab != null)
				{
					System.Threading.Thread.Sleep(10);
					tabDVRList.TabPages.Remove(dvrtab);
					dvrtab.CloseDVR();
					prB.SetProgress();
				}
				listView1.Items.Remove(items);
			}
			this.Enabled = true;
			this.Activate();
			//refeshListView();
		}

		private void startAllDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages dvrTab = tab as DVRTabPages;
				if (dvrTab != null)
				{
					dvrTab.StartDVR();
				}
			}
		}

		private void stopAllDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages dvrTab = tab as DVRTabPages;
				if (dvrTab != null)
				{
					dvrTab.StopDVR();
				}
			}
		}

		private void disconnectAllDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages dvrTab = tab as DVRTabPages;
				if (dvrTab != null)
				{
					dvrTab.DisconnectDVR();
				}
			}
		}

		private void closeAllDVRsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool isCanncel = false;
			ProgressBarFrm prB = new ProgressBarFrm(tabDVRList.TabPages.Count, tabDVRList.TabPages.Count, isRemove: true);
			prB.StartPosition = FormStartPosition.Manual;
			prB.Location = new Point(this.Location.X + (this.Width/2) - prB.Width/2,
				this.Location.Y + this.Height/2 - prB.Height/2);
			prB.Show(this);
			prB.FormClosing += (d, s) =>
			{
				isCanncel = true;
			};
			this.Enabled = false;

			foreach (var tab in tabDVRList.TabPages)
			{
				if (isCanncel) return;
				DVRTabPages dvrTab = tab as DVRTabPages;
				if (dvrTab != null)
				{
					tabDVRList.TabPages.Remove(dvrTab);
					dvrTab.CloseDVR();
					prB.SetProgress();
					Thread.Sleep(20);
				}
			}
			refeshListView();
			this.Enabled = true;
			//this.Activate();
		}

		private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListView listView = sender as ListView;
			ListViewItem item = listView.GetItemAt(e.X, e.Y);
			if (item != null)
			{
				item.Selected = true;
				var dvrtab = SearchDVR(item.Text);
				if (dvrtab != null)
				{
					this.tabDVRList.SelectTab(dvrtab);
				}
			}
		}

		private void refeshListView()
		{
			listView1.Items.Clear();
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages tabs = tab as DVRTabPages;
				AddItemToList(tabs.Text);
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			List<Utils.DVR_Info> dvrs = new List<Utils.DVR_Info>();
			foreach (var tab in tabDVRList.TabPages)
			{
				DVRTabPages tabs = tab as DVRTabPages;
				dvrs.Add(tabs.GetDvrInfo());
			}

			if(dvrs.Count == 0)
			{
				MessageBox.Show("No DVRs to Save.", "Simulator"); return;}

			DvrInfoList.WriteXml(dvrs, m_startpath);
		}

		private void ConverterSimulators_FormClosing(object sender, FormClosingEventArgs e)
		{
			closeAllDVRsToolStripMenuItem_Click(null, null);
		}
	}

	public class DVRSimulatorEventArgs: EventArgs
	{
		public DVRSimulatorEventArgs(int states, int value)
		{
			State = states;
			Value = value;
		}

		public int State {get;set;}
		public int Value {get;set;}
	}

	public class DVRTabPages : System.Windows.Forms.TabPage
	{
		public event EventHandler<DVRSimulatorEventArgs> DVRAction;
		//public ConverterDVRSimulator Wconverter;
		string m_dvrguid;
		string m_cmsIP;
		string m_dvrIP;
		public string Startpath;
		public string CMSIP
		{
			get { return m_cmsIP; }
		}
		public string DVRGuid
		{
			get { return m_dvrguid; }
		}
		public string DVRIP
		{
			get { return m_dvrIP; }
		}
		DVRSimulator dvr;

		public string TabID
		{
			get { return m_dvrguid; }
		}
		public DVRTabPages(string startpath, TimerSetting timersetting, Utils.DVR_Info dvrinfo, int iToTalRow, bool bKeepXml, bool autoConnect, ConverterSimulators converter)
		{
			string dvrname = dvrinfo.Server_Version + "#"+(dvrinfo.Id).ToString("0000");
			this.Disposed += DVRTabPages_Disposed;
			m_dvrIP = dvrinfo.Server_IP;
			m_cmsIP = dvrinfo.ServerUrl;
			this.Text = dvrname;
			m_dvrguid = dvrinfo.Mac;
			this.Startpath = startpath;
			//this.Wconverter = converter.Wconverter;
			dvr = new DVRSimulator(timersetting, dvrinfo, iToTalRow, bKeepXml, this);
		}

		void DVRTabPages_Disposed(object sender, EventArgs e)
		{
			//converter.Indextab--;
		}

		public void RiseDVRAction(int states, int value)
		{
			OnDVRAction(states, value);
		}

		protected virtual void OnDVRAction(int states, int value)
		{
			var del = DVRAction as EventHandler<DVRSimulatorEventArgs>;
			if (del != null)
			{
				del(this, new DVRSimulatorEventArgs(states, value));
			}
		}

		public Utils.DVR_Info GetDvrInfo()
		{
			return dvr.GetDvrInfo();
		}

		public void AddControl()
		{
			if (dvr == null)
				return;
			dvr.Location = new System.Drawing.Point(0, 0);
			dvr.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Controls.Add(dvr);
		}

		public void StartDVR()
		{
			if (dvr != null)
				dvr.StartDVR();
			//await Task.Run(() =>
			//{
			//	if (dvr != null)
			//		dvr.StartDVR();
			//});
		}
		public void StopDVR()
		{
			if (dvr != null)
				dvr.StopClient();
		}

		public void DisconnectDVR()
		{
			if (dvr != null)
				dvr.DisconnectClient();
		}

		public void CloseDVR()
		{
			if (dvr != null)
				dvr.CloseClient();
		}

		public void DeleteDVR()
		{
			if (dvr != null)
			{
				dvr.RemoveDVR();
				dvr = null;
			}
			this.Dispose();
		}

		public void ChangeTabCaption(string newcaption)
		{
			this.Text = newcaption;
		}

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

	public enum SOCKET_ERROR : int
	{
		INVALID_SOCKET = 0,
		CONNECTED_FAIL,
		CONECT_SUCCESSFUL,
		SEND_FAILED,
		RECEIVE_FAILED,
		SEND_SUCCESSFUL,

	}
}
