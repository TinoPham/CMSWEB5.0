using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ConvertMessage.PACDMObjects.LABOR
{
    [DataContract(Namespace = Consts.Empty, Name = Consts.str_Labor)]
    public partial class Labor
    {
        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public int StoreID { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public int EmployeeID { get; set; }

        [DataMember]
        public int InPunch { get; set; }

        [DataMember]
        public int OutPunch { get; set; }

        [DataMember]
        public int T_PACID { get; set; }
    }
}
