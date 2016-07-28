using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CMSSVR.Models.Api.Configuration
{
	public class DBConfigModel
	{
		[DisplayName("Trusted connection")]
		public bool Trusted{ get;set;}
		[DisplayName("Data base")]
		public string DBName { get;set;}
		[DisplayName("SQL Server")]
		public string Server{ get;set;}
		[DisplayName("USer ID")]
		public string UserID { get; set; }
		[DisplayName("Password")]
		public string Password{ get; set;}
	}
}