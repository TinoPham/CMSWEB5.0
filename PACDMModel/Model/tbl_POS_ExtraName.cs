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
    
    public partial class tbl_POS_ExtraName
    {
        public tbl_POS_ExtraName()
        {
            this.tbl_POS_RetailExtraNumber = new HashSet<tbl_POS_RetailExtraNumber>();
            this.tbl_POS_RetailExtraString = new HashSet<tbl_POS_RetailExtraString>();
            this.tbl_POS_TransExtraNumber = new HashSet<tbl_POS_TransExtraNumber>();
            this.tbl_POS_TransExtraString = new HashSet<tbl_POS_TransExtraString>();
        }
    
        public int ExtraID { get; set; }
        public string ExtraName { get; set; }
    
        public virtual ICollection<tbl_POS_RetailExtraNumber> tbl_POS_RetailExtraNumber { get; set; }
        public virtual ICollection<tbl_POS_RetailExtraString> tbl_POS_RetailExtraString { get; set; }
        public virtual ICollection<tbl_POS_TransExtraNumber> tbl_POS_TransExtraNumber { get; set; }
        public virtual ICollection<tbl_POS_TransExtraString> tbl_POS_TransExtraString { get; set; }
    }
}