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
    
    public partial class tDVRVLPoints
    {
        public int KVLTracker { get; set; }
        public int VLPointIndex { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    
        public virtual tDVRVLTrackers tDVRVLTrackers { get; set; }
    }
}
