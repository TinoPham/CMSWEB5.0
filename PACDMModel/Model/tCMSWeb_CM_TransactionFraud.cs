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
    
    public partial class tCMSWeb_CM_TransactionFraud
    {
        public int TransactionFraudID { get; set; }
        public Nullable<int> FraudTypeID { get; set; }
        public Nullable<int> CaseTransactID { get; set; }
    
        public virtual tCMSWeb_CM_FraudType tCMSWeb_CM_FraudType { get; set; }
        public virtual tCMSWeb_CM_TransactionInfo tCMSWeb_CM_TransactionInfo { get; set; }
    }
}
