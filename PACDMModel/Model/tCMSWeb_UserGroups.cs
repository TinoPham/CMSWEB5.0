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
    
    public partial class tCMSWeb_UserGroups
    {
        public tCMSWeb_UserGroups()
        {
            this.tCMSWeb_DashBoard_GroupUser_Element = new HashSet<tCMSWeb_DashBoard_GroupUser_Element>();
            this.tCMSWeb_Function_Level = new HashSet<tCMSWeb_Function_Level>();
            this.tCMSWeb_UserList = new HashSet<tCMSWeb_UserList>();
        }
    
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<byte> GroupLevel { get; set; }
    
        public virtual ICollection<tCMSWeb_DashBoard_GroupUser_Element> tCMSWeb_DashBoard_GroupUser_Element { get; set; }
        public virtual ICollection<tCMSWeb_Function_Level> tCMSWeb_Function_Level { get; set; }
        public virtual tCMSWeb_UserList tCMSWeb_UserList_Owner { get; set; }
        public virtual ICollection<tCMSWeb_UserList> tCMSWeb_UserList { get; set; }
    }
}