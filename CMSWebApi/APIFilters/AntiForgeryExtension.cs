using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages.Html;

namespace CMSWebApi.APIFilters
{
	public static class AntiForgeryExtension
	{
		public static string RequestVerificationToken(this HtmlHelper helper)
		{
			return String.Format("ncg-request-verification-token={0}", GetTokenHeaderValue());
		}

		public static string GetTokenHeaderValue()
		{
			string cookieToken, formToken;
			System.Web.Helpers.AntiForgery.GetTokens(null, out cookieToken, out formToken);
			return cookieToken + ":" + formToken;
		}
	}
}
