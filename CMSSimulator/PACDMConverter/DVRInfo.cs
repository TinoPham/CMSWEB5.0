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

namespace PACDMSimulator
{
	internal class DVRInfos
	{
		private static readonly Lazy<DVRInfos> Lazy = new Lazy<DVRInfos>(() => new DVRInfos());
		public static DVRInfos Instance { get { return Lazy.Value; } }
		SRXProInfos _SRXProInfo = null;
		
		PACDMInfos _PACDMInfo = null; 
		string _SPKID = string.Empty;
		MessageDVRInfo _MsgDVRInfo = null;
		List<MacAddressInfo> _MacAddressInfos = null;

		public SRXProInfos SRXProInfo
		 { get
			{
				if( _SRXProInfo == null)
					_SRXProInfo = GetSRXProInfos();

					return _SRXProInfo;
			}
		 }

		public PACDMInfos PACDMInfo
		{ 
			get
			{ 
				if( _PACDMInfo == null)
					_PACDMInfo = GetPACDMInfos();
				return _PACDMInfo;
			}
		}
		
		public List<MacAddressInfo> MacAddressInfos
		 {
			get
			{ 
				if( _MacAddressInfos == null || _MacAddressInfos.Count == 0)
					_MacAddressInfos = GetNetworkInfo();
				return _MacAddressInfos;
			}
		}

		public string SPKID
		{
			get{
				if( string.IsNullOrEmpty(_SPKID))
					_SPKID = GetSPKID();

				return _SPKID;
			}
		}

		public MessageDVRInfo MsgDVRInfo
		{
			get{
				if( _MsgDVRInfo == null)
					_MsgDVRInfo = GetMsgDVRInfo();
				return _MsgDVRInfo;
			}
				
		}
		private DVRInfos()
		{
		}

		public void Refresh()
		{
			_MacAddressInfos = null;
			_PACDMInfo = null;
			_SRXProInfo = null;
			_SPKID = string.Empty;
			_MsgDVRInfo = null;
		}
		private MessageDVRInfo GetMsgDVRInfo()
		{
			return new MessageDVRInfo{
			HASPK = SPKID,
			MACs = MacAddressInfos.Select( item => new ConvertMessage.MacInfo{ IP_Address = item.IpAddress, IP_Version = item.Version, MAC_Address = item.MACAddress, Active = item.Active, MacOrder = item.MacOrder}).ToList(),
			Date = DateTime.Now,
			PACinfo = new PACInfo { PACID = PACDMInfo.PACID, PACVersion = PACDMInfo.PACVersion }
			};
		}
		private string GetPhysicalAddress( PhysicalAddress physAddress, char separate = ':')
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
			return  String.Empty;
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
				

				if ((NetInterface.OperationalStatus == OperationalStatus.Up)
					&& (ip.Address.AddressFamily == AddressFamily.InterNetwork || ip.Address.AddressFamily == AddressFamily.InterNetworkV6))
				{
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
				}
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
				if (Nic.OperationalStatus != OperationalStatus.Up)
					continue;
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
			srxinfo.ServerAppPath = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.SRX_Pro_Reg_Settings_Path, Consts.STR_ServerAppPath);
			srxinfo.SRXProVersion = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.SRX_Pro_Reg_Settings_Path, Consts.STR_SRX_Pro_Version); 
			return srxinfo;
		}
		
		private PACDMInfos GetPACDMInfos()
		{
			PACDMInfos pinfo = new PACDMInfos();
			pinfo.PACDir = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.PAC_Reg_Path, Consts.STR_DATADIR);
			pinfo.PACID = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.PAC_Reg_Path, Consts.STR_PACID);
			pinfo.PACVersion = RegistryUtils.GetRegValue(RegistryHive.LocalMachine, RegistryView.Registry32, Utils.PAC_Reg_Path, Consts.STR_Version);
			return pinfo;
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
