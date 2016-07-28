using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSSVR.Models.Api.Configuration
{
	public class UserModel
	{
		
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public int UserID { get; set; }

	}
	public class UserEditModel : UserModel
	{
		public string ConfirmPassword { get; set; }
	}
}