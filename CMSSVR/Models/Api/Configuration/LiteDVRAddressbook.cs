using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSSVR.Models.Api.Configuration
{
	public class LiteDVRAddressbook
	{
		public  long KDVR{ get;set;}
		public string DVRGuid{ get;set;}
		public string ServerIP{ get;set;}
		public string DVRAlias{ get;set;}
		public DateTime? FirstAccess{ get;set;} 
		public short? CMSMode{ get; set;}
		public string HaspLicense { get; set; }
	}
}