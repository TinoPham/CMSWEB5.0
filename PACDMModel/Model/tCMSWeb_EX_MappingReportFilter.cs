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
    
    public partial class tCMSWeb_EX_MappingReportFilter
    {
        public int MappingReportID { get; set; }
        public Nullable<int> ReportFilterFieldID { get; set; }
        public Nullable<int> ExceptionReportID { get; set; }
        public string OperationCompare { get; set; }
        public string QueryValue { get; set; }
        public Nullable<int> SortingID { get; set; }
        public string SortingOrder { get; set; }
    
        public virtual tCMSWeb_EX_ReportFilterFields tCMSWeb_EX_ReportFilterFields { get; set; }
        public virtual tCMSWeb_EX_Report tCMSWeb_EX_Report { get; set; }
    }
}
