using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using PACDMModel.Model;


namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{
	internal class AlertEvent : AlertHandlerBase
	{
		private static readonly Lazy<AlertEvent> Lazy = new Lazy<AlertEvent>(() => new AlertEvent());

		public static AlertEvent Instance
		{
			get { return Lazy.Value; }
		}

		public override bool HandleFixAlert<Tmodel>(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, Tmodel model)
		{
			if( config == null || !config.ALertType.HasValue || !config.ALertFixType.HasValue)
				return false;

			if(!( model is tAlertEvent))
				return base.HandleFixAlert<Tmodel>(db, dvr, config, model);

			tAlertEvent alert = model as tAlertEvent;
			
			if( alert.KAlertEvent <= 0 || alert.KDVR <= 0)
				return false;
			//add new or update if alert is one of lat occur list
			tAlertEventLast last_occur;
			if( isLastOccurAlert(config, alert) )
			{
				AddLastOccurAndDetailAlert(db, config, alert);
				db.Save();
				return true;
			}

			
			last_occur = LastAlert(db, alert.KDVR.Value, config.ALertType.Value);
			if( last_occur == null)// cannot fix alert if last occur not happen
				return false;

			IEnumerable<tAlertEventDetail> details = AlertDetail( db, last_occur.KAlertEvent);
			if( !details.Any())
				return true;
			tAlertEventDetail dtail = details.First();
			dtail.FixEventID = alert.KAlertEvent;
			//dtail.Time = alert.Time;
			//dtail.TimeZone = alert.TimeZone;
			db.Update<tAlertEventDetail>(dtail);
			DeleteLastOccur(db, last_occur);//delete last occur when alert already fix
			db.Save();
			return true;

		}

	}
}
