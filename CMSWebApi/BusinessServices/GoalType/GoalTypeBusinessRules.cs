using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CMSWebApi.Utils;
using CMSWebApi.Resources;
namespace CMSWebApi.BusinessServices.GoalType
{
	internal class GoalTypeBusinessRules : ValidationRules
	{
		public GoalTypeBusinessRules(CultureInfo culture) : base(culture) { }

		public void ValidateInput( string goalName)
		{
			string 	goal = string.Empty;
			if (string.IsNullOrEmpty(goalName))
			{
				goal = ResourceManagers.Instance.GetResourceString(ResourceKey.STR_GOALNAME, culture);
				string errorMessage = string.Format(ResourceManagers.Instance.GetResourceString(CMSWebError.REQUIRED_FIELD, base.culture), goal);
				AddValidationError(goal, errorMessage, CMSWebError.REQUIRED_FIELD);
			}						
		}
		
	}
}
