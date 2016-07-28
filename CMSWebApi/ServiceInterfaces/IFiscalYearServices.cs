using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface  IFiscalYearServices
	{
        tCMSWeb_FiscalYear GetFiscalYearInfo(int UserID, bool isClone = false);
		tCMSWeb_FiscalYear GetFiscalYearInfo(UserContext userID, DateTime searchDate, bool isClone = false);
		tCMSWeb_FiscalYear Add(tCMSWeb_FiscalYear fiscalYear);
		tCMSWeb_FiscalYear Update(tCMSWeb_FiscalYear fiscalYear);
		List<FiscalPeriod> GetFiscalPeriods(tCMSWeb_FiscalYear fyInfo, DateTime date, DateTime fyStart);
		FiscalWeek GetFiscalWeek(tCMSWeb_FiscalYear fyInfo, DateTime searchDate, DateTime fyStartDate);
	}
}
