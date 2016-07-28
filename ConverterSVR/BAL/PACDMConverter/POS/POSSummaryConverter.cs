using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage.PACDMObjects.POS;

namespace ConverterSVR.BAL.PACDMConverter.POS
{
	internal class POSSummaryConverter : ConvertBase
	{
		public POSSummaryConverter(PACDMModel.PACDMDB PACModel, SVRDatabase.SVRManager LogModel )
		{
			base.LogDB = LogDB;
			base.PACDB = PACModel;
		}
		public ConvertMessage.MessageResult POSTransactionSummary(TransactionSummary Summary, ConvertMessage.MessageDVRInfo DVRInfo)
		{
			if( Summary == null)
			{
				return new ConvertMessage.MessageResult { ErrorID = Commons.ERROR_CODE.SERVICE_CANNOT_PARSER_DATA, Data = "TransactionSummary cannot be null" };
			}
			if(DVRInfo == null || DVRInfo.KDVR <= 0)
			{
				return new ConvertMessage.MessageResult { ErrorID = Commons.ERROR_CODE.DVR_INVALID_INFO, Data = "DVR info is invalid" };
			}
			 var obj = CMSWebApi.Wrappers.Wrapper.Instance.DBWareHouse.POSTransactionSummary(base.PACDB as PACDMModel.Model.IResposity, DVRInfo.KDVR,Summary.Date, Summary.Hour, Summary.TotalTrans, Summary.TotalAmount );
			 if( obj == null)
				return new ConvertMessage.MessageResult{ ErrorID = Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED};
			return new ConvertMessage.MessageResult{ ErrorID = Commons.ERROR_CODE.OK};

		}
	}
}
