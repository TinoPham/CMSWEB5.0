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
    
    public partial class Fact_IOPC_Periodic_Hourly_Traffic
    {
        public int PACID { get; set; }
        public System.DateTime DVRDateKey { get; set; }
        public int C_Hour { get; set; }
        public int CameraID { get; set; }
        public Nullable<int> Count_IN { get; set; }
        public Nullable<int> Count_OUT { get; set; }
        public Nullable<int> Count_IN_N { get; set; }
        public Nullable<int> Count_OUT_N { get; set; }
        public Nullable<bool> Normalize { get; set; }
        public Nullable<bool> ReportNormalize { get; set; }
        public Nullable<int> UpdateTimeInt { get; set; }
    
        public virtual Dim_POS_CameraNB Dim_POS_CameraNB { get; set; }
        public virtual Dim_POS_Date Dim_POS_Date { get; set; }
        public virtual Dim_POS_PACID Dim_POS_PACID { get; set; }
    }
}
