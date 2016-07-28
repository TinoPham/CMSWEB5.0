using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;
using PACDMModel;
using SVRDatabase;

namespace ConverterSVR.Services
{
	public class ConvertSummaryService : IServices.IConvertSummaryService
	{
		PACDMModel.PACDMDB  PACModel;
		SVRDatabase.SVRManager LogModel;
		public ConvertSummaryService(PACDMModel.PACDMDB PACmodel, SVRDatabase.SVRManager Logmodel)
		{
			PACModel = PACmodel;
			LogModel = Logmodel;

		}
		
		public ConvertMessage.MessageResult SummaryTransaction(TransactionSummary Summary, MessageDVRInfo DVRInfo)
		{
			var ret = new BAL.PACDMConverter.POS.POSSummaryConverter(PACModel, LogModel).POSTransactionSummary(Summary, DVRInfo);
			return ret;
		}
	}
}
