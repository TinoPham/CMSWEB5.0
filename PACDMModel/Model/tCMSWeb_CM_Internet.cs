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
    
    public partial class tCMSWeb_CM_Internet
    {
        public tCMSWeb_CM_Internet()
        {
            this.tCMSWeb_CM_GeneralIncident = new HashSet<tCMSWeb_CM_GeneralIncident>();
        }
    
        public int InternetID { get; set; }
        public string Internet { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_GeneralIncident> tCMSWeb_CM_GeneralIncident { get; set; }
    }
}