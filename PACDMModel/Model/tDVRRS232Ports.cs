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
    
    public partial class tDVRRS232Ports
    {
        public tDVRRS232Ports()
        {
            this.tDVRSystemInfoes = new HashSet<tDVRSystemInfo>();
        }
    
        public int KDVR { get; set; }
        public int KPort { get; set; }
        public string PortName { get; set; }
    
        public virtual tDVRAddressBook tDVRAddressBook { get; set; }
        public virtual ICollection<tDVRSystemInfo> tDVRSystemInfoes { get; set; }
    }
}