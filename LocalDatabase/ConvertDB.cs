using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Commons.Resources;
using ConverterDB.Model;

namespace ConverterDB
{
	public class ConvertDB : IResposity, IDisposable
	{
		private class Mapping
		{
			public string Provider { get;set;}
			public string ClassName { get; set;}
		}

		private class MappingContext : Commons.SingletonStringTypeMappingBase<MappingContext, Mapping>
		{
			private MappingContext()
			{
				base.AddMapping(MongoDB_Provider, new Mapping { Provider = "mongodb://", ClassName = typeof(MongoResposity).FullName });
				base.AddMapping(SqlCe_Provider, new Mapping { Provider = "Data Source", ClassName = typeof(SqlCeResposity).FullName });
			}

			public string GetContextClass(string connection)
			{
				KeyValuePair<string, Mapping> mapping = base.MappingDict.FirstOrDefault(item => connection.ToLower().Contains(item.Value.Provider.ToLower()));
				return mapping.Value == null ? string.Empty : mapping.Value.ClassName;
			}
			public string[] Allkeys()
			{
				return MappingDict.Select(item => item.Key).ToArray();
			}

		}

		//private static Dictionary<string, Mapping> _MappingContext = null;

		//private Dictionary<string, Mapping> MappingContext
		//{
		//    get 
		//    {
		//        if( _MappingContext == null)
		//        {
		//            _MappingContext = new Dictionary<string, Mapping>();
		//            _MappingContext.Add(MongoDB_Provider, new Mapping { Provider = "mongodb://", ClassName = typeof(MongoResposity).FullName });
		//            _MappingContext.Add(SqlCe_Provider, new Mapping { Provider = "Data Source", ClassName = typeof(SqlCeResposity).FullName });
		//        }
		//        return _MappingContext;
		//    }
		//}



		const string MongoDB_Provider = "mongodb";
		const string SqlCe_Provider = "System.Data.SqlServerCe";
		readonly object locker = new object();
		public ServiceConfig ServiceConfig
		{
			get
			{
				lock(locker)
				{
				return database.FirstOrDefault<ServiceConfig>( item => string.IsNullOrEmpty(item.Url) == false);
				}
				//return database.Query<ServiceConfig>().FirstOrDefault();
			}
		}
		
		public IEnumerable<ConvertInfo> ConvertInfo
		{
			get{
				lock(locker)
				{
					return database.Query<ConvertInfo>().OrderBy( item => item.Order).ToList();
				}
			}
		}
		public DVRConverter DvrConverter
		{
			get{
				lock(locker)
				{
					return database.Query<DVRConverter>().FirstOrDefault();
				}
		}
		}

		public DVRConverter GetDvrConverter(int socketPort)
		{
			lock (locker)
			{
				return database.Query<DVRConverter>(item => item.Enable == true && item.TCPPort == socketPort).FirstOrDefault();
			}
		}

		public List<DVRConverter> GetAllDvrConverter()
		{
			lock (locker)
			{
				return database.Query<DVRConverter>(item => item.Enable == true).ToList();
			}
		}

		public IQueryable<DVRMessage> DVRMessages
		{
			get
			{
				lock(locker)
				{
				return database.Query<DVRMessage>();
			}
		}
		}

		IResposity database;
		private string GetConnectionString(string NameOrConnectionString)
		{
			ConnectionStringSettings con_setting = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().FirstOrDefault(item => string.Compare(item.Name, NameOrConnectionString, true) == 0);
			return con_setting == null ? NameOrConnectionString : con_setting.ConnectionString;
		}

		public ConvertDB(string NameOrConnectionString)
		{
			string connstrionstring = GetConnectionString(NameOrConnectionString);
			if( string.IsNullOrEmpty(connstrionstring))
			{
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString( Commons.ERROR_CODE.DB_CONNECTION_STRING_NULL) );
			}
			string contextclass =MappingContext.Instance.GetContextClass(connstrionstring);
			if( string.IsNullOrEmpty(contextclass))
			{
				string all_context = String.Join(", ", MappingContext.Instance.Allkeys());
				Commons.Utils.ThrowExceptionMessage( string.Format( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_INVALID), all_context) );
			}

			database = (IResposity)Commons.ObjectUtils.InitObject(Type.GetType(contextclass), new object[]{ connstrionstring} );

			if( !(database as DBResposityBase).InititalDB())
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_FAILED));

		}
		
		//private string GetContextClass( string connection)
		//{
		//    KeyValuePair<string, Mapping> mapping = MappingContext.FirstOrDefault( item => connection.ToLower().Contains(item.Value.Provider.ToLower()));
		//    return mapping.Value == null? string.Empty : mapping.Value.ClassName;
		//}
		public void AddLog(Log log)
		{
			lock (locker)
			{
				try
				{

					database.Insert<Log>(log);
					database.Save();
				}
				catch { }
			}
		}

		#region interface functions
		public void Save()
		{
			lock(locker)
			{
				database.Save();
			}
		}

		public void CancelChanges()
		{
			lock(locker)
			{
			database.CancelChanges();
			}
		}
		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
			lock(locker)
				{
			return database.Query<T>(filter);
		}
		}
		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			lock(locker)
			{
			return database.FirstOrDefault(predicate);
		}
		}

		public void Insert<T>(T entity) where T : class
		{
			lock(locker)
			{
			database.Insert<T>(entity);
		}
		}

		public void Update<T>(T entity) where T : class
		{
			lock(locker)
			{
			database.Update<T>(entity);
		}
		}

		public void Delete<T>(T entity) where T : class
		{	lock(locker)
		{
			database.Delete<T>(entity);
		}
		}

		public int ExecuteCommand(string sqlcmd, params object[] prams)
		{
			lock(locker)
			{
			return database.ExecuteCommand( sqlcmd, prams);
		}
		}

		public dynamic FirstOrDefault(Type dbsetType, Func<dynamic, bool> predicate, bool CasttoRequestType = false)
		{
			lock(locker)
			{
			return database.FirstOrDefault(dbsetType, predicate, CasttoRequestType);
		}
		}

		public void Insert(Type dbsetType, dynamic item)
		{
			lock(locker)
			{
			database.Insert(dbsetType, item);
		}
		}

		public IEnumerable<dynamic> Query(Type dbsetType, Func<dynamic, bool> filter = null)
		{
			lock(locker)
			{
			return database.Query( dbsetType, filter);
		}
		}
		public void Dispose()
		{
			database = null;
		}
		public string SqlTableName( Type dbsetType)
		{
			TableAttribute tatt = dbsetType.GetCustomAttributes(typeof(TableAttribute),true).FirstOrDefault() as TableAttribute;
			if( tatt == null )
				return dbsetType.Name;
			return tatt.Name;
		}
		public string SqlTableName<T>() where T : class
		{
			return SqlTableName( typeof(T));
		}

		public void Refresh<T>() where T : class
		{
			lock(locker)
			{
				database.Refresh<T>();
			}
		}
		public void RefreshAll()
		{
			lock(locker)
			{
				database.RefreshAll();
			}
		}
		public void DeleteData(string table)
		{
			SqlCeResposity sqlce = (database as SqlCeResposity);
			if( sqlce == null)
				return;
			lock(locker){
			sqlce.DeleteData(table);
		}
		}
		#endregion
	}
}
