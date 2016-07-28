using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;

namespace ServiceConfig
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		const string Cmd_Own = "-own";
		public bool IsWarningVersion{ get; private set;}
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			IsWarningVersion = true;
			Process proc = Process.GetCurrentProcess();
			int count = Process.GetProcesses().Where(p => p.ProcessName == proc.ProcessName).Count();
			if (count > 1)
			{
				MessageBox.Show("Already an instance of Converter Configuration is running...");
				App.Current.Shutdown();
			}
			if(e.Args != null && e.Args.Length > 0)
			{
				int idex = Array.FindIndex<string>(e.Args, delegate(string val){ return string.Compare( val,Cmd_Own, true) == 0;});
				if(idex >=0 && (idex + 1) < e.Args.Length)
				{
					string caller = e.Args[idex + 1];
					IsWarningVersion = string.Compare(ServiceConfig.MainWindow.APP_Upgrade, caller, true) != 0;
				}
			}
		}
	}
}
