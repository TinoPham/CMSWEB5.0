using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons.Resources;
using ConverterDB;
using PACDMConverter.DVRConverter;
using PACDMConverter.Events;
using Commons;
using System.Diagnostics;
using System.IO;
using ConverterDB.Model;
using System.Net.Http.Formatting;
using PACDMConverter.PACDMConverter;
using System.Data.SqlClient;
using ConvertMessage;
using Pipes.Interfaces;
using System.Data.SqlServerCe;
using Microsoft.Win32;

namespace PACDMConverter
{
	internal class ConverteData : IDisposable
	{
		const string CacheResourceFileName = "CacheTable.txt";
		const string tbl_cache_header = "tbl_";
		public delegate void NotifySystemLog( LogEvent evt);
		public event NotifySystemLog OnSystemLog;
		volatile bool Stop = false;
		
		ConvertDB LocalDb;
		Pipes.Client.PipeClient pipe_client;
		ConverterDVR DVRConverter;
		PACConverter PACConverter;
		MonitorTask monitor_Task;
		string new_msiPath = null;
		RegistryChangeMonitor registry_monitor;

		#region
		
		CancellationTokenSource Monitor_CancelTokenSource = new CancellationTokenSource();
		CancellationTokenSource PACConverter_CancelTokenSource;
		CancellationTokenSource DVRConverter_CancelTokenSource;

		#endregion
		#if DEBUG

		readonly Enums.CovnertTypes ConvertType = Enums.CovnertTypes.PACDM_CONVERT | Enums.CovnertTypes.DVR_CONVERT;
		#else
		readonly Enums.CovnertTypes ConvertType = Enums.CovnertTypes.PACDM_CONVERT | Enums.CovnertTypes.DVR_CONVERT; 
#endif

		public ConverteData()
		{

		}
		#region
		private ConvertDB InitDB()
		{
			var connnection = ConfigurationManager.ConnectionStrings[Consts.LogContextConnection];
			if( connnection == null || string.IsNullOrEmpty(connnection.Name) || string.IsNullOrEmpty(connnection.ConnectionString))
			{
				RaiseSystemLog(new LogEvent(ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_STRING_NULL), EventLogEntryType.Information));
				return null;
			}
			try
			{
				ConvertDB LocalDb = new ConvertDB(Consts.LogContextConnection);
				return LocalDb;
			}
			catch(Exception ex)
			{
				RaiseSystemLog(new LogEvent(String.Format("{0}. {1}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_FAILED), ex.Message), EventLogEntryType.Information));
				LocalDb = null;
				return LocalDb;
			}
		}

		private void RaiseSystemLog(LogEvent evt)
		{
			if( OnSystemLog == null)
				return;
			OnSystemLog(evt);
		}
		private void StopRegistryMonitor()
		{
			if( registry_monitor != null)
			{
				registry_monitor.Changed -=new RegistryChangeHandler(registry_monitor_Changed);
				registry_monitor.Stop();
				registry_monitor = null;

			}
		}
		private void StartRegistryMonitor( RegistryHive Hive, string regpath )
		{
#if X64
			registry_monitor = new RegistryChangeMonitor(Hive, regpath, Microsoft.Win32.RegistryView.Registry64, RegistryChangeMonitor.REG_NOTIFY_CHANGE.NAME | RegistryChangeMonitor.REG_NOTIFY_CHANGE.ATTRIBUTES);
#else
			registry_monitor = new RegistryChangeMonitor(Hive, regpath, Microsoft.Win32.RegistryView.Registry32, RegistryChangeMonitor.REG_NOTIFY_CHANGE.NAME | RegistryChangeMonitor.REG_NOTIFY_CHANGE.ATTRIBUTES | RegistryChangeMonitor.REG_NOTIFY_CHANGE.LAST_SET);
#endif
			registry_monitor.Changed += new RegistryChangeHandler(registry_monitor_Changed);
			registry_monitor.Start();
		}

		void registry_monitor_Changed(object sender, RegistryChangeEventArgs e)
		{
			string proversion = Utils.Instance.GetRegValue( RegistryHive.LocalMachine, Utils.Instance.SRX_Pro_Reg_Settings_Path,Consts.STR_SRX_Pro_Version);
			if (string.Compare(proversion, DVRInfos.Instance.SRXProInfo.SRXProVersion, true) != 0)
			{
				//stop all process
				Stop = true;
				StopTask();
				DisposeTask();
				goto Start_Convert;
			}
			else
				return;

			//dispose all process
		 Start_Convert:
			Stop = false;
			DVRInfos.Instance.Refresh();
			Monitor_CancelTokenSource = new CancellationTokenSource();
			StartConvertTask( false);
		}

		public bool StartConvertTask(bool monitorProInstall = true )
		{
			if (ConvertType == Enums.CovnertTypes.None)
				return false;

			if( (this.LocalDb = InitDB()) == null)
			{
				RaiseSystemLog( new LogEvent( String.Format("{0}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.DB_CONNECTION_FAILED), EventLogEntryType.Error)));
				return false;
			}

			if (monitorProInstall)
				StartRegistryMonitor(RegistryHive.LocalMachine, Consts.STR_SOFTWARE);

			string domain_sid = Utils.Instance.GetRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Utils.Instance.PAC_Reg_Path, Consts.str_DomainSid);
			string domaindefault = Utils.Instance.GetRegValue(Microsoft.Win32.RegistryHive.LocalMachine, Utils.Instance.PAC_Reg_Path, Consts.str_AskDomain);

			if( DVRInfos.Instance == null || DVRInfos.Instance.SRXProInfo == null ||  string.IsNullOrEmpty( DVRInfos.Instance.SRXProInfo.ServerAppPath) || string.IsNullOrEmpty(domaindefault))
			{
				if (LocalDb.ServiceConfig == null || string.IsNullOrEmpty(LocalDb.ServiceConfig.Url))
				{
					string msg = String.Format("{0}", ResourceManagers.Instance.GetResourceString(ERROR_CODE.CONVERTER_INVALID_WEBAPI));
					RaiseSystemLog(new LogEvent(msg, EventLogEntryType.Information));
					LocalDb.AddLog(new Log { DVRDate = DateTime.Now, LogID = (int)ERROR_CODE.CONVERTER_INVALID_WEBAPI, Message = msg, ProgramSet = (byte)Commons.Programset.UnknownType, Owner = true });
					return false;
				}
			}
			else
			{
				if( LocalDb.DvrConverter == null)
				{
					LocalDb.Insert<ConverterDB.Model.DVRConverter>( new ConverterDB.Model.DVRConverter{ DvrSocketRetry= Consts.DVR_SOCKET_RETRY, Enable =true, TCPPort = Consts.DEFAULT_CMS_TCP_PORT} );
					LocalDb.Save();
					LocalDb.Refresh<ConverterDB.Model.DVRConverter>();
				}
				if (LocalDb.ServiceConfig == null || string.IsNullOrEmpty(LocalDb.ServiceConfig.Url))
				{
					LocalDb.Insert<ServiceConfig>( new ServiceConfig{
							Interval = Consts.MIN_CONVERT_INTERVAL,
							NumDVRMsg = Consts.DEFAULT_DVR_MSG,
							LogRecycle = Consts.DEFAULT_LOG_RECYCLE,
							Url = domaindefault
						}
						);
						LocalDb.Save();
						LocalDb.Refresh<ServiceConfig>();
				}
			}
			if(LocalDb.ServiceConfig == null)
				return false;
			if( CheckPACDMConverter(LocalDb))
				LocalDb.Refresh<ConvertInfo>();

			AddDBLog( new Log{ DVRDate = DateTime.Now, Owner = true, ProgramSet = (byte)Commons.Programset.DVR, Message = Messages.STR_START_SUCCESSFULL});

			monitor_Task = new MonitorTask();
			monitor_Task.OnHttpKeepAlive += new MonitorTask.HttpKeepAlive(monitor_Task_OnHttpLoginEvent);
			monitor_Task.OnHttpLoginEvent += new MonitorTask.HttpLoginEvent(monitor_Task_OnHttpLoginEvent);
			monitor_Task.OnRecycleLog += new MonitorTask.RecycleLog(monitor_Task_OnRecycleLog);
			monitor_Task.OnUpgradeEvent += new MonitorTask.UpgradeEvent(monitor_Task_OnUpgradeEvent);
			monitor_Task.OnlogMessage += new delegateLog(monitor_Task_OnlogMessage);
			monitor_Task.OnAskDomainResponse += new MonitorTask.AskDomainResponse(monitor_Task_OnAskDomainResponse);
			monitor_Task.OnAskDomainResponseDVR += new MonitorTask.AskDomainResponseDVR(monitor_Task_OnAskDomainResponseDVR);
			monitor_Task.OnDvrCommuniationportChange += new MonitorTask.DvrCommuniationportChange(monitor_Task_OnDvrCommuniationportChange);
			monitor_Task.OnStopConverter += new MonitorTask.StopConverter(monitor_Task_OnStopConverter);
			monitor_Task.StartMonitor(LocalDb.ServiceConfig, Monitor_CancelTokenSource.Token, DVRInfos.Instance.SRXProInfo, domaindefault, domain_sid, (UInt16)LocalDb.DvrConverter.TCPPort);

			return true;
		}

		void monitor_Task_OnStopConverter(object sender, bool stopPACConverter, bool StopDvr)
		{
			if(StopDvr)
			{
				StopDVRConvert();
				DisposeDVRConvert();
			}
			else
			{
				InitDVRConverter(null, (int)(TimeSpan.FromDays(ConvertMessage.Consts.DVR_Message_Recyle).TotalMinutes));
				DVRConverter.DVRInfoChange( null,(int)(TimeSpan.FromDays(ConvertMessage.Consts.DVR_Message_Recyle).TotalMinutes));
			}
			if( stopPACConverter)
			{
				StopPACConvert();
				DisposePACConvert();
			}
		}

		void monitor_Task_OnDvrCommuniationportChange(object sender, ushort newvalue)
		{
			if (LocalDb.DvrConverter == null || newvalue <= 0 )
				return;

			if( LocalDb.DvrConverter.TCPPort != (int)newvalue)
			{
				LocalDb.DvrConverter.TCPPort = newvalue;
				LocalDb.Save();
				LocalDb.Refresh<ConverterDB.Model.DVRConverter>();
				StopDVRConvert();
				DVRConverter = null;
			}

			//if(LocalDb.DvrConverter.Enable && InitSocket)
			//{
			//    if(DVRConverter == null)
			//        InitDVRConverter(loginToken, (int)(TimeSpan.FromDays(ConvertMessage.Consts.DVR_Message_Recyle).TotalMinutes));
			//    else
			//        DVRConverter.DVRInfoChange(loginToken, (int)(TimeSpan.FromDays(ConvertMessage.Consts.DVR_Message_Recyle).TotalMinutes));
			//}
			//ConverterUpdateNewKeepalive(KeepAlive,loginToken);

		}

		void monitor_Task_OnAskDomainResponseDVR(object sender, string sid)
		{
			Utils.Instance.SetRegValue( Microsoft.Win32.RegistryHive.LocalMachine,  Utils.Instance.PAC_Reg_Path, Consts.str_DomainSid, sid);
		}

		void monitor_Task_OnAskDomainResponse(object sender, DomainResponse response)
		{
			try
			{
				if(response != null && string.Compare(LocalDb.ServiceConfig.Url, response.Url, true) != 0)
				{
					LocalDb.ServiceConfig.Url = response.Url;
					LocalDb.Save();
					LocalDb.Refresh<ServiceConfig>();
					if( DVRConverter != null)
						DVRConverter.DVRXMLConfigChange();
				}
			}
			catch(Exception){}
		}

		void monitor_Task_OnlogMessage(object sender, string msg, Commons.ERROR_CODE errID)
		{
			Log log = new Log{ DVRDate = DateTime.Now, Owner = true, ProgramSet = (byte)Commons.Programset.DVR, Message = msg, LogID = (int)errID};
			AddDBLog( log);
		}

		private void AddDBLog( Log log)
		{
			if( log == null || LocalDb == null)
				return;
			LocalDb.AddLog(log);
		}

		void monitor_Task_OnUpgradeEvent(object sender, string msipath, string version)
		{
			//throw new NotImplementedException();
			string src_appupgrade = Path.Combine(Utils.Instance.StartupDir, Consts.APP_Upgrade);
			string des = Path.Combine(Utils.Instance.StartupDir, Consts.STR_UPGRADE, Consts.APP_Upgrade);
			new_msiPath = msipath;
			if( Utils.FileCopy(src_appupgrade, des))
			{
				StopConvertTask();
				if( !TriggerGUI( version))
					ExcuteUpgradeApp(des, msipath);
				else
				{
					//watting event from Config tool to excute upgrade
				}
			}
		}
		
		private void ExcuteUpgradeApp(string msipath)
		{
			string src_appupgrade = Path.Combine(Utils.Instance.StartupDir, Consts.APP_Upgrade);
			string des = Path.Combine(Utils.Instance.StartupDir, Consts.STR_UPGRADE, Consts.APP_Upgrade);
			ExcuteUpgradeApp(des, msipath);
		}
		
		private void ExcuteUpgradeApp(string path, string msipath)
		{
			ProcessStartInfo pinfo = new ProcessStartInfo(path);
			pinfo.Arguments += Consts.Cmd_Stop_service + string.Format(" \"{0}\"", Consts.STR_CONVERTER);
			pinfo.Arguments += " " + Consts.Cmd_KillProcessID + string.Format(" {0}", Process.GetCurrentProcess().Id);
			pinfo.Arguments += " " + Consts.Cmd_Install + string.Format(" \"{0}\"", msipath);
			pinfo.UseShellExecute = false;
			Process.Start(pinfo);
		}
		
		private bool TriggerGUI( string newversion )
		{
			pipe_client = new Pipes.Client.PipeClient(Consts.PIPE_NAME);
			pipe_client.ClientDisconnectedEvent += new EventHandler<Pipes.Interfaces.ClientDisconnectedEventArgs>(pipe_client_ClientDisconnectedEvent);
			pipe_client.MessageReceivedEvent +=new EventHandler<Pipes.Interfaces.MessageReceivedEventArgs>(pipe_client_MessageReceivedEvent);
			if(!pipe_client.Start())
				return false;

			PipeModels.VersionModel model = new PipeModels.VersionModel{ OldVersion = Utils.Instance.Version.ToString(), NewVersion = newversion};
			PipeModels.MessageModel message = new PipeModels.MessageModel{ ClassModel = model.GetType().FullName, Data = model.ToString()};
			Task<Pipes.Utilities.TaskResult> rsult = pipe_client.SendMessage( message.ToString() );
			return rsult.Result.IsSuccess;
		}
	
		private void pipe_client_MessageReceivedEvent( object sender, MessageReceivedEventArgs agrs)
		{

		}

		private void pipe_client_ClientDisconnectedEvent( object sender, Pipes.Interfaces.ClientDisconnectedEventArgs events )
		{
			if( !string.IsNullOrEmpty(new_msiPath) && File.Exists(new_msiPath))
			{
				ExcuteUpgradeApp( new_msiPath);
			}
		}

		private void StopMonitor()
		{
			if( monitor_Task != null)
			{
				monitor_Task.OnHttpKeepAlive -= monitor_Task_OnHttpLoginEvent;
				monitor_Task.OnHttpLoginEvent -= monitor_Task_OnHttpLoginEvent;
				monitor_Task.OnRecycleLog -= monitor_Task_OnRecycleLog;
				monitor_Task.OnUpgradeEvent -= monitor_Task_OnUpgradeEvent;
			}

			if (Monitor_CancelTokenSource != null)
			{
				Monitor_CancelTokenSource.Cancel();
				
			}
		}
		
		void monitor_Task_OnRecycleLog(object sender, int numofday)
		{
			int total_days = ConvertMessage.Consts.Default_LogRecyle;
			if( numofday > 0)
			{
				total_days = numofday;
			}
			DateTime last_date = DateTime.Now.AddDays(-total_days);

			//ConvertDB db = InitDB();
			if( LocalDb == null)
				return;
			try
			{
				LocalDb.ExecuteCommand("Delete [Logs] Where DVRDate < @date", new SqlCeParameter("@date", last_date));
			}catch(Exception){}
			//throw new NotImplementedException();
		}

		void monitor_Task_OnHttpLoginEvent(object sender, LoginEventArgs eventargs)
		{
			MessageKeepAlive keepalive = eventargs.KeepAlive;
			if( keepalive == null)
				return;

			bool refresh_cache = LocalDb != null && LocalDb.ServiceConfig != null && string.Compare(LocalDb.ServiceConfig.ServerID, keepalive.ServerID, false) != 0;
			if( refresh_cache)
				ClearCacheTables();

			LocalDb.ServiceConfig.ServerID = keepalive.ServerID;
			LocalDb.Save();
			LocalDb.Refresh<ServiceConfig>();

			if(!Stop)
				DVRUpdateNewKeepalive(keepalive, eventargs.Token);
			if( !Stop)
			{
				ConverterUpdateNewKeepalive(keepalive, eventargs.Token);
			}
		}

		private void DVRUpdateNewKeepalive(MessageKeepAlive keepalive, string tokenID)
		{
			if (keepalive == null || Stop)
				return;
			ConverterDB.Model.DVRConverter dvrconfig = LocalDb.DvrConverter;
#if KEEPALIVE
			if (keepalive.DVRConvert.HasValue && dvrconfig.Enable != keepalive.DVRConvert.Value)//disable dvr from api
			{
				StopDVRConvert();
				DisposeDVRConvert();
				dvrconfig.Enable = keepalive.DVRConvert.Value;
				LocalDb.Update<ConverterDB.Model.DVRConverter>(dvrconfig);
				LocalDb.Save();
			}
#endif
			if(dvrconfig.Enable) //already enable dvr
			{
				if( DVRConverter != null )
				{
					int keepdvrmsg = keepalive.DVRMessageRecycle.HasValue?  keepalive.DVRMessageRecycle.Value : ConvertMessage.Consts.DVR_Message_Recyle;
					DVRConverter.DVRInfoChange(tokenID, (int)(TimeSpan.FromDays(keepdvrmsg).TotalMinutes));
					return;
				}
				if (DVRConverter == null || DVRConverter_CancelTokenSource == null || DVRConverter_CancelTokenSource.IsCancellationRequested)
				{
					int keepdvrmsg = keepalive.DVRMessageRecycle.HasValue?  keepalive.DVRMessageRecycle.Value : ConvertMessage.Consts.DVR_Message_Recyle;
					InitDVRConverter(tokenID, (int)(TimeSpan.FromDays(keepdvrmsg).TotalMinutes));
				}
			}
			else//already disable DVR
			{
#if KEEPALIVE
				if (DVRConverter != null)
				{
					StopDVRConvert();
					DisposeDVRConvert();
				}
#endif
			}
		}
		private void ClearCacheTables()
		{
			if( LocalDb == null || Stop)
				return;
			IEnumerable<string> tables = LocalDb.AllTables();
			foreach( string tbl in tables)
			{
				if(!tbl.StartsWith(tbl_cache_header, StringComparison.InvariantCultureIgnoreCase))
					continue;
				LocalDb.DeleteData(tbl);
				LocalDb.Refresh(tbl);
			}
		}
		private void ConverterUpdateNewKeepalive(MessageKeepAlive keepalive, string tokenID)
		{
			if( keepalive == null || Stop)
				return;
#if KEEPALIVE
			ConvertMessage.ConvertInfoConfig[] newcfgs = keepalive.ConvertInfo;
			bool need_stop = LocalDb.ConvertInfo.Any( it => it.Enable == true);
			if( need_stop) //stop converter before update new config
			{
				StopPACConvert();
				DisposePACConvert();
			}
			ConverterDB.Model.ConvertInfo info = null;
			short order = -1;
			if(keepalive.ConvertInfo != null)
			{
				foreach( var newcfg in keepalive.ConvertInfo)
				{
					order++;
					info = LocalDb.ConvertInfo.FirstOrDefault( it => it.Programset == newcfg.Programset && string.Compare(newcfg.TableName, newcfg.TableName, true) ==0 );
					if( info == null)
						continue;
					info.Enable = newcfg.Enable;
					info.Order = (byte)order;
					if(newcfg.DvrDate.HasValue)
						info.DvrDate = newcfg.DvrDate.Value;
					if(!string.IsNullOrEmpty( newcfg.LastKey))
						info.LastKey = newcfg.LastKey;
					LocalDb.Update<ConvertInfo>(info);
				}

				LocalDb.Save();
			}

			if(keepalive.DataReset.HasValue && keepalive.DataReset.Value)
			{
				List<string> table = Loadtable(CacheResourceFileName);
				table.ForEach(item => DeleteCache(LocalDb, item));
			}
#endif
			if( LocalDb.ConvertInfo.Any( it => it.Enable == true) && !Stop)
			{
				InitPACConverter(tokenID);
			}
		}

		List<string> Loadtable(string filename)
		{
			List<string> ret = new List<string>();

			string filepath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), filename);
			StreamReader reader = null;
			if (File.Exists(filepath))
				reader = new StreamReader(filepath, Encoding.UTF8, true);
			else
			{
				string full_resName = string.Format("{0}.Resource.{1}", this.GetType().Namespace, filename);
				reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream(full_resName));
			}
			if (reader != null)
			{
				ret = LoadTable(reader);
				reader.Close();

				reader.Dispose();
			}

			return ret;
		}

		List<string> LoadTable(StreamReader reader)
		{
			if (reader == null)
				return new List<string>();

			List<String> ret = new List<string>();
			string line = string.Empty;
			while (!string.IsNullOrEmpty(line = reader.ReadLine()))
				ret.Add(line);
			return ret;
		}

		private void DeleteCache(ConvertDB database, string tablename)
		{
			database.DeleteData(tablename);
		}

		private void InitDVRConverter(string tokenID, int offlivekeeping)
		{
			ConvertDB db = InitDB();
			if( db == null || DVRConverter != null)
				return;

			if ((ConvertType & Enums.CovnertTypes.DVR_CONVERT) == Enums.CovnertTypes.DVR_CONVERT && db.DvrConverter != null && db.DvrConverter.Enable == true)
			{
				DVRConverter_CancelTokenSource = new CancellationTokenSource();
				DVRConverter = new ConverterDVR(db, DVRConverter_CancelTokenSource.Token, tokenID, offlivekeeping);
				DVRConverter.OnApiTokenExpired += PACConverter_OnApiTokenExpired;
				DVRConverter.StartDVRConverter();
			}
			else
			{
				if( LocalDb != null)
				{
					LocalDb.AddLog(new Log
					{
						LogID = (int)Commons.ERROR_CODE.CONVERTER_DISABLE_DVRCONNECTION,
						DVRDate = DateTime.Now,
						Owner = true,
						ProgramSet = (byte)Commons.Programset.DVR,
						Message = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_DISABLE_DVRCONNECTION)
					});
				}
			}
		}
		
		private void StopDVRConvert()
		{
			
			if (DVRConverter != null)
			{
				DVRConverter.StopDVRConvert();
			}
			if (DVRConverter_CancelTokenSource != null)
				DVRConverter_CancelTokenSource.Cancel(false);
		}
		
		private void InitPACConverter(string tokenID)
		{
			ConvertDB ConvertDB = InitDB();
			if( ConvertDB == null)
				return;
			DisposePACConvert();
			ConvertInfo cinfo = ConvertDB.ConvertInfo.FirstOrDefault(item => item.Enable == true);
			if ((ConvertType & Enums.CovnertTypes.PACDM_CONVERT) == Enums.CovnertTypes.PACDM_CONVERT && cinfo != null)
			{
				PACConverter_CancelTokenSource = new CancellationTokenSource();
				PACConverter = new PACDMConverter.PACConverter(ConvertDB, PACConverter_CancelTokenSource.Token, tokenID);
				PACConverter.OnApiTokenExpired += new ApitokenExpired(PACConverter_OnApiTokenExpired);
				PACConverter.StartConvertTask();
			}
			else
				ConvertDB.AddLog(new Log
				{
					LogID = (int)Commons.ERROR_CODE.CONVERTER_DISABLE_ALL_PACDATA,
					DVRDate = DateTime.Now,
					Owner = true,
					ProgramSet = (byte)Commons.Programset.DVR,
					Message = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.CONVERTER_DISABLE_ALL_PACDATA)
				});

		}

		void PACConverter_OnApiTokenExpired(object sender, ERROR_CODE error)
		{
			//throw new NotImplementedException();
			if( Stop)
				return;
			AddDBLog( new Log{ LogID = (int)error, ProgramSet = sender.Equals(DVRConverter)?(byte)Programset.DVR : (byte)Programset.POS, Owner = true, DVRDate = DateTime.Now, Message = null, MsgClass = null});
			if(!Stop && monitor_Task != null)
					monitor_Task.TriggerTokenExpired();
		}

		private bool CheckPACDMConverter(ConvertDB convertDB)
		{
			bool issave = false;
			issave |= CheckPACConverter(convertDB, Commons.Programset.POS, MSAccessObjects.ConstEnums.Transact);
			issave |= CheckPACConverter(convertDB, Commons.Programset.POS, MSAccessObjects.ConstEnums.Sensor);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.TrafficCount);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.Alarm);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.DriveThrough);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.Count);
			issave |= CheckPACConverter(convertDB, Commons.Programset.CA, MSAccessObjects.ConstEnums.Transact);
			issave |= CheckPACConverter(convertDB, Commons.Programset.ATM, MSAccessObjects.ConstEnums.Transact);
			issave |= CheckPACConverter(convertDB, Commons.Programset.LPR, MSAccessObjects.ConstEnums.Info);
			if (issave)
				convertDB.Save();

			return issave;
		}

		private bool CheckPACConverter(ConvertDB convertDB, Commons.Programset pset, string tablename)
		{
			ConvertInfo info = convertDB.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)pset && string.Compare(item.TableName, tablename, true) == 0);
			if (info == null)
			{
				int count = convertDB.ConvertInfo.Count();
				int lastmonth = Consts.Last_Month_Convert_Data_Support;
				DateTime dvrdate = DateTime.Now.AddMonths(-lastmonth);
				convertDB.Insert<ConvertInfo>(new ConvertInfo { TableName = tablename, Programset = (byte)pset, Enable = true, LastKey = "0", Order = (byte)count, UpdateDate = dvrdate, DvrDate = dvrdate });
				return true;
			}
			return false;
		}

		private void StopPACConvert()
		{
			
			if(PACConverter_CancelTokenSource != null)
				PACConverter_CancelTokenSource.Cancel( false);
			
		}
	
		private void DisposeMonitor()
		{
			if (monitor_Task != null)
			{
				monitor_Task.Dispose();
				monitor_Task = null;
				Monitor_CancelTokenSource.Dispose();
				Monitor_CancelTokenSource = null;
			}
		}
	
		private void DisposeDVRConvert()
		{
			if( DVRConverter != null)
			{
				DVRConverter.Dispose();
				DVRConverter = null;
				DVRConverter_CancelTokenSource.Dispose();
				DVRConverter_CancelTokenSource = null;
			}
		}
	
		private void DisposePACConvert()
		{
			if (PACConverter != null)
			{
				PACConverter.OnApiTokenExpired -= new ApitokenExpired(PACConverter_OnApiTokenExpired);
				PACConverter.Dispose();
				PACConverter = null;
				PACConverter_CancelTokenSource.Dispose();
				PACConverter_CancelTokenSource = null;

			}
		}
		private void DisposeTask()
		{
			DisposeMonitor();
			DisposeDVRConvert();
			DisposePACConvert();
		}
		private void StopTask()
		{
			StopMonitor();
			StopPACConvert();
			StopDVRConvert();
		}
		#endregion

		public void StopConvertTask()
		{
			Stop = true;
			StopRegistryMonitor();
			StopTask();
		}

		public void Dispose()
		{
			
			DisposeTask();
			AddDBLog(new Log { DVRDate = DateTime.Now, Owner = true, ProgramSet = (byte)Commons.Programset.DVR, Message = Messages.STR_STOP_SUCCESSFULL });
			if( LocalDb != null)
				LocalDb.Save();
		}
		#region

		#endregion
		
	}

}

