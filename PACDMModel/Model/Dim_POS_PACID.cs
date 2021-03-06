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
    
    public partial class Dim_POS_PACID
    {
        public Dim_POS_PACID()
        {
            this.Fact_IOPC_Alarm = new HashSet<Fact_IOPC_Alarm>();
            this.Fact_IOPC_Count = new HashSet<Fact_IOPC_Count>();
            this.Fact_IOPC_DriveThrough = new HashSet<Fact_IOPC_DriveThrough>();
            this.Fact_IOPC_Periodic_Hourly_Traffic = new HashSet<Fact_IOPC_Periodic_Hourly_Traffic>();
            this.Fact_IOPC_QTime = new HashSet<Fact_IOPC_QTime>();
            this.Fact_IOPC_TrafficCount = new HashSet<Fact_IOPC_TrafficCount>();
            this.Fact_POS_Periodic_Hourly_Transact = new HashSet<Fact_POS_Periodic_Hourly_Transact>();
            this.Fact_POS_RetailExtraNumber = new HashSet<Fact_POS_RetailExtraNumber>();
            this.Fact_POS_RetailExtraString = new HashSet<Fact_POS_RetailExtraString>();
            this.Fact_POS_Sensor = new HashSet<Fact_POS_Sensor>();
            this.Fact_POS_Transact = new HashSet<Fact_POS_Transact>();
            this.Fact_POS_TransactTax = new HashSet<Fact_POS_TransactTax>();
            this.Fact_POS_TransExtraNumber = new HashSet<Fact_POS_TransExtraNumber>();
            this.Fact_POS_TransExtraString = new HashSet<Fact_POS_TransExtraString>();
            this.Fact_POS_TransactPayment = new HashSet<Fact_POS_TransactPayment>();
            this.Fact_IOPC_LPR_Info = new HashSet<Fact_IOPC_LPR_Info>();
        }
    
        public int PACID_ID { get; set; }
        public int KDVR { get; set; }
        public string PACID_Name { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public bool Stop_Convert { get; set; }
        public bool Updated_ConvertInfo { get; set; }
        public Nullable<System.DateTime> Last_Convert { get; set; }
        public string SPK_KeyID { get; set; }
    
        public virtual ICollection<Fact_IOPC_Alarm> Fact_IOPC_Alarm { get; set; }
        public virtual ICollection<Fact_IOPC_Count> Fact_IOPC_Count { get; set; }
        public virtual ICollection<Fact_IOPC_DriveThrough> Fact_IOPC_DriveThrough { get; set; }
        public virtual ICollection<Fact_IOPC_Periodic_Hourly_Traffic> Fact_IOPC_Periodic_Hourly_Traffic { get; set; }
        public virtual ICollection<Fact_IOPC_QTime> Fact_IOPC_QTime { get; set; }
        public virtual ICollection<Fact_IOPC_TrafficCount> Fact_IOPC_TrafficCount { get; set; }
        public virtual ICollection<Fact_POS_Periodic_Hourly_Transact> Fact_POS_Periodic_Hourly_Transact { get; set; }
        public virtual ICollection<Fact_POS_RetailExtraNumber> Fact_POS_RetailExtraNumber { get; set; }
        public virtual ICollection<Fact_POS_RetailExtraString> Fact_POS_RetailExtraString { get; set; }
        public virtual ICollection<Fact_POS_Sensor> Fact_POS_Sensor { get; set; }
        public virtual ICollection<Fact_POS_Transact> Fact_POS_Transact { get; set; }
        public virtual ICollection<Fact_POS_TransactTax> Fact_POS_TransactTax { get; set; }
        public virtual ICollection<Fact_POS_TransExtraNumber> Fact_POS_TransExtraNumber { get; set; }
        public virtual ICollection<Fact_POS_TransExtraString> Fact_POS_TransExtraString { get; set; }
        public virtual ICollection<Fact_POS_TransactPayment> Fact_POS_TransactPayment { get; set; }
        public virtual ICollection<Fact_IOPC_LPR_Info> Fact_IOPC_LPR_Info { get; set; }
    }
}
