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
    
    public partial class tCMSWeb_CM_EvidenceInfo
    {
        public tCMSWeb_CM_EvidenceInfo()
        {
            this.tCMSWeb_CM_InterviewInfo = new HashSet<tCMSWeb_CM_InterviewInfo>();
        }
    
        public int EvidenceInfoID { get; set; }
        public string EvidenceType { get; set; }
        public string EvidenceAttch { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_InterviewInfo> tCMSWeb_CM_InterviewInfo { get; set; }
    }
}
