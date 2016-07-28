using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons;

namespace CMSWebApi.Email.SMTP
{
	public class SendMailResult
	{
		SmtpStatusCode _SmtpStatusCode = SmtpStatusCode.GeneralFailure;
		public SmtpStatusCode SmtpStatusCode { get { return _SmtpStatusCode; } }
		public string Message{ get; private set;}
		readonly List<SmtpCommandDataResult> _Result = new List<SmtpCommandDataResult>();
		public IEnumerable<SmtpCommandDataResult> Results { get { return _Result; } }
		internal void SetResult(SmtpStatusCode error)
		{
			_SmtpStatusCode = error;
			Message = SmtpStatusMessage.Instance.ErrString(_SmtpStatusCode);
			_Result.Add(new SmtpCommandDataResult { Message = null, Result = null, SmtpStatusCode = error });
		}
		internal void SetResult(SmtpStatusCode error, string msg)
		{
			_SmtpStatusCode = error;
			Message = msg;
			_Result.Add(new SmtpCommandDataResult { Message = null, Result = msg, SmtpStatusCode = error});
		}

		internal void SetResult(SmtpCommandResult cmdresult, string command )
		{
			_SmtpStatusCode = cmdresult.SmtpStatusCode;
			Message = SmtpStatusMessage.Instance.ErrString(_SmtpStatusCode);
			_Result.Add( new SmtpCommandDataResult{Message = command, Result = cmdresult.Message, SmtpStatusCode = cmdresult.SmtpStatusCode });
		}
	}

	public class SmtpCommandDataResult
	{
		public string  Message{ get ;set;}
		public string Result{ get ;set;}
		SmtpStatusCode _SmtpStatusCode = SmtpStatusCode.GeneralFailure;
		public SmtpStatusCode SmtpStatusCode{ get{ return _SmtpStatusCode;} set { _SmtpStatusCode = value;}}

	}

	class SmtpCommandResult
	{
		SmtpStatusCode _SmtpStatusCode = SmtpStatusCode.GeneralFailure;
		private string _Message = string.Empty;
		public SmtpStatusCode SmtpStatusCode { get { return _SmtpStatusCode; } }
		public string Message { get { return _Message; } }
		public string Command{ get ;set;}

		public SmtpCommandResult(SmtpCommandResultLine [] lines)
		{
			StringBuilder sb = new StringBuilder();
			if (lines == null || lines.Length == 0)
			{ return; }

			this._SmtpStatusCode = lines [0].StatusCode;
			for (int i = 0; i < lines.Length; i++)
			{
				sb.Append(lines [i].Message);
				sb.AppendLine();
			}
			this._Message = sb.ToString();
		}
	}

	class SmtpCommandResultLine
	{

		private Boolean _InvalidFormat = false;
		private Int32 _StatusCodeNumber = 0;
		private SmtpStatusCode _StatusCode = SmtpStatusCode.GeneralFailure;
		private Boolean _HasNextLine = false;
		private String _Message = "";
		/// <summary>
		/// 
		/// </summary>
		public Boolean InvalidFormat
		{
			get { return this._InvalidFormat; }
		}
		/// <summary>
		/// 
		/// </summary>
		public Int32 CodeNumber
		{
			get { return this._StatusCodeNumber; }
		}
		/// <summary>
		/// 
		/// </summary>
		public SmtpStatusCode StatusCode
		{
			get { return this._StatusCode; }
		}
		/// <summary>
		/// 
		/// </summary>
		public Boolean HasNextLine
		{
			get { return this._HasNextLine; }
		}
		/// <summary>
		/// 
		/// </summary>
		public String Message
		{
			get { return this._Message; }
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		public SmtpCommandResultLine(String line)
		{
			Match m = RegexList.SmtpResultLine.Match(line);
			this._InvalidFormat = !Int32.TryParse(m.Groups ["StatusCode"].Value, out this._StatusCodeNumber);
			this._StatusCode = (SmtpStatusCode)this._StatusCodeNumber;
			this._HasNextLine = m.Groups ["HasNextLine"].Value == "-";
			this._Message = m.Groups ["Message"].Value;
		}
	}

	class RegexList
	{
		public static readonly Regex SmtpResultLine = new Regex(@"(?<StatusCode>[0-9]{3})(?<HasNextLine>[\s-]{0,1})(?<Message>.*)", RegexOptions.IgnoreCase);
	}

	class SmtpStatusMessage : SingletonClassBase<SmtpStatusMessage>
	{
		Dictionary<int, string>SmtpstatusMap = new Dictionary<int,string>();
		SmtpStatusMessage()
		{
			SmtpstatusMap.Add((int)SmtpStatusCode.GeneralFailure, "SMTP host cannot be found.");
			SmtpstatusMap.Add((int)SmtpStatusCode.SystemStatus, "A system status or system Help reply.");
			SmtpstatusMap.Add((int)SmtpStatusCode.HelpMessage, "A Help message was returned by the service.");
			SmtpstatusMap.Add((int)SmtpStatusCode.ServiceReady, "The SMTP service is ready.");
			SmtpstatusMap.Add((int)SmtpStatusCode.ServiceClosingTransmissionChannel, "The SMTP service is closing the transmission channel.");
			SmtpstatusMap.Add((int)SmtpStatusCode.Ok, "The email was successfully sent to the SMTP service.");
			SmtpstatusMap.Add((int)SmtpStatusCode.UserNotLocalWillForward, "The user mailbox is not located on the receiving server; the server forwards the e-mail.");
			SmtpstatusMap.Add((int)SmtpStatusCode.CannotVerifyUserWillAttemptDelivery, "The specified user is not local, but the receiving SMTP service accepted the message and attempted to deliver it.");
			SmtpstatusMap.Add((int)SmtpStatusCode.TextPartContent, "Text part containing the [BASE64] encoded string.");
			SmtpstatusMap.Add((int)SmtpStatusCode.StartMailInput, "The SMTP service is ready to receive the e-mail content.");
			SmtpstatusMap.Add((int)SmtpStatusCode.ServiceNotAvailable, "The SMTP service is not available; the server is closing the transmission channel.");
			SmtpstatusMap.Add((int)SmtpStatusCode.MailboxBusy, "The destination mailbox is in use.");
			SmtpstatusMap.Add((int)SmtpStatusCode.LocalErrorInProcessing, "The SMTP service cannot complete the request. This error can occur if the client's IP address cannot be resolved (that is, a reverse lookup failed). You can also receive this error if the client domain has been identified as an open relay or source for unsolicited e-mail (spam).");
			SmtpstatusMap.Add((int)SmtpStatusCode.InsufficientStorage, "The SMTP service does not have sufficient storage to complete the request.");
			SmtpstatusMap.Add((int)SmtpStatusCode.ClientNotPermitted, "The client was not authenticated or is not allowed to send mail using the specified SMTP host.");
			SmtpstatusMap.Add((int)SmtpStatusCode.CommandUnrecognized, "The SMTP service does not recognize the specified command.");
			SmtpstatusMap.Add((int)SmtpStatusCode.SyntaxError, "The syntax used to specify a command or parameter is incorrect.");
			SmtpstatusMap.Add((int)SmtpStatusCode.CommandNotImplemented, "The SMTP service does not implement the specified command.");
			SmtpstatusMap.Add((int)SmtpStatusCode.BadCommandSequence, "The commands were sent in the incorrect sequence.");
			SmtpstatusMap.Add((int)SmtpStatusCode.CommandParameterNotImplemented, "The SMTP service does not implement the specified command parameter.");
			SmtpstatusMap.Add((int)SmtpStatusCode.AuthenticationWeak, "Authentication mechanism is to weak.");
			SmtpstatusMap.Add((int)SmtpStatusCode.Authenticationinvalid, "Authentication credentials invalid.");
			SmtpstatusMap.Add((int)SmtpStatusCode.MustIssueStartTlsFirst, "The SMTP server is configured to accept only TLS connections, and the SMTP client is attempting to connect by using a non-TLS connection. The solution is for the user to set EnableSsl=true on the SMTP Client.");
			SmtpstatusMap.Add((int)SmtpStatusCode.MailboxUnavailable, "The destination mailbox was not found or could not be accessed.");
			SmtpstatusMap.Add((int)SmtpStatusCode.UserNotLocalTryAlternatePath, "The user mailbox is not located on the receiving server.");
			SmtpstatusMap.Add((int)SmtpStatusCode.ExceededStorageAllocation, "The message is too large to be stored in the destination mailbox.");
			SmtpstatusMap.Add((int)SmtpStatusCode.MailboxNameNotAllowed, "The syntax used to specify the destination mailbox is incorrect.");
			SmtpstatusMap.Add((int)SmtpStatusCode.TransactionFailed, "The transaction failed.");
		}
		public string ErrString( SmtpStatusCode status)
		{
			if(SmtpstatusMap.ContainsKey( (int)status))
				return SmtpstatusMap[ (int)status];
			return null;
		}
	}
	
	public enum SmtpStatusCode : int
	{
		// Summary:
		//     The transaction could not occur. You receive this error when the specified
		//     SMTP host cannot be found.
		GeneralFailure = -1,
		//
		// Summary:
		//     A system status or system Help reply.
		SystemStatus = 211,
		//
		// Summary:
		//     A Help message was returned by the service.
		HelpMessage = 214,
		//
		// Summary:
		//     The SMTP service is ready.
		ServiceReady = 220,
		//
		// Summary:
		//     The SMTP service is closing the transmission channel.
		ServiceClosingTransmissionChannel = 221,
		//
		// Summary:
		//     AuthenticationSuccessful.
		AuthenticationSuccessful = 235,
		//
		// Summary:
		//     The email was successfully sent to the SMTP service.
		Ok = 250,
		//
		// Summary:
		//     The user mailbox is not located on the receiving server; the server forwards
		//     the e-mail.
		UserNotLocalWillForward = 251,
		//
		// Summary:
		//     The specified user is not local, but the receiving SMTP service accepted
		//     the message and attempted to deliver it. This status code is defined in RFC
		//     1123, which is available at http://www.ietf.org.
		CannotVerifyUserWillAttemptDelivery = 252,
		/// <summary>
		/// Text part containing the [BASE64] encoded string
		/// </summary>
		TextPartContent = 334,
		//
		// Summary:
		//     The SMTP service is ready to receive the e-mail content.
		StartMailInput = 354,
		//
		// Summary:
		//     The SMTP service is not available; the server is closing the transmission
		//     channel.
		ServiceNotAvailable = 421,
		//
		// Summary:
		//     The destination mailbox is in use.
		MailboxBusy = 450,
		//
		// Summary:
		//     The SMTP service cannot complete the request. This error can occur if the
		//     client's IP address cannot be resolved (that is, a reverse lookup failed).
		//     You can also receive this error if the client domain has been identified
		//     as an open relay or source for unsolicited e-mail (spam). For details, see
		//     RFC 2505, which is available at http://www.ietf.org.
		LocalErrorInProcessing = 451,
		//
		// Summary:
		//     The SMTP service does not have sufficient storage to complete the request.
		InsufficientStorage = 452,
		//
		// Summary:
		//     The client was not authenticated or is not allowed to send mail using the
		//     specified SMTP host.
		ClientNotPermitted = 454,
		//
		// Summary:
		//     The SMTP service does not recognize the specified command.
		CommandUnrecognized = 500,
		//
		// Summary:
		//     The syntax used to specify a command or parameter is incorrect.
		SyntaxError = 501,
		//
		// Summary:
		//     The SMTP service does not implement the specified command.
		CommandNotImplemented = 502,
		//
		// Summary:
		//     The commands were sent in the incorrect sequence.
		BadCommandSequence = 503,
		//
		// Summary:
		//     The SMTP service does not implement the specified command parameter.
		CommandParameterNotImplemented = 504,
		//
		// Summary:
		//     The SMTP server is configured to accept only TLS connections, and the SMTP
		//     client is attempting to connect by using a non-TLS connection. The solution
		//     is for the user to set EnableSsl=true on the SMTP Client.
		MustIssueStartTlsFirst = 530,
		/// <summary>
		/// Authentication mechanism is to weak
		/// </summary>
		AuthenticationWeak = 534,
		/// <summary>
		/// Authentication credentials invalid
		/// </summary>
		Authenticationinvalid = 535,
		//
		// Summary:
		//     The destination mailbox was not found or could not be accessed.
		MailboxUnavailable = 550,
		//
		// Summary:
		//     The user mailbox is not located on the receiving server. You should resend
		//     using the supplied address information.
		UserNotLocalTryAlternatePath = 551,
		//
		// Summary:
		//     The message is too large to be stored in the destination mailbox.
		ExceededStorageAllocation = 552,
		//
		// Summary:
		//     The syntax used to specify the destination mailbox is incorrect.
		MailboxNameNotAllowed = 553,
		//
		// Summary:
		//     The transaction failed.
		TransactionFailed = 554,
	}  
}
