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
    
    public partial class tCMSWeb_CM_CaseType
    {
        public tCMSWeb_CM_CaseType()
        {
            this.tCMSWeb_CM_CaseEvent = new HashSet<tCMSWeb_CM_CaseEvent>();
            this.tCMSWeb_CM_IncidentField = new HashSet<tCMSWeb_CM_IncidentField>();
        }
    
        public int CaseTypeID { get; set; }
        public string CaseTypeName { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_CaseEvent> tCMSWeb_CM_CaseEvent { get; set; }
        public virtual ICollection<tCMSWeb_CM_IncidentField> tCMSWeb_CM_IncidentField { get; set; }
    }
}
