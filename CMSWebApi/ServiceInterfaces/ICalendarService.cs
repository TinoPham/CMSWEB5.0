using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ICalendarService
	{
		IQueryable<Tout> CalendarEvents<Tout>(int Createdby, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> CalendarEvents<Tout>(int Createdby, DateTime sData, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<tCMSWeb_CalendarEvents> GetCalendarEvents();
		IQueryable<tCMSWeb_CalendarEvents> CalendarEvents();
		tCMSWeb_CalendarEvents GetCalendarEvent(int id);
		Tout GetCalendarEvent<Tout>(int id, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<int> GetCalendarRecipients(int id);
		IQueryable<int> GetCalendarSites(int id);
		bool UpdateCalendarRecipients(tCMSWeb_CalendarEvents calEvent, IEnumerable<int> recipIDs);
		bool UpdateCalendarSites(tCMSWeb_CalendarEvents calEvent, IEnumerable<int> siteIDs);
		tCMSWeb_CalendarEvents EditCalendarEvent(tCMSWeb_CalendarEvents calEvent);
		tCMSWeb_CalendarEvents AddCalendarEvent(tCMSWeb_CalendarEvents calEvent);
	}
}
