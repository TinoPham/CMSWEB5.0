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
    
    public partial class tCMSWeb_FiscalYear_Types
    {
        public tCMSWeb_FiscalYear_Types()
        {
            this.tCMSWeb_FiscalYear = new HashSet<tCMSWeb_FiscalYear>();
        }
    
        public int FYTypesID { get; set; }
        public string FYTylesName { get; set; }
    
        public virtual ICollection<tCMSWeb_FiscalYear> tCMSWeb_FiscalYear { get; set; }
    }
}