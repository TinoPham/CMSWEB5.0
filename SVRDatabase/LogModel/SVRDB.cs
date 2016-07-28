using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Commons.Resources;
using SVRDatabase.Model;


namespace SVRDatabase
{
	class SVRDB : IResposity, IDisposable
	{
		private class Mapping
		{
			public string Provider { get;set;}
			public string ClassName { get; set;}
		}
		private class MappingContext: Commons.SingletonStringTypeMappingBase<MappingContext, Mapping>
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
		//	get 
		//	{
		//		if( _MappingContext == null)
		//		{
		//			_MappingContext = new Dictionary<string, Mapping>();
		//			_MappingContext.Add(MongoDB_Provider, new Mapping { Provider = "mongodb://", ClassName = typeof(MongoResposity).FullName });
		//			_MappingContext.Add(SqlCe_Provider, new Mapping { Provider = "Data Source", ClassName = typeof(SqlCeResposity).FullName });
		//		}
		//		return _MappingContext;
		//	}
		//}



		const string MongoDB_Provider = "mongodb";
        const string SqlCe_Provider = "System.Data.SqlServerCe";
		IResposity database;

		private string GetConnectionString(string NameOrConnectionString)
		{
			ConnectionStringSettings con_setting = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().FirstOrDefault(item => string.Compare(item.Name, NameOrConnectionString, true) == 0);
			return con_setting == null ? NameOrConnectionString : con_setting.ConnectionString;
		}

		public SVRDB(string NameOrConnectionString)
		{
			string connstrionstring = GetConnectionString( NameOrConnectionString);
			if( string.IsNullOrEmpty(connstrionstring))
			{
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString( Commons.ERROR_CODE.DB_CONNECTION_STRING_NULL) );
			}
			string contextclass = MappingContext.Instance.GetContextClass(connstrionstring);
            //string contextclass = typeof(MongoResposity).FullName;
			if( string.IsNullOrEmpty(contextclass))
			{
				string all_context = String.Join(", ", MappingContext.Instance.Allkeys());
				Commons.Utils.ThrowExceptionMessage( string.Format( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_INVALID), all_context) );
			}

			database = (IResposity)Commons.ObjectUtils.InitObject(Type.GetType(contextclass), new object [] { NameOrConnectionString });

			if( !(database as DBResposityBase).InititalDB())
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_FAILED));

		}
		
		
		#region interface functions
		public void Save()
		{
			database.Save();
		}

		public void CancelChanges()
		{
			database.CancelChanges();
		}


		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
			return database.Query<T>(filter);
		}
		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return database.FirstOrDefault<T>(predicate);
		}

		public void Insert<T>(T entity) where T : class
		{
			database.Insert<T>(entity);
		}

		public void Update<T>(T entity) where T : class
		{
			database.Update<T>(entity);
		}

		public void Delete<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			database.Delete<T>(predicate);
		}
		public void Delete<T>(T entity) where T : class
		{
			database.Delete<T>(entity);
		}

		public int ExecuteCommand(string sqlcmd, object[] prams)
		{
			return database.ExecuteCommand( sqlcmd, prams);
		}

		public dynamic FirstOrDefault(Type dbsetType, Func<dynamic, bool> predicate, bool CasttoRequestType = false)
		{
			return database.FirstOrDefault(dbsetType, predicate, CasttoRequestType);
		}

		public void Insert(Type dbsetType, dynamic item)
		{
			database.Insert(dbsetType, item);
		}

		public ParallelQuery<dynamic> Query(Type dbsetType, Func<dynamic, bool> filter = null)
		{
			return database.Query( dbsetType, filter);
		}
		public void Dispose()
		{
			database = null;
		}
		#endregion
	}
}
