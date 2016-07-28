using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Configurations
{
	[ConfigurationCollection(typeof(TableCacheConfig), AddItemName = Defines.STR_Table, ClearItemsName = Defines.STR_clear, RemoveItemName = Defines.STR_remove, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class TableCacheConfigCollection : ConfigurationElementCollection
	{
		public TableCacheConfig this [int index]
		{
			get { return (TableCacheConfig)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		public void Add(TableCacheConfig serviceConfig)
		{
			BaseAdd(serviceConfig);
		}

		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new TableCacheConfig();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((TableCacheConfig)element).Name;
		}

		public void Remove(TableCacheConfig serviceConfig)
		{
			BaseRemove(serviceConfig.Name);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(String name)
		{
			BaseRemove(name);
		}
	}

	public class TableCacheConfig : ConfigurationElement
	{
		[ConfigurationProperty(Defines.STR_name, IsRequired = true)]
		public string Name
		{
			get
			{
				return (string)this [Defines.STR_name];
			}
			set
			{
				this [Defines.STR_name] = value;
			}
		}

		[ConfigurationProperty(Defines.STR_autoupdate, IsRequired = false)]
		public bool AutoUpdate
		{
			get { return (bool)this [Defines.STR_autoupdate]; }
			set { this [Defines.STR_autoupdate] = value; }
		}

		[ConfigurationProperty(Defines.STR_enable, DefaultValue = true, IsRequired = true)]
		public bool Enable
		{
			get { return (bool)this [Defines.STR_enable]; }
			set { this [Defines.STR_enable] = value; }
		}

	}
}
