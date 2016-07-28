namespace CMSTest
{
	partial class UCtrlDVR
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cmdClear = new System.Windows.Forms.Button();
			this.cbDVRMode = new System.Windows.Forms.CheckBox();
			this.cmdDisconnect = new System.Windows.Forms.Button();
			this.cmsSetDelay = new System.Windows.Forms.Button();
			this.cmsClose = new System.Windows.Forms.Button();
			this.txtServerName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtServerID = new System.Windows.Forms.TextBox();
			this.txtUser = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.txtCMSPort = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.txtCMSIP = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cmdStart = new System.Windows.Forms.Button();
			this.txtVersion = new System.Windows.Forms.TextBox();
			this.txtModel = new System.Windows.Forms.TextBox();
			this.txtlocation = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.lblModel = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.lblDVRIP = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblGUID = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lstTimerInfo = new System.Windows.Forms.ListBox();
			this.cbViewXMLMessage = new System.Windows.Forms.CheckBox();
			this.DgStatus = new System.Windows.Forms.DataGridView();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.lblQeue = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lstMsgQueu = new System.Windows.Forms.ListBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.cbAutoScroll = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DgStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cmdClear);
			this.groupBox1.Controls.Add(this.cbDVRMode);
			this.groupBox1.Controls.Add(this.cmdDisconnect);
			this.groupBox1.Controls.Add(this.cmsSetDelay);
			this.groupBox1.Controls.Add(this.cmsClose);
			this.groupBox1.Controls.Add(this.txtServerName);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.txtServerID);
			this.groupBox1.Controls.Add(this.txtUser);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.txtCMSPort);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.txtCMSIP);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.cmdStart);
			this.groupBox1.Controls.Add(this.txtVersion);
			this.groupBox1.Controls.Add(this.txtModel);
			this.groupBox1.Controls.Add(this.txtlocation);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.lblModel);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.lblDVRIP);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.lblGUID);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.lstTimerInfo);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(753, 132);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "DVR/CMS Info";
			// 
			// cmdClear
			// 
			this.cmdClear.Location = new System.Drawing.Point(573, 104);
			this.cmdClear.Name = "cmdClear";
			this.cmdClear.Size = new System.Drawing.Size(73, 23);
			this.cmdClear.TabIndex = 40;
			this.cmdClear.Text = "Clear result";
			this.cmdClear.UseVisualStyleBackColor = true;
			this.cmdClear.Click += new System.EventHandler(this.cmdClear_Click);
			// 
			// cbDVRMode
			// 
			this.cbDVRMode.AutoSize = true;
			this.cbDVRMode.Enabled = false;
			this.cbDVRMode.Location = new System.Drawing.Point(432, 76);
			this.cbDVRMode.Name = "cbDVRMode";
			this.cbDVRMode.Size = new System.Drawing.Size(106, 17);
			this.cbDVRMode.TabIndex = 38;
			this.cbDVRMode.Text = "Standard version";
			this.cbDVRMode.UseVisualStyleBackColor = true;
			// 
			// cmdDisconnect
			// 
			this.cmdDisconnect.Location = new System.Drawing.Point(417, 104);
			this.cmdDisconnect.Name = "cmdDisconnect";
			this.cmdDisconnect.Size = new System.Drawing.Size(69, 23);
			this.cmdDisconnect.TabIndex = 31;
			this.cmdDisconnect.Text = "Disconnect";
			this.cmdDisconnect.UseVisualStyleBackColor = true;
			this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
			// 
			// cmsSetDelay
			// 
			this.cmsSetDelay.Location = new System.Drawing.Point(492, 104);
			this.cmsSetDelay.Name = "cmsSetDelay";
			this.cmsSetDelay.Size = new System.Drawing.Size(77, 23);
			this.cmsSetDelay.TabIndex = 30;
			this.cmsSetDelay.Text = "Timer setting";
			this.cmsSetDelay.UseVisualStyleBackColor = true;
			this.cmsSetDelay.Click += new System.EventHandler(this.cmsSetDelay_Click);
			// 
			// cmsClose
			// 
			this.cmsClose.Location = new System.Drawing.Point(652, 104);
			this.cmsClose.Name = "cmsClose";
			this.cmsClose.Size = new System.Drawing.Size(67, 23);
			this.cmsClose.TabIndex = 20;
			this.cmsClose.Text = "Close";
			this.cmsClose.UseVisualStyleBackColor = true;
			this.cmsClose.Click += new System.EventHandler(this.cmsClose_Click);
			// 
			// txtServerName
			// 
			this.txtServerName.Location = new System.Drawing.Point(79, 106);
			this.txtServerName.Name = "txtServerName";
			this.txtServerName.Size = new System.Drawing.Size(100, 20);
			this.txtServerName.TabIndex = 19;
			this.txtServerName.Text = "SRX-Pro Server";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 109);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Server name";
			// 
			// txtServerID
			// 
			this.txtServerID.Location = new System.Drawing.Point(79, 78);
			this.txtServerID.Name = "txtServerID";
			this.txtServerID.Size = new System.Drawing.Size(100, 20);
			this.txtServerID.TabIndex = 17;
			this.txtServerID.Text = "100-011";
			// 
			// txtUser
			// 
			this.txtUser.Location = new System.Drawing.Point(243, 105);
			this.txtUser.Name = "txtUser";
			this.txtUser.Size = new System.Drawing.Size(100, 20);
			this.txtUser.TabIndex = 16;
			this.txtUser.Text = "I3DVR";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(196, 109);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(29, 13);
			this.label9.TabIndex = 15;
			this.label9.Text = "User";
			// 
			// txtCMSPort
			// 
			this.txtCMSPort.Location = new System.Drawing.Point(433, 45);
			this.txtCMSPort.Name = "txtCMSPort";
			this.txtCMSPort.Size = new System.Drawing.Size(100, 20);
			this.txtCMSPort.TabIndex = 13;
			this.txtCMSPort.Text = "1000";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(350, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(83, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "CMS server port";
			// 
			// txtCMSIP
			// 
			this.txtCMSIP.Location = new System.Drawing.Point(433, 19);
			this.txtCMSIP.Name = "txtCMSIP";
			this.txtCMSIP.Size = new System.Drawing.Size(100, 20);
			this.txtCMSIP.TabIndex = 11;
			this.txtCMSIP.Text = "10.0.0.11";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(350, 23);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "CMS server IP";
			// 
			// cmdStart
			// 
			this.cmdStart.Location = new System.Drawing.Point(350, 105);
			this.cmdStart.Name = "cmdStart";
			this.cmdStart.Size = new System.Drawing.Size(61, 23);
			this.cmdStart.TabIndex = 1;
			this.cmdStart.Text = "Start";
			this.cmdStart.UseVisualStyleBackColor = true;
			this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
			// 
			// txtVersion
			// 
			this.txtVersion.Location = new System.Drawing.Point(243, 75);
			this.txtVersion.Name = "txtVersion";
			this.txtVersion.Size = new System.Drawing.Size(100, 20);
			this.txtVersion.TabIndex = 10;
			this.txtVersion.Text = "1.600 (Simulator)";
			// 
			// txtModel
			// 
			this.txtModel.Location = new System.Drawing.Point(243, 47);
			this.txtModel.Name = "txtModel";
			this.txtModel.Size = new System.Drawing.Size(100, 20);
			this.txtModel.TabIndex = 9;
			this.txtModel.Text = "Model";
			// 
			// txtlocation
			// 
			this.txtlocation.Location = new System.Drawing.Point(243, 19);
			this.txtlocation.Name = "txtlocation";
			this.txtlocation.Size = new System.Drawing.Size(100, 20);
			this.txtlocation.TabIndex = 1;
			this.txtlocation.Text = "Sai Gon";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(196, 82);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(42, 13);
			this.label7.TabIndex = 8;
			this.label7.Text = "Version";
			// 
			// lblModel
			// 
			this.lblModel.AutoSize = true;
			this.lblModel.Location = new System.Drawing.Point(196, 54);
			this.lblModel.Name = "lblModel";
			this.lblModel.Size = new System.Drawing.Size(36, 13);
			this.lblModel.TabIndex = 7;
			this.lblModel.Text = "Model";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(193, 26);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 13);
			this.label5.TabIndex = 6;
			this.label5.Text = "Location";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 81);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Server ID";
			// 
			// lblDVRIP
			// 
			this.lblDVRIP.AutoSize = true;
			this.lblDVRIP.Location = new System.Drawing.Point(82, 54);
			this.lblDVRIP.Name = "lblDVRIP";
			this.lblDVRIP.Size = new System.Drawing.Size(58, 13);
			this.lblDVRIP.TabIndex = 3;
			this.lblDVRIP.Text = "10.0.0.111";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 54);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(51, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Server IP";
			// 
			// lblGUID
			// 
			this.lblGUID.AutoSize = true;
			this.lblGUID.Location = new System.Drawing.Point(76, 26);
			this.lblGUID.Name = "lblGUID";
			this.lblGUID.Size = new System.Drawing.Size(96, 13);
			this.lblGUID.TabIndex = 1;
			this.lblGUID.Text = "A3-49-90-0F-C1-97";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "DVR GUID:";
			// 
			// lstTimerInfo
			// 
			this.lstTimerInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstTimerInfo.FormattingEnabled = true;
			this.lstTimerInfo.Location = new System.Drawing.Point(546, 19);
			this.lstTimerInfo.Name = "lstTimerInfo";
			this.lstTimerInfo.Size = new System.Drawing.Size(201, 82);
			this.lstTimerInfo.TabIndex = 39;
			// 
			// cbViewXMLMessage
			// 
			this.cbViewXMLMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbViewXMLMessage.AutoSize = true;
			this.cbViewXMLMessage.Location = new System.Drawing.Point(428, 5);
			this.cbViewXMLMessage.Name = "cbViewXMLMessage";
			this.cbViewXMLMessage.Size = new System.Drawing.Size(96, 17);
			this.cbViewXMLMessage.TabIndex = 21;
			this.cbViewXMLMessage.Text = "XML  message";
			this.cbViewXMLMessage.UseVisualStyleBackColor = true;
			this.cbViewXMLMessage.CheckedChanged += new System.EventHandler(this.cbViewXMLMessage_CheckedChanged);
			// 
			// DgStatus
			// 
			this.DgStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgStatus.Location = new System.Drawing.Point(0, 0);
			this.DgStatus.Name = "DgStatus";
			this.DgStatus.Size = new System.Drawing.Size(529, 260);
			this.DgStatus.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 132);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(753, 289);
			this.splitContainer1.SplitterDistance = 220;
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.lblQeue);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.lblStatus);
			this.splitContainer3.Panel2.Controls.Add(this.lstMsgQueu);
			this.splitContainer3.Size = new System.Drawing.Size(220, 289);
			this.splitContainer3.SplitterDistance = 25;
			this.splitContainer3.TabIndex = 0;
			// 
			// lblQeue
			// 
			this.lblQeue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblQeue.Location = new System.Drawing.Point(0, 0);
			this.lblQeue.Name = "lblQeue";
			this.lblQeue.Size = new System.Drawing.Size(220, 25);
			this.lblQeue.TabIndex = 3;
			this.lblQeue.Text = "Socket history";
			this.lblQeue.Click += new System.EventHandler(this.lblQeue_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblStatus.Location = new System.Drawing.Point(0, 247);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(220, 13);
			this.lblStatus.TabIndex = 1;
			this.lblStatus.Text = "Status";
			// 
			// lstMsgQueu
			// 
			this.lstMsgQueu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstMsgQueu.FormattingEnabled = true;
			this.lstMsgQueu.Location = new System.Drawing.Point(0, 0);
			this.lstMsgQueu.Name = "lstMsgQueu";
			this.lstMsgQueu.Size = new System.Drawing.Size(220, 225);
			this.lstMsgQueu.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.cbAutoScroll);
			this.splitContainer2.Panel1.Controls.Add(this.label10);
			this.splitContainer2.Panel1.Controls.Add(this.cbViewXMLMessage);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.DgStatus);
			this.splitContainer2.Size = new System.Drawing.Size(529, 289);
			this.splitContainer2.SplitterDistance = 25;
			this.splitContainer2.TabIndex = 3;
			// 
			// cbAutoScroll
			// 
			this.cbAutoScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbAutoScroll.AutoSize = true;
			this.cbAutoScroll.Checked = true;
			this.cbAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbAutoScroll.Location = new System.Drawing.Point(335, 6);
			this.cbAutoScroll.Name = "cbAutoScroll";
			this.cbAutoScroll.Size = new System.Drawing.Size(75, 17);
			this.cbAutoScroll.TabIndex = 22;
			this.cbAutoScroll.Text = "Auto scroll";
			this.cbAutoScroll.UseVisualStyleBackColor = true;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 3);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(40, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Result:";
			// 
			// UCtrlDVR
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.groupBox1);
			this.Name = "UCtrlDVR";
			this.Size = new System.Drawing.Size(753, 421);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DgStatus)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblGUID;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblDVRIP;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtVersion;
		private System.Windows.Forms.TextBox txtModel;
		private System.Windows.Forms.TextBox txtlocation;
		private System.Windows.Forms.Label lblModel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.TextBox txtCMSPort;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtCMSIP;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.DataGridView DgStatus;
		private System.Windows.Forms.TextBox txtUser;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtServerID;
		private System.Windows.Forms.TextBox txtServerName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.Label lblQeue;
		private System.Windows.Forms.ListBox lstMsgQueu;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmsClose;
        private System.Windows.Forms.CheckBox cbViewXMLMessage;
		private System.Windows.Forms.Button cmsSetDelay;
		private System.Windows.Forms.Button cmdDisconnect;
        private System.Windows.Forms.CheckBox cbAutoScroll;
		private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox cbDVRMode;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListBox lstTimerInfo;
        private System.Windows.Forms.Button cmdClear;
	}
}
