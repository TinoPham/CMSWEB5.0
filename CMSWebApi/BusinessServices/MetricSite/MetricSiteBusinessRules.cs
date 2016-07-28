using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.MetricSite
{
	internal class MetricSiteBusinessRules : ValidationRules
	{
		public MetricSiteBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string metricName)
		{
			string 	company = string.Empty;
			if (string.IsNullOrEmpty(metricName))
			{
				company = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_METRICNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), company);
				AddValidationError(company, errorMessage, CMSWebError.REQUIRED_FIELD);
			}						
		}
	}
}
