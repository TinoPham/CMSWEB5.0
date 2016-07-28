using CMSWebApi.BusinessServices.ExportReports;
using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.ServiceInterfaces;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace CMSWebApi.Controllers
{
	public class ExportController: ApiControllerBase<IExportService, ExportBusiness>
	{
		[HttpPost]
		public HttpResponseMessage BAMDashboardToExcel(ExportModel param)
		{
			return ResponseData<string>(BusinessService.BAMDashboardToExcel(param));
		}

		[HttpPost]
		public HttpResponseMessage BAMDashboardToPDF(ExportModel param)
		{
			return ResponseData<string>(BusinessService.BAMDashboardToPDF(param));
		}

		[HttpGet]
		public HttpResponseMessage DownloadExport(string filename)
		{
			string filepath = BusinessService.GetFilePath(filename);
			return ResponseExportFile(filepath);
		}

		[HttpPost]
		public HttpResponseMessage ExportExcel(ExportModel param)
		{
			return ResponseData<string>(BusinessService.ExportToExcel(param));
		}
	}
}
