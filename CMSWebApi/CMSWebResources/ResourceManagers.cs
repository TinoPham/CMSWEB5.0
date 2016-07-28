using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;
using Commons;


namespace CMSWebApi.Resources
{
	public class ResourceManagers : SingletonClassBase<ResourceManagers>
	{
		const string Default_Culture_Name = "en-US";
		const string STR_ResourceName = "Resx";
		const string ResourceFolder = "Resources";
		private ResourceManager _resmanager = null;
		
		readonly CultureInfo Default_Culture = new CultureInfo(Default_Culture_Name);
		private ResourceManagers()
		{
			_resmanager = new ResourceManager(string.Format("{0}.{1}.{2}", typeof(ResourceManagers).Namespace, ResourceFolder, STR_ResourceName), Assembly.GetExecutingAssembly());
		}
		public string GetResourceString(string key, CultureInfo cul_info = null)
		{
			try
			{
				return _resmanager.GetString(key, cul_info == null ? Default_Culture : cul_info);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		public string GetResourceString(CMSWebError error_code, CultureInfo cul_info = null)
		{
			return GetResourceString(error_code.ToString(), cul_info);
		}
	}
}
