using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.Account;
using CMSWebApi.BusinessServices.SiteAlerts;
using CMSWebApi.BusinessServices.Emailsetting;
using CMSWebApi.ServiceInterfaces;
using SVRDatabase;
using System.IO;
using CMSWebApi.DataModels;
using System.Net;
using CMSWebApi.APIFilters._3rdToken;

namespace CMSWebApi._3rd
{
	
	public class EmailingAlertController : ApiControllerBase<IAlertService, EmailsettingBusinessService>
	{
		[_3rdAuthenticationAttribute]
		public IHttpActionResult Get(int? id)
		{
			if(id == 0)
				return NotFound();

			IEnumerable<AlertReportActive> reports = BusinessService.AlertReportActive();
			if( id.HasValue)
				return new ApiActionResult<AlertReportActive>(HttpStatusCode.OK,reports.FirstOrDefault( it => it.ReportKey == id.Value), Request);
			else
				return new ApiActionResult<IEnumerable<AlertReportActive>>(HttpStatusCode.OK, reports, Request);

		}

		[HttpGet]
		public IHttpActionResult Image(string id)
		{
			if( string.IsNullOrEmpty(id))
				return BadRequest("Invalid id"); //base.ResponseData<string>( null, System.Net.HttpStatusCode.BadRequest);
			string data = null;
			 try{ data = Commons.Utils.Base64toString(id);} catch(Exception){}
			if (string.IsNullOrEmpty(data))
				return BadRequest("Invalid id");
			string[]infos = data.Split(':');
			if(infos == null || infos.Length != 3)
				return BadRequest("Image Id doesn't match.");
			int kdvr, channel;
			long img_time;
			Int32.TryParse(infos[0], out kdvr);
			Int32.TryParse(infos [1], out  channel);
			long.TryParse(infos [2], out  img_time);
			SiteAlertsBusiness svr = base.Resolve(typeof(SiteAlertsBusiness)) as SiteAlertsBusiness;
			string img = svr.GetImagesAlert(kdvr, channel, new DateTime(img_time)).FirstOrDefault();
			if(string.IsNullOrEmpty(img) || File.Exists(img) == false)
				return BadRequest("The Image was removed.");

			return new FileActionResult(Request, img, string.Format("{0}_{1}_{2}{3}", kdvr.ToString(), channel.ToString(), img_time.ToString(), Path.GetExtension(img)), false);
		}


		[HttpPut]
		[_3rdAuthenticationAttribute]
		public async Task<IHttpActionResult> Put(int id, AlertReportActive model)
		{
			if (id == 0 || model == null || model.ReportKey != id)
				return BadRequest();
			if (model.NextRunDate > DateTime.Now)
				return new ApiActionResult<string>(HttpStatusCode.NotAcceptable, "NextRunDate is greater than current system date.", Request);

			string url_image_action = Request.RequestUri.AbsoluteUri;
			url_image_action = url_image_action.Remove(url_image_action.Length - Request.RequestUri.Segments.Last().Length);
			url_image_action += "{0}/Image";
			SVRManager svr = base.Configuration.DependencyResolver.GetService(typeof(SVRDatabase.SVRManager)) as SVRDatabase.SVRManager;
			AlertReportResult ret = await BusinessService.EmailingAlert(svr.SVRConfig.KeepAliveInterval, model, url_image_action).ConfigureAwait(false);
			return new ApiActionResult<AlertReportResult>(System.Net.HttpStatusCode.OK, ret, Request);
		}
	}
}
