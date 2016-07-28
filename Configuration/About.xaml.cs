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
using System.Windows.Interop;
using Microsoft.Windows.Shell;
using Commons;
using Microsoft.Win32;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : Window
	{
		public string AppVersion{ get; private set;}
		public About()
		{
			InitializeComponent();
			this.DataContext = this;
			AppVersion = string.Format("Version: " + AppInfo.Instance.AppVersion);
		}
		public void move_window(object sender, MouseButtonEventArgs e)
		{
			MainWindow.ReleaseCapture();
			MainWindow.SendMessage(new WindowInteropHelper(this).Handle, MainWindow.WM_NCLBUTTONDOWN, MainWindow.HT_CAPTION, 0);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}
	}
	public class AppInfo: Commons.SingletonClassBase<AppInfo>
	{
		private const string STR_SOFTWARE = "SOFTWARE";
		private const string STR_PAC = "PAC";
		private const string STR_PACConverterVer = "PACConverterVer";
		private const string PACConverterVerOld = "PACConverterVerOld";

		public string AppVersion{ get ;private set;}

		public string AppOldVersion{ get ;private set;}
		public bool IsNewUpgrade{ get {
			if( string.IsNullOrEmpty(AppOldVersion))
				return true;
			return string.Compare( AppVersion, AppOldVersion, true) != 0;
		}}

		AppInfo()
		{
			LoadVersion();
		}

		private void LoadVersion()
		{
			AppVersion = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, System.IO.Path.Combine(STR_SOFTWARE, STR_PAC), STR_PACConverterVer);
			AppOldVersion = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, System.IO.Path.Combine(STR_SOFTWARE, STR_PAC), PACConverterVerOld);
			if (IsNewUpgrade)
				RegistryUtils.SetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, System.IO.Path.Combine(STR_SOFTWARE, STR_PAC), PACConverterVerOld, AppVersion);
			
		}
	}
}
