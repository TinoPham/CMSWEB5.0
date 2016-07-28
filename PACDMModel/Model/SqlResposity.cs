using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Data.Entity.Core.Objects;

namespace PACDMModel.Model
{
	internal class SqlResposity : DBResposityBase, IResposity
	{
		//const string DBName = "LocalDatabase.sdf";
		const string SqlCe_Provider = "System.Data.SqlServer";
		//const string SqlCe_ConnectionString = @"Data Source= {0};Persist Security Info=False;";

		readonly string dbPath = string.Empty;

		DbContextTransaction CTXTransaction = null;

		private DbContextTransaction CurrentTransaction
		{
			get{
				if( CTXTransaction ==  null)
					CTXTransaction = dataContext.Database.BeginTransaction();
				return CTXTransaction;

			}
			
		}
		
		 
		private PACDMModel.Model.PACDMDB dataContext;

		public SqlResposity(string connectionString)
			: base(connectionString)
		{
				dataContext = new PACDMDB();
				dataContext.Configuration.ProxyCreationEnabled = false;//don't remove this line
				dataContext.Configuration.ValidateOnSaveEnabled = false;
				dataContext.Configuration.LazyLoadingEnabled = false;
		}


		public void DeleteItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2[] children) where T : class  where T2 : class
		{
			if( children == null || !children.Any())
				return;
			dataContext.Set<T>().Attach(parent);
			IObjectContextAdapter objctx = dataContext as IObjectContextAdapter;
			foreach (T2 child in children)
			{
				dataContext.Set<T2>().Attach(child);
				objctx.ObjectContext.ObjectStateManager.ChangeRelationshipState(parent, child, expression, EntityState.Deleted);
			}
		}

		public void AddItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2 [] children)
			where T : class
			where T2 : class
		{
			if (children == null || !children.Any())
				return;
			dataContext.Set<T>().Attach(parent);
			IObjectContextAdapter objctx = dataContext as IObjectContextAdapter;

			ObjectStateManager objectStateManager = objctx.ObjectContext.ObjectStateManager;
			ObjectStateEntry stateEntry = null;
			bool isPresent = false;
			foreach (T2 child in children)
			{
				 isPresent = objctx.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(child, out stateEntry);
				 if (isPresent && stateEntry != null && stateEntry.State == EntityState.Added)
					objctx.ObjectContext.ObjectStateManager.ChangeObjectState(child, EntityState.Added);
				else
				dataContext.Set<T2>().Attach(child);
				
				objctx.ObjectContext.ObjectStateManager.ChangeRelationshipState(parent, child, expression, EntityState.Added);
			}
		}


		public int Save()
		{
			try
			{
				//return await dataContext.SaveChangesAsync();

				return dataContext.SaveChanges();
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
			catch(Exception)
			{
				DiscardChanges();
			}
			return -1;
			
		}
		
		internal IEnumerable<DbEntityEntry> GetChanges()
		{
			IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
			IEnumerable<DbEntityEntry> changes = entities.Where( it => it.State != EntityState.Unchanged);
			return changes;
		}
		private void DiscardChanges()
		{
			 IEnumerable<DbEntityEntry> entities = dataContext.ChangeTracker.Entries();
			foreach( var it in entities)
			{
				if( it.State == EntityState.Unchanged)
					continue;
				try
				{
				it.State = EntityState.Unchanged;
				}
				catch(Exception ex)
				{
				}
			}
			 //entities.Where( item => item.State  != EntityState.Unchanged).ToList().ForEach( item => item.State = EntityState.Unchanged);

		}

		public int DeleteWhere<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			var delList = predicate == null ? dataContext.Set<T>() :dataContext.Set<T>().Where(predicate);
			DbEntityEntry entityEntry = null;
			foreach (var entity in delList)
			{
				entityEntry = dataContext.Entry(entity);
				entityEntry.State = EntityState.Deleted;
			}
			return delList.Count();
		}

		public void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> predicate)
			where TEntity : class
			where TProperty : class
		{
			DbEntityEntry<TEntity> entityEntry = dataContext.Entry<TEntity>(entity);
			DbReferenceEntry reference = entityEntry.Reference<TProperty>(predicate);
			if (reference != null && !reference.IsLoaded)
				reference.Load();

		}

		public void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, ICollection<TProperty>>> predicate)
			where TEntity : class
			where TProperty : class
		{
			DbEntityEntry<TEntity> entityEntry = dataContext.Entry<TEntity>(entity);
			DbCollectionEntry reference = entityEntry.Collection<TProperty> (predicate);
			if(reference != null && !reference.IsLoaded)
				reference.Load();
			
		}

		public IQueryable<U> Query<T, U>(Expression<Func<T, U>> columns, Expression<Func<T, bool>> filter = null, string [] includes = null)
			where T : class
			where U : class
		{
			if (includes != null && includes.Count() > 0)
			{
				var query = dataContext.Set<T>().Include(includes.First());
				foreach (var include in includes.Skip(1))
					query = query.Include(include);

				return filter == null ? query.Select<T,U>(columns) : query.Where<T>(filter).Select<T,U>(columns);
			}

			return filter == null ? dataContext.Set<T>().Select<T, U>(columns) : dataContext.Set<T>().Where<T>(filter).Select<T, U>(columns);
		}

		public IQueryable<T> QueryNoTrack<T>(Expression<Func<T, bool>> filter = null, string [] includes = null) where T : class
		{
			if (includes != null && includes.Count() > 0)
			{
				var query = dataContext.Set<T>().AsNoTracking<T>().Include(includes.First());
				foreach (var include in includes.Skip(1))
					query = query.Include(include);

				return filter == null ? query.AsQueryable<T>() : query.Where<T>(filter);
			}

			return filter == null ? dataContext.Set<T>().AsNoTracking<T>().AsQueryable<T>() : dataContext.Set<T>().AsNoTracking<T>().Where<T>(filter);
		}
		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null, string[] includes = null) where T : class
		{
			if (includes != null && includes.Count() > 0)
			{
				var query = dataContext.Set<T>().Include(includes.First());
				foreach (var include in includes.Skip(1))
					query = query.Include(include);

				return filter == null? query.AsQueryable<T>() : query.Where<T>(filter);
			}

			return filter == null?  dataContext.Set<T>().AsQueryable<T>() :  dataContext.Set<T>().Where<T>(filter);
		}

		public void Insert(Type EntityType, object item)
		{
			DbSet dset = dataContext.Set(EntityType);
			dset.Add(item);
		}

		private List<dynamic> GetDBset(Type EntityType)
		{
			DbSet dset = dataContext.Set(EntityType);
			
			return dset.ToListAsync().Result;
			
		}

		public dynamic FirstOrDefault(Type EntityType, Func<dynamic, bool> predicate, bool CasttoRequestType = false) 
		{
			 
			dynamic found = GetDBset(EntityType).AsParallel().FirstOrDefault(predicate);
			return found == null ? found : (CasttoRequestType ? Convert.ChangeType(found, EntityType) : found);
		}

		public ParallelQuery<dynamic> Query(Type EntityType, Func<dynamic, bool> filter = null)
		{
			List<dynamic> lstobj = GetDBset(EntityType);

			return filter == null ? lstobj.AsParallel() : lstobj.AsParallel().Where<dynamic>(filter);
			
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string[] includes = null) where T : class
		{
			return Query<T>(predicate, includes).FirstOrDefault();

		}

		
		public IQueryable<T> Filter<T>(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50, string[] includes = null) where T : class
		{
			int skipCount = index * size;
			IQueryable<T> _resetSet = Query<T>(predicate, includes);

			////HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
			//if (includes != null && includes.Count() > 0)
			//{
			//	var query = dataContext.Set<T>().Include(includes.First());
			//	foreach (var include in includes.Skip(1))
			//		query = query.Include(include);
			//	_resetSet = predicate != null ? query.Where<T>(predicate).AsQueryable() : query.AsQueryable();
			//}
			//else
			//{
			//	_resetSet = predicate != null ? dataContext.Set<T>().Where<T>(predicate).AsQueryable() : dataContext.Set<T>().AsQueryable();
			//}

			_resetSet = skipCount == 0 ? _resetSet.Take(size) : _resetSet.Skip(skipCount).Take(size);
			total = _resetSet.Count();
			return _resetSet;
		}

		public bool Contains<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return dataContext.Set<T>().Count<T>(predicate) > 0;
		}

		public T Insert<T>(T entity) where T : class
		{
			return dataContext.Set<T>().Add(entity);
		}

		public void Update<T>(T entity) where T : class
		{
			DbEntityEntry entityEntry = dataContext.Entry(entity);
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
		
		public int ExecuteCommand(string sqlcmd, object[] prams)
		{
			try
			{
				dataContext.Database.Connection.Open();
				return dataContext.Database.ExecuteSqlCommand(sqlcmd, prams);
			}
			catch(Exception){}
			finally{ dataContext.Database.Connection.Close();}
			return 0;
		}

		/// <summary>
		/// IEnumerable<Products> products = 
		//_unitOfWork.ProductRepository.ExecWithStoreProcedure( "spGetProducts @bigCategoryId",new SqlParameter("bigCategoryId", SqlDbType.BigInt) { Value = categoryId } );


		public Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, List<SqlParameter> parameters) where T : class
		{
			
			return ExecWithStoreProcedureAsync<T>(query, parameters == null? null : parameters.ToArray());
		}

		public Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, params object [] parameters) where T : class
		{
			return dataContext.Database.SqlQuery<T>(query, parameters).ToListAsync();
		}


		public IEnumerable<T> ExecWithStoreProcedure<T>(string query, params object [] parameters) where T : class
		{
			return dataContext.Database.SqlQuery<T>(query, parameters).ToList();
		}
		public IEnumerable<T> ExecWithStoreProcedure<T>(string query, List<SqlParameter> parameters) where T : class
		{
			 return ExecWithStoreProcedure<T>(query, parameters == null? null : parameters.ToArray());
		}

		public DataSet ExecuteQuery(string query, CommandType type, IEnumerable<SqlParameter> parameters)
		{
			var connectionString = dataContext.Database.Connection.ConnectionString;
			var ds = new DataSet();
			var parms = parameters ?? new List<SqlParameter>();
			using (var conn = new SqlConnection(connectionString))
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = query;
					cmd.CommandType = type;
					cmd.Parameters.AddRange(parms.ToArray());

					using (var adapter = new SqlDataAdapter(cmd))
					{
						adapter.Fill(ds);
					}
				}
			}

			return ds;
		}
		
		public bool InsertWithTransaction<T>(T entity) where T:class
		{
			try
			{
				InitTransaction();
			//dataContext.Database.UseTransaction(CurrentTransaction.UnderlyingTransaction);
				dataContext.Set<T>().Add(entity);
				return Save() != -1 ;
				
				
			}
			catch(Exception)
			{
				return false;
			}
		}
		public bool UpdateWithTransaction<T>(T entity) where T : class
		{
			try
			{
				InitTransaction();
				//dataContext.Database.UseTransaction(CurrentTransaction.UnderlyingTransaction);
				DbEntityEntry entityEntry = dataContext.Entry(entity);
				if (entityEntry.State == EntityState.Detached)
				{
					dataContext.Set<T>().Attach(entity);
					entityEntry.State = EntityState.Modified;
					return Save() != -1;
				}
				return true;
			}
			catch(System.Exception)
			{
				 return false;
			}
		}

		public void CommitTransaction()
		{
			if( CTXTransaction == null)
				return;
			CTXTransaction.Commit();
			CTXTransaction.Dispose();
			CTXTransaction = null;
		}

		public void RollBackTransaction()
		{
			if( CTXTransaction == null)
				return; 
			CTXTransaction.Rollback();
			CTXTransaction.Dispose();
			CTXTransaction = null;
		}

		private void InitTransaction()
		{
			if (CTXTransaction == null)
				CTXTransaction = dataContext.Database.BeginTransaction();
		}

		public void Dispose()
		{
			if (dataContext != null)
				dataContext.Dispose();
		}
	}

	public partial class PACDMDB : DbContext
    {

		public PACDMDB(string NameorConnectionString, bool AutoDetectChangesEnabled, bool LazyLoadingEnabled)
			: base(NameorConnectionString)
		{
			Configuration.ProxyCreationEnabled = false;//don't remove this line
			Configuration.ValidateOnSaveEnabled = false;
			Configuration.LazyLoadingEnabled = LazyLoadingEnabled;
			Configuration.AutoDetectChangesEnabled = AutoDetectChangesEnabled;


			//Configuration =  new DbContextConfiguration();
			//{
			//	AutoDetectChangesEnabled = AutoDetectChangesEnabled,
			//	LazyLoadingEnabled = LazyLoadingEnabled,
			//	ProxyCreationEnabled = false
			//};
		}
	}

}
