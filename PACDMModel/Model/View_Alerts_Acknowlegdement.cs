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
    
    public partial class View_Alerts_Acknowlegdement
    {
        public int KAlertEvent { get; set; }
        public Nullable<int> KDVR { get; set; }
        public byte KAlertType { get; set; }
        public Nullable<System.DateTime> TimeZone { get; set; }
        public string DVRUser { get; set; }
        public System.DateTime Time { get; set; }
        public int KChannel { get; set; }
        public Nullable<int> ChannelNo { get; set; }
        public string Name { get; set; }
        public bool IsManual { get; set; }
        public int FixEventID { get; set; }
        public string Detail { get; set; }
        public string Description { get; set; }
        public string AlertType { get; set; }
        public string Image { get; set; }
        public Nullable<int> LastKAlertEvent { get; set; }
    }
}
