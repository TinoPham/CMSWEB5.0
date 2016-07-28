using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ICompanyService
	{
		tCMSWeb_Company SelectCompanyInfo(int companyID);
		tCMSSystemConfig SelectRecordingDay(int companyID);
		tCMSSystemConfig UpdateConfig(tCMSSystemConfig config);
		tCMSWeb_Company UpdateCompanyInfo(tCMSWeb_Company model);
		tCMSWeb_UserList SelectUser(int userID);
		tCMSWebRegion SelectRegion(int userID);
		tCMSWebRegion UpdateRegion(tCMSWebRegion model);
	}
}

