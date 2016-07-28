using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.Company
{
	internal class CompanyBusinessRules : ValidationRules
	{
		public CompanyBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string companyName, byte[] companyLogo)
		{
			string 	company = string.Empty;
			if (string.IsNullOrEmpty(companyName))
			{
				company = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_COMPANYNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), company);
				AddValidationError(company, errorMessage, CMSWebError.REQUIRED_FIELD);
			}						
		}
	}
}
