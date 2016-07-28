namespace CMSTest
{
	partial class MenuSimulator
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
			this.btnConSimulator = new System.Windows.Forms.Button();
			this.btnDVRSimulator = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnConSimulator
			// 
			this.btnConSimulator.Location = new System.Drawing.Point(29, 21);
			this.btnConSimulator.Name = "btnConSimulator";
			this.btnConSimulator.Size = new System.Drawing.Size(157, 40);
			this.btnConSimulator.TabIndex = 0;
			this.btnConSimulator.Text = "Converter Simulator";
			this.btnConSimulator.UseVisualStyleBackColor = true;
			this.btnConSimulator.Click += new System.EventHandler(this.btnConSimulator_Click);
			// 
			// btnDVRSimulator
			// 
			this.btnDVRSimulator.Location = new System.Drawing.Point(29, 87);
			this.btnDVRSimulator.Name = "btnDVRSimulator";
			this.btnDVRSimulator.Size = new System.Drawing.Size(157, 37);
			this.btnDVRSimulator.TabIndex = 1;
			this.btnDVRSimulator.Text = "DVR Simulator";
			this.btnDVRSimulator.UseVisualStyleBackColor = true;
			this.btnDVRSimulator.Click += new System.EventHandler(this.btnDVRSimulator_Click);
			// 
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(68, 145);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(81, 27);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// MenuSimulator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(217, 189);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnDVRSimulator);
			this.Controls.Add(this.btnConSimulator);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(233, 227);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(233, 227);
			this.Name = "MenuSimulator";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MenuSimulator";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnConSimulator;
		private System.Windows.Forms.Button btnDVRSimulator;
		private System.Windows.Forms.Button btnClose;
	}
}