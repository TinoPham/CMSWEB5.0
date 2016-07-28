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

		internal class ConvertItemATMTransXString : ConvertItemBase
		{
			public ConvertItemATMTransXString(ItemKeyConfig itemconfig, ConvertDB localDB) : base(itemconfig, localDB)
			{

			}

			protected override void InitializeMappingText()
			{
				base.MappingText = ReportingDB.Instance.ATM_AliasDiscrete;
			}

			protected override ConverterDB.Model.ItemBase GetItemKey(dynamic trans, IEnumerable<dynamic> lstITem)
			{
				return base.GetItemKey((object)trans, lstITem);
			}

			private ItemBase EXStringValueItemKey(string value, out Commons.ERROR_CODE error)
			{
				error = Commons.ERROR_CODE.OK;
				ItemBase ret = LocalDB.Query<ATMXString>().FirstOrDefault(item => string.Compare(item.Name, value, true) == 0);
				if (ret != null && ret.ID > 0)
					return ret;
				MessageItemKey msgitem = new MessageItemKey { ID = 0, Name = value };
				RequestItemKey(ref msgitem, LocalDB.SqlTableName<ATMXString>(), out error);
				ret = Commons.ObjectUtils.InitObject<ATMXString>();
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
				if (item == null || item.ID == 0)
					return Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;

				ItemBase exValue = EXStringValueItemKey(str_value, out ret_code);
				if (exValue == null || exValue.ID == 0)
					return ret_code;// Commons.ERROR_CODE.DB_CANNOT_FIND_ITEMKEY;
				if (TransInfo.ExStrings == null)
					TransInfo.ExStrings = new List<KeyValuePair<int, int>>();

				TransInfo.ExStrings.Add(new KeyValuePair<int, int>(item.ID, exValue.ID));
				return ret_code;
			}

		}
	}