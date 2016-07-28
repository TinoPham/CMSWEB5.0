using System;
using System.Text;
using System.Windows.Forms;

namespace CMSTest
{
	public partial class ProgressBarFrm : Form
	{
		public ProgressBarFrm()
		{
			InitializeComponent();
		}

		private bool _isRemove = false;

		public ProgressBarFrm(int maxIndex, int value= 0,  bool isRemove = false)
		{
			InitializeComponent();
			_isRemove = isRemove;
			if (isRemove)
			{
				progressBar.Minimum = 0;
				progressBar.Maximum = maxIndex;
				progressBar.Step = -1;
				progressBar.Value = value;
			}
			else
			{
				progressBar.Minimum = 0;
				progressBar.Maximum = maxIndex;
				progressBar.Step = 1;
				progressBar.Value = 0;
			}

		}

		public void SetProgress()
		{
			progressBar.PerformStep();
			StringBuilder strBuilder = new StringBuilder();
			strBuilder.Append(progressBar.Value);
			strBuilder.Append("/");
			strBuilder.Append(progressBar.Maximum);
			strBuilder.Append("DVRs");
			lblInfo.Text = strBuilder.ToString();
			if (_isRemove)
			{
				if (progressBar.Value == progressBar.Minimum)
					this.Close();
			}
			else
			{
				if (progressBar.Value == progressBar.Maximum)
					this.Close();
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ProgressBarFrm_Load(object sender, EventArgs e)
		{
		
		}
	}
}
