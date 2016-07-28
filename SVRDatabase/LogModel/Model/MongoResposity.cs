using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using MongoDB.Bson.Serialization;
using System.ComponentModel.DataAnnotations;


namespace SVRDatabase.Model
{
	internal class MongoResposity : DBResposityBase,IResposity
	{
		MongoDatabase DBContext;
		public MongoResposity(string dbconnnectionstring):base( dbconnnectionstring)
		{
		}

		#region Public methods
		public override bool InititalDB()
		{
			try
			{
				var _databaseName = MongoUrl.Create(base.DB_connection).DatabaseName;
				var client = new MongoClient(base.DB_connection);
				var server = client.GetServer();
				DBContext = server.GetDatabase(_databaseName);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void Save()
		{
			
		}

		public void CancelChanges()
		{
		}

		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
            //MongoCollection<T> moncollection = DBContext.GetCollection<T>(GetTableName(typeof(T))).AsQueryable();
            IQueryable<T> moncollection;
            if (filter == null)
            {
                moncollection = DBContext.GetCollection<T>(GetTableName(typeof(T))).AsQueryable();
            }
            else 
            {
                moncollection = DBContext.GetCollection<T>(GetTableName(typeof(T))).AsQueryable().Where(filter);
            }
			return moncollection;
		}

		public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate)
		{
			return FirstOrDefault(EntityType, predicate, false);
		}

		public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate, bool CasttoRequestType = false)
		{
			IEnumerable<dynamic> dyn_collection = GetDBSet(EntityType);
			dynamic found = dyn_collection.FirstOrDefault(predicate);
			return found == null ? found : (CasttoRequestType ? Convert.ChangeType(found, EntityType) : found);
		}

		public ParallelQuery<dynamic> Query(Type EntityType, Func<dynamic, bool> filter = null)
		{
			ParallelQuery<dynamic> pquery = GetDBSetParallel(EntityType);
            IEnumerable<dynamic> dyn_collection = GetDBSet(EntityType);
             IEnumerable<dynamic>  ret = dyn_collection.Where( filter);
			return filter == null ? pquery : pquery.Where<dynamic>(filter);
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			MongoCollection collection = GetCreateTableData(typeof(T));
			return collection.AsQueryable<T>().FirstOrDefault(predicate);
		}

		public void Insert(Type EntityType, object item)
		{
			string collectionName = GetTableName(EntityType);
			MongoCollection collection = GetCreateTableData(EntityType);
			
			collection.Insert(EntityType, item);
		}

		public void Insert<T>(T entity) where T : class
		{
			Insert(typeof(T), entity);
		}

		public void Update<T>(T entity) where T : class
		{
			Update(typeof(T), entity);
		}

		public void Update(Type EntityType, object entity)
		{
			var query = QueryEQByKey(entity);
			if (query != null)
			{
				Delete(entity.GetType(), entity);
			}

			Insert(EntityType, entity);
		}

		public void Delete(Type EntityType, object entity)
		{
			var query = QueryEQByKey(entity);

			MongoCollection mcollection = GetCreateTableData(EntityType);
			mcollection.Remove(query);
		}

		public void Delete<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			throw new Exception("The Function doesn't implement yet.");

		}
		public void Delete<T>(T entity) where T : class
		{
			Delete(typeof(T), entity);
		}

		public int ExecuteCommand(string sqlcmd, object[] prams)
		{
			throw new Exception("The Function doesn't implement yet.");

		}

		#endregion

		

		#region Private Methods

		private bool CreateTable(Type EntityType)
		{
			return CreateTable(GetTableName(EntityType));
		}

		private bool CreateTable(string TableName)
		{
			if (IsValidTable(TableName))
				return true;
			
			DBContext.CreateCollection(TableName);
			return IsValidTable(TableName);
		}

		private bool IsValidTable(string tablename)
		{
			return DBContext.CollectionExists(tablename);
		}

		private bool IsValidTable(Type EntityType)
		{
			return IsValidTable(GetTableName(EntityType));
		}

		private MongoCollection GetCreateTableData(Type EntityType, string tablename = null)
		{
			string TableName = string.IsNullOrEmpty(tablename) ? GetTableName(EntityType) : tablename;
			if (!CreateTable(TableName))
				return null;

			MongoCollection collection = DBContext.GetCollection(EntityType, TableName);

			return collection;
		}

		private string GetTableName(Type EntityType)
		{
			Attribute att = EntityType.GetCustomAttribute(typeof(TableAttribute));
			return att == null || string.IsNullOrEmpty((att as TableAttribute).Name) ? EntityType.Name : (att as TableAttribute).Name;
		}

		private IEnumerable<dynamic> GetDBSet(Type EntityType)
		{
			string collection_Name = GetTableName(EntityType);
			MongoCollection mcollection = GetCreateTableData(EntityType);
			MongoCursor cursor = mcollection.FindAllAs(EntityType);
			return cursor.Cast<dynamic>();
		}

		ParallelQuery<dynamic> GetDBSetParallel(Type EntityType)
		{
			return GetDBSet(EntityType).AsParallel<dynamic>();
		}

		private IMongoQuery QueryEQByKey(object Entity)
		{
			Type type = Entity.GetType();
			PropertyInfo[] pinfos = type.GetProperties();
			IEnumerable<PropertyInfo> ie_pinfo = pinfos.Where(item => item.GetCustomAttribute(typeof(KeyAttribute)) != null);
			IEnumerable<IMongoQuery> ie_iqueries = ie_pinfo.Select(item => BuildEQQuery(item.Name, item.GetValue(Entity)));
			return MongoDB.Driver.Builders.Query.And(ie_iqueries);
		}

		private IMongoQuery BuildEQQuery(string fieldName, object value)
		{
			return MongoDB.Driver.Builders.Query.EQ(fieldName, BsonValue.Create(value));
		}

		private IMongoQuery BuildQuery(object Entity)
		{
			Type type = Entity.GetType();
			PropertyInfo[] pinfos = type.GetProperties();

			BsonValue bvalue = pinfos[0].GetValue(Entity) as BsonValue;
			IEnumerable<IMongoQuery> queries = pinfos.Select(item => BuildEQQuery(item.Name, item.GetValue(Entity)));

			return MongoDB.Driver.Builders.Query.And(queries);
		}
		
		#endregion

	}
}
