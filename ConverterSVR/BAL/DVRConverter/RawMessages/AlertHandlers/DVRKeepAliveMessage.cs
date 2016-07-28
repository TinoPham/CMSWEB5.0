using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using PACDMModel.Model;
namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{
	internal class DVRKeepAliveMessage : AlertHandlerBase
	{
		private static readonly Lazy<DVRKeepAliveMessage> Lazy = new Lazy<DVRKeepAliveMessage>(() => new DVRKeepAliveMessage());

		public static DVRKeepAliveMessage Instance
		{
			get { return Lazy.Value; }
		}
		public override bool HandleFixAlert<Tmodel>(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, Tmodel model)
		{
			if( model is tAlertEvent)
			{
				tAlertEvent alt = model as tAlertEvent;
				if( isLastOccurAlert(config, alt))
				{
					AddLastOccurAndDetailAlert(db, config, alt);
					db.Save();
				}
				return true;
			}

			tAlertEventLast last_Occur = base.LastAlert(db, dvr.KDVR, config.ALertType.Value);
			if( last_Occur == null || dvr == null || !dvr.tDVRChannels.Any())
				return true;
			tDVRChannels r_chan = dvr.tDVRChannels.FirstOrDefault(ch => ch.Status.HasValue && ch.Status.Value == Consts.DVR_CHANNEL_STATUS_RECORD);
			if( r_chan == null)
				return true;
				//delete all detail alerts that relate to not record because we cannot know alertID to fix it
			IEnumerable<int> dtail = AlertDetail(db, last_Occur.KAlertEvent).Select( it=> it.KAlertEvent);
			foreach( int kalt in dtail)
				db.DeleteWhere<tAlertEventDetail>( it => it.KAlertEvent == kalt);
			DeleteLastOccur(db, last_Occur);
			db.Save();
			return base.HandleFixAlert<Tmodel>(db, dvr, config, model);
		}
	}
}
