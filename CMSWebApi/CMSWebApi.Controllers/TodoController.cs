using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.APIFilters.ErrorHandler;
using CMSWebApi.BusinessServices.Todo;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class TodoController : ApiControllerBase<ITodoService, TodoBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetTodo(int todoId = 0)
		{
			return ExecuteBusiness<List<ToDo>>(() => BusinessService.GetTodo(usercontext.ID, todoId).ToList());
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage InsertTodo(ToDo todo)
		{
			return ExecuteBusiness<ToDo>(() => BusinessService.InsertTodo(usercontext.ID, todo));
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage EditTodo(ToDo todo)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.EditTodo(usercontext.ID, todo);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteTodo([FromBody]int todoId)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.DeleteTodo(usercontext.ID, todoId);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}
	}
}
