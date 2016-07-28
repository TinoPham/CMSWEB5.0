using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class QSSummaryModel{
		public int PACID{ get ;set;}
		public long TransID{ get ;set;}
	
		public Nullable<long> T_0TransNB { get; set; }
		public Nullable<decimal> T_6TotalAmount { get; set; }
		public Nullable<decimal> T_1SubTotal { get; set; }
		public Nullable<decimal> T_8ChangeAmount { get; set; }
		public Nullable<System.DateTime> TransDate { get; set; }
		public Nullable<System.DateTime> DVRDate { get; set; }
		public Nullable<int> T_9RecItemCount { get; set; }
		public Nullable<int> T_CameraNB { get; set; }
		public Nullable<int> T_OperatorID { get; set; }
		public Nullable<int> T_StoreID { get; set; }
		public Nullable<int> T_TerminalID { get; set; }
		public Nullable<int> T_RegisterID { get; set; }
		public Nullable<int> T_ShiftID { get; set; }
		public Nullable<int> T_CheckID { get; set; }
		public Nullable<int> T_CardID { get; set; }
		public string T_00TransNBText { get; set; }

		public IEnumerable<RetailSummary> Description { get; set; }
		public IEnumerable<Taxes> Taxes { get; set; }
		public IEnumerable<Payment> Payments { get; set; }
	}
	public class RetailSummary{
		public Nullable<int> Description {get;set;}
		public Nullable<decimal> R_0Amount { get; set; }
	}

	public class TransactionDetailModel
	{
		public long TransID { get; set; }
		public Nullable<long> T_0TransNB { get; set; }
		public Nullable<decimal> T_6TotalAmount { get; set; }
		public Nullable<decimal> T_1SubTotal { get; set; }
		public Nullable<decimal> T_8ChangeAmount { get; set; }
		public Nullable<System.DateTime> TransDate { get; set; }
		public Nullable<System.DateTime> DVRDate { get; set; }
		public Nullable<int> T_9RecItemCount { get; set; }
		public Nullable<int> T_CameraNB { get; set; }
		public Nullable<int> T_OperatorID { get; set; }
		public Nullable<int> T_StoreID { get; set; }
		public Nullable<int> T_TerminalID { get; set; }
		public Nullable<int> T_RegisterID { get; set; }
		public Nullable<int> T_ShiftID { get; set; }
		public Nullable<int> T_CheckID { get; set; }
		public Nullable<int> T_CardID { get; set; }
		public Nullable<int> T_TOBox { get; set; }
		public string T_00TransNBText { get; set; }
		public int T_PACID { get; set; }
		
		public IEnumerable<RetailModel> Retails{ get ;set;}
		public IEnumerable<ExtraNumber> ExtraNumber{ get ;set;}
		public IEnumerable<ExtraString> ExtraString{ get ;set;}
		public IEnumerable<Taxes>Taxes { get ;set;}
		public IEnumerable<Payment> Payments { get ;set;}
	}

	public class RetailModel
	{
		public long RetailID { get; set; }
		public Nullable<int> R_2ItemLineNb { get; set; }
		public Nullable<double> R_1Qty { get; set; }
		public Nullable<decimal> R_0Amount { get; set; }
		public Nullable<int> R_Description { get; set; }
		public Nullable<int> R_ItemCode { get; set; }
		public Nullable<System.DateTime> R_DVRDate { get; set; }
		public IEnumerable<SubRetailModel>SubRetail{ get ;set;}
		public IEnumerable<ExtraNumber> ExtraNumber{ get ;set;}
		public IEnumerable<ExtraString> ExtraString{ get ;set;}
	}

	public class SubRetailModel
	{
		public long SubRetailID { get; set; }
		public Nullable<int> SR_2SubItemLineNb { get; set; }
		public Nullable<double> SR_1Qty { get; set; }
		public Nullable<decimal> SR_0Amount { get; set; }
		public Nullable<int> SR_Description { get; set; }
	}

	public class ExtraNumber
	{
		public int ExtraID { get; set; }
		public Nullable<decimal> ExNum_Value { get; set; }
	}
	public class ExtraString
	{
		public int ExtraID { get; set; }
		public int ExString_ValueID { get; set; }

	}
	public class Taxes {
		public int TaxID { get; set; }
		public Nullable<decimal> TaxAmount { get; set; }
	}
	public class Payment{

        public int PaymentID { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
	}
}
