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
    
    public partial class tbl_IOPC_AlarmArea
    {
        public tbl_IOPC_AlarmArea()
        {
            this.tbl_IOPC_Alarm = new HashSet<tbl_IOPC_Alarm>();
        }
    
        public int AreaID { get; set; }
        public string AreaName { get; set; }
    
        public virtual ICollection<tbl_IOPC_Alarm> tbl_IOPC_Alarm { get; set; }
    }
}