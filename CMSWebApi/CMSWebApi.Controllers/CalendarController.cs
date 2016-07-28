using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.BusinessServices.Configuration;
using System.Net.Http;
using CMSWebApi.Utils;
using CMSWebApi.DataModels;
using System.Net;
using CMSWebApi.APIFilters;

namespace CMSWebApi.Controllers
{
	[RoutePrefix("CalendarEvents")]
	[WebApiAuthenication]
	public class CalendarController : ApiControllerBase<ICalendarService, CalendarBusinessService>
	{
		[HttpGet]
		public HttpResponseMessage CalendarEvent()
		{
			return ExecuteBusiness<IQueryable<CalendarEvent>>(() =>
			{
				IQueryable<CalendarEvent> calEvts = base.BusinessService.GetCalendarEvent( base.usercontext);
				return calEvts;
			});
		}

		[HttpGet]
		public HttpResponseMessage GetCalendarList()
		{
			//return ExecuteBusiness<IEnumerable<CalendarEventSimple>>(() =>
			//	{
			//		IEnumerable<CalendarEventSimple> calst = base.BusinessService.GetCalendarList(base.usercontext);
			//		return calst;
			//	});
			return base.ExecuteBusiness<IQueryable<CalendarEventSimple>>(() =>
			{
				IQueryable<CalendarEventSimple> ret = BusinessService.GetCalendarList(usercontext);
				return ret;
			});
		}

		[HttpPost]
		public HttpResponseMessage CalendarEvent(CalendarEvent model)
		{
			if (model == null)
				return UnAuthorizeMessage();

			DateTime tNow = DateTime.Now;
			if (model.CreatedDate == DateTime.MinValue)
			{
				model.CreatedDate = tNow;
			}
			if (model.EndDate == DateTime.MinValue)
			{
				model.EndDate = tNow;
			}
			if (model.StartDate == DateTime.MinValue)
			{
				model.StartDate = tNow;
			}
			if (model.NormalizeTrigger == DateTime.MinValue)
			{
				model.NormalizeTrigger = tNow;
			}

			return ExecuteBusiness<TransactionalModel<CalendarEvent>>(() =>
			{
				TransactionalModel<CalendarEvent> retData = base.BusinessService.UpdateCalendar(model);
				return retData;
				//return Request.CreateResponse(HttpStatusCode.OK, retData);
			});
		}

	}
}
