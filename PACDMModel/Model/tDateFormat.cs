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
    
    public partial class tDateFormat
    {
        public tDateFormat()
        {
            this.tDVRTimeAttributes = new HashSet<tDVRTimeAttributes>();
        }
    
        public short KDateFormat { get; set; }
        public string Format { get; set; }
    
        public virtual ICollection<tDVRTimeAttributes> tDVRTimeAttributes { get; set; }
    }
}
