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
    
    public partial class tAlertEventLast
    {
        public int KDVR { get; set; }
        public byte KAlertType { get; set; }
        public int KAlertEvent { get; set; }
        public System.DateTime Time { get; set; }
        public System.DateTime TimeZone { get; set; }
    
        public virtual tAlertEvent tAlertEvent { get; set; }
    }
}