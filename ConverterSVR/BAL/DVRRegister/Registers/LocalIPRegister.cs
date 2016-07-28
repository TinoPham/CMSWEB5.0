using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using PACDMModel.Model;
namespace ConverterSVR.BAL.DVRRegister
{
	internal class LocalIPRegister : IDVRRegister
	{
		private const char MAC_SEPERATOR_SEMICOLON = ';';
		private const char MAC_SEPERATOR_MINUS = '-';
		public IEnumerable<int> FindDVR(MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacModel)
		{
			var dbnetCards = pacModel.Query<tDVRNetworkCard>().AsQueryable();
			//Anh Huynh, Updated Feb 05, 2015 
			//IEnumerable<int> KDVRs = from dbcard in dbnetCards
			//						 from card in dvrinfo.MACs
			//						 where ValidIpAddress(dbcard.IPv4List, card.IP_Address) == true
			//						 select dbcard.KDVR;
			char[] split = new char[2] { MAC_SEPERATOR_MINUS, MAC_SEPERATOR_SEMICOLON };
			List<string> dvrIPs = dvrinfo.MACs.Select(x => x.IP_Address).ToList();
			IEnumerable<int> KDVRs = from dbcard in dbnetCards
									 from ip in dvrIPs
									 where (!String.IsNullOrEmpty(dbcard.IPv4List) && !String.IsNullOrEmpty(ip) && dbcard.IPv4List.Split(split).FirstOrDefault(item=>item.Equals(ip))!=null )//ValidIpAddress(dbcard.IPv4List, ip) == true // Will fixed later
									 select dbcard.KDVR;

			return KDVRs;
		}
		private bool ValidIpAddress( string cmsiplist, string ipaddress)
		{
			if( string.IsNullOrEmpty(ipaddress) || string.IsNullOrEmpty(cmsiplist))
				return false;

			string[] ip_subnet = cmsiplist.Split(new char[] { Consts.cms_ip_split });

			if( ip_subnet.Length == 0)
				return false;
			string found = ip_subnet.FirstOrDefault(item => string.Compare(item, ipaddress, true) == 0 || item.ToLower().StartsWith(ipaddress.ToLower() + Consts.cms_ip_subnet_split.ToString()));
			return !string.IsNullOrEmpty(found);
		}
	}
}
