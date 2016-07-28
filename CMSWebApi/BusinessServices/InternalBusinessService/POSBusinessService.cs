using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Extensions.Linq;
using LinqKit;
using PACDMModel.Model;
using System.Data.Entity;


namespace CMSWebApi.BusinessServices.InternalBusinessService
{
	internal class POSBusinessService // : BusinessServices.BusinessBase<DataServices.POS.TransactModelService>
	{
		protected DataServices.ServiceBase ServiceBase{ get ;set;}
		ServiceInterfaces.IServiceBase<tbl_POS_Transact> DataService;
		Expression<Func<tbl_POS_Transact, bool>> expression_And;
		Expression<Func<tbl_POS_Transact, bool>> expression_Or;
		Expression<Func<tbl_POS_Transact, bool>> expression_Root;
		IQueryable<tbl_POS_Transact>Transaction;

		public POSBusinessService(DataServices.ServiceBase _ServiceBase, Nullable<DateTime> DateFrom, Nullable<DateTime> DateTo, List<int> PACIds)
		{
			DataService = new DataServices.POS.TransactModelService(_ServiceBase);
			
			ServiceBase = _ServiceBase;
			expression_Root = PredicateBuilder.True<tbl_POS_Transact>();
			
			if(PACIds != null && PACIds.Any())
				expression_Root = expression_Root.And(it => PACIds.Contains(it.T_PACID));

			if (DateFrom.HasValue && DateTo.HasValue)
			{
				expression_Root = expression_Root.And(it => it.TransDate >= DateFrom.Value);
				expression_Root = expression_Root.And(it => it.TransDate <= DateTo.Value);
			}


			Transaction =  DataService.Gets(null);
		}

		internal void Include<Property>(Expression<Func<tbl_POS_Transact, ICollection<Property>>> name)
		{
			Transaction = Transaction.Include<tbl_POS_Transact, ICollection<Property>>(name);
		}
		internal void Include(string path)
		{
			Transaction = Transaction.Include(path);
		}

		internal void AndRoot(Expression<Func<tbl_POS_Transact, bool>> condition)
		{
			expression_Root = expression_Root.And( condition);
		}
		internal void Or( Expression<Func<tbl_POS_Transact, bool>> condition)
		{
			if (condition == null)
				return;
			if( expression_Or == null)
				expression_Or = PredicateBuilder.False<tbl_POS_Transact>();

			expression_Or = expression_Or.Or(condition);
		}

		internal void And(Expression<Func<tbl_POS_Transact, bool>> condition)
		{
			if( condition == null)
				return;

			if(expression_And == null)
				expression_And = PredicateBuilder.True<tbl_POS_Transact>(); 

			expression_And  = expression_And.And( condition);
		}

		internal Expression<Func<tbl_POS_Transact, bool>> Compare<T>(string op, T value, Expression<Func<tbl_POS_Transact, T>> selector)
		{
			Expression<Func<tbl_POS_Transact, bool>> _comapre = null;

			var parameter = selector.Parameters [0];
			var left = selector.Body;
			var right = Expression.Constant(value, typeof(T));
			BinaryExpression binaryExpression = null;
			switch (op)
			{
				case "=":
					binaryExpression = Expression.Equal(left, right);
					break;
				case ">":
					binaryExpression = Expression.GreaterThan(left, right);
					break;
				case "<":
					binaryExpression = Expression.LessThan(left, right);
					break;
				case ">=":
					binaryExpression = Expression.LessThanOrEqual(left, right);
					break;
				case "<=":
					binaryExpression = Expression.LessThanOrEqual(left, right);
					break;
			}
			_comapre = Expression.Lambda<Func<tbl_POS_Transact, bool>>(binaryExpression, parameter);
			return _comapre;
		}

		internal IQueryable<T> Result<T>( Expression<Func<tbl_POS_Transact,T>> selector)
		{
			IQueryable<tbl_POS_Transact> result = null;
			if( expression_And != null || expression_Or != null)
			{
				var combine = PredicateBuilder.True<tbl_POS_Transact>();
				if(expression_And != null)
					combine = combine.And(expression_And);
				if( expression_Or != null)
					combine = combine.Or( expression_Or);

				result = Transaction.Where(expression_Root).AsExpandable().Where(combine.Expand());
			}
			else
				result = Transaction.AsExpandable().Where(expression_Root.Expand());
			
			return selector == null? result as IQueryable<T> : result.Select<tbl_POS_Transact,T>( selector);
		}

	}
}
