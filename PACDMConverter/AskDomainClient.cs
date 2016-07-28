using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using ConvertMessage;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.Sockets;

namespace PACDMConverter
{
	internal class AskDomainClient
	{
		const string DVR_Controller = "api/AskDomain";
		const string DVR_Controller_SID = "api/AskDomain/?sid={0}";
		const string DVR_Controller_SID_Time = "api/AskDomain/?sid={0}&t={1}";

		readonly Uri baseUrl;
		readonly HttpClient client;
		readonly MediaTypeFormatter _DataFormat = new JsonMediaTypeFormatter();
		public MediaTypeFormatter DataFormat{ get{ return _DataFormat;}}
		public AskDomainClient( string baseurl)
		{
			baseUrl = CorrectUrl(baseurl);
			if( baseurl != null)
			{
				client = new HttpClient();

				client.BaseAddress = baseUrl;
				SetupAcceptClient(DataFormat, client);
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			}
		}
		private Uri CorrectUrl(string url)
		{
			if( string.IsNullOrEmpty( url) )
				return null;
			Uri ret;
			if( !url.StartsWith( Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith( Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase))
			{
				Uri.TryCreate(Uri.UriSchemeHttp + Uri.SchemeDelimiter + url, UriKind.RelativeOrAbsolute, out ret);
				return ret;
			}
			Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out ret);
			return ret;
			
		}
		private void SetupAcceptClient(MediaTypeFormatter formatter, HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			formatter.SupportedMediaTypes.ToList().ForEach
			(
				delegate(MediaTypeHeaderValue item)
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(item.MediaType));
				}
			);

		}
		
		public MessageResult GetDomain(string token, CancellationToken canceltoken)
		{
			string url = string.IsNullOrEmpty(token) ? DVR_Controller : string.Format(DVR_Controller_SID, HttpUtility.UrlEncode(token));
			Task<HttpResponseMessage> tskresponse = client.GetAsync(url, canceltoken);

			return ReadResponseContent(tskresponse, DataFormat, canceltoken);
		}
		public MessageResult LocalDomainChange(string token, DateTime val, CancellationToken canceltoken)
		{
			string url = string.IsNullOrEmpty(token) ? DVR_Controller : string.Format(DVR_Controller_SID_Time, HttpUtility.UrlEncode(token), val.Ticks);
			Task<HttpResponseMessage> tskresponse = client.GetAsync(url, canceltoken);

			return ReadResponseContent(tskresponse, DataFormat, canceltoken);
		}

		public MessageResult RequestDomain(ConvertMessage.MessageAgreement info, CancellationToken canceltoken)
		{
			ObjectContent content = new ObjectContent(info.GetType(),info,DataFormat);
			Task<HttpResponseMessage> tskresponse = client.PostAsync(DVR_Controller, content, canceltoken);
			return ReadResponseContent(tskresponse, DataFormat, canceltoken);
		}

		private MessageResult ReadResponseContent(Task<HttpResponseMessage> Tresponse, MediaTypeFormatter formatter, CancellationToken canceltoken)
		{
			if( Tresponse == null)
				return null;
			MessageResult ret = null;
			Task<MessageResult> tsk_ret = null;
			try
			{
					tsk_ret = Tresponse.ContinueWith<MessageResult>(res_task =>
					{
				
						if(canceltoken.IsCancellationRequested)
						{
							canceltoken.ThrowIfCancellationRequested();
							return new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL};
						}
						if (res_task.IsCompleted && !res_task.IsFaulted)
							return ReadResponseContent<MessageResult>(res_task.Result, formatter, canceltoken);

						else if (res_task.IsFaulted)
							return new MessageResult{ ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, httpStatus = res_task.Result.StatusCode };
						else
							return new MessageResult { ErrorID = Commons.ERROR_CODE.CONVERTER_CANCELREQUEST, httpStatus = res_task.Result.StatusCode };

					}, canceltoken, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

			
				tsk_ret.Wait(canceltoken);
				ret = tsk_ret.Result;
				tsk_ret.Dispose();
				tsk_ret = null;
			}
			catch(SocketException sk_exception)
			{
				ret = new MessageResult{ ErrorID= Commons.ERROR_CODE.SERVICE_BADGATEWAY, httpStatus = HttpStatusCode.BadGateway, Data = sk_exception.Message};
			}
			catch(OperationCanceledException)
			{
				ret = new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL };
			}
			catch(ObjectDisposedException)
			{
				ret = new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL };
			}
			catch (AggregateException) 
			{
				ret = new MessageResult { ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL};
			}
			catch(Exception ex)
			{
				if( canceltoken.IsCancellationRequested)
					ret = new MessageResult{ ErrorID = Commons.ERROR_CODE.HTTP_CLIENT_TASK_CANCEL, Data = ex.Message};
				else
				{
					if( tsk_ret != null && tsk_ret.Status == TaskStatus.Canceled)
						ret = new MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_BADGATEWAY, httpStatus = HttpStatusCode.BadGateway};
					else
						ret = new MessageResult{ ErrorID = Commons.ERROR_CODE.SERVICE_EXCEPTION, Data = ex.Message};
				}
			}
			
			return ret;
		}

		private T ReadResponseContent<T>(HttpResponseMessage response, MediaTypeFormatter formatter, CancellationToken canceltoken)
		{
			T ret = default(T);
			if( response == null || response.StatusCode >= HttpStatusCode.InternalServerError)
			{
				return ret;
			}
			try
			{
				Task _task = response.Content.ReadAsAsync<T>(new MediaTypeFormatter[] { formatter }).ContinueWith
					(
						task =>
						{
							if(canceltoken.IsCancellationRequested)
							{
								canceltoken.ThrowIfCancellationRequested();
								return;
							}
							if (task.IsCompleted)
							{
								ret = task.Result;
							}
						}
					, canceltoken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
				_task.Wait(canceltoken);
			}
			catch (AggregateException e)
			{
			}
			catch (Exception) { }

			return ret;
		}

	}
}
