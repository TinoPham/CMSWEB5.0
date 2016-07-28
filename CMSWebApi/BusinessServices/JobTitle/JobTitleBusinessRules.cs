using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.JobTitle
{
	internal class JobtitleBusinessRules : ValidationRules
	{
		public JobtitleBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string username)
		{
			string 	jobName = string.Empty;
			if( string.IsNullOrEmpty(username))
			{
				jobName = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_JOBNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), jobName);
				AddValidationError(jobName, errorMessage, CMSWebError.REQUIRED_FIELD);
			}						
		}
	}
}
