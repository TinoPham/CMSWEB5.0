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
    
    public partial class tCMSWeb_CM_OperationalInfo
    {
        public int OperationalInfoID { get; set; }
        public Nullable<int> OpIssueHumanTypeID { get; set; }
        public Nullable<int> OpIssueSystemTypeID { get; set; }
        public string OpIssueDetails { get; set; }
    
        public virtual tCMSWeb_CM_OperationHumanType tCMSWeb_CM_OperationHumanType { get; set; }
        public virtual tCMSWeb_CM_OperationSystemType tCMSWeb_CM_OperationSystemType { get; set; }
    }
}
