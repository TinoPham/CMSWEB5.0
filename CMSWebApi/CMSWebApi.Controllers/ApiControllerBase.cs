using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Threading;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using System.Web;
using CMSWebApi.Utils;
using Extensions;
using Cryptography;
using System.Linq.Expressions;
using System.Linq;
using MessageContentHandler;
using System.Net.Http.Headers;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using CMSWebApi.BusinessServices;

namespace CMSWebApi.Controllers
{
	[CMSWebDataFormat]
	public abstract class ApiControllerBase<IService, Business> : ApiController where IService : class where Business : BusinessServices.BusinessBase<IService>
	{
		protected const string SID_KEY = "SID";

		protected CultureInfo Culture_Lang = new CultureInfo(Consts.DEFAULT_CULTURE);

		protected string SID { get; private set; }

		protected UserContext usercontext { get { return User == null || User.Identity == null ? null : User.Identity as UserContext; } }

		//private IService DataService { get; set; }

		protected Business BusinessService { get; private set; }

		protected ApiControllerBase()
		{
			//DataService = DependencyResolver<IService>();
			BusinessService = InitBusiness<Business, IService>() as Business;

		}

		protected HttpResponseMessage UnAuthorizeMessage()
		{
			TransactionalInformation value = new TransactionalInformation();
			value.IsAuthenicated = false;
			value.ReturnStatus = false;
			HttpResponseMessage response = Request.CreateResponse<TransactionalInformation>(System.Net.HttpStatusCode.Unauthorized, value);
			return response;
		}

		protected string InitSID()
		{
			byte[] key, iv;
			Cryptography.Utils.GenerateKeyIV(out key, out iv);
			byte[] bsid = new byte[key.Length + iv.Length];
			Buffer.BlockCopy(key, 0, bsid, 0, key.Length);
			Buffer.BlockCopy(iv, 0, bsid, key.Length, iv.Length);

			return Convert.ToBase64String(bsid);

		}

		protected BusinessServices.BusinessBase<IType> InitBusiness<BType, IType>() where IType : class where BType : BusinessServices.BusinessBase<IType>
		{
			BusinessServices.BusinessBase<IType> BService = Resolve(typeof(BType)) as BusinessServices.BusinessBase<IType>;// Commons.ObjectUtils.InitObject<Business>();
																														   //(BusinessService as BusinessServices.BusinessBase<IService>).User = null;
			(BService as BType).Culture = Culture_Lang;
			IType iservice = DependencyResolver<IType>();
			(BService as BType).DataService = iservice;

			PropertyInfo[] Pinfos = typeof(BType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo p in Pinfos)
			{
				if (!p.CanRead || !p.CanWrite || p.PropertyType.Equals(typeof(IType)) || !p.PropertyType.IsInterface)
					continue;
				p.SetValue(BService, DependencyResolver(p.PropertyType));
			}

			return BService;
		}

		protected object DependencyResolver(Type isvr)
		{
			return GlobalConfiguration.Configuration.DependencyResolver.GetService(isvr);
		}
		protected Isvr DependencyResolver<Isvr>() where Isvr : class
		{
			return DependencyResolver(typeof(Isvr)) as Isvr;
		}

		protected string GetCookie(string cookieName)
		{
			HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
			HttpCookie cookie = cookies[cookieName];
			var clientPath = GetAliasPath(Request.RequestUri.AbsolutePath);
			//if (cookie == null || clientPath != HttpContext.Current.Request.ApplicationPath)
			//{
			//	return null;
			//}

			return cookie == null ? null : cookie.Value;
		}
		protected CookieHeaderValue CreateCookie(string cookieName, string cookieValue, long maxage, bool persitenisPersistentCookie = false)
			{
			CookieHeaderValue cookie = new CookieHeaderValue(cookieName, cookieValue);
			cookie.Expires = DateTime.UtcNow.AddSeconds(maxage);
			if (IsCookieDomain(Request.RequestUri))
				cookie.Domain = Request.RequestUri.Host;
			cookie.Path = HttpContext.Current.Request.ApplicationPath.ToLower(); // "/";
			return cookie;
			}
		protected void SetCookie(string cookieName, string cookieValue, long maxage, bool persitenisPersistentCookie = false)
		{
			HttpCookie cookie = new HttpCookie(cookieName, cookieValue);
			cookie.Expires = DateTime.UtcNow.AddSeconds(maxage);
			if (IsCookieDomain(Request.RequestUri))
				cookie.Domain = Request.RequestUri.Host;
			cookie.Path = HttpContext.Current.Request.ApplicationPath.ToLower(); // "/";
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		protected string GetCookie(HttpRequestMessage rques, string cookieName)
		{
			CookieHeaderValue cookie = rques.Headers.GetCookies(cookieName).FirstOrDefault();
			var clientPath = GetAliasPath(rques.RequestUri.AbsolutePath);
			if(cookie == null || clientPath != HttpContext.Current.Request.ApplicationPath){ return null; }
			return cookie[cookieName].Value;

		}
		protected void SetCookie(HttpResponseMessage res, string cookieName, string cookieValue, int seconds)
		{
			var cookie = new CookieHeaderValue(cookieName, cookieValue);
			cookie.MaxAge = new TimeSpan(0, 0, seconds);
			if (IsCookieDomain(Request.RequestUri))
				cookie.Domain = Request.RequestUri.Host;
			cookie.Path = HttpContext.Current.Request.ApplicationPath.ToLower();  //"/";

			res.Headers.AddCookies(new CookieHeaderValue[] { cookie });
		}
		private bool IsCookieDomain(Uri uri)
		{
			if (uri.HostNameType != UriHostNameType.Dns)
				return true;

			string[] token = uri.DnsSafeHost.Split('.');
			if (token == null || token.Length == 0)
				return false;
			return token.First() != uri.DnsSafeHost;
		}

		/// <summary>
		/// Handler Error and Excute API function
		/// </summary>
		/// <param name="excute">Function excute</param>
		/// <returns></returns>
		protected HttpResponseMessage ExecuteBusiness(Func<HttpResponseMessage> excute)
		{
			HttpResponseMessage result = null;
			try
			{
				return excute.Invoke();
			}
			catch (CmsErrorException ehEx)
			{
				result = Request.CreateResponse(HttpStatusCode.BadRequest, ehEx.ErrorInfo);
			}
			catch (Exception ex)
			{
				var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR_MSG.ToString(), ex);
				result = Request.CreateResponse(HttpStatusCode.InternalServerError, serverError.ErrorInfo);
			}
			return result;
		}

		//protected async  Task<T> ExecuteBusinessAync<T>(Func<Task<T>> excute)
		//{
		//	//HttpResponseMessage result = null;
		//	try
		//	{
		//		Task<T> result = excute();
		//		return await result;
		//	}
		//	catch (CmsErrorException ehEx)
		//	{
		//		result = Request.CreateResponse(HttpStatusCode.BadRequest, ehEx.ErrorInfo);
		//	}
		//	catch (Exception ex)
		//	{
		//		var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR.ToString(), ex);
		//		result = Request.CreateResponse(HttpStatusCode.InternalServerError, serverError.ErrorInfo);
		//	}
		//	return result;
		//}

		protected HttpResponseMessage ResponseData<T>(T data, HttpStatusCode httpcode = HttpStatusCode.OK) where T : class
		{
			HttpResponseMessage response = null;
			Type Ttype = typeof(T);
			if (httpcode != HttpStatusCode.OK)
			{

				MediaTypeWithQualityHeaderValue encptheader = Request.AcceptHeader(MessageContentHandler.Content.EncryptMediaFormatter.Accept_Header);
				if (encptheader != null)
					Request.Headers.Accept.Remove(encptheader);
			}

			if (Ttype.BaseType == null || !Ttype.BaseType.Equals(typeof(TransactionalInformation)))
			{
				TransactionalModel<T> model = TransactionalModel<T>.CreateModel(data);
				response = Request.CreateResponse<TransactionalModel<T>>(httpcode, model);
			}

			else
			{
				response = Request.CreateResponse<T>(httpcode, data);
			}
			return response;
		}
		protected HttpResponseMessage ExecuteBusiness<T>(Func<T> excute, HttpStatusCode httpcode = HttpStatusCode.OK) where T : class
		{
			HttpResponseMessage response = null;
			try
			{
				T result = excute();
				response = ResponseData<T>(result);
			}
			catch (CmsErrorException ehEx)
			{
				response = ResponseData<TransactionalInformation>(ehEx.ErrorInfo, HttpStatusCode.BadRequest);
			}
			catch (Exception ex)
			{
				var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR_MSG, ex);
				response = ResponseData<TransactionalInformation>(serverError.ErrorInfo, HttpStatusCode.InternalServerError);
			}
			return response;
		}
		protected async Task<HttpResponseMessage> ExecuteBusinessTask<T>(Func<Task<T>> excute, HttpStatusCode httpcode = HttpStatusCode.OK) where T : class
		{
			HttpResponseMessage response = null;
			try
			{
				T model = await excute();
				response = ResponseData<T>(model);
			}
			catch (CmsErrorException ehEx)
			{
				response = ResponseData<TransactionalInformation>(ehEx.ErrorInfo, HttpStatusCode.BadRequest);
			}
			catch (Exception ex)
			{
				var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR_MSG, ex);
				response = ResponseData<TransactionalInformation>(serverError.ErrorInfo, HttpStatusCode.InternalServerError);
			}
			return response;
		}
		protected Task<HttpResponseMessage> ExecuteBusinessAcsyn<T>(Func<Task<T>> excute, HttpStatusCode httpcode = HttpStatusCode.OK) where T : class
		{
			return ExecuteBusinessTask<T>(excute, httpcode);

		}
		//
		// Summary:
		//     Executes asynchronously a single HTTP operation.
		//
		// Parameters:
		//   controllerContext:
		//     The controller context for a single HTTP operation.
		//
		//   cancellationToken:
		//     The cancellation token assigned for the HTTP operation.
		//
		// Returns:
		//     The newly started task.
		public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
		{
			(BusinessService as BusinessServices.BusinessBase<IService>).Userctx = this.usercontext;
			SID = controllerContext.Request.GetHeaderValue(SID_KEY);

			return base.ExecuteAsync(controllerContext, cancellationToken);

		}
		protected MediaTypeHeaderValue ContentType(CMSWebApi.Utils.MineTypesMapping.MineTypes mtype)
		{
			return ContentType(mtype.ToString());
		}
		protected MediaTypeHeaderValue ContentType(string datatype)
		{
			return new MediaTypeHeaderValue(CMSWebApi.Utils.MineTypesMapping.MinetypeHeader(datatype));
		}
		protected HttpResponseMessage ResponseFile(string filepath)
		{
			if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
				return Request.CreateResponse(HttpStatusCode.NotFound);
			HttpResponseMessage response = Request.CreateResponse();
			FileStreamContent content = new FileStreamContent(filepath);
			MediaTypeHeaderValue minetype = ContentType(Path.GetExtension(filepath));
			response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)content.WriteToStream, minetype);

			return response;
		}
		protected HttpResponseMessage ResponseExportFile(string filepath)
		{
			//vnd.openxmlformats - officedocument.spreadsheetml.sheet
			if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
				return Request.CreateResponse(HttpStatusCode.NotFound);
			HttpResponseMessage response = Request.CreateResponse();
			response.Content = new ByteArrayContent(File.ReadAllBytes(filepath));
			response.Content.Headers.ContentType = ContentType(MineTypesMapping.MineTypes.stream); //ContentType(MineTypesMapping.MineTypes.xls); //ContentType(Path.GetExtension(filepath));
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Consts.STR_attachment);
			response.Content.Headers.ContentDisposition.FileName = Path.GetFileName(filepath);
			return response;
		}
		protected HttpResponseMessage ResponseStream(byte[] buff, string format, bool download = false, string filename = null)
		{
			if (buff == null)
				return Request.CreateResponse(HttpStatusCode.NotFound);
			if (buff.LongLength == 0)
				return Request.CreateResponse(HttpStatusCode.NoContent);
			HttpResponseMessage response = Request.CreateResponse();
			response.Content = new ByteArrayContent(buff);
			response.Content.Headers.ContentType = ContentType(format);//new MediaTypeHeaderValue(CMSWebApi.Utils.MineTypesMapping.MinetypeHeader(format));
			if (download)
			{
				response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(CMSWebApi.Utils.Consts.STR_attachment);
				response.Content.Headers.ContentDisposition.FileName = filename;
			}
			return response;
		}
		private object Resolve(Type type)
		{
			ConstructorInfo constructor = type.GetConstructors().Last();
			ParameterInfo[] parameters = constructor.GetParameters();

			if (!parameters.Any())
			{
				return Activator.CreateInstance(type);
			}
			else
			{
				return constructor.Invoke(ResolveParameters(parameters).ToArray());
			}
		}
		private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
		{
			return parameters.Select(p => Resolve(p.ParameterType)).ToList();
		}
		private string GetAliasPath(string path)
		{
			string ret = "/";
			if (string.IsNullOrEmpty(path)) { return ret; }

			string[] aliasPath = path.Split('/');
			if(aliasPath.Length > 1)
			{
				if (aliasPath[1] == "api" || aliasPath[1] == "cmsweb") { return ret; }
				ret = ret + aliasPath[1];
			}
			return ret;
		}
	}

	internal class FileStreamContent
	{
		const int BUFFER_SIZE = 65536;
		private string FilePath { get; set; }
		internal FileStreamContent(string filepath)
		{
			FilePath = filepath;
		}

		internal async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
		{
			try
			{
				var buffer = new byte[BUFFER_SIZE];

				using (var reader = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var length = (int)reader.Length;
					var bytesRead = 1;

					while (length > 0 && bytesRead > 0)
					{
						bytesRead = await reader.ReadAsync(buffer, 0, Math.Min(length, buffer.Length));
						await outputStream.WriteAsync(buffer, 0, bytesRead);
						length -= bytesRead;
					}
				}
			}
			catch (HttpException)
			{
				return;
			}
			finally
			{
				outputStream.Close();
			}
		}
	}

}
