using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Net.Http;

namespace MessageContentHandler
{
	public class ActionSelector : ApiControllerActionSelector
	{
		//config.Routes.MapHttpRoute(
		//name: "DefaultApi",
		//routeTemplate: "api/{controller}/{id}/{action}/{actionid}/{subaction}/{subactionid}",
		//defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional, actionid = RouteParameter.Optional, subaction = RouteParameter.Optional, subactionid = RouteParameter.Optional}
		//);
		const string str_action = "action";
		const string str_subaction = "subaction";
		const string str_controller = "controller";
		

		private readonly IDictionary<ReflectedHttpActionDescriptor, string []> _actionParams = new Dictionary<ReflectedHttpActionDescriptor, string []>();

		public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
		{
			object actionName, subactionName;
			var hasActionName = controllerContext.RouteData.Values.TryGetValue(str_action, out actionName);
			var hasSubActionName = controllerContext.RouteData.Values.TryGetValue(str_subaction, out subactionName);

			var method = controllerContext.Request.Method;
			var allMethods = controllerContext.ControllerDescriptor.ControllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			var validMethods = Array.FindAll(allMethods, IsValidActionMethod);

			var actionDescriptors = new HashSet<ReflectedHttpActionDescriptor>();
			var act = validMethods.Select(m => new ReflectedHttpActionDescriptor(controllerContext.ControllerDescriptor, m));
			foreach (var actionDescriptor in act)
			{
				actionDescriptors.Add(actionDescriptor);

				try
				{
				_actionParams.Add(
					actionDescriptor,
					actionDescriptor.ActionBinding.ParameterBindings
								 .Where(b => !b.Descriptor.IsOptional && b.Descriptor.ParameterType.UnderlyingSystemType.IsPrimitive)
								 .Select(b => b.Descriptor.Prefix ?? b.Descriptor.ParameterName).ToArray());
								 }
								catch(Exception ex)
								{

								}
			}

			IEnumerable<ReflectedHttpActionDescriptor> actionsFoundSoFar;

			if (hasSubActionName)
			{
				actionsFoundSoFar =
					actionDescriptors.Where(
						i => i.ActionName.ToLowerInvariant() == subactionName.ToString().ToLowerInvariant() && i.SupportedHttpMethods.Contains(method)).ToArray();
			}
			else if (hasActionName)
			{
				actionsFoundSoFar =
					actionDescriptors.Where(
						i =>
						i.ActionName.ToLowerInvariant() == actionName.ToString().ToLowerInvariant() &&
						i.SupportedHttpMethods.Contains(method)).ToArray();
			}
			else
			{
				actionsFoundSoFar = actionDescriptors.Where(i => i.ActionName.ToLowerInvariant().Contains(method.ToString().ToLowerInvariant()) && i.SupportedHttpMethods.Contains(method)).ToArray();
			}

			var actionsFound = FindActionUsingRouteAndQueryParameters(controllerContext, actionsFoundSoFar);

			if (actionsFound == null || !actionsFound.Any())
				throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Cannot find a matching action."));
			if (actionsFound.Count() > 1)
				throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(HttpStatusCode.Ambiguous, "Multiple matches found."));

			return actionsFound.FirstOrDefault();
		}

		private IEnumerable<ReflectedHttpActionDescriptor> FindActionUsingRouteAndQueryParameters(HttpControllerContext controllerContext, IEnumerable<ReflectedHttpActionDescriptor> actionsFound)
		{
			var routeParameterNames = new HashSet<string>(controllerContext.RouteData.Values.Keys, StringComparer.OrdinalIgnoreCase);

			if (routeParameterNames.Contains(str_controller))
				routeParameterNames.Remove(str_controller);
			if (routeParameterNames.Contains(str_action))
				routeParameterNames.Remove(str_action);
			if (routeParameterNames.Contains(str_subaction))
				routeParameterNames.Remove(str_subaction);

			var hasQueryParameters = controllerContext.Request.RequestUri != null && !String.IsNullOrEmpty(controllerContext.Request.RequestUri.Query);
			var hasRouteParameters = routeParameterNames.Count != 0;

			if (hasRouteParameters || hasQueryParameters)
			{
				var combinedParameterNames = new HashSet<string>(routeParameterNames, StringComparer.OrdinalIgnoreCase);
				if (hasQueryParameters)
				{
					foreach (var queryNameValuePair in controllerContext.Request.GetQueryNameValuePairs())
					{
						combinedParameterNames.Add(queryNameValuePair.Key);
					}
				}

				actionsFound = actionsFound.Where(descriptor => _actionParams [descriptor].All(combinedParameterNames.Contains));

				if (actionsFound.Count() > 1)
				{
					actionsFound = actionsFound
						.GroupBy(descriptor => _actionParams [descriptor].Length)
						.OrderByDescending(g => g.Key)
						.First();
				}
			}
			else
			{
				actionsFound = actionsFound.Where(descriptor => _actionParams [descriptor].Length == 0);
			}

			return actionsFound;
		}

		private static bool IsValidActionMethod(MethodInfo methodInfo)
		{
			if (methodInfo.IsSpecialName)
				return false;
			return !methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(ApiController));
		}
	}
}
