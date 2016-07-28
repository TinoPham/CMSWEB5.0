using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PACDMConverter.Events;
using System.Reflection;

namespace PACDMConverter
{
	public partial class PACDMService : ServiceBase
	{
		ConverteData converter;
		public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
		public PACDMService()
		{
			InitializeComponent();
			base.AutoLog = true;
#if DEBUG
			bool init = false;
			while (true)
			{
				if (!init)
				{
					OnStart(null);
					init = true;
					//HASPkey.HaspLicense.Instance.ReadLicense(@"C:\i3Pro Server\");
				}
				System.Threading.Thread.Sleep(1000);
			}
#endif
		}

		protected override void OnStart(string[] args)
		{
			StartAllProgress();
			base.OnStart(args);
		}

		private void StopAllProgress()
		{
			if (converter != null)
			{
				converter.StopConvertTask();
				converter.Dispose();
				converter = null;
			}
		}
		private void StartAllProgress()
		{
			//System.Threading.Thread.Sleep(10000);
			converter = new ConverteData();
			converter.OnSystemLog += converter_OnSystemLog;
			if (!converter.StartConvertTask())
			{
				converter.Dispose();
				converter = null;
			}
		}
		void converter_OnSystemLog(LogEvent evt)
		{
			LogEvent( evt);
		}

		protected override void OnStop()
		{

			StopAllProgress();
			base.OnStop();
		}

		void LogEvent(LogEvent evt)
		{
			//if (!EventLog.SourceExists(Consts.LOG_SOURCE))
			//{
			//	EventLog.CreateEventSource(Consts.LOG_SOURCE, Consts.STR_APPLICATION);
			//}

			//EventLog eLog = new EventLog();
			//eLog.Source = Consts.LOG_SOURCE;

			//eLog.WriteEntry(evt.Message,  evt.EventType);
			#if !DEBUG
			base.EventLog.WriteEntry(evt.Message,  evt.EventType);
			#endif
		}

	}
}
