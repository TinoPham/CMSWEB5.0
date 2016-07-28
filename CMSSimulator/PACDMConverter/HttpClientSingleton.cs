using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConverterDB.Model;
using ConvertMessage;

namespace PACDMSimulator
{
	class HttpClientSingleton
	{
		const string STR_TOKEN = "Token";
		const string METHOD_LOGIN = "DVRLogin";
		const string METHOD_MESSAGE = "DVRMessage";
		const string STR_User_Agent = "User-Agent";
		//private static readonly Lazy<HttpClientSingleton> Lazy = new Lazy<HttpClientSingleton>(() => new HttpClientSingleton());
		private HttpClient client;
		private volatile string Token = string.Empty;

		//public static HttpClientSingleton Instance { get { return Lazy.Value; } }
		ServiceConfig SVRConfig;
		MessageDVRInfo Info;

		private CancellationTokenSource _cancellation;

		public HttpClientSingleton(ServiceConfig svrconfig)
		{
			SVRConfig = svrconfig;
		}

		public HttpClientSingleton(ServiceConfig svrconfig, MessageDVRInfo info, CancellationTokenSource cancellation)
		{
			SVRConfig = svrconfig;
			Info = info;
			_cancellation = cancellation;
		}

		private HttpClient SetupClient(MediaTypeFormatter formatter)
		{

			HttpClientHandler clienthandler = new HttpClientHandler();

			if (clienthandler.SupportsAutomaticDecompression)
				clienthandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			clienthandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
			HttpClient client = new HttpClient(clienthandler);
			client.DefaultRequestHeaders.TryAddWithoutValidation(STR_User_Agent, Utils.CovnerterAgent);
			//client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(STR_TOKEN, Token);
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			formatter.SupportedMediaTypes.ToList().ForEach
			(
				delegate(MediaTypeHeaderValue item)
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue( item.MediaType));
				}
			);
			client.Timeout = new TimeSpan(0, 0,3,0);
			return client;
			
		}
		
		Uri GetUri(ServiceConfig svrconfig, string action)
		{
			string urlpath = svrconfig.Url.Trim( new char[]{'/'});
			return new Uri(new Uri(svrconfig.Url + "/"), action);
		}
		
		private async Task<HttpResponseMessage> GetToken(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter)
		{
			Uri url = GetUri(svrconfig, METHOD_LOGIN);
			HttpRequestMessage rquest = new HttpRequestMessage( HttpMethod.Post, url);
			if (Info == null)
			{
				rquest.Content = new ObjectContent<MessageDVRInfo>(DVRInfos.Instance.MsgDVRInfo, Formatter);
			}
			else
			{
				rquest.Content = new ObjectContent<MessageDVRInfo>(Info, Formatter);
			}
			
			return await client.SendAsync(rquest,_cancellation.Token).ConfigureAwait(false);
		}
	
		private Task<HttpResponseMessage> PostData<T>(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter, string token, T msgData) where T:class
		{
			return PostData(client, svrconfig, Formatter,token,msgData, typeof(T));
		}

		private async Task<HttpResponseMessage> PostData(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter, string token, object msgData, Type datatype)
		{
			Uri url = GetUri(svrconfig, METHOD_MESSAGE);
			HttpRequestMessage rquest = new HttpRequestMessage(HttpMethod.Post, url);
			rquest.Content = new ObjectContent(datatype, msgData, Formatter);
			AddAuthenticateData(client);
			return await client.SendAsync(rquest, _cancellation.Token).ConfigureAwait(true);
		}
		
		private void AddAuthenticateData( HttpClient client)
		{
			if( client.DefaultRequestHeaders.Authorization == null)
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(STR_TOKEN, Token);
		}
		
		private MessageResult ValidateToken(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter formatter)
		{
			if (string.IsNullOrEmpty(Token))
			{
				HttpResponseMessage Response = null;
				Task<HttpResponseMessage> response_task = null;
				try
				{
					response_task = GetToken(client, svrconfig, formatter);
					response_task.Wait();
					Response = response_task.Result;
				}
				catch(System.AggregateException aggex)
				{
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, Data = aggex.Message};
				}
				catch(System.ObjectDisposedException dpsex)
				{
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_DISPOSEED, Data = dpsex.Message};
				}
				catch(Exception ex)
				{
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message};
				}

				if (Response.StatusCode != HttpStatusCode.OK)
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data  = Commons.Resources.ResourceManagers.Instance.GetResourceString( Commons.ERROR_CODE.SERVICE_EXCEPTION)};
				MessageResult Result_msg = Response.Content.ReadAsAsync<MessageResult>().Result;

				if( Result_msg.ErrorID == Commons.ERROR_CODE.OK)
				{
					AuthenticationHeaderValue header =  Response.Headers.WwwAuthenticate.FirstOrDefault();
					this.Token = header.Parameter;
					AddAuthenticateData(client);
				}
			else
					AddAuthenticateData(client);

				return Result_msg;
			}

			return new MessageResult{ ErrorID= Commons.ERROR_CODE.OK};
		}
		
		public MessageResult PostData<T>(T msgdata, MediaTypeFormatter dataformatter = null)
		{
			return PostData( msgdata, typeof(T), dataformatter);
		}

		public T PostData<T>(object msgdata, Type objType, MediaTypeFormatter dataformatter) where T: class
		{
			MessageResult result = PostData( msgdata, dataformatter);
			if( result.ErrorID == Commons.ERROR_CODE.OK)
				return Commons.ObjectUtils.DeSerialize<T>(dataformatter, result.Data) as T; 
			return default(T);
		}

		public void CancelRequest()
		{
			//client.CancelPendingRequests();
			if(_cancellation != null)
				_cancellation.Cancel(true);
		}

		public void SetCancelToken(CancellationTokenSource token)
		{
			_cancellation = token;
		}

		public MessageResult PostData(object msgdata, Type objType, MediaTypeFormatter dataformatter = null)
		{
			HttpResponseMessage Response = null;
			Task<HttpResponseMessage> response_task = null;
			if( dataformatter == null)
				dataformatter = new JsonMediaTypeFormatter();
			client = SetupClient( dataformatter);
			MessageResult msgResult = ValidateToken(client, SVRConfig, dataformatter);
			if ( msgResult.ErrorID != Commons.ERROR_CODE.OK )
				return msgResult;

			try
			{
				response_task = PostData(client, SVRConfig, dataformatter, this.Token, msgdata, msgdata.GetType());
				response_task.Wait(_cancellation.Token);
				Response = response_task.Result;
				if( Response.StatusCode != HttpStatusCode.OK)
					return new MessageResult{ ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data = Response.Content == null? string.Empty : Response.Content.ReadAsStringAsync().Result};
				return Response.Content.ReadAsAsync<MessageResult>().Result;
			}
			catch(System.AggregateException aggex)
			{
				return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, Data = aggex.Message};
			}
			catch(System.ObjectDisposedException dpsex)
			{
				return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_DISPOSEED, Data = dpsex.Message};
			}
			catch(Exception ex)
			{
				return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message};
			}

		}
	}
}
