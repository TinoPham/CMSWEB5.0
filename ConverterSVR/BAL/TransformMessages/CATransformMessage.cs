using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using ConvertMessage.PACDMObjects.CA;
using PACDMModel.Model;

namespace ConverterSVR.BAL.TransformMessages
{
	internal class CATransformMessage: Commons.SingletonClassBase<CATransformMessage>, ITransformMessage<Transact, tbl_CA_Transact>
	{
		//private static readonly Lazy<CATransformMessage> Lazy = new Lazy<CATransformMessage>(() => new CATransformMessage());

		//public static CATransformMessage Instance { get { return Lazy.Value; } }

		public tbl_CA_Transact TransForm(Transact input, MessageDVRInfo DVRInfo)
		{
			tbl_CA_Transact output = new tbl_CA_Transact();

			output.DVRDate = Commons.Utils.toSQLDate( input.DVRDate);
			output.T_Batch = input.T_Batch;
			output.T_CameraNB = input.T_CameraNB;
			output.T_Card = input.T_Card;
			output.T_DevName = input.T_DevName;
			output.T_FullName = input.T_FullName;
			output.T_PACID = DVRInfo.KDVR;
			output.T_SiteID = input.T_SiteID;
			output.T_TranType = input.T_TranType;
			output.T_UnitID = input.T_UnitID;
			output.TransDate = Commons.Utils.toSQLDate( input.TransDate);
			output.Transact_Key = input.Transact_Key;
			if( input.ExStrings != null)
				input.ExStrings.ForEach
				(
					item => output.tbl_CA_Trans_XString.Add( new tbl_CA_Trans_XString{ tbl_CA_Transact = output, ExtraNameID = item.Key, XString_ID = item.Value})
				);
			return output;
		}
	}
}
