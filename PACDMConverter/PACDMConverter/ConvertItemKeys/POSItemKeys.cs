using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;
using MSAccessObjects.POS;
using PACDMConverter.PACDMConverter;
namespace PACDMConverter.PACDMConverter.ConvertItemKeys
{
	#region Transact

	internal class ConvertItemPOSTransNumber : ConvertItemBase
	{
		public ConvertItemPOSTransNumber(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}

		protected override void InitializeMappingText()
		{
			base.MappingText = ReportingDB.Instance.AliasNumber;
		}
		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			return base.GetItemKey((object)trans, lstITem);
			
		}
		protected override object ItemKeyMessage(dynamic trans)
		{
			return base.ItemKeyMessage((object)trans);
		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;// base.UpdateItemKey(TransInfo, value, lstITem);
			object value = Commons.ObjectUtils.GetPropertyValue(Accessvalue, itemConfig.ColumnName);
			if( value == null || string.IsNullOrEmpty(value.ToString()))
				return ret_code;

			ItemBase item = RequestItemKey(Accessvalue, base.LocalDB.Query(base.itemConfig.DBSetType), itemConfig.SQLTable );

			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;

			if (TransInfo.ExNumbers == null)
				TransInfo.ExNumbers = new List<KeyValuePair<int, double>>();

			TransInfo.ExNumbers.Add(new KeyValuePair<int, double>(item.ID, Commons.ObjectUtils.GetValueInObject<double>(Accessvalue, itemConfig.ColumnName)));
			return ret_code;

		}
	}
	
	internal class ConvertItemPOSTax : ConvertItemBase
	{
		public ConvertItemPOSTax(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override void InitializeMappingText()
		{
			base.MappingText = ReportingDB.Instance.AliasNumber;
			
		}

		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			return base.GetItemKey((object)trans, lstITem);
		}
		protected override object ItemKeyMessage(dynamic trans)
		{
			return base.ItemKeyMessage((object)trans);
		}

		public override Commons.ERROR_CODE UpdateItemKey( object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code =  Commons.ERROR_CODE.OK;
			object value = Commons.ObjectUtils.GetPropertyValue(Accessvalue, itemConfig.ColumnName);
			if (value == null || string.IsNullOrEmpty(value.ToString()))
				return ret_code;

			ItemBase item = RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable);

			if (item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			if (TransInfo.Taxes == null)
				TransInfo.Taxes = new List<KeyValuePair<int, double>>();

			TransInfo.Taxes.Add(new KeyValuePair<int, double>(item.ID, Commons.ObjectUtils.GetValueInObject<double>(Accessvalue, itemConfig.ColumnName)));

			return ret_code;

		}
		
	}

	internal class ConvertItemPOSTransXString : ConvertItemBase
	{
		public ConvertItemPOSTransXString(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override void InitializeMappingText()
		{
			base.MappingText = ReportingDB.Instance.AliasDiscrete;
		}
		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			return base.GetItemKey((object)trans, lstITem);
		}

		private ItemBase EXStringValueItemKey( string value, out Commons.ERROR_CODE error)
		{
			error = Commons.ERROR_CODE.OK;
			ItemBase ret = LocalDB.Query<POSExtraStringValue>().FirstOrDefault( item =>  string.Compare( item.Name, value, true ) == 0);
			if( ret != null && ret.ID > 0)
				return ret;
			MessageItemKey msgitem = new MessageItemKey{ ID = 0, Name = value};
			RequestItemKey(ref msgitem, LocalDB.SqlTableName<POSExtraStringValue>(), out error);
			ret = Commons.ObjectUtils.InitObject<POSExtraStringValue>();
			ret.ID = msgitem.ID;
			ret.Name = msgitem.Name;
			base.AddItemKey(ret);
			return ret;
		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			string str_value = Commons.ObjectUtils.GetValueInObject<string>(Accessvalue, itemConfig.ColumnName);
			if (string.IsNullOrEmpty(str_value))
				return ret_code;
			ItemBase item = RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable);
			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			ItemBase exValue = EXStringValueItemKey( str_value, out ret_code);
			if( exValue == null || exValue.ID == 0)
				return ret_code ;// Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			if (TransInfo.ExStrings == null)
				TransInfo.ExStrings = new List<KeyValuePair<int, int>>();

			TransInfo.ExStrings.Add(new KeyValuePair<int, int>(item.ID,  exValue.ID));
			return ret_code;
		}
		
	}

	internal class ConvertItemPOSPayment : ConvertItemBase
	{
		const string STR_T_7PaymAmount = "T_7PaymAmount";


		public ConvertItemPOSPayment(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override bool ValidData(dynamic trans)
		{
			return (trans as AccessTransPOS).T_7PaymAmount.HasValue;
		}
		protected override void InitializeMappingText()
		{
			base.InitializeMappingText();
		}

		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			string Payment_name = (trans as AccessTransPOS).T_MethOfPaymID;
			dynamic found = string.IsNullOrEmpty(Payment_name) ? lstITem.FirstOrDefault(item =>(item as ItemBase).Name == null)  : lstITem.FirstOrDefault(item => string.Compare((item as ItemBase).Name, Payment_name, true) == 0);
			if (found == null)
			{
				ItemBase item = Commons.ObjectUtils.InitObject(itemConfig.DBSetType) as ItemBase;
				item.Name = string.IsNullOrEmpty(Payment_name)? null : Payment_name;
				return item;
			}

			return found as ItemBase;

		}

		protected override object ItemKeyMessage( dynamic trans)
		{
			string Payment_name = (trans as MSAccessObjects.POS.AccessTransPOS).T_MethOfPaymID;
			return new ConvertMessage.MessageItemKey{ ID = 0, Name = Payment_name} ;
		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic TransInfo)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			if( !(Accessvalue as AccessTransPOS).T_7PaymAmount.HasValue)
				return ret_code;

			ItemBase item = RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable); 
			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;

			if (TransInfo.Payments == null)
				TransInfo.Payments = new List<KeyValuePair<int, double>>();

			TransInfo.Payments.Add(new KeyValuePair<int, double>(item.ID, Commons.ObjectUtils.GetValueInObject<double>(Accessvalue, STR_T_7PaymAmount)));
			return ret_code;
		}
		
	}

	#endregion

	#region Retail

	internal class ConvertItemPOSRetailXString : ConvertItemBase
	{
		public ConvertItemPOSRetailXString(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override void InitializeMappingText()
		{
			MappingText = ReportingDB.Instance.AliasDiscrete;
		}

		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			return base.GetItemKey((object)trans, lstITem);
		}

		protected override object ItemKeyMessage(dynamic trans)
		{
			return base.ItemKeyMessage((object)trans);
		}
		 private ItemBase GetExValue( string value, out Commons.ERROR_CODE error)
		 {
			error = Commons.ERROR_CODE.OK;
			ItemBase ret = LocalDB.Query<POSExtraStringValue>().FirstOrDefault( item => string.Compare( item.Name, value, true) == 0);
			if( ret != null)
				return ret;
			MessageItemKey msgItem = new MessageItemKey{ ID = 0, Name = value};
			RequestItemKey(ref msgItem, LocalDB.SqlTableName<POSExtraStringValue>(), out error);
			ret = Commons.ObjectUtils.InitObject<POSExtraStringValue>();
			ret.ID = msgItem.ID;
			ret.Name = msgItem.Name;
			AddItemKey(ret);
			return ret;
		 }
		 public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic sqlRtail)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			string str_value = Commons.ObjectUtils.GetValueInObject<string>(Accessvalue, itemConfig.ColumnName);
			if( string.IsNullOrEmpty( str_value))
				return ret_code;
			ItemBase exName = RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable);
			if( exName == null || exName.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
			ItemBase exValue = GetExValue(str_value, out ret_code);
			if( exValue == null || exValue.ID == 0)
				return ret_code;// Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;

			if (sqlRtail.ExStrings == null)
				sqlRtail.ExStrings = new List<KeyValuePair<int, int>>();

			sqlRtail.ExStrings.Add(new KeyValuePair<int, int>( exName.ID, exValue.ID));

			return ret_code;
		}
		
	}

	internal class ConvertItemPOSRetailNumber : ConvertItemBase
	{
		public ConvertItemPOSRetailNumber(ItemKeyConfig itemconfig, ConvertDB localDB)
			: base(itemconfig, localDB)
		{

		}
		protected override void InitializeMappingText()
		{
			MappingText = ReportingDB.Instance.AliasNumber;
			
		}
		protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
		{
			//ItemBase ret = new POSExtraName { ID = 0, Name = base.ItemMap.Value };
			//return ret;
			return base.GetItemKey((object)trans, lstITem);
		}

		public override Commons.ERROR_CODE UpdateItemKey(object Accessvalue, dynamic sqlRtail)
		{
			Commons.ERROR_CODE ret_code = Commons.ERROR_CODE.OK;
			object value = Commons.ObjectUtils.GetValueInObject<double>(Accessvalue, itemConfig.ColumnName);
			if( value == null)
				return ret_code;

			ItemBase item =  RequestItemKey(Accessvalue, LocalDB.Query(itemConfig.DBSetType), itemConfig.SQLTable);
			if( item == null || item.ID == 0)
				return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;

			if (sqlRtail.ExNumbers == null)
				sqlRtail.ExNumbers = new List<KeyValuePair<int, double>>();

			sqlRtail.ExNumbers.Add(new KeyValuePair<int, double>(item.ID, Commons.ObjectUtils.GetValueInObject<double>(Accessvalue, itemConfig.ColumnName)));
			return ret_code;
		}
	}
	
	#endregion
}