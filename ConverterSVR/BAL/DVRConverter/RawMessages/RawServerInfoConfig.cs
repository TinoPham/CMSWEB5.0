using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawServerInfoConfig : RawDVRConfig<RawServerInfoBody>
	{
		#region Parameter
		public const string STR_SiteInfo = "site_info";
		public const string STR_ServerInfo = "server_info";
		public const string STR_UserName = "user_name";
		//public const string STR_Version = "version";
		public const string STR_ServerID = "server_id";
		public const string STR_PacID = "pacid";
		public const string STR_IPAdd = "ip_add";
		public const string STR_IPv6Add = "ipv6_add";
		public const string STR_ServerName = "server_name";
		public const string STR_Location = "location";
		public const string STR_Model = "model";
		public const string STR_Distributor = "distributor";
		public const string STR_SaleDate = "sale_date";
		//public const string STR_Day = "day";
		//public const string STR_Month = "month";
		//public const string STR_Year = "year";
		public const string STR_NetworkCards = "network_cards";
		public const string STR_NetworkCard = "network_card_info";
		public const string STR_AdapterName = "adapterName";
		public const string STR_Description = "description";
		public const string STR_MacAddress = "mac_address";
		public const string STR_Addressv4List = "address_v4_list";
		public const string STR_Addressv4 = "ipv4";
		public const string STR_Address = "address";
		public const string STR_SubnetMask = "subnet_mask";
		public const string STR_Gatewayv4List = "gateway_v4_list";
		public const string STR_Gatewayv4 = "gateway_v4";
		public const string STR_MetricValue = "metric_value";
		public const string STR_AutomaticMetric = "automatic_metric";
		public const string STR_DefaultGateway = "default_gateway";
		public const string STR_DNSv4List = "dns_v4_list";
		public const string STR_DNSv4 = "dns_v4";
		//public const string STR_Address = "address";
		public const string STR_Addressv6List = "address_v6_list";
		public const string STR_ipv6 = "ipv6";
		public const string STR_SubnetPrefix = "subnet_prefix";
		public const string STR_Gatewayv6List = "gateway_v6_list";
		public const string STR_Gatewayv6 = "gateway_v6";
		public const string STR_DNSv6List = "dns_v6_list";
		public const string STR_DNSv6 = "dns_v6";
		public const string STR_Note = "note";
		public const string STR_Display = "display";
		public const string STR_SystemInfo = "system_info";
		//public const string STR_UserName = "user_name";
		public const string STR_CompName = "comp_name";
		public const string STR_Drives = "drives";
		public const string STR_Drive = "drive";
		public const string STR_TotalSpace = "total_space";
		public const string STR_FreeSpace = "free_space";
		public const string STR_Computer = "computer";
		public const string STR_CPUMode = "cpu_model";
		public const string STR_TotalMemory = "total_memory";
		public const string STR_FreeMemory = "free_memory";
		public const string STR_System = "system";
		public const string STR_OS = "os";
		//public const string STR_Version = "version";
		public const string STR_MACAddress = "mac_address";
		public const string STR_Language = "language";
		
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.siteData == null)
				return await base.UpdateToDB();

			if (UpdateServerInfoConfig(DVRAdressBook, msgBody.siteData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_SERVER_INFO, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_SERVER_INFO, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateServerInfoConfig(tDVRAddressBook dvrAdressBook, ServerInfoData siteData)
		{
			bool ret = false;
			ret |= UpdateServerIpAdd(dvrAdressBook, siteData);

			ret |= SetServerInfo(dvrAdressBook,siteData.serverInfo,siteData.systemInfo);

			ret |= SetOsInfos(dvrAdressBook, siteData.systemInfo);

			ret |= SetNetworkCards(dvrAdressBook,siteData.serverInfo);

			return ret;
		}

		private bool SetNetworkCards(tDVRAddressBook dvrAdressBook, ServerInfo serverInfo)
		{
			if (serverInfo == null || serverInfo.NetworkCards == null)
				return false;

			//Join 2 list to 1 Object list with samme channel ID
			var lsNetCards = db.Query<tDVRNetworkCard>(x => x.KDVR == dvrAdressBook.KDVR).ToList();
			bool result = false;
			var updates = from networkCard in lsNetCards
						  from networkCardInfo in serverInfo.NetworkCards
						  where networkCardInfo.MacAddress == networkCard.MACAddress
						  select new { Item = networkCard, InfoItem = networkCardInfo };

			//Update Object list above
			tDVRNetworkCard tDvrNetworkCard;
			foreach (var item in updates)
			{
				if (!item.InfoItem.Equal(item.Item))
				{
					tDvrNetworkCard = item.Item;
					item.InfoItem.SetEntity(ref tDvrNetworkCard);
					db.Update<tDVRNetworkCard>(tDvrNetworkCard);
					result = true;
				}
			}

			//Use channels list and except item have updates list for deleting.
			IEnumerable<tDVRNetworkCard> deletes = lsNetCards.Except(updates.Select(item => item.Item));
			foreach (tDVRNetworkCard delete in deletes)
			{
				db.Delete<tDVRNetworkCard>(delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				result = true;
			}

			//Use channels Info list and except item have updates list for inserting.
			IEnumerable<NetworkCardInfo> newitems = serverInfo.NetworkCards.Except(updates.Select(item => item.InfoItem));
			foreach (NetworkCardInfo newitem in newitems)
			{
				tDvrNetworkCard = new tDVRNetworkCard() { KDVR = dvrAdressBook.KDVR };
				newitem.SetEntity(ref tDvrNetworkCard);
				db.Insert<tDVRNetworkCard>(tDvrNetworkCard);
				result = true;
			}

			return result;
		}

		//private bool SetNetworkCards(tDVRAddressBook dvrAdressBook, ServerInfo serverInfo)
		//{
		//	var lsNetCards = db.Query<tDVRNetworkCard>(x => x.KDVR == dvrAdressBook.KDVR).ToList();
		//	Func<tDVRNetworkCard, NetworkCardInfo, bool> func_filter = (dbitem, info) => dbitem.MACAddress.Trim() == info.MacAddress.Trim();
		//	Func<tDVRNetworkCard, NetworkCardInfo, bool> compare_update = null;
		//	Expression<Func<tDVRNetworkCard, object>> updatedata = item => lsNetCards;

		//	Expression<Func<tDVRNetworkCard, string>> db_key = dbitem => dbitem.MACAddress;
		//	Expression<Func<NetworkCardInfo, string>> info_key = info => info.MacAddress;
		//	return base.UpdateDBData<tDVRNetworkCard, NetworkCardInfo, string, string>(lsNetCards, serverInfo.NetworkCards, func_filter, compare_update, updatedata, lsNetCards, db_key, info_key);
		//}

		private bool SetOsInfos(tDVRAddressBook dvrAdressBook, SystemInfo systemInfo)
		{
			bool ret = false;
			if (systemInfo == null) return false;
			tDVROSInfo osInfo = db.FirstOrDefault<tDVROSInfo>(item => item.KDVR == DVRAdressBook.KDVR);
			if (osInfo == null)
			{
				osInfo = new tDVROSInfo(){KDVR = dvrAdressBook.KDVR};
				systemInfo.SetEntity(ref osInfo);
				db.Insert<tDVROSInfo>(osInfo);
				ret = true;
			}
			else
			{
				if (systemInfo.Equal(osInfo)) return false;
				systemInfo.SetEntity(ref osInfo);
				db.Update<tDVROSInfo>(osInfo);
				ret = true;
			}
			return ret;
		}

		private bool UpdateServerIpAdd(tDVRAddressBook dvrAdressBook, ServerInfoData siteData)
		{
			if (siteData.serverInfo == null) return false;
			bool ret = false;
			if (dvrAdressBook.ServerID != siteData.serverInfo.ServerID || DVRAdressBook.ServerIP != siteData.serverInfo.IPAdd)
			{
				dvrAdressBook.ServerID = siteData.serverInfo.ServerID;
				dvrAdressBook.ServerIP = siteData.serverInfo.IPAdd;
				db.Update<tDVRAddressBook>(dvrAdressBook);
				ret = true;
			}
			return ret;
		}

		private bool SetServerInfo(tDVRAddressBook addressBook, ServerInfo serverInfo, SystemInfo systemInfo)
		{
			bool ret = false;
			if (serverInfo == null) return false;
			tDVRServerInfo serInfo = db.FirstOrDefault<tDVRServerInfo>(item => item.KDVR == DVRAdressBook.KDVR);
			string kLang = GetKlanguage(systemInfo);
			if (serInfo == null)
			{
				serInfo = new tDVRServerInfo() { KDVR = addressBook.KDVR, KLanguage = kLang};
				serverInfo.SetEntity(ref serInfo);
				db.Insert<tDVRServerInfo>(serInfo);
				ret = true;
			}
			else
			{
				if (serverInfo.Equal(serInfo) && serInfo.KLanguage == kLang) return false;
				serverInfo.SetEntity(ref serInfo);
				serInfo.KLanguage = kLang;
				db.Update<tDVRServerInfo>(serInfo);
				ret = true;
			}
			return ret;
		}

		private string GetKlanguage(SystemInfo systemInfo)
		{
			string kLang = string.Empty;
			if (systemInfo == null) return string.Empty;
			tLanguage lang = db.FirstOrDefault<tLanguage>(x => x.Language == systemInfo.Language);   // System.String.CompareOrdinal(x.Language, systemInfo.Language) == 0);
			if (lang != null)
			{
				kLang = lang.KLanguage;
			}
			return kLang;
		}


		#region Unused

		//private List<NetworkCardInfo> GetNetworkCardList()
		//{
		//	List<NetworkCardInfo> nwCardInfolst = IsNetworkCards() ? msgBody.siteData.serverInfo.NetworkCards.ToList() : new List<NetworkCardInfo>();
		//	return nwCardInfolst;
		//}

		//private void SetNetworkCard2s(Int32 kDvr)
		//{
		//	List<tDVRNetworkCard> lsNetCards = db.Query<tDVRNetworkCard>(x => x.KDVR == kDvr).ToList();

		//	List<NetworkCardInfo> nwCardInfolst = GetNetworkCardList();

		//	foreach (var cn in lsNetCards)
		//	{
		//		tDVRNetworkCard networkCard = cn;
		//		NetworkCardInfo nw = nwCardInfolst.FirstOrDefault(x => x.MacAddress.Trim().ToUpper() == networkCard.MACAddress.Trim().ToUpper());
		//		if (nw != null)
		//		{
		//			if (!CompareNetworkCardInfo(kDvr, nw, networkCard))
		//			{
		//				SetNetworkCardInfo(kDvr, nw, ref networkCard);
		//				db.Update<tDVRNetworkCard>(networkCard);
		//			}
		//			nwCardInfolst.Remove(nw);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRNetworkCard>(networkCard);
		//		}
		//	}

		//	foreach (var nw in nwCardInfolst)
		//	{
		//		var netcard = new tDVRNetworkCard();
		//		SetNetworkCardInfo(kDvr, nw, ref netcard);
		//		db.Insert<tDVRNetworkCard>(netcard);
		//	}
		//	//db.Save();
		//}

		//private void SetNetworkCardInfo(Int32 kDvr, NetworkCardInfo nwc, ref tDVRNetworkCard netcard)
		//{
		//	netcard.KDVR = kDvr;
		//	//netcard.NetworkCardID = msgBody.siteData.serverInfo.NetworkCards[idx];
		//	netcard.AdapterName = nwc.AdapterName;
		//	netcard.Description = nwc.Description;
		//	netcard.MACAddress = nwc.MacAddress;
		//	netcard.IPv4List = nwc.Addv4List.ToString();
		//	netcard.Gatewayv4List = nwc.Gatewayv4List.ToString();
		//	netcard.DNSv4List = nwc.DNSv4List.ToString();
		//	netcard.IPv6List = nwc.ipv6List.ToString();
		//	netcard.Gatewayv6List = nwc.Gatewayv6List.ToString();
		//	netcard.DNSv6List = nwc.DNSv6List.ToString();
		//}

		//private bool CompareNetworkCardInfo(Int32 kDvr, NetworkCardInfo nwc, tDVRNetworkCard netcard)
		//{
		//	bool result = netcard.KDVR == kDvr &&
		//				  netcard.AdapterName == nwc.AdapterName &&
		//				  netcard.Description == nwc.Description &&
		//				  netcard.MACAddress == nwc.MacAddress &&
		//				  netcard.IPv4List == nwc.Addv4List.ToString() &&
		//				  netcard.Gatewayv4List == nwc.Gatewayv4List.ToString() &&
		//				  netcard.DNSv4List == nwc.DNSv4List.ToString() &&
		//				  netcard.IPv6List == nwc.ipv6List.ToString() &&
		//				  netcard.Gatewayv6List == nwc.Gatewayv6List.ToString() &&
		//				  netcard.DNSv6List == nwc.DNSv6List.ToString();
		//	return result;
		//}

		#endregion

	}

	#region Class for Server infor
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawServerInfoBody
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
		[XmlElement(RawServerInfoConfig.STR_SiteInfo)]
		public ServerInfoData siteData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawServerInfoConfig.STR_SiteInfo)]
	public class ServerInfoData
	{
		[XmlElement(RawServerInfoConfig.STR_ServerInfo)]
		public ServerInfo serverInfo { get; set; }

		[XmlElement(RawServerInfoConfig.STR_SystemInfo)]
		public SystemInfo systemInfo { get; set; }
	}

	[XmlRoot(RawServerInfoConfig.STR_ServerInfo)]
	public class ServerInfo : IMessageEntity<tDVRServerInfo>
	{
		[XmlElement(RawServerInfoConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(MessageDefines.STR_Version)]
		public string Version { get; set; }

		[XmlElement(RawServerInfoConfig.STR_ServerID)]
		public string ServerID { get; set; }

		[XmlElement(RawServerInfoConfig.STR_PacID)]
		public string PacID { get; set; }

		[XmlElement(RawServerInfoConfig.STR_IPAdd)]
		public string IPAdd { get; set; }

		[XmlElement(RawServerInfoConfig.STR_IPv6Add)]
		public string IPv6Add { get; set; }

		[XmlElement(RawServerInfoConfig.STR_ServerName)]
		public string ServerName { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Location)]
		public string Location { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Model)]
		public string Model { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Distributor)]
		public string Distributor { get; set; }

		[XmlElement(RawServerInfoConfig.STR_SaleDate)]
		public SaleDate saleDate { get; set; }

		[XmlArray(RawServerInfoConfig.STR_NetworkCards)]
		[XmlArrayItem(RawServerInfoConfig.STR_NetworkCard)]
		public List<NetworkCardInfo> NetworkCards = new List<NetworkCardInfo>();

		[XmlElement(RawServerInfoConfig.STR_Note)]
		public string Note { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Display)]
		public Int32 Display { get; set; }

		public bool Equal(tDVRServerInfo value)
		{
			bool result = //value.KDVR == kDvr &&
						  value.Version == Version &&
						  value.DvrName == ServerName &&
						  value.Location == Location &&
						  value.Model == Model &&
						  value.UserName == UserName &&
						  value.PACID == PacID &&
						  value.Distributor == Distributor &&
						  value.Note == Note &&
						  value.Display == Display &&
						  //value.KLanguage == kLang &&
						  value.ServerIPv6 == IPv6Add;
			if (saleDate != null)
			{
				result = result &&
						 value.SaleDate == saleDate.Value;
			}
			return result;
		}

		public void SetEntity(ref tDVRServerInfo value)
		{
			if (value == null)
				value = new tDVRServerInfo();
			
			//value.KDVR = kDvr;
			value.Version = Version;
			value.DvrName = ServerName;
			value.Location = Location;
			value.Model = Model;
			value.UserName = UserName;
			value.PACID = PacID;
			value.Distributor = Distributor;
			if (saleDate != null)
				value.SaleDate = saleDate.Value;
			value.Note = Note;
			value.Display = Display;
			//value.KLanguage = kLang;
			value.ServerIPv6 = IPv6Add;
		}
	}

	[XmlRoot(RawServerInfoConfig.STR_SaleDate)]
	public class SaleDate
	{
		[XmlElement(MessageDefines.STR_Day)]
		public Int32 Day { get; set; }

		[XmlElement(MessageDefines.STR_Month)]
		public Int32 Month { get; set; }

		[XmlElement(MessageDefines.STR_Year)]
		public Int32 Year { get; set; }

		public DateTime Value
		{
			get
			{
				return new DateTime(Year, Month, Day);
			}
		}
	}

	[XmlRoot(RawServerInfoConfig.STR_NetworkCard)]
	public class NetworkCardInfo : IMessageEntity<tDVRNetworkCard>
	{
		[XmlElement(RawServerInfoConfig.STR_AdapterName)]
		public string AdapterName { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Description)]
		public string Description { get; set; }

		[XmlElement(RawServerInfoConfig.STR_MacAddress)]
		public string MacAddress { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Addressv4List)]
		public Addressv4List Addv4List = new Addressv4List();

		[XmlElement(RawServerInfoConfig.STR_Gatewayv4List)]
		public Gatewayv4List Gatewayv4List = new Gatewayv4List();

		[XmlElement(RawServerInfoConfig.STR_DNSv4List)]
		public DNSv4List DNSv4List = new DNSv4List();

		[XmlElement(RawServerInfoConfig.STR_Addressv6List)]
		public Addressv6List ipv6List = new Addressv6List();

		[XmlElement(RawServerInfoConfig.STR_Gatewayv6List)]
		public Gatewayv6List Gatewayv6List = new Gatewayv6List();

		[XmlElement(RawServerInfoConfig.STR_DNSv6List)]
		public DNSv6List DNSv6List = new DNSv6List();

		public bool Equal(tDVRNetworkCard value)
		{
			bool result = //value.KDVR == kDvr &&
						  value.AdapterName == AdapterName &&
						  value.Description == Description &&
						  value.MACAddress == MacAddress &&
						  value.IPv4List == Addv4List.ToString() &&
						  value.Gatewayv4List == Gatewayv4List.ToString() &&
						  value.DNSv4List == DNSv4List.ToString() &&
						  value.IPv6List == ipv6List.ToString() &&
						  value.Gatewayv6List == Gatewayv6List.ToString() &&
						  value.DNSv6List == DNSv6List.ToString();
			return result;
		}

		public void SetEntity(ref tDVRNetworkCard value)
		{
			if (value == null)
				value= new tDVRNetworkCard();
			//value.KDVR = kDvr;
			//netcard.NetworkCardID = msgBody.siteData.serverInfo.NetworkCards[idx];
			value.AdapterName = AdapterName;
			value.Description = Description;
			value.MACAddress = MacAddress;
			value.IPv4List = Addv4List.ToString();
			value.Gatewayv4List = Gatewayv4List.ToString();
			value.DNSv4List = DNSv4List.ToString();
			value.IPv6List = ipv6List.ToString();
			value.Gatewayv6List = Gatewayv6List.ToString();
			value.DNSv6List = DNSv6List.ToString();
		}
	}

	#region IPv4
	[XmlRoot(RawServerInfoConfig.STR_Addressv4List)]
	public class Addressv4List
	{
		[XmlElement(RawServerInfoConfig.STR_Addressv4)]
		public List<Addressv4Info> Addressv4s = new List<Addressv4Info>();

		public override string ToString()
		{
			string sRet = string.Empty;
			if (Addressv4s == null) return sRet;
			foreach (Addressv4Info addr in Addressv4s)
			{
				sRet += addr.Address + "-" + addr.SubnetMask + ";";
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_Addressv4)]
	public class Addressv4Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_Address)]
		public string Address { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_SubnetMask)]
		public string SubnetMask { get; set; }
	}
	[XmlRoot(RawServerInfoConfig.STR_Gatewayv4List)]
	public class Gatewayv4List
	{
		[XmlElement(RawServerInfoConfig.STR_Gatewayv4)]
		public List<Gatewayv4Info> Gatewayv4S = new List<Gatewayv4Info>();

		public override string ToString()
		{
			string sRet = string.Empty;
			if (Gatewayv4S == null) return sRet;
			foreach (Gatewayv4Info gate in Gatewayv4S)
			{
				sRet += gate.DefaultGateway + "-" + gate.AutomaticMetric + "-" + gate.MetricValue + ";";
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_Gatewayv4)]
	public class Gatewayv4Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_DefaultGateway)]
		public string DefaultGateway { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_AutomaticMetric)]
		public string AutomaticMetric { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_MetricValue)]
		public string MetricValue { get; set; }
	}
	[XmlRoot(RawServerInfoConfig.STR_DNSv4List)]
	public class DNSv4List
	{
		[XmlElement(RawServerInfoConfig.STR_DNSv4)]
		public List<DNSv4Info> DnSv4S = new List<DNSv4Info>();

		public override string ToString()
		{
			string sRet = string.Empty;
			if (DnSv4S == null) return sRet;
			foreach (DNSv4Info dns in DnSv4S)
			{
				sRet += dns.Address + ";";
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_DNSv4)]
	public class DNSv4Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_Address)]
		public string Address { get; set; }
	}
	#endregion
	[XmlRoot(RawServerInfoConfig.STR_Addressv6List)]
	public class Addressv6List
	{
		[XmlElement(RawServerInfoConfig.STR_ipv6)]
		public List<ipv6Info> ipv6s = new List<ipv6Info>();
		public override string ToString()
		{
			string sRet = string.Empty;
			if (ipv6s != null)
			{
				foreach (ipv6Info ipv6 in ipv6s)
				{
					sRet += ipv6.Address + "-" + ipv6.SubnetPrefix + ";";
				}
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_ipv6)]
	public class ipv6Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_Address)]
		public string Address { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_SubnetPrefix)]
		public string SubnetPrefix { get; set; }
	}
	[XmlRoot(RawServerInfoConfig.STR_Addressv6List)]
	public class Gatewayv6List
	{
		[XmlElement(RawServerInfoConfig.STR_Gatewayv6)]
		public List<Gatewayv6Info> Gatewayv6s = new List<Gatewayv6Info>();
		public override string ToString()
		{
			string sRet = string.Empty;
			if (Gatewayv6s != null)
			{
				foreach (Gatewayv6Info gate in Gatewayv6s)
				{
					sRet += gate.DefaultGateway + "-" + gate.AutomaticMetric + "-" + gate.MetricValue + ";";
				}
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_Gatewayv6)]
	public class Gatewayv6Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_DefaultGateway)]
		public string DefaultGateway { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_AutomaticMetric)]
		public string AutomaticMetric { get; set; }

		[XmlAttribute(RawServerInfoConfig.STR_MetricValue)]
		public string MetricValue { get; set; }
	}
	[XmlRoot(RawServerInfoConfig.STR_DNSv6List)]
	public class DNSv6List
	{
		[XmlElement(RawServerInfoConfig.STR_DNSv6)]
		public List<DNSv6Info> DnSv6S = new List<DNSv6Info>();

		public override string ToString()
		{
			string sRet = string.Empty;
			if (DnSv6S == null) return sRet;
			foreach (DNSv6Info dns in DnSv6S)
			{
				sRet += dns.Address + ";";
			}
			return sRet;
		}
	}
	[XmlRoot(RawServerInfoConfig.STR_DNSv6)]
	public class DNSv6Info
	{
		[XmlAttribute(RawServerInfoConfig.STR_Address)]
		public string Address { get; set; }
	}

	[XmlRoot(RawServerInfoConfig.STR_SystemInfo)]
	public class SystemInfo : IMessageEntity<tDVROSInfo>
	{
		[XmlElement(RawServerInfoConfig.STR_UserName)]
		public string UserName { get; set; }

		[XmlElement(RawServerInfoConfig.STR_CompName)]
		public string CompName { get; set; }

		[XmlArray(RawServerInfoConfig.STR_Drives)]
		[XmlArrayItem(RawServerInfoConfig.STR_Drive)]
		public List<SysDriverInfo> Drivers = new List<SysDriverInfo>();

		[XmlElement(RawServerInfoConfig.STR_Computer)]
		public ComputerInfo compInfo { get; set; }

		[XmlElement(RawServerInfoConfig.STR_System)]
		public SysInfo sysInfo { get; set; }

		[XmlElement(RawServerInfoConfig.STR_Language)]
		public string Language { get; set; }

		public bool Equal(tDVROSInfo value)
		{
			bool result = //value.KDVR == kDvr &&
			value.ComputerName == CompName &&
			value.UserName == UserName;

			if (compInfo != null)
			{
				result = result &&
				value.CPUModel == compInfo.CPUMode &&
				value.TotalMemory == compInfo.TotalMemory &&
				value.FreeMemory == compInfo.FreeMemory;
			}
			if (sysInfo != null)
			{
				result = result &&
				value.OSPlatform ==sysInfo.OS &&
				value.OSVersion == sysInfo.Version;
			}
			return result;
		}

		public void SetEntity(ref tDVROSInfo value)
		{
			if(value == null) 
				value = new tDVROSInfo();

			value.ComputerName = CompName;
			value.UserName = UserName;

			if (compInfo != null)
			{
				value.CPUModel =compInfo.CPUMode;
				value.TotalMemory = compInfo.TotalMemory;
				value.FreeMemory = compInfo.FreeMemory;
			}
			if (sysInfo != null)
			{
				value.OSPlatform = sysInfo.OS;
				value.OSVersion = sysInfo.Version;
			}
		}
	}

	[XmlRoot(RawServerInfoConfig.STR_Drive)]
	public class SysDriverInfo
	{
		[XmlAttribute(RawServerInfoConfig.STR_TotalSpace)]
		public Int32 TotalSpace {get; set;}

		[XmlAttribute(RawServerInfoConfig.STR_FreeSpace)]
		public Int32 FreeSpace { get; set; }

		[XmlText]
		public string Name { get; set; }
	}

	[XmlRoot(RawServerInfoConfig.STR_Computer)]
	public class ComputerInfo
	{
		[XmlElement(RawServerInfoConfig.STR_CPUMode)]
		public string CPUMode { get; set; }

		[XmlElement(RawServerInfoConfig.STR_TotalMemory)]
		public string TotalMemory { get; set; }

		[XmlElement(RawServerInfoConfig.STR_FreeMemory)]
		public string FreeMemory { get; set; }
	}

	[XmlRoot(RawServerInfoConfig.STR_System)]
	public class SysInfo
	{
		[XmlElement(RawServerInfoConfig.STR_OS)]
		public string OS { get; set; }

		[XmlElement(MessageDefines.STR_Version)]
		public string Version { get; set; }

		[XmlElement(RawServerInfoConfig.STR_MACAddress)]
		public string MACAddress { get; set; }
	}
#endregion
}
