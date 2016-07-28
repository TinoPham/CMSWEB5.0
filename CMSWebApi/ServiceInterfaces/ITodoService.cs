using System;
using System.Linq;
using System.Linq.Expressions;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ITodoService
	{
		IQueryable<T> GetTodoList<T>(int userId, Expression<Func<tCMSWeb_ToDoListItem, T>> selector);		
		tCMSWeb_ToDoListItem GetTodoById(int userId, int todoId);
		void InsertTodo(tCMSWeb_ToDoListItem todo);
		void EditTodo(tCMSWeb_ToDoListItem todo);
		void DeleteTodo(int userId, int todoId);
		void Save();
	}
}
