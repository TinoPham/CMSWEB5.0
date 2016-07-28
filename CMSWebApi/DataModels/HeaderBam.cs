using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
    public class HeaderBamModel
    {
       public int Caculate { get; set; }
       public int POSdata { get; set; }
       public int Trafficdata { get; set; }
       public int Normalized { get; set; }
    }

    public class HeaderBamParam
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
