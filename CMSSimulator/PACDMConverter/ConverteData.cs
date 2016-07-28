using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons.Resources;
using ConverterDB;
using PACDMSimulator.DVRConverter;
using PACDMSimulator.Events;
using Commons;
using System.Diagnostics;
using System.IO;
using ConverterDB.Model;

namespace PACDMSimulator
{
	internal class ConverteData : IDisposable
	{
		[Flags]
		private enum CovnertTypes:uint
		{
			None = 0,
			PACDM_CONVERT = 1 << 0,
			DVR_CONVERT = PACDM_CONVERT << 1
			
		}
		public int SocketPort {get;set;}
		public delegate void NotifySystemLog( LogEvent evt);
		public event NotifySystemLog OnSystemLog;
		ConvertDB LocalDb;

		ConverterDVR DVRConverter;
		PACDMConverter.PACConverter pacConverter;
		readonly CovnertTypes ConvertType = CovnertTypes.DVR_CONVERT | CovnertTypes.PACDM_CONVERT; 

		public ConverteData()
		{

		}

		public ConverteData(int _socketPort)
		{
			SocketPort = _socketPort;
		}

		private bool InitDB()
		{
			var connnection = ConfigurationManager.ConnectionStrings[Consts.LogContextConnection];
			if( connnection == null || string.IsNullOrEmpty(connnection.Name) || string.IsNullOrEmpty(connnection.ConnectionString))
			{
				RaiseSystemLog(new LogEvent(ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_STRING_NULL), EventLogEntryType.Information));
				return false;
			}
			try
			{
				LocalDb = new ConvertDB(Consts.LogContextConnection);
				return true;
			}
			catch(Exception ex)
			{
				RaiseSystemLog(new LogEvent(String.Format("{0}. {1}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_FAILED), ex.Message), EventLogEntryType.Information));
				LocalDb = null;
				return false;
			}
			
		}

		private void RaiseSystemLog(LogEvent evt)
		{
			if( OnSystemLog == null)
				return;
			OnSystemLog(evt);
		}

		public bool StartConvertTask()
		{
			if(ConvertType == CovnertTypes.None)
				return false;

			 if( !InitDB())
				return false;
			if( LocalDb.ServiceConfig == null || string.IsNullOrEmpty( LocalDb.ServiceConfig.Url))
			{
				string msg = String.Format("{0}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.CONVERTER_INVALID_WEBAPI));
				RaiseSystemLog(new LogEvent(msg , EventLogEntryType.Information));
				LocalDb.AddLog(new Log { DVRDate = DateTime.Now, LogID = (int)ERROR_CODE.CONVERTER_INVALID_WEBAPI, Message = msg, ProgramSet = (byte)Commons.Programset.UnknownType, Owner = true });
				return false;
			}
			if( (ConvertType & CovnertTypes.PACDM_CONVERT) == CovnertTypes.PACDM_CONVERT)
			{
				pacConverter = new PACDMConverter.PACConverter(this.LocalDb);
				pacConverter.StartConvertTask();
			}
			if( (ConvertType & CovnertTypes.DVR_CONVERT) == CovnertTypes.DVR_CONVERT)
			{
				DVRConverter = new ConverterDVR(this.LocalDb, SocketPort);
				DVRConverter.StartDVRConverter();
			}
			return true;
		}

		public void StopConvertTask()
		{
			if( DVRConverter != null)
			{
				DVRConverter.StopDVRConvert();
				DVRConverter.Dispose();
				DVRConverter = null;
			}

			if( pacConverter != null)
			{
				pacConverter.Dispose();
				pacConverter = null;
			}
		}

		public void Dispose()
		{
			if( LocalDb != null)
				LocalDb.Save();
		}
		
	}

}

