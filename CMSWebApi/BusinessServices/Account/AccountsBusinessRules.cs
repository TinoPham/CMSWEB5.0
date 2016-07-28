using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.Account
{
	internal class AccountsBusinessRules : ValidationRules
	{
		public AccountsBusinessRules( CultureInfo culture):base(culture){}

		public void ValidateLogin( string username, string password)
		{
			string 	friendlyName = string.Empty;
			if( string.IsNullOrEmpty(username))
			{
				friendlyName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_USERNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), friendlyName);
				AddValidationError(friendlyName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}
			if( string.IsNullOrEmpty(password))
			{
				friendlyName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_PASSWORD, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), friendlyName);
				AddValidationError(friendlyName, errorMessage, CMSWebError.REQUIRED_FIELD);
			
			}
			
		}
	}
}
