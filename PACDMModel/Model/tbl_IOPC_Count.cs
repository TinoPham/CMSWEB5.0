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
    
    public partial class tbl_IOPC_Count
    {
        public Nullable<int> C_CameraNumber { get; set; }
        public Nullable<int> C_AreaNameID { get; set; }
        public Nullable<int> C_ObjectTypeID { get; set; }
        public Nullable<long> C_Count { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public Nullable<int> T_PACID { get; set; }
        public long Count_ID { get; set; }
    
        public virtual tbl_IOPC_Count_Area tbl_IOPC_Count_Area { get; set; }
        public virtual tbl_IOPC_Count_ObjectType tbl_IOPC_Count_ObjectType { get; set; }
        public virtual tbl_POS_CameraNBList tbl_POS_CameraNBList { get; set; }
        public virtual tbl_POS_PACID tbl_POS_PACID { get; set; }
    }
}
