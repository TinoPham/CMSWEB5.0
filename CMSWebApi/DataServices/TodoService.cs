using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Linq;

namespace CMSWebApi.DataServices
{
	public class TodoService : ServiceBase, ITodoService
	{
		public TodoService(IResposity model)
			: base(model)
		{
		}

		public TodoService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWeb_ToDoListItem GetTodoById(int userId, int todoId)
		{
			return DBModel.FirstOrDefault<tCMSWeb_ToDoListItem>(t => t.ToDoListItemID == todoId && t.CreatedBy == userId);
		}

		public IQueryable<T> GetTodoList<T>(int userId, Expression<Func<tCMSWeb_ToDoListItem, T>> selector)
		{
			return Query<tCMSWeb_ToDoListItem, T>(t => t.CreatedBy == userId, selector);
		}

		public void InsertTodo(tCMSWeb_ToDoListItem todo)
		{
			DBModel.Insert<tCMSWeb_ToDoListItem>(todo);
		}

		public void EditTodo(tCMSWeb_ToDoListItem todo)
		{
			DBModel.Update<tCMSWeb_ToDoListItem>(todo);
		}

		public void DeleteTodo(int userid, int todoId)
		{
			DBModel.DeleteWhere<tCMSWeb_ToDoListItem>(t => t.ToDoListItemID == todoId && t.CreatedBy == userid);
		}

		public void Save()
		{
			DBModel.Save();
		}
	}
}
