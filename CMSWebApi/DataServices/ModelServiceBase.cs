using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices
{
    public class ModelDataService<T> : ModelServiceBase<T> where T : class
    {
        protected ModelDataService():base(){}

		internal ModelDataService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
        public ModelDataService(ServiceBase svrbase)
			: base(svrbase.DBModel)
		{
		}
    }
	public abstract class ModelServiceBase<T> : ServiceBase, CMSWebApi.ServiceInterfaces.IServiceBase<T> where T:class
	{
		protected ModelServiceBase():base(){}

		public ModelServiceBase(PACDMModel.Model.IResposity dbModel):base( dbModel)
		{
		}

		public IQueryable<T> Gets(Expression<Func<T, bool>> filter, string [] includes = null)
		{
			return DBModel.Query<T>(filter,includes);
		}

		public T Get(Expression<Func<T, bool>> filter, string [] includes = null)
		{
			IQueryable<T> collection = Gets(filter, includes);
			return collection == null? null : collection.FirstOrDefault();
		}

		public IQueryable<Tout> Gets<Tout>(Expression<Func<T, bool>> filter, string [] includes = null, Expression<Func<T, Tout>> selector = null)
		{
			if( selector == null)
				return Gets(filter, includes).Cast<Tout>();
			else
				return Gets(filter, includes).Select(selector);
		}

		public Tout Get<Tout>(Expression<Func<T, bool>> filter, string [] includes = null, Expression<Func<T, Tout>> selector = null)
		{
			IQueryable<Tout> collection = Gets<Tout>( filter, includes, selector);
			return collection == null? default(Tout) : collection.FirstOrDefault();
		}

		public virtual T Add(T value, bool save = true)
		{
			if( value == null)
				return value;
			DBModel.Insert<T>(value);
			if( save)
				DBModel.Save();

			return value;
		}

		public virtual T Edit(T value, bool save = true)
		{
			if( value == null)
				return value;
			DBModel.Update<T>(value);
			if( save)
				DBModel.Save();
			return value;
		}

		public virtual bool Delete(T value, bool save = true)
		{
			if( value == null)
				return false;
			DBModel.Delete<T>(value);
			if( save)
				return DBModel.Save() >= 0;

			return true;
		}

		public virtual bool DeleteWhere(Expression<Func<T, bool>> filter, bool save = true)
		{
			IQueryable<T> items = Gets( filter);
			bool valid = items.Any();
			if(!valid)
				return true;

			while(items.Any())
			{
				Delete( items.First(), false);
			}
			if( save)
				return DBModel.Save() >= 0;

			return valid;
		}
	}
}
