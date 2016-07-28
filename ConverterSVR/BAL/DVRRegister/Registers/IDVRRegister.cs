using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;

namespace ConverterSVR.BAL.DVRRegister
{
	internal interface IDVRRegister
	{
		IEnumerable<int> FindDVR(MessageDVRInfo dvrInfo, PACDMModel.PACDMDB pacModel);
	}
}
