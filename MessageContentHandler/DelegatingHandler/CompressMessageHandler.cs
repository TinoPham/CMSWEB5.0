﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Commons;
using MessageContentHandler.Content;

namespace MessageContentHandler.DelegatingHandler
{
	public class CompressMessageHandler : System.Net.Http.DelegatingHandler
	{
		public CompressMessageHandler(HttpConfiguration httpConfiguration)
		{
			InnerHandler = new HttpControllerDispatcher(httpConfiguration);
			//object logobj = httpConfiguration.DependencyResolver.GetService(typeof(SVRDatabase.SVRManager));
			//if( logobj != null)
			//	LogModel = logobj as SVRDatabase.SVRManager;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Task<HttpResponseMessage> tresponse;

			tresponse = base.SendAsync(request, cancellationToken);

			StringWithQualityHeaderValue compresstype = GetCompressType(request.Headers.AcceptEncoding);
			if (compresstype != null)
			{
				return tresponse.ContinueWith<HttpResponseMessage>((
				  responseToCompleteTask) =>
				{
					HttpResponseMessage response = responseToCompleteTask.Result;
					if (response.Content != null)
						response.Content = new CompressedContent(response.Content, compresstype.Value);

					return response;
				},
				TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			else
			{
				return tresponse.ContinueWith<HttpResponseMessage>((
				  responseToCompleteTask) =>
				{
					HttpResponseMessage response = responseToCompleteTask.Result;
					return response;
				},
				TaskContinuationOptions.OnlyOnRanToCompletion);

			}
		}

		private StringWithQualityHeaderValue GetCompressType(HttpHeaderValueCollection<StringWithQualityHeaderValue> Encoding)
		{
			if (Encoding == null || Encoding.Count == 0)
				return null;
			StringWithQualityHeaderValue compress = Encoding.FirstOrDefault(item => string.Compare(item.Value, HttpConstant.STR_gzip, true) == 0);
			if (compress == null)
				compress = Encoding.FirstOrDefault(item => string.Compare(item.Value, HttpConstant.STR_deflate, true) == 0);
			return compress;

		}
	}
}
