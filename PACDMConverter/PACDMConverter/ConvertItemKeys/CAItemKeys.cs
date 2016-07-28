using System.Collections.Generic;
using ConverterDB.Model;
using PACDMConverter.PACDMConverter;
using System.Linq;
using ConverterDB;
using ConvertMessage;


namespace PACDMConverter.PACDMConverter.ConvertItemKeys
{
	internal class ConvertItemCAFullName : ConvertItemBase
	{
		public ConvertItemCAFullName(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override bool ValidData(dynamic trans)
		{
			if( trans == null) return true;
			string fname = (trans as MSAccessObjects.CA.AccessTransCA).T_FirstName;
			string lname = (trans as MSAccessObjects.CA.AccessTransCA).T_LastName;
			return !string.IsNullOrEmpty(fname) || !string.IsNullOrEmpty(lname);
		}

		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			string fname = (trans as MSAccessObjects.CA.AccessTransCA).T_FirstName;
			string lname = (trans as MSAccessObjects.CA.AccessTransCA).T_LastName;

			dynamic found = lstITem.FirstOrDefault
			(
				delegate( dynamic item)
				{
					return string.Compare((item as CAFullName).Name, fname, true) == 0 && string.Compare((item as CAFullName).LastName, lname, true) == 0;
				}
			)  ;
			
			if (found == null)
			{
				CAFullName item = Commons.ObjectUtils.InitObject(itemConfig.DBSetType) as CAFullName;
				item.Name = fname;
				item.LastName = lname;
				return item;
			}
			return found as ItemBase;

		}
		protected override ItemBase MessagetoItemKey(dynamic data)
		{
			CAFullName ret = Commons.ObjectUtils.InitObject(itemConfig.DBSetType) as CAFullName;
			ret.ID = (data as ConvertMessage.MessageItemCAFullName).ID;
			ret.Name = (data as ConvertMessage.MessageItemCAFullName).Name;
			ret.LastName = (data as ConvertMessage.MessageItemCAFullName).LastName;
			return ret;
		}
		protected override object ItemKeyMessage(dynamic trans)
		{
			string fname = (trans as MSAccessObjects.CA.AccessTransCA).T_FirstName;
			string lname = (trans as MSAccessObjects.CA.AccessTransCA).T_LastName;
			return new ConvertMessage.MessageItemCAFullName{ ID = 0, Name = fname, LastName = lname};
		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			ItemBase item = RequestItemKey(Accessvalue, LocalDB.Query( itemConfig.DBSetType), itemConfig.SQLTable);
			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			(TransInfo as ConvertMessage.PACDMObjects.CA.Transact).T_FullName = item.ID;
			return ret_code;
		}

	}

	internal class ConvertItemCAXString : ConvertItemBase
	{
		public ConvertItemCAXString(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override void InitializeMappingText()
		{
			base.MappingText = ReportingDB.Instance.CA_AliasDiscrete;
		}

		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			return base.GetItemKey((object)trans, lstITem);
		}

		private ItemBase GetExStringValue( string value, out Commons.ERROR_CODE error)
		{
			error = Commons.ERROR_CODE.OK;
			ItemBase ret = LocalDB.Query<CAXString>().FirstOrDefault( item => string.Compare( item.Name, value, true ) == 0);
			if( ret != null && ret.ID > 0)
				return ret;

			MessageItemKey msgitem = new MessageItemKey{ ID = 0, Name = value};
			RequestItemKey(ref msgitem, LocalDB.SqlTableName<CAXString>(), out error);
			ret = new CAXString() { ID = msgitem.ID, Name = msgitem.Name };
			return ret;

		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			string str_value = Commons.ObjectUtils.GetValueInObject<string>(Accessvalue, itemConfig.ColumnName);
			if( string.IsNullOrEmpty(str_value))
				return ret_code;

			ItemBase item = RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable);
			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			ItemBase exValue = GetExStringValue(str_value, out ret_code);
			if( exValue == null || exValue.ID == 0)
				return ret_code;

			if ((TransInfo as ConvertMessage.PACDMObjects.CA.Transact).ExStrings == null)
				TransInfo.ExStrings = new List<KeyValuePair<int, int>>();

			(TransInfo as ConvertMessage.PACDMObjects.CA.Transact).ExStrings.Add(new KeyValuePair<int, int>(item.ID, exValue.ID));
			return ret_code;
		}
		
	}
}