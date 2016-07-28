using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Commons;
using SVRDatabase;
using Extensions;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawDVRConnect : RawDVRConfig<RawDVRConnectBody>
	{
		#region Parameter
		public const string STR_ServerInfo = "server_info";
		public const string STR_ServerName = "server_name";
		//public new const string STR_Version = "version";
		public const string STR_ServerIp = "server_ip";
		public const string STR_ServerId = "server_id";
		public const string STR_CmsMode = "cms_mode";
		public const string STR_HaspLicense = "hasp_license";

		public const string STR_Configs = "configs";
		public const string STR_Config = "config";
		public const string STR_TimeChange = "time_change";
		public const string STR_Checksum = "checksum";
		public const string STR_Public_Ip = "public_ip";

		private enum DVRStatus { 
			OFFLINE =0,
			ONLINE
		}

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawDVRConnectBody msgBody { get; set; }
		#endregion

		public RawDVRConnect()
		{
		}

		public RawDVRConnect(string strMsg)
		{
			RawDVRConnect rw = Commons.ObjectUtils.DeSerialize(typeof(RawDVRConnect), strMsg) as RawDVRConnect;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{

			if (msgHeader.MsgID == (int)CMSMsg.MSG_DVR_DISCONNECT)
			{
				if (DVRAdressBook == null)
				{
					DVRAdressBook = db.Query<tDVRAddressBook>(item => item.DVRGuid == msgHeader.DVRGuid).OrderByDescending(x => x.KDVR).FirstOrDefault();
				}
				if (DVRAdressBook != null)
				{
					DVRAdressBook.Online = (int)DVRStatus.OFFLINE;
					DVRAdressBook.TimeDisConnect = DateTime.UtcNow;
					db.Update<tDVRAddressBook>(DVRAdressBook);
				}
				return await Task.FromResult<Commons.ERROR_CODE>(db.Save() == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : ERROR_CODE.OK);	
			}
			if (msgBody.dvrData == null) //DVRAdressBook == null || 
			{
				return await base.UpdateToDB();
			}
			if (DVRAdressBook == null) //new DVR
			{
				DVRAdressBook = new tDVRAddressBook();
				SetDVRInfo(ref DVRAdressBook);
				DVRAdressBook.tDVRKeepAlife = new tDVRKeepAlives() { LastAccess = (int)DateTime.Now.FullDateTimeToUnixTimestamp(), KDVR = DVRAdressBook.KDVR }; //Update lastaccess for new DVR

				db.Insert<tDVRAddressBook>(DVRAdressBook);
			}
			else
			{
				tDVRKeepAlives keepalive = db.FirstOrDefault<tDVRKeepAlives>(item => item.KDVR == DVRAdressBook.KDVR);
				if( keepalive != null)
				{
					keepalive.LastAccess = (int)DateTime.Now.FullDateTimeToUnixTimestamp();//(int)ToUnixTimestamp(DateTime.Now);
					db.Update<tDVRKeepAlives>(keepalive);
				}
				else 
				{
					keepalive = new tDVRKeepAlives { LastAccess = (int)DateTime.Now.FullDateTimeToUnixTimestamp(), KDVR = DVRAdressBook.KDVR }; //ToUnixTimestamp(DateTime.Now)
					db.Insert<tDVRKeepAlives>(keepalive);
				}

				SetDVRInfo(ref DVRAdressBook);
				db.Update<tDVRAddressBook>(DVRAdressBook);
			}

			return await Task.FromResult<Commons.ERROR_CODE>( db.Save() == -1? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : ERROR_CODE.OK);
		}

		private tDVRVersion GetVersion(DVRConnectData dvrData)
		{
			tCMSDVRSupportVersion tCmsSupportVersion = null;
			var _db = db.Query<tCMSDVRSupportVersion>().ToList();
			//foreach (var t in _db)
			//{
			//	if (dvrData.Version.StartsWith(t.VersionName))
			//	{
			//		tCmsSupportVersion = t;
			//	}
			//}
			db.Query<tCMSDVRSupportVersion>().ToList().ForEach(
				t =>
				{
					if (dvrData.Version.StartsWith(t.VersionName))
					{
						tCmsSupportVersion = t;
					}
				}
				);

			if (tCmsSupportVersion == null) return null;

			var tdvrVersion = db.FirstOrDefault<tDVRVersion>(t => t.Product == tCmsSupportVersion.Product && t.Version == tCmsSupportVersion.Version);
			
			return tdvrVersion;
		}

		private void SetDVRInfo(ref tDVRAddressBook dvrInfo)
		{
			dvrInfo.DVRGuid = msgHeader.DVRGuid;
			dvrInfo.ServerID = msgBody.dvrData.ServerId;
			dvrInfo.ServerIP = msgBody.dvrData.ServerIp;
			dvrInfo.CMSMode = msgBody.CmsMode;
			dvrInfo.DVRAlias = msgBody.dvrData.ServerName;
			dvrInfo.FirstAccess = DateTime.UtcNow;
			dvrInfo.Online = (int)DVRStatus.ONLINE; ;
			dvrInfo.LastConnectTime = dvrInfo.CurConnectTime;
			dvrInfo.CurConnectTime = (int)DateTime.Now.FullDateTimeToUnixTimestamp();// ToUnixTimestamp(DateTime.UtcNow);
			
			dvrInfo.PublicServerIP = msgBody.dvrData.PublicServerIP;
			dvrInfo.HaspLicense = msgBody.HaspLicense;

			var tdvrVersion = GetVersion(msgBody.dvrData);
			if (tdvrVersion != null)
			{
				dvrInfo.KDVRVersion = tdvrVersion.KDVRVersion;
			}
		}

		public override async Task<string> GetResponseMsg()
		{
			if (msgHeader.MsgID == (int)CMSMsg.MSG_DVR_DISCONNECT)
			{
				return await Task.FromResult<string>(string.Empty);
			}
			List<String> seqMessage = new List<string>();

			List<tDVRConfigChangeTime> lsChecksums = db.Query<tDVRConfigChangeTime>(x=>x.KDVR == DVRAdressBook.KDVR).ToList();

			List<int> changedConfigs = null;
			if (msgBody.ConfigChecksums != null && msgBody.ConfigChecksums.Count > 0)
			{
				changedConfigs = (from dbcs in msgBody.ConfigChecksums
								  where (from dvr in lsChecksums
										 where (int)dbcs.id == (int)dvr.KConfig
											  && (int)dbcs.Checksum != (int)dvr.Checksum
										 select dvr).Any() ||
										 !(from b in lsChecksums select b.KConfig).Contains(dbcs.id)
								  select (int)dbcs.id).ToList();
			}
			else
			{
				changedConfigs = Enum.GetValues(typeof(DVR_CONFIGURATION)).Cast<int>().Where(item => item >= (int)DVR_CONFIGURATION.EMS_CFG_MANAGE_USERS && item < (int)DVR_CONFIGURATION.EMS_CFG_END).ToList();
			}

			if (msgHeader.MsgID == (int)CMSMsg.MSG_DVR_CONNECT)
			{
				int keepAliveInterval = LogDB.SVRConfig.KeepAliveInterval;
				seqMessage.Add(Commons.Utils.String2Base64(GetConnectResponseMsg(msgHeader.DVRGuid, keepAliveInterval)));
			}

			if (changedConfigs.Contains((int)DVR_CONFIGURATION.EMS_CFG_HARDWARE) || changedConfigs.Contains((int)DVR_CONFIGURATION.EMS_CFG_ISEARCH))
			{
				changedConfigs.Remove((int)DVR_CONFIGURATION.EMS_CFG_HARDWARE);
				changedConfigs.Remove((int)DVR_CONFIGURATION.EMS_CFG_ISEARCH);
				changedConfigs.RemoveAll(x => _notSend.Contains(x));

				//string sChangedConfigs = String.Join(";", changedConfigs);
				//seqMessage.Add(Commons.Utils.String2Base64(GetRequestConfigMsg(msgHeader.DVRGuid, (int)DVR_CONFIGURATION.EMS_CFG_HARDWARE, sChangedConfigs)));
			}
			else
			{
				changedConfigs.RemoveAll(x => _notSend.Contains(x));
				//foreach (int cfgID in changedConfigs)
				//{
				//	seqMessage.Add(Commons.Utils.String2Base64(GetRequestConfigMsg(msgHeader.DVRGuid, cfgID)));
				//}
			}
			//Anh Huynh, Always refresh Hardware page - not update number of channel when HASP has been changed 
			string sChangedConfigs = String.Join(";", changedConfigs);
			seqMessage.Add(Commons.Utils.String2Base64(GetRequestConfigMsg(msgHeader.DVRGuid, (int)DVR_CONFIGURATION.EMS_CFG_HARDWARE, sChangedConfigs)));

			var combined = string.Join(", ", seqMessage);
			return await Task.FromResult<string>(combined);
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawDVRConnectBody
	{
		[XmlElement(RawDVRConnect.STR_ServerInfo)]
		public DVRConnectData dvrData { get; set; }

		[XmlElement(RawDVRConnect.STR_CmsMode)]
		public Int16 CmsMode { get; set; }

		[XmlElement(RawDVRConnect.STR_HaspLicense)]
		public string HaspLicense { get; set; }

		[XmlArray(RawDVRConnect.STR_Configs)]
		[XmlArrayItem(RawDVRConnect.STR_Config)]
		public List<DVRConfigChecksum> ConfigChecksums = new List<DVRConfigChecksum>();
	}

	[XmlRoot(RawDVRConnect.STR_ServerInfo)]
	public class DVRConnectData
	{
		[XmlElement(RawDVRConnect.STR_ServerName)]
		public string ServerName { get; set; }

		[XmlElement(MessageDefines.STR_Version)]
		public string Version { get; set; }

		[XmlElement(RawDVRConnect.STR_ServerIp)]
		public string ServerIp { get; set; }

		[XmlElement(RawDVRConnect.STR_ServerId)]
		public string ServerId { get; set; }

		[XmlElement(RawDVRConnect.STR_Public_Ip)]
		public string PublicServerIP {get;set; }
	}

	[XmlRoot(RawDVRConnect.STR_Config)]
	public class DVRConfigChecksum
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public byte id { get; set; }

		[XmlAttribute(RawDVRConnect.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlAttribute(RawDVRConnect.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlText]
		public string Name { get; set; }
	}
	#endregion
}
