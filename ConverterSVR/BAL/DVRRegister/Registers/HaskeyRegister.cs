using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRRegister
{
	internal class HaskeyRegister : IDVRRegister
	{
		public IEnumerable<int> FindDVR(MessageDVRInfo dvrinfo, PACDMModel.PACDMDB pacModel)
		{
            if (string.IsNullOrEmpty(dvrinfo.HASPK))
            {
                return Enumerable.Empty<int>();
            }
			var DvrAddress = pacModel.Query<tDVRAddressBook>().AsQueryable();
			IEnumerable<int> KDVRs = DvrAddress.Where(item => string.Compare(item.HaspLicense, dvrinfo.HASPK) == 0).Select(match => match.KDVR);
			return KDVRs;
		}
	}
}
