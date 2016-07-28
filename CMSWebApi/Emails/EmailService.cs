using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Configurations;
using System.Net.Mail;
using System.IO;
using CMSWebApi.Utils;
using System.Threading;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
namespace CMSWebApi.Email
{

	public abstract class EmailService : IDisposable
	{
		EmailSettingSection MailSettings{ get ;set;}
		protected MailServer Server {  get { return MailSettings == null ? null : MailSettings.Server;}}
		protected MailAccount Account{ get { return MailSettings == null? null : MailSettings.Account;}}
		protected MailAddressCollection To{ get; set;}
		protected List<Attachment> Attachments { get; set; }
		//protected CancellationToken CancelToken;

		internal EmailService(EmailSettingSection Settings)
		{
			MailSettings = Settings;
			To = new MailAddressCollection();
			Attachments = new List<Attachment>();
			//CancelToken = canceltoken;
		}

		private void UpdateAttachment(MailMessage message)
		{
			if (message == null)
				return;
			foreach (Attachment it in Attachments)
			{
				if(message.Attachments.Any(itmsg => string.Compare(it.Name, itmsg.Name, true) == 0) )
					continue;
				message.Attachments.Add( it);

			}
		}
		private void UpdateTo( MailMessage message)
		{
			if( message == null)
				return;
			foreach( MailAddress add in To )
			{
				if( message.To.Any( it => string.Compare(it.Address,add.Address, true) == 0))
					continue;
				message.To.Add(add);
			}
		}
		public static EmailService Create(EmailSettingSection setttings)
		{
			if (setttings == null || setttings.Server == null)
				return null;

			if (setttings.Account == null || !Commons.Utils.ValidateEmailAddress(setttings.Account.Address))
				return null;

			if (setttings.Server.Type == MailServerType.SMTP)
				return new SMTP.SMTP(setttings);
			return new MSExchange.MSExchange(setttings);
		}

		protected bool IgnoreCertificateErrorsCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			//if (sslPolicyErrors != SslPolicyErrors.None)
			//{
			//    return false;
			//}
			return true;
		}

		public void AddAttachment(Stream contentStream, string name, CMSWebApi.Utils.MineTypesMapping.MineTypes type )
		{
			if( string.IsNullOrEmpty(name) || contentStream == null|| contentStream.Length == 0)
				return;

			Attachment item = new Attachment( contentStream, name, MineTypesMapping.MinetypeHeader(type));
		}
		
		public void AddAttachment(string filepath)
		{
			if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
				return;
			FileInfo finfo = new FileInfo(filepath);
			string minitype = MineTypesMapping.MinetypeHeader(finfo.Extension);
			Attachment item = new Attachment(filepath, minitype);
			Attachments.Add(item);
		}

		public void AddReceipt( string address, string name)
		{
			if( string.IsNullOrEmpty(address))
				return;
			if( To.Any( it => string.Compare(it.Address, address, true) == 0))
				return;
			To.Add( string.IsNullOrEmpty(name)? new MailAddress(address) : new MailAddress(address, name) );
		}
		#region Async
		public virtual Task<SMTP.SendMailResult> SendMessageAsync( MailMessage Message, CancellationToken canceltoken)
		{
			return Task.FromResult<SMTP.SendMailResult>( new SMTP.SendMailResult());
		}

		public virtual Task<SMTP.SendMailResult> SendMessageAsync(string from, string to, string subject, string body, bool IsBodyHtml, List<string> attachements, CancellationToken canceltoken)
		{
			return SendMessageAsync(new MailAddress(from), new MailAddress(to), subject, body, IsBodyHtml, attachements, canceltoken);
		}

		public virtual Task<SMTP.SendMailResult> SendMessageAsync(MailAddress to, string subject, string body, bool IsBodyHtml, List<string> attachements, CancellationToken canceltoken)
		{
			MailAddress sender = string.IsNullOrEmpty(Account.Name) ? new MailAddress(Account.Address) : new MailAddress(Account.Address, Account.Name);
			return SendMessageAsync(sender, to, subject, body, IsBodyHtml, attachements, canceltoken);

		}

		public virtual Task<SMTP.SendMailResult> SendMessageAsync(string to, string subject, string body, bool IsBodyHtml, List<string> attachements, CancellationToken canceltoken)
		{
			return SendMessageAsync(new MailAddress(to), subject, body, IsBodyHtml, attachements, canceltoken);
		}

		public virtual Task<SMTP.SendMailResult> SendMessageAsync(string subject, string body, bool IsBodyHtml, CancellationToken canceltoken)
		{
			MailAddress sender = string.IsNullOrEmpty(Account.Name) ? new MailAddress(Account.Address) : new MailAddress(Account.Address, Account.Name);
			MailMessage msg = new MailMessage();
			msg.Sender = sender;
			msg.From = sender;
			foreach (MailAddress it in To)
				msg.To.Add(it);

			foreach (Attachment it in Attachments)
				msg.Attachments.Add(it);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = IsBodyHtml;
			return SendMessageAsync(msg, canceltoken);

		}

		public virtual Task<SMTP.SendMailResult> SendMessageAsync(MailAddress from, MailAddress to, string subject, string body, bool IsBodyHtml, List<string> attachements, CancellationToken canceltoken)
		{
			MailMessage msg = new MailMessage();
			msg.From = from;
			msg.To.Add(to);
			UpdateTo(msg);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = IsBodyHtml;
			if (attachements != null)
			{
				foreach (string it in attachements)
				{
					if (!File.Exists(it))
						continue;
					msg.Attachments.Add(new Attachment(it, MineTypesMapping.MinetypeHeader(new FileInfo(it).Extension)));
				}
			}
			UpdateAttachment(msg);
			return SendMessageAsync(msg, canceltoken);
		}
		#endregion

		#region Sync
		public virtual async Task<SMTP.SendMailResult> SendMessage(MailMessage Message)
		{
			CancellationTokenSource tks = new CancellationTokenSource();
			CancellationToken canceltoken = tks.Token;
			SMTP.SendMailResult Tresult = await SendMessageAsync(Message, canceltoken);
			return Tresult;
		}

		public virtual async Task<SMTP.SendMailResult> SendMessage(MailAddress from, MailAddress to, string subject, string body, bool IsBodyHtml, List<string> attachements)
		{
			MailMessage msg = new MailMessage();
			msg.From= from;
			msg.To.Add(to);
			UpdateTo(msg);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = IsBodyHtml;
			
			if(attachements != null)
			{
				foreach( string it in attachements)
				{
					if(!File.Exists( it))
						continue;
					msg.Attachments.Add( new Attachment(it, MineTypesMapping.MinetypeHeader( new FileInfo(it).Extension) ) );
				}
			}
			UpdateAttachment(msg);
			return await SendMessage(msg);
		}

		public virtual async Task<SMTP.SendMailResult> SendMessage(string from, string to, string subject, string body, bool IsBodyHtml, List<string> attachements)
		{
			return await SendMessage( new MailAddress(from), new MailAddress(to), subject, body, IsBodyHtml, attachements);
		}

		public virtual async Task<SMTP.SendMailResult> SendMessage(MailAddress to, string subject, string body, bool IsBodyHtml, List<string> attachements)
		{
			MailAddress sender = string.IsNullOrEmpty( Account.Name)? new MailAddress( Account.Address) : new MailAddress( Account.Address, Account.Name);
			return await SendMessage( sender, to,subject, body, IsBodyHtml, attachements);
			 
		}

		public virtual async Task<SMTP.SendMailResult> SendMessage(string to, string subject, string body, bool IsBodyHtml, List<string> attachements)
		{
			return await SendMessage( new MailAddress(to) , subject, body, IsBodyHtml, attachements);
		}

		public virtual async Task<SMTP.SendMailResult> SendMessage(string subject, string body, bool IsBodyHtml)
		{
			MailAddress sender = string.IsNullOrEmpty(Account.Name) ? new MailAddress(Account.Address) : new MailAddress(Account.Address, Account.Name);
			MailMessage msg = new MailMessage();
			msg.Sender = sender;

			foreach(MailAddress it in To)
				msg.To.Add(it);

			foreach(Attachment it in Attachments)
				msg.Attachments.Add(it);
			msg.Subject = subject;
			msg.Body = body;
			msg.IsBodyHtml = IsBodyHtml;
			return await SendMessage(msg);

		}
		#endregion
		public virtual void Dispose()
		{
			Attachments.Clear();
		}
	}
}
