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
    
    public partial class tCMSWeb_Levels
    {
        public tCMSWeb_Levels()
        {
            this.tCMSWeb_Function_Level = new HashSet<tCMSWeb_Function_Level>();
        }
    
        public int LevelID { get; set; }
        public string LevelName { get; set; }
    
        public virtual ICollection<tCMSWeb_Function_Level> tCMSWeb_Function_Level { get; set; }
    }
}
