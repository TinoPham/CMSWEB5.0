using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;

namespace CMSSVR.Extension
{
	public static class HttpRequestMessageExtention
	{
		public static MediaTypeFormatter GetFormatter(this  HttpRequestMessage Request, MediaTypeFormatterCollection FormatterCollection)
		{
			HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accepts = Request.Headers.Accept;
			IEnumerable<string> Accept = Accepts.Select(item => item.MediaType);
			IEnumerable<MediaTypeFormatter> iFormat = FormatterCollection.Where(item => item.SupportedMediaTypes.FirstOrDefault(f => Accept.Contains(f.MediaType, StringComparer.InvariantCultureIgnoreCase)) != null);
			if (iFormat.Count() > 0)
				return iFormat.FirstOrDefault();
			return new JsonMediaTypeFormatter();
		}
	}
}