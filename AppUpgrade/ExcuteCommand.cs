using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace AppUpgrade
{
	internal class ExcuteCommand
	{
		const string MSIEXEC = "msiexec.exe";
		const string Upgrade_Arguments = "/quiet /i\"{0}\" ALLUSERS=\"1\" TARGETDIR=\"{1}\" silent=1 /lv+ \"{2}\" ";
		const string Upgrade_Arguments_Default = "/quiet /i\"{0}\" ALLUSERS=\"1\" silent=1 /l* \"{2}\" ";
		const string Cmd_Stop_service = "-s";
		const string Cmd_Install = "-i";
		const string Cmd_KillProcessID="-k";
		const string Log_Extension = ".log";
		const string Msi_Extension = ".msi";
		public static bool ExcuteUpgradeCommand(string[] args)
		{
			Dictionary<string, string> Arguments = ParserCommand(args);
			if(Arguments == null || args.Length == 0)
				return false;
			KeyValuePair<string, string>cmd = Arguments.FirstOrDefault( it => string.Compare(it.Key, Cmd_Stop_service, true) == 0);
			string svrName= string.Empty;
			if(!string.IsNullOrEmpty( cmd.Value))
			{
				ForceStopService( cmd.Value);
				svrName = cmd.Value;
			}

			cmd = Arguments.FirstOrDefault(it => string.Compare(it.Key, Cmd_KillProcessID, true) == 0);
			if (!string.IsNullOrEmpty(cmd.Value))
			{
				int pid = 0;
				Int32.TryParse(cmd.Value,out pid);
				if(pid > 0)
					KillProcess(pid);
				

			}

			cmd = Arguments.FirstOrDefault(it => string.Compare(it.Key, Cmd_Install, true) == 0);

			if(!string.IsNullOrEmpty(cmd.Value) && File.Exists(cmd.Value))
			{
				string targetdir = InstallDir( svrName);
				string logfile = LogFile(cmd.Value);

				Process p = ExcuteInstall( cmd.Value, targetdir, logfile);
				if(p != null)
					p.WaitForExit();
				CleanUp(logfile, cmd.Value);

			}
			return true;
		}

		private static string LogFile( string msipath)
		{
			if( string.IsNullOrEmpty(msipath))
				return DateTime.Now.Ticks.ToString() + Log_Extension;
			try
			{
				string logname = Path.GetFileNameWithoutExtension(msipath) + Log_Extension;
				string path = Path.GetDirectoryName(msipath);
				return Path.Combine(path, logname);
			}
			catch(Exception){
				return DateTime.Now.Ticks.ToString() + Log_Extension;
			}
		}
	
		private static string InstallDir( string servicename)
		{
			try
			{
				Win32Service.ServiceInfo svrinfo = Win32Service.QueryServiceInfo(servicename);
				return System.IO.Path.GetDirectoryName(svrinfo.binaryPathName);
			}
			catch(Exception ){ return null;}
		}
	
		private static Dictionary<string, string> ParserCommand(string[] args)
		{
			Dictionary<string, string> Arguments = null;
			Arguments = new Dictionary<string,string>();
			for (int i = 0; i < args.Length; i += 2)
			{
				if( i +1 >= args.Length)
					break;
				Arguments.Add(args[i], args[i +1]);
			}
		return Arguments;
		}

		private static bool ForceStopService( string servicename)
		{
			Win32Service.SERVICE_STATE state = Win32Service.GetServiceState(servicename);
			if( state == Win32Service.SERVICE_STATE.SERVICE_STOPPED)
				return true;
			int cmd = Win32Service.StopService(servicename);

			return cmd == 0;
		}

		private static void KillProcess(int id)
		{
			if( id > 0)
			{
				try
				{
					Process p = Process.GetProcessById(id);
					if(p != null)
						p.Kill();
				}
				catch(Exception){}
			}
		}

		private static Process ExcuteInstall( string msipath, string targetdir, string logpath)
		{
			if (!File.Exists(msipath))
				return null;
			ProcessStartInfo p = new ProcessStartInfo();
			p.FileName = MSIEXEC;
			p.Arguments = string.Format(Upgrade_Arguments, msipath, targetdir, logpath);
			return Process.Start(p); 
		}

		private static void CleanUp( string logfile, string msipath)
		{
			
			string dir = Path.GetDirectoryName(msipath);
			DirectoryInfo dinfo = new DirectoryInfo(dir);
			IEnumerable<FileInfo> finfos = GetFilesByExtensions( dinfo, Log_Extension, Msi_Extension);
			string lname = Path.GetFileName(logfile);
			string msiname = Path.GetFileName(msipath);
			foreach( FileInfo finfo in finfos)
			{
				if (string.Compare(finfo.Name, lname, true) == 0 || string.Compare(finfo.Name, msiname, true) == 0)
					continue;

				DeleteFile(finfo.FullName);
			}
		}
		static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
		{
			if (extensions == null)
				return Enumerable.Empty<FileInfo>();

			IEnumerable<FileInfo> files = dir.EnumerateFiles();
			return files.Where(f => extensions.Contains(f.Extension));
		}
		private static void DeleteFile( string path)
		{
			if( string.IsNullOrEmpty(path))
				return;
				try{
					File.Delete(path);
				}
				catch(Exception){}
					
		}

	}
}
