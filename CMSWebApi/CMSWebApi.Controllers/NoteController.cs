using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.APIFilters.ErrorHandler;
using CMSWebApi.BusinessServices.Notes;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class NoteController : ApiControllerBase<INoteService, NoteBusinessService>
	{	
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetNote(int noteId = 0)
		{
			return ExecuteBusiness<List<Note>>(() => BusinessService.GetNote(usercontext.ID, noteId).ToList());
		}

		[HttpPost]
		[ValidateModel]
		public HttpResponseMessage InsertNote(Note note)
		{
			return ExecuteBusiness<Note>(() => BusinessService.InsertNote(usercontext.ID, note));
		}

		[HttpPost]
		[ValidateModel]
		[ActivityLog]
		public HttpResponseMessage EditNote(Note note)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.EditNote(usercontext.ID, note);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteNote([FromBody]int noteId)
		{
			return ExecuteBusiness(() =>
			{
				BusinessService.DeleteNote(usercontext.ID, noteId);
				return Request.CreateResponse(HttpStatusCode.OK);
			});
		}
	}
}
