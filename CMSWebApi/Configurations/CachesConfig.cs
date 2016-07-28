using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Configurations
{

	public class CachesConfig: ConfigurationSection
	{
		public const string CachesSection_Name = "Caches";
		[ConfigurationProperty(Defines.STR_DashBoards)]
		public DashboardCacheConfigCollection DashBoards
		{
			get { return base [Defines.STR_DashBoards] as DashboardCacheConfigCollection; }
		}
		[ConfigurationProperty(Defines.STR_Tables)]
		public TableCacheConfigCollection Tables
		{
			get { return base [Defines.STR_Tables] as TableCacheConfigCollection; }
		}
	}
}
