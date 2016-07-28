using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public partial class CalendarService : ServiceBase, ICalendarService
	{
		public CalendarService(PACDMModel.Model.IResposity model ) : base(model){}

		public CalendarService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<Tout> CalendarEvents<Tout>(int Createdby, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> calEvents = Query<tCMSWeb_CalendarEvents, Tout>(item => item.ECCreatedBy == Createdby, selector, includes);
			return calEvents;
		}

		public IQueryable<Tout> CalendarEvents<Tout>(int Createdby, DateTime sData, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string[] includes) where Tout : class
		{
			IQueryable<Tout> calEvents = Query<tCMSWeb_CalendarEvents, Tout>(item => (item.ECCreatedBy == Createdby && item.ECStartDate <= sData && item.ECEndDate >= sData), selector, includes);
			return calEvents;
		}

		public Tout GetCalendarEvent<Tout>(int id, Expression<Func<tCMSWeb_CalendarEvents, Tout>> selector, string [] includes) where Tout : class
		{
			Tout calEvent = FirstOrDefault<tCMSWeb_CalendarEvents, Tout>(item => item.ECalID == id, selector, includes);
			return calEvent;
		}

		public IQueryable<tCMSWeb_CalendarEvents> GetCalendarEvents()
		{
			IQueryable<tCMSWeb_CalendarEvents> calEvents = DBModel.Query<tCMSWeb_CalendarEvents>();
			return calEvents;
		}

		public IQueryable<tCMSWeb_CalendarEvents> CalendarEvents()
		{
			IQueryable<tCMSWeb_CalendarEvents> calEvents = DBModel.Query<tCMSWeb_CalendarEvents>();
			return calEvents;
		}

		public tCMSWeb_CalendarEvents GetCalendarEvent(int id)
		{
			tCMSWeb_CalendarEvents calEvent = DBModel.FirstOrDefault<tCMSWeb_CalendarEvents>(x => x.ECalID == id);
			return calEvent;
		}
		public IQueryable<int> GetCalendarRecipients(int id)
		{
			//if (id == 0)
				return null;
			//IQueryable<tCMSWeb_CalendarEvent_Recipient> recips = DBModel.Query<tCMSWeb_CalendarEvent_Recipient>(r => r.ECalID == id);
			//IQueryable<int> lsIDs = recips.Select(r => r.RecipientID).Distinct();
			//return lsIDs;
		}
		public IQueryable<int> GetCalendarSites(int id)
		{
			//if (id == 0)
				return null;
			//var sites = DBModel.Query<tCMSWeb_CalendarEvents_Sites>(r => r.ECalID == id);
			//IQueryable<int> lsIDs = sites.Select(r => (int)r.SiteID).Distinct();
			//return lsIDs;
		}
		public bool UpdateCalendarRecipients(tCMSWeb_CalendarEvents calEvent, IEnumerable<int> recipIDs)
		{
			bool bRet = true;
			//if (recipIDs == null)
			//	recipIDs = new List<int>();
			//if (calEvent.tCMSWebRecipients == null)
			//{
			//	calEvent.tCMSWebRecipients = new HashSet<tCMSWebRecipients>();
			//}
			//else
			//{
			//	calEvent.tCMSWebRecipients.Clear();
			//}
			//calEvent.tCMSWebRecipients = DBModel.Query<tCMSWebRecipients>(x => recipIDs.Contains(x.RecipientID), null).ToList();
			/*
			List<int> lsDelete = null;
			List<int> lsInsert = null;

			IQueryable<int> varQuery = GetCalendarRecipients(calEvent.ECalID);
			List<int> dbRecipIDs = (varQuery == null) ? null : varQuery.ToList();
			if (dbRecipIDs == null)
				dbRecipIDs = new List<int>();

			lsDelete = dbRecipIDs.Where(r => !recipIDs.Any(r2 => r2 == r)).ToList();
			lsInsert = recipIDs.Where(r2 => !dbRecipIDs.Any(r => r == r2)).ToList();

			if (calEvent.ECalID > 0 && lsDelete != null && lsDelete.Count > 0)
			{
				DBModel.DeleteWhere<tCMSWebRecipients>(x => x.ECalID == calEvent.ECalID && lsDelete.Contains(x.RecipientID));
			}

			if (lsInsert != null && lsInsert.Count > 0)
			{
				foreach (int recip in lsInsert)
				{
					tCMSWeb_CalendarEvent_Recipient calRecip = new tCMSWeb_CalendarEvent_Recipient();
					calRecip.tCMSWeb_CalendarEvents = calEvent;
					calRecip.RecipientID = recip;
					calEvent.tCMSWeb_CalendarEvent_Recipient.Add(calRecip);
				}
			}*/
			return bRet;
		}
		public bool UpdateCalendarSites(tCMSWeb_CalendarEvents calEvent, IEnumerable<int> siteIDs)
		{
			bool bRet = true;
			if (siteIDs == null)
				siteIDs = new List<int>();

			if (calEvent.tCMSWebSites == null)
			{
				calEvent.tCMSWebSites = new HashSet<tCMSWebSites>();
			}
			else
			{
				calEvent.tCMSWebSites.Clear();
			}
			calEvent.tCMSWebSites = DBModel.Query<tCMSWebSites>(x => siteIDs.Contains(x.siteKey), null).ToList();

			//IQueryable<int> varQuery = GetCalendarSites(calEvent.ECalID);
			//List<int> dbSiteIDs = (varQuery == null) ? null : varQuery.ToList();
			//if (dbSiteIDs == null)
			//	dbSiteIDs = new List<int>();

			//List<int> lsDelete = dbSiteIDs.Where(s => !siteIDs.Any(s2 => s2 == s)).ToList();
			//List<int> lsInsert = siteIDs.Where(s2 => !dbSiteIDs.Any(s => s == s2)).ToList();
			//if (calEvent.ECalID > 0 && lsDelete != null && lsDelete.Count > 0)
			//{
			//	DBModel.DeleteWhere<tCMSWeb_CalendarEvents_Sites>(x => x.ECalID == calEvent.ECalID && lsDelete.Contains((int)x.SiteID));
			//}

			//if (lsInsert != null && lsInsert.Count > 0)
			//{
			//	foreach (int sid in lsInsert)
			//	{
			//		tCMSWeb_CalendarEvents_Sites calSite = new tCMSWeb_CalendarEvents_Sites();
			//		calSite.tCMSWeb_CalendarEvents = calEvent;
			//		calSite.SiteID = sid;
			//		calEvent.tCMSWeb_CalendarEvents_Sites.Add(calSite);
			//	}
			//}
			return bRet;
		}

		public tCMSWeb_CalendarEvents AddCalendarEvent(tCMSWeb_CalendarEvents calEvent)
		{
			//DBModel.Insert<tCMSWeb_CalendarEvents>(calEvent);
			//foreach (tCMSWeb_CalendarEvent_Recipient calRecipt in calEvent.tCMSWeb_CalendarEvent_Recipient)
			//{
			//	DBModel.Insert<tCMSWeb_CalendarEvent_Recipient>(calRecipt);
			//}

			//tCMSWeb_CalendarEvents_Sites calSites = new tCMSWeb_CalendarEvents_Sites();
			//foreach (tCMSWeb_CalendarEvents_Sites calSite in calEvent.tCMSWeb_CalendarEvents_Sites)
			//{
			//	DBModel.Insert<tCMSWeb_CalendarEvents_Sites>(calSite);
			//}

			return DBModel.Save() > 0 ? calEvent : null;
		}
		public tCMSWeb_CalendarEvents EditCalendarEvent(tCMSWeb_CalendarEvents calEvent)
		{
			DBModel.Update<tCMSWeb_CalendarEvents>(calEvent);
			return DBModel.Save() >= 0 ? calEvent : null;
		}
		
	}
}
