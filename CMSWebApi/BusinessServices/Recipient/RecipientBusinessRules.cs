using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.Recipient
{
	internal class RecipientBusinessRules : ValidationRules
	{
		public RecipientBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string email, string fName)
		{
			if (string.IsNullOrEmpty(email))
			{
				string strEmail = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_EMAIL, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), strEmail);
				AddValidationError(strEmail, errorMessage, CMSWebError.REQUIRED_FIELD);
			}

			if (string.IsNullOrEmpty(fName))
			{
				string strFName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_FNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), strFName);
				AddValidationError(strFName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}			
		}
	}
}
