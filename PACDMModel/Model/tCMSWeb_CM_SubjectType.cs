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
    
    public partial class tCMSWeb_CM_SubjectType
    {
        public tCMSWeb_CM_SubjectType()
        {
            this.tCMSWeb_CM_SubjectDetail = new HashSet<tCMSWeb_CM_SubjectDetail>();
        }
    
        public int SubjectTypeID { get; set; }
        public string SubjectTypeName { get; set; }
    
        public virtual ICollection<tCMSWeb_CM_SubjectDetail> tCMSWeb_CM_SubjectDetail { get; set; }
    }
}