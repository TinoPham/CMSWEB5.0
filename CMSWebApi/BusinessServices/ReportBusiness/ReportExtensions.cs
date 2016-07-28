using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.BusinessServices.ReportBusiness
{
	 internal static class ReportExtensions
	{
		public static decimal toValueCompare(this decimal current, decimal compare)
		 {
			 if (compare == 0)
				 return (current == 0) ? 0 : 100; //#896

			 decimal value = (Math.Abs(current - compare) / compare) * 100;
			 return Math.Round(value, 2);
		 }
		public static decimal toValueCompare(this int current, int compare)
		{
			if (compare == 0)
				return (current == 0) ? 0 : 100; //#896

			decimal value = (Math.Abs((decimal)current - compare) / (decimal)compare) * 100;
			return Math.Round(value, 2);
		}
		public static decimal toValueCompare(this long current, long compare)
		{
			if (compare == 0)
				return (current == 0) ? 0 : 100; //#896

			decimal value = (Math.Abs((decimal)current - compare) / (decimal)compare) * 100;
			return Math.Round(value, 2);
		}
		
	}
}
