using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.APIFilters;
using System.IO;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.BusinessServices;
using CMSWebApi.BusinessServices.Caches;
using CMSWebApi.BusinessServices.Account;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication(true) ]
	public class CachesController : ApiControllerBase<IAccountService,Caches> 
	{
		[HttpGet]
		public HttpResponseMessage Caches(string name, string format, bool download = false, bool refresh = false)
		{
			HttpResponseMessage res = null;
			string status = string.Empty;
			using( MemoryStream mem = BusinessService.GetCache(name, format, out status))
			{
				if( string.IsNullOrEmpty(status) || string.Compare(status, CMSWebApi.Utils.Consts.CacheStatus_Defines.CACHE_READY, true) == 0)
				{
					if (!download)
						res = ResponseStream(mem == null ? null : mem.ToArray(), format);
					else
						res = ResponseStream(mem == null ? null : mem.ToArray(), format, true, name + "." + format);
				}
				else
				{
					res = Request.CreateResponse( System.Net.HttpStatusCode.OK);
					res.Content = new StringContent(status, Encoding.UTF8);
					res.Content.Headers.ContentType = base.ContentType( Utils.MineTypesMapping.MineTypes.html);
				}
			}
			return res;
		}
		[HttpGet]
		public HttpResponseMessage Refresh( string name)
		{
			return null;
		}
		
	}
}
