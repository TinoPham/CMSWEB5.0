using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Net.Http.Formatting;


namespace Extensions
{
	public static class HttpRequestMessageExtensions
	{
		private const string HttpContext = "MS_HttpContext";
		private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
		private const string OwinContext = "MS_OwinContext";

		public static MediaTypeFormatter GetFormatter(this  HttpRequestMessage Request, MediaTypeFormatterCollection FormatterCollection)
		{
			HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accepts = Request.Headers.Accept;
			IEnumerable<string> Accept = Accepts.Select(item => item.MediaType);
			IEnumerable<MediaTypeFormatter> iFormat = FormatterCollection.Where(item => item.SupportedMediaTypes.FirstOrDefault(f => Accept.Contains(f.MediaType, StringComparer.InvariantCultureIgnoreCase)) != null);
			if (iFormat.Count() > 0)
				return iFormat.FirstOrDefault();
			return new JsonMediaTypeFormatter();
		}

		public static MediaTypeWithQualityHeaderValue AcceptHeader(this  HttpRequestMessage Request, string name)
		{
			HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accepts = Request.Headers.Accept;
			return Accepts.FirstOrDefault( item => string.Compare( item.MediaType, name, true) == 0);
			
		}

		/// <summary>
		/// Returns a dictionary of QueryStrings that's easier to work with 
		/// than GetQueryNameValuePairs KevValuePairs collection.
		/// 
		/// If you need to pull a few single values use GetQueryString instead.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
		{
			return request.GetQueryNameValuePairs()
						  .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Returns an individual querystring value
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetQueryString(this HttpRequestMessage request, string key)
		{
			// IEnumerable<KeyValuePair<string,string>> - right!
			var queryStrings = request.GetQueryNameValuePairs();
			if (queryStrings == null)
				return null;

			var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, true) == 0);
			if (string.IsNullOrEmpty(match.Value))
				return null;

			return match.Value;
		}

		/// <summary>
		/// Returns an individual HTTP Header value
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetHeaderValue(this HttpRequestMessage request, string key)
		{
			IEnumerable<string> keys = null;
			if (!request.Headers.TryGetValues(key, out keys))
				return null;

			return keys.First();
		}

		/// <summary>
		/// Retrieves an individual cookie from the cookies collection
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cookieName"></param>
		/// <returns></returns>
		public static string GetCookie(this HttpRequestMessage request, string cookieName)
		{
			CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault();
			if (cookie != null)
				return cookie [cookieName].Value;

			return null;
		}


		public static string GetClientIpAddress(this HttpRequestMessage request)
		{
			// Web-hosting. Needs reference to System.Web.dll
			if (request.Properties.ContainsKey(HttpContext))
			{
				dynamic ctx = request.Properties [HttpContext];
				if (ctx != null)
				{
					return ctx.Request.UserHostAddress;
				}
			}

			// Self-hosting. Needs reference to System.ServiceModel.dll. 
			if (request.Properties.ContainsKey(RemoteEndpointMessage))
			{
				dynamic remoteEndpoint = request.Properties [RemoteEndpointMessage];
				if (remoteEndpoint != null)
				{
					return remoteEndpoint.Address;
				}
			}

			// Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
			if (request.Properties.ContainsKey(OwinContext))
			{
				dynamic owinContext = request.Properties [OwinContext];
				if (owinContext != null)
				{
					return owinContext.Request.RemoteIpAddress;
				}
			}

			return null;
		}
		public static string GetRequestHeader(this HttpRequestMessage request, string header_name)
		{
			IEnumerable<string>header = request.GetRequestHeaders(header_name);
			if(!header.Any())
				return null;
			return string.Join(" ", header);

		}

		public static IEnumerable<string> GetRequestHeaders(this HttpRequestMessage request, string header_name)
		{
			return request.Headers.GetValues( header_name);
		}
		public static Dictionary<string,string>GettHeaders(this HttpRequestMessage request, string header_name)
		{
			Dictionary<string, string> result = new Dictionary<string,string>();
			IEnumerable<string> header = request.GetRequestHeaders(header_name);
			if (!header.Any())
				return result;
			string[]str_header = header.First().Split( new char[]{','},  StringSplitOptions.RemoveEmptyEntries);
			int index = -1;
			foreach(string item in str_header)
			{
				index = item.IndexOf(':');
				if(index <= 0)
					continue;
				result.Add( item.Substring(0, index), item.Substring(index + 1)); 
			}

			return result;
		}
	}
}
