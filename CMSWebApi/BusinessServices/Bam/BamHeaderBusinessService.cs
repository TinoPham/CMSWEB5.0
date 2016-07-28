using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.BusinessServices.GoalType;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using  Extensions;

namespace CMSWebApi.BusinessServices.Bam
{
    public class BamHeaderBusinessService : BusinessBase<IBamHeaderService>
	{

        public async Task<HeaderBamModel> GetHeaderBam(UserContext user, MetricParam param)
		{
            IEnumerable<Proc_BAM_Get_Header_Stores_Count_Result> DataHeader = await DataService.GetCountDataHeader(user.ID, StartTimeOfDate(param.StartDate));
			HeaderBamModel result = new HeaderBamModel()
                {
                    Caculate = 0,
                    Normalized = 0,
                    POSdata = 0,
                    Trafficdata = 0
                };

			if (DataHeader.Any())
			{
				result = new HeaderBamModel()
				{
                    Caculate = (int)DataHeader.FirstOrDefault().AllSites,
					Normalized = (int)DataHeader.FirstOrDefault().Normalized,
					POSdata = (int)DataHeader.FirstOrDefault().MissingPOS,
					Trafficdata = (int)DataHeader.FirstOrDefault().MissingIOPC
				};
			}
			
			return result;
		}

	}
}
