using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using CMSWebApi.BusinessServices.ReportService;
using CMSWebApi.DataModels;
using Microsoft.Reporting.WebForms;

namespace CMSSVR.Report
{
	public partial class ReportViewer : System.Web.UI.Page
	{
		public bool IsCrystal = false;
		public bool FailLoadReport = false;

		protected void Page_Load(object sender, EventArgs e)
		{
			var crystalExt = ".rpt";
			var rptExt = ".rdlc";
			var paramTable = "ParamReport";
			var user = Context.User;

			UserContext userContext = null;
			if (user.Identity.IsAuthenticated)
			{
				userContext = User.Identity as UserContext;
			}
			else
			{
				return;
			}

			string requestId = Request.QueryString["reportId"];

			//try
			//{
				var languageId = Request.QueryString["languageId"] ?? string.Empty;
				if (!string.IsNullOrEmpty(languageId))
				{
					Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageId, false);
				}


				ReportBamService reportSvc = new ReportBamService();

				if (userContext != null)
				{
					var dataResult = reportSvc.GetReportData(userContext, Convert.ToInt32(requestId), Request.QueryString);


					if (!IsPostBack)
					{

						ReportViewerManager.ProcessingMode = ProcessingMode.Local;
						ReportViewerManager.LocalReport.ReportPath =
							Server.MapPath("~/" + AppSettings.AppSettings.Instance.ReportPath + "/" + dataResult.ReportLocation + rptExt);

						ReportViewerManager.ShowParameterPrompts = false;
						ReportViewerManager.ShowRefreshButton = false;
						ReportViewerManager.ShowWaitControlCancelLink = false;
						ReportViewerManager.ShowBackButton = false;
						ReportViewerManager.ShowCredentialPrompts = false;
						ReportViewerManager.LocalReport.EnableExternalImages = true;


						List<ReportParameter> parameters = new List<ReportParameter>();
						if (dataResult.Data.Tables.Contains(paramTable) && dataResult.Data.Tables[paramTable].Rows.Count > 0)
						{
							for (int i = 0; i < dataResult.Data.Tables[paramTable].Rows.Count; i++)
							{
								parameters.Add(new ReportParameter(dataResult.Data.Tables[paramTable].Rows[i]["NameField"].ToString(),
									dataResult.Data.Tables[paramTable].Rows[i]["ValueField"].ToString()));

							}
							ReportViewerManager.LocalReport.SetParameters(parameters);

							dataResult.Data.Tables.Remove(dataResult.Data.Tables[paramTable]);
						}

						//ReportViewerManager.LocalReport.DataSources.Add(new ReportDataSource(dataResult.ReportResourceName,
						//    dataResult.Data.Tables[dataResult.ReportResourceName]));

						foreach (DataTable table in dataResult.Data.Tables)
						{
							ReportViewerManager.LocalReport.DataSources.Add(new ReportDataSource(table.TableName, table));
						}

                        ReportViewerManager.ZoomMode = ZoomMode.PageWidth;
                        ReportViewerManager.ShowZoomControl = false;

						ReportViewerManager.KeepSessionAlive = false;
						ReportViewerManager.AsyncRendering = false;
					}

				}
			//}
			//catch (Exception ex)
			//{
			//	FailLoadReport = true;
			//}
		}
	}
}