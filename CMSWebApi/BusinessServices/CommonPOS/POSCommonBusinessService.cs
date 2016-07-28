using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System.Linq.Expressions;

namespace CMSWebApi.BusinessServices.CommonPOS
{
	public class POSCommonBusinessService : BusinessBase<IUsersService>
	{
		#region mark
		//internal class MappingList : Commons.SingletonStringTypeMappingBase<MappingList, Type>
		//{
			
		//	MappingList()
		//	{
		//		base.AddMapping("Cameras", typeof(tbl_POS_CameraNBList));
		//		base.AddMapping("CardIDs", typeof(tbl_POS_CardIDList));
		//		base.AddMapping("CheckIDs", typeof(tbl_POS_CheckIDList));
		//		base.AddMapping("Descriptions", typeof(tbl_POS_DescriptionList));
		//		base.AddMapping("ItemCodes", typeof(tbl_POS_ItemCodeList));
		//		base.AddMapping("ExtraNames", typeof(tbl_POS_ExtraName));
		//		base.AddMapping("Operators", typeof(tbl_POS_OperatorList));
		//		base.AddMapping("Payments", typeof(tbl_POS_PaymentList));
		//		base.AddMapping("Registers", typeof(tbl_POS_RegisterList));
		//		base.AddMapping("Shifts", typeof(tbl_POS_ShiftList));
		//		base.AddMapping("Stores", typeof(tbl_POS_StoreList));
		//		base.AddMapping("Taxes", typeof(tbl_POS_TaxesList));
		//		base.AddMapping("Terminals", typeof(tbl_POS_TerminalList));

		//	}
		//}
		#endregion

		public IQueryable<ListModel> Listmodels( string name)
		{
			IQueryable<ListModel> data = null;
			switch( name.ToLower())
			{
				case "cameras":
					data = GetData<tbl_POS_CameraNBList>( null, it => new ListModel{ ID = it.CameraNB_ID, Name = it.CameraNB_Name});
					break;
				case "cardids":
					data = GetData<tbl_POS_CardIDList>(null, it => new ListModel { ID = it.CardID_ID, Name = it.CardID_Name });
					break;
				case "checkids":
					data = GetData<tbl_POS_CheckIDList>(null, it => new ListModel { ID = it.CheckID_ID, Name = it.CheckID_Name });
					break;
				case "descriptions":
					data = GetData<tbl_POS_DescriptionList>(null, it => new ListModel { ID = it.Description_ID, Name = it.Description_Name });
					break;
				case "itemcodes":
					data = GetData<tbl_POS_ItemCodeList>(null, it => new ListModel { ID = it.ItemCode_ID, Name = it.ItemCode_Name });
					break;
				case "extranames":
					data = GetData<tbl_POS_ExtraName>(null, it => new ListModel { ID = it.ExtraID, Name = it.ExtraName });
					break;
				case "operators":
					data = GetData<tbl_POS_OperatorList>(null, it => new ListModel { ID = it.Operator_ID, Name = it.Operator_Name });
					break;
				case "payments":
					data = GetData<tbl_POS_PaymentList>(null, it => new ListModel { ID = it.PaymentID, Name = it.PaymentName });
					break;
				case "registers":
					data = GetData<tbl_POS_RegisterList>(null, it => new ListModel { ID = it.Register_ID, Name = it.Register_Name });
					break;
				case "shifts":
					data = GetData<tbl_POS_ShiftList>(null, it => new ListModel { ID = it.Shift_ID, Name = it.Shift_Name });
					break;
				case "stores":
					data = GetData<tbl_POS_StoreList>(null, it => new ListModel { ID = it.Store_ID, Name = it.Store_Name });
					break;
				case "taxes":
					data = GetData<tbl_POS_TaxesList>(null, it => new ListModel { ID = it.TaxID, Name = it.TaxName });
					break;
				case "terminals":
					data = GetData<tbl_POS_TerminalList>(null, it => new ListModel { ID = it.Terminal_ID, Name = it.Terminal_Name });
					break;
			}

			return data;
		}

		private IQueryable<ListModel> GetData<T>( Expression<Func<T,bool>> filter, Expression<Func<T, ListModel>> selector) where T:class
		{
			InternalBusinessService.POSListBusinessService<T> data = new InternalBusinessService.POSListBusinessService<T>(base.ServiceBase);
			return data.Gets( filter).Select( selector);

		}
	}
}
