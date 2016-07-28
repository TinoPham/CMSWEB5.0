using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.SynUser
{
	internal class SynUserBusinessRules : ValidationRules
	{
		public SynUserBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string severIP, string userName, string password)
		{
			string friendlyName = string.Empty;
			if (string.IsNullOrEmpty(severIP))
			{
				friendlyName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_SERVERIP, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), friendlyName);
				AddValidationError(friendlyName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}
			if (string.IsNullOrEmpty(userName))
			{
				friendlyName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_USERNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), friendlyName);
				AddValidationError(friendlyName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}
			if (string.IsNullOrEmpty(password))
			{
				friendlyName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_PASSWORD, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), friendlyName);
				AddValidationError(friendlyName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}	
		}
	}
}
