using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using System.Linq.Expressions;


namespace CMSWebApi.DataServices
{
	public partial class RecipientService : ServiceBase, IRecipientService
	{

		public RecipientService(PACDMModel.Model.IResposity model) : base(model) { }

		public RecipientService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		/// <summary>
		/// Insert Update Delete tCMSWebRecipients
		/// </summary>

		//public tCMSWebRecipients AddRecipient(tCMSWebRecipients recipient)
		//{
		//	DBModel.Insert<tCMSWebRecipients>(recipient);
		//	return DBModel.Save() > 0 ? recipient : null;
		//}

		//public tCMSWebRecipients UpdateRecipient(tCMSWebRecipients recipient)
		//{
		//	DBModel.Update<tCMSWebRecipients>(recipient);
		//	return DBModel.Save() >= 0 ? recipient : null;
		//}

		public bool DeleteRecipient(List<int> recipientIDs)
		{
		//	var abc = DBModel.DeleteWhere<tCMSWebRecipients>(recipient => recipientIDs.Contains(recipient.RecipientID));
		//	return DBModel.Save() > 0 ? true : false;
			return true;
		}

		//public IQueryable<Tout> SelectAllRecipient<Tout>(int Createdby, Expression<Func<tCMSWebRecipients, Tout>> selector, string [] includes) where Tout : class
		//{
		//	IQueryable<Tout> result = Query<tCMSWebRecipients, Tout>(i => i.CreateBy == Createdby, selector, includes);
		//	return result;
		//}
		//public IQueryable<tCMSWebRecipients> SelectAllRecipient(int userID)
		//{
		//	return DBModel.Query<tCMSWebRecipients>(i => i.CreateBy == userID);
		//}

		//public IQueryable<Tout> SelectRecipient<Tout>(string email, Expression<Func<tCMSWebRecipients, Tout>> selector, string [] includes) where Tout : class
		//{
		//	return Query<tCMSWebRecipients, Tout>(i => i.Email == email, selector, includes);
		//}

		////public IQueryable<tCMSWebRecipients> SelectRecipient(string email)
		////{
		////	return DBModel.Query<tCMSWebRecipients>(i => i.Email == email);
			
		////}
		//public Tout SelectRecipient<Tout>(int recipientID, Expression<Func<tCMSWebRecipients, Tout>> selector, string [] includes) where Tout : class
		//{
		//	Tout result = FirstOrDefault<tCMSWebRecipients, Tout>(i => i.RecipientID == recipientID, selector, includes);
		//	return result;
		//}
		//public tCMSWebRecipients SelectRecipient(int recipientID)
		//{
		//	tCMSWebRecipients model = DBModel.FirstOrDefault<tCMSWebRecipients>(i => i.RecipientID == recipientID);// DBModel.Query<tCMSWebRecipients>(i => i.RecipientID == recipientID).FirstOrDefault();
		//	return model;
		//}

		///
		///User
		///
		//public tCMSWeb_UserList SelectUser(int userID)
		//{
		//	tCMSWeb_UserList model = DBModel.FirstOrDefault<tCMSWeb_UserList>(i=> i.UserID==userID); //DBModel.Query<tCMSWeb_UserList>(i=> i.UserID==userID).FirstOrDefault();
		//	return model;
		//}
	}
}
