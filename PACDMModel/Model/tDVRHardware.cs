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
    
    public partial class tDVRHardware
    {
        public int KDVR { get; set; }
        public int ExtMonitor { get; set; }
        public int Dwell { get; set; }
        public long InputMask { get; set; }
        public int HPKCode { get; set; }
        public int HPKPAC { get; set; }
        public long HPKVideoFPS { get; set; }
        public int HPKAudio { get; set; }
        public int HPKIPCamera { get; set; }
        public int PreRecordingTime { get; set; }
        public int PostRecordingTime { get; set; }
        public int EnableAlarm { get; set; }
        public Nullable<int> NumPtzType { get; set; }
        public Nullable<int> CCBoardNum { get; set; }
        public Nullable<int> CCChipNum { get; set; }
        public Nullable<int> KVideoFormat { get; set; }
        public Nullable<int> CardType { get; set; }
        public Nullable<int> HPKVideoLogix { get; set; }
        public Nullable<int> HPKVisionCount { get; set; }
        public Nullable<int> HPKLPR { get; set; }
        public Nullable<int> HPKVideoLogixBasic { get; set; }
        public Nullable<int> HPKVisionCountBasic { get; set; }
        public Nullable<int> CCEncode { get; set; }
        public Nullable<int> HPKCMSMode { get; set; }
        public Nullable<int> HPKControlCount { get; set; }
        public Nullable<int> HPKSensorCount { get; set; }
        public Nullable<int> EnableExtMonitor { get; set; }
        public Nullable<int> MonitorSensor { get; set; }
        public Nullable<int> NumMonitor { get; set; }
        public Nullable<int> HPKAnalog { get; set; }
        public Nullable<int> HPKMonitor { get; set; }
        public Nullable<int> HPKFaceBlur { get; set; }
        public Nullable<int> HPKISearch { get; set; }
        public Nullable<int> HPKVersion { get; set; }
        public Nullable<int> HPKMaxConnection { get; set; }
        public Nullable<int> HPKUpgradable { get; set; }
        public Nullable<int> HPKHeatmap { get; set; }
    
        public virtual tDVRAddressBook tDVRAddressBook { get; set; }
    }
}
