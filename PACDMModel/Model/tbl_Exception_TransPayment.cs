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
    using System.Collections.Generic;
    
    public partial class tbl_Exception_TransPayment
    {
        public long TransID { get; set; }
        public int PaymentID { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
    
        public virtual tbl_Exception_Transact tbl_Exception_Transact { get; set; }
        public virtual tbl_POS_PaymentList tbl_POS_PaymentList { get; set; }
    }
}
