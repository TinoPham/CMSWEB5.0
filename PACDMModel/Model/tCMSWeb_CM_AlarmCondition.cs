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
    
    public partial class tCMSWeb_CM_AlarmCondition
    {
        public int AlarmConditionID { get; set; }
        public string ContactedBy { get; set; }
        public string RespondedBy { get; set; }
        public Nullable<bool> FireAlarm { get; set; }
        public string Cause { get; set; }
        public Nullable<bool> PoliceContacted { get; set; }
        public Nullable<bool> PropertyDamage { get; set; }
        public Nullable<decimal> Amount { get; set; }
    }
}
