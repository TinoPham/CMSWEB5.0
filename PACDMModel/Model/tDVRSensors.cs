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
    
    public partial class tDVRSensors
    {
        public int SensorNo { get; set; }
        public int KDVR { get; set; }
        public int KIOCard { get; set; }
        public int KSensor { get; set; }
        public int Enable { get; set; }
        public Nullable<int> ActivePanicBackup { get; set; }
        public string Name { get; set; }
        public Nullable<int> ActiveEmail { get; set; }
        public Nullable<int> NoNc { get; set; }
        public Nullable<decimal> LinkWithChannel { get; set; }
        public Nullable<decimal> LinkWithControl { get; set; }
        public Nullable<int> RealIndex { get; set; }
        public Nullable<int> SensorType { get; set; }
    
        public virtual tDVRAddressBook tDVRAddressBook { get; set; }
        public virtual tDVRIOCard tDVRIOCard { get; set; }
    }
}