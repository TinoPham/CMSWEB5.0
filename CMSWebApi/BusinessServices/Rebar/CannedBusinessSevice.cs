using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.BusinessServices.Rebar
{
	public class CannedBusinessSevice : BusinessBase<ICannedService>
	{
		#region Properties

		public IUsersService IUser { get; set; }
		public ICompanyService comSvc { get; set; }
		public ISiteService ISiteSvc { get; set; }
		public IRebarDataService IRebar { get; set; }

		#endregion

		public async Task<List<Proc_Exception_GetReport_Result>> GetCannedReport(UserContext userLogin, CannedRptParam param)
		{
			ConvertParams(ref param, userLogin);
			var data = await DataService.GetCannedReports(param);
			return data;
		}

		private void ConvertParams(ref CannedRptParam param, UserContext userLogin)
		{
			CannedRptParam ret = param;
			ret.DateFrom = param.DateFrom;
			ret.DateTo = param.DateTo;
			if (param.PACIDs.Count == 0)
			{
				IEnumerable<UserSiteDvrChannel> userSiteDvrChannel = UserSites(IUser, userLogin).Where(w => ret.SiteKeys.Contains(w.siteKey.Value)).Distinct();
				ret.PACIDs = userSiteDvrChannel.Select(s => s.PACID.Value).Distinct().ToList();
			}
			else
			{
				ret.PACIDs = param.PACIDs;
			}

			ret.MaxRows = param.MaxRows == 0 ? 100000 : param.MaxRows;

			param = ret;
		}
	}
}
