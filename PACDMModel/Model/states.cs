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
    
    public partial class states
    {
        public states()
        {
            this.tCMSWebSites = new HashSet<tCMSWebSites>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> sort { get; set; }
        public string country { get; set; }
        public string Code { get; set; }
    
        public virtual ICollection<tCMSWebSites> tCMSWebSites { get; set; }
    }
}