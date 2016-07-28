using CMSWebApi.DataModels.ExportModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IExportService
	{
		void BAMDashboardToExcel(string filePath, byte[] logoImage, byte[] logoI3, ExportModel model);
		void ExportToExcel(string filePath, byte[] logoImage, byte[] logoI3, ExportModel model);
		void BAMDashboardToPDF(string filePath, byte[] logoImage, ExportModel model);
		void BAMDashboardToCSV(string filePath, byte[] logoImage, ExportModel model);
	}
}
