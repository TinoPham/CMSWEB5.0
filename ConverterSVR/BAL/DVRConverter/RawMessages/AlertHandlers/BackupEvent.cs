using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{

	internal class BackupEvent : AlertHandlerBase
	{
		private static readonly Lazy<BackupEvent> Lazy = new Lazy<BackupEvent>(() => new BackupEvent());
		
		public static BackupEvent Instance
		{
			get { return Lazy.Value; }
		}
		public override bool HandleFixAlert<Tmodel>(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, Tmodel model)
		{
			if (config == null || !config.ALertType.HasValue || !config.ALertFixType.HasValue)
				return false;

			if (!(model is tAlertEvent))
				return base.HandleFixAlert<Tmodel>(db, dvr, config, model);

			tAlertEvent alert = model as tAlertEvent;

			if (alert.KAlertEvent <= 0 || alert.KDVR <= 0)
				return false;
			//add new or update if alert is one of lat occur list
			
			if (isLastOccurAlert(config, alert))
			{
				AddLastOccurAndDetailAlert(db, config, alert);
				db.Save();
				return true;
			}

			IQueryable<tAlertEventLast> LastOccurs = LastOccurALertEvents(db, alert.KDVR.Value);
			if(!LastOccurs.Any())
				return true;
			
			IQueryable<tAlertEventDetail> details;
			bool issave = false;
			foreach(tAlertEventLast last_occur in LastOccurs)
			{
				details = AlertDetail(db, last_occur.KAlertEvent);
				if(!details.Any())
				{
					DeleteLastOccur(db, last_occur);//delete last occur when alert already fix
					issave = true;
					continue;
				}
				tAlertEventDetail dtail = details.First();
				dtail.FixEventID = alert.KAlertEvent;
				db.Update<tAlertEventDetail>(dtail);
				DeleteLastOccur(db, last_occur);//delete last occur when alert already fix
				issave = true;
			}
			if(issave)
				db.Save();
			return true;
		}
		private IQueryable<tAlertEventLast> LastOccurALertEvents(IResposity pacdb, int kdvr)
		{
			if( pacdb == null)
				return Enumerable.Empty<tAlertEventLast>().AsQueryable();

			return pacdb.Query<tAlertEventLast>(it => it.KDVR == kdvr && (it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Backup_Started || it.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Insufficient_Disk_Space_Backup));
		}
		private IEnumerable<tAlertEventDetail> AlertDetails(IResposity pacdb, IEnumerable<int> kalertEvents)
		{
			if( pacdb == null || kalertEvents == null || !kalertEvents.Any())
				return Enumerable.Empty<tAlertEventDetail>();
			IQueryable<tAlertEventDetail> details = pacdb.Query<tAlertEventDetail>(it => kalertEvents.Contains<int>(it.KAlertEvent));
			return details.ToList();
		}

	}
}
