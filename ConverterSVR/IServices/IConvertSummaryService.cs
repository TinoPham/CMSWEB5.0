using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;

namespace ConverterSVR.IServices
{
	public interface IConvertSummaryService
	{
		ConvertMessage.MessageResult SummaryTransaction(TransactionSummary Summary, MessageDVRInfo DVRInfo);
	}
}
