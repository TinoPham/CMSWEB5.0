using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using System.IO;

namespace CMSWebApi.APIFilters
{
	public class ActivityLogAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override async void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			try
			{
				IActivityLogService IActivity = actionContext.RequestContext.Configuration.DependencyResolver.GetService(typeof(IActivityLogService)) as IActivityLogService;
				tCMSWeb_ActivityLog activityLog = new tCMSWeb_ActivityLog();
				HttpContext ctx = default(HttpContext);
				ctx = HttpContext.Current;
				var request = actionContext.Request;
				var routeData = request.GetRouteData();

				if (routeData != null && routeData.Route != null && routeData.Route.DataTokens["Namespaces"] != null)
				{
					// Look up controller, action in route data
					object controllerName;
					routeData.Values.TryGetValue(Consts.ControllerKey, out controllerName);
					activityLog.Controller = controllerName as string;
					object actionName;
					routeData.Values.TryGetValue(Consts.ActionKey, out actionName);
					activityLog.Action = actionName as string;
				}

				var headers = request.Headers;
				var cook = headers.GetCookies(Consts.XSRF_TOKEN_KEY).FirstOrDefault();
				string token = cook == null ? null : cook[Consts.XSRF_TOKEN_KEY].Value;
				LoginModel model = WebUserToken.GetModel(token);

				if (model != null)
				{
					activityLog.UserID = model.ID;
				}

				string IpAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : string.Empty;
				activityLog.IPAddress = IpAddress.Replace(":", "-");
				activityLog.Method = request.Method.Method.ToString();
				activityLog.PageURL = request.RequestUri.ToString();

				//write log file
				string contentLog = string.Empty;
				DateTime timeNow = DateTime.Now;
				activityLog.ActivityDate = timeNow;
				string time = timeNow.ToString(Consts.LOG_DATE_FORMAT).Replace(":", "-");
				string fileName = string.Format("{0}_{1}_{2}_{3}.txt", IpAddress, activityLog.UserID, activityLog.Action, time);
				string path = Path.Combine(AppSettings.AppSettings.Instance.LogsPath, fileName);

				Stream stream = await request.Content.ReadAsStreamAsync();
				stream.Position = 0;
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					contentLog = reader.ReadToEnd();
				}
				bool result = await WriteFile(path, System.Text.Encoding.UTF8.GetBytes(contentLog));

				activityLog.Data = result ? path : null;
				activityLog = IActivity.Add(activityLog);
			}
			catch (Exception)
			{
				
			}
			base.OnActionExecuting(actionContext);
		}

		private async Task<bool> WriteFile(string fpath, byte[] mem, bool overwrite = true)
		{
			if (mem == null || mem.Length == 0 || string.IsNullOrEmpty(fpath))
				return false;
			string dir = Path.GetDirectoryName(fpath);
			if (!CMSWebApi.Utils.Utilities.CreateDir(dir))
				return false;
			if (File.Exists(fpath) && overwrite == false)
				return true;

			FileStream fs = null;
			try
			{
				fs = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
				await fs.WriteAsync(mem, 0, (int)mem.Length); 
				return fs.Length == mem.Length;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
					fs = null;
				}
			}
		}
	}
}
