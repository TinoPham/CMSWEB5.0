using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Principal;

namespace AppUpgrade
{
	class Program
	{
		//static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
		static EventWaitHandle s_event;
		const string TITLE = "PAC Converter Upgrading";
		[STAThread]
		static void Main(string[] args)
		{
			bool created;
			WindowsIdentity iuser = WindowsIdentity.GetCurrent();
			s_event = new EventWaitHandle(false, EventResetMode.ManualReset, iuser.IsSystem? "System_" : "Users_" + "8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F", out created);
			if (created)
			{
				try
				{
					if(iuser.IsSystem)
					{
						//bool waitting = true;
						//while (waitting)
						//{
						//    System.Threading.Thread.Sleep(10000);
						//}

						Pipes.PipeClient pipe_client = new Pipes.PipeClient();
						int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
						pipe_client.Send( pid.ToString(), Utils.Pipe_Name);
						ExcuteCommand.ExcuteUpgradeCommand(args);
					}
					else
					{
#if !DEBUG
						Win32Service.DeleteMenu(Win32Service.GetSystemMenu(Win32Service.GetConsoleWindow(), false), Win32Service.SC_CLOSE, Win32Service.MF_BYCOMMAND);
#endif
						//bool waitting = true;
						//while (waitting)
						//{
						//    System.Threading.Thread.Sleep(10000);
						//}
						Console.Title = Program.TITLE;
						UpgradeMonitor.Instance.UpgradeMonitorCommand(args);
					}

				}
				catch (Exception) { }

			}
			
			//else
			//{
			//    //WindowsIdentity iuser = WindowsIdentity.GetCurrent();
			//    Console.WriteLine(iuser.IsSystem);
			//}


			//if (mutex.WaitOne(TimeSpan.Zero, true))
			//{
			//    //System.Threading.Thread.Sleep(7000);
			//    try{
			//        ExcuteCommand.ExcuteUpgradeCommand(args);
			//    }
			//    catch(Exception){}

			//    mutex.ReleaseMutex();
			//}
			//else
			//{
			//    WindowsIdentity iuser = WindowsIdentity.GetCurrent();
				
			//    Console.WriteLine(iuser.IsSystem);
			//}
			//Console.ReadKey();


			//Win32Service.ServiceInfo svrinfo = Win32Service.QueryServiceInfo("PACDM Converter");

			//Win32Service.StopService("PACDM Converter");
			//Win32Service.UnInstallService("PACDM Converter");
			//Win32Service.InstallService(@"D:\PAC\PACDMConverter\PACDMConverter.exe", "PACDM Converter");
			//Win32Service.StartService("PACDM Converter");
		}

		
	}
}
