using System;
using System.Collections;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.Routing;
using CMSSVR.Filter;
using MessageContentHandler;
using MessageContentHandler.Content;
using MessageContentHandler.DelegatingHandler;
using Extensions.HttpRouteCollection;
using System.Web.Http.Controllers;

namespace CMSSVR
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			#region
			//config.Routes.MapHttpRoute(
			//	name: "DefaultApi",
			//	routeTemplate: "api/{controller}/{id}",
			//	defaults: new { id = RouteParameter.Optional }
			//);

			// Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
			// To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
			// For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
			//config.EnableQuerySupport();

			// To disable tracing in your application, please comment out or remove the following line of code
			// For more information, refer to: http://www.asp.net/web-api
			//Add route for converter
			#endregion


			
			config.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(config));
			config.Services.Replace(typeof(IHttpActionSelector), new InspectActionSelector(config));

			config.MapHttpAttributeRoutes();
			config.Formatters.Insert(0, new EncryptMediaFormatter( config, AppSettings.AppSettings.Instance.MessageDelay));
			config.SetTimeZoneInfo(TimeZoneInfo.Utc);

			RegisterConverter(config);
			RegisterCMSWebApi(config);
			Register3rdApp( config);

			#if DEBUG
			config.EnableSystemDiagnosticsTracing();
			#endif
			config.Filters.Add(new CmsApiHandledExceptionFilter());
		}
		private static void RegisterConverter(HttpConfiguration config)
		{
			#region
			//string[] Ignore_Check_Header_Actions = new string[]{"DVRLogin"};

			//config.Routes.MapHttpRoute(
			//	name: "ConverterApi",
			//	routeTemplate: "api/{controller}/{action}",
			//	defaults: new { action = "DVRLogin" },
			//	constraints: null,
			//	handler: new ConverterMessageHandler(GlobalConfiguration.Configuration, Ignore_Check_Header_Actions)
			//);
			#endregion

			IHttpRoute route = null;
			//route = config.Routes.MapHttpRoute(
			//	name: "ConverterApiVersion",
			//	routeTemplate: "api/converter/{controller}/version/{id}",
			//	defaults: new { controller = "Converter"},
			//	constraints: null,
			//	handler: new CompressMessageHandler(GlobalConfiguration.Configuration)
			//	, tokens: new { Namespaces = new string [] { "ApiConverter.Controllers.Converter", "CMSSVR.Controllers.Api.Converter" } }
			//);
			

			route = config.Routes.MapHttpRoute(
				name: "ConverterApi",
				routeTemplate: "api/converter/{controller}/{action}/{id}",
				defaults: new { action = "DVRLogin", controller = "Converter", id = RouteParameter.Optional, detail = RouteParameter.Optional },
				constraints: null,
				handler: new CompressMessageHandler(GlobalConfiguration.Configuration)
				, tokens: new { Namespaces = new string [] { "ApiConverter.Controllers.Converter", "CMSSVR.Controllers.Api.Converter" } }
			);
			//route = config.Routes.MapHttpRoute(
			//	name: "ConverterApiname",
			//	routeTemplate: "api/converter/{controller}/{action}/{id}/{detail}",
			//	defaults: new { action = "DVRLogin", controller = "Converter", detail  = RouteParameter.Optional},
			//	constraints: new { detail = @"^(detail)?$" },
			//	handler: new CompressMessageHandler(GlobalConfiguration.Configuration)
			//	, tokens: new { Namespaces = new string [] { "ApiConverter.Controllers.Converter", "CMSSVR.Controllers.Api.Converter" } }
			//);
		}
		private static void RegisterCMSWebApi(HttpConfiguration config)
		{
			var route =  config.Routes.MapHttpRoute(
				 name: "CMSWebApi",
				 routeTemplate: "api/cmsweb/{controller}",
				 defaults: new { action = "Account", controller = "Account" },
				 constraints: null,
				 handler: new CompressMessageHandler(GlobalConfiguration.Configuration)
				 , tokens: new { Namespaces = new string [] { "CMSWebApi.Controllers" }}
			 );

			route = config.Routes.MapHttpRoute(
				name: "CMSWebApiAction",
				routeTemplate: "api/cmsweb/{controller}/{action}/{id}",
				defaults: new { action = "Account", controller = "Account", id = RouteParameter.Optional },
				constraints: null,
				handler: new CompressMessageHandler(GlobalConfiguration.Configuration),
				tokens: new { Namespaces = new string [] { "CMSWebApi.Controllers" }}
			);
		}

		private static void Register3rdApp(HttpConfiguration config)
		{
			var route = config.Routes.MapHttpRoute(
				 name: "3rdApi",
				 routeTemplate: "api/3rd/{controller}/{id}",
				 defaults: new{ id = RouteParameter.Optional},
				 constraints: null,
				 handler: new CompressMessageHandler(GlobalConfiguration.Configuration)
				 , tokens: new { Namespaces = new string [] { "CMSWebApi._3rd" } }
			 );
			route = config.Routes.MapHttpRoute(
			   name: "3rdApiAction",
			   routeTemplate: "api/3rd/{controller}/{id}/{action}",
			   defaults: null,
			   constraints: null,
			   handler: new CompressMessageHandler(GlobalConfiguration.Configuration),
			   tokens: new { Namespaces = new string [] { "CMSWebApi._3rd" } }
		   );
		}

	}

}
