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
    
    public partial class tbl_ATM_TransTypeList
    {
        public tbl_ATM_TransTypeList()
        {
            this.tbl_ATM_Transact = new HashSet<tbl_ATM_Transact>();
        }
    
        public int TransType_ID { get; set; }
        public string TransType_Name { get; set; }
    
        public virtual ICollection<tbl_ATM_Transact> tbl_ATM_Transact { get; set; }
    }
}
