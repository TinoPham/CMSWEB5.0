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
    
    public partial class Dim_IOPC_ObjectType
    {
        public Dim_IOPC_ObjectType()
        {
            this.Fact_IOPC_Alarm = new HashSet<Fact_IOPC_Alarm>();
            this.Fact_IOPC_Count = new HashSet<Fact_IOPC_Count>();
        }
    
        public int ObjectTypeID { get; set; }
        public int ObjectTypeID_BK { get; set; }
        public string ObjectType { get; set; }
        public string SourceName { get; set; }
    
        public virtual ICollection<Fact_IOPC_Alarm> Fact_IOPC_Alarm { get; set; }
        public virtual ICollection<Fact_IOPC_Count> Fact_IOPC_Count { get; set; }
    }
}
