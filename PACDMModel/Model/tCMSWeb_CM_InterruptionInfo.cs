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
    
    public partial class tCMSWeb_CM_InterruptionInfo
    {
        public int InterruptionInfoID { get; set; }
        public Nullable<bool> BusinessClosed { get; set; }
        public Nullable<System.DateTime> InterrStartTime { get; set; }
        public Nullable<System.DateTime> InterrEndTime { get; set; }
        public string ContactedBy { get; set; }
        public string RespondedBy { get; set; }
        public Nullable<bool> FalseAlarm { get; set; }
        public Nullable<int> BITypeID { get; set; }
    
        public virtual tCMSWeb_CM_BusInterruptionType tCMSWeb_CM_BusInterruptionType { get; set; }
    }
}
