using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMConverter.DVRConverter;
using System.Net;

namespace PACDMConverter.Events
{
	public delegate void delegateLog( object sender, string msg, Commons.ERROR_CODE errID);
	public delegate void ApitokenExpired( object sender, Commons.ERROR_CODE error);

	internal class LogEvent :EventArgs
	{
		public EventLogEntryType EventType{ get; private set;}
		public string Message{ get; set;}

		public LogEvent(): base()
		{
		}
		public LogEvent(string Msg, EventLogEntryType type = EventLogEntryType.Information)
			: base()
		{
			Message = Msg;
			EventType = type;

		}

	}

	public abstract class EventBase : EventArgs 
	{
		public virtual string Message{ get; set;}
		public object Sender{ get;set;}
		public Commons.ERROR_CODE ErrorCode{ get;set;}
	}

	public class ConvertDBEventArgs: EventBase
	{
		public Commons.Programset Programset{ get; set;}
	}

	public class SocketEvent : EventBase
	{
		public System.Net.Sockets.SocketError SockError{ get; set;}
	}

	public class SocketReceivedData: EventBase
	{
		public DVRDataMessage Data{ get; set;}
	}

	public class LoginEventArgs : EventArgs
	{
		public Commons.ERROR_CODE Status { get; set; }

		public string Token { get; set; }

		public ConvertMessage.MessageKeepAlive KeepAlive { get; set; }
	}

	public class RegistryChangeEventArgs : EventArgs
	{
		#region Fields
		private bool _stop;
		private Exception _exception;
		private RegistryChangeMonitor _monitor;
		#endregion

		#region Constructor
		public RegistryChangeEventArgs(RegistryChangeMonitor monitor)
		{
			this._monitor = monitor;
		}
		#endregion

		#region Properties
		public RegistryChangeMonitor Monitor
		{
			get { return this._monitor; }
		}

		public Exception Exception
		{
			get { return this._exception; }
			set { this._exception = value; }
		}

		public bool Stop
		{
			get { return this._stop; }
			set { this._stop = value; }
		}
		#endregion
	}
}
