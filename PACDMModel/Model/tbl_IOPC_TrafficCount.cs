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
    
    public partial class tbl_IOPC_TrafficCount
    {
        public System.Guid EventGUI { get; set; }
        public int EventID { get; set; }
        public int RegionIndex { get; set; }
        public Nullable<long> PersonID { get; set; }
        public System.DateTime RegionEnterTime { get; set; }
        public System.DateTime RegionExitTime { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public long EventAutoID { get; set; }
    
        public virtual tbl_IOPC_TrafficCountRegion tbl_IOPC_TrafficCountRegion { get; set; }
    }
}
