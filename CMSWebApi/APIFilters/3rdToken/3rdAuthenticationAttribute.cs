using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using CMSWebApi.DataModels;
using Extensions;
using System.Text.RegularExpressions;

namespace CMSWebApi.APIFilters._3rdToken
{
	public class _3rdAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		//private readonly UInt64 requestMaxAgeInSeconds = 300;  //5 mins
		const string authenticationScheme = "3rd-auth";

		public bool AllowMultiple
		{
			get { return false; }
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			context.Result = new ResultWithChallenge(context.Result);
			return Task.FromResult(0);
		}

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var req = context.Request;

			if (req.Headers.Authorization != null && authenticationScheme.Equals(req.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				var rawAuthzHeader = req.Headers.Authorization.Parameter;

				var autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthzHeader);

				if (autherizationHeaderArray != null)
				{
					var APPId = autherizationHeaderArray [0];
					var incomingBase64Signature = autherizationHeaderArray [1];
					var nonce = autherizationHeaderArray [2];
					var requestTimeStamp = autherizationHeaderArray [3];
					AppSettings._3rdConfig app_config = AppSettings._3rdAppSettings.Instance.GetConfig(APPId);
					if (isValidSettingRequest(req, APPId, app_config) )
					{
						var token = WebUserToken.RawToken( autherizationHeaderArray[4]);
						_3rdLoginModel auth_obj = new _3rdLoginModel();
						auth_obj.Parser(token);
						if (isValidToken(auth_obj) == false)
						{
							context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue [0], context.Request);
						}
						else
						{
							var isValid = isValidCheckSumRequest(req, APPId, auth_obj.SID, incomingBase64Signature, nonce, requestTimeStamp, app_config);

							if (isValid.Result)
							{
								var currentPrincipal = new GenericPrincipal(new _3rdUserContext(auth_obj) , null);
								context.Principal = currentPrincipal;
							}
							else
							{
								context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue [0], context.Request);
							}
						}
					}
					else
					context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue [0], context.Request);
				}
				else
				{
					context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue [0], context.Request);
				}
			}
			else
			{
				context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue [0], context.Request);
			}

			return Task.FromResult(0);
		}

		private bool isValidToken(_3rdLoginModel model)
		{
			if( model == null)
				return false;
			if(string.Compare(model.ServerID, AppSettings.AppSettings.Instance.ServerID, false) !=  0)
				return false;
			return model.ID > 0;
		}

		private bool isValidSettingRequest(HttpRequestMessage request, string appID, AppSettings._3rdConfig config)
		{
			//AppSettings._3rdConfig config = AppSettings._3rdAppSettings.Instance.GetConfig(appID);
			if (config == null)
				return false;
			return new IPAddressValidate().isValidRequest(request, config);


		}
		private async Task<bool> isValidCheckSumRequest(HttpRequestMessage req, string APPId,string apikey, string incomingBase64Signature, string nonce, string requestTimeStamp, AppSettings._3rdConfig appconfig)
		{
			string requestContentBase64String = "";
			string requestUri = HttpUtility.UrlEncode(req.RequestUri.AbsoluteUri.ToLower());
			string requestHttpMethod = req.Method.Method;


			var sharedKey = apikey;

			if (isReplayRequest(nonce, requestTimeStamp, appconfig))
			{
				return false;
			}

			byte [] hash = await ComputeHash(req.Content);

			if (hash != null)
			{
				requestContentBase64String = Convert.ToBase64String(hash);
			}

			string data = String.Format("{0}{1}{2}{3}{4}{5}", APPId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

			var secretKeyBytes = Convert.FromBase64String(sharedKey);

			byte [] signature = Encoding.UTF8.GetBytes(data);

			using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
			{
				byte [] signatureBytes = hmac.ComputeHash(signature);

				return (incomingBase64Signature.Equals(Convert.ToBase64String(signatureBytes), StringComparison.Ordinal));
			}

		}
		
		private bool isReplayRequest(string nonce, string requestTimeStamp,  AppSettings._3rdConfig config)
		{
			if (MemoryCache.Default.Contains(nonce) || config == null)
			{
				return true;
			}
			if( config.RequestAge <= 0)
				return false;

			DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan currentTs = DateTime.UtcNow - epochStart;

			var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
			var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

			if ((serverTotalSeconds - requestTotalSeconds) > (ulong)config.RequestAge)
			{
				return true;
			}

			MemoryCache.Default.Add(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(config.RequestAge));

			return false;
		}

		private static async Task<byte []> ComputeHash(HttpContent httpContent)
		{
			using (MD5 md5 = MD5.Create())
			{
				byte [] hash = null;
				var content = await httpContent.ReadAsByteArrayAsync();
				if (content.Length != 0)
				{
					hash = md5.ComputeHash(content);
				}
				return hash;
			}
		}
		
		private string [] GetAutherizationHeaderValues(string rawAuthzHeader)
		{

			var credArray = rawAuthzHeader.Split(':');

			if (credArray.Length == 5)
			{
				return credArray;
			}
			else
			{
				return null;
			}

		}


		private class ResultWithChallenge : IHttpActionResult
		{
			private readonly string authenticationScheme = "3rd-auth";
			private readonly IHttpActionResult next;

			public ResultWithChallenge(IHttpActionResult next)
			{
				this.next = next;
			}

			public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = await next.ExecuteAsync(cancellationToken);

				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authenticationScheme));
				}

				return response;
			}
		}

	}

	internal interface IValidate_App
	{
		bool isValidRequest(HttpRequestMessage request, AppSettings._3rdConfig config);
	}

	internal class IPAddressValidate : IValidate_App
	{
		const string str_IP = "IP:";
		public bool isValidRequest(HttpRequestMessage request, AppSettings._3rdConfig config)
		{
			if( config == null)
				return false;
			bool islocal = request.IsLocal();
			string remote_ip = request.GetClientIpAddress();
			var allow_Ip = config.Allow == null? Enumerable.Empty<string>() : config.Allow.Where(it => it.StartsWith(str_IP, StringComparison.InvariantCultureIgnoreCase));
			var Denied_Ip = config.Denied == null ? Enumerable.Empty<string>() : config.Denied.Where(it => it.StartsWith(str_IP, StringComparison.InvariantCultureIgnoreCase));
			bool valid = Denied_Ip.Any() == false ? true : CheckIPAddress(Denied_Ip, remote_ip, islocal) != null;
			if( valid && Denied_Ip.Any())
				return false;// return false when remote match with Denied seting
			valid = allow_Ip.Any() == false? false : CheckIPAddress(allow_Ip, remote_ip, islocal) != null;
			return valid;
		}
		private string CheckIPAddress(IEnumerable<string>  define, string ip, bool islocal)
		{
			string value;
			foreach( string setting in define)
			{
				value = setting.Remove(0, str_IP.Length).Trim();
				if( islocal &&	( string.Compare("localhost", value, true) == 0 || string.Compare(value, "127.0.0.1") == 0) )
					return setting;
				
				if( IsMatch(value, ip))
					return setting;

			}
			return null;
		}
		private bool IsMatch( string value, string ip)
		{
			if( string.Compare(value, ip, true) == 0)
				return true;
			string rgex = value.Replace(".", @"\.").Replace("*", ".+");
			Regex rx = new Regex("^" + rgex + "$");
			return rx.IsMatch( ip);
		}
	}
}
