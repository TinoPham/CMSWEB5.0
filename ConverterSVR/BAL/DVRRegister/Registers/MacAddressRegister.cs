using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRRegister
{
	internal class MacAddressRegister : IDVRRegister
	{
		//private const string MAC_SEPERATOR_COLON= ":";
		//private const string MAC_SEPERATOR_MINUS = "-";
		public IEnumerable<int> FindDVR(MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacModel)
		{
			var dbnetCards = pacModel.Query<tDVRNetworkCard>();//.AsQueryable();
			
			//Anh Huynh, Updated Feb 05, 2015 
			//IEnumerable<int> KDVRs = from dbcard in dbnetCards
			//			from card in dvrinfo.MACs
			//			where  string.Compare(dbcard.MACAddress, card.MAC_Address, true) == 0
			//			select dbcard.KDVR;

			List<string> dvrMACs = dvrinfo.MACs.Select(x => x.MAC_Address).Distinct().ToList();
			//Nghi mark July 30 2015 begin
			#region
			//List<string> macs = new List<string>();
			//dvrMACs.ForEach(item =>
			//{
			//	var _item = item;
			//	while (_item.Contains(MAC_SEPERATOR_COLON))
			//	{
			//		_item = _item.Replace(MAC_SEPERATOR_COLON, MAC_SEPERATOR_MINUS);
			//	}
			//	macs.Add(_item);

			//});
			//dvrMACs = dvrMACs.Concat(macs).ToList();
			#endregion
			//Nghi mark July 30 2015 end

			IEnumerable<int> KDVRs = from dbcard in dbnetCards
									 from mac in dvrMACs
									 where string.Compare(dbcard.MACAddress, mac, true) == 0
						select dbcard.KDVR;
            if (!KDVRs.Any())
            {
               KDVRs =  from dbcard in pacModel.Query<tDVRAddressBook>()
									 from mac in dvrMACs
									 where string.Compare(dbcard.DVRGuid, mac, true) == 0
	                    select dbcard.KDVR;
               
            }
			return KDVRs;
			
			//return KDVRs;
		}
	}
}
