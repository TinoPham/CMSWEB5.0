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
    
    public partial class tCMSWeb_CalendarEvents_Sites
    {
        public int ECSID { get; set; }
        public Nullable<int> SiteID { get; set; }
        public Nullable<int> ECalID { get; set; }
    
        public virtual tCMSWeb_CalendarEvents tCMSWeb_CalendarEvents { get; set; }
    }
}
