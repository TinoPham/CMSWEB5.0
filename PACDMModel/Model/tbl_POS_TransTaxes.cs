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
    
    public partial class tbl_POS_TransTaxes
    {
        public int TaxID { get; set; }
        public long TransID { get; set; }
        public Nullable<decimal> TaxAmount { get; set; }
    
        public virtual tbl_POS_TaxesList tbl_POS_TaxesList { get; set; }
        public virtual tbl_POS_Transact tbl_POS_Transact { get; set; }
    }
}
