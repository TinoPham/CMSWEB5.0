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
    
    public partial class tbl_POS_TaxesList
    {
        public tbl_POS_TaxesList()
        {
            this.tbl_POS_TransTaxes = new HashSet<tbl_POS_TransTaxes>();
            this.tbl_Exception_TransTaxes = new HashSet<tbl_Exception_TransTaxes>();
        }
    
        public int TaxID { get; set; }
        public string TaxName { get; set; }
    
        public virtual ICollection<tbl_POS_TransTaxes> tbl_POS_TransTaxes { get; set; }
        public virtual ICollection<tbl_Exception_TransTaxes> tbl_Exception_TransTaxes { get; set; }
    }
}
