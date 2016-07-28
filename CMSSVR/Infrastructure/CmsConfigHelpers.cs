using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CMSSVR.Infrastructure
{
	public class CmsConfigHelpers
	{
		public static bool IsMongoDB(string connectionstring = "LogContext")
		{
			return ConfigurationManager.ConnectionStrings[connectionstring].ProviderName.ToUpper() == "mongodb".ToUpper();

		}
	}
}