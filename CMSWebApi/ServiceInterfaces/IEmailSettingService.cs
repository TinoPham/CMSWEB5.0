using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
    public interface IEmailSettingService
    {
        //IQueryable<tCMSWebReport> GetEmailSetting();
       IEnumerable<Func_CMSWebReport_Schedule_Result> CurrentReportSchedule(DateTime date);

        
    }
}
