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
    
    public partial class Fact_POS_RetailExtraNumber
    {
        public long TransID { get; set; }
        public Nullable<System.DateTime> T_DVRDateKey { get; set; }
        public Nullable<System.DateTime> T_DVRDate { get; set; }
        public Nullable<System.DateTime> T_TransDateKey { get; set; }
        public Nullable<System.DateTime> T_TransDate { get; set; }
        public Nullable<int> siteKey { get; set; }
        public int T_PACID { get; set; }
        public Nullable<int> T_CameraNB { get; set; }
        public Nullable<int> T_OperatorID { get; set; }
        public Nullable<int> T_StoreID { get; set; }
        public Nullable<int> T_TerminalID { get; set; }
        public Nullable<int> T_RegisterID { get; set; }
        public Nullable<int> T_ShiftID { get; set; }
        public Nullable<int> T_CheckID { get; set; }
        public Nullable<int> T_CardID { get; set; }
        public Nullable<int> T_TransNBTextID { get; set; }
        public long RetailID { get; set; }
        public Nullable<System.DateTime> R_DVRDateKey { get; set; }
        public Nullable<System.DateTime> R_DVRDate { get; set; }
        public Nullable<int> R_Description_ID { get; set; }
        public Nullable<int> R_ItemCode_ID { get; set; }
        public Nullable<int> R_TransTypeID { get; set; }
        public int ExtraID { get; set; }
        public Nullable<decimal> ExNum_Value { get; set; }
    
        public virtual Dim_POS_CameraNB Dim_POS_CameraNB { get; set; }
        public virtual Dim_POS_CardID Dim_POS_CardID { get; set; }
        public virtual Dim_POS_CheckID Dim_POS_CheckID { get; set; }
        public virtual Dim_POS_Date Dim_POS_Date { get; set; }
        public virtual Dim_POS_Date Dim_POS_Date1 { get; set; }
        public virtual Dim_POS_Date Dim_POS_Date2 { get; set; }
        public virtual Dim_POS_Description Dim_POS_Description { get; set; }
        public virtual Dim_POS_ExtraName Dim_POS_ExtraName { get; set; }
        public virtual Dim_POS_ItemCode Dim_POS_ItemCode { get; set; }
        public virtual Dim_POS_Operator Dim_POS_Operator { get; set; }
        public virtual Dim_POS_PACID Dim_POS_PACID { get; set; }
        public virtual Dim_POS_Register Dim_POS_Register { get; set; }
        public virtual Dim_POS_Shift Dim_POS_Shift { get; set; }
        public virtual Dim_POS_Site Dim_POS_Site { get; set; }
        public virtual Dim_POS_Store Dim_POS_Store { get; set; }
        public virtual Dim_POS_Terminal Dim_POS_Terminal { get; set; }
        public virtual Dim_POS_TransactionType Dim_POS_TransactionType { get; set; }
        public virtual Dim_POS_TransNBText Dim_POS_TransNBText { get; set; }
    }
}
