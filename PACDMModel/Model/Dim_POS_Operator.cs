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
    
    public partial class Dim_POS_Operator
    {
        public Dim_POS_Operator()
        {
            this.Fact_POS_RetailExtraNumber = new HashSet<Fact_POS_RetailExtraNumber>();
            this.Fact_POS_RetailExtraString = new HashSet<Fact_POS_RetailExtraString>();
            this.Fact_POS_Transact = new HashSet<Fact_POS_Transact>();
            this.Fact_POS_TransactTax = new HashSet<Fact_POS_TransactTax>();
            this.Fact_POS_TransExtraNumber = new HashSet<Fact_POS_TransExtraNumber>();
            this.Fact_POS_TransExtraString = new HashSet<Fact_POS_TransExtraString>();
            this.Fact_POS_TransactPayment = new HashSet<Fact_POS_TransactPayment>();
        }
    
        public int Operator_ID { get; set; }
        public int Operator_BK { get; set; }
        public string Operator_Name { get; set; }
    
        public virtual ICollection<Fact_POS_RetailExtraNumber> Fact_POS_RetailExtraNumber { get; set; }
        public virtual ICollection<Fact_POS_RetailExtraString> Fact_POS_RetailExtraString { get; set; }
        public virtual ICollection<Fact_POS_Transact> Fact_POS_Transact { get; set; }
        public virtual ICollection<Fact_POS_TransactTax> Fact_POS_TransactTax { get; set; }
        public virtual ICollection<Fact_POS_TransExtraNumber> Fact_POS_TransExtraNumber { get; set; }
        public virtual ICollection<Fact_POS_TransExtraString> Fact_POS_TransExtraString { get; set; }
        public virtual ICollection<Fact_POS_TransactPayment> Fact_POS_TransactPayment { get; set; }
    }
}
