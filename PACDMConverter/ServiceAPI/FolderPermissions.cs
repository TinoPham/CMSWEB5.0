using System;
using System.Collections.Generic;
using System.Text;
using System.Security.AccessControl;
using System.IO;
using Microsoft.Win32;
using System.Security.Principal;

namespace ServiceManager
{
	class FolderPermissions
	{
		public static void SetFullControlReg(string regpath, WellKnownSidType WorldSid)
		{
			SetFullPermissionReg(regpath, WorldSid);
		}
		public static void SetFullControlFolder(string pacdir, WellKnownSidType WorldSid)
		{
			AddDirectorySecurity(pacdir, WorldSid, FileSystemRights.FullControl, AccessControlType.Allow);
		}
		private static void AddDirectorySecurity(string FileName, WellKnownSidType WorldSid, FileSystemRights Rights, AccessControlType ControlType)
		{
			// Create a new DirectoryInfo object.
			DirectoryInfo dInfo = new DirectoryInfo(FileName);
			// Get a DirectorySecurity object that represents the 
			// current security settings.
			DirectorySecurity dSecurity = dInfo.GetAccessControl();
			IdentityReference idenUser = GetIdentity(WorldSid);
			// Add the FileSystemAccessRule to the security settings. 
			FileSystemAccessRule securfile = new FileSystemAccessRule(idenUser,
															Rights,
															InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
															ControlType);
			dSecurity.AddAccessRule(securfile);

			// Set the new access settings.
			dInfo.SetAccessControl(dSecurity);
		}
		private static void SetFullPermissionReg(string regPath, WellKnownSidType WorldSid)
		{
			RegistryKey reg = Registry.LocalMachine.CreateSubKey(regPath);
			RegistrySecurity serreg = reg.GetAccessControl();
			IdentityReference idenUser = GetIdentity(WorldSid);
			RegistryAccessRule accessrule = new RegistryAccessRule(idenUser, RegistryRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
			serreg = new RegistrySecurity();
			serreg.AddAccessRule(accessrule);
			reg.SetAccessControl(serreg);
		}
		private static IdentityReference GetIdentity(WellKnownSidType WorldSid)
		{
			SecurityIdentifier sid = new SecurityIdentifier(WorldSid, null);
			NTAccount Account = sid.Translate(typeof(NTAccount)) as NTAccount;
			IdentityReference idenUser = Account.Translate(typeof(SecurityIdentifier));
			return idenUser;
		}
		public static string GetUserID(string username)
		{
			NTAccount account = new NTAccount(username);
			IdentityReference idenUser = account.Translate(typeof(System.Security.Principal.SecurityIdentifier));
			return idenUser.Value;

		}
	}
}
