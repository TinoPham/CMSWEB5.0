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
    
    public partial class tCMSWeb_CM_OperationSystemType
    {
        public tCMSWeb_CM_OperationSystemType()
        {
            this.tCMSWeb_CM_OperationalInfo = new HashSet<tCMSWeb_CM_OperationalInfo>();
        }
    
        public int OpIssueSystemTypeID { get; set; }
        public Nullable<bool> OpIssueSystemSoftware { get; set; }
        public Nullable<bool> OpIssueSystemPOS { get; set; }
        public Nullable<bool> OpIssueSystemComp { get; set; }
        public Nullable<bool> OpIssueReceiving { get; set; }
        public Nullable<bool> OpIssueInventory { get; set; }
        public Nullable<bool> OpIssueFinance { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_OperationalInfo> tCMSWeb_CM_OperationalInfo { get; set; }
    }
}
