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

namespace CMSWebApi.BusinessServices.SynUser
{
	public class SynUserBusinessService: BusinessBase<ISynUserService>
	{
		public IUsersService IUser { get; set; }

		public IQueryable<SynUserModel> GetAllSynUser(UserContext userValue)
		{
			int userID = userValue.ParentID; //(!userValue.Createdby.HasValue ? userValue.ID : userValue.Createdby.Value);
			string tUser = IUser.Get<string>(userID, item => string.Format("{0} {1}", item.UFirstName, item.ULastName));//  DataService.SelectUser(userID);

			IQueryable<tCMSWeb_SynUser> SynUser = DataService.SelectAllSynUser < tCMSWeb_SynUser >(userValue.ID, item=> item, null); //DataService.SelectAllSynUser(userID);
			IQueryable<tCMSWeb_SynUser_Types> SynUserType = DataService.SelectSynUserType<tCMSWeb_SynUser_Types>( item=> item, null); //DataService.SelectSynUserType();
			//tCMSWeb_UserList user = DataService.SelectUser(userID);
			IQueryable<SynUserModel> models = (from item in SynUser
										join types in SynUserType on item.SynType equals types.SynID	
										  select new SynUserModel {SynID= item.SynID, ServerIP= item.ServerIP, UserID= item.UserID,
										  PassWord= item.PassWord, isSSL=item.isSSL, Interval= item.Interval, Time= item.Time, LastSyn= item.LastSyn, 
										  isEnable=item.isEnable, isForceUpdate= item.isForceUpdate, SynType= item.SynType, SynName= types.SynName,
										  CreateBy= item.CreateBy, LastSynresult= item.LastSynresult, UUsername= tUser});
			return models;
		}


		public IQueryable<SynUserTypeModel> GetAllSynUserType()
		{
			IQueryable<SynUserTypeModel> SynUserType = DataService.SelectSynUserType<SynUserTypeModel>(item => new SynUserTypeModel { SynID = item.SynID, SynName = item.SynName, SynDes = item.SynDes, SynConnection = item.SynConnection }, null);//DataService.SelectSynUserType();
			return SynUserType;
			//SynUserTypeModel[] models = (from item in SynUserType
			//							 select new SynUserTypeModel { SynID= item.SynID, SynName= item.SynName, SynDes= item.SynDes, SynConnection= item.SynConnection }).ToArray();
			//return models;
		}

		public bool DeleteSynUser(int SynID)
		{
			bool result = DataService.DeleteSynUser(SynID);
			return result;
		}

		public TransactionalModel<SynUserModel> UpdateSynUser(SynUserModel model)
		{
			TransactionalModel<SynUserModel> returnmodel = new TransactionalModel<SynUserModel>();
			//SynUserData data = new SynUserData();

			SynUserBusinessRules Rules = new SynUserBusinessRules(Culture);
			Rules.ValidateInput(model.ServerIP, model.SynName, model.PassWord);
			if (!Rules.ValidationStatus)
			{
				Rules.SetTransactionInfomation(returnmodel);
				return returnmodel;
			}

			returnmodel.ReturnStatus = true;

			if (CheckRegistExist(model.ServerIP, model.SynType, model.SynID))
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(CMSWebError.LDAP_IP_EXIST.ToString());
			}
			else
			{
				tCMSWeb_SynUser tSynUser = new tCMSWeb_SynUser();

				if (model.SynID != 0)
				{
					tSynUser = DataService.SelectSynUser(model.SynID);
					SetEntitySynUSer(ref tSynUser, model);
					if (DataService.UpdateSynUser(tSynUser) == null)
					{
						returnmodel.ReturnStatus = false;
						returnmodel.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					}
				}
				else
				{
					SetEntitySynUSer(ref tSynUser, model);
					if (DataService.AddSynUser(tSynUser) == null)
					{
						returnmodel.ReturnStatus = false;
						returnmodel.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
					}
				}
				
			}
			returnmodel.Data = model;
			return returnmodel;
		}

		private bool CheckRegistExist(string ServerIP, int SynType, int SynID)
		{
			IQueryable<tCMSWeb_SynUser> tSynUser = DataService.SelectSynUser<tCMSWeb_SynUser>(ServerIP, SynType, item=> item, null); //DataService.SelectSynUser(ServerIP, SynType);
			//check insert
			if (SynID == 0)
				return (tSynUser.Count() > 0) ? true : false;
			//check update
			else
			{
				var name = from item in tSynUser
						   where item.SynID != SynID
						   select item;
				return name.Count() > 0 ? true : false;
			}
		}


		public void SetEntitySynUSer(ref tCMSWeb_SynUser tSynUser, SynUserModel model)
		{
			if (tSynUser == null)
				tSynUser = new tCMSWeb_SynUser();

			tSynUser.SynID = model.SynID;
			tSynUser.ServerIP = model.ServerIP;
			tSynUser.UserID = model.UserID;
			tSynUser.PassWord = model.PassWord;
			tSynUser.isSSL = model.isSSL;
			tSynUser.Interval = model.Interval;
			tSynUser.Time = model.Time;
			tSynUser.LastSyn = model.LastSyn;
			tSynUser.isEnable = model.isEnable;
			tSynUser.isForceUpdate = model.isForceUpdate;
			tSynUser.SynType = model.SynType;
			tSynUser.CreateBy = model.CreateBy;
			tSynUser.LastSynresult = model.LastSynresult;

		}		
	}
}
