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
    
    public partial class tCMSWeb_CM_LocationInjury
    {
        public tCMSWeb_CM_LocationInjury()
        {
            this.tCMSWeb_CM_InjuriedPerson = new HashSet<tCMSWeb_CM_InjuriedPerson>();
        }
    
        public int LocationID { get; set; }
        public string LocationName { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_InjuriedPerson> tCMSWeb_CM_InjuriedPerson { get; set; }
    }
}