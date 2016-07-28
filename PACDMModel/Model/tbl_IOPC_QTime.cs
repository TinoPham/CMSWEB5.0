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
    
    public partial class tbl_IOPC_QTime
    {
        public long EventID { get; set; }
        public Nullable<long> PersonID { get; set; }
        public Nullable<System.DateTime> ServiceEnterTime { get; set; }
        public Nullable<System.DateTime> RegisterEnterTime { get; set; }
        public Nullable<System.DateTime> RegisterExitTime { get; set; }
        public Nullable<System.DateTime> PickupEnterTime { get; set; }
        public Nullable<System.DateTime> PickupExitTime { get; set; }
        public Nullable<System.DateTime> ServiceExitTime { get; set; }
        public Nullable<int> ExternalChannel { get; set; }
        public Nullable<int> InternalChannel { get; set; }
        public int T_PACID { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
    
        public virtual tbl_POS_CameraNBList tbl_POS_CameraNBList { get; set; }
        public virtual tbl_POS_PACID tbl_POS_PACID { get; set; }
    }
}