using CMSWebApi.APIFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using LicenseInfo.Models;


namespace CMSWebApi.Controllers
{
    [RoutePrefix("LicenseInfo")]
    [WebApiAuthenication]
    public class LicenseInfoController : ApiController
    {
        [HttpGet]
        public LicenseModel Get()
        {
            return AppSettings.AppSettings.Instance.Licenseinfo; //new string[] { "value1", "value2" };
        }

    }
}
