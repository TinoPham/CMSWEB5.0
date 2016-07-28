using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using PACDMSimulator;
namespace CMSTest
{
	public partial class frmMain : Form
	{
		private const string STR_TABLE_SITE_IP = "CMSIP";
		private const string STR_TABLE_DVR_ID = "DVRID";
		private const string STR_SITE_ID = "SiteID";
		private const string STR_DVRID = "DVR_ID";
		private const string STR_DVR_IP = "DVR_IP";
        private const string STR_DVR_MODE = "DVR_MODE";
		private const string STR_FILENAME_SITE_LIST = "Siteinfo.lst";
		private const string STR_DVR_SITES = "DVRSites";
		private const string STR_DVR_LIST = "DVR List";
		private const string STR_DVR_LIST_NODE_NAME = "DVRList";
		private delegate void UpdateTreenodeText(Utils.DVR_Info dvrinfo, string cmsIP, int cmsport, string dvrguid,string version, Utils.DVR_Status dvrstatus);
		private delegate void AddDVR(bool autoconnect, bool updatesitelist, CMSTest.DVRNode dvrnode, CMSTest.DVRTabPage dvrtab);
		private delegate void CloseLoading( IntPtr LoadingHnd);
		private delegate void StartDVR(int dvrindex, bool stop, bool remove, ref bool waitstop);
		bool mstop = false;
		bool m_closing = false;
		ConverterSimulators Converter;
		Thread m_threadLoad = null;
		ManualResetEvent m_wait_complete;
		DataSet m_dsSiteList;
		string m_startpath = string.Empty;
        UCtrlDVR.TimerSetting m_TimerSetting = new UCtrlDVR.TimerSetting();
        public frmMain()
        {
			InitializeComponent();
			Converter = new ConverterSimulators();

            m_startpath = Application.StartupPath;
            tabDVRList.TabPages.Clear();
            TreeDVR.ShowNodeToolTips = cbShowinfo.Checked;
            if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
            {
                TreeNode root = new TreeNode(STR_DVR_LIST);
                root.Name = STR_DVR_LIST_NODE_NAME;
                TreeDVR.Nodes.Add(root);
            }
            m_TimerSetting = Utils.DVRTimerSetting();
            ApplyTimer2GUI(m_TimerSetting);
            TreeDVR.ExpandAll();
            m_wait_complete = new ManualResetEvent(false);
            m_wait_complete.Set();
            LoadCMSList();
            UpdateDVRTotal();

            string[] sVerList = GetVersionList();
            foreach (string sVer in sVerList)
            {
                cboVersion.Items.Add(sVer);
            }
            cboVersion.Text = sVerList[0];
        }

		//public List<Socket> NewListSocket()
		//{
		//	List<Socket> lst = new List<Socket>();
		//	for (int i = 1; i < 10; i++)
		//	{

		//		byte[] bytes = new Byte[1024];

		//		// Establish the local endpoint for the socket.
		//		// The DNS name of the computer
		//		// running the listener is "host.contoso.com".
		//		IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
		//		IPAddress ipAddress = Dns.GetHostEntry("10.0.1.14").AddressList[0];
		//		//IPAddress ipAddress = ipHostInfo.AddressList[0];
		//		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1000+i);

		//		// Create a TCP/IP socket.
		//		Socket listener = new Socket(AddressFamily.InterNetwork,
		//			SocketType.Stream, ProtocolType.Tcp);

		//		// Bind the socket to the local endpoint and listen for incoming connections.
		//		try
		//		{
		//			listener.Bind(localEndPoint);
		//			listener.Listen(100);
		//			lst.Add(listener);

		//		}
		//		catch (Exception e)
		//		{
		//			Console.WriteLine(e.ToString());
		//		}
		//	}
		//	return lst;
		//}

		private void ThreadStartConnectDVR(IntPtr loadinghnd, int startindex, int endindex, bool stop, bool remove, int delaytime)
		{
			m_wait_complete.Reset();
			delaytime = delaytime * 1000;
			bool waitstop = false;
			int countsleep = 0;
			int sleep_time = 100;
			for (int i = 0; i < endindex; i++ )
			{
				if (mstop)
					break;
				WIN32API.SendMessage(loadinghnd, frmLoading.MSG_UPDATE_STATUS_ID, new IntPtr(endindex - startindex), new IntPtr(1));
				System.Threading.Thread.Sleep(100);
				if (!remove)
					OnStartDVR(i, stop, remove, ref waitstop);
				else
					OnStartDVR(0, stop, remove, ref waitstop);
				
				if (!stop)
				{
					while (countsleep < delaytime && !mstop)
					{
						System.Threading.Thread.Sleep(sleep_time);
						countsleep += sleep_time;
					}
					countsleep = 0;	
				}
				else
				{
					if (waitstop)
						System.Threading.Thread.Sleep(1000);
				}
			}
			m_wait_complete.Set();
			OnCloseLoading(loadinghnd);
		}
		private void ThreadstartLoadDVR(string startpath,UCtrlDVR.TimerSetting timersetting, int delaytime,IntPtr hnd, bool autoconnect, bool updatelist, int current, string cmsip, int cmsport,bool[] dvrmodes,string[]dvrIDs, string[] dvrips,string dvrversion, string dvrlocation)
		{
			if (dvrIDs == null || dvrIDs.Length == 0)
				return;
			m_wait_complete.Reset();
			delaytime = delaytime * 1000;
			string dvrname = dvrversion + "#" ;
			Utils.DVR_Info dvrinfo = new Utils.DVR_Info();
			int len = dvrIDs.Length;
			int countsleep = 0;
			int sleep_time = 100;
			for (int i = 0; i < len; i++ )
			{
				if (mstop)
					break;
				dvrinfo = new Utils.DVR_Info();
				dvrinfo.Id = current;
				dvrinfo.DVRPort = cmsport;
				dvrinfo.Server_Location = dvrlocation;
				dvrinfo.Server_Version = dvrversion;
				dvrinfo.Server_IP = dvrips[i];
                dvrinfo.Server_standard = dvrmodes[i];
                dvrinfo.Server_ID = dvrIDs[i];
                dvrinfo.Server_Name = dvrname + (current).ToString("00");
                dvrinfo.Server_Model = "";
				DVRNode dvr = new DVRNode(dvrinfo, cmsip, cmsport, current, dvrname + (current).ToString("00"), dvrIDs[i]);

                int iTotalRow = Convert.ToInt32(numberRows.Text);
                bool bKeepXml = keepXml.Checked;
                DVRTabPage dvrtab = new DVRTabPage(startpath, dvrmodes[i], timersetting, dvrname + (current).ToString("00"), dvrIDs[i], dvrinfo, cmsip, cmsport + current, iTotalRow, bKeepXml);
				OnAddDVR(autoconnect, updatelist, dvr, dvrtab);
				WIN32API.SendMessage(hnd, frmLoading.MSG_UPDATE_STATUS_ID, new IntPtr(len), new IntPtr(1));
				if (autoconnect)
				{
					while (countsleep < delaytime && !mstop)
					{
						System.Threading.Thread.Sleep(sleep_time);
						countsleep += sleep_time;
					}
					countsleep = 0;	
				}
				else
					System.Threading.Thread.Sleep(100);
				current++;
			}
			if(updatelist)
				SaveConfigFile(); ;
			m_wait_complete.Set();
			OnCloseLoading(hnd);
		}
		
		private void OnStartDVR(int dvrindex, bool stop, bool remove, ref bool waitstop)
		{
			if( this.InvokeRequired)
			{
				this.Invoke(new StartDVR(OnStartDVR), new object[] { dvrindex, stop, remove, waitstop });
			}
			else
			{
				if (tabDVRList.TabPages == null && tabDVRList.TabPages.Count <= 0)
					return;
				if (!stop)
				{
					(tabDVRList.TabPages[dvrindex] as CMSTest.DVRTabPage).StartDVR();
					tabDVRList.SelectedTab = (tabDVRList.TabPages[dvrindex] as CMSTest.DVRTabPage);
				}
				else
				{
					waitstop = (tabDVRList.TabPages[dvrindex] as CMSTest.DVRTabPage).StopDVR();
					if( remove)
						(tabDVRList.TabPages[dvrindex] as CMSTest.DVRTabPage).DeleteDVR();
				}
	
			}
		}
		private void OnAddDVR(bool autoconnect, bool updatesitelist,CMSTest.DVRNode dvrnode, CMSTest.DVRTabPage dvrtab)
		{
			if (this.InvokeRequired)
				this.Invoke(new AddDVR(OnAddDVR), new object[] {autoconnect, updatesitelist, dvrnode, dvrtab });
			else
			{
				TreeNode root = null;
				if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
				{
					root = new TreeNode(STR_DVR_LIST);
					root.Name = STR_DVR_LIST_NODE_NAME;
					TreeDVR.Nodes.Add(root);
				}
				else
					root = TreeDVR.Nodes[0];

				root.Nodes.Add(dvrnode);
				tabDVRList.TabPages.Add(dvrtab);
				this.PerformLayout();
				System.Threading.Thread.Sleep(10);
				dvrtab.AutoScroll = true;
				dvrtab.AddControl();
				dvrtab.OnSocketEvents += new UCtrlDVR.SocketEvents(dvrtab_OnSocketEvents);
				dvrtab.OnCloseDVR += new UCtrlDVR.CloseDVR(dvrtab_OnCloseDVR);
				UpdateDVRTotal();
				if (autoconnect)
				{
					tabDVRList.SelectedTab = dvrtab;
					dvrtab.StartDVR();
				}
				if (updatesitelist)
					AddNewSite(dvrtab.CMSIP,dvrnode.DVRInfo.Server_standard, dvrtab.DVRGuid, dvrtab.DVRIP);
			}
		}
		private void StartThread(UCtrlDVR.TimerSetting timersetting,int delaytime, bool autoconnect, bool updatelist, string cmsip, int cmsport,bool[] dvrmodes, string[] dvrIDs, string[] dvrips, string dvrversion, string dvrlocation)
		{
			;//Luan Converter.StartConverter();

			mstop = false;
			m_closing = false;
			if (m_wait_complete == null)
			{
				m_wait_complete = new ManualResetEvent(false);
				m_wait_complete.Set();
			}

			TreeNode root = null;
			if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
			{
				root = new TreeNode(STR_DVR_LIST);
				root.Name = STR_DVR_LIST_NODE_NAME;
				TreeDVR.Nodes.Add(root);
			}
			else
				root = TreeDVR.Nodes[0];
			int current = root.Nodes.Count;
			if (current >= 1)
			{
				DVRNode lstnode = (DVRNode)root.Nodes[current - 1];
				current = lstnode.NodeID;
			}
			ManualResetEvent m_startevent = new ManualResetEvent(false);
			frmLoading objload = new frmLoading(this.Handle,m_startevent, "Loading", dvrIDs.Length);
			objload.StartPosition = FormStartPosition.Manual;
			objload.Location = new Point(this.Location.X + (this.Width / 2) - objload.Width / 2, this.Location.Y + this.Height / 2 - objload.Height / 2);
			objload.Owner = this;
			objload.Show(this);
			IntPtr hnd = objload.Handle;
			this.Enabled = false;
			m_startevent.WaitOne();
			System.Threading.Thread.Sleep(100);
			ThreadStart tstart = delegate { ThreadstartLoadDVR(m_startpath,timersetting ,delaytime, hnd, autoconnect, updatelist, current, cmsip, cmsport,dvrmodes, dvrIDs, dvrips, dvrversion, dvrlocation); };
			m_threadLoad = new Thread(tstart,Utils.MAX_STACK_SIZE);
			m_threadLoad.SetApartmentState(ApartmentState.STA);
			m_threadLoad.Start();
			m_startevent.Close();
			m_startevent = null;
		}
		private void OnCloseLoading(IntPtr hnd)
		{
			if (this.InvokeRequired)
				this.BeginInvoke(new CloseLoading(OnCloseLoading), new object[] { hnd });
			else
			{
				WIN32API.SendMessage(hnd, 0x10, IntPtr.Zero, IntPtr.Zero);
				this.Enabled = true;
				this.Activate();
				if( !mstop && m_closing )
					this.Close();
			}
			
		}
		private void StartThreadConnect(int indexstart, int indexend, bool stop,bool remove, int delaytime)
		{
			mstop = false;
			m_closing = false;
			if (m_wait_complete == null)
			{
				m_wait_complete = new ManualResetEvent(false);
				m_wait_complete.Set();
			}

			TreeNode root = null;
			if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
			{
				root = new TreeNode(STR_DVR_LIST);
				root.Name = STR_DVR_LIST_NODE_NAME;
				TreeDVR.Nodes.Add(root);
			}
			else
				root = TreeDVR.Nodes[0];
			int current = root.Nodes.Count;
			if (current >= 1)
			{
				DVRNode lstnode = (DVRNode)root.Nodes[current - 1];
				current = lstnode.NodeID;
			}
			string strstatus = (stop == false) ? "Starting":"Closing";
			ManualResetEvent m_startevent = new ManualResetEvent(false);
			frmLoading objload = new frmLoading(this.Handle, m_startevent, strstatus, indexend - indexstart);

			objload.StartPosition = FormStartPosition.Manual;
			objload.Location = new Point(this.Location.X + (this.Width / 2) - objload.Width / 2, this.Location.Y + this.Height / 2 - objload.Height / 2);
			objload.Owner = this;
			objload.Show(this);
			m_startevent.WaitOne();
			IntPtr hnd = objload.Handle;
			this.Enabled = false;
			System.Threading.Thread.Sleep(100);
			ThreadStart tstart = delegate { ThreadStartConnectDVR(hnd, indexstart, indexend, stop,remove, delaytime); };
			m_threadLoad = new Thread(tstart, Utils.MAX_STACK_SIZE);
			m_threadLoad.SetApartmentState(ApartmentState.STA);
			m_threadLoad.Start();
			m_startevent.Close();
			m_startevent = null;
		}
		#region mark
		/*
		private void AddDVR2Tree(bool updatesiltelist, bool saveconfig, string dvrid, int numberDVR, string servername, string cmsIP, int cmsport, int delaymsg)
		{
			TreeNode root = null;
			if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
			{
				root = new TreeNode(STR_DVR_LIST);
				root.Name = STR_DVR_LIST_NODE_NAME;
				TreeDVR.Nodes.Add(root);
			}
			else
				root = TreeDVR.Nodes[0];
			int count = root.Nodes.Count;
			if( count >=1)
			{
				DVRNode lstnode = (DVRNode)root.Nodes[count - 1];
				count = lstnode.NodeID;
			}
			
			string sname = servername + "#";
			Utils.DVR_Info dvrinfo = new Utils.DVR_Info();
			string dvrguid = string.Empty;
			for (int i = 0; i < numberDVR; i++ )
			{
				if (dvrid == null || dvrid.Length == 0)
					dvrguid = Utils.GetRandomMacAddress();
				else
					dvrguid = dvrid;
				count++;
			
				dvrinfo = new Utils.DVR_Info();
				dvrinfo.Server_Location = txtlocation.Text;
				dvrinfo.Server_Version = txtVersion.Text;
				dvrinfo.Server_IP = Utils.RandomIP();
				DVRNode dvr = new DVRNode(dvrinfo,cmsIP, cmsport, count, sname + (count).ToString(), dvrguid);
				root.Nodes.Add(dvr);

				DVRTabPage dvrtab = new DVRTabPage(delaymsg, sname + (count).ToString(), dvrguid, dvrinfo, cmsIP, cmsport);
				tabDVRList.TabPages.Add(dvrtab);
				dvrtab.OnSocketEvents += new UCtrlDVR.SocketEvents(dvrtab_OnSocketEvents);
				dvrtab.OnCloseDVR += new UCtrlDVR.CloseDVR(dvrtab_OnCloseDVR);
				if (cbAutoConnect.Checked)
					dvrtab.StartDVR();
				if( updatesiltelist)
					AddNewSite(dvrtab.CMSIP, dvrtab.DVRGuid, dvrtab.DVRIP);
				System.Threading.Thread.Sleep(100);
			}
			UpdateDVRTotal();
			if( saveconfig)
				SaveConfigFile();
		}
		 */
		#endregion
		private void SaveConfigFile()
		{
			try
			{
				string apppath = Application.StartupPath;
				if (!apppath.EndsWith("\\"))
					apppath += "\\" + STR_DVR_SITES;
				if (!Directory.Exists(apppath))
					Directory.CreateDirectory(apppath);
				string fpath = apppath + "\\" + STR_FILENAME_SITE_LIST;
				if (m_dsSiteList != null)
					m_dsSiteList.WriteXml(fpath);
			}
			catch (System.Exception)
			{

			}
		}
		private void AddNewSite(string siteip,bool dvrstandard, string dvrID, string dvrIP)
		{
			DataRow[] rows = m_dsSiteList.Tables[STR_TABLE_SITE_IP].Select(STR_SITE_ID + "='" + siteip + "'");
			if( rows == null || rows.Length == 0)
			{
				DataRow r = m_dsSiteList.Tables[STR_TABLE_SITE_IP].NewRow();
				r[ STR_SITE_ID] = siteip;
				m_dsSiteList.Tables[STR_TABLE_SITE_IP].Rows.Add( r);
				m_dsSiteList.Tables[STR_TABLE_SITE_IP].AcceptChanges();
			}
			rows = m_dsSiteList.Tables[STR_TABLE_DVR_ID].Select(STR_SITE_ID + "='" + siteip + "' and " + STR_DVRID + "='" + dvrID + "' and " + STR_DVR_IP + " ='" + dvrIP + "'");
			if( rows == null || rows.Length == 0)
			{
				DataRow r = m_dsSiteList.Tables[STR_TABLE_DVR_ID].NewRow();
				r[STR_SITE_ID] = siteip;
				r[STR_DVRID] = dvrID;
				r[STR_DVR_IP] = dvrIP;
                r[STR_DVR_MODE] = (dvrstandard == true ? "1" : "0");
				m_dsSiteList.Tables[STR_TABLE_DVR_ID].Rows.Add(r);
				m_dsSiteList.Tables[STR_TABLE_DVR_ID].AcceptChanges();

			}

		}
		void dvrtab_OnCloseDVR(string dvrguid)
		{
			//throw new Exception("The method or operation is not implemented.");
			DVRNode delNode = GetNode(dvrguid);
			DVRTabPage deltab = GetTabPage(dvrguid);
			if (delNode != null)
			{
				TreeDVR.Nodes.Remove( delNode);
			}
			if( deltab != null)
			{
				tabDVRList.TabPages.Remove(deltab);
			}
			UpdateDVRTotal();
		}
		void dvrtab_OnSocketEvents(Utils.DVR_Info dvrinfo, string cmsIP, int cmsPort, string dvrguid,string version, Utils.DVR_Status curStatus)
		{
			//throw new Exception("The method or operation is not implemented.");
			del_UpdateTReenodeText(dvrinfo, cmsIP, cmsPort,dvrguid,version, curStatus);
		}
		private void cmdAddDVR_Click(object sender, EventArgs e)
		{
			cmdAddDVR.Enabled = false;
			string cmsip = string.Empty;
			if (cboCMSList.SelectedItem == null)
			{
				cmsip =  cboCMSList.Text;
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
			mstop = false;
			string[] dvrguids = null;
			string[] dvrips = null;
            bool[] dvrmodes = null;
			if (!CheckDelayvalue(true))
			{
				cmdAddDVR.Enabled = true;
				return;
			}
            Utils.GetDVRInfo(cbDVRMode.Checked,ref dvrmodes, ref dvrguids, ref dvrips, Convert.ToInt32(txtDVRNum.Text));
            StartThread(m_TimerSetting, delaytime, cbAutoConnect.Checked, true, cmsip, Convert.ToInt32(txtCMSPort.Text), dvrmodes, dvrguids, dvrips, /*txtVersion.Text*/cboVersion.Text, txtlocation.Text);
			cmdAddDVR.Enabled = true;
		}
		private void TreeDVR_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if( TreeDVR.SelectedNode != null && TreeDVR.SelectedNode.GetType().Equals( typeof(DVRNode)))
			{
				DVRTabPage seltab = GetTabPage( ((DVRNode)TreeDVR.SelectedNode).DVRID);
				if (seltab != null)
					tabDVRList.SelectedTab = seltab;
			}
			
		}
		private DVRNode GetNode(string strnodeID)
		{
			if (TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
				return null;
			DVRNode retNode = null;
			TreeNode root = TreeDVR.Nodes[STR_DVR_LIST_NODE_NAME];
			foreach (TreeNode node in root.Nodes)
			{
				if( string.Compare( ((DVRNode)node).DVRID, strnodeID, true) == 0)
				{
					retNode = (DVRNode)node;
					break;
				}
			}
			return retNode;
		}
		private DVRTabPage GetTabPage(string strid)
		{
			DVRTabPage childtab = null;
			foreach (TabPage tab in tabDVRList.TabPages)
			{
				if( string.Compare( ((DVRTabPage)tab).TabID, strid, true) == 0)
				{
					childtab = (DVRTabPage)tab;
					break;
				}
			}
			return childtab;
		}
		private void del_UpdateTReenodeText(Utils.DVR_Info dvrinfo, string cmsIP, int cmsport, string dvrguid, string version, Utils.DVR_Status dvrstatus)
		{
			if( this.InvokeRequired)
			{
				this.BeginInvoke(new UpdateTreenodeText(del_UpdateTReenodeText), new object[] {dvrinfo,cmsIP,cmsport, dvrguid,version, dvrstatus});
			}
			else
			{
				DVRNode changenode = GetNode(dvrguid);
				DVRTabPage changetab = GetTabPage(dvrguid);
				if( changenode != null)
				{
					changenode.UpdateDvrSatus(dvrinfo, cmsIP,cmsport, version, dvrstatus);
					if( changetab != null && version != null && version.Length > 0)
					{
						string newcaption = version + "#" + changenode.NodeID.ToString();
						changetab.ChangeTabCaption(newcaption);
					}
				}
			}
		}
		private void LoadCMSList()
		{
			string apppath = Application.StartupPath;
			if( !apppath .EndsWith("\\"))
				apppath += "\\" + STR_DVR_SITES;
			if (!Directory.Exists(apppath))
				Directory.CreateDirectory(apppath);
			string fpath = apppath + "\\" + STR_FILENAME_SITE_LIST;
			if( File.Exists( fpath))
			{
				m_dsSiteList = new DataSet();
				m_dsSiteList.ReadXml(fpath);
				if(m_dsSiteList.Tables != null && m_dsSiteList.Tables.Contains(STR_TABLE_DVR_ID) && !m_dsSiteList.Tables[STR_TABLE_DVR_ID].Columns.Contains(STR_DVR_IP))
				{
					//dvr IP
					DataColumn dvrcol = new DataColumn();
					dvrcol.DataType = typeof(System.String);
					dvrcol.AllowDBNull = true;
					dvrcol.DefaultValue = string.Empty;
					dvrcol.ColumnName = STR_DVR_IP;
					m_dsSiteList.Tables[STR_TABLE_DVR_ID].Columns.Add(dvrcol);
				}
                if (m_dsSiteList.Tables != null && m_dsSiteList.Tables.Contains(STR_TABLE_DVR_ID) && !m_dsSiteList.Tables[STR_TABLE_DVR_ID].Columns.Contains(STR_DVR_MODE))
                {
                    //dvr Mode
                    DataColumn dvrcol = new DataColumn();
                    dvrcol.DataType = typeof(System.String);
                    dvrcol.AllowDBNull = true;
                    dvrcol.DefaultValue = "0";
                    dvrcol.ColumnName = STR_DVR_MODE;
                    m_dsSiteList.Tables[STR_TABLE_DVR_ID].Columns.Add(dvrcol);
                }
			}
			else
			{
				m_dsSiteList = InitSiteList();
			}
			if (m_dsSiteList != null && m_dsSiteList.Tables.Contains(STR_TABLE_SITE_IP) && m_dsSiteList.Tables.Contains(STR_TABLE_DVR_ID))
			{
				foreach (DataRow row in m_dsSiteList.Tables[STR_TABLE_SITE_IP].Rows)
				{
					cboCMSList.Items.Add(row[STR_SITE_ID].ToString());

				}
			}
			else
				m_dsSiteList = InitSiteList();
		}
		private DataSet InitSiteList()
		{
			DataSet ds = new DataSet();
			DataTable tblSite = new DataTable();
			DataColumn col = new DataColumn();
			col.AllowDBNull = true;
			col.DataType = typeof(System.String);
			col.DefaultValue = string.Empty;
			col.ColumnName = STR_SITE_ID;
			tblSite.Columns.Add(col);
			tblSite.TableName = STR_TABLE_SITE_IP;
			//add DVR ID
			DataTable tbldvr = new DataTable();
			DataColumn dvrcol = null;
			dvrcol = new DataColumn();
			dvrcol.DataType = typeof(System.String);
			dvrcol.AllowDBNull = true;
			dvrcol.DefaultValue = string.Empty;
			dvrcol.ColumnName = STR_SITE_ID;
			tbldvr.Columns.Add(dvrcol);

			dvrcol = new DataColumn();
			dvrcol.DataType = typeof(System.String);
			dvrcol.AllowDBNull = true;
			dvrcol.DefaultValue = string.Empty;
			dvrcol.ColumnName = STR_DVRID;
			tbldvr.Columns.Add(dvrcol);
			//dvr IP
			dvrcol = new DataColumn();
			dvrcol.DataType = typeof(System.String);
			dvrcol.AllowDBNull = true;
			dvrcol.DefaultValue = string.Empty;
			dvrcol.ColumnName = STR_DVR_IP;
			tbldvr.Columns.Add(dvrcol);

            //dvr Mode
            dvrcol = new DataColumn();
            dvrcol.DataType = typeof(System.String);
            dvrcol.AllowDBNull = true;
            dvrcol.DefaultValue = "0";
            dvrcol.ColumnName = STR_DVR_MODE;
            tbldvr.Columns.Add(dvrcol);

			tbldvr.TableName = STR_TABLE_DVR_ID;
			ds.Tables.Add(tblSite);
			ds.Tables.Add(tbldvr);
			return ds;
		}
		private void SetupMenu()
		{
			foreach (ToolStripItem mitem in menu_all_List.Items)
			{
				mitem.Enabled = false;
			}

			if( TreeDVR.SelectedNode == null || TreeDVR.Nodes.Count == 0)
				return;
			if ( TreeDVR.SelectedNode.GetType() == typeof(TreeNode))
			{
				menu_dvr_disconnect_all.Enabled = true;
				menu_dvr_connect_all.Enabled = true;
				menu_dvr_close_all.Enabled = true;
				menu_dvr_delete_all.Enabled = true;
				return;
			}
			if (TreeDVR.SelectedNode.GetType() == typeof(DVRNode))
			{
				menu_dvr_close.Enabled = true;
				menu_dvr_disconect.Enabled = true;
				menu_dvr_connect.Enabled = true;
				menu_dvr_delete.Enabled = true;
				return;
			}
			
		}
		private void DeleteDVR(bool saveconfig, string siteip, string dvrID)
		{
			DataRow[] rows = null;
			rows = m_dsSiteList.Tables[STR_TABLE_DVR_ID].Select(STR_SITE_ID + "='" + siteip + "' and " + STR_DVRID + "='" + dvrID + "'");
			if (rows != null && rows.Length > 0)
			{
				m_dsSiteList.Tables[STR_TABLE_DVR_ID].Rows.Remove(rows[0]);
				m_dsSiteList.Tables[STR_TABLE_DVR_ID].AcceptChanges();
			}
			rows = m_dsSiteList.Tables[STR_TABLE_DVR_ID].Select(STR_SITE_ID + "='" + siteip + "'");
			DataRow[] siterows = m_dsSiteList.Tables[STR_TABLE_SITE_IP].Select(STR_SITE_ID + "='" + siteip + "'");
			if ( (rows == null || rows.Length == 0) && (siterows != null && siterows.Length > 0) )
			{
				m_dsSiteList.Tables[STR_TABLE_SITE_IP].Rows.Remove(siterows[0]);
				m_dsSiteList.Tables[STR_TABLE_SITE_IP].AcceptChanges();
				cboCMSList.Items.Remove(siteip);
			}

			if( saveconfig)
				SaveConfigFile();
			UpdateDVRTotal();
		}
		private void cmdLoadDVR_Click(object sender, EventArgs e)
		{
			if( cboCMSList.SelectedItem == null)
			{
				MessageBox.Show("No DVR found.");
				return;
			}
			else
			{
				if( !CheckDelayvalue(false))
				{
					return;
				}
				string cmsIP = cboCMSList.SelectedItem.ToString();
				DataRow[] dvrlist = m_dsSiteList.Tables[STR_TABLE_DVR_ID].Select(STR_SITE_ID + "='" + cmsIP + "'");
				string dvrguid = string.Empty;
				string dvrip = string.Empty;
				string[] dvrIPs = new string[dvrlist.Length];
				string[] dvrGUIDs = new string[dvrlist.Length];
                bool[] dvrmodes = new bool[dvrlist.Length];
				int count = 0;
                string dvrmode = string.Empty;
				foreach (DataRow row in dvrlist )
				{
					dvrguid = row[STR_DVRID].ToString();
					dvrip = row[STR_DVR_IP].ToString();
                    dvrmode = row[STR_DVR_MODE].ToString();
					if (dvrip.Length == 0)
						dvrip = Utils.RandomIP();
					if (dvrguid.Length == 0)
						dvrguid = Guid.NewGuid().ToString().ToUpper();

					dvrIPs[count] = dvrip;
					dvrGUIDs[count] = dvrguid;
                    dvrmodes[count] = (dvrmode == "1"? true:false);
					dvrip = dvrguid = string.Empty;
					count++;
				}
				if( dvrIPs.Length > 0)
				{
					int delaytime = Utils.DELAY_TIME_DISCONNECT_DVR;
					try
					{
						delaytime = Convert.ToInt32(txtconnectDelay.Text);
					}
					catch (System.Exception ex)
					{
						delaytime = Utils.DELAY_TIME_DISCONNECT_DVR;
					}
                    string sDVRVersion = cboVersion.SelectedText;
                    sDVRVersion = cboVersion.Text;

                    StartThread(m_TimerSetting, delaytime, cbAutoConnect.Checked, false, cmsIP, Convert.ToInt32(txtCMSPort.Text), dvrmodes, dvrGUIDs, dvrIPs, /*txtVersion.Text*/cboVersion.Text, txtlocation.Text);
				}
			}
		}
		private void UpdateDVRTotal()
		{
			if( TreeDVR.Nodes == null || TreeDVR.Nodes.Count == 0)
			{
				lblTotalDVR.Text = "Total: 0 DVR";
				return;
			}
			if( TreeDVR.Nodes[0].Nodes != null)
				lblTotalDVR.Text = string.Format("Total: {0} DVR(s)", TreeDVR.Nodes[0].Nodes.Count) ;
		}
		private void menu_all_List_Opening(object sender, CancelEventArgs e)
		{
			SetupMenu();
		}
		private void ProcessMenuEvents(ToolStripItem mitem)
		{
			#region disconnect all
			if (mitem.Equals(menu_dvr_disconnect_all))
			{
				StartThreadConnect(0, tabDVRList.TabPages.Count, true, false, Utils.DELAY_TIME_DISCONNECT_DVR);
				return;
			}
			#endregion
			#region Connect all
			if (mitem.Equals(menu_dvr_connect_all))
			{

				int delay = 0;
				try
				{
					delay = Convert.ToInt32(txtconnectDelay.Text);
					if( delay < Utils.DELAY_TIME_CONNECT_DVR )
					{
						MessageBox.Show("Delay time to connect CMS must be greater than  " + (Utils.DELAY_TIME_CONNECT_DVR -1).ToString() + " second(s).");
						return;
					}
				}
				catch (System.Exception ex)
				{
					return;
				}


				StartThreadConnect(0, tabDVRList.TabPages.Count, false,false, delay);
				
				return;
			}
			#endregion
			#region Close all
			if (mitem.Equals(menu_dvr_close_all))
			{
				StartThreadConnect(0, tabDVRList.TabPages.Count, true, true, Utils.DELAY_TIME_DISCONNECT_DVR);
				//foreach (DVRTabPage dvrtab in tabDVRList.TabPages)
				//{
				//    dvrtab.DeleteDVR();
				//}
				return;
			}
			#endregion
			#region delete all
			if (mitem.Equals(menu_dvr_delete_all))
			{
				foreach (DVRTabPage dvrtab in tabDVRList.TabPages)
				{
					dvrtab.DeleteDVR();
					DeleteDVR(false, dvrtab.CMSIP, dvrtab.DVRGuid);
				}
				SaveConfigFile();
				return;
			}
			#endregion 
			if (mitem.Equals(menu_dvr_connect))
			{
				DVRNode selnode = (DVRNode)TreeDVR.SelectedNode;
				DVRTabPage dvrtab = GetTabPage(selnode.DVRID);
				if( dvrtab != null)
				{
					dvrtab.StartDVR();
				}
				return;
			}
			if (mitem.Equals(menu_dvr_disconect))
			{
				DVRNode selnode = (DVRNode)TreeDVR.SelectedNode;
				DVRTabPage dvrtab = GetTabPage(selnode.DVRID);
				if (dvrtab != null)
				{
					dvrtab.StopDVR();
				}
				return;
			}
			if (mitem.Equals(menu_dvr_close))
			{
				DVRNode selnode = (DVRNode)TreeDVR.SelectedNode;
				DVRTabPage dvrtab = GetTabPage(selnode.DVRID);
				if (dvrtab != null)
				{
					dvrtab.DeleteDVR();
				}
				return;
			}
			if (mitem.Equals(menu_dvr_delete))
			{
				DVRNode selnode = (DVRNode)TreeDVR.SelectedNode;
				DVRTabPage dvrtab = GetTabPage(selnode.DVRID);
				if (dvrtab != null)
				{
					dvrtab.DeleteDVR();
					DeleteDVR(true, dvrtab.CMSIP, dvrtab.DVRGuid);
				}
				return;
			}
			
			
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
				if( Convert.ToInt32(txtDVRNum.Text)<0)
				{
					MessageBox.Show("DVR number must be numeric.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
			}
			return true;
		}
        private void ApplyTimer2GUI(UCtrlDVR.TimerSetting tmsetting)
        {
            lstTimer.Items.Clear();
            string stritem = string.Format("Message time: {0} min(s)", tmsetting.m_msg_timeout);
            lstTimer.Items.Add(stritem);

            stritem = string.Format("Message retry: {0} min(s)", tmsetting.m_msg_retry);
            lstTimer.Items.Add(stritem);

            stritem = string.Format("Reconnect time: {0} min(s)", tmsetting.m_dvr_reconnect);
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
		private void menu_dvr_connect_all_Click(object sender, EventArgs e)
		{
			ProcessMenuEvents(sender as ToolStripItem);
		}
		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			mstop = true;
			m_wait_complete.WaitOne();
			if( !this.Enabled)
			{
				e.Cancel = true;
				return;
			}
			if( this.TreeDVR.Nodes != null && TreeDVR.Nodes.Count > 0  && TreeDVR.Nodes[0].Nodes.Count > 0)
			{
				ProcessMenuEvents(menu_dvr_close_all);
				m_closing = true;
				e.Cancel = true;
			}
		}

		private void TreeDVR_MouseDoubleClick(object sender, MouseEventArgs e)
		{
            return;
			TreeView curtree = sender as TreeView;
			TreeNode curnode = curtree.SelectedNode;
			if(curnode != null && curnode.GetType().Equals( typeof(DVRNode) ))
			{
				DVRNode dvrnode = curnode as DVRNode;
				if (dvrnode.NodeStatus == Utils.DVR_Status.Socket_Connected || dvrnode.NodeStatus == Utils.DVR_Status.CMS_Connected)
				{
					ProcessMenuEvents(menu_dvr_disconect);
				}
				else
				{
					ProcessMenuEvents(menu_dvr_connect);
				}
			}
			
		}

		private void cbShowinfo_CheckedChanged(object sender, EventArgs e)
		{
			TreeDVR.ShowNodeToolTips = cbShowinfo.Checked;
		}
		protected override void WndProc(ref Message m)
		{
			if( m.Msg == frmLoading.MSG_CANCEL_ADD_ITEMS)
			{
				mstop = true;			
			}
			else
				base.WndProc(ref m);
		}

        private void cmdSettingtimer_Click(object sender, EventArgs e)
        {
            frmSetupTimer obj = new frmSetupTimer(m_TimerSetting,true);
            if (obj.ShowDialog(this) == DialogResult.OK)
            {
                m_TimerSetting = obj.Setting;
                ApplyTimer2GUI(m_TimerSetting);
                
            }
        }

        private string GetVersionName(string sDirName)
        {
            string sVersion = "";
            switch(sDirName)
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
                //case "3.00":
                //    sVersion = "3.00 (Simulator)";
                //    break;
                case "1.600":
                default:
                    sVersion = "1.600 (Simulator)";
                    break;
            }
            return sVersion;
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
	}
}