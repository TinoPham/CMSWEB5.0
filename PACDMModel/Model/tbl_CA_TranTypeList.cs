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
    
    public partial class tbl_CA_TranTypeList
    {
        public tbl_CA_TranTypeList()
        {
            this.tbl_CA_Transact = new HashSet<tbl_CA_Transact>();
        }
    
        public int TranType_ID { get; set; }
        public string TranType_Name { get; set; }
    
        public virtual ICollection<tbl_CA_Transact> tbl_CA_Transact { get; set; }
    }
}
