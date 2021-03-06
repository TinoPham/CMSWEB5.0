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
    
    public partial class tDVRVCTrackers
    {
        public tDVRVCTrackers()
        {
            this.tDVRVCPoints = new HashSet<tDVRVCPoints>();
        }
    
        public int KVCTracker { get; set; }
        public int KChannel { get; set; }
        public Nullable<int> TrackerNo { get; set; }
        public string UserName { get; set; }
        public Nullable<int> TransactionChannel { get; set; }
        public Nullable<int> AlarmWithoutCondition { get; set; }
        public Nullable<int> AlarmConditionMinute { get; set; }
        public Nullable<int> AlarmConditionSecond { get; set; }
        public Nullable<int> RegionType { get; set; }
        public Nullable<int> NumOfPoint { get; set; }
        public Nullable<int> VCPolyType { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
        public virtual ICollection<tDVRVCPoints> tDVRVCPoints { get; set; }
    }
}
