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
    
    public partial class tbl_Exception_Reports
    {
        public tbl_Exception_Reports()
        {
            this.tbl_Exception_ReportAssignment = new HashSet<tbl_Exception_ReportAssignment>();
            this.tbl_Exception_ReportColumns = new HashSet<tbl_Exception_ReportColumns>();
            this.tbl_Exception_ReportCriteria = new HashSet<tbl_Exception_ReportCriteria>();
        }
    
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> FolderID { get; set; }
        public string ReportDesc { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> PromoteToDashboard { get; set; }
    
        public virtual ICollection<tbl_Exception_ReportAssignment> tbl_Exception_ReportAssignment { get; set; }
        public virtual ICollection<tbl_Exception_ReportColumns> tbl_Exception_ReportColumns { get; set; }
        public virtual ICollection<tbl_Exception_ReportCriteria> tbl_Exception_ReportCriteria { get; set; }
        public virtual tbl_Exception_ReportFolders tbl_Exception_ReportFolders { get; set; }
        public virtual tCMSWeb_UserList tCMSWeb_UserList { get; set; }
    }
}