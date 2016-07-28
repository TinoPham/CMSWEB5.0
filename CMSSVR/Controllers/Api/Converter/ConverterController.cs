using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using Commons.Resources;
using ConverterSVR.IServices;
using ConvertMessage;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using ApiConverter.Filters;
using CMSSVR.Infrastructure;
using CMSSVR;
using Extensions;
using ConverterSVR.Services;
using System.IO;
using StreamContent;
using System.Net.Http.Headers;

namespace ApiConverter.Controllers.Converter
{

	public class ConverterController : ApiController
	{
		const string STR_User_Agent = "User-Agent";
		const string STR_attachment = "attachment";
		const string STR_APPPlatform = "Platform";
		IConvertService _Iservice;

		public KDVRToken Token { get { return HttpContext.Current.User.Identity as KDVRToken; } }

		public ConverterController( IConvertService isvr)
		{
			if( isvr == null)
			{
				throw new Exception(ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.SERVICE_CANNOT_BE_NULL) );
			}
			_Iservice = isvr;
		}
		[HttpGet]
		public string Converter()
		{
			return DateTime.Now.ToString();
		}

		[HttpPost]
		public HttpResponseMessage ServiceInfo([FromBody] MessageDVRInfo message)
		{
			MessageResult res_header = new MessageResult { ErrorID = Commons.ERROR_CODE.OK, Data = "" };
			return Request.CreateResponse(HttpStatusCode.OK, res_header);
		}


		[HttpPost]
		public HttpResponseMessage DVRLogin([FromBody] MessageDVRInfo message)
		{
			MediaTypeFormatter Formatter = Request.GetFormatter(ControllerContext.Configuration.Formatters);
			
			bool isnewdvr = false;
			MessageResult res_header = _Iservice.DVRRegister(ref message, Formatter, out isnewdvr);
			if (res_header.ErrorID == Commons.ERROR_CODE.OK)
			{
				MacInfo activeMAC = message.MACs.FirstOrDefault( item => item.Active == true);
				KDVRToken token = new KDVRToken{ 
												ActiveMacAddress = activeMAC == null? string.Empty : activeMAC.MAC_Address, 
												KDVR = message.KDVR, 
												HASKeyID = message.HASPK, 
												CreateDate = DateTime.Now, 
												EndDate = DateTime.Now,
												ServerID = AppSettings.AppSettings.Instance.ServerID
												};

				string encryptkey = token.ToString(AppSettings.AppSettings.Instance.DVRTokenKey);
				ConvertMessage.MessageKeepAlive keepalive = KeepAliveService.Instance.DVRKeepAliveMessage(token.KDVR, 0, null, isnewdvr);
				keepalive.ServerID = AppSettings.AppSettings.Instance.ServerID;
				res_header.Data = Commons.ObjectUtils.Serialize<ConvertMessage.MessageKeepAlive>( Formatter, keepalive);
				HttpResponseMessage response = Request.CreateResponse<MessageResult>(HttpStatusCode.OK, res_header);
				response.Headers.WwwAuthenticate.Add( new System.Net.Http.Headers.AuthenticationHeaderValue(AppSettings.AppSettings.TokenKey_Config,encryptkey)); 
				
				return response;
			}
			return Request.CreateResponse(HttpStatusCode.OK, res_header);
		}

		[HttpPost]
		[DVRTokenFilter]
		public async Task<HttpResponseMessage> DVRMessage([FromBody] MessageData message)
		{
			if(Token == null)
				return Request.CreateResponse(HttpStatusCode.Unauthorized);

			MediaTypeFormatter Formatter = Request.GetFormatter(ControllerContext.Configuration.Formatters);
			MessageResult res_header = await _Iservice.DVRMessage(message, Token.SinpleDVRMessageInfo(), Formatter);
			return await Task.FromResult<HttpResponseMessage> (Request.CreateResponse(HttpStatusCode.OK, res_header));
		}

		[HttpGet]
		[DVRTokenFilter]
		public HttpResponseMessage KeepAlive(long id = 0)
		{
			MediaTypeFormatter Formatter = Request.GetFormatter(ControllerContext.Configuration.Formatters);
			string str_agent = Request.GetRequestHeader( STR_User_Agent);
			Version v = Commons.Utils.ParserVersion(str_agent);

			MessageKeepAlive msg = KeepAliveService.Instance.DVRKeepAliveMessage(Token.KDVR, id, v == null? null : v.ToString(), false);
			if(msg == null)
				Request.CreateResponse( HttpStatusCode.NoContent);
			MessageResult msgret = new MessageResult{ ErrorID= Commons.ERROR_CODE.OK, Data = Commons.ObjectUtils.Serialize<ConvertMessage.MessageKeepAlive>( Formatter, msg)};
			return msg == null? Request.CreateResponse( HttpStatusCode.NoContent) : Request.CreateResponse<MessageResult>(msgret);
		}
		
		
		[HttpGet]
		public HttpResponseMessage Version(string id = null)
		{
			string platform = Request.GetHeaderValue(STR_APPPlatform);

			if( string.IsNullOrEmpty(id) && string.IsNullOrEmpty(platform))
				return Version(null, KeepAliveService.STR_X86, false);

			if(string.IsNullOrEmpty(platform))
				platform = id;

			if (string.Compare(platform, KeepAliveService.STR_X86) == 0 || string.Compare(platform, KeepAliveService.STR_X64) == 0)
				return Version(null, platform, false);

			return Version(null, null, true);
			//return Version(null, id == null ? false : true);
		}
		

		[NonAction]
		private HttpResponseMessage Version(string name, string platform, bool description = false)
		{
			if( string.IsNullOrEmpty(name) )
				name = KeepAliveService.Instance.LastVersionName();
			if( string.IsNullOrEmpty(name))
				return Request.CreateResponse(HttpStatusCode.NoContent);
			string iPath, desPath;

			KeepAliveService.Instance.GetInstallversion( name, platform, out iPath, out  desPath);
			string resPath = null;
			if (description == false)//only download installation
				resPath = iPath;
			else
				resPath = desPath;
			if( string.IsNullOrEmpty(resPath) || !File.Exists(resPath))
				return Request.CreateResponse( HttpStatusCode.NotFound);

			HttpResponseMessage response = Request.CreateResponse();
			FileStreamContent content = null;
			MediaTypeHeaderValue minetype = ContentType(Path.GetExtension(resPath));
			if (Request.Headers.Range == null || Request.Headers.Range.Ranges.Count == 0 || Request.Headers.Range.Ranges.FirstOrDefault().From.Value == 0)
			{
				content = new FileStreamContent(resPath);
				response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)content.WriteToStream, minetype);
			}
			else
			{
				var item = Request.Headers.Range.Ranges.FirstOrDefault();
				if (item != null && item.From.HasValue)
				{
					content = new FileStreamContent(resPath, item.From.Value);
					response = Request.CreateResponse(HttpStatusCode.PartialContent);
					response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)content.WriteToStream, minetype);

				}

			}
			if( response != null)
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(STR_attachment) { FileName = name + Path.GetExtension(resPath) };

			return response == null? Request.CreateResponse( HttpStatusCode.NotFound): response;
		}
		[NonAction]
		protected MediaTypeHeaderValue ContentType(CMSWebApi.Utils.MineTypesMapping.MineTypes mtype)
		{
			return ContentType(mtype.ToString());
		}
		[NonAction]
		protected MediaTypeHeaderValue ContentType(string datatype)
		{
			return new MediaTypeHeaderValue(CMSWebApi.Utils.MineTypesMapping.MinetypeHeader(datatype));
		}
	}
}
