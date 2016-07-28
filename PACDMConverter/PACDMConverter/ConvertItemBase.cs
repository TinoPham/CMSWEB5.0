using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;
using MSAccessObjects;

namespace PACDMConverter.PACDMConverter
{
	internal abstract class ConvertItemBase
	{
		public delegate void delRequestItemKey(ref MessageItemKey msgItem, string sqltable,out Commons.ERROR_CODE error);
		public delegate void delAddItemKey(ItemBase newitem);

		public event delRequestItemKey OnRequestItemKey;
		public event delAddItemKey OnAddItemKey;
		protected ConvertDB LocalDB;
		protected ItemKeyConfig itemConfig;
		protected Dictionary<string, string>MappingText = new Dictionary<string,string>();

		protected KeyValuePair<string, string> ItemMap
		{
			get { return GetMappingText( itemConfig.ColumnName);}
		}

		public ConvertItemBase(ItemKeyConfig itemcfg,ConvertDB localDB)
		{
			itemConfig = itemcfg;
			InitializeMappingText();
			LocalDB = localDB;
		}

		protected virtual void InitializeMappingText()
		{
			MappingText = new Dictionary<string, string>();

		}

		protected virtual bool ValidData(dynamic trans)
		{
			if(string.IsNullOrEmpty(ItemMap.Value) )
				return false;
			object value = Commons.ObjectUtils.GetPropertyValue(trans, ItemMap.Key);
			if(value != null && value.GetType().Equals( typeof(System.String)) )
				return !string.IsNullOrEmpty( value.ToString());
			return value != null;
		}
		private KeyValuePair<string, string> GetMappingText( string keyName)
		{
			return MappingText.FirstOrDefault( item => string.Compare( item.Key, keyName, true) == 0);
		}

		protected virtual ConverterDB.Model.ItemBase GetItemKey( dynamic trans, IEnumerable<dynamic> lstITem)
		{
			dynamic found = lstITem.FirstOrDefault( item => string.Compare((item as ItemBase).Name, ItemMap.Value, true) == 0);
			if( found == null)
			{
				ItemBase item = Commons.ObjectUtils.InitObject( itemConfig.DBSetType) as ItemBase;
				item.Name = ItemMap.Value;
				return item;
			}
			return found as ItemBase;
		}
		
		protected virtual ItemBase MessagetoItemKey( dynamic data)
		{
			ItemBase item = Commons.ObjectUtils.InitObject( itemConfig.DBSetType) as ItemBase;
			item.ID = data.ID;
			item.Name = data.Name;
			return item;
		}

		protected virtual object ItemKeyMessage( dynamic trans)
		{
			return new ConvertMessage.MessageItemKey{ ID = 0, Name = ItemMap.Value};
		}

		protected void AddItemKey( ItemBase item)
		{
			if( OnAddItemKey != null)
				OnAddItemKey(item);
		}
		protected void RequestItemKey(ref MessageItemKey msgItem, string sqltable, out Commons.ERROR_CODE error)
		{
			error = Commons.ERROR_CODE.OK;
			if( OnRequestItemKey != null)
				OnRequestItemKey(ref msgItem, sqltable, out error);
		}
		
		protected virtual ItemBase RequestItemKey( dynamic data, IEnumerable<dynamic> lstITem, string sqltable)
		{
			 ItemBase item = GetItemKey(data, lstITem);
			 if( item != null && item.ID > 0)
				return item;
			Commons.ERROR_CODE error = Commons.ERROR_CODE.OK;
			MessageItemKey msgItem =  ItemKeyMessage(data) as MessageItemKey;
			RequestItemKey(ref msgItem, sqltable, out error);
			item = MessagetoItemKey(msgItem);
			AddItemKey(item);
			return item;


		}

		public virtual Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			return Commons.ERROR_CODE.OK;
		}


	}
}
