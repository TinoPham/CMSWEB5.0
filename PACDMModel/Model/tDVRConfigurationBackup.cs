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
    
    public partial class tDVRConfigurationBackup
    {
        public int KDVR { get; set; }
        public byte KConfig { get; set; }
        public Nullable<System.DateTime> TimeBackup { get; set; }
        public string Configuration { get; set; }
        public string Version { get; set; }
        public Nullable<short> Product { get; set; }
    
        public virtual tCMSConfigPage tCMSConfigPage { get; set; }
        public virtual tDVRAddressBook tDVRAddressBook { get; set; }
    }
}
