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
    
    public partial class tDVRIGChannel
    {
        public int KChannel { get; set; }
        public short IntervalAlarm { get; set; }
    
        public virtual tDVRChannels tDVRChannels { get; set; }
    }
}