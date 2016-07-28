using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Configurations
{
	//public class DashBoardsSection : ConfigurationSection
	//{
	//	public const string DashBoardsSection_Name = "DashboardCache";
	//	[ConfigurationProperty(Defines.STR_DashBoards)]
	//	public CacheConfigCollection DashBoards
	//	{
	//		get { return base [Defines.STR_DashBoards] as CacheConfigCollection; }
	//	}
	//}
	[ConfigurationCollection(typeof(DashboardCacheConfig), AddItemName = Defines.STR_add, ClearItemsName = Defines.STR_clear, RemoveItemName = Defines.STR_remove, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class DashboardCacheConfigCollection : ConfigurationElementCollection
	{
		public DashboardCacheConfig this [int index]
		{
			get { return (DashboardCacheConfig)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		public void Add(DashboardCacheConfig serviceConfig)
		{
			BaseAdd(serviceConfig);
		}

		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new DashboardCacheConfig();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((DashboardCacheConfig)element).Name;
		}

		public void Remove(DashboardCacheConfig serviceConfig)
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

	public class DashboardCacheConfig : ConfigurationElement
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

		//[ConfigurationProperty(Defines.STR_items, DefaultValue = Defines.Default_Items, IsRequired = false)]
		//public int Items
		//{
		//	get { return (int)this [Defines.STR_items]; }
		//	set { this [Defines.STR_items] = value <= 0 ? Defines.Default_Items : Math.Min(value, Defines.Max_Items) ; }
		//}


		[ConfigurationProperty(Defines.STR_Parallelism, DefaultValue = Defines.Default_Parallelism, IsRequired = false)]
		public int Parallelism
		{
			get { return (int)this [Defines.STR_Parallelism] <= 0 ? Environment.ProcessorCount : (int)this [Defines.STR_Parallelism]; }
			set { this [Defines.STR_Parallelism] = value <= 0 ? Environment.ProcessorCount : Math.Min(Environment.ProcessorCount, value); }
		}


		[ConfigurationProperty(Defines.STR_chunkzise, DefaultValue = Defines.Default_Items, IsRequired = false)]
		public int ChunkSize
		{
			get { return (int)this [Defines.STR_chunkzise]; }
			set { this [Defines.STR_chunkzise] = value <= 0 ? Defines.Default_Items : value; }
		}

		[ConfigurationProperty(Defines.STR_save, DefaultValue = true, IsRequired = false)]
		public bool Save
		{
			get { return (bool)this [Defines.STR_save]; }
			set { this [Defines.STR_save] = value; }
		}

		[ConfigurationProperty(Defines.STR_enable, DefaultValue = true, IsRequired = false)]
		public bool Enable
		{
			get { return (bool)this [Defines.STR_enable]; }
			set { this [Defines.STR_enable] = value; }
		}
		[ConfigurationProperty(Defines.STR_live, DefaultValue = true, IsRequired = false)]
		public bool Live
		{
			get { return (bool)this [Defines.STR_live]; }
			set { this [Defines.STR_live] = value; }
		}

		[ConfigurationProperty(Defines.STR_Period, DefaultValue = "None")]
		[TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<Period>))]
		public Period Period
		{
			get { return (Period)this [Defines.STR_Period]; }
			set { this [Defines.STR_Period] = value.ToString(); }
		}

		[ConfigurationProperty(Defines.STR_interval, DefaultValue = Defines.Default_Interval, IsRequired = false)]
		public int Interval
		{
			get { return (int)this [Defines.STR_interval]; }
			set { this [Defines.STR_interval] = value; }
		}
		public DateTime PeriodDate(DateTime date)
		{
			return Defines.PeriodDate(date, Period, Interval );
		}
	}

	public class CaseInsensitiveEnumConfigConverter<T> : ConfigurationConverterBase
	{
		
		public override object ConvertFrom(
		ITypeDescriptorContext ctx, CultureInfo ci, object data)
		{
			return Commons.Utils.GetEnum<T>(data);
			//if( data is string)
			//	return Enum.Parse(typeof(T), (string)data, true);
			//return (T)data;
		}
	}

}
