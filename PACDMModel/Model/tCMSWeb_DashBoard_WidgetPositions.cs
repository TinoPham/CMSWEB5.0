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
    
    public partial class tCMSWeb_DashBoard_WidgetPositions
    {
        public tCMSWeb_DashBoard_WidgetPositions()
        {
            this.tCMSWeb_DashBoard_GroupUser_Element = new HashSet<tCMSWeb_DashBoard_GroupUser_Element>();
            this.tCMSWeb_DashBoard_User_Element = new HashSet<tCMSWeb_DashBoard_User_Element>();
            this.tCMSWeb_DashBoardLayouts = new HashSet<tCMSWeb_DashBoardLayouts>();
            this.tCMSWeb_DashBoard_UserLevel_Element = new HashSet<tCMSWeb_DashBoard_UserLevel_Element>();
        }
    
        public int WidgetGroupPositionID { get; set; }
        public Nullable<byte> StartRow { get; set; }
        public Nullable<byte> StartCol { get; set; }
        public Nullable<byte> NumRowsExpanded { get; set; }
        public Nullable<byte> NumColsExpanded { get; set; }
        public byte WidgetGroupSizeID { get; set; }
    
        public virtual ICollection<tCMSWeb_DashBoard_GroupUser_Element> tCMSWeb_DashBoard_GroupUser_Element { get; set; }
        public virtual ICollection<tCMSWeb_DashBoard_User_Element> tCMSWeb_DashBoard_User_Element { get; set; }
        public virtual tCMSWeb_DashBoard_WidgetGroupSize tCMSWeb_DashBoard_WidgetGroupSize { get; set; }
        public virtual ICollection<tCMSWeb_DashBoardLayouts> tCMSWeb_DashBoardLayouts { get; set; }
        public virtual ICollection<tCMSWeb_DashBoard_UserLevel_Element> tCMSWeb_DashBoard_UserLevel_Element { get; set; }
    }
}
