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
    
    public partial class tbl_POS_OperatorList
    {
        public tbl_POS_OperatorList()
        {
            this.tbl_POS_Transact = new HashSet<tbl_POS_Transact>();
            this.tbl_Exception_Transact = new HashSet<tbl_Exception_Transact>();
        }
    
        public int Operator_ID { get; set; }
        public string Operator_Name { get; set; }
    
        public virtual ICollection<tbl_POS_Transact> tbl_POS_Transact { get; set; }
        public virtual ICollection<tbl_Exception_Transact> tbl_Exception_Transact { get; set; }
    }
}
