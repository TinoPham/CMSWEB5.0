using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMSimulator.DVRConverter;

namespace PACDMSimulator.Events
{
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
}
