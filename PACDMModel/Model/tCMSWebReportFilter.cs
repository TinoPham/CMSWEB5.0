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
    
    public partial class tCMSWebReportFilter
    {
        public int ReportConditionID { get; set; }
        public Nullable<int> ReportKey { get; set; }
        public Nullable<int> Column_ID { get; set; }
        public Nullable<int> Condition_ID { get; set; }
        public string ValueSearch { get; set; }
    
        public virtual tCMSWebReport tCMSWebReport { get; set; }
        public virtual tCMSWebReportColumns tCMSWebReportColumns { get; set; }
    }
}
