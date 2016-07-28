using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebConfigEncryption
{
	public class WebConfigEncryption
	{
		public static void Encrypt ()
		{
			using(WebConfigProtection wprotect = new WebConfigProtection())
			{
				wprotect.EncryptConfigFile(new string [] { "connectionStrings", "appSettings" }, "EncryptionProvider", @"D:\Source_Code\CMSWeb\CMSService\CMSSVR");
			}
		}
		public static void Decrypt()
		{
			using (WebConfigProtection wprotect = new WebConfigProtection())
			{
				wprotect.DecryptConfigFile(new string [] { "connectionStrings", "appSettings" }, @"D:\Source_Code\CMSWeb\CMSService\CMSSVR");
			}
		}
	}
}
