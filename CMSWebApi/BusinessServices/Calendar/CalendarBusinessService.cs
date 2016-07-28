using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using CMSWebApi.APIFilters;

namespace CMSWebApi.BusinessServices.Configuration
{
	public partial class CalendarBusinessService : BusinessBase<ICalendarService>
	{
		private Action<CalendarEvent, tCMSWeb_CalendarEvents> Act_UpdateDBCalendarEvent = (calModel, calEvent) =>
		{
																										calEvent.ECalName = calModel.Name;
																										calEvent.ECColor = calModel.Color;
																										calEvent.ECCreatedBy = calModel.CreatedBy;
																										calEvent.ECCreatedDate = calModel.CreatedDate;
																										calEvent.ECDescription = calModel.Description;
																										calEvent.ECEndDate = calModel.EndDate;
																										calEvent.ECStartDate = calModel.StartDate;

																										calEvent.RemindBefore = calModel.RemindBefore;
																										calEvent.ScheduleType = calModel.ScheduleType;
																										calEvent.RemindID = 0;
																										calEvent.EventTrigger = calModel.EventTrigger;
																										calEvent.RelatedFunction = calModel.RelatedFunction;
																										calEvent.NormalizeAllSite = calModel.NormalizeAllSite;
																										calEvent.NormalizeTrigger = calModel.NormalizeTrigger;
																									};
		private Expression<Func<tCMSWeb_CalendarEvents, CalendarEvent>> Selector_CalendarEvent = calEvent => new CalendarEvent{
																										ID = calEvent.ECalID,
																										Name = calEvent.ECalName,
																										Color = (calEvent.ECColor == null) ? 0 : (int)calEvent.ECColor,
																										CreatedBy = calEvent.ECCreatedBy,
																										CreatedDate = (calEvent.ECCreatedDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECCreatedDate,
																										Description = calEvent.ECDescription,
																										StartDate = (calEvent.ECStartDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECStartDate,
																										EndDate = (calEvent.ECEndDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECEndDate,
																										EventTrigger = (calEvent.EventTrigger == null) ? false : (bool)calEvent.EventTrigger,
																										RemindBefore = (calEvent.RemindBefore == null) ? (byte)0 : (byte)calEvent.RemindBefore,
																										ScheduleType = (calEvent.ScheduleType == null) ? (short)0 : (short)calEvent.ScheduleType,
																										RemindID = (calEvent.RemindID == null) ? 0 : (int)calEvent.RemindID,
																										RelatedFunction = (calEvent.RelatedFunction == null) ? (short)0 : (short)calEvent.RelatedFunction,
																										NormalizeAllSite = (calEvent.NormalizeAllSite == null) ? false : (bool)calEvent.NormalizeAllSite,
																										NormalizeTrigger = (calEvent.NormalizeTrigger == null) ? DateTime.MinValue : (DateTime)calEvent.NormalizeTrigger,
																										//RecipientIDs = calEvent.tCMSWebRecipients.Select( rcpt => rcpt.RecipientID), //DataService.GetCalendarRecipients(calEvent.ECalID).ToList(),
																										SiteIDs = calEvent.tCMSWebSites.Select(  calsite => calsite.siteKey) //DataService.GetCalendarSites(calEvent.ECalID).ToList()

		};
		
		//public ConfigurationBusinessService(CultureInfo culture, IConfigurationService dbservice) : base(dbservice, culture) { }

		public IQueryable<CalendarEvent> GetCalendarEvent(UserContext user)
		{
			IQueryable<CalendarEvent> evts = DataService.CalendarEvents<CalendarEvent>(user.ParentID, Selector_CalendarEvent, null);
			return evts;
			//List<CalendarEvent> modelEvts = new List<CalendarEvent>();//Enumerable.Empty<CalendarEvent>().AsQueryable();
			//foreach (tCMSWeb_CalendarEvents evt in evts)
			//{
			//	CalendarEvent cal = new CalendarEvent();
			//	ImportCalendar(ref cal, evt);
			//	modelEvts.Add(cal);
			//}

			//return modelEvts.AsQueryable();
		}

		public IQueryable<CalendarEventSimple> GetCalendarList(UserContext userctx)
		{
			return DataService.CalendarEvents<CalendarEventSimple>(userctx.ParentID, t => new CalendarEventSimple
			{
				ID = t.ECalID,
				Name = t.ECalName,
				EndDate = t.ECEndDate,
				StartDate = t.ECStartDate
			}, null);

		}

		//public CalendarEvent GetCalendarEvent(int id)
			//{
		//	IEnumerable<string>childs = ServiceBase.ChildProperties( typeof(tCMSWeb_CalendarEvents));
		//	CalendarEvent calEvent = DataService.GetCalendarEvent<CalendarEvent>(id, Selector_CalendarEvent, childs == null || childs.Count() == 0? null : childs.ToArray()); //DataService.GetCalendarEvent(id);
		//	return calEvent;
		//	//if (calEvent != null)
		//	//{
		//	//	CalendarEvent evt = new CalendarEvent();
		//	//	ImportCalendar(ref evt, calEvent);
		//	//	return evt;
		//	//}
		//	//return null;
		//}
		public TransactionalModel<CalendarEvent> UpdateCalendar(CalendarEvent calModel)
		{
			tCMSWeb_CalendarEvents calEvent = null;
			if (calModel.ID > 0)
			{
				calEvent = DataService.GetCalendarEvent<tCMSWeb_CalendarEvents>(calModel.ID, item => item, null);
				//UpdateCalendar(calModel, ref calEvent);
				Act_UpdateDBCalendarEvent(calModel, calEvent);
				DataService.UpdateCalendarRecipients(calEvent, calModel.RecipientIDs);
				DataService.UpdateCalendarSites(calEvent, calModel.SiteIDs);
				DataService.EditCalendarEvent(calEvent);
			}
			else
			{
				calEvent = new tCMSWeb_CalendarEvents();
				//UpdateCalendar(calModel, ref calEvent);
				Act_UpdateDBCalendarEvent(calModel, calEvent);
				DataService.UpdateCalendarRecipients(calEvent, calModel.RecipientIDs);
				DataService.UpdateCalendarSites(calEvent, calModel.SiteIDs);
				DataService.AddCalendarEvent(calEvent);
			}
			calModel.ID = calEvent.ECalID;
			TransactionalModel<CalendarEvent> ReturnModel = new TransactionalModel<CalendarEvent>();
			ReturnModel.Data = calModel;
			return ReturnModel;
		}

		//private void UpdateCalendar(CalendarEvent calModel, ref tCMSWeb_CalendarEvents calEvent)
		//{
		//	if (calEvent == null || calModel == null)
		//		return;
		//	calEvent.ECalName = calModel.Name;
		//	calEvent.ECColor = calModel.Color;
		//	calEvent.ECCreatedBy = calModel.CreatedBy;
		//	calEvent.ECCreatedDate = calModel.CreatedDate;
		//	calEvent.ECDescription = calModel.Description;
		//	calEvent.ECEndDate = calModel.EndDate;
		//	calEvent.ECStartDate = calModel.StartDate;

		//	calEvent.RemindBefore = calModel.RemindBefore;
		//	calEvent.ScheduleType = calModel.ScheduleType;
		//	calEvent.RemindID = 0;
		//	calEvent.EventTrigger = calModel.EventTrigger;
		//	calEvent.RelatedFunction = calModel.RelatedFunction;
		//	calEvent.NormalizeAllSite = calModel.NormalizeAllSite;
		//	calEvent.NormalizeTrigger = calModel.NormalizeTrigger;

		//	DataService.UpdateCalendarRecipients(calEvent, calModel.RecipientIDs);
		//	DataService.UpdateCalendarSites(calEvent, calModel.SiteIDs);
		//}
		
		//private void ImportCalendar(ref CalendarEvent calModel, tCMSWeb_CalendarEvents calEvent)
		//{
		//	if (calEvent == null || calModel == null)
		//		return;
		//	calModel.ID = calEvent.ECalID;
		//	calModel.Name = calEvent.ECalName;
		//	calModel.Color = (calEvent.ECColor == null) ? 0 : (int)calEvent.ECColor;
		//	calModel.CreatedBy = calEvent.ECCreatedBy;
		//	calModel.CreatedDate = (calEvent.ECCreatedDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECCreatedDate;
		//	calModel.Description = calEvent.ECDescription;
		//	calModel.StartDate = (calEvent.ECStartDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECStartDate;
		//	calModel.EndDate = (calEvent.ECEndDate == null) ? DateTime.MinValue : (DateTime)calEvent.ECEndDate;
		//	calModel.EventTrigger = (calEvent.EventTrigger == null) ? false : (bool)calEvent.EventTrigger;

		//	calModel.RemindBefore = (calEvent.RemindBefore == null) ? (byte)0 : (byte)calEvent.RemindBefore;
		//	calModel.ScheduleType = (calEvent.ScheduleType == null) ? (short)0 : (short)calEvent.ScheduleType;
		//	calModel.RemindID = (calEvent.RemindID == null) ? 0 : (int)calEvent.RemindID;
		//	calModel.RelatedFunction = (calEvent.RelatedFunction == null) ? (short)0 : (short)calEvent.RelatedFunction;
		//	calModel.NormalizeAllSite = (calEvent.NormalizeAllSite == null) ? false : (bool)calEvent.NormalizeAllSite;
		//	calModel.NormalizeTrigger = (calEvent.NormalizeTrigger == null) ? DateTime.MinValue : (DateTime)calEvent.NormalizeTrigger;

		//	calModel.RecipientIDs = DataService.GetCalendarRecipients(calEvent.ECalID).ToList();
		//	calModel.SiteIDs = DataService.GetCalendarSites(calEvent.ECalID).ToList();
		//}
	}
}
