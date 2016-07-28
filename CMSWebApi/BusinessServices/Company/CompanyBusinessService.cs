using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Globalization;
using PACDMModel.Model;
using System.IO;
using CMSWebApi.APIFilters;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.Company
{
	public class CompanyBusinessService : BusinessBase<ICompanyService>
	{
		public IUsersService IUsersvr{ get ;set;}

		public CompanyModel GetCompanyInfo(UserContext userctx)
		{
			int? CompanyID = IUsersvr.Get<int?>( userctx.ParentID, item => item.CompanyID);  //DataService.SelectUser(userValue.ID);
			tCMSWeb_Company tCompany = DataService.SelectCompanyInfo((int)CompanyID);
			int recordingDay= DataService.SelectRecordingDay((int)CompanyID)!= null? DataService.SelectRecordingDay((int)CompanyID).Value: 0;
			CompanyModel model = new CompanyModel
								{
									CompanyID = tCompany.CompanyID,
									CompanyName = tCompany.CompanyName,
									CompanyLogo = tCompany.CompanyLogo,
									UserID = userctx.ID,
									UpdateDate = tCompany.UpdateDate,
									NumberRecording = recordingDay
								};
			return model;			
		}

		public TransactionalModel<CompanyModel> UpdateCompanyInfo(CompanyModel model, int userID)
		{
			TransactionalModel<CompanyModel> response = new TransactionalModel<CompanyModel>();
			CompanyBusinessRules Rules = new CompanyBusinessRules(Culture);
			Rules.ValidateInput(model.CompanyName, model.CompanyLogo);
			if (!Rules.ValidationStatus)
			{
				Rules.SetTransactionInfomation(response);
				return response;
			}

			tCMSWebRegion tRegion = new tCMSWebRegion();
			tCMSWeb_Company tCompany= model.CompanyID!= 0?  DataService.SelectCompanyInfo(model.CompanyID) : new tCMSWeb_Company();
			tRegion = DataService.SelectRegion(userID);
			tRegion.RegionName = model.CompanyName;
			SetEntityCompany(ref tCompany, model);
			if (DataService.UpdateCompanyInfo(tCompany) == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return response;
			}

				tCMSSystemConfig tConfig = SetEntityConfig(model);
				if (DataService.UpdateConfig(tConfig) == null)
				{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return response;
				}

				if (model.CompanyID == 0)
				{
					if (DataService.UpdateRegion(tRegion) == null)
					{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					return response;
				}
			}			
			
			model.CompanyID = tCompany.CompanyID;
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			response.Data = model;
			return response;
		}

		private tCMSSystemConfig SetEntityConfig(CompanyModel model)
		{
			tCMSSystemConfig tConfig = DataService.SelectRecordingDay(model.CompanyID)!= null ? DataService.SelectRecordingDay(model.CompanyID) : new tCMSSystemConfig();
			tConfig.Name = Consts.RECORDING_DAY;
			tConfig.Label = Consts.RECORDING_DAY;
			tConfig.Value = model.NumberRecording;
			tConfig.CompanyID = model.CompanyID;

			return tConfig;
		}

		private void SetEntityCompany(ref tCMSWeb_Company tCompany, CompanyModel model)
		{
			if (tCompany == null)
				tCompany = new tCMSWeb_Company();

			tCompany.CompanyID = model.CompanyID;
			tCompany.CompanyName = model.CompanyName;
			tCompany.CompanyLogo = model.CompanyLogo;
			tCompany.UpdateDate = model.UpdateDate;
		}

		internal int  CMSSystemRecordingDayConfig(int companyID)
		{
			tCMSSystemConfig cfg = CMSSystemConfig(companyID);
			return cfg != null ? cfg.Value : AppSettings.AppSettings.Instance.RecordDayExpected;
		}
		internal tCMSSystemConfig CMSSystemConfig(int companyID)
		{
			tCMSSystemConfig sysConfig = companyID > 0 ? DataService.SelectRecordingDay(companyID) : null;
			return sysConfig;
		}
	}
}
