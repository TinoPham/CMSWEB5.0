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
    
    public partial class tbl_IOPC_Alarm
    {
        public Nullable<int> A_CameraNumber { get; set; }
        public Nullable<int> AreaID { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public Nullable<int> ObjectTypeID { get; set; }
        public int AlarmTypeID { get; set; }
        public Nullable<int> T_PACID { get; set; }
        public long Alarm_ID { get; set; }
    
        public virtual tbl_IOPC_AlarmAlarmType tbl_IOPC_AlarmAlarmType { get; set; }
        public virtual tbl_IOPC_AlarmArea tbl_IOPC_AlarmArea { get; set; }
        public virtual tbl_IOPC_AlarmObjectType tbl_IOPC_AlarmObjectType { get; set; }
        public virtual tbl_POS_CameraNBList tbl_POS_CameraNBList { get; set; }
        public virtual tbl_POS_PACID tbl_POS_PACID { get; set; }
    }
}
