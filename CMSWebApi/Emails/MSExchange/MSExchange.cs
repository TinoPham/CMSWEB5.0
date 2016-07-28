using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMSWebApi.Configurations;
using Microsoft.Exchange.WebServices.Data;
using System.Net.Mail;
using System.Net;
using System.Net.Security;

namespace CMSWebApi.Email.MSExchange
{
	internal class MSExchange : EmailService
	{
		private const string MSExchangeServicePath = @"ews/exchange.asmx";
		private const string Full_exchange_regex = @"^(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z0-9]{2,3}([/a-zA-Z0-9\.])+$";
		internal MSExchange(EmailSettingSection settings)
			: base(settings)
		{
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(IgnoreCertificateErrorsCallback);
		}

		public override async Task<SMTP.SendMailResult> SendMessageAsync(MailMessage message, CancellationToken canceltoken)
		{
			SMTP.SendMailResult result = await SendMessage(message);
			return result; //System.Threading.Tasks.Task.Run<SMTP.SendMailResult>(() => SendMessage(message), canceltoken);
		}
		public override async Task<SMTP.SendMailResult> SendMessage(MailMessage message)
		{
			ExchangeService service = InitService(Server, Account);
			SMTP.SendMailResult result = SendMail(service, message);
			return await System.Threading.Tasks.Task.FromResult<SMTP.SendMailResult>(result);
		}

		private SMTP.SendMailResult SendMail(ExchangeService service, MailMessage message)
		{
			SMTP.SendMailResult result = new SMTP.SendMailResult();
			if (service == null)
			{
				result.SetResult(SMTP.SmtpStatusCode.GeneralFailure);
				return result;

			}

			EmailMessage ex_message = ToExchangeMessage(service, message);
			if (ex_message == null)
			{
				result.SetResult(SMTP.SmtpStatusCode.GeneralFailure);
				return result;
			}
			try
			{
				ex_message.Send();
				result.SetResult(SMTP.SmtpStatusCode.Ok);
			}
			catch (Exception ex)
			{
				result.SetResult(SMTP.SmtpStatusCode.TransactionFailed, ex.Message);
			}
			return result;
		}
		private ExchangeService InitService(MailServer server, MailAccount account)
		{
			ExchangeService service = new ExchangeService(server.ExchangeExtend.Version);
			try
			{
				if (server.ExchangeExtend.AutodiscoverUrl && string.IsNullOrEmpty(server.Host))
					service.AutodiscoverUrl(account.Address, RedirectionCallback);
				else
				{
					string url = ExchangeUrl(server);
					service.Url = new Uri(url);
				}
				service.Credentials = new NetworkCredential(account.UserID, account.Pwd);
			}
			catch (Exception)
			{
				service = null;
			}
			return service;
		}

		private string ExchangeUrl(MailServer server)
		{
			string uripath = string.Empty;
			if (!Commons.Utils.ValidationString(Full_exchange_regex, server.Host))
			{
				bool isheader = server.Host.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter) || server.Host.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter);
				uripath = isheader ? server.Host : ((server.Tsl_Ssl == MailSercure.SSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + server.Host);
				uripath += uripath.EndsWith("/") ? MSExchangeServicePath : "/" + MSExchangeServicePath;
			}
			else
				uripath = server.Host;

			return uripath;
		}

		private EmailMessage ToExchangeMessage(ExchangeService exchangeService, MailMessage message)
		{
			EmailMessage ex_msg = new EmailMessage(exchangeService);
			ex_msg.Body = new MessageBody(message.IsBodyHtml ? BodyType.HTML : BodyType.Text, message.Body);
			ex_msg.Subject = message.Subject;
			ex_msg.Sender = ToEmailAddress(message.From);
			foreach (MailAddress add in message.To)
				ex_msg.ToRecipients.Add(ToEmailAddress(add));
			foreach (MailAddress add in message.CC)
				ex_msg.CcRecipients.Add(ToEmailAddress(add));
			foreach (System.Net.Mail.Attachment att in message.Attachments)
				ex_msg.Attachments.AddFileAttachment(att.Name, att.ContentStream);
			return ex_msg;
		}

		bool RedirectionCallback(string url)
		{
			Uri uri = new Uri(url);
			return uri.Scheme == Uri.UriSchemeHttps;
		}

		private EmailAddress ToEmailAddress(MailAddress add)
		{
			return string.IsNullOrEmpty(add.DisplayName) ? new EmailAddress(add.Address) : new EmailAddress(add.DisplayName, add.Address);
		}
	}
}
