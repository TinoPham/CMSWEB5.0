using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;
using CMSWebApi.Configurations;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Net.Mail;

namespace CMSWebApi.Email.SMTP
{
	internal class SMTP : EmailService
	{
		#region SMTP defines
		const string STR_UserName = "UserName";
		const string STR_Passworde = "Password";
		const string STR_STARTTLS = "STARTTLS";
		const string STR_AUTH = "AUTH";
		const string STR_AUTH_PLAIN = "AUTH PLAIN";
		const string STR_AUTH_LOGIN = "AUTH LOGIN";
		const string STR_AUTH_CRAM_MD5 = "AUTH CRAM-MD5";
		const string STR_RCPT_TO = "RCPT TO: ";
		const string STR_MAIL_FROM = "MAIL FROM: ";
		const string STR_DATA = "DATA";
		const string STR_FROM = "From: ";
		const string STR_TO = "To: ";
		const string STR_DATE = "Date: ";
		const string STR_REPLY_TO = "Reply-To: ";
		const string STR_Subject = "Subject: ";
		const string STR_DATE_FORMAT = "ddd, dd MM yyyy H:m:s -0000";
		const string STR_QUIT = "QUIT";
		const string STR_EHLO = "EHLO";
		const string BOUNDARY = "__MESSAGE__ID__54yg6f6h6y456345";
		#endregion
		public delegate void SMTPClientStatus(string message);
		public event SMTPClientStatus OnSmtpMailStatus;

		readonly string NewLine = Environment.NewLine;
		private Encoding _Encoding = Encoding.ASCII;
		const int Net_READ_BUFFER_LENGTH = 1024;
		const int Net_SEND_BUFFER_LENGTH = 8192;
		const int Socket_TimeOut = 30 * 1000;//30 second
		//readonly bool ssl = false;
		Stream _Stream = null;
		Socket _Socket = null;
		ManualResetEvent connectDone = new ManualResetEvent(false);

		internal SMTP(EmailSettingSection settings): base(settings){}

		public override async Task<SendMailResult> SendMessageAsync(System.Net.Mail.MailMessage Message, CancellationToken CancelToken)
		{
			
			SendMailResult Tresult = await SendMail(Message, CancelToken);
			return Tresult;
		}
		public override void Dispose()
		{
			base.Dispose();
			CloseSocket();
			CloseStream();

		}
		#region private methods
		private void CloseSocket()
		{
			if (_Socket != null)
			{
				_Socket.Shutdown(SocketShutdown.Both);
				_Socket.Close();
				_Socket = null;
			}
		}

		private void CloseStream()
		{
			if (_Stream != null)
			{
				_Stream.Close();
				_Stream.Dispose();
				_Stream = null;
			}
		}

		private async Task<SendMailResult> SendMail(MailMessage mailmsg, CancellationToken CancelToken)
		{
			_Encoding = Encoding.ASCII;
			SendMailResult result = new SendMailResult();
			if (!Connect(Server.Host, Server.Port))
			{
				//SmtpClientStatus(MSG_CANNOT_CONNECT_TO_MAILSERVER);
				//return new SendMailResult(false, MSG_CANNOT_CONNECT_TO_MAILSERVER);
				result.SetResult( SmtpStatusCode.GeneralFailure);
				goto EXIT;
			}

			SmtpCommandResult commandresult = null;
			commandresult = await GetResponse(_Stream, _Encoding, CancelToken);
			result.SetResult(commandresult, null);

			if (commandresult.SmtpStatusCode != SmtpStatusCode.ServiceReady)
			{
				goto EXIT;
			}

			//send helo command
			String EhloCommandString = this.GetCommandString(STR_EHLO, Environment.MachineName) + NewLine;

			commandresult = await Execute(EhloCommandString,CancelToken);
			result.SetResult(commandresult, EhloCommandString);
			//check  response message
			if (commandresult.SmtpStatusCode != SmtpStatusCode.Ok)
			{
				goto EXIT;
			}

			//check and Utilizer TLS command
			if ( Server.Tsl_Ssl == MailSercure.TSL || commandresult.Message.IndexOf(STR_STARTTLS, StringComparison.InvariantCultureIgnoreCase) > 0)
			{
				commandresult = await StartTls(Server.Host, STR_STARTTLS, CancelToken);
				result.SetResult(commandresult, STR_STARTTLS);
				if (commandresult.SmtpStatusCode != SmtpStatusCode.ServiceReady)
				{
					goto EXIT;
				}

				commandresult = await Execute(EhloCommandString, CancelToken);
				result.SetResult(commandresult, EhloCommandString);
				if (commandresult.SmtpStatusCode != SmtpStatusCode.Ok)
				{
					goto EXIT;
				}

			}
			//login 

			bool is_auth = await Authenticate(commandresult.Message, Account.UserID, Account.Pwd, result, CancelToken);
			if (!is_auth)
			{
				goto EXIT;
			}
			//send mail from
			if ( (await SendEmailAddress(STR_MAIL_FROM, mailmsg.From, result, CancelToken)).SmtpStatusCode != SmtpStatusCode.Ok)
			{
				goto EXIT;
			}
			//send receipts
			is_auth = await SendEmailAddress(STR_RCPT_TO, mailmsg.To, result, CancelToken);
			if (! is_auth)
			{
				goto EXIT;
			}
			if (!( await SendMailMessage(mailmsg, result, CancelToken)))
				goto EXIT;

			commandresult = await Execute(STR_QUIT + NewLine, CancelToken);
			result.SetResult(commandresult, STR_QUIT + NewLine);
			bool ret = (commandresult.SmtpStatusCode == SmtpStatusCode.Ok || commandresult.SmtpStatusCode == SmtpStatusCode.ServiceClosingTransmissionChannel) ? true : false;
			result.SetResult(ret ? SmtpStatusCode.Ok : commandresult.SmtpStatusCode);
			goto EXIT;

			EXIT:
				Dispose();
				return result;
		}

		private StringBuilder GetHeaderFromMailMessage(MailMessage message)
		{
			StringBuilder header = new StringBuilder();
			header.Append(STR_DATE + DateTime.UtcNow.ToString(STR_DATE_FORMAT) + NewLine);
			header.Append(STR_FROM + MailFormat(message.From) + NewLine);
			header.Append(STR_REPLY_TO + message.From.Address + NewLine);

			string strto = STR_TO;
			foreach (MailAddress _add in message.To)
			{
				strto += MailFormat(_add) + ",";
			}
			if (strto.EndsWith(","))
				strto = strto.Substring(0, strto.Length - 1);

			header.Append(strto + NewLine);
			header.Append(STR_Subject + message.Subject + NewLine);

			if (message.Attachments != null && message.Attachments.Count > 0)
			{
				header.Append("MIME-Version: 1.0" + NewLine);
				header.Append(string.Format("Content-Type: multipart/mixed; boundary={0}", BOUNDARY));
				header.Append(NewLine);
				header.Append(NewLine);
			}
			else
			{
				if (!message.IsBodyHtml)
					header.Append("Content-type: text/plain; charset=\"" + (message.SubjectEncoding == null ? message.BodyEncoding.WebName : message.SubjectEncoding.WebName) + "\"");
				else
					header.Append("Content-type: text/html; charset=\"" + (message.SubjectEncoding == null ? message.BodyEncoding.WebName : message.SubjectEncoding.WebName) + "\"");
				header.Append(NewLine);
				header.Append("Content-Transfer-Encoding: 7Bit" + NewLine);

			}
			return header;
		}

		private StringBuilder GetBodyFromMailMessage(MailMessage message)
		{
			StringBuilder body = new StringBuilder();
			if (message.Attachments == null || message.Attachments.Count == 0)
			{
				body.Append(message.Body + NewLine);
				return body;
			}
			body.Append(string.Format("--{0}", BOUNDARY));// --__MESSAGE__ID__54yg6f6h6y456345" + newline;
			body.Append(NewLine);
			if (!message.IsBodyHtml)
				body.Append("Content-type: text/plain; charset=" + (message.SubjectEncoding == null ? message.BodyEncoding.WebName : message.SubjectEncoding.WebName));
			else
				body.Append("Content-type: text/html; charset=" + (message.SubjectEncoding == null ? message.BodyEncoding.WebName : message.SubjectEncoding.WebName));
			body.Append(NewLine);

			body.Append("Content-Transfer-Encoding: 7Bit" + NewLine);
			body.Append(NewLine);
			body.Append(message.Body + NewLine);
			body.Append(NewLine);
			AddAttachMents(body, message.Attachments);

			return body;
		}

		private async Task<Boolean> SendMailMessage(MailMessage message, SendMailResult result, CancellationToken CancelToken)
		{
			//Boolean ret = true;

			StringBuilder header = this.GetHeaderFromMailMessage(message);
			StringBuilder body = this.GetBodyFromMailMessage(message);
			string cmd = STR_DATA + NewLine;
			SmtpCommandResult cmdret = await Execute(cmd, CancelToken);
			result.SetResult(cmdret, cmd);

			if (cmdret.SmtpStatusCode != SmtpStatusCode.StartMailInput && cmdret.SmtpStatusCode != SmtpStatusCode.Ok)//Van add to test
				return false;
			//send header
			string data = header.ToString() + NewLine;
			Boolean retsend = await SendbyStream(_Stream, data, this._Encoding, CancelToken);

			//send body + attachment
			if (message.Attachments != null && message.Attachments.Count > 0)
			{
				body.Append(string.Format("{0}--{1}--{0}", NewLine, BOUNDARY));

			}
			body.Append("." + NewLine);
			data = body.ToString();

			//retsend = SendbyStream(_Stream, data , this._Encoding);
			cmdret = await Execute(data, CancelToken);
			result.SetResult(cmdret, "." + NewLine);
			////send end message
			//if (message.Attachments != null && message.Attachments.Count > 0)
			//{
			//    data = string.Format("{0}--{1}--{0}", NewLine, BOUNDARY);
			//    retsend = SendbyStream(_Stream, data, this._Encoding);
			//}

			//return ret;
			return cmdret.SmtpStatusCode == SmtpStatusCode.Ok || cmdret.SmtpStatusCode == SmtpStatusCode.ServiceClosingTransmissionChannel;
		}

		private void AddAttachMents(StringBuilder data, AttachmentCollection attments)
		{
			if (attments == null || attments.Count == 0)
				return;
			if (data == null)
				data = new StringBuilder();

			foreach (Attachment att in attments)
			{
				AddAttachMent(data, att);
			}

		}

		private void AddAttachMent(StringBuilder data, Attachment attment)
		{
			if (attment == null || attment.ContentStream == null || attment.ContentStream.Length == 0)
				return;
			if (data == null)
				data = new StringBuilder();
			byte [] binaryData = new byte [attment.ContentStream.Length];
			attment.ContentStream.Seek(0, SeekOrigin.Begin);
			attment.ContentStream.Read(binaryData, 0, binaryData.Length);

			data.Append(string.Format("--{0}", BOUNDARY));
			data.Append(NewLine);

			string str_Content_Type = MineTypesMapping.MinetypeHeader(new FileInfo((attment.ContentStream as FileStream).Name).Extension);
			data.Append(String.Format("Content-Type: {0}; file={1}{2}", str_Content_Type, attment.Name, NewLine));
			data.Append("Content-Transfer-Encoding: base64" + Environment.NewLine);
			data.Append("Content-Disposition: attachment; filename=\"" + attment.Name + "\"" + NewLine);
			data.Append(NewLine);

			string base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length, Base64FormattingOptions.InsertLineBreaks);
			//for (int i = 0; i < base64String.Length; )
			//{
			//    int nextchunk = 100;
			//    if (base64String.Length - (i + nextchunk) < 0)
			//        nextchunk = base64String.Length - i;
			//    data.Append(base64String.Substring(i, nextchunk));
			//    data.Append(NewLine);
			//    i += nextchunk;
			//}

			//data.Append("==");
			data.Append(base64String);
			if (!base64String.EndsWith(NewLine))
				data.Append(NewLine);

		}

		private async Task<Boolean> SendEmailAddress(string header, MailAddressCollection lstaddress, SendMailResult result, CancellationToken CancelToken)
		{
			Boolean ret = false;
			foreach (MailAddress _add in lstaddress)
			{
				await SendEmailAddress(header, _add, result, CancelToken);
				ret = true;
			}
			return ret;
		}

		private async Task<SmtpCommandResult> SendEmailAddress(string header, MailAddress address, SendMailResult result, CancellationToken CancelToken)
		{
			return await SendEmailAddress(header, /*address.DisplayName,*/ address.Address, result, CancelToken);//only using email address in command
		}

		private async Task<SmtpCommandResult> SendEmailAddress(string header, /*string name,*/ string emailaddress, SendMailResult result, CancellationToken CancelToken)
		{
			//string data = header + MailFormat(name, emailaddress) + NewLine;            
			string data = header + string.Format("<{0}>", emailaddress) + NewLine;//only using email address in command
			SmtpCommandResult rs = await Execute(data, CancelToken);
			result.SetResult(rs, data);
			return rs;
		}

		private string MailFormat(MailAddress address)
		{
			return MailFormat(address.DisplayName, address.Address);
		}

		private string MailFormat(string name, string address)
		{
			return string.Format("{0}<{1}>", name, address);
		}

		private async Task<Boolean> Authenticate(string responselogintype, string userid, string pwd, SendMailResult result, CancellationToken CancelToken)
		{
			SmtpCommandResult rs = null;
			if (!responselogintype.Contains(STR_AUTH) || (string.IsNullOrEmpty(userid) && string.IsNullOrEmpty(pwd)))
				return true;
			if (responselogintype.IndexOf(STR_AUTH_LOGIN, StringComparison.InvariantCultureIgnoreCase) > 0)
				rs = await AuthenticateByLogin(userid, pwd, result, CancelToken);

			else if (responselogintype.IndexOf(STR_AUTH_PLAIN, StringComparison.InvariantCultureIgnoreCase) > 0)
				rs = await AuthenticateByPlain(userid, pwd, result, CancelToken);

			if (rs != null)
				SmtpClientStatus(rs.Message);

			return rs == null ? false : rs.SmtpStatusCode == SmtpStatusCode.AuthenticationSuccessful;
		}

		private async Task<SmtpCommandResult> AuthenticateByLogin(string userid, string pwd, SendMailResult result, CancellationToken CancelToken)
		{
			SmtpCommandResult rs = null;
			string response = string.Empty;
			string base64 = string.Empty;
			bool isUserName = true;
			string command= string.Empty;
			command = STR_AUTH_LOGIN + NewLine;
			rs =  await Execute(STR_AUTH_LOGIN + NewLine, CancelToken);
			result.SetResult(rs, command);

			if( rs.SmtpStatusCode != SmtpStatusCode.TextPartContent)
			{
				base64 	= Convert.ToBase64String(_Encoding.GetBytes(userid));
				command = base64 + NewLine;

			}
			else
				{
					response = Commons.Utils.Base64toString(rs.Message.Replace(NewLine, ""));
					isUserName = response.StartsWith( STR_UserName, StringComparison.InvariantCultureIgnoreCase);
					base64 = isUserName ? Convert.ToBase64String(_Encoding.GetBytes(userid)) : Convert.ToBase64String(_Encoding.GetBytes(pwd));
					command = base64 + NewLine;
					
				}

			rs = await this.Execute(command, CancelToken);
			result.SetResult(rs, string.IsNullOrEmpty(response)?command : response + command);
			response = null;
			if (rs.SmtpStatusCode != SmtpStatusCode.TextPartContent)
			{
				base64 = Convert.ToBase64String(_Encoding.GetBytes(pwd));
				command = base64 + NewLine;
			}
			else
			{
				response = Commons.Utils.Base64toString(rs.Message.Replace(NewLine, ""));
				isUserName = response.StartsWith(STR_UserName, StringComparison.InvariantCultureIgnoreCase);
				base64 = isUserName ? Convert.ToBase64String(_Encoding.GetBytes(userid)) : Convert.ToBase64String(_Encoding.GetBytes(pwd));
				command = base64 + NewLine;

			}

			rs = await this.Execute(command, CancelToken);
			result.SetResult(rs, string.IsNullOrEmpty(response) ? command : response + command);

			return rs;
		}

		private async Task<SmtpCommandResult> AuthenticateByPlain(string userid, string pwd, SendMailResult result, CancellationToken CancelToken)
		{
			SmtpCommandResult rs = await Execute(STR_AUTH_PLAIN + NewLine, CancelToken);
			result.SetResult(rs, STR_AUTH_PLAIN + NewLine);
			String data = string.Format("{0}{1}{2}{1}", userid, NewLine, pwd);
			data = Convert.ToBase64String(_Encoding.GetBytes(data));
			rs = await Execute(data, CancelToken);
			result.SetResult(rs, data);

			return rs;
		}

		private async Task<SmtpCommandResult> StartTls(string host, string tsl, CancellationToken CancelToken)
		{

			//SmtpCommandResult result = await this.Execute(STR_STARTTLS + NewLine);
			SmtpCommandResult result = await this.Execute(tsl.EndsWith(NewLine)? tsl : (tsl + NewLine), CancelToken );
			if (result.SmtpStatusCode != SmtpStatusCode.ServiceReady)
				return new SmtpCommandResult(null);
			_Stream = OpenSSL(_Socket, host);
			if (_Stream == null)
				return new SmtpCommandResult(null);

			return result;
		}

		private void SmtpClientStatus(string mesage)
		{
			if (OnSmtpMailStatus != null)
				OnSmtpMailStatus(mesage);
		}
		#region Socket IO
		

		private String GetCommandString(string commandName, string param)
		{
			return String.Format("{0} {1}", commandName, param);
		}
		private void Connect(EndPoint remoteEP, Socket client)
		{
			client.BeginConnect(remoteEP,
				new AsyncCallback(ConnectCallback), client);

			connectDone.WaitOne();
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = (Socket)ar.AsyncState;

				// Complete the connection.
				client.EndConnect(ar);
				// Signal that the connection has been made.
				connectDone.Set();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		private Socket TryGetSocket(IPAddress address, int port)
		{
			IPEndPoint ipe = new IPEndPoint(address, port);
			Socket tc = null;

			try
			{
				tc = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				Connect(ipe, tc);
				connectDone.WaitOne();
				if (tc.Connected == true)
				{
					tc.ReceiveTimeout = Socket_TimeOut;
					tc.SendBufferSize = Socket_TimeOut;
					tc.SendBufferSize = Net_SEND_BUFFER_LENGTH;
					tc.ReceiveBufferSize = Net_READ_BUFFER_LENGTH;
				}
			}
			catch
			{
				tc = null;
			}
			return tc;
		}

		private IPHostEntry GetHostEntry(string hostname)
		{
			try
			{
				return Dns.GetHostEntry(hostname);
			}
			catch { }
			return null;
		}

		private Socket GetSocket(string host, int port)
		{
			Socket tc = null;
			IPHostEntry hostEntry = null;

			hostEntry = this.GetHostEntry(host);
			if (hostEntry != null)
			{
				foreach (IPAddress address in hostEntry.AddressList)
				{
					tc = this.TryGetSocket(address, port);
					if (tc != null) { break; }
				}
			}
			return tc;
		}

		private Boolean Connect(string host, int port)
		{
			if (_Socket == null || !_Socket.Connected)
				_Socket = this.GetSocket(host, port);
			if (_Socket == null)
			{
				//SmtpClientStatus(MSG_CANNOT_INIT_SOCKET + host);
				return false;
			}
			else
			{
				if (Server.Tsl_Ssl == MailSercure.SSL)
				{
					_Stream = OpenSSL(_Socket, host);
				}
				else
				{
					_Stream = new NetworkStream(_Socket, true);
				}
			}

			return _Stream != null;

		}

		private SslStream OpenSSL(Socket socket, string hostname)
		{
			try
			{
				RemoteCertificateValidationCallback certValidationCallback = null;

				certValidationCallback = new RemoteCertificateValidationCallback(IgnoreCertificateErrorsCallback);
				SslStream ssl = new SslStream(new NetworkStream(_Socket), true, certValidationCallback);
				System.Security.Cryptography.X509Certificates.X509Certificate2Collection xc = new System.Security.Cryptography.X509Certificates.X509Certificate2Collection();
				ssl.AuthenticateAsClient(hostname, xc, System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Ssl2 | System.Security.Authentication.SslProtocols.Tls, false);
				//ssl.AuthenticateAsClient(hostname);
				return ssl.IsAuthenticated ? ssl : null;
			}
			catch (IOException)
			{
				return null;
			}
		}

		private async Task<SmtpCommandResult> Execute(String command, CancellationToken CancelToken)
		{
			bool send = await SendbyStream(_Stream, command, _Encoding,CancelToken);
			if (!send)
				return new SmtpCommandResult(null);

			return await GetResponse(_Stream, _Encoding, CancelToken);
		}

		private async Task<SmtpCommandResult> GetResponse(Stream stream, Encoding endcoding, CancellationToken CancelToken)
		{

			if (stream == null || !stream.CanRead)
			{
				SmtpCommandResultLine responseline = new SmtpCommandResultLine("Data stream cannot be null");
				return new SmtpCommandResult(new SmtpCommandResultLine [] { responseline });
			}
			List<SmtpCommandResultLine> lines = new List<SmtpCommandResultLine>();
			MemoryStream msgmem = new MemoryStream();
			byte [] buff = new byte [Net_READ_BUFFER_LENGTH];
			int read_len = 0;
			try
			{
				//while (stream != null && (read_len = stream.Read(buff, 0, Net_READ_BUFFER_LENGTH)) > 0)
				read_len = await stream.ReadAsync(buff, 0, Net_READ_BUFFER_LENGTH, CancelToken);
				while (stream != null &&  read_len > 0)
				{
					msgmem.Write(buff, 0, read_len);
					if (read_len < Net_READ_BUFFER_LENGTH)
						break;
					read_len = await stream.ReadAsync(buff, 0, Net_READ_BUFFER_LENGTH, CancelToken);
				}
				msgmem.Seek(0, SeekOrigin.Begin);
				StringBuilder sb = new StringBuilder(endcoding.GetString(msgmem.ToArray()));
				string [] allmsg = Regex.Split(sb.ToString(), NewLine, RegexOptions.IgnoreCase);
				foreach (string msg in allmsg)
				{
					if (string.IsNullOrEmpty(msg))
						continue;
					lines.Add(new SmtpCommandResultLine(msg));
				}
			}
			catch (System.Exception ex)
			{

			}
			finally
			{
				msgmem.Close();
				msgmem.Dispose();
				msgmem = null;
				buff = null;
			}
			return new SmtpCommandResult(lines.ToArray());
		}

		private async Task<Boolean> SendbyStream(Stream stream, string data, Encoding endcoding, CancellationToken CancelToken)
		{
			if (stream == null)
				return false;

			byte [] message = null;
			message = endcoding.GetBytes(data);
			int total_send = 0;
			int cur_index = 0;
			int cur_len = 0;
			int msg_length = message.Length;
			try
			{
				while (cur_index < msg_length && !CancelToken.IsCancellationRequested)
				{
					cur_len = cur_index + Net_SEND_BUFFER_LENGTH < msg_length ? Net_SEND_BUFFER_LENGTH : msg_length - cur_index;
					//stream.Write(message, cur_index, cur_len);
					await stream.WriteAsync( message, cur_index,cur_len, CancelToken);
					cur_index += cur_len;
					total_send += cur_len;
				}

			}
			catch (System.Exception ex)
			{
				total_send = 0;
			}
			return total_send == msg_length;
		}
		#endregion

		#endregion
	}
}
