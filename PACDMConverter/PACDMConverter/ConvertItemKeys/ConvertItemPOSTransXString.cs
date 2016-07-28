using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConverterDB.Model;
using ConvertMessage.PACDMObjects.POS;
using PACDMConverter.Converter;

namespace PACDMConverter.PACDMConverter.ConvertItemKeys
{
	internal class ConvertItemPOSTransXString : ConvertItemBase
	{
		public ConvertItemPOSTransXString(ItemKeyConfig itemconfig)
			: base(itemconfig)
		{

		}
		protected override void InitializeMappingText()
		{
			base.InitializeMappingText();
			base.MappingText.Add("T_TransXString1", "Extra String 1");
			base.MappingText.Add("T_TransXString2", "Extra String 1");
		}
		public override ConverterDB.Model.ItemBase GetItemKey(MSAccessObjects.AccessTransBase trans)
		{
			ItemBase ret = new POSExtraStringValue{ ID = 0, Name = base.ItemMap.Value};
			return ret;
		}
		public override void UpdateItemKey(Transact TransInfo, ItemBase ItemKey, object value)
		{
			//base.UpdateItemKey(TransInfo, ItemKey);
			if( TransInfo.ExStrings == null)
				TransInfo.ExStrings = new List<KeyValuePair<int,string>>();

			TransInfo.ExStrings.Add(new KeyValuePair<int, string>(ItemKey.ID, Commons.ObjectUtils.GetValueInObject<string>(value, itemConfig.ColumnName)));
		}
	}
}
