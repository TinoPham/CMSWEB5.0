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
    
    public partial class tbl_IOPC_AlarmAlarmType
    {
        public tbl_IOPC_AlarmAlarmType()
        {
            this.tbl_IOPC_Alarm = new HashSet<tbl_IOPC_Alarm>();
        }
    
        public int AlarmTypeID { get; set; }
        public string AlarmType { get; set; }
    
        public virtual ICollection<tbl_IOPC_Alarm> tbl_IOPC_Alarm { get; set; }
    }
}
