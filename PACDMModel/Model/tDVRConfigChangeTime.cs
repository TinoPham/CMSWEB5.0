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
    
    public partial class tDVRConfigChangeTime
    {
        public int KDVR { get; set; }
        public byte KConfig { get; set; }
        public Nullable<int> TimeChange { get; set; }
        public Nullable<System.DateTime> CMSTime { get; set; }
        public Nullable<System.DateTime> DVRTime { get; set; }
        public Nullable<long> Checksum { get; set; }
    
        public virtual tCMSConfigPage tCMSConfigPage { get; set; }
        public virtual tDVRAddressBook tDVRAddressBook { get; set; }
    }
}