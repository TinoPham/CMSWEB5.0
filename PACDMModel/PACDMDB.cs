using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Commons.Resources;
using PACDMModel.Model;

namespace PACDMModel
{
	public class PACDMDB : IResposity, IDisposable
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
				//base.AddMapping(MongoDB_Provider, new Mapping { Provider = "mongodb://", ClassName = typeof(MongoResposity).FullName });
				base.AddMapping(Sql_Provider, new Mapping { Provider = "Data Source", ClassName = typeof(SqlResposity).FullName });
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

		
		const string MongoDB_Provider = "mongodb";
		const string Sql_Provider = "System.Data.SqlClient";
		IResposity database;

		private string GetConnectionString( string NameOrConnectionString)
		{
			ConnectionStringSettings con_setting = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().FirstOrDefault(item => string.Compare(item.Name, NameOrConnectionString, true) == 0);
			return con_setting == null?  NameOrConnectionString : con_setting.ConnectionString;
		}
		
		public PACDMDB( string NameorConection)
		{

			string connstrionstring = GetConnectionString(NameorConection);

			if (string.IsNullOrEmpty(connstrionstring))
			{
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString( Commons.ERROR_CODE.DB_CONNECTION_STRING_NULL) );
			}
			string contextclass = MappingContext.Instance.GetContextClass(connstrionstring);

			if( string.IsNullOrEmpty(contextclass))
			{
				string all_context = String.Join(", ", MappingContext.Instance.Allkeys());
				Commons.Utils.ThrowExceptionMessage( string.Format( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_INVALID), all_context) );
			}

			database = (IResposity)Commons.ObjectUtils.InitObject(Type.GetType(contextclass), new object[] { connstrionstring });

			if( !(database as DBResposityBase).InititalDB())
				Commons.Utils.ThrowExceptionMessage( ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.DB_CONNECTION_FAILED));

		}
		
		public void Changes()
		{
			var changes = (database as SqlResposity).GetChanges();
		}
		public bool UpdateWithTransaction<T>(T entity) where T : class
		{
			if( database as SqlResposity != null)
				return (database as SqlResposity).UpdateWithTransaction<T>(entity);

			return true;
		}
		
		public bool InsertWithTransaction<T>(T entity) where T:class
		{
			if (database as SqlResposity != null)
				return (database as SqlResposity).InsertWithTransaction<T>(entity);

			return true;
		}
		
		public void CommitTransaction()
		{
		if (database as SqlResposity != null)
			(database as SqlResposity).CommitTransaction();
		}
		
		public void RollBackTransaction()
		{
			if (database as SqlResposity != null)
				(database as SqlResposity).RollBackTransaction();
		}
		
		#region interface functions
		public int Save()
		{
			return database.Save();
		}
		
		public int DeleteWhere<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return database.DeleteWhere<T>(predicate);
		}

		public void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> predicate)
			where TEntity : class
			where TProperty : class
		{
			database.Include<TEntity, TProperty>(entity, predicate);
		}

		public void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, ICollection<TProperty>>> predicate)
			where TEntity : class
			where TProperty : class
		{
			database.Include<TEntity, TProperty>(entity, predicate);
		}

		public IQueryable<T> QueryNoTrack<T>(Expression<Func<T, bool>> filter = null, string [] includes = null) where T : class
		{
			return database.QueryNoTrack<T>( filter, includes);
		}
		public IQueryable<U> Query<T, U>(Expression<Func<T, U>> columns, Expression<Func<T, bool>> filter = null, string [] includes = null)
			where T : class
			where U : class
		{
			return database.Query<T,U>( columns, filter, includes);
		}
		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null, string[] includes = null) where T : class
		{
			return database.Query<T>(filter, includes);
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string[] includes = null) where T : class
		{
			return database.FirstOrDefault(predicate, includes);
		}

		public IQueryable<T> Filter<T>(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50, string[] includes = null) where T : class
		{
			return database.Filter<T>(predicate, out total, index, size, includes);
		}

		public bool Contains<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return database.Contains<T>( predicate);
		}
		public T Insert<T>(T entity) where T : class
		{
			return database.Insert<T>(entity);
		}

		public void Update<T>(T entity) where T : class
		{
			database.Update<T>(entity);
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
	

		public Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, List<SqlParameter> parameters) where T : class
		{
			return database.ExecWithStoreProcedureAsync<T>(query, parameters);
		}

		public Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, params object [] parameters) where T : class
		{
			return database.ExecWithStoreProcedureAsync<T>(query, parameters);
		}

		/// <summary>
		/// IEnumerable<Products> products = 
		//_unitOfWork.ProductRepository.ExecWithStoreProcedure( "spGetProducts @bigCategoryId",new SqlParameter("bigCategoryId", SqlDbType.BigInt) { Value = categoryId } );
		public IEnumerable<T> ExecWithStoreProcedure<T>(string query, params object [] parameters) where T : class
		{
			return database.ExecWithStoreProcedure<T>(query, parameters);
		}
		
		public IEnumerable<T> ExecWithStoreProcedure<T>(string query, List<SqlParameter> parameters) where T : class
		{
			return database.ExecWithStoreProcedure<T>(query, parameters);
		}

		public DataSet ExecuteQuery(string query, CommandType type, IEnumerable<SqlParameter> parameters)
		{
			return database.ExecuteQuery(query, type, parameters);
		}

		public void DeleteItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2 [] children)
			where T : class
			where T2 : class
		{
			database.DeleteItemRelation<T, T2>(parent, expression, children);
		}
		 public void AddItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2 [] children)
			where T : class
			where T2 : class
		{
			database.AddItemRelation<T, T2>(parent, expression, children);
		}
		public void Dispose()
		{
			database = null;
		}

		#endregion
		
	}
}
