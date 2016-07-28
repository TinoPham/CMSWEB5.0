using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace HASPkey
{
	public class HaspLicense
	{
		const string XPATH_CHILD_NODE = "./{0}";
		const string str_PACDM = "PACDM";
		const string license_file = "license.dll";
		const string LicenseFunctionName = "GetLicenseInfoString";
		const int License_Length = 2048;
		const string serial_code = "serial_code";
		const string hasp_id = "hasp_id";

		[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private delegate bool del_GetLicenseInfoString(string app_path, [MarshalAs(UnmanagedType.LPStr)] StringBuilder _license_info);

		private static readonly Lazy<HaspLicense> _sInstance = new Lazy<HaspLicense>(() => new HaspLicense());

		public static HaspLicense Instance { get { return _sInstance.Value; } }

		public string LicensInfo { get ; private set;}

		private HaspLicense()
		{
			LicensInfo = null;
		}

		public bool ReadLicense(string srx_Dir)
		{
			string lisence_path = Path.Combine( srx_Dir,license_file);
			if( !File.Exists(lisence_path))
				return false;
			IntPtr module = UnanageDynamic.LoadLibrary(Path.Combine(srx_Dir, license_file));
			if (module == IntPtr.Zero)
				return false;
			del_GetLicenseInfoString LicenseInfoString;

			LicenseInfoString = UnanageDynamic.LoadFunction<del_GetLicenseInfoString>(module, LicenseFunctionName) as del_GetLicenseInfoString;
			if (LicenseInfoString == null)
			{
				UnanageDynamic.FreeLibrary(module);
				return false;
			}

			StringBuilder licenseInfoBuilder = new StringBuilder(License_Length);
			bool ret = LicenseInfoString(srx_Dir, licenseInfoBuilder);
			UnanageDynamic.FreeLibrary(module);
			if (!ret)
				return ret;

			if (licenseInfoBuilder == null || licenseInfoBuilder.Length == 0)
				return false;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(licenseInfoBuilder.ToString());
			}	catch(XmlException){}
			ret = true;
			ret &= Read_LicenseInfo( doc.DocumentElement as XmlNode, serial_code);
			if( ret)
				return ret;

			ret = Read_LicenseInfo(doc.DocumentElement as XmlNode, hasp_id);

			return ret;
		}
		private	bool Read_LicenseInfo( XmlNode root ,string nodename)
		{
			if( root == null)
				return false;

			string xpath = string.Format(XPATH_CHILD_NODE, nodename);

			XmlNode node = root.SelectSingleNode(xpath);
			if( node == null)
				return false;
			LicensInfo = node.InnerText;
			return !string.IsNullOrEmpty(LicensInfo);
		}
	}
}
