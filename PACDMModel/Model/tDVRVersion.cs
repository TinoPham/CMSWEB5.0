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
    
    public partial class tDVRVersion
    {
        public tDVRVersion()
        {
            this.tDVRAddressBooks = new HashSet<tDVRAddressBook>();
        }
    
        public int KDVRVersion { get; set; }
        public int Version { get; set; }
        public int Product { get; set; }
        public string DVRFullName { get; set; }
    
        public virtual ICollection<tDVRAddressBook> tDVRAddressBooks { get; set; }
    }
}