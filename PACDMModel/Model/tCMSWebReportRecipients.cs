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
    
    public partial class tCMSWebReportRecipients
    {
        public int CMSReportRecipientID { get; set; }
        public Nullable<int> ReportKey { get; set; }
        public Nullable<int> RecipientID { get; set; }
    
        public virtual tCMSWebReport tCMSWebReport { get; set; }
        public virtual tCMSWebRecipients tCMSWebRecipients { get; set; }
    }
}
