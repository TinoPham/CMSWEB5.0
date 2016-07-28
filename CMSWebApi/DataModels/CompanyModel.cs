using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{
	public class CompanyModel
	{
		public int CompanyID { get; set; }
		public string CompanyName { get; set; }
		public byte[] CompanyLogo { get; set; }
		public Nullable<System.DateTime> UpdateDate { get; set; }
		public int UserID { get; set; }
		public int NumberRecording { get; set; }
	}
}
