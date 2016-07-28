using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace MessageContentHandler
{

	public class InspectActionSelector : ApiControllerActionSelector, IHttpActionSelector
	{
		const string  str_Action = "action";
		const string str_Controller = "controller";
		
		public InspectActionSelector(HttpConfiguration configuration): base()
		{
		
		}
		public override ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
		{
			ILookup<string, HttpActionDescriptor> result = base.GetActionMapping(controllerDescriptor);
			return result;
		}
		public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
		{
			IHttpRouteData routeData = controllerContext.RouteData;

			bool containsAction = routeData.Values.ContainsKey(str_Action);

			ILookup<string,HttpActionDescriptor> actiondescriptor = GetActionMapping( controllerContext.ControllerDescriptor);
			bool valid_action = containsAction == false ? false : actiondescriptor.Any(m => string.Compare(m.First().ActionName, routeData.Values[str_Action].ToString(), true) == 0);
			if (containsAction && valid_action)
			{
				return base.SelectAction(controllerContext);
			}
 
			try
			{
				string http_method = controllerContext.Request.Method.Method;
				object http_param = routeData.Values [str_Action];
				routeData.Values [str_Action] = http_method;
				if(!routeData.Values.ContainsKey("id"))
				{
					routeData.Values.Add(new KeyValuePair<string,object>("id", http_param));
				}
				return base.SelectAction(controllerContext);

			}
			catch(Exception)
			{
				return null;
			}
			finally
			{
				routeData.Values.Remove("action");
			}
		}
	}
}
