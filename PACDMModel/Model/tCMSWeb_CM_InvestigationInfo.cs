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
    
    public partial class tCMSWeb_CM_InvestigationInfo
    {
        public tCMSWeb_CM_InvestigationInfo()
        {
            this.tCMSWeb_CM_LawEnforcement = new HashSet<tCMSWeb_CM_LawEnforcement>();
        }
    
        public int InvestInfoID { get; set; }
        public Nullable<System.DateTime> InvestDate { get; set; }
        public string InvestInitiatedBy { get; set; }
        public string InvestConductedBy { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_LawEnforcement> tCMSWeb_CM_LawEnforcement { get; set; }
    }
}