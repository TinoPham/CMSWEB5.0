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
    
    public partial class Proc_Exception_QuickSearch_Result
    {
        public Nullable<int> PACID { get; set; }
        public Nullable<long> TransID { get; set; }
        public Nullable<int> TypeID { get; set; }
        public Nullable<int> EmpID { get; set; }
        public string EmpName { get; set; }
        public Nullable<int> RegisterID { get; set; }
        public string RegisterName { get; set; }
        public Nullable<int> PaymentID { get; set; }
        public string PaymentName { get; set; }
        public Nullable<long> TransNB { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public Nullable<int> DescriptionID { get; set; }
        public string DescriptionName { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public Nullable<System.DateTime> TransDate { get; set; }
        public Nullable<decimal> TAX { get; set; }
        public Nullable<decimal> ChangeAmount { get; set; }
        public Nullable<int> TerminalID { get; set; }
        public Nullable<int> StoreID { get; set; }
        public Nullable<int> CheckID { get; set; }
        public string ExtraString { get; set; }
        public Nullable<decimal> ExtraNumber { get; set; }
        public Nullable<bool> Tracking { get; set; }
    }
}
