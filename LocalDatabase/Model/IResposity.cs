using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConverterDB.Model
{
	
	public interface IResposity
	{
		void Save();
		void CancelChanges();
		IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class;
		T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;
		void Insert<T>(T entity) where T : class;
		void Update<T>(T entity) where T : class;
		void Delete<T>(T entity) where T : class;
		int ExecuteCommand(string sqlcmd,params object[] prams);
		dynamic FirstOrDefault(Type dbsetType, Func<dynamic, bool> predicate, bool CasttoRequestType = false);
		void Insert(Type dbsetType, dynamic item);
		IEnumerable<dynamic> Query(Type dbsetType, Func<dynamic, bool> filter = null);
		void RefreshAll();
		void Refresh<T>() where T : class;
	}

	internal class DBResposityBase
	{
		protected readonly string DB_connection = string.Empty;
		public DBResposityBase( string connectionstring)
		{
			DB_connection = connectionstring;
		}
		public virtual bool InititalDB()
		{
			return true;
		}
	}
}
