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
    
    public partial class tCMSWeb_EX_ReportTimeRange
    {
        public int ReportTimeRangeID { get; set; }
        public Nullable<int> ExceptionReportID { get; set; }
        public Nullable<bool> FiscalYear { get; set; }
        public Nullable<int> ReportType { get; set; }
        public Nullable<int> ReportPeriod { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    
        public virtual tCMSWeb_EX_Report tCMSWeb_EX_Report { get; set; }
    }
}