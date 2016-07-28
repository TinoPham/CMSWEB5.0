using Commons;
using Commons.Resources;
using ConverterDB;
//using PACDMSimulator.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PACDMSimulator
{
	public class ConverterSimulator
	{
		//ConverteData converter;
		ConvertDB localDB;
		public int SocketPort {get;set;}
		public bool IsRuning = false;	

		public ConverterSimulator()
		{
			//bool init = false;
			//while (true)
			//{
			//	if (!init)
			//	{
			//		OnStart(null);
			//		init = true;
			//	}
			//}
		}

		public ConverterSimulator(int socket)
		{
			SocketPort = socket;
		}

		public void OnStart(string[] args)
		{
			StartConverter();
			//Task t = Task.Factory.StartNew(StartConverter);
		}

		public void StartConverter()
		{
			////bool result = false;
			//try
			//{
			//	converter = new ConverteData(SocketPort);
			//	converter.OnSystemLog += converter_OnSystemLog;
			//	if (!converter.StartConvertTask())
			//	{
			//		converter.Dispose();
			//		converter = null;
			//	}
			//	IsRuning = true;
			//}
			//catch
			//{
			//	IsRuning = false;
			//}

			//return result;
		}

		public void StopAllProgress()
		{
			//if (converter != null)
			//{
			//	converter.StopConvertTask();
			//	converter = null;
			//	IsRuning = false;
			//}
		}

		//void converter_OnSystemLog(LogEvent evt)
		//{
		//	LogEvent(evt);
		//}

		//public void OnStop()
		//{
		//	StopAllProgress();
		//}

		//void LogEvent(LogEvent evt)
		//{
		//	//base.EventLog.WriteEntry(evt.Message, evt.EventType);
		//}

		//public void StopConverter()
		//{
		//	//bool result = false;
		//	try
		//	{
		//		foreach (var converter in converterList.Where(t => t.SocketPort == SocketPort))
		//		{
		//			if (converter != null)
		//			{
		//				converter.StopConvertTask();
		//				converterList.Remove(converter);
		//				//converter = null;
		//			}
		//		}
		//		IsRuning = true;
		//	}
		//	catch
		//	{
		//		IsRuning = false;
		//	}
		//	//return result;
		//}

		//public bool StartConverter(int socket)
		//{
		//	bool result = false;
		//	try
		//	{
		//		ConverteData converter = new ConverteData(socket);
		//		converterList.Add(converter);
		//		converter.OnSystemLog += converter_OnSystemLog;
		//		if (!converter.StartConvertTask())
		//		{
		//			converter.Dispose();
		//			converterList.Remove(converter);
		//		}

		//		return true;
		//	}
		//	catch
		//	{
		//		return false;
		//	}

		//	return result;
		//}

		//public bool StopConverter(int socket)
		//{
		//	bool result = false;
		//	try
		//	{
		//		foreach (var converter in converterList.Where(t => t.SocketPort == socket))
		//		{
		//			if (converter != null)
		//			{
		//				converter.StopConvertTask();
		//				converterList.Remove(converter);
		//				//converter = null;
		//			}
		//		}
		//		return true;
		//	}
		//	catch
		//	{
		//		return false;
		//	}
		//	return result;
		//}


		//public void StartAllProgress()
		//{
		//	if (!InitDB())
		//	{
		//		return;
		//	}
		//	if (localDB.ServiceConfig == null || string.IsNullOrEmpty(localDB.ServiceConfig.Url))
		//	{
		//		string msg = String.Format("{0}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.CONVERTER_INVALID_WEBAPI));
		//		//RaiseSystemLog(new LogEvent(msg, EventLogEntryType.Information));
		//		//LocalDb.AddLog(new Log { DVRDate = DateTime.Now, LogID = (int)ERROR_CODE.CONVERTER_INVALID_WEBAPI, Message = msg, ProgramSet = (byte)Commons.Programset.UnknownType, Owner = true });
		//		return;
		//	}

		//	foreach (var dvrconverter in localDB.GetAllDvrConverter())
		//	{
		//		ConverteData converter = new ConverteData(dvrconverter.TCPPort);
		//		converterList.Add(converter);
		//	}

		//	foreach (var converter in converterList)
		//	{
		//		converter.OnSystemLog += converter_OnSystemLog;
		//		if (!converter.StartConvertTask())
		//		{
		//			converter.Dispose();
		//			converterList.Remove(converter);
		//			//converter = null;
		//		}
		//	}
		//}

	}



}
