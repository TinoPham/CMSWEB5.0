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
    
    public partial class tbl_BAM_Metric_ReportUser
    {
        public short ReportID { get; set; }
        public short MetricID { get; set; }
        public int UserID { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<byte> MetricOrder { get; set; }
    
        public virtual tbl_BAM_Metric tbl_BAM_Metric { get; set; }
        public virtual tbl_BAM_Metric_ReportList tbl_BAM_Metric_ReportList { get; set; }
        public virtual tCMSWeb_UserList tCMSWeb_UserList { get; set; }
    }
}