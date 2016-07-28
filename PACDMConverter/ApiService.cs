using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using ConverterDB.Model;
using ConvertMessage;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.IO;
using PACDMConverter;
using System.Net.Mime;
using System.Collections.Specialized;


namespace PACDMConverter
{
	class ApiService :IDisposable
	{
		const string STR_SERVICE = "SERVICE_";
		const string STR_TOKEN = "Token";
		const string METHOD_LOGIN = "DVRLogin";
		const string METHOD_MESSAGE = "DVRMessage";
		const string METHOD_KEEPALIVE = "KeepAlive";
		const string METHOD_VERSION = "version";
		const string STR_User_Agent = "User-Agent";
		const string STR_DATA = "Data";
		const string STR_attachment = "attachment";
		const string STR_Content_Disposition = "Content-Disposition";
		//#if !DEBUG
		//const int HTTP_TIMEOUT = 60;//60 seconds
		//#else
		//const int HTTP_TIMEOUT = 2 * 60;//60 seconds
		//#endif
		private volatile int Time_Out_Request = 2 * 60;
		public int TimeOutRequest { 
										get { return Time_Out_Request;}
										set{ Time_Out_Request = value;
												if( client != null)
													client.Timeout = new TimeSpan(0,0, value);
										}
									}
		private volatile string Token = string.Empty;
		public string TokenID{ get { return Token;}}
		 
		HttpClient client;
		ServiceConfig SVRConfig;
		Uri UrlWebAPI = null;

		public ApiService (ServiceConfig svrconfig)
		{
			SVRConfig = svrconfig;
			Initialize();
		}

		public ApiService(ServiceConfig svrconfig, string token)
			: this(svrconfig)
		{
			this.Token = token;
			AddAuthenticateData(this.client, token);
		}
		
		public void Dispose()
		{
			if( client != null)
			{
				client.Dispose();
				client  = null;
			}
		}
		
		private void Initialize()
		{
			HttpClientHandler clienthandler = new HttpClientHandler();
			client = new HttpClient(clienthandler);
			if (clienthandler.SupportsAutomaticDecompression)
				clienthandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			clienthandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
			client.DefaultRequestHeaders.TryAddWithoutValidation(STR_User_Agent, Utils.Instance.CovnerterAgent);
			client.DefaultRequestHeaders.TryAddWithoutValidation(Consts.STR_APPID, Utils.Instance.CovnerterAppID);
			client.DefaultRequestHeaders.TryAddWithoutValidation(Consts.STR_APPPlatform, Utils.Instance.Platform);
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			client.Timeout = new TimeSpan(0, 0, TimeOutRequest);

		}
		
		private void SetupAcceptClient(MediaTypeFormatter formatter, HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			formatter.SupportedMediaTypes.ToList().ForEach
			(
				delegate(MediaTypeHeaderValue item)
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue( item.MediaType));
				}
			);

		}
		
		private Uri GetUri(ServiceConfig svrconfig, string action)
		{
			if (UrlWebAPI == null)
			{
				string inputURL = svrconfig.Url.Trim(new char[] { '/' });
				if (!inputURL.StartsWith(Consts.STR_Http) && !inputURL.StartsWith(Consts.STR_Https))
				{
					inputURL = Consts.STR_Http + inputURL;
				}
				//Uri urlpath = new Uri(inputURL);
				//Uri baseURL = new Uri(urlpath, Consts.STR_ConverterURL);
				UrlWebAPI = new Uri (UriExtension.Combine(inputURL, Consts.STR_ConverterURL)); //baseURL;
			}
			//Uri lastURI = new Uri(baseURL, action);
			//LocalDb.AddLog(new Log { DVRDate = DateTime.Now, LogID = (int)Commons.ERROR_CODE.CONVERTER_INVALID_WEBAPI, Message = lastURI.ToString(), ProgramSet = (byte)Commons.Programset.UnknownType, Owner = true });
			//UrlWebAPI = new Uri(baseURL, action);

			return new Uri(UriExtension.Combine(UrlWebAPI.ToString(), action));//(new Uri(svrconfig.Url + "/"), action);
		}
		
		private  Task<HttpResponseMessage> GetToken(HttpClient client, ServiceConfig svrconfig, CancellationToken canceltoken, MediaTypeFormatter Formatter)
		{
			Uri url = GetUri(svrconfig, METHOD_LOGIN);
			HttpRequestMessage rquest = new HttpRequestMessage( HttpMethod.Post, url);
			rquest.Content = new ObjectContent<MessageDVRInfo>(DVRInfos.Instance.MsgDVRInfo, Formatter);

			return client.SendAsync(rquest, canceltoken);//.ConfigureAwait(false);
		}
	
		private Task<HttpResponseMessage> PostData<T>(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter, string token, T msgData) where T : class
		{
			return PostData(client, svrconfig, Formatter,token,msgData, typeof(T));
		}

		private Task<HttpResponseMessage> PostData(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter, string token, object msgData, Type datatype)
		{
			Uri url = GetUri(svrconfig, METHOD_MESSAGE);
			HttpRequestMessage rquest = new HttpRequestMessage(HttpMethod.Post, url);
			rquest.Content = new ObjectContent(datatype, msgData, Formatter);
			AddAuthenticateData(client, this.Token);
			return client.SendAsync(rquest);//.ConfigureAwait(false);
		}
		private Task<HttpResponseMessage> GetData(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter Formatter, CancellationToken canceltoken, string token, string method)
		{
			Uri url = GetUri(svrconfig, method);
			if(!string.IsNullOrEmpty(token))
				AddAuthenticateData(client, this.Token);
			SetupAcceptClient(Formatter, client);
			return client.GetAsync(url, canceltoken);
		}


		private T ReadResponseContent<T>(HttpResponseMessage response, MediaTypeFormatter formatter,CancellationToken canceltoken)
		{
			T ret = default(T);
			try
			{
			Task _task = response.Content.ReadAsAsync<T>(new MediaTypeFormatter[]{ formatter}).ContinueWith
				( 
					task =>
					{
						if( task.IsCompleted)
						{
							ret = task.Result;
						}
					}
				, canceltoken, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
			_task.Wait(canceltoken);
			}
			catch(Exception){}

			return ret;
		}

		private void ClearToken()
		{
			this.Token = string.Empty;
			client.DefaultRequestHeaders.Authorization = null;
		}

		private void AddAuthenticateData( HttpClient client, string token)
		{
			if( client.DefaultRequestHeaders.Authorization == null)
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(STR_TOKEN, token);
		}
		
		private MessageResult ValidateToken(HttpClient client, ServiceConfig svrconfig, MediaTypeFormatter formatter, CancellationToken canceltoken)
		{
			if (string.IsNullOrEmpty(Token) || client.DefaultRequestHeaders.Authorization == null)
			{
				HttpResponseMessage Response = null;
				Task<HttpResponseMessage> response_task;
				try
				{
					Commons.ERROR_CODE err = Commons.ERROR_CODE.OK;
					response_task = GetToken(client, svrconfig,canceltoken, formatter);
					//response_task.Wait(canceltoken);
					//Response = response_task.Result;
					Task tsk_ret = response_task.ContinueWith( res_task => 
						{
							if( res_task.IsCompleted)
								Response = res_task.Result;
							else if( res_task.IsFaulted)
								err = Commons.ERROR_CODE.SERVICE_EXCEPTION;
							else
								err = Commons.ERROR_CODE.CONVERTER_CANCELREQUEST;

						}
					, canceltoken, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
					tsk_ret.Wait(canceltoken);
					tsk_ret.Dispose();
					tsk_ret = null;
					if(err != Commons.ERROR_CODE.OK)
					{
						return new MessageResult{ ErrorID =  err};
					}
					
				}
				catch (HttpRequestException httpex)
				{
					var oskc = (httpex.GetBaseException() as System.Net.Sockets.SocketException);

					if (httpex.InnerException is WebException)
					{
						//throw WebException(httpex.InnerException as WebException);
						return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_GATEWAYTIMEOUT, Data = httpex.InnerException.Message, httpStatus = HttpStatusCode.NotFound };
					}
					return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_SERVICEUNAVAILABLE, Data = httpex.InnerException.Message, httpStatus = HttpStatusCode.NotFound };
				}
				catch(System.AggregateException aggex)
				{
					if( canceltoken.IsCancellationRequested)
						return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, Data = aggex.Message, httpStatus = HttpStatusCode.BadRequest};
					else
						return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_GATEWAYTIMEOUT, Data = aggex.Message, httpStatus = HttpStatusCode.BadRequest };
				}
				catch(System.ObjectDisposedException dpsex)
				{

					return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_DISPOSEED, Data = dpsex.Message, httpStatus = HttpStatusCode.BadRequest };
				}
				catch(Exception ex)
				{

					return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message, httpStatus = HttpStatusCode.BadRequest };
				}

				if (Response.StatusCode != HttpStatusCode.OK)
				{
					
					return new MessageResult{ httpStatus = Response.StatusCode, ErrorID = HttpStatus_to_Error(Response.StatusCode), Data  = Commons.Resources.ResourceManagers.Instance.GetResourceString( Commons.ERROR_CODE.SERVICE_EXCEPTION)};
				}

				MessageResult Result_msg = ReadResponseContent<MessageResult>(Response,formatter, canceltoken);//Response.Content.ReadAsAsync<MessageResult>().Result;
				if( Result_msg == null )
				{
					Result_msg = new MessageResult();
					if( canceltoken.IsCancellationRequested)
						Result_msg.ErrorID = Commons.ERROR_CODE.CONVERTER_CANCELREQUEST;
					else
						Result_msg.ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION;

					Result_msg.httpStatus = Response.StatusCode;
					return Result_msg;
				}
				if( canceltoken.IsCancellationRequested)
				{
					Result_msg.ErrorID = Commons.ERROR_CODE.CONVERTER_CANCELREQUEST;
				}

				if(Result_msg.ErrorID == Commons.ERROR_CODE.OK)
				{
					AuthenticationHeaderValue header =  Response.Headers.WwwAuthenticate.FirstOrDefault();
					this.Token = header.Parameter;
					AddAuthenticateData(client, this.Token);
				}
				else
					ClearToken();

				Result_msg.httpStatus = Response.StatusCode;
				return Result_msg;
			}

			return new MessageResult{ ErrorID= Commons.ERROR_CODE.OK};
		}
		
		public MessageResult PostData<T>(T msgdata, CancellationToken canceltoken, MediaTypeFormatter dataformatter = null)
		{
			return PostData( msgdata, typeof(T), canceltoken, dataformatter);
		}

		public T PostData<T>(object msgdata, Type objType, CancellationToken canceltoken, MediaTypeFormatter dataformatter) where T : class
		{
			MessageResult result = PostData( msgdata, canceltoken, dataformatter);
			if( result.ErrorID == Commons.ERROR_CODE.OK)
				return Commons.ObjectUtils.DeSerialize<T>(dataformatter, result.Data) as T; 
			return default(T);
		}
		
		public MessageResult PostData(object msgdata, Type objType,CancellationToken canceltoken, MediaTypeFormatter dataformatter = null)
		{
			//HttpResponseMessage Response = null;
			Task<HttpResponseMessage> response_task ;
			if( dataformatter == null)
				dataformatter = new JsonMediaTypeFormatter();
			SetupAcceptClient( dataformatter, this.client );
			MessageResult msgResult = ValidateToken(client, SVRConfig, dataformatter, canceltoken);
			if ( msgResult.ErrorID != Commons.ERROR_CODE.OK )
				return msgResult;
			response_task = PostData(client, SVRConfig, dataformatter, this.Token, msgdata, msgdata.GetType());
			MessageResult result = TaskResponse( response_task,dataformatter,canceltoken);
			if( result != null && (result.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_EXPIRED || result.ErrorID == Commons.ERROR_CODE.SERVICE_TOKEN_INVALID) )
				ClearToken();

			return result;
		}

		public MessageResult Login(CancellationToken canceltoken, MediaTypeFormatter dataformatter = null)
		{
			ClearToken();
			if (dataformatter == null)
				dataformatter = new JsonMediaTypeFormatter();
			SetupAcceptClient(dataformatter, this.client);
			return ValidateToken(client, SVRConfig, dataformatter, canceltoken);
			
		}

		public MessageResult GetNewVesion( string version, CancellationToken canceltoken, string dir = null, bool overwrite = true)
		{
			string fpath = null;
			HttpStatusCode httpcode = HttpStatusCode.OK;
			Task<HttpResponseMessage> tresponse = GetData(client, this.SVRConfig, new JsonMediaTypeFormatter(), canceltoken, this.Token, METHOD_VERSION);
			Task copytask =null;
			try
			{
			Task result = tresponse.ContinueWith( (requestTask) =>
													{
														// Get HTTP response from completed task.
														HttpResponseMessage response = requestTask.Result;
														httpcode = response.StatusCode;
														if( response.StatusCode == HttpStatusCode.OK)
														{
															// Check that response was successful or throw exception
															try{
															response.EnsureSuccessStatusCode();

															// Read content into buffer
															response.Content.LoadIntoBufferAsync();
															copytask = ReadAsFileAsync(response.Content, dir, canceltoken, true, out fpath);
															copytask.Wait(canceltoken);
															}catch(Exception){}
														}
														
													}, canceltoken);
			result.Wait(canceltoken);
			return new MessageResult{ ErrorID = Commons.ERROR_CODE.OK, Data = fpath, httpStatus = httpcode}; //Commons.ERROR_CODE.OK;
			}catch(Exception ex){
				return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message, httpStatus = HttpStatusCode.BadRequest }; 
			}
		}

		public MessageResult KeepAlive(Int64 token, CancellationToken canceltoken, MediaTypeFormatter dataformatter = null)
		{
			if( string.IsNullOrEmpty(Token))
				return new MessageResult{ ErrorID = Commons.ERROR_CODE.SERVICE_TOKEN_INVALID};

			if (dataformatter == null)
				dataformatter = new JsonMediaTypeFormatter();
			SetupAcceptClient(dataformatter, this.client);
			Task<HttpResponseMessage> tresponse = GetData(client,this.SVRConfig, dataformatter, canceltoken, this.Token, string.Format("{0}/{1}", METHOD_KEEPALIVE, token.ToString()) );
			return TaskResponse(tresponse, dataformatter, canceltoken);
		}
		
		private MessageResult TaskResponse(Task<HttpResponseMessage> Tresponse, MediaTypeFormatter dataformatter, CancellationToken canceltoken)
		{
			if( Tresponse == null)
				return new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION};
			try
			{
				Task<MessageResult> Tresult = Tresponse.ContinueWith<MessageResult>(res_task =>
						{
						if( res_task.IsCompleted && !res_task.IsFaulted)
							return ToMessageResult( res_task.Result, dataformatter, canceltoken);
						else if (res_task.IsCanceled)
						{
							return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, httpStatus = HttpStatusCode.BadRequest };
						}
						else
						{
							return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, httpStatus = HttpStatusCode.BadRequest };
						}

					}, canceltoken, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
				Tresult.Wait( canceltoken);
				MessageResult ret = Tresult.Result;
				Tresult.Dispose();
				Tresult = null;
				Tresponse.Dispose();
				Tresponse = null;
				return ret;

			}
			catch (System.AggregateException aggex)
			{
				if (Tresponse != null && Tresponse.Exception != null && Tresponse.Exception.InnerException != null && Tresponse.Exception.InnerException is HttpRequestException  )
				{
					HttpRequestException RException = Tresponse.Exception.InnerException as HttpRequestException;
					WebExceptionStatus wstatus = (RException.InnerException as WebException).Status;
					return new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_SERVICEUNAVAILABLE, Data = RException.Message, httpStatus = HttpStatusCode.NotFound };
					//return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, Data = aggex.Message, httpStatus = HttpStatusCode.BadRequest };
				}
				else
					return new MessageResult { ErrorID = canceltoken.IsCancellationRequested? Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL: Commons.ERROR_CODE.SERVICE_GATEWAYTIMEOUT, Data = aggex.InnerExceptions.Any()? aggex.InnerExceptions.First().Message : aggex.Message, httpStatus = canceltoken.IsCancellationRequested? HttpStatusCode.BadRequest : HttpStatusCode.RequestTimeout};
			}
			catch (System.ObjectDisposedException dpsex)
			{
					return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_DISPOSEED, Data = dpsex.Message, httpStatus = HttpStatusCode.BadRequest };
			}
			catch (Exception ex)
			{
				return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_EXCEPTION, Data = ex.Message, httpStatus = HttpStatusCode.BadRequest };
			}
		}
		
		private MessageResult ToMessageResult(HttpResponseMessage response, MediaTypeFormatter dataformatter, CancellationToken canceltoken)
		{
			if( response.StatusCode != HttpStatusCode.OK && response.StatusCode >= HttpStatusCode.InternalServerError)
			{
				return new MessageResult{ ErrorID = HttpStatus_to_Error(response.StatusCode), httpStatus = response.StatusCode};
			}
			MessageResult result = ReadResponseContent<MessageResult>(response, dataformatter, canceltoken);
			if( result != null && response != null)
				result.httpStatus = response.StatusCode;
			return result;

		}

		private Task ReadAsFileAsync(HttpContent content, string dir, CancellationToken canceltoken, bool overwrite, out string filepath)
		{
			string pathname = FilePath(dir, content);
			filepath = pathname;
			FileStream fileStream = null;
				try{
						fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
						return content.CopyToAsync(fileStream).ContinueWith(
																			copyTask =>
																			{
																				fileStream.Close();
																			}, canceltoken);
				}
				catch(Exception)
				{
					if (fileStream != null)
						{fileStream.Close();}
					return TaskEx.FromResult<int>(0);
				}
		}

		private Commons.ERROR_CODE HttpStatus_to_Error(HttpStatusCode status)
		{
			if(status == HttpStatusCode.OK)
				return Commons.ERROR_CODE.OK;
			if( (int)status >= (int)HttpStatusCode.InternalServerError)
			{
				string str_Status = STR_SERVICE + status.ToString();
				return Commons.Utils.GetEnum<Commons.ERROR_CODE>(str_Status);
			}
			return Commons.ERROR_CODE.SERVICE_EXCEPTION;
		}
		
		private string FilePath(string dir, HttpContent content)
		{
			if( content == null)
				return dir;
			string cpString = content.Headers.GetValues(STR_Content_Disposition).FirstOrDefault();
			if (string.IsNullOrEmpty(cpString))
				return dir;
			
			ContentDisposition contentDisposition = new ContentDisposition(cpString);
			string filename = contentDisposition.FileName;
			StringDictionary parameters = contentDisposition.Parameters;

			return Path.Combine(dir, filename);

		}

	}
}
