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
    
    public partial class tCMSWeb_UserPosition
    {
        public tCMSWeb_UserPosition()
        {
            this.tCMSWeb_UserList = new HashSet<tCMSWeb_UserList>();
        }
    
        public int PositionID { get; set; }
        public string PositionName { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public Nullable<int> Color { get; set; }
    
        public virtual tCMSWeb_UserList tCMSWeb_UserList_Owner { get; set; }
        public virtual ICollection<tCMSWeb_UserList> tCMSWeb_UserList { get; set; }
    }
}
