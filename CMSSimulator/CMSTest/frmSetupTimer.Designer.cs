namespace CMSTest
{
    partial class frmSetupTimer
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
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txtKeepAlive = new System.Windows.Forms.MaskedTextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.txtreconnect = new System.Windows.Forms.MaskedTextBox();
            this.txtretry = new System.Windows.Forms.MaskedTextBox();
            this.txtTimeOut = new System.Windows.Forms.MaskedTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gbEvents = new System.Windows.Forms.GroupBox();
            this.txteventalert = new System.Windows.Forms.MaskedTextBox();
            this.txteventvideo = new System.Windows.Forms.MaskedTextBox();
            this.txteventconfig = new System.Windows.Forms.MaskedTextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cbDVrMode = new System.Windows.Forms.CheckBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.txtChanelStatus = new System.Windows.Forms.MaskedTextBox();
            this.labelChannelStatusMin = new System.Windows.Forms.Label();
            this.labelChannelStatus = new System.Windows.Forms.Label();
            this.gbConnection.SuspendLayout();
            this.gbEvents.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.txtChanelStatus);
            this.gbConnection.Controls.Add(this.labelChannelStatusMin);
            this.gbConnection.Controls.Add(this.labelChannelStatus);
            this.gbConnection.Controls.Add(this.txtKeepAlive);
            this.gbConnection.Controls.Add(this.label13);
            this.gbConnection.Controls.Add(this.label14);
            this.gbConnection.Controls.Add(this.txtreconnect);
            this.gbConnection.Controls.Add(this.txtretry);
            this.gbConnection.Controls.Add(this.txtTimeOut);
            this.gbConnection.Controls.Add(this.label6);
            this.gbConnection.Controls.Add(this.label5);
            this.gbConnection.Controls.Add(this.label4);
            this.gbConnection.Controls.Add(this.label3);
            this.gbConnection.Controls.Add(this.label2);
            this.gbConnection.Controls.Add(this.label1);
            this.gbConnection.Location = new System.Drawing.Point(12, 35);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(248, 186);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "Communication";
            // 
            // txtKeepAlive
            // 
            this.txtKeepAlive.Location = new System.Drawing.Point(121, 122);
            this.txtKeepAlive.Mask = "00000";
            this.txtKeepAlive.Name = "txtKeepAlive";
            this.txtKeepAlive.Size = new System.Drawing.Size(66, 20);
            this.txtKeepAlive.TabIndex = 13;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(192, 125);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "second(s)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(16, 125);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(57, 13);
            this.label14.TabIndex = 11;
            this.label14.Text = "Keep alive";
            // 
            // txtreconnect
            // 
            this.txtreconnect.Location = new System.Drawing.Point(121, 84);
            this.txtreconnect.Mask = "00000";
            this.txtreconnect.Name = "txtreconnect";
            this.txtreconnect.Size = new System.Drawing.Size(66, 20);
            this.txtreconnect.TabIndex = 10;
            // 
            // txtretry
            // 
            this.txtretry.Location = new System.Drawing.Point(121, 54);
            this.txtretry.Mask = "00000";
            this.txtretry.Name = "txtretry";
            this.txtretry.Size = new System.Drawing.Size(66, 20);
            this.txtretry.TabIndex = 9;
            // 
            // txtTimeOut
            // 
            this.txtTimeOut.Location = new System.Drawing.Point(121, 23);
            this.txtTimeOut.Mask = "00000";
            this.txtTimeOut.Name = "txtTimeOut";
            this.txtTimeOut.Size = new System.Drawing.Size(66, 20);
            this.txtTimeOut.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(192, 87);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "minute(s)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(193, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "minute(s)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(192, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "minute(s)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Reconnect after";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Retry send timeout";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Timeout message";
            // 
            // gbEvents
            // 
            this.gbEvents.Controls.Add(this.txteventalert);
            this.gbEvents.Controls.Add(this.txteventvideo);
            this.gbEvents.Controls.Add(this.txteventconfig);
            this.gbEvents.Controls.Add(this.label11);
            this.gbEvents.Controls.Add(this.label12);
            this.gbEvents.Controls.Add(this.label9);
            this.gbEvents.Controls.Add(this.label10);
            this.gbEvents.Controls.Add(this.label7);
            this.gbEvents.Controls.Add(this.label8);
            this.gbEvents.Location = new System.Drawing.Point(280, 35);
            this.gbEvents.Name = "gbEvents";
            this.gbEvents.Size = new System.Drawing.Size(248, 157);
            this.gbEvents.TabIndex = 1;
            this.gbEvents.TabStop = false;
            this.gbEvents.Text = "Time inteval Events";
            // 
            // txteventalert
            // 
            this.txteventalert.Location = new System.Drawing.Point(120, 87);
            this.txteventalert.Mask = "00000";
            this.txteventalert.Name = "txteventalert";
            this.txteventalert.Size = new System.Drawing.Size(66, 20);
            this.txteventalert.TabIndex = 17;
            // 
            // txteventvideo
            // 
            this.txteventvideo.Location = new System.Drawing.Point(121, 54);
            this.txteventvideo.Mask = "00000";
            this.txteventvideo.Name = "txteventvideo";
            this.txteventvideo.Size = new System.Drawing.Size(66, 20);
            this.txteventvideo.TabIndex = 16;
            // 
            // txteventconfig
            // 
            this.txteventconfig.Location = new System.Drawing.Point(120, 27);
            this.txteventconfig.Mask = "00000";
            this.txteventconfig.Name = "txteventconfig";
            this.txteventconfig.Size = new System.Drawing.Size(66, 20);
            this.txteventconfig.TabIndex = 11;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(192, 90);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(49, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "minute(s)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 91);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "Alert";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(192, 60);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "minute(s)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(15, 60);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Video";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "minute(s)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 30);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Configuration";
            // 
            // cbDVrMode
            // 
            this.cbDVrMode.AutoSize = true;
            this.cbDVrMode.Enabled = false;
            this.cbDVrMode.Location = new System.Drawing.Point(12, 12);
            this.cbDVrMode.Name = "cbDVrMode";
            this.cbDVrMode.Size = new System.Drawing.Size(106, 17);
            this.cbDVrMode.TabIndex = 2;
            this.cbDVrMode.Text = "Standard version";
            this.cbDVrMode.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(453, 198);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 3;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(362, 198);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 4;
            this.cmdOK.Text = "Save";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // txtChanelStatus
            // 
            this.txtChanelStatus.Location = new System.Drawing.Point(121, 151);
            this.txtChanelStatus.Mask = "00000";
            this.txtChanelStatus.Name = "txtChanelStatus";
            this.txtChanelStatus.Size = new System.Drawing.Size(66, 20);
            this.txtChanelStatus.TabIndex = 16;
            // 
            // labelChannelStatusMin
            // 
            this.labelChannelStatusMin.AutoSize = true;
            this.labelChannelStatusMin.Location = new System.Drawing.Point(192, 154);
            this.labelChannelStatusMin.Name = "labelChannelStatusMin";
            this.labelChannelStatusMin.Size = new System.Drawing.Size(49, 13);
            this.labelChannelStatusMin.TabIndex = 15;
            this.labelChannelStatusMin.Text = "minute(s)";
            // 
            // labelChannelStatus
            // 
            this.labelChannelStatus.AutoSize = true;
            this.labelChannelStatus.Location = new System.Drawing.Point(16, 154);
            this.labelChannelStatus.Name = "labelChannelStatus";
            this.labelChannelStatus.Size = new System.Drawing.Size(77, 13);
            this.labelChannelStatus.TabIndex = 14;
            this.labelChannelStatus.Text = "Channel status";
            // 
            // frmSetupTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 227);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cbDVrMode);
            this.Controls.Add(this.gbEvents);
            this.Controls.Add(this.gbConnection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSetupTimer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timer seting";
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.gbEvents.ResumeLayout(false);
            this.gbEvents.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.GroupBox gbEvents;
        private System.Windows.Forms.CheckBox cbDVrMode;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.MaskedTextBox txtTimeOut;
        private System.Windows.Forms.MaskedTextBox txtreconnect;
        private System.Windows.Forms.MaskedTextBox txtretry;
        private System.Windows.Forms.MaskedTextBox txteventalert;
        private System.Windows.Forms.MaskedTextBox txteventvideo;
        private System.Windows.Forms.MaskedTextBox txteventconfig;
        private System.Windows.Forms.MaskedTextBox txtKeepAlive;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.MaskedTextBox txtChanelStatus;
        private System.Windows.Forms.Label labelChannelStatusMin;
        private System.Windows.Forms.Label labelChannelStatus;
    }
}