using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using System.Text.RegularExpressions;
using Extensions.Linq;
using AppSettings;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{
	internal class VideoEvent: AlertHandlerBase
	{
		private static readonly Lazy<VideoEvent> Lazy = new Lazy<VideoEvent>(() => new VideoEvent());

		public static VideoEvent Instance
		{
			get { return Lazy.Value; }
		}

		const string Channel_Regex_Pattern = @"(?<Channel>\d+)[,]?";
		const string STR_Channel = "Channel";
		readonly Regex rx = new Regex(Channel_Regex_Pattern, RegexOptions.CultureInvariant);

		public bool HandleFixByAlertAll(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, tAlertEvent alert, Dictionary<int,int> ChannelMap)
		{
			if (config == null || !config.ALertType.HasValue || !config.ALertFixType.HasValue)
				return false;

			LoadChannels(db, dvr);
			
			tAlertEventLast loccur = base.LastAlert(db, alert.KDVR.Value, config.ALertType.Value);

			if (alert.KAlertType == config.ALertType.Value)// when alert is video loss
			{
				VideoLoss(db, alert, dvr, loccur, ChannelMap.Values);
				db.Save();
				return true;
			}

			IEnumerable<tAlertEventDetail> dtail = base.AlertDetail(db, loccur.KAlertEvent);

			if (ReturntoNormal(db, alert, dtail, dvr, ChannelMap.Values, loccur))
				db.Save();
			return true;
		}

		public override bool HandleFixAlert<Tmodel>(IResposity db, tDVRAddressBook dvr, AlertFixConfig config, Tmodel model)
		{
			if (config == null || !config.ALertType.HasValue || !config.ALertFixType.HasValue)
				return false;

			if (!(model is tAlertEvent))
				return base.HandleFixAlert<Tmodel>(db, dvr, config, model);
			LoadChannels(db, dvr);
			tAlertEvent alt = model as tAlertEvent;
			IEnumerable<int>channels = GetChannels( alt.Description);

			IEnumerable<int> kchannels = dvr.tDVRChannels.Join(channels, kch => kch.ChannelNo + 1, ch => ch, (kch, ch) => kch.KChannel);

			tAlertEventLast loccur = base.LastAlert(db, alt.KDVR.Value, config.ALertType.Value);

			if( alt.KAlertType == config.ALertType.Value)// when alert is video loss
			{
				VideoLoss(db, alt, dvr, loccur, kchannels);
				db.Save();
				return true;
			}

			IEnumerable<tAlertEventDetail> dtail = base.AlertDetail(db, loccur.KAlertEvent);

			if (ReturntoNormal(db, alt, dtail, dvr, kchannels, loccur))
				db.Save();
			return true;
		}

		public IEnumerable<int> GetChannels( string strchannels)
		{
			MatchCollection matchs = rx.Matches(strchannels);
			if( matchs.Count == 0)
				return System.Linq.Enumerable.Empty<int>();
			List<int>channels = new List<int>();
			int channel = 0;
			foreach (Match m in matchs)
			{
				if( !m.Success)
					continue;
				if( !Int32.TryParse(m.Groups[STR_Channel].Value, out channel))
					continue;
				channels.Add(channel);
			}

			return channels;
		}

		private bool VideoLoss(IResposity db, tAlertEvent alt, tDVRAddressBook dvr, tAlertEventLast loccur, IEnumerable<int> kchannels)
		{
			if(loccur == null)// first video loss => update 
			{
				AddLastOccur(db, alt);
				AddVideoLossDetail(db, dvr, alt, kchannels);
				return true;
			}
			else
			{
				
				int KalertEventLast = loccur.KAlertEvent;
				
				IEnumerable<tAlertEventDetail> dtail = base.AlertDetail(db, KalertEventLast);
				IEnumerable<int> last_channels = dtail.Select( it => it.KChannel).ToList();
				//delete last dtail
				db.DeleteWhere<tAlertEventDetail>(it => it.KAlertEvent == KalertEventLast);
				//delete last occur
				db.DeleteWhere<tAlertEventLast>(it => it.KAlertEvent == KalertEventLast);
				AddLastOccur(db, alt);
				AddVideoLossDetail(db, dvr, alt, kchannels.Union( last_channels) );
				return true;
				
			}
		}
	
	private bool ReturntoNormal(IResposity db, tAlertEvent alt,IEnumerable<tAlertEventDetail> dtail, tDVRAddressBook dvr, IEnumerable<int> kchannels, tAlertEventLast lastOccur)
	{
		if (!dtail.Any())
			return false;
		IEnumerable<tAlertEventDetail> afixed = dtail.Where(it => kchannels.Any(ch => it.KChannel == ch));
		if(!afixed.Any())
			return false;

		foreach (tAlertEventDetail af in afixed)
		{
			af.FixEventID = alt.KAlertEvent;
			//af.TimeZone = alt.TimeZone;
			//af.Time = alt.Time;
			db.Update<tAlertEventDetail>(af);
		}
		IEnumerable<int> dtail_channels = dtail.Select( it => it.KChannel);
		IEnumerable<int> fix_channels = afixed.Select(it => it.KChannel);
		bool is_valid_loss = dtail_channels.Except( fix_channels).Any();
		if ( ! is_valid_loss && lastOccur != null)
		{
			db.Delete<tAlertEventLast>(lastOccur);
		}

		return true;
	}
		private bool FixbyChannels(IResposity db, tAlertEvent alt,IEnumerable<tAlertEventDetail> dtail, tDVRAddressBook dvr, IEnumerable<int> kchannels)
		{
			if( !dtail.Any())
				return false;

			//fixed channel begin
			IEnumerable<tAlertEventDetail> afixed = dtail.Where(it => !kchannels.Any(ch => it.KChannel == ch));
			foreach (tAlertEventDetail af in afixed)
			{
				af.FixEventID = alt.KAlertEvent;
				af.TimeZone = alt.TimeZone;
				af.Time = alt.Time;
				db.Update<tAlertEventDetail>(af);
			}
			//fixed channel end
			//new channel loss
			IEnumerable<int> current_Channel = dtail.Select( it => it.KAlertEvent);
			IEnumerable<int> newchannels = kchannels.Where(kch => !current_Channel.Any(af => af == kch));
			AddVideoLossDetail(db, dvr, alt, newchannels);
			return true;


		}
	
		private void AddVideoLossDetail(IResposity db, tDVRAddressBook kdvr, tAlertEvent alert, IEnumerable<int> kchannels)
		{
			
			foreach( int kch in kchannels)
			{
				base.AddAlertDetail(db, alert,kch);
			}
		}

		
	}
}
