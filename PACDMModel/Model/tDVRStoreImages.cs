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
    
    public partial class tDVRStoreImages
    {
        public int KImage { get; set; }
        public int KAlert { get; set; }
        public string DVRGuid { get; set; }
        public Nullable<int> ChannelID { get; set; }
        public Nullable<System.DateTime> DVRTime { get; set; }
        public string ImageName { get; set; }
    
        public virtual tAlertEvent tAlertEvent { get; set; }
    }
}
