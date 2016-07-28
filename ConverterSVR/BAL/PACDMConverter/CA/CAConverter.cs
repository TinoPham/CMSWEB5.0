using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using ConvertMessage.PACDMObjects.CA;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.PACDMConverter.CA
{
	internal class CAConverter : PACDMConvertBase
	{
		public CAConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}
		public override MessageResult ConvertData()
		{
			
			MessageResult msg_result = SaveTransact<Transact,tbl_CA_Transact>(base.MsgData.Data, CATransformMessage.Instance);

			return msg_result;
		}
	}
}
