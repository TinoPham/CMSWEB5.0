using CMSWebApi.DataModels.ExportModel;
using CMSWebApi.DataServices.ExportDataService.Excel;
using CMSWebApi.DataServices.ExportDataService.PDF;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices
{
	public class ExportService:ServiceBase, IExportService
	{
		public void BAMDashboardToExcel(string filePath, byte[] logoImage, byte[] logoI3, ExportModel model)
		{
			BAMDashboardExcel.ExportToExcel(filePath, logoImage, logoI3, model);
		}
		public void ExportToExcel(string filePath, byte[] logoImage, byte[] logoI3, ExportModel model)
		{
			BAMDashboardExcel.ExportToExcel(filePath, logoImage, logoI3, model);
		}
		public void BAMDashboardToPDF(string filePath, byte[] logoImage, ExportModel model)
		{
			BAMDashboardPDF.ExportToPDF(filePath, logoImage, model);
		}
		public void BAMDashboardToCSV(string filePath, byte[] logoImage, ExportModel model)
		{

		}
	}
}
