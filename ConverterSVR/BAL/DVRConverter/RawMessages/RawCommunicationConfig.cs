using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawCommunicationConfig : RawDVRConfig<RawCommunicationBody>
	{
		#region Parameter
		public const string STR_Networking = "networking";
		public const string STR_NetworkType = "network_type";
		public const string STR_Bandwidth = "band_width";
		public const string STR_ControlPort = "control_port";
		public const string STR_TOPort = "text_overlay_port";
		public const string STR_LiveSearchPort = "live_search_port";
		public const string STR_PACDBPort = "pac_db_port";
		public const string STR_BackupPort1 = "backup_port1";
		public const string STR_BackupPort2 = "backup_port2";
		public const string STR_BackupPort3 = "backup_port3";
		public const string STR_PublicIP = "dvr_public_ip";
		public const string STR_CMSPort = "cms_port";
		public const string STR_MobileSetting = "mobile_setting";
		public const string STR_MobiMainPort = "mobileMainPort";
		public const string STR_MobiVideoPort = "mobileVideoPort";
		public const string STR_MobiKeepEventLog = "mobileKeepEventLog";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawCommunicationBody msgBody { get; set; }
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);

		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
        {
			if (DVRAdressBook == null || msgBody.NetworkData == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (UpdateCommunicationConfigs(DVRAdressBook, msgBody.NetworkData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_NETWORK, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_NETWORK, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			return await base.UpdateToDB();
		}

		private bool UpdateCommunicationConfigs(tDVRAddressBook dvrAdressBook, NetworkInfo networkData)
		{
			bool ret = false;
			if (System.String.Compare(dvrAdressBook.PublicServerIP, networkData.PublicIP, System.StringComparison.OrdinalIgnoreCase) != 0)
			{
				DVRAdressBook.PublicServerIP = networkData.PublicIP;
				db.Update<tDVRAddressBook>(DVRAdressBook);
				ret = true;
			}

			if (dvrAdressBook.KDVRVersion < VersionForBandwidth)
			{
				networkData.Bandwidth = db.Query<tBandwidth>(t => t.KBandwidth == networkData.Bandwidth).Select(t => t.Value).FirstOrDefault();
			}
			tDVRNetwork commInfo = db.FirstOrDefault<tDVRNetwork>( item => item.KDVR == DVRAdressBook.KDVR);
			if (commInfo == null) //insert new
			{
				commInfo = new tDVRNetwork() {KDVR =  dvrAdressBook.KDVR};
				networkData.SetEntity(ref commInfo);
				db.Insert<tDVRNetwork>(commInfo);
				ret = true;
			}
			else //update
			{
				if (!networkData.Equal(commInfo))
				{
					networkData.SetEntity(ref commInfo);
					db.Update<tDVRNetwork>(commInfo);
					ret = true;
				}
			}
			return ret;
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawCommunicationBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public string DVRTime { get; set; }
		public DateTime dtDVRTime
		{
			get
			{
				return DateTime.ParseExact(DVRTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawCommunicationConfig.STR_Networking)]
		public NetworkInfo NetworkData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	public class NetworkInfo : IMessageEntity<tDVRNetwork>
	{
		[XmlElement(RawCommunicationConfig.STR_NetworkType)]
        public Int32 NetworkType { get; set; }

		[XmlElement(RawCommunicationConfig.STR_Bandwidth)]
		public Int32 Bandwidth { get; set; }

		[XmlElement(RawCommunicationConfig.STR_ControlPort)]
		public Int32 ControlPort { get; set; }

		[XmlElement(RawCommunicationConfig.STR_TOPort)]
		public Int32 TOPort { get; set; }

		[XmlElement(RawCommunicationConfig.STR_LiveSearchPort)]
		public Int32 LiveSearchPort { get; set; }

		[XmlElement(RawCommunicationConfig.STR_PACDBPort)]
		public Int32 PACDBPort { get; set; }

		[XmlElement(RawCommunicationConfig.STR_BackupPort1)]
		public Int32 BackupPort1 { get; set; }

		[XmlElement(RawCommunicationConfig.STR_BackupPort2)]
		public Int32 BackupPort2 { get; set; }

		[XmlElement(RawCommunicationConfig.STR_BackupPort3)]
		public Int32 BackupPort3 { get; set; }

		[XmlElement(RawCommunicationConfig.STR_PublicIP)]
		public string PublicIP { get; set; }

		[XmlElement(RawCommunicationConfig.STR_CMSPort)]
		public Int32 CMSPort { get; set; }

		[XmlElement(RawCommunicationConfig.STR_MobileSetting)]
		public MobileSetting MobileSettings { get; set; }

		public bool Equal(tDVRNetwork value)
		{
			bool result = 
						  value.KNetworkType == NetworkType &&
						  value.KBandwidth == Bandwidth &&
						  value.ControlPort == ControlPort &&
						  value.TextOverlayPort == TOPort &&
						  value.LiveSearchPort == LiveSearchPort &&
						  value.PACDBPort == PACDBPort &&
						  value.BackupPort1 == BackupPort1 &&
						  value.BackupPort2 == BackupPort2 &&
						  value.BackupPort3 == BackupPort3 &&
						  value.CMSServerPort == CMSPort;
			if (MobileSettings != null)
			{
				result = result &&
						 value.MobileMainPort == MobileSettings.MobiMainPort &&
						 value.MobileVideoPort == MobileSettings.MobiVideoPort &&
						 value.MobileKeepEventLog == MobileSettings.MobiKeepEventLog;
			}
			return result;
		}

		public void SetEntity(ref tDVRNetwork value)
		{
			if (value == null)
				value = new tDVRNetwork();
			value.KNetworkType = NetworkType;
			value.KBandwidth = Bandwidth;
			value.ControlPort = ControlPort;
			value.TextOverlayPort = TOPort;
			value.LiveSearchPort = LiveSearchPort;
			value.PACDBPort = PACDBPort;
			value.BackupPort1 = BackupPort1;
			value.BackupPort2 = BackupPort2;
			value.BackupPort3 = BackupPort3;
			//nwInfo.
			value.CMSServerPort = CMSPort;
			if (MobileSettings != null)
			{
				value.MobileMainPort = MobileSettings.MobiMainPort;
				value.MobileVideoPort = MobileSettings.MobiVideoPort;
				value.MobileKeepEventLog = MobileSettings.MobiKeepEventLog;
			}
		}
	}

	public class MobileSetting
	{
		[XmlAttribute(RawCommunicationConfig.STR_MobiMainPort)]
		public Int32 MobiMainPort { get; set; }

		[XmlAttribute(RawCommunicationConfig.STR_MobiVideoPort)]
		public Int32 MobiVideoPort { get; set; }

		[XmlAttribute(RawCommunicationConfig.STR_MobiKeepEventLog)]
		public Int32 MobiKeepEventLog { get; set; }
	}
	#endregion
}
