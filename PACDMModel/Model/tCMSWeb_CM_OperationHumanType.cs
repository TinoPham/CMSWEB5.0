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
    
    public partial class tCMSWeb_CM_OperationHumanType
    {
        public tCMSWeb_CM_OperationHumanType()
        {
            this.tCMSWeb_CM_OperationalInfo = new HashSet<tCMSWeb_CM_OperationalInfo>();
        }
    
        public int OpIssueHumanTypeID { get; set; }
        public Nullable<int> PolicyViolationTypeID { get; set; }
        public Nullable<bool> PolicyViolationHRContacted { get; set; }
        public Nullable<bool> PolicyViolationWrittenCounselingConducted { get; set; }
        public string InvestigatedBy { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_OperationalInfo> tCMSWeb_CM_OperationalInfo { get; set; }
        public virtual tCMSWeb_CM_PolicyViolationType tCMSWeb_CM_PolicyViolationType { get; set; }
    }
}
