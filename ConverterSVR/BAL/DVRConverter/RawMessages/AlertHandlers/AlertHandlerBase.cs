using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{
	

	internal abstract class AlertHandlerBase
	{
		static readonly string ns_handler = typeof(AlertHandlerBase).Namespace;
		public static AlertHandlerBase AlertHandler(string name)
		{
			if( string.IsNullOrEmpty( name))
				return null;
			Type type = Type.GetType( string.Format("{0}.{1}", ns_handler, name));
			return type == null ? null : AlertHandler(type);
		}
		static AlertHandlerBase AlertHandler(Type handlertype, string InstanceProperty = "Instance")
		{
			PropertyInfo singleton = handlertype.GetProperty(InstanceProperty, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
			return singleton == null ? null : singleton.GetValue(null) as AlertHandlerBase;
		}

		public virtual bool HandleFixAlert<Tmodel>(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, Tmodel model) where Tmodel : class 
		{
			return true;
		}

		protected tAlertEventLast LastAlert(IResposity db, int kdvr, byte alertype)
		{
			return db.FirstOrDefault<tAlertEventLast>( it => it.KDVR == kdvr && it.KAlertType == alertype);
		}

		protected IQueryable<tAlertEventDetail>AlertDetail( IResposity db, int KalertEvent)
		{
			return db.Query<tAlertEventDetail>(it => it.KAlertEvent == KalertEvent && it.FixEventID.HasValue == false);
		}

		protected IQueryable<int> AlertEventDetail(IResposity db, int KalertEvent)
		{
			return db.Query<tAlertEventDetail>(it => it.KAlertEvent == KalertEvent && it.FixEventID.HasValue == false).Select( it => it.KAlertEvent);
		}
	
		protected void LoadChannels(IResposity db, tDVRAddressBook dvr)
		{
			if( dvr.tDVRChannels == null || !dvr.tDVRChannels.Any())
				db.Include<tDVRAddressBook, tDVRChannels>( dvr, it => it.tDVRChannels);
		}
	
		protected void AddLastOccur( IResposity db, tAlertEvent alert)
		{
			db.Insert<tAlertEventLast>(
			new tAlertEventLast{
				KDVR = alert.KDVR.Value,
				KAlertType = alert.KAlertType,
				KAlertEvent = alert.KAlertEvent,
				Time = alert.Time,
				TimeZone = alert.TimeZone.Value
			}
			);
		}

		protected void UpdateLastOccur(IResposity db, tAlertEvent alert, ref tAlertEventLast last)
		{
			last.KAlertEvent = alert.KAlertEvent;
			last.Time = alert.Time;
			last.TimeZone = alert.TimeZone.Value;
			db.Update<tAlertEventLast>(last);
		}
		
		protected void DeleteLastOccur(IResposity db, tAlertEventLast last)
		{
			db.DeleteWhere<tAlertEventLast>( it => it.KDVR == last.KDVR && it.KAlertType == last.KAlertType);
		}
		
		protected bool isLastOccurAlert(AlertFixConfig config, tAlertEvent alert)
		{
			return config.ALertType.Value == alert.KAlertType;
			//tAlertEventLast last_occur = null;
			//if (alert.KAlertType == config.ALertType.Value)
			//{
			//	last_occur = LastAlert(db, alert.KDVR.Value, config.ALertType.Value);
			//	if (last_occur == null)
			//		AddLastOccur(db, alert);
			//	else
			//		UpdateLastOccur(db, alert, ref last_occur);
			//	if( issave)
			//		db.Save();
			//	return true;
			//}
			//return false;
		}
	
		protected void AddLastOccurAndDetailAlert(IResposity db, AlertFixConfig config, tAlertEvent alert)
		{
			tAlertEventLast last_occur;
			last_occur = LastAlert(db, alert.KDVR.Value, config.ALertType.Value);
			if( last_occur != null)
			{
				if (last_occur.KAlertEvent == alert.KAlertEvent)
					return;

				IEnumerable<int> dtails = AlertEventDetail(db, last_occur.KAlertEvent);
				if (dtails.Any())
					DeleteAlertdetail(db, dtails.First());
				DeleteLastOccur(db, last_occur);
				
			}
			AddLastOccur(db, alert);
			AddAlertDetail(db, alert);

		}
	
		protected void DeleteAlertdetail(IResposity db, int kalertevent, int kchannel = 0)
		{
			
			db.DeleteWhere<tAlertEventDetail>( it => it.KAlertEvent == kalertevent && it.KChannel == kchannel);
		}

		protected void AddAlertDetail( IResposity db, tAlertEvent alert, int kchannel = 0)
		{
			db.Insert<tAlertEventDetail>(
			new tAlertEventDetail
			{
				KAlertEvent = alert.KAlertEvent,
				KChannel = kchannel,
				Time = alert.Time,
				TimeZone = alert.TimeZone
			}
			);
		}

	}
}
