using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using ConvertMessage.PACDMObjects.ATM;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.PACDMConverter.ATM
{
	internal class ATMConverter : PACDMConvertBase
	{
		public ATMConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}
		public override MessageResult ConvertData()
		{
			
			MessageResult msg_result = SaveTransact<Transact,tbl_ATM_Transact>(base.MsgData.Data, ATMTransformMessage.Instance);

			return msg_result;
		}
	}
}
