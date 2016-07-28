﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CMSWebApi.APIFilters
{
	public class AntiForgeryValidate : ActionFilterAttribute
	{
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			string cookieToken = "";
			string formToken = "";

			IEnumerable<string> tokenHeaders;
			if (actionContext.Request.Headers.TryGetValues("RequestVerificationToken", out tokenHeaders))
			{
				string [] tokens = tokenHeaders.First().Split(':');
				if (tokens.Length == 2)
				{
					cookieToken = tokens [0].Trim();
					formToken = tokens [1].Trim();
				}

			}
			
			System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken);

			base.OnActionExecuting(actionContext);
		}
	}

}
