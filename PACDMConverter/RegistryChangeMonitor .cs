using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using PACDMConverter.Events;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace PACDMConverter
{
		#region Delegates
	public delegate void RegistryChangeHandler(object sender, RegistryChangeEventArgs e);
	#endregion

	public class RegistryChangeMonitor : IDisposable
	{
		#region Fields
		private string _registryPath;
		private REG_NOTIFY_CHANGE _filter;
		private Thread _monitorThread;
		private RegistryKey _monitorKey;
		private readonly RegistryView _view;
		readonly RegistryHive _Hive;
		#endregion

		#region Imports
		[DllImport("Advapi32.dll")]
		private static extern int RegNotifyChangeKeyValue( IntPtr hKey, bool watchSubtree, REG_NOTIFY_CHANGE notifyFilter, IntPtr hEvent, bool asynchronous);
		#endregion

		#region Enumerations
		[Flags]
		public enum REG_NOTIFY_CHANGE : uint
		{
			NAME = 0x1,
			ATTRIBUTES = 0x2,
			LAST_SET = 0x4,
			SECURITY = 0x8
		}
		#endregion

		#region Constructors
		public RegistryChangeMonitor(RegistryHive Hive, string registryPath, RegistryView view) : this(Hive, registryPath,view, REG_NOTIFY_CHANGE.LAST_SET) { ; }
		public RegistryChangeMonitor(RegistryHive Hive, string registryPath, RegistryView view, REG_NOTIFY_CHANGE filter)
		{
			this._Hive = Hive;
			this._view = view;
			this._registryPath = registryPath.ToUpper();
			this._filter = filter;
		}
		~RegistryChangeMonitor()
		{
			this.Dispose(false);
		}
		#endregion

		#region Methods
		private void Dispose(bool disposing)
		{
			if (disposing)
				GC.SuppressFinalize(this);

			this.Stop();
		}
		public void Dispose()
		{
			this.Dispose(true);
		}
		public void Start()
		{
			lock (this)
			{
				if (this._monitorThread == null)
				{
					ThreadStart ts = new ThreadStart(this.MonitorThread);
					this._monitorThread = new Thread(ts);
					this._monitorThread.IsBackground = true;
				}

				if (!this._monitorThread.IsAlive)
				{
					this._monitorThread.Start();
				}
			}
		}
		public void Stop()
		{
			lock (this)
			{
				this.Changed = null;
				this.Error = null;

				if (this._monitorThread != null)
				{
					this._monitorThread = null;
				}

				// The "Close()" will trigger RegNotifyChangeKeyValue if it is still listening
				if (this._monitorKey != null)
				{
					this._monitorKey.Close();
					this._monitorKey = null;
				}
			}
		}

		private string RegistryHiveName(RegistryHive Hive)
		{
			string ret = null;
			switch(Hive)
			{
				case RegistryHive.ClassesRoot:
					ret = "HKEY_CLASSES_ROOT";
				break;
				case RegistryHive.CurrentConfig:
					ret = "HKEY_CURRENT_CONFIG";
				break;
				case RegistryHive.CurrentUser:
				ret = "HKEY_CURRENT_USER";
				break;
				case RegistryHive.DynData:
				ret = "HKEY_DYN_DATA";
				break;
				case RegistryHive.LocalMachine :
				ret = "HKEY_LOCAL_MACHINE";
				break;
				case RegistryHive.PerformanceData:
				ret = "HKEY_PERFORMANCE_DATA";
				break;
				case RegistryHive.Users:
				ret = "HKEY_USERS";
				break;
			}
			return ret;
		}

		private RegistryKey GetMonitorKey(RegistryHive Hive, string registryPath, RegistryView view)
		{
			
			RegistryKey key = null; //RegistryKey.OpenBaseKey(_Hive, _view).OpenSubKey(registryPath);
			RegistryKey rootkey = RegistryKey.OpenBaseKey(_Hive, _view);
			string current = registryPath;
			int count = 1;
			string[] segments = current.Split('\\');
			while( key == null && rootkey != null)
			{
				key = RegistryKey.OpenBaseKey(_Hive, _view).OpenSubKey(current);
				if( count == segments.Length)
					break;
				current = string.Join("\\", segments,0, segments.Length - count);
				count ++;
			}

			return key;
		}
		private bool MatchPath( RegistryKey key)
		{
			string orgpath = System.IO.Path.Combine(RegistryHiveName(this._Hive), _registryPath );
			return string.Compare( key.ToString(), orgpath) == 0;
		}
		private void MonitorThread()
		{
			try
			{
			IntPtr ptr = IntPtr.Zero;

			lock (this)
			{
				
				_monitorKey = GetMonitorKey(_Hive, _registryPath, _view); //RegistryKey.OpenBaseKey(_Hive,_view).OpenSubKey(_registryPath);

				//if (this._registryPath.StartsWith("HKEY_CLASSES_ROOT"))
				//    this._monitorKey =  Registry.ClassesRoot.OpenSubKey(this._registryPath);
				//else if (this._registryPath.StartsWith("HKCR"))
				//    this._monitorKey = Registry.ClassesRoot.OpenSubKey(this._registryPath.Substring(5));
				//else if (this._registryPath.StartsWith("HKEY_CURRENT_USER"))
				//    this._monitorKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, this._view).OpenSubKey(this._registryPath.Substring(18));//Registry.CurrentUser.OpenSubKey(this._registryPath.Substring(18));
				//else if (this._registryPath.StartsWith("HKCU"))
				//    this._monitorKey = Registry.CurrentUser.OpenSubKey(this._registryPath.Substring(5));
				//else if (this._registryPath.StartsWith("HKEY_LOCAL_MACHINE"))
				//    this._monitorKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, this._view).OpenSubKey(this._registryPath.Substring(19)); //Registry.LocalMachine.OpenSubKey(this._registryPath.Substring(19));
				//else if (this._registryPath.StartsWith("HKLM"))
				//    this._monitorKey = Registry.LocalMachine.OpenSubKey(this._registryPath.Substring(5));
				//else if (this._registryPath.StartsWith("HKEY_USERS"))
				//    this._monitorKey = Registry.Users.OpenSubKey(this._registryPath.Substring(11));
				//else if (this._registryPath.StartsWith("HKU"))
				//    this._monitorKey = Registry.Users.OpenSubKey(this._registryPath.Substring(4));
				//else if (this._registryPath.StartsWith("HKEY_CURRENT_CONFIG"))
				//    this._monitorKey = Registry.CurrentConfig.OpenSubKey(this._registryPath.Substring(20));
				//else if (this._registryPath.StartsWith("HKCC"))
				//    this._monitorKey = Registry.CurrentConfig.OpenSubKey(this._registryPath.Substring(5));

				// Fetch the native handle
				if (this._monitorKey != null)
				{
					object hkey = typeof(RegistryKey).InvokeMember(
						"hkey",
						BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
						null,
						this._monitorKey,
						null
						);

					ptr = (IntPtr)typeof(SafeHandle).InvokeMember(
						"handle",
						BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
						null,
						hkey,
						null);
				}
			}

			if (ptr != IntPtr.Zero)
			{
				while (true)
				{
				// If this._monitorThread is null that probably means Dispose is being called. Don't monitor anymore.
				if ((this._monitorThread == null) || (this._monitorKey == null))
					break;
				// RegNotifyChangeKeyValue blocks until a change occurs.
				int result = RegNotifyChangeKeyValue(ptr, true, this._filter, IntPtr.Zero, false);

				if ((this._monitorThread == null) || (this._monitorKey == null))
					break;

				if (result == 0)
				{
					RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
					this.Changed(this, e);
					if (e.Stop)
						break;
				}
				else
				{
					if (this.Error != null)
					{
					Win32Exception ex = new Win32Exception();

					// Unless the exception is thrown, nobody is nice enough to set a good stacktrace for us. Set it ourselves.
					typeof(Exception).InvokeMember(
					"_stackTrace",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField,
					null,
					ex,
					new object[] { new StackTrace(true) }
					);

					RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
					e.Exception = ex;
					this.Error(this, e);
					}

					break;
				}
				}
			}
			}
			catch (Exception ex)
			{
			if (this.Error != null)
			{
				RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
				e.Exception = ex;
				this.Error(this, e);
			}
			}
			finally
			{
			this.Stop();
			}
		}
		#endregion

		#region Events
		public event RegistryChangeHandler Changed;
		public event RegistryChangeHandler Error;
		#endregion

		#region Properties
		public bool Monitoring
		{
			get
			{
			if (this._monitorThread != null)
				return this._monitorThread.IsAlive;

			return false;
			}
		}
		#endregion
		}
}
