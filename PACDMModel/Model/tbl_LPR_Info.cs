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
    
    public partial class tbl_LPR_Info
    {
        public long LPR_ID { get; set; }
        public Nullable<int> CamNo { get; set; }
        public Nullable<System.DateTime> DVRDate { get; set; }
        public string LPR_Num { get; set; }
        public string LPR_Possibility { get; set; }
        public int LPR_PACID { get; set; }
        public string LPR_isMatch { get; set; }
        public byte[] LPR_Image { get; set; }
        public string LPR_ImageName { get; set; }
    
        public virtual tbl_POS_CameraNBList tbl_POS_CameraNBList { get; set; }
        public virtual tbl_POS_PACID tbl_POS_PACID { get; set; }
    }
}
