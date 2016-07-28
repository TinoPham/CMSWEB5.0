using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PACDMModel.Model
{
	
	public interface IResposity
	{
		int Save();

		void DeleteItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2[] children) where T : class  where T2 : class;
		void AddItemRelation<T, T2>(T parent, Expression<Func<T, object>> expression, params T2 [] children) where T : class  where T2 : class	;

		IQueryable<T> Query<T>(Expression<Func<T, bool>> filter = null, string[] includes = null) where T : class;

		IQueryable<T> QueryNoTrack<T>(Expression<Func<T, bool>> filter = null, string [] includes = null) where T : class;
		
		IQueryable<U> Query<T, U>(Expression<Func<T, U>> columns, Expression<Func<T, bool>> filter = null, string [] includes = null)
			where T : class
			where U : class;

		T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string[] includes = null) where T : class;

		T Insert<T>(T entity) where T : class;

		void Update<T>(T entity) where T : class;

		void Delete<T>(T entity) where T : class;

		int ExecuteCommand(string sqlcmd, object[] prams);

		dynamic FirstOrDefault(Type dbsetType, Func<dynamic, bool> predicate, bool CasttoRequestType = false);

		void Insert(Type dbsetType, dynamic item);

		ParallelQuery<dynamic> Query(Type dbsetType, Func<dynamic, bool> filter = null);

		int DeleteWhere<T>(Expression<Func<T, bool>> predicate) where T : class;

		void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, ICollection<TProperty>>> predicate)
			where TEntity : class
			where TProperty : class;

		void Include<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> predicate)
			where TEntity : class
			where TProperty : class;

		/// <summary>
		/// Gets objects from database with filting and paging.
		/// </summary>
		/// <typeparam name="Key"></typeparam>
		/// <param name="filter">Specified a filter</param>
		/// <param name="total">Returns the total records count of the filter.</param>
		/// <param name="index">Specified the page index.</param>
		/// <param name="size">Specified the page size</param>
		/// <returns></returns>
		IQueryable<T> Filter<T>(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50, string[] includes = null) where T : class;

		/// <summary>
		/// Gets the object(s) is exists in database by specified filter.
		/// </summary>
		/// <param name="predicate">Specified the filter expression</param>
		/// <returns></returns>
		bool Contains<T>(Expression<Func<T, bool>> predicate) where T : class;

		IEnumerable<T> ExecWithStoreProcedure<T>(string query, params object [] parameters) where T:class;
		IEnumerable<T> ExecWithStoreProcedure<T>(string query, List<SqlParameter> parameters) where T : class;

		Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, params object [] parameters) where T : class;
		Task<List<T>> ExecWithStoreProcedureAsync<T>(string query, List<SqlParameter> parameters) where T : class;
		DataSet ExecuteQuery(string query, CommandType type, IEnumerable<SqlParameter> parameters);
		//{
		//	return Table.Where<T>(exp).Select<T, U>(columns);
		//}
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
