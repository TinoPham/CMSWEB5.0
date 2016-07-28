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

namespace CMSWebApi.BusinessServices.Recipient
{
	public class RecipientBusinessService : BusinessBase<IRecipientService>
	{
		public IUsersService IUser { get; set; }
		//public IQueryable<RecipientModel> GetAllRecipient(UserContext usercontext)
		//{
		//	int userID = usercontext.ParentID; //(!userValue.Createdby.HasValue ? userValue.ID : userValue.Createdby.Value);
		//	IQueryable<tCMSWebRecipients> recipient = DataService.SelectAllRecipient<tCMSWebRecipients>(userID, item => item, null); //DataService.SelectAllRecipient(userID);
		//	//tCMSWeb_UserList user = DataService.SelectUser(userID);
		//	string tUser = IUser.Get<string>(userID, item => string.Format("{0} {1}", item.UFirstName, item.ULastName));//  DataService.SelectUser(userID);
		//	IQueryable<RecipientModel> models = (from item in recipient
		//							   select new RecipientModel
		//							   {
		//								   RecipientID = item.RecipientID,
		//								   Email = item.Email,
		//								  CreateBy= item.CreateBy, LastName=item.LastName, FirstName= item.FirstName,
		//								   UUsername = tUser//user.UUsername
		//							   });
		//	return models;			
		//}

		//public TransactionalModel<RecipientModel> DeleteRecipient(List<int> reciptientIDs)
		//{
		//	TransactionalModel<RecipientModel> recipientmodel = new TransactionalModel<RecipientModel>();
		//	recipientmodel.ReturnStatus = false;
		//	recipientmodel.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());

		//	if (DataService.DeleteRecipient(reciptientIDs))
		//	{
		//		recipientmodel.ReturnStatus = true;
		//		recipientmodel.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
		//	}
			
		//	return recipientmodel;
		//}

		//public TransactionalModel<RecipientModel> UpdateRecipient(RecipientModel model)
		//{
		//	//RecipientData data = new RecipientData();
		//	TransactionalModel<RecipientModel> returnmodel = new TransactionalModel<RecipientModel>();

		//	RecipientBusinessRules Rules = new RecipientBusinessRules(Culture);
		//	Rules.ValidateInput(model.Email, model.FirstName);
		//	if (!Rules.ValidationStatus)
		//	{
		//		Rules.SetTransactionInfomation(returnmodel);
		//		return returnmodel;
		//	}

		//	returnmodel.ReturnStatus = true;
		//	if (CheckRegistExist(model.Email, model.RecipientID))
		//	{
		//		returnmodel.ReturnStatus = false;
		//		returnmodel.ReturnMessage.Add(CMSWebError.EMAIL_EXIST_MSG.ToString());
		//	}
		//	else
		//	{
		//		tCMSWebRecipients tRecipient = new tCMSWebRecipients();

		//		if (model.RecipientID != 0)
		//		{
		//			tRecipient = DataService.SelectRecipient < tCMSWebRecipients >( model.RecipientID, item => item, null); //DataService.SelectRecipient(model.RecipientID);
		//			SetEntityRecipient(ref tRecipient, model);
		//			if (DataService.UpdateRecipient(tRecipient) == null)
		//			{
		//				returnmodel.ReturnStatus = false;
		//				returnmodel.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
		//			}
		//		}
		//		else
		//		{
		//			SetEntityRecipient(ref tRecipient, model);
		//			if (DataService.AddRecipient(tRecipient) == null)
		//			{
		//				returnmodel.ReturnStatus = false;
		//				returnmodel.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
		//			}
		//		}
		//		if( tRecipient != null)
		//			model.RecipientID = tRecipient.RecipientID;
				
		//	}

		//	returnmodel.Data = model;
		//	return returnmodel;
		//}

		//private bool CheckRegistExist(string email, int recipientID)
		//{
		//	IQueryable<tCMSWebRecipients> tRecipient = DataService.SelectRecipient<tCMSWebRecipients>(email, item=> item, null); //DataService.SelectRecipient(email);
		//	//check insert
		//	if (recipientID == 0)
		//		return (tRecipient.Count() > 0) ? true : false;
		//	//check update
		//	else
		//	{
		//		var name = from item in tRecipient
		//				   where item.RecipientID != recipientID
		//				   select item;
		//		return name.Count() > 0 ? true : false;
		//	}
		//}

		//public void SetEntityRecipient(ref tCMSWebRecipients tRecipient, RecipientModel model)
		//{
		//	if (tRecipient == null)
		//		tRecipient = new tCMSWebRecipients();

		//	tRecipient.RecipientID = model.RecipientID;
		//	tRecipient.Email = model.Email;
		//	tRecipient.CreateBy = model.CreateBy;
		//	tRecipient.FirstName = model.FirstName;
		//	tRecipient.LastName = model.LastName;
		//}
	}
}
