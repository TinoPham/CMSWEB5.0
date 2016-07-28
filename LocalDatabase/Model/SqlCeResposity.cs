using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ConverterDB.Migrations;
using System.Data.SqlClient;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
namespace ConverterDB.Model
{
	internal class SqlCeResposity : DBResposityBase, IResposity
	{
		//const string DBName = "LocalDatabase.sdf";
		const string SqlCe_Provider = "System.Data.SqlServerCe.4.0";
		//const string SqlCe_ConnectionString = @"Data Source= {0};Persist Security Info=False;";

		readonly string dbPath = string.Empty;

		private DBModel dataContext;

		public SqlCeResposity(string connectionString):base(connectionString)
		{
		}

		public override bool InititalDB()
		{
			bool ret = base.InititalDB();
			try
			{
				dataContext = new DBModel(base.DB_connection);
				dataContext.Configuration.ProxyCreationEnabled = false;
				UpdateDBModel();
			}
			catch(Exception)
			{
				ret = false;
			}
			return ret;
		}
		
		private void UpdateDBModel()
		{
			var database = dataContext.Database;
			var migrationConfiguration = new Configuration();
			migrationConfiguration.TargetDatabase = new DbConnectionInfo(database.Connection.ConnectionString, SqlCe_Provider);
			var migrator = new DbMigrator(migrationConfiguration);
			migrator.Update();
		}

		public void Save()
		{
			try
			{
				//return await dataContext.SaveChangesAsync();

				dataContext.SaveChanges();
				return;
			}
			catch (DbEntityValidationException dbEx)
			{
				//foreach (var validationErrors in dbEx.EntityValidationErrors)
				//{
				//    foreach (var validationError in validationErrors.ValidationErrors)
				//    {
				//        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
				//    }
				//}
				DiscardChanges();
			}
			catch (Exception)
			{
				DiscardChanges();
			}
			return;
		}

		private void DiscardChanges()
		{
			IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
			try
			{
				IEnumerable<DbEntityEntry> items = entities.Where(item => item.State != EntityState.Unchanged);
				foreach (DbEntityEntry entry in items)
				{
					if (entry.State == EntityState.Modified)
						entry.State = EntityState.Unchanged;
					else if (entry.State == EntityState.Added)
						entry.State = EntityState.Detached;
				}
			}
			catch(Exception){}

		}
		public void CancelChanges()
		{
			if( dataContext == null)
				return;
			IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
			foreach(DbEntityEntry enti in entities)
				enti.State = EntityState.Unchanged;

		}

		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
			DbSet<T> dbset = dataContext.Set<T>();
			if (filter != null)
				return dbset.Where(filter);
			return dbset;
			//IQueryable<T> query = dataContext.Set<T>();
			//if (filter != null)
			//	query = query.Where(filter);
			//return query;
		}

		public void Insert(Type EntityType, object item)
		{
			DbSet dset = dataContext.Set(EntityType);
			dset.Add(item);
		}

		//private List<dynamic> GetDBset(Type EntityType)
		//{
		//    DbSet dset = dataContext.Set(EntityType);
		//    Task<List<dynamic>> tlist = dset.ToListAsync();
		//    tlist.Wait();
		//    return tlist.Result;

		//}

		private ParallelQuery<dynamic> GetDBset(Type EntityType)
		{
			DbSet dset = dataContext.Set(EntityType);
			var DbObject = dset.AsParallel();
			//return dset.AsQueryable();
			return DbObject.OfType<dynamic>();
			
		}


		//public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate, bool CasttoRequestType = false) 
		//{
		//    dynamic found = GetDBset(EntityType).AsParallel().FirstOrDefault(predicate);
		//    return found == null ? found : (CasttoRequestType ? Convert.ChangeType(found, EntityType) : found);
		//}

		public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate, bool CasttoRequestType = false) 
		{
			var dbset = GetDBset(EntityType);
			dynamic found = dbset.FirstOrDefault(predicate);

			return found == null ? found : (CasttoRequestType ? Convert.ChangeType(found, EntityType) : found);
		}

		//public IEnumerable<dynamic> Query(Type EntityType, Func<dynamic, bool> filter = null)
		//{
		//    List<dynamic> lstobj = GetDBset(EntityType);
		//    return filter == null ? lstobj.AsEnumerable() : lstobj.Where(filter);//  //lstobj.AsParallel().Where<dynamic>(filter);
		//    //return filter == null ? lstobj.AsParallel() : lstobj.AsParallel().Where<dynamic>(filter);

		//}

		public IEnumerable<dynamic> Query(Type EntityType, Func<dynamic, bool> filter = null)
		{
			var var = GetDBset(EntityType);
			
			return filter == null ? var.AsEnumerable() : var.Where( filter).AsEnumerable();//  //lstobj.AsParallel().Where<dynamic>(filter);
			//return filter == null ? lstobj.AsParallel() : lstobj.AsParallel().Where<dynamic>(filter);
			
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			DbSet<T> dbset = dataContext.Set<T>();
			return dbset == null? null : dbset.FirstOrDefault<T>( predicate);
			//return dataContext.Set<T>().FirstOrDefault(predicate);
		}

		public void Insert<T>(T entity) where T : class
		{
			dataContext.Set<T>().Add(entity);
		}


		System.Data.Entity.Core.EntityKey GetEntityKey<T>(DbContext context, T entity) where T : class
		{
			var oc = ((IObjectContextAdapter)context).ObjectContext;
			ObjectStateEntry ose;
			if (null != entity && oc.ObjectStateManager
									.TryGetObjectStateEntry(entity, out ose))
			{
				return ose.EntityKey;
			}
			return null;
		}

		System.Data.Entity.Core.EntityKey GetEntityKey<T>(DbContext context, DbEntityEntry<T> dbEntityEntry) where T : class
		{
			if (dbEntityEntry != null)
			{
				return GetEntityKey(context, dbEntityEntry.Entity);
			}
			return null;
		}

		public void Update<T>(T entity) where T : class
		{
			DbEntityEntry<T> entityEntry = dataContext.Entry<T>(entity);

			if (entityEntry.State == EntityState.Detached)
			{
				dataContext.Set<T>().Attach(entity);
				entityEntry.State = EntityState.Modified;
			}
		}

		public void Delete<T>(T entity) where T : class
		{
			dataContext.Set<T>().Remove(entity);
			
		}
		public void DeleteData( string tableName)
		{
			string sql = string.Format( "Delete From [{0}]", tableName) ;
			dataContext.Database.ExecuteSqlCommand( sql );
		}
		public int ExecuteCommand(string sqlcmd,params object[] prams)
		{
			try
			{
				//dataContext.Database.Connection.Open();
				return dataContext.Database.ExecuteSqlCommand(sqlcmd, prams);
			}
			catch(Exception){}

			return 0;
		}


		public void Refresh<T>() where T : class
		{
			
			IObjectContextAdapter objctx = dataContext as IObjectContextAdapter;
			ObjectStateManager stateManager = objctx.ObjectContext.ObjectStateManager;

			var allentry = stateManager.GetObjectStateEntries(EntityState.Deleted
													  | EntityState.Modified
													  | EntityState.Unchanged);
			var refresh = allentry.Where(it => it.EntityKey != null && it.EntitySet.ElementType.FullName == typeof(T).FullName).Select( entry => entry.Entity);
			if( refresh != null && refresh.Any())
				objctx.ObjectContext.Refresh(RefreshMode.StoreWins, refresh);
		}
		public void RefreshAll()
		{
			// Get all objects in statemanager with entityKey
			// (context.Refresh will throw an exception otherwise)
			IObjectContextAdapter objctx = dataContext as IObjectContextAdapter;
			ObjectStateManager  stateManager = objctx.ObjectContext.ObjectStateManager;
			var refreshableObjects = (from entry in stateManager.GetObjectStateEntries(
														EntityState.Deleted
													  | EntityState.Modified
													  | EntityState.Unchanged)
									  where entry.EntityKey != null
									  select entry.Entity);

			objctx.ObjectContext.Refresh(RefreshMode.StoreWins, refreshableObjects);
		}
	}
}
