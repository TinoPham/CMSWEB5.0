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
using System.Data.SqlClient;
using System.Reflection;
using SVRDatabase.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
namespace SVRDatabase.Model
{
    internal class SqlCeResposity : DBResposityBase,IResposity
	{
		readonly object locker = new object();
		//const string DBName = "LocalDatabase.sdf";
		const string SqlCe_Provider = "System.Data.SqlServerCe.4.0";
		//const string SqlCe_ConnectionString = @"Data Source= {0};Persist Security Info=False;";

		readonly string dbPath = string.Empty;

		private SVRModel dataContext;

        //public SqlCeResposity()
        //{
        //    dataContext = new SVRModel();
        //}

        public SqlCeResposity(string connectionString)
            : base(connectionString)
        {

        }

        public override bool InititalDB()
        {
            bool ret = base.InititalDB();
			lock(locker)
			{
				try
				{
					//System.Data.Entity.Infrastructure.SqlCeConnectionFactory c = new SqlCeConnectionFactory(SqlCe_Provider);
					//System.Data.Common.DbConnection dbcon = c.CreateConnection(base.DB_connection);

					dataContext = new SVRModel(base.DB_connection);
					dataContext.Configuration.ProxyCreationEnabled = false;
					UpdateDBModel();
				}
				catch (Exception)
				{
					ret = false;
				}
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
				dataContext.SaveChanges();
			}
			catch (DbEntityValidationException dbEx)
			{
				foreach (var validationErrors in dbEx.EntityValidationErrors)
				{
					foreach (var validationError in validationErrors.ValidationErrors)
					{
						Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
					}
				}
				DiscardChanges();
			}
			catch (Exception)
			{
				DiscardChanges();
			}

		}

		private void DiscardChanges()
		{
			IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
			entities.Where(item => item.State != EntityState.Unchanged).ToList().ForEach(item => item.State = EntityState.Unchanged);

		}

		public void CancelChanges()
		{
			lock (locker)
			{
				IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
				foreach (DbEntityEntry enti in entities)
					enti.State = EntityState.Unchanged;
			}
		}

		
		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
		IQueryable<T> query  = null;
			lock(locker)
			{
				query = dataContext.Set<T>();
				if (filter != null)
					query = query.Where(filter);
			}
			return query;
		}

		public void Insert(Type EntityType, object item)
		{
			lock(locker)
			{
				DbSet dset = dataContext.Set(EntityType);
				dset.Add(item);
			}
		}

		private List<dynamic> GetDBset(Type EntityType)
		{
			DbSet dset = dataContext.Set(EntityType);

			Task<List<dynamic>> tlist = dset.ToListAsync();
			tlist.Wait();
			return tlist.Result;

		}

		public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate, bool CasttoRequestType = false) 
		{
			dynamic found = null;
			lock(locker)
			{
			 GetDBset(EntityType).AsParallel().FirstOrDefault(predicate);
			}
			return found == null ? found : (CasttoRequestType ? Convert.ChangeType(found, EntityType) : found);
		}

		public ParallelQuery<dynamic> Query(Type EntityType, Func<dynamic, bool> filter = null)
		{
			List<dynamic> lstobj = null;
			lock(locker)
			{
				lstobj = GetDBset(EntityType);
			}

			return filter == null ? lstobj.AsParallel() : lstobj.AsParallel().Where<dynamic>(filter);
			
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			lock(locker)
			{
				return  predicate == null? dataContext.Set<T>().FirstOrDefault() :  dataContext.Set<T>().FirstOrDefault(predicate);
			}
		}

		public void Insert<T>(T entity) where T : class
		{
			lock(locker)
			{
				dataContext.Set<T>().Add(entity);
			}
		}

		public void Update<T>(T entity) where T : class
		{
			DbEntityEntry entityEntry = null;
			lock(locker)
			{
				entityEntry = dataContext.Entry(entity);
				if (entityEntry.State == EntityState.Detached)
				{
					dataContext.Set<T>().Attach(entity);
					entityEntry.State = EntityState.Modified;
				}
			}
		}

		public void Delete<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			var delList = predicate == null ? dataContext.Set<T>() : dataContext.Set<T>().Where(predicate);
			DbEntityEntry entityEntry = null;
			foreach (var entity in delList)
			{
				entityEntry = dataContext.Entry(entity);
				entityEntry.State = EntityState.Deleted;
			}
		}
		public void Delete<T>(T entity) where T : class
		{
            DbEntityEntry entityEntry = null;
			lock(locker)
			{
				entityEntry = dataContext.Entry(entity);
				entityEntry.State = EntityState.Deleted;
				dataContext.Set<T>().Remove(entity);
			}
		}
		
		public int ExecuteCommand(string sqlcmd, object[] prams)
		{
			lock(locker)
			{
				try
				{
					dataContext.Database.Connection.Open();
					return dataContext.Database.ExecuteSqlCommand(sqlcmd, prams);
				}
				catch(Exception){}
				finally{ dataContext.Database.Connection.Close();}
			}

			return 0;
		}
	}
}
