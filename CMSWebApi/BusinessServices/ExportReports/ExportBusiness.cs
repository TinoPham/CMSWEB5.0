using System;
using CMSWebApi.DataModels.ExportModel;
using System.IO;
using CMSWebApi.Utils;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ExportReports
{
	public class ExportBusiness: BusinessBase<IExportService>
	{
		#region Properties
		private readonly string DASHBOARD_STRING = "BAMDashboard";
		private readonly string FOOTER_STRING = "Prepared under contract for the exclusive use of i3 International. Reproduction of this report is prohibited";
		private readonly string LOGO_I3_NAME = "i3_logo.jpg";
		public ICompanyService ICompanySvc { get; set; }

		#endregion

		public string BAMDashboardToPDF(ExportModel param)
		{
			tCMSWeb_Company tcmswebCompany = ICompanySvc.SelectCompanyInfo(param.ReportInfo.CompanyID);
			byte[] logoImage = tcmswebCompany.CompanyLogo;
			param.ReportInfo.CompanyName = tcmswebCompany.CompanyName;
			param.ReportInfo.Footer = FOOTER_STRING;
			string filename = string.Concat(DASHBOARD_STRING, ExportConst.PDF_EXTENSION);
			filename = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, filename);

			DataService.BAMDashboardToPDF(filename, logoImage, param);
			return Path.GetFileName(filename);
		}

		public string BAMDashboardToExcel(ExportModel param)
		{
			tCMSWeb_Company tcmswebCompany = ICompanySvc.SelectCompanyInfo(param.ReportInfo.CompanyID);
			byte[] logoImage = tcmswebCompany.CompanyLogo;
			param.ReportInfo.CompanyName = tcmswebCompany.CompanyName;
			param.ReportInfo.Footer = FOOTER_STRING;

			string filename = string.Concat(DASHBOARD_STRING, ExportConst.EXCEL_EXTENSION);
			filename = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, filename);
		
			string logoI3Path = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, LOGO_I3_NAME);
			byte[] logoI3 = FileManager.ReadFile(logoI3Path);

			DataService.BAMDashboardToExcel(filename, logoImage, logoI3, param);

			return Path.GetFileName(filename);
		}

		public string GetFilePath(string filename)
		{
			string filepath = string.Empty;
			filepath = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, filename);
			return filepath;
		}

		public string ExportToExcel(ExportModel param)
		{
			tCMSWeb_Company tcmswebCompany = ICompanySvc.SelectCompanyInfo(param.ReportInfo.CompanyID);
			byte[] logoImage = tcmswebCompany.CompanyLogo;
			param.ReportInfo.CompanyName = tcmswebCompany.CompanyName;
			param.ReportInfo.Footer = FOOTER_STRING;

			string filename = string.Concat(param.ReportInfo.TemplateName, ExportConst.EXCEL_EXTENSION);
			filename = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, filename);

			string logoI3Path = Path.Combine(AppSettings.AppSettings.Instance.ExportFolder, LOGO_I3_NAME);
			byte[] logoI3 = FileManager.ReadFile(logoI3Path);

			DataService.ExportToExcel(filename, logoImage, logoI3, param);

			return Path.GetFileName(filename);
		}
	}
}
