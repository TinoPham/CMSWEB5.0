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
    
    public partial class tDVRIGTrackers
    {
        public int KTracker { get; set; }
        public int KChannel { get; set; }
        public short StartX { get; set; }
        public short StartY { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public bool EnSendEmail { get; set; }
        public bool EnableSound { get; set; }
        public string WaveFile { get; set; }
        public bool UpdateBackground { get; set; }
        public string AreaName { get; set; }
        public short DetectionSize { get; set; }
        public short DetectionSense { get; set; }
        public short SoundDuration { get; set; }
        public short FreshTime { get; set; }
        public int ActiveTime { get; set; }
        public int EnableControl { get; set; }
        public byte ControlNumber { get; set; }
        public byte ControlHour { get; set; }
        public byte ControlMinute { get; set; }
        public byte ControlSecond { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
    }
}
