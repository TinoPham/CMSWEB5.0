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
    
    public partial class tDVRVLChannels
    {
        public tDVRVLChannels()
        {
            this.tDVRVLSchedules = new HashSet<tDVRVLSchedules>();
        }
    
        public int KChannel { get; set; }
        public Nullable<int> EnableVL { get; set; }
        public Nullable<int> HideDetection { get; set; }
        public Nullable<int> ControlNumber { get; set; }
        public Nullable<int> MinObjectSize { get; set; }
        public Nullable<int> OverheadCamera { get; set; }
        public Nullable<int> Crowded { get; set; }
        public Nullable<int> NumberOfTracker { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
        public virtual ICollection<tDVRVLSchedules> tDVRVLSchedules { get; set; }
    }
}