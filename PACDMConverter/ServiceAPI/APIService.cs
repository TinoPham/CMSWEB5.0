using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Security.Principal;
using System.Diagnostics;

namespace ServiceManager
{
	class APIService
	{
		private const int SERVICE_NO_CHANGE = 0xFFFF;
		internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
		internal const int SC_STATUS_PROCESS_INFO = 0;
		#region API Structures
		public struct ServiceInfo
		{
			public int serviceType;
			public int startType;
			public int errorControl;
			public string binaryPathName;
			public string loadOrderGroup;
			public int tagID;
			public string dependencies;
			public string startName;
			public string displayName;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct QueryServiceConfigStruct
		{
			public int serviceType;
			public int startType;
			public int errorControl;
			public IntPtr binaryPathName;
			public IntPtr loadOrderGroup;
			public int tagID;
			public IntPtr dependencies;
			public IntPtr startName;
			public IntPtr displayName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SERVICE_STATUS
		{
			public int serviceType;
			public int currentState;
			public int controlsAccepted;
			public int win32ExitCode;
			public int serviceSpecificExitCode;
			public int checkPoint;
			public int waitHint;
		}

		[StructLayoutAttribute(LayoutKind.Sequential)]
		struct SECURITY_DESCRIPTOR
		{
			public byte revision;
			public byte size;
			public short control;
			public IntPtr owner;
			public IntPtr group;
			public IntPtr sacl;
			public IntPtr dacl;
		}


		[StructLayout(LayoutKind.Sequential)]
		internal sealed class SERVICE_STATUS_PROCESS
		{
			[MarshalAs(UnmanagedType.U4)]
			public uint dwServiceType;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwCurrentState;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwControlsAccepted;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwWin32ExitCode;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwServiceSpecificExitCode;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwCheckPoint;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwWaitHint;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwProcessId;
			[MarshalAs(UnmanagedType.U4)]
			public uint dwServiceFlags;
		}
		#endregion

		#region API enums
		[Flags]
		enum SECURITY_INFORMATION : uint
		{
			OWNER_SECURITY_INFORMATION = 0x00000001,
			GROUP_SECURITY_INFORMATION = 0x00000002,
			DACL_SECURITY_INFORMATION = 0x00000004,
			SACL_SECURITY_INFORMATION = 0x00000008,
			UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
			UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
			PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
			PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
		}

		public enum SERVICE_START : uint
		{
			/// <summary>
			/// A device driver started by the system loader. This value is valid
			/// only for driver services.
			/// </summary>
			SERVICE_BOOT_START = 0x00000000,

			/// <summary>
			/// A device driver started by the IoInitSystem function. This value 
			/// is valid only for driver services.
			/// </summary>
			SERVICE_SYSTEM_START = 0x00000001,

			/// <summary>
			/// A service started automatically by the service control manager 
			/// during system startup. For more information, see Automatically 
			/// Starting Services.
			/// </summary>         
			SERVICE_AUTO_START = 0x00000002,

			/// <summary>
			/// A service started by the service control manager when a process 
			/// calls the StartService function. For more information, see 
			/// Starting Services on Demand.
			/// </summary>
			SERVICE_DEMAND_START = 0x00000003,

			/// <summary>
			/// A service that cannot be started. Attempts to start the service
			/// result in the error code ERROR_SERVICE_DISABLED.
			/// </summary>
			SERVICE_DISABLED = 0x00000004,
		}

		enum SERVICE_CONTROL : uint
		{
			STOP = 0x00000001,
			PAUSE = 0x00000002,
			CONTINUE = 0x00000003,
			INTERROGATE = 0x00000004,
			SHUTDOWN = 0x00000005,
			PARAMCHANGE = 0x00000006,
			NETBINDADD = 0x00000007,
			NETBINDREMOVE = 0x00000008,
			NETBINDENABLE = 0x00000009,
			NETBINDDISABLE = 0x0000000A,
			DEVICEEVENT = 0x0000000B,
			HARDWAREPROFILECHANGE = 0x0000000C,
			POWEREVENT = 0x0000000D,
			SESSIONCHANGE = 0x0000000E
		}

		public enum SERVICE_STATE : uint
		{
			SERVICE_STOPPED = 0x00000001,
			SERVICE_START_PENDING = 0x00000002,
			SERVICE_STOP_PENDING = 0x00000003,
			SERVICE_RUNNING = 0x00000004,
			SERVICE_CONTINUE_PENDING = 0x00000005,
			SERVICE_PAUSE_PENDING = 0x00000006,
			SERVICE_PAUSED = 0x00000007
		}

		enum SERVICE_ACCEPT : uint
		{
			STOP = 0x00000001,
			PAUSE_CONTINUE = 0x00000002,
			SHUTDOWN = 0x00000004,
			PARAMCHANGE = 0x00000008,
			NETBINDCHANGE = 0x00000010,
			HARDWAREPROFILECHANGE = 0x00000020,
			POWEREVENT = 0x00000040,
			SESSIONCHANGE = 0x00000080,
		}

		enum SCM_ACCESS : uint
		{
			/// <summary>
			/// Required to connect to the service control manager.
			/// </summary>
			SC_MANAGER_CONNECT = 0x00001,

			/// <summary>
			/// Required to call the CreateService function to create a service
			/// object and add it to the database.
			/// </summary>
			SC_MANAGER_CREATE_SERVICE = 0x00002,

			/// <summary>
			/// Required to call the EnumServicesStatusEx function to list the 
			/// services that are in the database.
			/// </summary>
			SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

			/// <summary>
			/// Required to call the LockServiceDatabase function to acquire a 
			/// lock on the database.
			/// </summary>
			SC_MANAGER_LOCK = 0x00008,

			/// <summary>
			/// Required to call the QueryServiceLockStatus function to retrieve 
			/// the lock status information for the database.
			/// </summary>
			SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

			/// <summary>
			/// Required to call the NotifyBootConfigStatus function.
			/// </summary>
			SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

			/// <summary>
			/// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access 
			/// rights in this table.
			/// </summary>
			SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
				SC_MANAGER_CONNECT |
				SC_MANAGER_CREATE_SERVICE |
				SC_MANAGER_ENUMERATE_SERVICE |
				SC_MANAGER_LOCK |
				SC_MANAGER_QUERY_LOCK_STATUS |
				SC_MANAGER_MODIFY_BOOT_CONFIG,

			GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
				SC_MANAGER_ENUMERATE_SERVICE |
				SC_MANAGER_QUERY_LOCK_STATUS,

			GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
				SC_MANAGER_CREATE_SERVICE |
				SC_MANAGER_MODIFY_BOOT_CONFIG,

			GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
				SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

			GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
		}

		enum SERVICE_ERROR
		{
			/// <summary>
			/// The startup program ignores the error and continues the startup
			/// operation.
			/// </summary>
			SERVICE_ERROR_IGNORE = 0x00000000,

			/// <summary>
			/// The startup program logs the error in the event log but continues
			/// the startup operation.
			/// </summary>
			SERVICE_ERROR_NORMAL = 0x00000001,

			/// <summary>
			/// The startup program logs the error in the event log. If the 
			/// last-known-good configuration is being started, the startup 
			/// operation continues. Otherwise, the system is restarted with 
			/// the last-known-good configuration.
			/// </summary>
			SERVICE_ERROR_SEVERE = 0x00000002,

			/// <summary>
			/// The startup program logs the error in the event log, if possible.
			/// If the last-known-good configuration is being started, the startup
			/// operation fails. Otherwise, the system is restarted with the 
			/// last-known good configuration.
			/// </summary>
			SERVICE_ERROR_CRITICAL = 0x00000003,
		}

		enum ACCESS_MASK : uint
		{
			DELETE = 0x00010000,
			READ_CONTROL = 0x00020000,
			WRITE_DAC = 0x00040000,
			WRITE_OWNER = 0x00080000,
			SYNCHRONIZE = 0x00100000,

			STANDARD_RIGHTS_REQUIRED = 0x000f0000,

			STANDARD_RIGHTS_READ = 0x00020000,
			STANDARD_RIGHTS_WRITE = 0x00020000,
			STANDARD_RIGHTS_EXECUTE = 0x00020000,

			STANDARD_RIGHTS_ALL = 0x001f0000,

			SPECIFIC_RIGHTS_ALL = 0x0000ffff,

			ACCESS_SYSTEM_SECURITY = 0x01000000,

			MAXIMUM_ALLOWED = 0x02000000,

			GENERIC_READ = 0x80000000,
			GENERIC_WRITE = 0x40000000,
			GENERIC_EXECUTE = 0x20000000,
			GENERIC_ALL = 0x10000000,

			DESKTOP_READOBJECTS = 0x00000001,
			DESKTOP_CREATEWINDOW = 0x00000002,
			DESKTOP_CREATEMENU = 0x00000004,
			DESKTOP_HOOKCONTROL = 0x00000008,
			DESKTOP_JOURNALRECORD = 0x00000010,
			DESKTOP_JOURNALPLAYBACK = 0x00000020,
			DESKTOP_ENUMERATE = 0x00000040,
			DESKTOP_WRITEOBJECTS = 0x00000080,
			DESKTOP_SWITCHDESKTOP = 0x00000100,

			WINSTA_ENUMDESKTOPS = 0x00000001,
			WINSTA_READATTRIBUTES = 0x00000002,
			WINSTA_ACCESSCLIPBOARD = 0x00000004,
			WINSTA_CREATEDESKTOP = 0x00000008,
			WINSTA_WRITEATTRIBUTES = 0x00000010,
			WINSTA_ACCESSGLOBALATOMS = 0x00000020,
			WINSTA_EXITWINDOWS = 0x00000040,
			WINSTA_ENUMERATE = 0x00000100,
			WINSTA_READSCREEN = 0x00000200,

			WINSTA_ALL_ACCESS = 0x0000037f
		}

		/// <summary>
		/// Access to the service. Before granting the requested access, the
		/// system checks the access token of the calling process.
		/// </summary>
		[Flags]
		public enum SERVICE_ACCESS : uint
		{
			/// <summary>
			/// Required to call the QueryServiceConfig and
			/// QueryServiceConfig2 functions to query the service configuration.
			/// </summary>
			SERVICE_QUERY_CONFIG = 0x00001,

			/// <summary>
			/// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function
			/// to change the service configuration. Because this grants the caller
			/// the right to change the executable file that the system runs,
			/// it should be granted only to administrators.
			/// </summary>
			SERVICE_CHANGE_CONFIG = 0x00002,

			/// <summary>
			/// Required to call the QueryServiceStatusEx function to ask the service
			/// control manager about the status of the service.
			/// </summary>
			SERVICE_QUERY_STATUS = 0x00004,

			/// <summary>
			/// Required to call the EnumDependentServices function to enumerate all
			/// the services dependent on the service.
			/// </summary>
			SERVICE_ENUMERATE_DEPENDENTS = 0x00008,

			/// <summary>
			/// Required to call the StartService function to start the service.
			/// </summary>
			SERVICE_START = 0x00010,

			/// <summary>
			///     Required to call the ControlService function to stop the service.
			/// </summary>
			SERVICE_STOP = 0x00020,

			/// <summary>
			/// Required to call the ControlService function to pause or continue
			/// the service.
			/// </summary>
			SERVICE_PAUSE_CONTINUE = 0x00040,

			/// <summary>
			/// Required to call the EnumDependentServices function to enumerate all
			/// the services dependent on the service.
			/// </summary>
			SERVICE_INTERROGATE = 0x00080,

			/// <summary>
			/// Required to call the ControlService function to specify a user-defined
			/// control code.
			/// </summary>
			SERVICE_USER_DEFINED_CONTROL = 0x00100,

			/// <summary>
			/// Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
			/// </summary>
			SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
				SERVICE_QUERY_CONFIG |
				SERVICE_CHANGE_CONFIG |
				SERVICE_QUERY_STATUS |
				SERVICE_ENUMERATE_DEPENDENTS |
				SERVICE_START |
				SERVICE_STOP |
				SERVICE_PAUSE_CONTINUE |
				SERVICE_INTERROGATE |
				SERVICE_USER_DEFINED_CONTROL),

			GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
				SERVICE_QUERY_CONFIG |
				SERVICE_QUERY_STATUS |
				SERVICE_INTERROGATE |
				SERVICE_ENUMERATE_DEPENDENTS,

			GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
				SERVICE_CHANGE_CONFIG,

			GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
				SERVICE_START |
				SERVICE_STOP |
				SERVICE_PAUSE_CONTINUE |
				SERVICE_USER_DEFINED_CONTROL,

			/// <summary>
			/// Required to call the QueryServiceObjectSecurity or
			/// SetServiceObjectSecurity function to access the SACL. The proper
			/// way to obtain this access is to enable the SE_SECURITY_NAME
			/// privilege in the caller's current access token, open the handle
			/// for ACCESS_SYSTEM_SECURITY access, and then disable the privilege.
			/// </summary>
			ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,

			/// <summary>
			/// Required to call the DeleteService function to delete the service.
			/// </summary>
			DELETE = ACCESS_MASK.DELETE,

			/// <summary>
			/// Required to call the QueryServiceObjectSecurity function to query
			/// the security descriptor of the service object.
			/// </summary>
			READ_CONTROL = ACCESS_MASK.READ_CONTROL,

			/// <summary>
			/// Required to call the SetServiceObjectSecurity function to modify
			/// the Dacl member of the service object's security descriptor.
			/// </summary>
			WRITE_DAC = ACCESS_MASK.WRITE_DAC,

			/// <summary>
			/// Required to call the SetServiceObjectSecurity function to modify
			/// the Owner and Group members of the service object's security
			/// descriptor.
			/// </summary>
			WRITE_OWNER = ACCESS_MASK.WRITE_OWNER,
		}

		[Flags]
		enum SERVICE_TYPE : uint
		{
			/// <summary>
			/// Driver service.
			/// </summary>
			SERVICE_KERNEL_DRIVER = 0x00000001,

			/// <summary>
			/// File system driver service.
			/// </summary>
			SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,

			/// <summary>
			/// Service that runs in its own process.
			/// </summary>
			SERVICE_WIN32_OWN_PROCESS = 0x00000010,

			/// <summary>
			/// Service that shares a process with one or more other services.
			/// </summary>
			SERVICE_WIN32_SHARE_PROCESS = 0x00000020,

			/// <summary>
			/// The service can interact with the desktop.
			/// </summary>
			SERVICE_INTERACTIVE_PROCESS = 0x00000100,
		}
		#endregion

		#region API define Functions
		[DllImport("advapi32.dll", SetLastError = true)]
		internal static extern bool QueryServiceStatusEx(IntPtr hService, int infoLevel, IntPtr lpBuffer, uint cbBufSize, out uint pcbBytesNeeded);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern int ChangeServiceConfig(IntPtr service, int serviceType, int startType, int errorControl, [MarshalAs(UnmanagedType.LPTStr)]
			string binaryPathName,
			[MarshalAs(UnmanagedType.LPTStr)]
			string loadOrderGroup,
			IntPtr tagID,
			[MarshalAs(UnmanagedType.LPTStr)]
			string dependencies,
			[MarshalAs(UnmanagedType.LPTStr)]
			string startName,
			[MarshalAs(UnmanagedType.LPTStr)]
			string password,
			[MarshalAs(UnmanagedType.LPTStr)]
			string displayName);
		[DllImport("advapi32.dll",
		SetLastError = true, CharSet = CharSet.Auto)]
		private static extern int QueryServiceConfig(IntPtr service, IntPtr queryServiceConfig, int bufferSize, ref int bytesNeeded);

		[DllImport("advapi32.dll", EntryPoint = "QueryServiceStatus", CharSet = CharSet.Auto)]
		private static extern bool QueryServiceStatus(IntPtr hService, ref SERVICE_STATUS dwServiceStatus);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl, ref SERVICE_STATUS lpServiceStatus);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteService(IntPtr hService);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseServiceHandle(IntPtr hSCObject);

		[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr CreateService(
			IntPtr hSCManager,
			string lpServiceName,
			string lpDisplayName,
			uint dwDesiredAccess,
			uint dwServiceType,
			uint dwStartType,
			uint dwErrorControl,
			string lpBinaryPathName,
			string lpLoadOrderGroup,
			uint lpdwTagId,
			string lpDependencies,
			string lpServiceStartName,
			string lpPassword);
		[DllImport("advapi32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool SetServiceObjectSecurity(SafeHandle serviceHandle, System.Security.AccessControl.SecurityInfos secInfos, byte[] lpSecDesrBuf);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool QueryServiceObjectSecurity(IntPtr serviceHandle, System.Security.AccessControl.SecurityInfos secInfo, ref SECURITY_DESCRIPTOR lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle, System.Security.AccessControl.SecurityInfos secInfo, byte[] lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool LookupAccountSid(String lpSystemName, IntPtr Sid, System.Text.StringBuilder lpName, ref int cchName, System.Text.StringBuilder ReferencedDomainName, ref int cchReferencedDomainName, out int peUse);
		#endregion

		public APIService() {}

		public static int InstallService(string filepath, string servicename)
		{
			//System.Threading.Thread.Sleep(10000);
			IntPtr schSCManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_CREATE_SERVICE);
			if (schSCManager == IntPtr.Zero)
				return Marshal.GetLastWin32Error();

			IntPtr schService = OpenService(schSCManager, servicename, (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			int result = 0;
			if (schService != IntPtr.Zero)
			{
				result = -1;
				//result = UnInstallService(schService);
			}
			//return when can't Un Install old Service
			if ( result != 0)
			{
				CloseServiceHandle(schService);
				CloseServiceHandle(schSCManager);
				return Marshal.GetLastWin32Error();

			 }
			result = 0;//reset flag
			schService = IntPtr.Zero;
			
			schService = CreateService
				(
				schSCManager,	/* SCManager database      */
				servicename,			/* name of service         */
				servicename,			/* service name to display */
				(uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS,        /* desired access          */
				(uint)(SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS), /* service type            */
				(uint)SERVICE_START.SERVICE_AUTO_START,      /* start type              */
				(uint)SERVICE_ERROR.SERVICE_ERROR_NORMAL,      /* error control type      */
				filepath,			/* service's binary        */
				null,                      /* no load ordering group  */
				0,                      /* no tag identifier       */
				null,                      /* no dependencies         */
				null,                      /* LocalSystem account     */
				null
				);                     /* no password             */
			if (schService != IntPtr.Zero)
				result = 0;
			else
			{
				result = Marshal.GetLastWin32Error();
			}

			CloseServiceHandle(schService);
			CloseServiceHandle(schSCManager);
			return result;
		}
		public static  int UnInstallService(string serviceName)
		{
			//System.Threading.Thread.Sleep(10000);
			IntPtr schSCManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
			if (schSCManager == IntPtr.Zero)
				return Marshal.GetLastWin32Error();

			IntPtr schService = OpenService(schSCManager, serviceName, (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS);

			int result = UnInstallService( schService);

			CloseServiceHandle(schService);
			CloseServiceHandle(schSCManager);
			return result;

		}
		private static int UnInstallService(IntPtr schService)
		{
			//System.Threading.Thread.Sleep(10000);
			if (schService == IntPtr.Zero)
				return 0;
			int result = 0;
			if (!DeleteService(schService))
			{
				result = Marshal.GetLastWin32Error();
			}

			SERVICE_STATUS_PROCESS svrProcess = QueryServiceStatusEx(schService);
			if (svrProcess.dwProcessId > 0)
			{
				Process p = Process.Start("taskkill", string.Format("/pid {0} /f", svrProcess.dwProcessId));
				if (p != null)
					p.WaitForExit(5000);
			}

			return result;
		}
		public static int StopService(string serviceName)
		{
			//System.Threading.Thread.Sleep(10000);
			IntPtr schSCManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
			if (schSCManager == IntPtr.Zero)
				return Marshal.GetLastWin32Error();

			IntPtr schService = OpenService(schSCManager, serviceName, (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			if (schService == IntPtr.Zero)
			{
				CloseServiceHandle(schSCManager);
				return Marshal.GetLastWin32Error();
			}

			SERVICE_STATUS status = new SERVICE_STATUS();
			int result = 0;
			if (ControlService(schService, SERVICE_CONTROL.STOP, ref status))
			{
				int time_out_stop = 1000 * 60;
				int time_out_check_status = 1000;
				while (QueryServiceStatus(schService, ref status) && time_out_stop > 0)
				{
					if (status.currentState == (int)SERVICE_STATE.SERVICE_STOPPED)
					{
						result = 0;
						break;
					}
					System.Threading.Thread.Sleep(time_out_check_status);
					time_out_stop -= time_out_check_status;
				}
			}
			else
			{
				if( status.currentState == (int)SERVICE_STATE.SERVICE_STOPPED)
					result = 0;
				else result = Marshal.GetLastWin32Error();
			}
			CloseServiceHandle(schService);
			CloseServiceHandle(schSCManager);
			return result;
		}
		public static int StartService(string serviceName)
		{
			IntPtr schSCManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
			if (schSCManager == IntPtr.Zero)
				return Marshal.GetLastWin32Error();

			IntPtr schService = OpenService(schSCManager, serviceName, (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			if (schService == IntPtr.Zero)
			{
				CloseServiceHandle(schSCManager);
				return Marshal.GetLastWin32Error();
			}
			int result = 0;
			if (!StartService(schService, 0, null))
				result = Marshal.GetLastWin32Error();

			CloseServiceHandle(schService);
			CloseServiceHandle(schSCManager);
			return result;
		}
		public static int ChangeServiceStart(string ServiceName, SERVICE_START service_start)
		{
			ServiceInfo serviceInfo = QueryServiceInfo(ServiceName);

			IntPtr scManager = OpenSCManager(null, null, (int)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
			if (scManager.Equals(IntPtr.Zero))
				return Marshal.GetLastWin32Error();

			IntPtr service = OpenService(scManager, ServiceName, (int)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			if (service.Equals(IntPtr.Zero))
			{
				CloseServiceHandle(scManager);
				return Marshal.GetLastWin32Error();
			}
			int ret = 0;
			if (ChangeServiceConfig(service, serviceInfo.serviceType,
				(int)service_start, serviceInfo.errorControl,
				null, null,
				IntPtr.Zero, serviceInfo.dependencies,
				null, null, null) == 0)
			{
				ret = Marshal.GetLastWin32Error();
			}
			CloseServiceHandle(service);
			CloseServiceHandle(scManager);
			return ret;
		}

		public static int ChageServiceSecurity(string ServiceName, WellKnownSidType WorldSid, SERVICE_ACCESS service_access)
		{
			ServiceController sc = new ServiceController(ServiceName);
			byte[] psd = new byte[0];
			uint bufSizeNeeded;
			bool ok = QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd, 0, out bufSizeNeeded);
			if (!ok)
			{
				int err = Marshal.GetLastWin32Error();
				if (err == ERROR_INSUFFICIENT_BUFFER) // ERROR_INSUFFICIENT_BUFFER
				{
					// expected; now we know bufsize
					psd = new byte[bufSizeNeeded];
					ok = QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd, bufSizeNeeded, out bufSizeNeeded);
				}
				else
				{
					return Marshal.GetLastWin32Error();
					//throw new ApplicationException("error calling QueryServiceObjectSecurity() to get DACL for SeaweedService: error code=" + err);
				}
			}
			if (!ok)
				return Marshal.GetLastWin32Error();
				//throw new ApplicationException("error calling QueryServiceObjectSecurity(2) to get DACL for SeaweedService: error code=" + Marshal.GetLastWin32Error());

			// get security descriptor via raw into DACL form so ACE
			// ordering checks are done for us.
			RawSecurityDescriptor rsd = new RawSecurityDescriptor(psd, 0);
			RawAcl racl = rsd.DiscretionaryAcl;
			DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, racl);

			// TODO: fiddle with the dacl to SetAccess() etc
			int access = (int)service_access;
			//NTAccount account = new NTAccount(User_Group);
			//SecurityIdentifier Sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
			IdentityReference Sid = GetIdentity(WorldSid);
			dacl.SetAccess(AccessControlType.Allow, (SecurityIdentifier)Sid, access, InheritanceFlags.None, PropagationFlags.None);

			// convert discretionary ACL back to raw form; looks like via byte[] is only way
			byte[] rawdacl = new byte[dacl.BinaryLength];
			dacl.GetBinaryForm(rawdacl, 0);
			rsd.DiscretionaryAcl = new RawAcl(rawdacl, 0);

			// set raw security descriptor on service again
			byte[] rawsd = new byte[rsd.BinaryLength];
			rsd.GetBinaryForm(rawsd, 0);
			ok = SetServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, rawsd);
			int ret = 0;
			if (!ok)
			{
				ret = Marshal.GetLastWin32Error();
				//throw new ApplicationException("error calling SetServiceObjectSecurity(); error code=" + Marshal.GetLastWin32Error());
			}
			sc.Close();
			sc.Dispose();
			sc = null;
			return ret;
		}

		public static SERVICE_START GetServiceStart(string serviceName)
		{
			ServiceInfo svr_info = QueryServiceInfo(serviceName);
			if (string.IsNullOrEmpty(svr_info.displayName))
				return SERVICE_START.SERVICE_DISABLED;

			return (SERVICE_START)svr_info.startType;
		}

		public static SERVICE_STATE GetServiceState(string serviceName)
		{
			IntPtr schSCManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
			if (schSCManager == IntPtr.Zero)
				return SERVICE_STATE.SERVICE_STOPPED;

			int ret = 0;
			IntPtr schService = OpenService(schSCManager, serviceName, (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			SERVICE_STATUS svr_status = new SERVICE_STATUS();
			if (QueryServiceStatus(schService, ref svr_status))
			{

				ret = svr_status.currentState;
			}
			else
				ret = (int)SERVICE_STATE.SERVICE_STOPPED;;


			CloseServiceHandle(schService);
			CloseServiceHandle(schSCManager);
			return (SERVICE_STATE)ret;

		}

		private static ServiceInfo QueryServiceInfo(string ServiceName)
		{
			if (ServiceName.Equals(""))
				return new ServiceInfo();
			IntPtr scManager = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);

			if (scManager == IntPtr.Zero)
				return new ServiceInfo();

			IntPtr service = OpenService(scManager, ServiceName, (int)SERVICE_ACCESS.SERVICE_ALL_ACCESS);
			if (service == IntPtr.Zero )
			{
				CloseServiceHandle(scManager);
				return new ServiceInfo();
			}

			int bytesNeeded = 5;
			QueryServiceConfigStruct qscs = new QueryServiceConfigStruct();
			IntPtr qscPtr = Marshal.AllocCoTaskMem(0);

			int retCode = QueryServiceConfig(service, qscPtr,0, ref bytesNeeded);
			if (retCode == 0 && bytesNeeded == 0)
			{
				CloseServiceHandle(service);
				CloseServiceHandle(scManager);
				return new ServiceInfo();
			}
			else
			{
				qscPtr = Marshal.AllocCoTaskMem(bytesNeeded);
				retCode = QueryServiceConfig(service, qscPtr,
				bytesNeeded, ref bytesNeeded);
				if (retCode == 0)
				{
					CloseServiceHandle(service);
					CloseServiceHandle(scManager);
					Marshal.FreeCoTaskMem(qscPtr);
					return new ServiceInfo();
				}
				qscs.binaryPathName = IntPtr.Zero;
				qscs.dependencies = IntPtr.Zero;
				qscs.displayName = IntPtr.Zero;
				qscs.loadOrderGroup = IntPtr.Zero;
				qscs.startName = IntPtr.Zero;

				qscs = (QueryServiceConfigStruct)
				Marshal.PtrToStructure(qscPtr,new QueryServiceConfigStruct().GetType());
			}

			CloseServiceHandle(service);
			CloseServiceHandle(scManager);

			ServiceInfo serviceInfo = new ServiceInfo();
			serviceInfo.binaryPathName = Marshal.PtrToStringAuto(qscs.binaryPathName);
			serviceInfo.dependencies = Marshal.PtrToStringAuto(qscs.dependencies);
			serviceInfo.displayName = Marshal.PtrToStringAuto(qscs.displayName);
			serviceInfo.loadOrderGroup = Marshal.PtrToStringAuto(qscs.loadOrderGroup);
			serviceInfo.startName = Marshal.PtrToStringAuto(qscs.startName);

			serviceInfo.errorControl = qscs.errorControl;
			serviceInfo.serviceType = qscs.serviceType;
			serviceInfo.startType = qscs.startType;
			serviceInfo.tagID = qscs.tagID;

			Marshal.FreeCoTaskMem(qscPtr);
			return serviceInfo;
		}
		
		private static IdentityReference GetIdentity(WellKnownSidType WorldSid)
		{
			SecurityIdentifier sid = new SecurityIdentifier(WorldSid, null);
			NTAccount Account = sid.Translate(typeof(NTAccount)) as NTAccount;
			IdentityReference idenUser = Account.Translate(typeof(SecurityIdentifier));
			return idenUser;
		}

		private static SERVICE_STATUS_PROCESS QueryServiceStatusEx(IntPtr serviceHandle)
		{
			IntPtr buf = IntPtr.Zero;
			try
			{
				uint size = 0;

				QueryServiceStatusEx(serviceHandle, 0, buf, size, out size);

				buf = Marshal.AllocHGlobal((int)size);

				if (!QueryServiceStatusEx(serviceHandle, 0, buf, size, out size))
				{
					return default(SERVICE_STATUS_PROCESS); 
				}

				return (SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(buf, typeof(SERVICE_STATUS_PROCESS));
			}
			finally
			{
				if (!buf.Equals(IntPtr.Zero))
					Marshal.FreeHGlobal(buf);
			}
		}

	}
}
