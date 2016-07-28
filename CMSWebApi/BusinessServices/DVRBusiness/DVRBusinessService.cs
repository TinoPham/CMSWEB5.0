using CMSWebApi.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using CMSWebApi.DataServices;
using CMSWebApi.DataModels;
using Extensions.Linq;

namespace CMSWebApi.BusinessServices.DVRBusiness
{
    public class DVRBusinessService : BusinessBase<IUsersService>
    {
        public async Task<IEnumerable<DVRInfoModel>> GetDVRs(UserContext user, int kDVR = 0)
        {
            List<int> TotalDVRs = new List<int>();
            if (kDVR == 0)
            {
                IEnumerable<UserSiteDvrChannel> sites = await DataService.GetDvrbyUserAsync(user.ID, item => new UserSiteDvrChannel
                {
                    KDVR = item.KDVR
                });

                TotalDVRs = sites.Where(it => it.KDVR.HasValue).Select(it => it.KDVR.Value).Distinct().ToList();
            }
            else
            {
                TotalDVRs.Add(kDVR);
            }

            SiteAlerts.SiteAlertsBusiness svr = new SiteAlerts.SiteAlertsBusiness { DVRService = new DVRService(base.ServiceBase) };

            IEnumerable<tDVRAddressBook> Total_DVRAddressBooks = svr.GetDVRs<tDVRAddressBook>(TotalDVRs, it => it);

            IEnumerable<DVRInfoModel> result = Total_DVRAddressBooks.Select(s => new DVRInfoModel
            {
                KDVR = s.KDVR,
                ServerID = s.ServerID,
                ServerIP = s.ServerIP,
                Port = s.tDVRNetwork != null ? s.tDVRNetwork.ControlPort : 0
            });

            return result;
        }

    }
}
