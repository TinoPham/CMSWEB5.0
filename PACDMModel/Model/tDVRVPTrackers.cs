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
    
    public partial class tDVRVPTrackers
    {
        public tDVRVPTrackers()
        {
            this.tDVRVPPoints = new HashSet<tDVRVPPoints>();
        }
    
        public int KVPTracker { get; set; }
        public int KChannel { get; set; }
        public Nullable<short> TrackerNo { get; set; }
        public string UserName { get; set; }
        public Nullable<int> MaskType { get; set; }
        public Nullable<int> MaskColor { get; set; }
        public Nullable<int> RepeatType { get; set; }
        public Nullable<int> BeginYear { get; set; }
        public Nullable<int> BeginMonth { get; set; }
        public Nullable<int> BeginDay { get; set; }
        public Nullable<int> BeginDayOfWeek { get; set; }
        public Nullable<int> EndYear { get; set; }
        public Nullable<int> EndMonth { get; set; }
        public Nullable<int> EndDay { get; set; }
        public Nullable<int> EndDayOfWeek { get; set; }
        public Nullable<System.DateTime> BeginTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<short> TrackerType { get; set; }
        public Nullable<int> NumberOfPoint { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
        public virtual ICollection<tDVRVPPoints> tDVRVPPoints { get; set; }
    }
}
