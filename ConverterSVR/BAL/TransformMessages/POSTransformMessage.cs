using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;
using PACDMModel.Model;

namespace ConverterSVR.BAL.TransformMessages
{
	internal class POSSensorTransformMessage : Commons.SingletonClassBase<POSSensorTransformMessage>, ITransformMessage<Sensor, tbl_POS_Sensor>
	{
		//private static readonly Lazy<POSSensorTransformMessage> Lazy = new Lazy<POSSensorTransformMessage>(() => new POSSensorTransformMessage());
		//public static POSSensorTransformMessage Instance { get { return Lazy.Value; } }
		const string STR_TIMEFORMAT = "HH:mm:ss:fff";
		private DateTime? AppendTime(DateTime dvrDate, string time)
		{
			DateTime _temp = new DateTime();
			if (time == null) return null;
			_temp = DateTime.ParseExact(time, STR_TIMEFORMAT , null);
			dvrDate = dvrDate.Date.AddHours(_temp.Hour).AddMinutes(_temp.Minute).AddSeconds(_temp.Second).AddMilliseconds(_temp.Millisecond);
			return dvrDate;

		}
		public tbl_POS_Sensor TransForm(Sensor input, MessageDVRInfo DVRInfo)
		{
			tbl_POS_Sensor output = new tbl_POS_Sensor();
			output.DVRDate = input.DVRDate;
			output.T_PACID = DVRInfo.KDVR;
			
			output.GT_End = AppendTime(input.DVRDate.Value, input.GT_End);
			output.GT_Start = AppendTime(input.DVRDate.Value, input.GT_Start);
			
			output.OT_End = AppendTime(input.DVRDate.Value, input.OT_End);
			output.OT_Start = AppendTime(input.DVRDate.Value, input.OT_Start);

			output.PU_End = AppendTime(input.DVRDate.Value, input.PU_End);
			output.PU_Start = AppendTime(input.DVRDate.Value, input.PU_Start);

			output.S_ID_GUI = input.S_ID;
			output.T_PACID = DVRInfo.KDVR;

			return output;
		}
	}

	internal class POSTransactTransformMessage : Commons.SingletonClassBase<POSTransactTransformMessage>, ITransformMessage<Transact, tbl_POS_Transact>
	{
		//private static readonly Lazy<POSTransactTransformMessage> Lazy = new Lazy<POSTransactTransformMessage>(() => new POSTransactTransformMessage());
		
		//public static POSTransactTransformMessage Instance { get { return Lazy.Value; } }
		public tbl_POS_Transact TransForm(Transact input, MessageDVRInfo DVRInfo)
		{
			tbl_POS_Transact output = new tbl_POS_Transact();
			output.DVRDate = Commons.Utils.toSQLDate( input.DVRDate);
			output.T_00TransNBText = input.T_00TransNBText;
			output.T_0TransNB = input.T_0TransNB;
			output.T_1SubTotal = input.T_1SubTotal;
			output.T_6TotalAmount = input.T_6TotalAmount;
			output.T_8ChangeAmount = input.T_8ChangeAmount;
			output.T_9RecItemCount = input.T_9RecItemCount;
			output.T_CameraNB = input.T_CameraNB;
			output.T_CardID = input.T_CardID;
			output.T_CheckID = input.T_CheckID;
			output.T_OperatorID = input.T_OperatorID;
			output.T_PACID = DVRInfo.KDVR;
			output.T_RegisterID = input.T_RegisterID;
			output.T_ShiftID = input.T_ShiftID;
			output.T_StoreID = input.T_StoreID;
			output.T_TerminalID = input.T_TerminalID;
			output.T_TOBox = input.T_TOBox;
			output.TransactKey = input.TransactKey;
			output.TransDate = Commons.Utils.toSQLDate( input.TransDate);
			if(input.ExNumbers != null)
				input.ExNumbers.ForEach( item => output.tbl_POS_TransExtraNumber.Add( new tbl_POS_TransExtraNumber{ ExtraID = item.Key, ExNum_Value = (decimal?)item.Value, tbl_POS_Transact = output} ) );
			if(input.ExStrings != null)
				input.ExStrings.ForEach(item => output.tbl_POS_TransExtraString.Add(new tbl_POS_TransExtraString { ExtraID = item.Key, ExString_ValueID = item.Value, tbl_POS_Transact = output }));
			if( input.Payments != null)
				input.Payments.ForEach(item => output.tbl_POS_TransPayment.Add(new tbl_POS_TransPayment { PaymentID = item.Key, PaymentAmount = (decimal?)item.Value, tbl_POS_Transact = output }));
			if( input.Taxes != null)
				input.Taxes.ForEach( item => output.tbl_POS_TransTaxes.Add( new tbl_POS_TransTaxes{ TaxID = item.Key, TaxAmount = (decimal?)item.Value, tbl_POS_Transact = output} ) );

			if( input.Retails != null)
				input.Retails.ForEach(
										//item => output.tbl_POS_Retail.Add(POSRetailTransformMessage.Instance.TransForm(item, DVRInfo))
										delegate(Retail item)
										{
											tbl_POS_Retail rtail = POSRetailTransformMessage.Instance.TransForm(item, DVRInfo);
											if (rtail != null)
											{
												rtail.tbl_POS_Transact = output;
												output.tbl_POS_Retail.Add(rtail);
											}
										}
									);
			output.T_PACID = DVRInfo.KDVR;
			return output;
		}

		class POSSubRetailTransformMessage : Commons.SingletonClassBase<POSSubRetailTransformMessage>, ITransformMessage<SubRetail, tbl_POS_SubRetail>
		{
			//private static readonly Lazy<POSSubRetailTransformMessage> Lazy = new Lazy<POSSubRetailTransformMessage>(() => new POSSubRetailTransformMessage());
			//public static POSSubRetailTransformMessage Instance { get { return Lazy.Value; } }

			public tbl_POS_SubRetail TransForm(SubRetail input, MessageDVRInfo DVRInfo)
			{
				return new tbl_POS_SubRetail
											{
											SR_0Amount = input.SR_0Amount
											, SR_1Qty = input.SR_1Qty
											, SR_2SubItemLineNb = input.SR_2SubItemLineNb
											, SR_Description = input.SR_Description
											};
			}
		}

		class POSRetailTransformMessage : Commons.SingletonClassBase<POSRetailTransformMessage>, ITransformMessage<Retail, tbl_POS_Retail>
		{
			//private static readonly Lazy<POSRetailTransformMessage> Lazy = new Lazy<POSRetailTransformMessage>(() => new POSRetailTransformMessage());
			//public static POSRetailTransformMessage Instance { get { return Lazy.Value; } }

			public tbl_POS_Retail TransForm(Retail input, MessageDVRInfo DVRInfo)
			{
				tbl_POS_Retail output = new tbl_POS_Retail();
				output.R_0Amount = input.R_0Amount;
				output.R_1Qty = input.R_1Qty;
				output.R_2ItemLineNb = input.R_2ItemLineNb;
				output.R_Description = input.R_Description;
				output.R_DVRDate = Commons.Utils.toSQLDate( input.R_DVRDate);
				output.R_ItemCode = input.R_ItemCode;
				output.R_TOBox = input.R_TOBox;
				output.RetailKey = input.RetailKey;
				if (input.SubRetails != null)
				{
					input.SubRetails.ForEach
						(
							delegate(SubRetail item)
							{
								tbl_POS_SubRetail srtail = POSSubRetailTransformMessage.Instance.TransForm(item, DVRInfo);
								if( srtail != null)
								{
									srtail.tbl_POS_Retail = output;
									output.tbl_POS_SubRetail.Add(srtail);
								}
							}
					);
					//item => output.tbl_POS_SubRetail.Add(POSSubRetailTransformMessage.Instance.TransForm(item, DVRInfo)));
				}
				if (input.ExNumbers != null)
					input.ExNumbers.ForEach(item => output.tbl_POS_RetailExtraNumber.Add(new tbl_POS_RetailExtraNumber { ExtraID = item.Key, ExNum_Value = (decimal?)item.Value, tbl_POS_Retail = output }));
				if (input.ExStrings != null)
					input.ExStrings.ForEach(item => output.tbl_POS_RetailExtraString.Add(new tbl_POS_RetailExtraString { ExString_ValueID = item.Value, ExtraID = item.Key, tbl_POS_Retail = output }));
				return output;
			}
		}
	}
}
