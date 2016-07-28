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
	internal class ConvertItemPOSTax : ConvertItemBase
	{
		public ConvertItemPOSTax(ItemKeyConfig itemconfig)
			: base(itemconfig)
		{

		}
		protected override void InitializeMappingText()
		{
			base.InitializeMappingText();
			base.MappingText.Add("T_2Tax1Amount", "Tax1");
			base.MappingText.Add("T_3Tax2Amount", "Tax2");
			base.MappingText.Add("T_4Tax3Amount", "Tax3");
			base.MappingText.Add("T_5Tax4Amount", "Tax4");
		}
		public override ConverterDB.Model.ItemBase GetItemKey(MSAccessObjects.AccessTransBase trans)
		{
			KeyValuePair<string, string> itemMap = base.ItemMap;
			POSTaxes item = new POSTaxes{ ID = 0, Name = itemMap.Value};
			return item;
		}
		public override void UpdateItemKey(Transact TransInfo, ItemBase ItemKey, object value)
		{
			//base.UpdateItemKey(TransInfo, ItemKey);
			if( TransInfo.Taxes == null)
				TransInfo.Taxes = new List<KeyValuePair<int,double>>();

			TransInfo.Taxes.Add( new KeyValuePair<int, double> ( ItemKey.ID, Commons.ObjectUtils.GetValueInObject<double>( value, itemConfig.ColumnName) ) );
		}
	}
}
