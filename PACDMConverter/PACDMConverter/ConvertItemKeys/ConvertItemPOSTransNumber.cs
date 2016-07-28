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
	internal class ConvertItemPOSTransNumber : ConvertItemBase
	{
		public ConvertItemPOSTransNumber(ItemKeyConfig itemconfig)
			: base(itemconfig)
		{

		}
		protected override void InitializeMappingText()
		{
			base.InitializeMappingText();
			base.MappingText.Add("T_TransXNbInt", "Extra Number Int");
			base.MappingText.Add("T_TransXNbFloat", "Extra Number Float");
		}
		public override ConverterDB.Model.ItemBase GetItemKey(MSAccessObjects.AccessTransBase trans)
		{
			ItemBase ret = new POSExtraName{ ID = 0, Name = base.ItemMap.Value};
			return ret;
		}
		public override void UpdateItemKey(Transact TransInfo, ItemBase ItemKey, object value)
		{
			//base.UpdateItemKey(TransInfo, ItemKey);
			if( TransInfo.ExNumbers == null)
				TransInfo.ExNumbers = new List<KeyValuePair<int, double>>();

			TransInfo.ExNumbers.Add(new KeyValuePair<int, double>(ItemKey.ID, Commons.ObjectUtils.GetValueInObject<double>(value, itemConfig.ColumnName)));
		}
	}
}
