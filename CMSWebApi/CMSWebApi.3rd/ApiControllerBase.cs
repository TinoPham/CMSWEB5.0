using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using Extensions;

namespace CMSWebApi._3rd
{
	public abstract class ApiControllerBase<IService, Business> : ApiController where IService : class  where Business: BusinessServices.BusinessBase<IService>
	{
		protected const string str_APPID = "AppID";
		const int Api_Key_Length = 32;
		protected CultureInfo Culture_Lang = new CultureInfo(Consts.DEFAULT_CULTURE);
		protected Business BusinessService { get; private set; }

		protected _3rdUserContext usercontext { get { return User == null || User.Identity == null ? null : User.Identity as _3rdUserContext; } }

		public ApiControllerBase()
		{
			BusinessService = InitBusiness<Business, IService>() as Business;
		}
		protected string AppSID()
		{
			string guiid = Guid.NewGuid().ToString("n");
			return guiid;
		}

		protected string CreateAPIKey()
		{
			byte [] key, iv;
			Cryptography.Utils.GenerateKeyIV(out key, out iv, Api_Key_Length, Api_Key_Length);
			byte [] bsid = new byte [key.Length + iv.Length];
			Buffer.BlockCopy(key, 0, bsid, 0, key.Length);
			Buffer.BlockCopy(iv, 0, bsid, key.Length, iv.Length);
			string apikey = Convert.ToBase64String(bsid);
			return apikey;
		}

		protected BusinessServices.BusinessBase<IType> InitBusiness<BType, IType>() 
					where IType : class where BType : BusinessServices.BusinessBase<IType>
		{
			BusinessServices.BusinessBase<IType> BService = Resolve(typeof(BType)) as BusinessServices.BusinessBase<IType>;// Commons.ObjectUtils.InitObject<Business>();
			//(BusinessService as BusinessServices.BusinessBase<IService>).User = null;
			(BService as BType).Culture = Culture_Lang;
			IType iservice = DependencyResolver<IType>();
			(BService as BType).DataService = iservice;

			PropertyInfo [] Pinfos = typeof(BType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

		protected HttpResponseMessage ExecuteBusiness(Func<HttpResponseMessage> excute)
		{
			HttpResponseMessage result = null;
			try
			{
				return excute.Invoke();
			}
			catch (Exception ex)
			{
#if DEBUG
				result = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
#else
				result = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
#endif
			}
			return result;
		}

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
			response = Request.CreateResponse<T>(httpcode, data);
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
			//catch (CmsErrorException ehEx)
			//{
			//	response = ResponseData<TransactionalInformation>(ehEx.ErrorInfo, HttpStatusCode.BadRequest);
			//}
			catch (Exception ex)
			{
				//var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR_MSG, ex);
				response = ResponseData<string>(ex.Message, HttpStatusCode.InternalServerError);
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
			//catch (CmsErrorException ehEx)
			//{
			//	response = ResponseData<TransactionalInformation>(ehEx.ErrorInfo, HttpStatusCode.BadRequest);
			//}
			catch (Exception ex)
			{
				//var serverError = new ServerErrorException(false, CMSWebError.SERVER_ERROR_MSG, ex);
				response = ResponseData<string>(ex.Message, HttpStatusCode.InternalServerError);
			}
			return response;
		}

		protected Task<HttpResponseMessage> ExecuteBusinessAcsyn<T>(Func<Task<T>> excute, HttpStatusCode httpcode = HttpStatusCode.OK) where T : class
		{
			return ExecuteBusinessTask<T>(excute, httpcode);

		}

		protected object Resolve(Type type)
		{
			ConstructorInfo constructor = type.GetConstructors().Last();
			ParameterInfo [] parameters = constructor.GetParameters();

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

		protected AppSettings._3rdConfig ClientConfig()
		{
			IEnumerable<string> apps =  Request.Headers.GetValues( str_APPID);
			if( apps == null || apps.Count() != 1)
				return null;
			return AppSettings._3rdAppSettings.Instance.GetConfig(apps.First());
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
		protected HttpResponseMessage ResponseStream(byte [] buff, string format, bool download = false, string filename = null)
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
				var buffer = new byte [BUFFER_SIZE];

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


	public class ApiKeyResult: IHttpActionResult
	{
		public const string API_KEY = "SID";
		HttpStatusCode _httpStatusCode;

		string _value;

		HttpRequestMessage _request;

		public ApiKeyResult(HttpStatusCode httpStatusCode, string apikey, HttpRequestMessage request)
		{
			_httpStatusCode = httpStatusCode;
			_value = apikey;
			_request = request;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = _request.CreateResponse(_httpStatusCode);
			if( _httpStatusCode == HttpStatusCode.OK)
			{
				response.Headers.Add( API_KEY, _value);
			}
			return Task.FromResult(response);
		}

	}

	public class ApiAuthResult: IHttpActionResult
	{
		public const string API_AUTH_KEY = "3rd-auth";
		HttpStatusCode _httpStatusCode;

		string _value;

		HttpRequestMessage _request;
		public ApiAuthResult(HttpStatusCode httpStatusCode, string authkey, HttpRequestMessage request)
		{
			_httpStatusCode = httpStatusCode;
			_value = authkey;
			_request = request;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = _request.CreateResponse(_httpStatusCode);
			if( _httpStatusCode == HttpStatusCode.OK)
			{
				response.Headers.WwwAuthenticate.Add( new AuthenticationHeaderValue(API_AUTH_KEY, _value) );
			}
			return Task.FromResult(response);
		}
	}

	public class ApiActionResult<T> : IHttpActionResult
	{
		HttpStatusCode _httpStatusCode;

		T _value;

		HttpRequestMessage _request;

		public ApiActionResult(HttpStatusCode httpStatusCode, T value, HttpRequestMessage request)
		{
			_httpStatusCode = httpStatusCode;
			_value = value;
			_request = request;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = _request.CreateResponse<T>(_httpStatusCode, _value);
			return Task.FromResult(response);
		}

	}

	public class FileActionResult : IHttpActionResult
	{
		public string FilePath{ get ; private set;}
		public bool Download{ get ;private set;}
		public string CustomFileName{ get ;private set;}
		HttpRequestMessage _request;
		public FileActionResult(HttpRequestMessage request, string filepath, string filename, bool download = false)
		{
			_request = request;
			FilePath = filepath;
			Download = download;
			CustomFileName = filename;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			
			if( File.Exists(FilePath) == false)
				return Task.FromResult( _request.CreateErrorResponse(HttpStatusCode.NoContent, string.Format("'{0}' was removed by system.", string.IsNullOrEmpty(CustomFileName) ? Path.GetFileName(FilePath) : CustomFileName)));

			HttpResponseMessage response = _request.CreateResponse();
			FileStreamContent content = new FileStreamContent(FilePath);
			MediaTypeHeaderValue minetype =new MediaTypeHeaderValue(CMSWebApi.Utils.MineTypesMapping.MinetypeHeader(Path.GetExtension(FilePath)));
			response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)content.WriteToStream, minetype);
			if(Download)
			{
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Consts.STR_attachment);
				response.Content.Headers.ContentDisposition.FileName = string.IsNullOrEmpty(CustomFileName)? Path.GetFileName(FilePath) : CustomFileName;
			}

			return Task.FromResult(response);
		}
	}
}
