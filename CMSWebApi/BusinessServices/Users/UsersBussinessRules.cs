using CMSWebApi.DataModels;
using CMSWebApi.Resources;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using CMSWebApi.BusinessServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.BusinessServices.Users
{
	internal class UsersBussinessRules : ValidationRules
	{
		public UsersBussinessRules(CultureInfo culture) : base(culture) { }

		
	}
}
