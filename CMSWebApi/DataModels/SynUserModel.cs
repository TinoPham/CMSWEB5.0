using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class SynUserModel
	{
		public int SynID { get; set; }
		public string ServerIP { get; set; }
		public string UserID { get; set; }
		public string PassWord { get; set; }
		public bool isSSL { get; set; }
		public int Interval { get; set; }
		public string Time { get; set; }
		public Nullable<System.DateTime> LastSyn { get; set; }
		public bool isEnable { get; set; }
		public bool isForceUpdate { get; set; }
		public int SynType { get; set; }
		public string SynName { get; set; }
		public int CreateBy { get; set; }
		public string LastSynresult { get; set; }
		public string UUsername { get; set; }
	}

	//public class SynUserData : TransactionalInformation
	//{
	//	public SynUserModel SynUser { get; set; }
	//}

	public class SynUserTypeModel//: TransactionalInformation
	{
		public int SynID { get; set; }
		public string SynName { get; set; }
		public string SynDes { get; set; }
		public string SynConnection { get; set; }
	}
}
