using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace PACDMConverter
{
	public static class UriExtension
	{

		

		/// <summary>
		/// The combine.
		/// </summary>
		/// <param name="parts">
		/// The parts.
		/// </param>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		public static string Combine(params string[] parts)
		{
			if (parts == null || parts.Length == 0)
				return string.Empty;

			var urlBuilder = new StringBuilder();
			foreach (var part in parts)
			{
				var tempUrl = tryCreateRelativeOrAbsolute(part);
				urlBuilder.Append(tempUrl);
			}
			return VirtualPathUtility.RemoveTrailingSlash(urlBuilder.ToString());
		}

		private static string tryCreateRelativeOrAbsolute(string s)
		{
			System.Uri uri;
			Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri);
			string tempUrl = VirtualPathUtility.AppendTrailingSlash(uri.ToString());
			return tempUrl;
		}

		public static Uri CreateUri(string url)
		{
			if (string.IsNullOrEmpty(url))
				return null;
			if (!url.StartsWith(Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase))
				url = Uri.UriSchemeHttp + Uri.SchemeDelimiter + url;
			try
			{
				Uri ret;
				Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out ret);
				return ret;
			}
			catch (Exception) { return null; }
		}

		public static bool isProtocol( string url, params string[] protocols)
		{
			if(string.IsNullOrEmpty(url) || protocols == null || protocols.Length == 0)
				return false;
			bool ret = false;

			foreach(string pro in protocols)
			{
				if(url.StartsWith(pro,  StringComparison.InvariantCultureIgnoreCase))
				{
					ret = true;
					break;
				}

			}
			return ret;
		}
	}
}
