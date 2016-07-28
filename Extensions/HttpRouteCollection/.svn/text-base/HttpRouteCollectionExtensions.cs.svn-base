using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using System.Web.Http;

namespace Extensions.HttpRouteCollection
{
	public static class HttpRouteCollectionExtensions
	{
		public static IHttpRoute MapHttpRoute(this System.Web.Http.HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, object tokens)
		{
			if (routes == null)
			{
				throw new System.Exception("Routes");
			}
			HttpRouteValueDictionary Dictionary_Default = new HttpRouteValueDictionary(defaults);
			HttpRouteValueDictionary Dictionary_Contrains = new HttpRouteValueDictionary(constraints);
			HttpRouteValueDictionary Dictionary_Tokens = new HttpRouteValueDictionary(tokens);
			IHttpRoute httpRoute = routes.CreateRoute(routeTemplate, Dictionary_Default, Dictionary_Contrains, Dictionary_Tokens, handler);
			routes.Add(name, httpRoute);
			return httpRoute;
		}
	}
}
