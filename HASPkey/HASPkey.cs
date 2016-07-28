using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using System.Runtime.InteropServices;
using Aladdin.HASP;

namespace HASPkey
{
	public class HASPkey
	{
		private static readonly Lazy<HASPkey> _sInstance = new Lazy<HASPkey>(() => new HASPkey());

		public static HASPkey Instance { get { return _sInstance.Value; } }

		public string HaspID{ get; private set;}


		public HASPkey()
		{
			this.HaspID = string.Empty;
		}

		public HaspStatus haspReadAPI()
		{
			HaspFeature feature = HaspFeature.ProgNumDefault;

			IntPtr zero = IntPtr.Zero;
			zero = Marshal.StringToHGlobalAnsi(VendorCode.vendorCodeString);
			int handle = 0;

			//login
			HaspStatus status = (HaspStatus)HaspAPI.hasp_login(feature.FeatureId, zero, ref handle);
			if (status != HaspStatus.StatusOk)
				goto LABEL_EXIT;
			string session_info = "";
			status = disp_get_sessioninfo(handle, HaspAPI.HASP_KEYINFO, ref session_info);
			if (status != HaspStatus.StatusOk)
				goto LABEL_EXIT;
			//logout
			status = (HaspStatus)HaspAPI.hasp_logout(handle);
			if (status != HaspStatus.StatusOk)
				goto LABEL_EXIT;
			if( !string.IsNullOrEmpty(session_info))
			{
				this.HaspID = ParserHaspID(session_info);
			}
			
			goto LABEL_EXIT;
		LABEL_EXIT:
			return status;
		}

		HaspStatus disp_get_sessioninfo(int handle, string format, ref string info)
		{
			HaspStatus haspStatus = HaspAPI.hasp_get_sessioninfo(handle, format, ref info);
			return haspStatus;
		}
		private string ParserHaspID(string sessioninfo)
		{
			if( string.IsNullOrEmpty( sessioninfo) )
				return null;
			XmlNode node = FindXML(sessioninfo, HaspAPI.HASP_HASPID_XPath);
			return node == null? null : node.InnerText;

		}
		private XmlNode FindXML(string xmlstring, string xpath)
		{
			if(string.IsNullOrEmpty(xmlstring))
				return null;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlstring);
			return string.IsNullOrEmpty(xpath)? doc.DocumentElement : FindXML(doc.DocumentElement, xpath);

		}
		private XmlNode FindXML( XmlNode node, string xpath)
		{
			return node == null? null : node.SelectSingleNode(xpath);
		}
		

	}

	internal class VendorCode
	{
		/// <summary>
		/// The Base64 encoded vendor code for a demoma key.
		/// </summary>
		public const string vendorCodeString =
			"yLb3XsHTZzLU11vhcpkQd95tBDG4Uj3eYwfjYPfnQaat9vbyhcVkpvzAFWLuI7llQ2UGenJekovjRZUz" +
			"cHh3lNSE1eh2puchtYiQnW9aj2cHIYPZ9M2lzDirOo2a/IaM8cR0hWiMXwrnYdsV/Jd92bz1cPcWMn5m" +
			"1iFc1QbsBYnlG3IFXXJ0GzY93Ba7ac1vS/0SXGMi3vTLyv1ikYTQiq4WiZvpGClCVzunibzgZdi4pPld" +
			"m2+6r899LvShjUD//lvP1o2AI5crQCdSoRzlTYusUlKelndqRx8JfkDmD6w+kksMUs8O3BRGBrNUOPaq" +
			"k5O294FesesqpWvAWYSpLkhumX746nHS3ObFxvFKvU9UoFY8dyDWFMRN7r8IT1tDLogMUWBBwA0B60AS" +
			"yV27hfMzv61Pj853ZgKwGSGoKI7ZD1iCC0+xVV8yc6R3dvSMVW7M3lsldPX5v9eTXz8QYkB1N1gFKu2/" +
			"i5ZcL7+bErwIpVMjZCuPx7cF99RdJp+4AT9NCyVxj79nk6JYONBRGHIiGBc2lj/kVRve7PIccyrCMGLY" +
			"oA52Hjpfz4Qw4IxB6hqykYSdCQtju5g2iIEPo6Ki6eSoZPs7M8x1WbIuy892Zck/+ZfyNHDgrmEkaZpj" +
			"X9CcYVbuz9aXmniTpTKNsj04zEdwx3ZADbfIbNSRsNU+ZtBr/inQhngJCbGDxNDdX2Aup3HqZiYUiCft" +
			"7WLUvYsXsDb/ow==";

		/// <summary>
		/// Constructor - does nothing by default
		/// </summary>
		public VendorCode()
		{
		}

		/// <summary>
		/// Returns the vendor code as a byte array.
		/// </summary>
		static public byte[] Code
		{
			get
			{
				byte[] code = ASCIIEncoding.Default.GetBytes(vendorCodeString);
				return code;
			}
		}
	}

	internal class HaspAPI
	{
		/// <summary>
		/// Hasp ID format: 
		/// <?xml version="1.0" encoding="UTF-8"?>
		///<hasp_info><feature><featureid>4294901760</featureid><activations>unlimited</activations></feature></hasp_info>
		/// </summary>
		public const string HASP_SESSIONINFO = "<haspformat format=\"sessioninfo\"/>";
		public const string HASP_SESSIONINFO_XPath = "feature/featureid";
		public const string HASP_KEYINFO = "<haspformat format=\"keyinfo\"/>";
		public const string HASP_HASPID_XPath = "keyspec/hasp/haspid";

		[DllImport("hasp_windows.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int hasp_login(int feature_id, IntPtr vendor_code, ref int handle);

		[DllImport("hasp_windows.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int hasp_get_size(int handle, int fileid, ref int size);
		[DllImport("hasp_windows.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int hasp_read(int handle, int fileid, int offset, int length, [Out] byte[] buffer);
		[DllImport("hasp_windows.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int hasp_logout(int handle);
		[DllImport("hasp_windows.dll", CharSet = CharSet.None, ExactSpelling = false)]
		public static extern HaspStatus hasp_get_sessioninfo(int handle, String format, ref string info);

	}
}
