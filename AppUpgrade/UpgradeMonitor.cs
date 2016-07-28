using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
namespace AppUpgrade
{
	internal class UpgradeMonitor : Singleton<UpgradeMonitor>
	{
		
		const string MSG_UPGRADE = "PAC Converter is upgrading...";
		const string MSG_Wating_Converter_Exit = "Wating PAC Covnerter Configuration exited.";
		//const string NOTIFY_UPGRADE = "";//"Please don't turn off your PC until upgrade process has been completed.";
		const string LUNCH_CONFIG_TOOL = "Lunching PAC Converter configuration tool.";

		const string Cmd_Old_Version = "-o";
		const string Cmd_New_Version = "-n";
		const string Cmd_Start = "-s";
		const string Cmd_PID = "-P";
		const string Cmd_Owner ="-own";
		volatile int UpgradeID = 0;
		ManualResetEvent monitor_file_event;
		readonly TimeSpan Wait_Timeout = new TimeSpan(0, 2 ,0);
		private UpgradeMonitor()
		{
			monitor_file_event = new ManualResetEvent(false);
			monitor_file_event.Reset();
			Pipes.PipeServer pipe_server = new Pipes.PipeServer();
			pipe_server.PipeMessage +=new Pipes.DelegateMessage(pipe_server_PipeMessage);

			pipe_server.Listen(Utils.Pipe_Name);
		}

		private void pipe_server_PipeMessage(string message)
		{
			int upgradeapp = 0;
			Int32.TryParse(message, out upgradeapp);
			UpgradeID = upgradeapp;

			monitor_file_event.Set();
			
		}
		public void UpgradeMonitorCommand( string[] args) 
		{
			Console.WriteLine(MSG_UPGRADE);
			string strpid = GetCommand(Cmd_PID, args);
			if (!string.IsNullOrEmpty(strpid))
			{
				Process p = GetProcessbyID(strpid);
				if( p != null)
				{
					Console.WriteLine(MSG_Wating_Converter_Exit);
					p.WaitForExit();
				}
			}
			string o_version = GetCommand(Cmd_Old_Version, args);
			string n_version = GetCommand(Cmd_New_Version, args);
			string call_app = GetCommand(Cmd_Start, args);
			if (!File.Exists(call_app))
				return;
			
			
			Console.WriteLine("Upgrading from {0} to {1}", o_version, n_version);
			//Console.WriteLine(NOTIFY_UPGRADE);

			monitor_file_event.WaitOne(Wait_Timeout);
			
			WaitUpgradeProcess(UpgradeID);
			Process p_current = Process.GetCurrentProcess();
			string apppath = p_current.MainModule.ModuleName;
			Process.Start(call_app, string.Format("-own \"{0}\"", apppath));
		}

		private string GetCommand(string command, string[] args)
		{
			if( args == null || args.Length == 0)
				return null;
			string pram = null;
			for( int i = 0; i < args.Length; i+= 2 )
			{
				if( string.Compare( args[i], command, true) != 0)
					continue;

				if( i + 1 < args.Length)
					pram = args[i +1];
				break;
			}
			return pram;
		}
		
		private void WaitUpgradeProcess( int pid)
		{
			if( pid == 0)
				return;
			try
			{
				Process p = Process.GetProcessById(pid);
				while( p != null)
				{
					Thread.Sleep(1000);
					p = Process.GetProcessById(pid);
				}
			}
			catch(Exception){}
		}

		private Process GetProcessbyID(string pid)
		{
			if( string.IsNullOrEmpty(pid))
				return null;
			int _pid = 0;
			Int32.TryParse(pid, out _pid);
			return GetProcessbyID(_pid);
		}
	
		private Process GetProcessbyID(int pid)
		{
			try
			{
				if(pid <= 0)
					return null;
				return Process.GetProcessById(pid);
			}
			catch(Exception){ return null;}
		}
	}
}
