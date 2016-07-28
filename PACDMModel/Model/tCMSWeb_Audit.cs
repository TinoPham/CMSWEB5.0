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
    
    public partial class tCMSWeb_Audit
    {
        public tCMSWeb_Audit()
        {
            this.tDVRAddressBook = new HashSet<tDVRAddressBook>();
            this.tCMSWeb_Audit_Media = new HashSet<tCMSWeb_Audit_Media>();
            this.tDVRChannels = new HashSet<tDVRChannels>();
            this.tCMSWebRecipients = new HashSet<tCMSWebRecipients>();
        }
    
        public int SessionID { get; set; }
        public string SessionName { get; set; }
        public Nullable<short> mediaType { get; set; }
        public Nullable<int> NoteID { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<short> Status { get; set; }
        public int ScheduleID { get; set; }
        public Nullable<short> Updated { get; set; }
        public Nullable<System.DateTime> Start_Date { get; set; }
        public Nullable<System.DateTime> End_Date { get; set; }
        public Nullable<System.DateTime> LastExecuted { get; set; }
        public Nullable<short> RelatedFunction { get; set; }
    
        public virtual tCMSWeb_Audit_Note tCMSWeb_Audit_Note { get; set; }
        public virtual tCMSWeb_Audit_Schedule tCMSWeb_Audit_Schedule { get; set; }
        public virtual ICollection<tDVRAddressBook> tDVRAddressBook { get; set; }
        public virtual ICollection<tCMSWeb_Audit_Media> tCMSWeb_Audit_Media { get; set; }
        public virtual ICollection<tDVRChannels> tDVRChannels { get; set; }
        public virtual ICollection<tCMSWebRecipients> tCMSWebRecipients { get; set; }
        public virtual tCMSWeb_UserList tCMSWeb_UserList { get; set; }
    }
}