using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSSVR.Models.Api.Configuration
{
	public class svrConfigmodels
	{
		public Int16 ConvertInterval { get;set;}
		public bool Authenticate{ get;set;}
		public List<DVRAuth> DVRAuths{ get;set;}
		public bool MatchAllinfo{ get;set;}
		public Int64 ConfigID { get;set;}
		public Int16 DVRKeepAlive{ get;set;}

	}
	public class DVRAuth
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public bool Checked { get; set; }
	}
}