//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PACDMModel.Model
{
    using System;
    
    public partial class Proc_Exception_GetReport_Result
    {
        public Nullable<int> T_PACID { get; set; }
        public Nullable<long> TransID { get; set; }
        public Nullable<int> TypeID { get; set; }
        public Nullable<int> T_OperatorID { get; set; }
        public string T_OperatorName { get; set; }
        public Nullable<int> T_RegisterID { get; set; }
        public string T_RegisterName { get; set; }
        public Nullable<int> PaymentID { get; set; }
        public string PaymentName { get; set; }
        public Nullable<long> T_0TransNB { get; set; }
        public Nullable<decimal> T_6TotalAmount { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public Nullable<int> T_CameraNB { get; set; }
        public Nullable<System.DateTime> TransDate { get; set; }
        public Nullable<decimal> Taxes { get; set; }
        public Nullable<decimal> T_8ChangeAmount { get; set; }
        public Nullable<int> T_TerminalID { get; set; }
        public Nullable<int> T_StoreID { get; set; }
        public Nullable<int> T_CheckID { get; set; }
        public string ExtraString { get; set; }
        public Nullable<decimal> ExtraNumber { get; set; }
        public Nullable<bool> Tracking { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
        public Nullable<int> TaxID { get; set; }
    }
}
