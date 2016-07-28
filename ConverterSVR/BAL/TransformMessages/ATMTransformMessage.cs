using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using ConvertMessage;
using ConvertMessage.PACDMObjects.ATM;
using PACDMModel.Model;

namespace ConverterSVR.BAL.TransformMessages
{
	internal class ATMTransformMessage :Commons.SingletonClassBase<ATMTransformMessage>, ITransformMessage<Transact, tbl_ATM_Transact>
	{
		//private static readonly Lazy<ATMTransformMessage> Lazy = new Lazy<ATMTransformMessage>(() => new ATMTransformMessage());

		//public static ATMTransformMessage Instance { get { return Lazy.Value; } }

		public tbl_ATM_Transact TransForm(Transact input, MessageDVRInfo DVRInfo)
		{
			tbl_ATM_Transact output = new tbl_ATM_Transact();
			output.BusinessDate = input.BusinessDate;
			output.DVRDate = Commons.Utils.toSQLDate(input.DVRDate);
			output.T_0TransNB = input.T_0TransNB;
			output.T_AcctBalance = input.T_AcctBalance;
			output.T_CameraNB = input.T_CameraNB;
			output.T_CardNB = input.T_CardNB;
			output.T_PACID = DVRInfo.KDVR;
			output.T_TransAmount = input.T_TransAmount;
			output.T_TransCode = input.T_TransCode;
			output.T_TransTermFee = input.T_TransTermFee;
			output.T_TransTotal = input.T_TransTotal;
			output.T_TransType = input.T_TransType;
			output.TransactKey = input.TransactKey;
			output.TransDate = Commons.Utils.toSQLDate( input.TransDate);
			if( input.ExStrings != null)
				input.ExStrings.ForEach(
					item => output.tbl_ATM_Trans_XString.Add( new tbl_ATM_Trans_XString{ tbl_ATM_Transact = output, ExtraNameID = item.Key, XString_ID = item.Value} )
				);
			
			return output;
		}
	}
}
