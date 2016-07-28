using ApiConverter.Filters;
using LicenseInfo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using ConverterSVR.IServices;
using System.Net.Http;
using System.Net;

namespace CMSSVR.Controllers.Api.Converter
{
    public class LicenseController : ApiController
    {
        ILicenseService IService;
        public LicenseController(ILicenseService isrv) 
        {
            IService = isrv;
        }
        [HttpGet]
        public  LicenseModel Get()
        {
          return AppSettings.AppSettings.Instance.Licenseinfo; 
        }
        [HttpPost]
        public HttpResponseMessage Post([FromBody] LicenseModel model)
        {
            try
            {
                IService.GenerateLicense(model);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized,ex.InnerException.StackTrace);
            }
        }
    }
}