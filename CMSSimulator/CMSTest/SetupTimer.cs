using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CMSTest
{
    public partial class SetupTimer : Form
    {
        TimerSetting m_setting; 
        public TimerSetting Setting
        {
            get { return m_setting; }

        }
        public SetupTimer()
        {
            InitializeComponent();
            m_setting = new TimerSetting();
            ApplyGUI(m_setting);
            EnableGUI(cbDVrMode.Checked);
        }
		public SetupTimer(TimerSetting tmsetting, bool standardversion)
        {
            InitializeComponent();
            m_setting = tmsetting;
            ApplyGUI(m_setting);
            EnableGUI(standardversion);
            cbDVrMode.Checked = standardversion;

        }
        private void ApplyGUI(TimerSetting setting)
        {
            txtTimeOut.Text = setting.m_msg_timeout.ToString();
            txtretry.Text = setting.m_msg_retry.ToString();
            txtreconnect.Text = setting.m_dvr_reconnect.ToString();
            txteventconfig.Text = setting.m_msg_configuration.ToString();
            txteventvideo.Text = setting.m_msg_video.ToString();
            txteventalert.Text = setting.m_msg_alert.ToString();
            txtKeepAlive.Text = setting.m_keep_alive.ToString();
            txtChanelStatus.Text = setting.m_channel_status.ToString();
        }
        private bool GetGUI(ref TimerSetting setting)
        {
            try
            {
                TimerSetting newsetting = new TimerSetting();
                newsetting.m_msg_timeout = Convert.ToInt32(txtTimeOut.Text);
                newsetting.m_msg_retry = Convert.ToInt32(txtretry.Text);
                newsetting.m_dvr_reconnect = Convert.ToInt32(txtreconnect.Text);
                newsetting.m_msg_configuration = Convert.ToInt32(txteventconfig.Text);
                newsetting.m_msg_video = Convert.ToInt32(txteventvideo.Text);
                newsetting.m_msg_alert = Convert.ToInt32(txteventalert.Text);
                newsetting.m_keep_alive = Convert.ToInt32(txtKeepAlive.Text);
                if (newsetting.m_keep_alive < 30)
                {
                    newsetting.m_keep_alive = 30;
                    txtKeepAlive.Text = newsetting.m_keep_alive.ToString();
                }
                newsetting.m_channel_status = Convert.ToInt32(txtChanelStatus.Text);
                setting = newsetting;
                return true;
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid input setting :(.");
                return false;
            }
            
        }
        private void EnableGUI(bool standardversion)
        {
            gbConnection.Enabled = true;
            gbEvents.Enabled = standardversion;
        }
        private void cmdOK_Click(object sender, EventArgs e)
        {
            if( GetGUI(ref m_setting))
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
         
        }

    }
}