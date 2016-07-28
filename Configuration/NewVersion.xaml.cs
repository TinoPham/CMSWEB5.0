using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Windows.Shell;
using System.Windows.Interop;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for NewVersion.xaml
	/// </summary>
	public partial class NewVersion : Window
	{
		public string AppVersion { get; private set; }
		public NewVersion()
		{
			InitializeComponent();
			this.DataContext = this;
		}
		public NewVersion( string vername): this()
		{
			AppVersion = string.Format("The new version {0} has been upgraded.", vername);
		}
		public void move_window(object sender, MouseButtonEventArgs e)
		{
			MainWindow.ReleaseCapture();
			MainWindow.SendMessage(new WindowInteropHelper(this).Handle, MainWindow.WM_NCLBUTTONDOWN, MainWindow.HT_CAPTION, 0);
		}
		private void frmNewVersion_LostFocus(object sender, RoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void frmNewVersion_Deactivated(object sender, EventArgs e)
		{
				SystemCommands.CloseWindow(this);
		}
		
	}
}
