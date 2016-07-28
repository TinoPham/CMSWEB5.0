using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public static class HttpResponseMessageExtensions
	{
		public static void SetHeaderValue(this HttpResponseMessage response, string key, string value)
		{
			IEnumerable<string> keys = null;
			keys = new List<string>() { value };
			response.Headers.Add(key, keys);
		}
		/// <summary>
		/// Set header cookie to response message
		/// </summary>
		/// <param name="response"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="timeout"></param>
		public static void SetHeaderCookie(this HttpResponseMessage response, string key, string value, TimeSpan timeout)
		{
			CookieHeaderValue cookie = new CookieHeaderValue(key, value);
			cookie.MaxAge = timeout;

			response.Headers.AddCookies(new List<CookieHeaderValue> { cookie });
		}

		public static void DeleteHeaderCookie(this HttpResponseMessage response, string key)
		{
			response.SetHeaderCookie(key, string.Empty, new TimeSpan(DateTime.MinValue.Ticks));
		}
	}
}
