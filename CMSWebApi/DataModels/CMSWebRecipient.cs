using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{
	public class CMSWebRecipient
	{
		public int RecipientID { get; set; }
		public Nullable<int> CreateBy { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		//public void Import(tCMSWebRecipients recipt)
		//{
		//	RecipientID = recipt.RecipientID;
		//	CreateBy = recipt.CreateBy;
		//	FirstName = recipt.FirstName;
		//	LastName = recipt.LastName;
		//	Email = recipt.Email;
		//}

		//public tCMSWebRecipients Export()
		//{
		//	tCMSWebRecipients recipt = new tCMSWebRecipients();
		//	recipt.RecipientID = RecipientID;
		//	recipt.CreateBy = CreateBy;
		//	recipt.FirstName = FirstName;
		//	recipt.LastName = LastName;
		//	recipt.Email = Email;

		//	return recipt;
		//}
	}
}
