using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{
	public class RecipientModel
	{
		public int RecipientID { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int ? CreateBy { get; set; }
		public string UUsername { get; set; }
	}

	//public class RecipientData : TransactionalInformation
	//{
	//	public RecipientModel Recipient { get; set; }
	//}
}
