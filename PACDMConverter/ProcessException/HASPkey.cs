#region using
using System;
using System.Collections.Generic;
using System.Text;
using Aladdin.HASP;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
#endregion using

namespace ProcessException
{
	internal class HaspLicense
	{
	
		const string XPATH_CHILD_NODE = "./{0}";
		const string str_PACDM = "PACDM";
		const string license_file = "license.dll";
		const string LicenseFunctionName = "GetLicenseInfoString";
		const int License_Length = 2048;
		const string serial_code = "serial_code";
		private delegate bool del_GetLicenseInfoString(string app_path, [MarshalAs(UnmanagedType.LPStr)] StringBuilder _license_info);
        int _PACNumber = 0;
        public int PACNumber { get { return _PACNumber; } set { _PACNumber = value; } }
		public HaspLicense()
		{
			PACNumber = 0;
		}
		//public bool ReadLicense(string srx_Dir)
		public string ReadLicense(/*string srx_Dir*/)
		{
			string srx_Dir = Data.ReadRegistryValue(Data.REG_I3PRO_PATH, Data.REG_I3PRO_KEY);
			bool ret = false;
			IntPtr module = UnanageDynamic.LoadLibrary(Path.Combine(srx_Dir, license_file));
			if (module == IntPtr.Zero)
				return "";

			del_GetLicenseInfoString LicenseInfoString;

			LicenseInfoString = UnanageDynamic.LoadFunction<del_GetLicenseInfoString>(module, LicenseFunctionName) as del_GetLicenseInfoString;
			if (LicenseInfoString == null)
			{
				UnanageDynamic.FreeLibrary(module);
				return "";
			}
			StringBuilder licenseInfoBuilder = new StringBuilder(License_Length);
			ret = LicenseInfoString(srx_Dir, licenseInfoBuilder);
			UnanageDynamic.FreeLibrary(module);
			if (!ret)
				return "";
			if (licenseInfoBuilder == null)
				return "";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(licenseInfoBuilder.ToString());
			XmlNodeList node = doc.GetElementsByTagName(serial_code);
			return node.Item(0).InnerText;
		}

		private bool ParserLicense( string licenseinfo)
		{
			if (string.IsNullOrEmpty(licenseinfo))
				return false;
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(licenseinfo);
				XmlNode root = doc.DocumentElement;
				GetPACNumber(root);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}

		}
		private void GetPACNumber( XmlNode root)
		{
			string xpath = string.Format(XPATH_CHILD_NODE, str_PACDM);
			XmlNode node = SelectNode(root, xpath);
			if (node == null)
				return;
			int pac = 0;
			Int32.TryParse(node.InnerText, out pac);
			PACNumber = pac;

		}
		private XmlNode SelectNode( XmlNode root, string xpath)
		{
			return root.SelectSingleNode(xpath);
		}
	}
}
