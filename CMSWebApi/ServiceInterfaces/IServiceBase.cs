using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IServiceBase<T> where T:class
	{
		IQueryable<T> Gets(Expression<Func<T, bool>> filter, string [] includes = null);

		T Get(Expression<Func<T, bool>> filter, string [] includes = null);

		IQueryable<Tout> Gets<Tout>(Expression<Func<T, bool>> filter, string [] includes = null, Expression<Func<T, Tout>> selector = null);

		Tout Get<Tout>(Expression<Func<T, bool>> filter, string [] includes = null, Expression<Func<T, Tout>> selector = null);

		T Add(T value, bool save = true);

		T Edit(T value, bool save = true);

		bool Delete(T value, bool save = true);

		bool DeleteWhere(Expression<Func<T, bool>> filter, bool save = true);
	}
}
