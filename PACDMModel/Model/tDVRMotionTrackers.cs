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
    
    public partial class tDVRMotionTrackers
    {
        public int KTracker { get; set; }
        public int KChannel { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Sensitivity { get; set; }
        public int ControlNo { get; set; }
        public int EnableAlarm { get; set; }
        public int EnableControl { get; set; }
        public int DwellTime { get; set; }
        public int AlarmEndHour { get; set; }
        public int AlarmEndMinute { get; set; }
        public int EnableFullScreen { get; set; }
        public int FullScreenChannelNo { get; set; }
        public int ControlHour { get; set; }
        public int ControlMinute { get; set; }
        public int ControlSecond { get; set; }
        public int AlarmStartHour { get; set; }
        public int AlarmStartMinute { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
    }
}
