using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace CMSTest
{
	public partial class frmLoading : Form
	{
		public const int MSG_UPDATE_STATUS_ID = 0x0400 + 100;
		public const int MSG_CANCEL_ADD_ITEMS = 0x0400 + 101;
		int m_current = 0;
		IntPtr m_parentHND = IntPtr.Zero;
		string m_status = string.Empty;
		System.Threading.ManualResetEvent m_waitevent;
		public frmLoading()
		{
			InitializeComponent();
		}
		public frmLoading(IntPtr phnd,string strstatus, int total)
		{
			InitializeComponent();
			m_status = strstatus;
			m_current = 0;
			pMain.Minimum = m_current;
			pMain.Maximum = total;
			pMain.Step = 1;
			lblStatus.Text = m_status +  ": " + m_current.ToString() + "/" + total.ToString() + " DVR(s).";
			m_parentHND = phnd;
		}
		public frmLoading(IntPtr phnd,System.Threading.ManualResetEvent startevent,  string strstatus, int total)
		{
			InitializeComponent();
			m_waitevent = startevent;
			if (m_waitevent == null)
				m_waitevent = new ManualResetEvent(false);

			m_waitevent.Reset();
			m_status = strstatus;
			m_current = 0;
			pMain.Minimum = m_current;
			pMain.Maximum = total;
			pMain.Step = 1;
			lblStatus.Text = m_status + ": " + m_current.ToString() + "/" + total.ToString() + " DVR(s).";
			m_parentHND = phnd;
		}
		private void LableUpdateStatus(int step, int total)
		{
			m_current += step;
			pMain.Value = m_current;
			lblStatus.Text = m_status +  ": " + m_current.ToString() + "/" + total.ToString() + " DVR(s).";		
		}
		protected override void WndProc(ref Message m)
		{
			if( m.Msg == MSG_UPDATE_STATUS_ID )
			{
				LableUpdateStatus(m.LParam.ToInt32(), m.WParam.ToInt32());
			}
			base.WndProc(ref m);
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			cmdCancel.Enabled = false;
			if( m_parentHND != IntPtr.Zero)
			{
				WIN32API.SendMessage(m_parentHND, MSG_CANCEL_ADD_ITEMS, IntPtr.Zero, IntPtr.Zero);
			}
			cmdCancel.Enabled = true;
		}

		private void frmLoading_Load(object sender, EventArgs e)
		{
			m_waitevent.Set();
		}
	}
}