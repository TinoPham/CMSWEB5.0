using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Configurations
{
	public class ApiConfigs : ConfigurationSection
	{
		public const string ApiConfigs_Name = "ApiConfigs";
		[ConfigurationProperty(Defines.Allow)]
		public AllowIpsCollection Allows
		{
			get { return base [Defines.Allow] as AllowIpsCollection; }
		}
		public IEnumerable<string> AllowIpAddress
		{
			get { return Allows.Cast<IPAllowConfig>().Select(it => it.Value); }
		}
		
	}

	[ConfigurationCollection(typeof(IPAllowConfig), AddItemName = Defines.STR_add, ClearItemsName = Defines.STR_clear, RemoveItemName = Defines.STR_remove, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class AllowIpsCollection : ConfigurationElementCollection
	{
		public IPAllowConfig this [int index]
		{
			get { return (IPAllowConfig)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		public void Add(IPAllowConfig serviceConfig)
		{
			BaseAdd(serviceConfig);
		}

		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new IPAllowConfig();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((IPAllowConfig)element).Value;
		}

		public void Remove(IPAllowConfig serviceConfig)
		{
			BaseRemove(serviceConfig.Value);
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

	public class IPAllowConfig : ConfigurationElement
	{
		[ConfigurationProperty(Defines.value, IsRequired = true)]
		public string Value
		{
			get
			{
				return (string)this [Defines.value];
			}
			set
			{
				this [Defines.value] = value;
			}
		}

	}
}
