using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ApiConverter.Filters;
using CMSSVR.Infrastructure;
using ConverterSVR.IServices;
using ConvertMessage;

namespace CMSSVR.Controllers.Api.Converter
{
	public class SummaryController : ApiController
	{
		const string STR_User_Agent = "User-Agent";
		const string STR_attachment = "attachment";
		IConvertSummaryService _Iservice;

		public CMSSVR.Infrastructure.KDVRToken Token { get { return HttpContext.Current.User.Identity as KDVRToken; } }
		public SummaryController( IConvertSummaryService isvr)
		{
			_Iservice = isvr;
		}
		// GET api/<controller>
		public IEnumerable<string> Get()
		{
			return new string [] { "value1", "value2" };
		}

		[HttpPost]
		[DVRTokenFilter]
		public HttpResponseMessage Transaction([FromBody]ConvertMessage.PACDMObjects.POS.TransactionSummary summary)
		{
			if (Token == null)
				return Request.CreateResponse(HttpStatusCode.Unauthorized);

			MessageResult res_header = _Iservice.SummaryTransaction(summary, Token.SinpleDVRMessageInfo());
			return Request.CreateResponse(HttpStatusCode.OK, res_header);
		}

	}
}