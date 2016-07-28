using System;
using System.Linq;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Todo
{
	public class TodoBusinessService : BusinessBase<ITodoService>
	{
		public IQueryable<ToDo> GetTodo(int userId, int id = 0)
		{
			DateTime notesPeriod = Utilities.ToPeriodDate(DateTime.Now, PeriodType.Day, AppSettings.AppSettings.Instance.NotesPeriodDay);
			var todos = DataService.GetTodoList(userId, t => new ToDo()
			{
				Id = t.ToDoListItemID,
				Color = t.ToDoListItemColor,
				CreatedOn = t.CreatedDate,
				Content = t.ToDoListItemContent,
				Font = t.ToDoListItemFont,
				UserId = t.CreatedBy,
				Icon = t.ToDoListItemIcon,
				Recurrence = t.ToDoListItemRecurrence,
				Status = t.ToDoListItemStatus,
				Urgency = t.ToDoListItemUrgency
			});
			return id == 0 ? todos.Where(t => t.CreatedOn >= notesPeriod || t.Status == (int)TodoStatus.Work) : todos.Where(t => t.Id == id);
		}

		public ToDo InsertTodo(int userId, ToDo todo)
		{
			todo.UserId = userId;
			todo.CreatedOn = DateTime.Now;

			var newtodo = new tCMSWeb_ToDoListItem()
			{
				CreatedBy = todo.UserId,
				CreatedDate = todo.CreatedOn,
				ToDoListItemColor = todo.Color,
				ToDoListItemContent = todo.Content,
				ToDoListItemFont = todo.Font,
				ToDoListItemIcon = todo.Icon,
				ToDoListItemRecurrence = todo.Recurrence,
				ToDoListItemStatus = todo.Status,
				ToDoListItemUrgency = todo.Urgency
			};
			DataService.InsertTodo(newtodo);
			DataService.Save();
			todo.Id = newtodo.ToDoListItemID;
			return todo;
		}

		public void EditTodo(int userId, ToDo todo)
		{
			var updateTodo = DataService.GetTodoById(userId, todo.Id);

			if (updateTodo == null)
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
			}

			updateTodo.ToDoListItemColor = todo.Color;
			updateTodo.ToDoListItemContent = todo.Content;
			updateTodo.ToDoListItemFont = todo.Font;
			updateTodo.ToDoListItemIcon = todo.Icon;
			updateTodo.ToDoListItemRecurrence = todo.Recurrence;
			updateTodo.ToDoListItemStatus = todo.Status;
			updateTodo.ToDoListItemUrgency = todo.Urgency;

			DataService.EditTodo(updateTodo);
			DataService.Save();
		}

		public void DeleteTodo(int userId, int todoId)
		{
			DataService.DeleteTodo(userId, todoId);
			DataService.Save();
		}
	}
}
