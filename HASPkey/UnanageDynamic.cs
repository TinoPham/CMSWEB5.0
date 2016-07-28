using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace HASPkey
{
	internal static class UnanageDynamic
	{
		[DllImport("kernel32.dll")]
		public static extern bool SetCurrentDirectory(string lpPathName);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport("kernel32.dll")]
		public static extern bool FreeLibrary(IntPtr hModule);

		public static Delegate LoadFunction<T>(IntPtr hModule, string functionName)
		{

			IntPtr functionAddress = GetProcAddress(hModule, functionName);
			if (functionAddress == IntPtr.Zero)
				return null;

			return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
		}
	}
}
