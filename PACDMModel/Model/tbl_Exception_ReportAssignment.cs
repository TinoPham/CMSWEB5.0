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
    
    public partial class tbl_Exception_ReportAssignment
    {
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public int ReportID { get; set; }
        public Nullable<System.DateTime> AssignedDate { get; set; }
        public Nullable<byte> Sharing { get; set; }
    
        public virtual tCMSWeb_UserList tCMSWeb_UserList { get; set; }
        public virtual tbl_Exception_Reports tbl_Exception_Reports { get; set; }
        public virtual tbl_Exception_SharingPermission tbl_Exception_SharingPermission { get; set; }
        public virtual tCMSWeb_UserList tCMSWeb_UserList1 { get; set; }
    }
}
