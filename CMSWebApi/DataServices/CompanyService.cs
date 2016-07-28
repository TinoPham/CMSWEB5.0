using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;


namespace CMSWebApi.DataServices
{
	public partial class CompanyService : ServiceBase, ICompanyService
	{
		public CompanyService(PACDMModel.Model.IResposity model) : base(model) { }
		public CompanyService(ServiceBase svrbase) : base(svrbase.DBModel) { }

		public tCMSWeb_Company SelectCompanyInfo(int companyID)
		{
			return DBModel.Query<tCMSWeb_Company>(i => i.CompanyID == companyID).FirstOrDefault();
		}


		public tCMSSystemConfig SelectRecordingDay(int companyID)
		{
			return DBModel.FirstOrDefault<tCMSSystemConfig>(i => i.CompanyID == companyID && i.Name == CMSWebApi.Utils.Consts.RECORDING_DAY);
		}

		public tCMSSystemConfig UpdateConfig(tCMSSystemConfig config)
		{
			if(config.ConfigNo!=0)
			{
				DBModel.Update<tCMSSystemConfig>(config);
				return DBModel.Save() >= 0 ? config : null;
			}
			else
			{
				DBModel.Insert<tCMSSystemConfig>(config);
			return DBModel.Save() > 0 ? config : null;
		}
		}

		public tCMSWeb_Company UpdateCompanyInfo(tCMSWeb_Company model)
		{
			if (model.CompanyID != 0)
			{
				DBModel.Update<tCMSWeb_Company>(model);
				return DBModel.Save() >= 0 ? model : null;
			}
			else
			{
				DBModel.Insert<tCMSWeb_Company>(model);
			return DBModel.Save() > 0 ? model : null;
		}
		}

		public tCMSWebRegion UpdateRegion(tCMSWebRegion model)
		{
			DBModel.Update<tCMSWebRegion>(model);
			return DBModel.Save() > 0 ? model : null;
		}

		public tCMSWebRegion SelectRegion(int userID)
		{
			return DBModel.FirstOrDefault<tCMSWebRegion>(i => i.UserKey == userID && i.RegionParentID == null);

		}

		public tCMSWeb_UserList SelectUser(int userID)
		{
			tCMSWeb_UserList model = DBModel.Query<tCMSWeb_UserList>(i => i.UserID == userID).FirstOrDefault();
			return model;
		}
	}
}
