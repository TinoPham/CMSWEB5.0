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
    
    public partial class tCMSWeb_CM_LossInjury_SectionOne
    {
        public tCMSWeb_CM_LossInjury_SectionOne()
        {
            this.tCMSWeb_CM_LossInjury_SectionTwo = new HashSet<tCMSWeb_CM_LossInjury_SectionTwo>();
        }
    
        public int SectionOneID { get; set; }
        public string School { get; set; }
        public string Area { get; set; }
        public Nullable<System.DateTime> DateTimeEvent { get; set; }
        public Nullable<int> LossTypeID { get; set; }
        public string LossTypeOther { get; set; }
        public string MeansofEntry { get; set; }
        public string Description { get; set; }
        public Nullable<bool> PoliceContact { get; set; }
        public string PoliceName { get; set; }
        public string PoliceBadge { get; set; }
        public string PoliceIncidentNo { get; set; }
        public Nullable<int> OfficeUseID { get; set; }
    
        public virtual tCMSWeb_CM_LossInjury_OfficeUse tCMSWeb_CM_LossInjury_OfficeUse { get; set; }
        public virtual tCMSWeb_CM_LossType tCMSWeb_CM_LossType { get; set; }
        public virtual ICollection<tCMSWeb_CM_LossInjury_SectionTwo> tCMSWeb_CM_LossInjury_SectionTwo { get; set; }
    }
}