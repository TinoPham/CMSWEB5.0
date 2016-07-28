using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.Caches;
using PACDMModel.Model;
using CMSWebApi.DataModels.DashBoardCache;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
	internal class FactPOSTransact : FactBase<Fact_POS_Transact,tbl_POS_Transact>
	{
		const int Thread_sleep = 10;
		protected override Fact_POS_Transact UpdateFact(IResposity pacdb, tbl_POS_Transact raw)
		{
			ICache<POSPeriodicCacheModel> icache = this.BackgroungTask.CacheMgr.Resolve<POSPeriodicCacheModel>();
			POSCache poscache = icache as POSCache;
			if (poscache == null || !poscache.Config.Enable || !poscache.Config.Live)
				return null;

			bool isnewmodel = false;
			Fact_POS_Transact fact = ToFactModel( pacdb, raw,ref isnewmodel);
			Fact_POS_Transact cfact = null;
			bool is_retail = raw.tbl_POS_Retail == null? false : raw.tbl_POS_Retail.Any();
			bool is_subretail = false;
			foreach (tbl_POS_Retail rtail in raw.tbl_POS_Retail)
			{
				cfact = new Fact_POS_Transact();
				CloneFactTransact( fact, cfact);
				ToFactRetail(cfact, rtail);
				is_subretail  = rtail.tbl_POS_SubRetail == null ? false : rtail.tbl_POS_SubRetail.Any();
				foreach( tbl_POS_SubRetail srt in rtail.tbl_POS_SubRetail)
				{
					cfact = new Fact_POS_Transact();
					CloneFactTransact(fact, cfact);
					CloneFactRetail(fact, cfact);
					ToFactSubretail(cfact, srt);
					pacdb.Insert<Fact_POS_Transact>(cfact);
					System.Threading.Thread.Sleep(Thread_sleep);
				}
				if(!is_subretail)
					pacdb.Insert<Fact_POS_Transact>(cfact);

				System.Threading.Thread.Sleep(Thread_sleep);
			}
			if(!is_retail)
				pacdb.Insert<Fact_POS_Transact>(fact);

			int saved = pacdb.Save();
			if( saved > 0)
			{
				
				int count = 0;
				count += InsertFactPayment(pacdb, raw, fact);
				count += InsertFactTransactTax(pacdb, raw, fact);
				count += InsertFactExtraNumber(pacdb, raw, fact);
				count += InsertTransExtraString(pacdb, raw, fact);
				if( count > 0)
					pacdb.Save();
			}
			if( saved > 0)
			{
				return is_retail? cfact : fact;
			}
			return null;
		}

		protected override Fact_POS_Transact ToFactModel(IResposity pacdb, tbl_POS_Transact raw, ref bool isnewmodel)
		{
			isnewmodel = true;
			Fact_POS_Transact fact = new Fact_POS_Transact();
			fact.TransID = raw.TransID;
			fact.T_DVRDateKey = raw.DVRDate.Value.Date;
			fact.T_DVRDate = raw.DVRDate;
			fact.T_TransDateKey = raw.TransDate.Value.Date;
			fact.T_TransDate = raw.TransDate;
			fact.T_PACID = PACIDbyraw( raw.T_PACID);
			if( raw.T_CameraNB.HasValue)
				fact.T_CameraNB = GetDim<Dim_POS_CameraNB, int>(it => it.CameraNB_BK == raw.T_CameraNB, it => it.CameraNB_ID);
			if( raw.T_OperatorID.HasValue)
				fact.T_OperatorID = GetDim<Dim_POS_Operator, int>( it => it.Operator_BK == raw.T_OperatorID.Value, it => it.Operator_ID);
			if(raw.T_StoreID.HasValue)
				fact.T_StoreID = GetDim<Dim_POS_Store, int>(it => it.Store_BK == raw.T_StoreID.Value, it => it.Store_ID);
			if( raw.T_TerminalID.HasValue)
				fact.T_TerminalID = GetDim<Dim_POS_Terminal, int>(it => it.Terminal_BK == raw.T_TerminalID.Value, it => it.Terminal_ID);
			if (raw.T_ShiftID.HasValue)
				fact.T_ShiftID = GetDim<Dim_POS_Shift, int>(it => it.Shift_BK == raw.T_ShiftID.Value, it => it.Shift_ID);
			if( raw.T_CheckID.HasValue)
				fact.T_CheckID = GetDim<Dim_POS_CheckID, int>( it => it.CheckID_BK == raw.T_CheckID.Value, it => it.CheckID_ID);
			if( raw.T_CardID.HasValue)
				fact.T_CardID = GetDim<Dim_POS_CardID, int>(it => it.CardID_BK == raw.T_CardID.Value, it => it.CardID_ID);
			if( !string.IsNullOrEmpty(raw.T_00TransNBText) )
			{
				Dim_POS_TransNBText dimnbtext = GetDim<Dim_POS_TransNBText, Dim_POS_TransNBText>(it => string.Compare(it.TransNBText, raw.T_00TransNBText, true) == 0, it => it);
				if( dimnbtext == null)
				{
					dimnbtext = new Dim_POS_TransNBText{ TransNBText = raw.T_00TransNBText};
					Dim_POS_TransNBText newdim = WarehouseManager.Instance.UpdateDim(dimnbtext) as Dim_POS_TransNBText;
					fact.T_TransNBTextID = (newdim == null || newdim.TransNBText_ID == 0)? (int?)null : newdim.TransNBText_ID;
				}
				else
				fact.T_TransNBTextID = dimnbtext.TransNBText_ID;
			}
			fact.T_0TransNB = raw.T_0TransNB;
			fact.T_6TotalAmount = raw.T_6TotalAmount;
			fact.T_1SubTotal = raw.T_1SubTotal;
			fact.T_8ChangeAmount =raw.T_8ChangeAmount;
			fact.T_9RecItemCount = raw.T_9RecItemCount;
			fact.T_TOBox = raw.T_TOBox;
			return fact;
		}

		private int InsertTransExtraString(IResposity pacdb, tbl_POS_Transact trans, Fact_POS_Transact fact_trans)
		{
			Fact_POS_TransExtraString factString = null;
			int count = 0;
			foreach (tbl_POS_TransExtraString item in trans.tbl_POS_TransExtraString)
			{
				factString = new Fact_POS_TransExtraString();
				factString.TransID = fact_trans.TransID;
				factString.ExtraID  = GetDim<Dim_POS_ExtraName, int>(it => it.ExtraID_BK == item.ExtraID, it => it.ExtraID);
				factString.ExString_ValueID =  GetDim<Dim_POS_ExtraStringValue, int>( it => it.ExString_ValueID_BK == item.ExString_ValueID, it => it.ExString_ValueID);
				factString.T_DVRDateKey = fact_trans.T_DVRDateKey;
				factString.T_DVRDate = fact_trans.T_DVRDate;
				factString.T_TransDateKey = fact_trans.T_TransDateKey;
				factString.T_TransDate = fact_trans.T_TransDate;
				factString.T_PACID = fact_trans.T_PACID;
				factString.T_CameraNB = fact_trans.T_CameraNB;
				factString.T_OperatorID = fact_trans.T_OperatorID;
				factString.T_StoreID = fact_trans.T_StoreID;
				factString.T_TerminalID = fact_trans.T_TerminalID;
				factString.T_RegisterID = fact_trans.T_RegisterID;
				factString.T_ShiftID = fact_trans.T_ShiftID;
				factString.T_CheckID = fact_trans.T_CheckID;
				factString.T_CardID = fact_trans.T_CardID;
				factString.T_TransNBTextID = fact_trans.T_TransNBTextID;
				pacdb.Insert<Fact_POS_TransExtraString>(factString);
				count++;
				System.Threading.Thread.Sleep(Thread_sleep);
			}
			return count;
		}
	
		private int InsertFactExtraNumber(IResposity pacdb, tbl_POS_Transact trans, Fact_POS_Transact fact_trans)
		{
			Fact_POS_TransExtraNumber factnumber = null;
			int count = 0;
			foreach (tbl_POS_TransExtraNumber num in trans.tbl_POS_TransExtraNumber)
			{
				factnumber = new Fact_POS_TransExtraNumber();
				factnumber.TransID = fact_trans.TransID;
				factnumber.ExtraID = GetDim<Dim_POS_ExtraName, int>(it => it.ExtraID_BK == num.ExtraID, it => it.ExtraID);
				factnumber.T_DVRDateKey = fact_trans.T_DVRDateKey;
				factnumber.T_DVRDate = fact_trans.T_DVRDate;
				factnumber.T_TransDateKey = fact_trans.T_TransDateKey;
				factnumber.T_TransDate = fact_trans.T_TransDate;
				factnumber.T_PACID = fact_trans.T_PACID;
				factnumber.T_CameraNB = fact_trans.T_CameraNB;
				factnumber.T_OperatorID = fact_trans.T_OperatorID;
				factnumber.T_StoreID = fact_trans.T_StoreID;
				factnumber.T_TerminalID = fact_trans.T_TerminalID;
				factnumber.T_RegisterID = fact_trans.T_RegisterID;
				factnumber.T_ShiftID = fact_trans.T_ShiftID;
				factnumber.T_CheckID = fact_trans.T_CheckID;
				factnumber.T_CardID = fact_trans.T_CardID;
				factnumber.T_TransNBTextID = fact_trans.T_TransNBTextID;
				factnumber.ExNum_Value = num.ExNum_Value;
				pacdb.Insert<Fact_POS_TransExtraNumber>(factnumber);
				count++;
				System.Threading.Thread.Sleep(Thread_sleep);
			}
			return count;
		}
	
		private int InsertFactTransactTax(IResposity pacdb, tbl_POS_Transact trans, Fact_POS_Transact fact_trans)
		{
			int count = 0;
			Fact_POS_TransactTax factTax = null;
			foreach( tbl_POS_TransTaxes tax in trans.tbl_POS_TransTaxes)
			{
				factTax = new Fact_POS_TransactTax();
				factTax.TransID = fact_trans.TransID;
				factTax.TaxID = GetDim<Dim_POS_Tax,int>( it => it.TaxID_BK == tax.TaxID, it => it.TaxID);
				factTax.T_DVRDateKey = fact_trans.T_DVRDateKey;
				factTax.T_DVRDate = fact_trans.T_DVRDate;
				factTax.T_TransDateKey = fact_trans.T_TransDateKey;
				factTax.T_TransDate = fact_trans.T_TransDate;
				factTax.T_PACID = fact_trans.T_PACID;
				factTax.T_CameraNB = fact_trans.T_CameraNB;
				factTax.T_OperatorID = fact_trans.T_OperatorID;
				factTax.T_StoreID = fact_trans.T_StoreID;
				factTax.T_TerminalID = fact_trans.T_TerminalID;
				factTax.T_RegisterID = fact_trans.T_RegisterID;
				factTax.T_ShiftID = fact_trans.T_ShiftID;
				factTax.T_CheckID = fact_trans.T_CheckID;
				factTax.T_CardID = fact_trans.T_CardID;
				factTax.T_TransNBTextID = fact_trans.T_TransNBTextID;
				factTax.TaxAmount = tax.TaxAmount;
				pacdb.Insert<Fact_POS_TransactTax>( factTax);
				count++;
				System.Threading.Thread.Sleep(Thread_sleep);
			}
			return count;
		}
	
		private int InsertFactPayment(IResposity pacdb, tbl_POS_Transact trans, Fact_POS_Transact fact_trans)
		{
			int count = 0;
			Fact_POS_TransactPayment factPay = null;

			foreach( tbl_POS_TransPayment item in trans.tbl_POS_TransPayment )
			{
				factPay = new Fact_POS_TransactPayment();
				factPay.PaymentAmount = item.PaymentAmount;
				factPay.PaymentID = GetDim<Dim_POS_Payment, int>(it => it.PaymentID_BK == item.PaymentID, it => it.PaymentID);
				factPay.T_CameraNB = fact_trans.T_CameraNB;
				factPay.T_CardID = fact_trans.T_CardID;
				factPay.T_CheckID = fact_trans.T_CheckID;
				factPay.T_DVRDate = fact_trans.T_DVRDate;
				factPay.T_DVRDateKey = fact_trans.T_DVRDateKey;
				factPay.T_OperatorID = fact_trans.T_OperatorID;
				factPay.T_PACID = fact_trans.T_PACID;
				factPay.T_RegisterID = fact_trans.T_RegisterID;
				factPay.T_ShiftID = fact_trans.T_ShiftID;
				factPay.T_StoreID = fact_trans.T_StoreID;
				factPay.T_TerminalID = fact_trans.T_TerminalID;
				factPay.T_TransDate = fact_trans.T_TransDate;
				factPay.T_TransDateKey = fact_trans.T_TransDateKey;
				factPay.T_TransNBTextID = fact_trans.T_TransNBTextID;
				factPay.TransID = fact_trans.TransID;
				pacdb.Insert<Fact_POS_TransactPayment>( factPay);
				System.Threading.Thread.Sleep(Thread_sleep);
				count++;
			}
			return count;
		}

		private void ToFactRetail( Fact_POS_Transact fact, tbl_POS_Retail raw)
		{
			fact.RetailID = raw.RetailID;
			if( raw.R_DVRDate.HasValue)
			{
				fact.R_DVRDateKey = raw.R_DVRDate.Value.Date;
				fact.R_DVRDate = raw.R_DVRDate;
			}

			if( raw.R_Description.HasValue)
			{
				Dim_POS_Description dim_des = GetDim<Dim_POS_Description>(it => it.Description_ID_BK == raw.R_Description);
				fact.R_Description_ID = dim_des.Description_ID;
				fact.R_TransTypeID = Transtype( dim_des.Description_Name);
			}
			if( raw.R_ItemCode.HasValue)
				fact.R_ItemCode_ID = GetDim<Dim_POS_ItemCode, int>(it => it.ItemCode_ID_BK == raw.R_ItemCode.Value, it => it.ItemCode_ID);
			if( raw.R_2ItemLineNb.HasValue)
				fact.R_2ItemLineNb = raw.R_2ItemLineNb;
			if( raw.R_1Qty.HasValue)
				fact.R_1Qty = raw.R_1Qty;
			if( raw.R_0Amount.HasValue)
				fact.R_0Amount = raw.R_0Amount;
			if( raw.R_TOBox.HasValue)
				fact.R_TOBox = raw.R_TOBox;
		}
		
		private void ToFactSubretail( Fact_POS_Transact fact, tbl_POS_SubRetail raw)
		{
			fact.SubRetailID = raw.SubRetailID;
			if( raw.SR_Description.HasValue)
				fact.SR_Description_ID = GetDim<Dim_POS_Description, int>(it => it.Description_ID_BK == raw.SR_Description.Value, it => it.Description_ID);
			fact.SR_2SubItemLineNb = raw.SR_2SubItemLineNb;
			fact.SR_1Qty = raw.SR_1Qty;
			fact.SR_0Amount = raw.SR_0Amount;
		}

		private int? Transtype( string description)
		{
			if( string.IsNullOrEmpty( description))
				return (int?)null;
			IEnumerable<Dim_POS_TransactionType>trantypes = WHManager.GetDims<Dim_POS_TransactionType>( null);
			if( trantypes == null)
				return (int?)null;
			Dim_POS_TransactionType dimtype = trantypes.FirstOrDefault( it => description.IndexOf( it.TransactionTypeName, StringComparison.InvariantCulture) >= 0 );
			return dimtype == null? (int?)null : dimtype.TransactionTypeID;
		}

		private void CloneFactRetail(Fact_POS_Transact source, Fact_POS_Transact des)
		{
			if (source == null)
				return;
			des.RetailID = source.RetailID;
			des.R_DVRDateKey = source.R_DVRDateKey;
			des.R_DVRDate = source.R_DVRDate;
			des.R_Description_ID= source.R_Description_ID;
			des.R_ItemCode_ID = source.R_ItemCode_ID;
			des.R_TransTypeID = source.R_TransTypeID;
			des.R_2ItemLineNb= source.R_2ItemLineNb;
			des.R_1Qty = source.R_1Qty;
			des.R_0Amount = source.R_0Amount;
			des.R_TOBox = source.R_TOBox;
		}

		private void CloneFactTransact(Fact_POS_Transact source, Fact_POS_Transact des)
		{
			if( source == null)
				return;
				des.T_0TransNB = source.T_0TransNB;
				des.T_1SubTotal = source.T_1SubTotal;
				des.T_6TotalAmount = source.T_6TotalAmount;
				des.T_8ChangeAmount = source.T_8ChangeAmount;
				des.T_9RecItemCount = source.T_9RecItemCount;
				des.T_CameraNB = source.T_CameraNB;
				des.T_CardID = source.T_CardID;
				des.T_CheckID = source.T_CheckID;
				des.T_DVRDate = source.T_DVRDate;
				des.T_DVRDateKey = source.T_DVRDateKey;
				des.T_OperatorID = source.T_OperatorID;
				des.T_PACID = source.T_PACID;
				des.T_RegisterID = source.T_RegisterID;
				des.T_ShiftID = source.T_ShiftID;
				des.T_StoreID = source.T_StoreID;
				des.T_TerminalID = source.T_TerminalID;
				des.T_TOBox = source.T_TOBox;
				des.T_TransDate = source.T_TransDate;
				des.T_TransDateKey = source.T_TransDateKey;
				des.T_TransNBTextID = source.T_TransNBTextID;
				des.TransID = source.TransID;
		}


	}
}
