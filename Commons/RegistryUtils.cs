using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Commons
{
	public static class RegistryUtils
	{

		public static string GetRegValue(RegistryHive hKey, RegistryView view, string path, string keyname)
		{
			using( RegistryKey key = RegistryKey.OpenBaseKey(hKey, view))
			{
				return GetRegValue(key, path, keyname);
			}
		}

		public static bool SetRegValue(RegistryHive hKey, RegistryView view, string path, string name, string value)
		{
			using( RegistryKey key = RegistryKey.OpenBaseKey(hKey, view))
			{
				return SetRegValue(key, path, name, value);
			}
			
		}
		public static void DeleteRegPath(RegistryHive hKey, RegistryView view, string path)
		{
			using( RegistryKey key = RegistryKey.OpenBaseKey(hKey, view))
			{
				key.DeleteSubKey(path ,false);
			}
			
		}
		public static void DeleteRegValue(RegistryHive hKey, RegistryView view, string path, string name)
		{
			try{
				using (RegistryKey key = RegistryKey.OpenBaseKey(hKey, view))
				{
					try{
						using(RegistryKey rkey = key.OpenSubKey(path, true))
						{
							try{
								rkey.DeleteValue(name, false);
							}catch(Exception){}
						}
					}catch(Exception){}
				}
			}
			catch(Exception){}
		}

		public static string GetRegValue(RegistryKey Hkey, string path, string keyname)
		{
			if (Hkey == null)
				return string.Empty;

			string result = string.Empty;
			RegistryKey key = Hkey.OpenSubKey(path);
			if (key == null)
				return null;
			result = key.GetValue(keyname, string.Empty).ToString();
			key.Close();
			key = null;
			return result;
		}

		public static bool SetRegValue(RegistryKey Hkey, string path, string name, string value)
		{
			if (Hkey == null)
				return false;

			RegistryKey rkey = null;

			try
			{
				rkey = Hkey.OpenSubKey(path, true);
				if (rkey == null)
					return false;

				rkey.SetValue(name, value);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
			finally
			{
				if (rkey != null)
					rkey.Close();
			}
		}
	}
}
