using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;
using Extensions;
using Extensions.Linq;
using AppSettings;
using ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawAlertEventAll : RawDVRConfig<RawAlertEventAllBody>
    {
        public const string STR_EventList = "event_list";
		private const byte ALERTTYPE_VL = 36;

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawAlertEventAllBody msgBody { get; set; }

		public RawAlertEventAll()
		{
		}

		public RawAlertEventAll(string strMsg)
		{
			RawAlertEventAll rw = Commons.ObjectUtils.DeSerialize(typeof(RawAlertEventAll), strMsg) as RawAlertEventAll;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			// Received Envert Data Object and save all to Alert Event in Database
			IOrderedEnumerable<EventData> evt_list = msgBody.lsEvents.Where(item => item.id != ALERTTYPE_VL).OrderBy(it => it.Time);
			List<tAlertEvent> cache_alerts = new List<tAlertEvent>();
			Dictionary<int, int> LostChannels = new Dictionary<int,int>();
			Dictionary<int, int> NormalChannels = new Dictionary<int,int>();
			foreach (EventData evt in evt_list)
			{
				tAlertEvent new_Event = new tAlertEvent();
				new_Event.KAlertType = (byte)evt.id;
				new_Event.Time = DateTime.UtcNow;//ToDateTime(evt.Time);
				new_Event.TimeZone = ToDateTime(evt.StringTime);
				new_Event.Channel = evt.ChannelID;
				new_Event.DVRUser = evt.User;
				new_Event.Description = evt.Infomation;
				new_Event.KDVR = DVRAdressBook.KDVR;
				if (new_Event.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss)
					UpdateVideoChannels(new_Event, LostChannels);
				if (new_Event.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_returned_to_normal)
					UpdateVideoChannels(new_Event, NormalChannels);
				/*var alertInfo = new tAlertEvent
				{
					KAlertType = (byte) evt.id,
					Time = ToDateTime(evt.Time),
					TimeZone = ToDateTime(evt.StringTime),
					Channel = evt.ChannelID,
					DVRUser = evt.User,
					Description = evt.Infomation,
					//DVRGuid = DVRAdressBook.DVRGuid,
					KDVR = DVRAdressBook.KDVR
				};*/
				cache_alerts.Add( new_Event);
				db.Insert<tAlertEvent>(new_Event);
			}
			Commons.ERROR_CODE ret_code = db.Save() == -1? Commons.ERROR_CODE.DB_INSERT_DATA_FAILED : Commons.ERROR_CODE.OK;
			if( ret_code  == Commons.ERROR_CODE.OK)
			{
				CheckAndFixAlert(cache_alerts, db, LostChannels, NormalChannels);
				CMSWebApi.Wrappers.Wrapper.Instance.DVRAlertEvent.Add(cache_alerts);
				//CMSWebApi.Cache.BackgroundTaskManager.Instance.Add(cache_alerts);
				
			}

			return await Task.FromResult<Commons.ERROR_CODE>( ret_code);
		}

		private void UpdateVideoChannels( tAlertEvent alert, Dictionary<int, int> DicChannels)
		{
			if (DVRAdressBook.tDVRChannels == null || !DVRAdressBook.tDVRChannels.Any())
				db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, it => it.tDVRChannels);
			
			IEnumerable<int> channels = VideoEvent.Instance.GetChannels(alert.Description);
			IEnumerable<int> new_channel = channels.Where( it => DicChannels.ContainsKey(it) == false);
			if(!new_channel.Any())
				return;
			var kchannels = DVRAdressBook.tDVRChannels.Join(new_channel, kch => kch.ChannelNo + 1, ch => ch, (kch, ch) => new{ KChannel = kch.KChannel, Channel = ch});
			foreach( var it in kchannels)
				DicChannels.Add( it.Channel, it.KChannel);

		}
		private void CheckAndFixAlert(IEnumerable<tAlertEvent> alerts, IResposity db, Dictionary<int, int> LostChannels, Dictionary<int, int> NormalChannels)
		{
			IEnumerable<tAlertEvent> v_alerts = alerts.Join(AlertFixConfigs.Instance.AlertTypes, alt=> alt.KAlertType, type=> type,(alt, type)=> alt);
			if(!v_alerts.Any())
				return;
			DateTime max_Time_Zone = v_alerts.Max( it => it.TimeZone).Value;
			IEnumerable<tAlertEvent>new_alerts = GetLastAlertWhenDVROffline(db, AlertFixConfigs.Instance.AlertTypes, max_Time_Zone, DVRAdressBook.KDVR);
			Commons.TPEqualityComparer<tAlertEvent, int> icomparer = new Commons.TPEqualityComparer<tAlertEvent,int>( (a,b) => {return a == b;}, alt => alt.KAlertEvent );
			IEnumerable<tAlertEvent> all_alerts = v_alerts.Union(new_alerts, icomparer as IEqualityComparer<tAlertEvent>).ToList();
			IEnumerable<tAlertEvent> group_alerts = all_alerts.GroupBy(it => it.KAlertType).Select( it => it.OrderByDescending( al => al.KAlertEvent).FirstOrDefault());

			//tAlertEvent evt_last = null;
			foreach (tAlertEvent evt_last in group_alerts)
			{
				if( evt_last.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_returned_to_normal)
					VideoEvent.Instance.HandleFixByAlertAll(db, DVRAdressBook, AlertFixConfigs.Instance.GetConfig(evt_last.KAlertType).FirstOrDefault(), evt_last, NormalChannels);
				else if (evt_last.KAlertType == (byte)CMSWebApi.Utils.AlertType.DVR_Video_Loss)
					VideoEvent.Instance.HandleFixByAlertAll(db, DVRAdressBook, AlertFixConfigs.Instance.GetConfig(evt_last.KAlertType).FirstOrDefault(), evt_last, LostChannels);
				else
					CheckAndFixAlert(db, evt_last);
			}
			//foreach (tAlertEvent alt in all_alerts)
			//CheckAndFixAlert( db, alt);

		}
		/// <summary>
		///for DVR Express or DVR reconnect to API the collection of alerts will be sent after dvr start alert 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<tAlertEvent> GetLastAlertWhenDVROffline( IResposity db, IEnumerable<byte> alerttypes, DateTime timezone, int kdvr)
		{
			if(db == null || alerttypes == null || !alerttypes.Any())
				return System.Linq.Enumerable.Empty<tAlertEvent>();

			IQueryable<tAlertEvent> alerts = db.QueryNoTrack<tAlertEvent>(it =>it.KDVR.HasValue && it.TimeZone.HasValue && it.KDVR.Value == kdvr && it.TimeZone.Value >= timezone);
			IQueryable<tAlertEvent> alert_time = alerts.Join( alerttypes, alt=> alt.KAlertType, it=>it, (alt, it)=> alt);
			if( !alert_time.Any())
				return System.Linq.Enumerable.Empty<tAlertEvent>();
			return alert_time.OrderBy(it => it.TimeZone.Value).AsEnumerable<tAlertEvent>();

		}
		private void CheckAndFixAlert(IResposity db, tAlertEvent alert)
		{
			IEnumerable<AlertFixConfig> configs = AlertFixConfigs.Instance.GetConfig(alert.KAlertType);
			AlertHandlerBase handler = null;
			foreach (AlertFixConfig config in configs)
			{
				handler = AlertHandlerBase.AlertHandler(config.Handler);
				if (handler == null)
					continue;
				if (handler.HandleFixAlert<tAlertEvent>(db, base.DVRAdressBook, config, alert))
					break;
			}
		}

    }

    [XmlRoot(MessageDefines.STR_Body)]
    public class RawAlertEventAllBody
    {
        [XmlArray(RawAlertEventAll.STR_EventList)]
        [XmlArrayItem(RawAlertEvent.STR_Event)]
        public List<EventData> lsEvents = new List<EventData>();
    }
}
