using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase.Model
{
	public interface IRepository
	{
		void Save();
		IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class;
		T SingleOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;
		void Insert<T>(T entity) where T : class;
		void Update<T>(T entity) where T : class;
		void Delete<T>(T entity) where T : class;
	}

	public class EntityFrameworkRepository : IRepository
	{
		private readonly DbContext dataContext;
		public EntityFrameworkRepository(DbContext dataContext)
		{
			this.dataContext = dataContext;
		}
		public void Save()
		{
			dataContext.SaveChanges();
		}
		public IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
		{
			IQueryable<T> query = dataContext.Set<T>();
			if (filter != null)
				query = query.Where(filter);
			return query;
		}
		public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			return dataContext.Set<T>().SingleOrDefault(predicate);
		}
		public void Insert<T>(T entity) where T : class
		{
			dataContext.Set<T>().Add(entity);
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
	}
	
	

}
