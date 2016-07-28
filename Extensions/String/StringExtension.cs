using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions.String
{
	public static class StringExtension
	{
		public static bool toBool(this string value)
		{
			return string.Compare( value, "true", true) == 0 || string.Compare("1", value) == 0;
		}
	}
}
