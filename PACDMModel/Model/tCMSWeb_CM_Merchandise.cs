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
    
    public partial class tCMSWeb_CM_Merchandise
    {
        public tCMSWeb_CM_Merchandise()
        {
            this.tCMSWeb_CM_StockInfo = new HashSet<tCMSWeb_CM_StockInfo>();
        }
    
        public int MerchandiseID { get; set; }
        public Nullable<bool> SecurityTag { get; set; }
        public Nullable<decimal> TotalCost { get; set; }
        public Nullable<decimal> Loss { get; set; }
        public Nullable<bool> Recovered { get; set; }
        public Nullable<int> ObjectConditionID { get; set; }
        public Nullable<int> StockInfoID { get; set; }
        public Nullable<int> DispositionID { get; set; }
    
        public virtual tCMSWeb_CM_Disposition tCMSWeb_CM_Disposition { get; set; }
        public virtual ICollection<tCMSWeb_CM_StockInfo> tCMSWeb_CM_StockInfo { get; set; }
        public virtual tCMSWeb_CM_ObjectCondition tCMSWeb_CM_ObjectCondition { get; set; }
        public virtual tCMSWeb_CM_StockInfo tCMSWeb_CM_StockInfo_Owner { get; set; }
    }
}
