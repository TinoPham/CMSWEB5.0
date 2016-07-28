using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMSTest
{
	public partial class MenuSimulator : Form
	{
		public MenuSimulator()
		{
			InitializeComponent();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnConSimulator_Click(object sender, EventArgs e)
		{
			ConverterSimulators obj = new ConverterSimulators();
			this.Hide();
			if (obj.ShowDialog(this) == DialogResult.Cancel)
			{
				this.Show();
			}
		}

		private void btnDVRSimulator_Click(object sender, EventArgs e)
		{
			//frmMain obj = new frmMain();
			//this.Hide();
			//if (obj.ShowDialog(this) == DialogResult.Cancel)
			//{
			//	this.Show();
			//}
		}
	}
}
