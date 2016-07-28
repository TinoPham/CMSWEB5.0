using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.IO;
using ConverterDB;
using ConverterDB.Model;

namespace ServiceConfig
{
	internal class PACDMConverterConfig : Commons.SingletonClassBase<PACDMConverterConfig>
	{
        public const string PACDMConverter_ProcessName = "PACDMConverter";
		//<add name="LocalDb" connectionString="Data Source=|DataDirectory|\ConverterDB.sdf" />
		

		//private static readonly Lazy<PACDMConverterConfig> Lazy = new Lazy<PACDMConverterConfig>(() => new PACDMConverterConfig());
		//public static PACDMConverterConfig Instance { get { return Lazy.Value; } }
		public string ConverterServiceName { get { return ConfigurationManager.AppSettings.Get(Consts.STR_ConverterServiceKey); } }
		public string ConvertDBConnection
		{	
			get{ return LoadConfig();}
		}
		private string LoadConfig()
		{
			string converterName = ConfigurationManager.AppSettings.Get(Consts.STR_Converter);
			string connectionName = ConfigurationManager.AppSettings.Get(Consts.STR_ConnectionName);
#if DEBUG
			string configpath = new DirectoryInfo( Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
			configpath = Path.Combine(configpath, @"PACDMConverter\bin\debug");
			configpath = @"D:\PAC\PACDMConverter";
#else
			string configpath = Directory.GetCurrentDirectory();
#endif
			Configuration config = ConfigurationManager.OpenExeConfiguration(Path.Combine(configpath, converterName));
			ConnectionStringSettings setting = config.ConnectionStrings.ConnectionStrings[connectionName];
			if( setting == null)
			{
				return AddDefaultConnection(config, connectionName, Consts.Default_DBConnection) == false ? string.Empty : Consts.Default_DBConnection;
			}

			return setting.ConnectionString;
		}

		private bool AddDefaultConnection(Configuration config, string conName, string value)
		{
			try
			{
				config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings { Name = conName, ConnectionString = value });
				config.Save();
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		public void CheckData(ConvertDB covnertDb)
		{
		   CheckDVRConnection(covnertDb);
		   CheckPACDMConverter(covnertDb);
		}
		private void CheckPACDMConverter(ConvertDB convertDB)
		{
			bool issave = false;
			issave |= CheckPACConverter( convertDB, Commons.Programset.POS, MSAccessObjects.ConstEnums.Transact);
			issave |= CheckPACConverter(convertDB, Commons.Programset.POS, MSAccessObjects.ConstEnums.Sensor);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.TrafficCount);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.Alarm);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.DriveThrough);
			issave |= CheckPACConverter(convertDB, Commons.Programset.IOPC, MSAccessObjects.ConstEnums.Count);
			issave |= CheckPACConverter(convertDB, Commons.Programset.CA, MSAccessObjects.ConstEnums.Transact);
			issave |= CheckPACConverter(convertDB, Commons.Programset.ATM, MSAccessObjects.ConstEnums.Transact);
			if( issave)
				convertDB.Save();

		}
		private bool CheckPACConverter(ConvertDB convertDB, Commons.Programset pset, string tablename)
		{
			ConvertInfo info = convertDB.ConvertInfo.FirstOrDefault( item => item.Programset == (byte)pset && string.Compare( item.TableName, tablename, true) == 0);
			if( info == null)
			{
				int count = convertDB.ConvertInfo.Count();
				string strlastmonth = ConfigurationManager.AppSettings.Get(Consts.STR_LastMonthConvert); 
				int lastmonth;
				Int32.TryParse(strlastmonth, out lastmonth);
				DateTime dvrdate = DateTime.Now.AddMonths( -lastmonth);
				convertDB.Insert<ConvertInfo>( new ConvertInfo{ TableName = tablename, Programset = (byte)pset, Enable = true, LastKey = "0", Order = (byte)count, UpdateDate =dvrdate, DvrDate = dvrdate});
				return true;
			}
			return false;
		}
		private void CheckDVRConnection(ConvertDB convertDB)
		{
			if( convertDB.DvrConverter == null || convertDB.DvrConverter.ID == 0)
			{
				string tcpport = ConfigurationManager.AppSettings.Get(Consts.STR_CMSServerPort);
				int port;
				Int32.TryParse(tcpport, out port);
				convertDB.Insert<DVRConverter>(new DVRConverter { DvrSocketRetry = Consts.DVR_SOCKET_RETRY, TCPPort = port, Enable = true });
				convertDB.Save();
			}
		}
	}
}
