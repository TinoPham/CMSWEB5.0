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
    
    public partial class tCMSWeb_Modules
    {
        public tCMSWeb_Modules()
        {
            this.tCMSWeb_Functions = new HashSet<tCMSWeb_Functions>();
        }
    
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public Nullable<short> ModuleOrder { get; set; }
        public string ModuleText { get; set; }
        public string ModuleResx { get; set; }
    
        public virtual ICollection<tCMSWeb_Functions> tCMSWeb_Functions { get; set; }
    }
}