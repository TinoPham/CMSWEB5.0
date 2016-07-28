using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Resources
{
	public class ResourceManagers: SingletonClassBase<ResourceManagers>
	{
		const string Default_Culture_Name = "en-US";
		const string STR_ResourceName = "ErrorDetail";
		private ResourceManager _resmanager = null;
		//private static readonly Lazy<ResourceManagers> Lazy = new Lazy<ResourceManagers>(() => new ResourceManagers());

		//public static ResourceManagers Instance { get { return Lazy.Value; } }
		readonly CultureInfo Default_Culture = new CultureInfo( Default_Culture_Name);
		private ResourceManagers()
		{
			_resmanager = new ResourceManager(string.Format("{0}.{1}", typeof(ResourceManagers).Namespace , STR_ResourceName) , Assembly.GetExecutingAssembly());
		}
		public string GetResourceString( string key, CultureInfo  cul_info = null)
		{
			try
			{
				return _resmanager.GetString(key, cul_info == null ? Default_Culture : cul_info);
			}
			catch(Exception)
			{
				return key;
			}
		}
		public string GetResourceString(Commons.ERROR_CODE error_code, CultureInfo cul_info = null)
		{
			return GetResourceString( error_code.ToString(), cul_info);
		}
	}
}

