namespace CMSTest
{
	partial class frmMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.cmdSettingtimer = new System.Windows.Forms.Button();
			this.lstTimer = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtconnectDelay = new System.Windows.Forms.TextBox();
			this.cbShowinfo = new System.Windows.Forms.CheckBox();
			this.PInfomation = new System.Windows.Forms.Panel();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.selectRow = new System.Windows.Forms.Label();
			this.numberRows = new System.Windows.Forms.TextBox();
			this.keepXml = new System.Windows.Forms.CheckBox();
			this.cboVersion = new System.Windows.Forms.ComboBox();
			this.cbDVRMode = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.cmdAddDVR = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.cbAutoConnect = new System.Windows.Forms.CheckBox();
			this.txtDVRNum = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtlocation = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cboCMSList = new System.Windows.Forms.ComboBox();
			this.txtCMSPort = new System.Windows.Forms.TextBox();
			this.cmdLoadDVR = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.txtVersion = new System.Windows.Forms.TextBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.TreeDVR = new System.Windows.Forms.TreeView();
			this.menu_all_List = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menu_dvr_connect_all = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_disconnect_all = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_close_all = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_delete_all = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menu_dvr_connect = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_disconect = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_close = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_dvr_delete = new System.Windows.Forms.ToolStripMenuItem();
			this.lblTotalDVR = new System.Windows.Forms.Label();
			this.tabDVRList = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.uCtrlDVR1 = new CMSTest.UCtrlDVR();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.uCtrlDVR2 = new CMSTest.UCtrlDVR();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.PInfomation.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.menu_all_List.SuspendLayout();
			this.tabDVRList.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
			this.splitContainer1.Panel1.Controls.Add(this.PInfomation);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(988, 640);
			this.splitContainer1.SplitterDistance = 118;
			this.splitContainer1.TabIndex = 0;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.cmdSettingtimer);
			this.groupBox3.Controls.Add(this.lstTimer);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.txtconnectDelay);
			this.groupBox3.Controls.Add(this.cbShowinfo);
			this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox3.Location = new System.Drawing.Point(610, 0);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(378, 118);
			this.groupBox3.TabIndex = 30;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Options";
			// 
			// cmdSettingtimer
			// 
			this.cmdSettingtimer.Location = new System.Drawing.Point(208, 89);
			this.cmdSettingtimer.Name = "cmdSettingtimer";
			this.cmdSettingtimer.Size = new System.Drawing.Size(75, 23);
			this.cmdSettingtimer.TabIndex = 35;
			this.cmdSettingtimer.Text = "Timer setting";
			this.cmdSettingtimer.UseVisualStyleBackColor = true;
			this.cmdSettingtimer.Click += new System.EventHandler(this.cmdSettingtimer_Click);
			// 
			// lstTimer
			// 
			this.lstTimer.FormattingEnabled = true;
			this.lstTimer.Location = new System.Drawing.Point(23, 56);
			this.lstTimer.Name = "lstTimer";
			this.lstTimer.Size = new System.Drawing.Size(179, 56);
			this.lstTimer.TabIndex = 34;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(191, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(168, 13);
			this.label4.TabIndex = 33;
			this.label4.Text = "Delay time connect to CMS server";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(270, 30);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 13);
			this.label3.TabIndex = 32;
			this.label3.Text = "second(s)";
			// 
			// txtconnectDelay
			// 
			this.txtconnectDelay.Location = new System.Drawing.Point(208, 27);
			this.txtconnectDelay.MaxLength = 10;
			this.txtconnectDelay.Name = "txtconnectDelay";
			this.txtconnectDelay.Size = new System.Drawing.Size(56, 20);
			this.txtconnectDelay.TabIndex = 31;
			this.txtconnectDelay.Text = "15";
			// 
			// cbShowinfo
			// 
			this.cbShowinfo.AutoSize = true;
			this.cbShowinfo.Location = new System.Drawing.Point(23, 14);
			this.cbShowinfo.Name = "cbShowinfo";
			this.cbShowinfo.Size = new System.Drawing.Size(143, 17);
			this.cbShowinfo.TabIndex = 29;
			this.cbShowinfo.Text = "Show DVR info as tolltip.";
			this.cbShowinfo.UseVisualStyleBackColor = true;
			this.cbShowinfo.CheckedChanged += new System.EventHandler(this.cbShowinfo_CheckedChanged);
			// 
			// PInfomation
			// 
			this.PInfomation.Controls.Add(this.groupBox2);
			this.PInfomation.Controls.Add(this.groupBox1);
			this.PInfomation.Dock = System.Windows.Forms.DockStyle.Left;
			this.PInfomation.Location = new System.Drawing.Point(0, 0);
			this.PInfomation.Name = "PInfomation";
			this.PInfomation.Size = new System.Drawing.Size(610, 118);
			this.PInfomation.TabIndex = 29;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.selectRow);
			this.groupBox2.Controls.Add(this.numberRows);
			this.groupBox2.Controls.Add(this.keepXml);
			this.groupBox2.Controls.Add(this.cboVersion);
			this.groupBox2.Controls.Add(this.cbDVRMode);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.cmdAddDVR);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.cbAutoConnect);
			this.groupBox2.Controls.Add(this.txtDVRNum);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.txtlocation);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(0, 40);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(610, 78);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "DVR server info";
			// 
			// selectRow
			// 
			this.selectRow.AutoSize = true;
			this.selectRow.Location = new System.Drawing.Point(476, 32);
			this.selectRow.Name = "selectRow";
			this.selectRow.Size = new System.Drawing.Size(92, 13);
			this.selectRow.TabIndex = 41;
			this.selectRow.Text = "Number result row";
			// 
			// numberRows
			// 
			this.numberRows.Location = new System.Drawing.Point(574, 29);
			this.numberRows.MaxLength = 3;
			this.numberRows.Name = "numberRows";
			this.numberRows.Size = new System.Drawing.Size(33, 20);
			this.numberRows.TabIndex = 40;
			this.numberRows.Text = "30";
			// 
			// keepXml
			// 
			this.keepXml.AutoSize = true;
			this.keepXml.Location = new System.Drawing.Point(479, 6);
			this.keepXml.Name = "keepXml";
			this.keepXml.Size = new System.Drawing.Size(114, 17);
			this.keepXml.TabIndex = 39;
			this.keepXml.Text = "Keep xml message";
			this.keepXml.UseVisualStyleBackColor = true;
			// 
			// cboVersion
			// 
			this.cboVersion.FormattingEnabled = true;
			this.cboVersion.Location = new System.Drawing.Point(108, 13);
			this.cboVersion.Name = "cboVersion";
			this.cboVersion.Size = new System.Drawing.Size(121, 21);
			this.cboVersion.TabIndex = 28;
			// 
			// cbDVRMode
			// 
			this.cbDVRMode.AutoSize = true;
			this.cbDVRMode.Location = new System.Drawing.Point(238, 61);
			this.cbDVRMode.Name = "cbDVRMode";
			this.cbDVRMode.Size = new System.Drawing.Size(106, 17);
			this.cbDVRMode.TabIndex = 27;
			this.cbDVRMode.Text = "Standard version";
			this.cbDVRMode.UseVisualStyleBackColor = true;
			this.cbDVRMode.Visible = false;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(17, 16);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(67, 13);
			this.label7.TabIndex = 20;
			this.label7.Text = "DVR version";
			// 
			// cmdAddDVR
			// 
			this.cmdAddDVR.Location = new System.Drawing.Point(372, 32);
			this.cmdAddDVR.Name = "cmdAddDVR";
			this.cmdAddDVR.Size = new System.Drawing.Size(85, 23);
			this.cmdAddDVR.TabIndex = 24;
			this.cmdAddDVR.Text = "Add new DVR";
			this.cmdAddDVR.UseVisualStyleBackColor = true;
			this.cmdAddDVR.Click += new System.EventHandler(this.cmdAddDVR_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(17, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 13);
			this.label2.TabIndex = 26;
			this.label2.Text = "Location";
			// 
			// cbAutoConnect
			// 
			this.cbAutoConnect.AutoSize = true;
			this.cbAutoConnect.Location = new System.Drawing.Point(238, 39);
			this.cbAutoConnect.Name = "cbAutoConnect";
			this.cbAutoConnect.Size = new System.Drawing.Size(128, 17);
			this.cbAutoConnect.TabIndex = 23;
			this.cbAutoConnect.Text = "Auto connect to CMS";
			this.cbAutoConnect.UseVisualStyleBackColor = true;
			// 
			// txtDVRNum
			// 
			this.txtDVRNum.Location = new System.Drawing.Point(373, 6);
			this.txtDVRNum.MaxLength = 4;
			this.txtDVRNum.Name = "txtDVRNum";
			this.txtDVRNum.Size = new System.Drawing.Size(53, 20);
			this.txtDVRNum.TabIndex = 21;
			this.txtDVRNum.Text = "1";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(235, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 22;
			this.label1.Text = "Number of DVR";
			// 
			// txtlocation
			// 
			this.txtlocation.Location = new System.Drawing.Point(108, 36);
			this.txtlocation.Name = "txtlocation";
			this.txtlocation.Size = new System.Drawing.Size(121, 20);
			this.txtlocation.TabIndex = 25;
			this.txtlocation.Text = "HCM";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cboCMSList);
			this.groupBox1.Controls.Add(this.txtCMSPort);
			this.groupBox1.Controls.Add(this.cmdLoadDVR);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.txtVersion);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(610, 40);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "CMS server info";
			// 
			// cboCMSList
			// 
			this.cboCMSList.FormattingEnabled = true;
			this.cboCMSList.Location = new System.Drawing.Point(108, 12);
			this.cboCMSList.Name = "cboCMSList";
			this.cboCMSList.Size = new System.Drawing.Size(121, 21);
			this.cboCMSList.TabIndex = 27;
			// 
			// txtCMSPort
			// 
			this.txtCMSPort.Enabled = false;
			this.txtCMSPort.Location = new System.Drawing.Point(416, 12);
			this.txtCMSPort.Name = "txtCMSPort";
			this.txtCMSPort.Size = new System.Drawing.Size(64, 20);
			this.txtCMSPort.TabIndex = 18;
			this.txtCMSPort.Text = "1000";
			// 
			// cmdLoadDVR
			// 
			this.cmdLoadDVR.Location = new System.Drawing.Point(235, 12);
			this.cmdLoadDVR.Name = "cmdLoadDVR";
			this.cmdLoadDVR.Size = new System.Drawing.Size(66, 23);
			this.cmdLoadDVR.TabIndex = 28;
			this.cmdLoadDVR.Text = "Load DVR";
			this.cmdLoadDVR.UseVisualStyleBackColor = true;
			this.cmdLoadDVR.Click += new System.EventHandler(this.cmdLoadDVR_Click);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(321, 15);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(83, 13);
			this.label8.TabIndex = 19;
			this.label8.Text = "CMS server port";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(19, 18);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 13);
			this.label6.TabIndex = 17;
			this.label6.Text = "CMS server IP";
			// 
			// txtVersion
			// 
			this.txtVersion.Location = new System.Drawing.Point(488, 12);
			this.txtVersion.Name = "txtVersion";
			this.txtVersion.Size = new System.Drawing.Size(117, 20);
			this.txtVersion.TabIndex = 15;
			this.txtVersion.Text = "1.600 (Simulator)";
			this.txtVersion.Visible = false;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.AutoScroll = true;
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.AutoScroll = true;
			this.splitContainer2.Panel2.Controls.Add(this.tabDVRList);
			this.splitContainer2.Size = new System.Drawing.Size(988, 518);
			this.splitContainer2.SplitterDistance = 222;
			this.splitContainer2.TabIndex = 0;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.TreeDVR);
			this.splitContainer3.Panel1MinSize = 20;
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.lblTotalDVR);
			this.splitContainer3.Size = new System.Drawing.Size(222, 518);
			this.splitContainer3.SplitterDistance = 489;
			this.splitContainer3.TabIndex = 2;
			// 
			// TreeDVR
			// 
			this.TreeDVR.ContextMenuStrip = this.menu_all_List;
			this.TreeDVR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeDVR.Location = new System.Drawing.Point(0, 0);
			this.TreeDVR.Name = "TreeDVR";
			this.TreeDVR.ShowNodeToolTips = true;
			this.TreeDVR.Size = new System.Drawing.Size(222, 489);
			this.TreeDVR.TabIndex = 0;
			this.TreeDVR.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeDVR_AfterSelect);
			this.TreeDVR.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TreeDVR_MouseDoubleClick);
			// 
			// menu_all_List
			// 
			this.menu_all_List.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_dvr_connect_all,
            this.menu_dvr_disconnect_all,
            this.menu_dvr_close_all,
            this.menu_dvr_delete_all,
            this.toolStripSeparator1,
            this.menu_dvr_connect,
            this.menu_dvr_disconect,
            this.menu_dvr_close,
            this.menu_dvr_delete});
			this.menu_all_List.Name = "menu_all_List";
			this.menu_all_List.Size = new System.Drawing.Size(179, 186);
			this.menu_all_List.Text = "Close all DVRs";
			this.menu_all_List.Opening += new System.ComponentModel.CancelEventHandler(this.menu_all_List_Opening);
			// 
			// menu_dvr_connect_all
			// 
			this.menu_dvr_connect_all.Name = "menu_dvr_connect_all";
			this.menu_dvr_connect_all.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_connect_all.Text = "Connect all DVRs";
			this.menu_dvr_connect_all.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_disconnect_all
			// 
			this.menu_dvr_disconnect_all.Name = "menu_dvr_disconnect_all";
			this.menu_dvr_disconnect_all.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_disconnect_all.Text = "Disconnect all DVRs";
			this.menu_dvr_disconnect_all.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_close_all
			// 
			this.menu_dvr_close_all.Name = "menu_dvr_close_all";
			this.menu_dvr_close_all.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_close_all.Text = "Close all DVRs";
			this.menu_dvr_close_all.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_delete_all
			// 
			this.menu_dvr_delete_all.Name = "menu_dvr_delete_all";
			this.menu_dvr_delete_all.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_delete_all.Text = "Delete all DVRs";
			this.menu_dvr_delete_all.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
			// 
			// menu_dvr_connect
			// 
			this.menu_dvr_connect.Name = "menu_dvr_connect";
			this.menu_dvr_connect.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_connect.Text = "Connect";
			this.menu_dvr_connect.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_disconect
			// 
			this.menu_dvr_disconect.Name = "menu_dvr_disconect";
			this.menu_dvr_disconect.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_disconect.Text = "Disconnect";
			this.menu_dvr_disconect.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_close
			// 
			this.menu_dvr_close.Name = "menu_dvr_close";
			this.menu_dvr_close.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_close.Text = "Close";
			this.menu_dvr_close.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// menu_dvr_delete
			// 
			this.menu_dvr_delete.Name = "menu_dvr_delete";
			this.menu_dvr_delete.Size = new System.Drawing.Size(178, 22);
			this.menu_dvr_delete.Text = "Delete";
			this.menu_dvr_delete.Click += new System.EventHandler(this.menu_dvr_connect_all_Click);
			// 
			// lblTotalDVR
			// 
			this.lblTotalDVR.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblTotalDVR.Location = new System.Drawing.Point(0, 8);
			this.lblTotalDVR.Name = "lblTotalDVR";
			this.lblTotalDVR.Size = new System.Drawing.Size(222, 17);
			this.lblTotalDVR.TabIndex = 1;
			this.lblTotalDVR.Text = "Total: 0 DVR(s)";
			this.lblTotalDVR.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tabDVRList
			// 
			this.tabDVRList.Controls.Add(this.tabPage1);
			this.tabDVRList.Controls.Add(this.tabPage2);
			this.tabDVRList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabDVRList.Location = new System.Drawing.Point(0, 0);
			this.tabDVRList.Name = "tabDVRList";
			this.tabDVRList.SelectedIndex = 0;
			this.tabDVRList.Size = new System.Drawing.Size(762, 518);
			this.tabDVRList.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.uCtrlDVR1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(754, 492);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// uCtrlDVR1
			// 
			this.uCtrlDVR1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.uCtrlDVR1.Location = new System.Drawing.Point(3, 3);
			this.uCtrlDVR1.Name = "uCtrlDVR1";
			this.uCtrlDVR1.Size = new System.Drawing.Size(748, 486);
			this.uCtrlDVR1.TabIndex = 0;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.uCtrlDVR2);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(754, 492);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "tabPage2";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// uCtrlDVR2
			// 
			this.uCtrlDVR2.Location = new System.Drawing.Point(3, 2);
			this.uCtrlDVR2.Name = "uCtrlDVR2";
			this.uCtrlDVR2.Size = new System.Drawing.Size(743, 482);
			this.uCtrlDVR2.TabIndex = 1;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 640);
			this.Controls.Add(this.splitContainer1);
			this.MinimumSize = new System.Drawing.Size(996, 587);
			this.Name = "frmMain";
			this.Text = "DVR - CMS server simulator";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.PInfomation.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.menu_all_List.ResumeLayout(false);
			this.tabDVRList.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TreeView TreeDVR;
		private System.Windows.Forms.TabControl tabDVRList;
		private System.Windows.Forms.TabPage tabPage1;
		private UCtrlDVR uCtrlDVR1;
		private System.Windows.Forms.TabPage tabPage2;
		private UCtrlDVR uCtrlDVR2;
		private System.Windows.Forms.TextBox txtCMSPort;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtVersion;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmdAddDVR;
		private System.Windows.Forms.CheckBox cbAutoConnect;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtDVRNum;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtlocation;
		private System.Windows.Forms.ComboBox cboCMSList;
		private System.Windows.Forms.Button cmdLoadDVR;
		private System.Windows.Forms.ContextMenuStrip menu_all_List;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_connect;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_disconect;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_connect_all;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_disconnect_all;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_close_all;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_close;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_delete;
		private System.Windows.Forms.ToolStripMenuItem menu_dvr_delete_all;
		private System.Windows.Forms.Panel PInfomation;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label lblTotalDVR;
		private System.Windows.Forms.CheckBox cbShowinfo;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtconnectDelay;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button cmdSettingtimer;
        private System.Windows.Forms.ListBox lstTimer;
        private System.Windows.Forms.CheckBox cbDVRMode;
        private System.Windows.Forms.ComboBox cboVersion;
        private System.Windows.Forms.Label selectRow;
        private System.Windows.Forms.TextBox numberRows;
        private System.Windows.Forms.CheckBox keepXml;

	}
}

