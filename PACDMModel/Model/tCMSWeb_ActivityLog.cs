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
    
    public partial class tCMSWeb_ActivityLog
    {
        public System.Guid ActivityLogID { get; set; }
        public int UserID { get; set; }
        public Nullable<System.DateTime> ActivityDate { get; set; }
        public string PageURL { get; set; }
        public string IPAddress { get; set; }
        public string Controller { get; set; }
        public string Method { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
    }
}