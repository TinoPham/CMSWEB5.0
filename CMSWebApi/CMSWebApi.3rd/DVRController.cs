using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.Account;
using CMSWebApi.BusinessServices.DVRBusiness;
using CMSWebApi.ServiceInterfaces;
using SVRDatabase;
using System.IO;
using CMSWebApi.DataModels;
using System.Net;
using CMSWebApi.APIFilters._3rdToken;

namespace CMSWebApi._3rd
{
	[_3rdAuthenticationAttribute]
    public class DVRController : ApiControllerBase<IUsersService, DVRBusinessService>
	{
        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            IEnumerable<DVRInfoModel> ret = await BusinessService.GetDVRs(usercontext, id).ConfigureAwait(false);
			if(id > 0)
				return new ApiActionResult<IEnumerable<DVRInfoModel>>(System.Net.HttpStatusCode.OK, ret, Request);
			else
				return new ApiActionResult<DVRInfoModel>(System.Net.HttpStatusCode.OK, ret == null? null : ret.FirstOrDefault(), Request);

        }

	}
}
