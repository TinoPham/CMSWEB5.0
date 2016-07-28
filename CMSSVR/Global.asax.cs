using System;
using System.Diagnostics;
using System.Web;
using System.Web.Http;

using System.Web.Optimization;
using System.Web.Routing;
using CMSSVR.Infrastructure;
using Microsoft.Ajax.Utilities;

namespace CMSSVR
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			//AreaRegistration.RegisterAllAreas();
		
			//FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			UnityConfig.RegisterComponents();
			//WebApiConfig.Register(GlobalConfiguration.Configuration);
			GlobalConfiguration.Configure(WebApiConfig.Register);//2.0
			//GlobalConfiguration.Configure(ApiConverter.WebApiConfig.Register);//2.0

			//BundleConfig.RegisterBundles(BundleTable.Bundles);
			//ViewEngines.Engines.Add(new CustomRazorViewEngine.FolderPerFeatureConventionViewEngine());

		}
	}
}