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
    
    public partial class tCMSWeb_CM_SkinColorType
    {
        public tCMSWeb_CM_SkinColorType()
        {
            this.tCMSWeb_CM_BiologicalInfo = new HashSet<tCMSWeb_CM_BiologicalInfo>();
        }
    
        public int SkinColorTypeID { get; set; }
        public string SkinColorName { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_BiologicalInfo> tCMSWeb_CM_BiologicalInfo { get; set; }
    }
}