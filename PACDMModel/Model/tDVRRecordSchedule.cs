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
    
    public partial class tDVRRecordSchedule
    {
        public int KSchedule { get; set; }
        public int KChannel { get; set; }
        public int ScheduleID { get; set; }
        public string Name { get; set; }
        public int RotationType { get; set; }
        public System.DateTime Date { get; set; }
        public int Size { get; set; }
        public Nullable<int> SubStreamMode { get; set; }
        public string Data { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
    }
}
