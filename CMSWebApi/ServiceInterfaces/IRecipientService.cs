using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IRecipientService
	{
		//tCMSWebRecipients AddRecipient(tCMSWebRecipients recipient);
		//tCMSWebRecipients UpdateRecipient(tCMSWebRecipients recipient);
		////IQueryable<tCMSWebRecipients> SelectRecipient(string email);
		//IQueryable<Tout> SelectRecipient<Tout>(string email, Expression<Func<tCMSWebRecipients, Tout>> selector, string[]includes) where Tout : class;
		//IQueryable<Tout> SelectAllRecipient<Tout>(int Createdby, Expression<Func<tCMSWebRecipients, Tout>> selector, string [] includes) where Tout : class;
		//Tout SelectRecipient<Tout>(int recipientID, Expression<Func<tCMSWebRecipients, Tout>> selector, string [] includes) where Tout : class;
		//tCMSWebRecipients SelectRecipient(int recipientID);
		bool DeleteRecipient(List<int> recipientIDs);

		//IQueryable<tCMSWebRecipients> SelectAllRecipient(int userID);
		
		//tCMSWeb_UserList SelectUser(int userID);
	}
}

