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
    
    public partial class tCMSWeb_CM_GeneralIncident
    {
        public int GeneralIncidentID { get; set; }
        public Nullable<int> EmergencyEvacuationID { get; set; }
        public Nullable<int> SuspiciousIncidentID { get; set; }
        public Nullable<int> InternetID { get; set; }
        public Nullable<bool> CustomerServiceTypeComplain { get; set; }
        public string Description { get; set; }
    
        public virtual tCMSWEb_CM_EmergencyEvacuation tCMSWEb_CM_EmergencyEvacuation { get; set; }
        public virtual tCMSWeb_CM_Internet tCMSWeb_CM_Internet { get; set; }
        public virtual tCMSWeb_CM_SuspiciousIncident tCMSWeb_CM_SuspiciousIncident { get; set; }
    }
}
