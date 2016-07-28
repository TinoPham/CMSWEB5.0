using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Commons;
using ConvertMessage;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace PACDMConverter
{
	internal class DVRInfos : Commons.SingletonClassBase<DVRInfos>
	{
		public const string Regex_SRXPRO_Version = @"^(?<V1>\d{1,3})\.(?<V2>\d{1,3})\.(?<V3>\d{1,3})(?:\.(?<V4>\d{1,3}))?";
		readonly Version SRX_PRO_VERSION_LICENSE = new Version(3,3,0,0);
		//private static readonly Lazy<DVRInfos> Lazy = new Lazy<DVRInfos>(() => new DVRInfos());
		//public static DVRInfos Instance { get { return Lazy.Value; } }
		readonly object lock_SRXProInfo = new object();
		readonly object lock_PACDMInfo = new object();
		readonly object lock_SPKID = new object();
		readonly object lock_MsgDVRInfo = new object();
		readonly object lock_MacAddressInfos = new object();
		
		SRXProInfos _SRXProInfo = null;
		
		PACDMInfos _PACDMInfo = null; 

		string _SPKID = string.Empty;

		MessageDVRInfo _MsgDVRInfo = null;

		List<MacAddressInfo> _MacAddressInfos = null;

		public SRXProInfos SRXProInfo
		{ 
			get
			{
				lock( lock_SRXProInfo)
				{
				if( _SRXProInfo == null)
					_SRXProInfo = GetSRXProInfos();
				}
					return _SRXProInfo;
			}
		 }

		public PACDMInfos PACDMInfo
		{ 
			get
			{ 
				lock(lock_PACDMInfo)
				{
					if( _PACDMInfo == null)
						_PACDMInfo = GetPACDMInfos();
				}
				return _PACDMInfo;
			}
		}
		
		public List<MacAddressInfo> MacAddressInfos
		 {
			get
			{ 
				lock(lock_MacAddressInfos)
				{
					if( _MacAddressInfos == null || _MacAddressInfos.Count == 0)
						_MacAddressInfos = GetNetworkInfo();
				}
				return _MacAddressInfos;
			}
		}

		public string SPKID
		{
			get{
				lock(lock_SPKID)
				{
					//if( string.IsNullOrEmpty(_SPKID) )//forece to load SPK
					//{
						SRXProInfos srxpro = SRXProInfo;
						Version pro_ver = SRXPro_VersionStringtoVersion(srxpro == null || string.IsNullOrEmpty(srxpro.SRXProVersion) ? "0.0.0.0" : srxpro.SRXProVersion);
						if( pro_ver.CompareTo( SRX_PRO_VERSION_LICENSE) >= 0)
						{
							if (HASPkey.HaspLicense.Instance.ReadLicense(srxpro.ServerAppPath))
								_SPKID = HASPkey.HaspLicense.Instance.LicensInfo;
//#if DEBUG
//                            _SPKID = "154001";
//#endif
						}
						//if( string.IsNullOrEmpty(_SPKID))
						else
							_SPKID = GetSPKID();
					//}
				}
				return _SPKID;
			}
		}

		public MessageDVRInfo MsgDVRInfo
		{
			get{
				lock(lock_MsgDVRInfo)
				{
					if( _MsgDVRInfo == null)
						_MsgDVRInfo = GetMsgDVRInfo();
					else
						_MsgDVRInfo.HASPK = SPKID;
				}
				return _MsgDVRInfo;
			}
				
		}

		public readonly string HostName;

		private DVRInfos()
		{
			try
			{
				HostName = Dns.GetHostName(); 
			}
			catch(Exception){}
		}

		public void Refresh()
		{
			lock(lock_MsgDVRInfo)
			{
				_MacAddressInfos = null;
			}
			lock(lock_PACDMInfo)
			{
				_PACDMInfo = null;
			}
			lock (lock_SRXProInfo)
			{
				_SRXProInfo = null;
			}
			lock(lock_SPKID)
			{
			_SPKID = string.Empty;
			}
			lock (lock_MsgDVRInfo)
			{
				_MsgDVRInfo = null;
			}
		}

		private MessageDVRInfo GetMsgDVRInfo()
		{
			return new MessageDVRInfo{
			HASPK = SPKID,
			MACs = MacAddressInfos.Select( item => new ConvertMessage.MacInfo{ IP_Address = item.IpAddress, IP_Version = item.Version, MAC_Address = item.MACAddress, Active = item.Active, MacOrder = item.MacOrder}).ToList(),
			Date = DateTime.Now,
			HostName = HostName,
			PACinfo = new PACInfo { PACID = PACDMInfo.PACID, PACVersion = PACDMInfo.PACVersion }
			};
		}

		private string GetPhysicalAddress(PhysicalAddress physAddress, char separate = '-')
		{
			byte[] bytes = physAddress.GetAddressBytes();
			string ret = string.Empty;
			for (int i = 0; i < bytes.Length; i++)
			{
				// Display the physical address in hexadecimal.
				ret += bytes[i].ToString(Consts.HEXA_FORMAT);
				if (i != bytes.Length - 1)
				{
					ret += separate.ToString();
				}
			}
			return ret;
		}
		
		private string GetSPKID()
		{
			HASPkey.HASPkey.Instance.haspReadAPI();
			return HASPkey.HASPkey.Instance.HaspID;

			//HASPkey.HASPkey haspKey = new HASPkey.HASPkey();
			//Aladdin.HASP.HaspStatus status = haspKey.haspGetHaspId();

			//return status == Aladdin.HASP.HaspStatus.StatusOk? haspKey.HaspID : string.Empty;
		}

		private IPAddress CheckGatewayAddress(GatewayIPAddressInformationCollection gwAddress)
		{
			if (gwAddress == null || gwAddress.Count == 0)
				return new IPAddress(0);
			IPAddress ipret = new IPAddress(0);
			foreach (GatewayIPAddressInformation add in gwAddress)
			{
				if (add.Address != ipret)
				{
					ipret = add.Address;
					break;
				}

			}
			return ipret;
		}
		
		private List<MacAddressInfo> GetNetworkInfo(NetworkInterface NetInterface, int index, bool isactive)
		{
			if( NetInterface == null)
				return null;
			List<MacAddressInfo> ret = new List<MacAddressInfo>();
			IPInterfaceProperties ipProps = NetInterface.GetIPProperties();
			string mac_add = string.Empty;
			foreach (var ip in ipProps.UnicastAddresses)
			{
				//if ((NetInterface.OperationalStatus == OperationalStatus.Up)
				//    && (ip.Address.AddressFamily == AddressFamily.InterNetwork || ip.Address.AddressFamily == AddressFamily.InterNetworkV6))
				//{
				if( string.IsNullOrEmpty(mac_add))
					mac_add = GetPhysicalAddress( NetInterface.GetPhysicalAddress());
				ret.Add( new MacAddressInfo
						{ 
							MACAddress = mac_add, 
							IpAddress = ip.Address.ToString(),
							LoopBack = NetInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback,
							Version = ip.Address.AddressFamily.ToString(), 
							Active = isactive,
							MacOrder = index + 1//because index from 0

						} );
				//}
			}
			return ret;
		}
		
		private List<MacAddressInfo> GetNetworkInfo()
		{
			List<MacAddressInfo> lst_Addressinfo = new List<MacAddressInfo>();
			NetworkInterface[] Nics = NetworkInterface.GetAllNetworkInterfaces();
			List<MacAddressInfo> nicInfos = null;
			int index = 0;
			MacAddressInfo active_ip = null;
			foreach (NetworkInterface Nic in Nics)
			{
				//if (Nic.OperationalStatus != OperationalStatus.Up)
				//    continue;
				if (Nic.NetworkInterfaceType == NetworkInterfaceType.Unknown)
					continue;
				if (Nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;
				active_ip = lst_Addressinfo.FirstOrDefault(item => item.Active && string.Compare(item.Version, AddressFamily.InterNetwork.ToString(), true) == 0);
				nicInfos = GetNetworkInfo(Nic, index++, active_ip == null );
				if( nicInfos == null || nicInfos.Count == 0)
					continue;
				lst_Addressinfo.AddRange( nicInfos);
			}
			return lst_Addressinfo;
		}

		private SRXProInfos GetSRXProInfos()
		{
			SRXProInfos srxinfo = new SRXProInfos();
			srxinfo.ServerAppPath =	Utils.Instance.GetRegValue(RegistryHive.LocalMachine, Utils.Instance.SRX_Pro_Reg_Settings_Path, Consts.STR_ServerAppPath);
			srxinfo.SRXProVersion = Utils.Instance.GetRegValue(RegistryHive.LocalMachine, Utils.Instance.SRX_Pro_Reg_Settings_Path, Consts.STR_SRX_Pro_Version); 
			return srxinfo;
		}
		
		private PACDMInfos GetPACDMInfos()
		{
			PACDMInfos pinfo = new PACDMInfos();
			pinfo.PACDir = Utils.Instance.GetReg32Value(RegistryHive.LocalMachine, Utils.Instance.PAC_Reg_Path, Consts.STR_DATADIR); //RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.Instance.PAC_Reg_Path, Consts.STR_DATADIR);
			if(string.IsNullOrEmpty(pinfo.PACDir))
				pinfo.PACDir = Consts.STR_PAC_DIR;

			pinfo.PACID = Utils.Instance.GetReg32Value(RegistryHive.LocalMachine, Utils.Instance.PAC_Reg_Path, Consts.STR_PACID);//RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.Instance.PAC_Reg_Path, Consts.STR_PACID);
			pinfo.PACVersion = Utils.Instance.GetReg32Value(RegistryHive.LocalMachine, Utils.Instance.PAC_Reg_Path, Consts.STR_Version);//RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.Instance.PAC_Reg_Path, Consts.STR_Version);
			return pinfo;
		}

		private Version SRXPro_VersionStringtoVersion(string strversion)
		{
			Regex rx = new Regex(Regex_SRXPRO_Version);
			Match match = rx.Match(strversion);
			if (!match.Success)
				return new Version("0.0.0.0");
			string str_v1 = match.Groups["V1"].Value;
			string str_v2 = match.Groups["V2"].Value;
			string str_v3 = match.Groups["V3"].Value;
			string str_v4 = match.Groups["V4"].Value;
			int V1 = 0;
			int V2 = 0;
			int V3 = 0;
			int V4 = 0;
			Int32.TryParse(str_v1, out V1);
			Int32.TryParse(str_v2, out V2);
			Int32.TryParse(str_v3, out V3);
			Int32.TryParse(str_v4, out V4);
			return new Version(V1, V2, V3, V4);
		}
	}


	internal class PACDMInfos
	{
		public string PACID { get; set;}
		public string PACDir{ get; set;}
		public string PACVersion { get; set;}
		public string ReportingDBPath
		{
			get { return System.IO.Path.Combine(PACDir, Consts.ReportingDB_FileName );}
		}

	}
	
	internal class SRXProInfos
	{
		public string ServerAppPath{ get;set;}
		public string SRXProVersion{ get; set; }
		public string ConfigurationPath{
			get{ return string.IsNullOrEmpty(ServerAppPath)? string.Empty : System.IO.Path.Combine(ServerAppPath, "Configuration");}
		}
		public Version Version{ get {
			return Commons.Utils.ParserVersion( SRXProVersion);}}
	}
	
	internal class MacAddressInfo
	{
		public string MACAddress { get; set;}
		public string IpAddress { get; set;}
		public bool LoopBack { get; set;}
		public string Version { get; set;}
		public int MacOrder { get; set; }
		public bool Active { get; set; }

	}
	
}
