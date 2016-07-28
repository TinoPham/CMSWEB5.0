﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace MessageContentHandler
{
	public class NamespaceHttpControllerSelector : DefaultHttpControllerSelector
	{
		private const string ControllerKey = "controller";
		const string str_Namespaces = "Namespaces";
		private readonly HttpConfiguration _configuration;
		private readonly Lazy<IEnumerable<Type>> _duplicateControllerTypes;

		public NamespaceHttpControllerSelector(HttpConfiguration configuration)
			: base(configuration)
		{
			_configuration = configuration;
			_duplicateControllerTypes = new Lazy<IEnumerable<Type>>(GetDuplicateControllerTypes);
		}

		public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
		{
			var routeData = request.GetRouteData();
			if (routeData == null || routeData.Route == null || routeData.Route.DataTokens == null || routeData.Route.DataTokens [str_Namespaces] == null)
				return base.SelectController(request);

			// Look up controller in route data
			object controllerName;
			routeData.Values.TryGetValue(ControllerKey, out controllerName);
			var controllerNameAsString = controllerName as string;
			if (controllerNameAsString == null)
				return base.SelectController(request);

			var namespaces = routeData.Route.DataTokens [str_Namespaces] as IEnumerable<string>;
			//get the currently cached default controllers - this will not contain duplicate controllers found so if
			// this controller is found in the underlying cache we don't need to do anything
			var map = base.GetControllerMapping();
			if (map.ContainsKey(controllerNameAsString))
			{
				HttpControllerDescriptor controller = map [controllerNameAsString];
				if( namespaces.Contains( controller.ControllerType.Namespace))
					return base.SelectController(request);
				return null;
			}

			//the cache does not contain this controller because it's most likely a duplicate, 
			// so we need to sort this out ourselves and we can only do that if the namespace token
			// is formatted correctly.
			
			if (namespaces == null)
				return base.SelectController(request);

			//see if this is in our cache
			var found = _duplicateControllerTypes.Value
				.Where(x => string.Equals(x.Name, controllerNameAsString + ControllerSuffix, StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault(x => namespaces.Contains(x.Namespace));

			if (found == null)
				return base.SelectController(request);

			return new HttpControllerDescriptor(_configuration, controllerNameAsString, found);
		}

		private IEnumerable<Type> GetDuplicateControllerTypes()
		{
			var assembliesResolver = _configuration.Services.GetAssembliesResolver();
			var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
			var controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

			//we have all controller types, so just store the ones with duplicate class names - we don't
			// want to cache too much and the underlying selector caches everything else

			var duplicates = controllerTypes.GroupBy(x => x.Name)
				.Where(x => x.Count() > 1)
				.SelectMany(x => x)
				.ToArray();

			return duplicates;
		}

	}
}
