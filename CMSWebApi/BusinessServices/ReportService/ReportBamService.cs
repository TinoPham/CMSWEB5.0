using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.GoalType;
using CMSWebApi.BusinessServices.ReportBusiness.IOPC;
using CMSWebApi.BusinessServices.ReportBusiness.POS;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using  Extensions;
using Extensions.Linq;
using CMSWebApi.BusinessServices.ReportBusiness;

namespace CMSWebApi.BusinessServices.ReportService
{
	public class ReportBamService : BusinessBase<IBamMetricService>
	{
		private IReportService _reportService = null;

		public CustomReportModel GetReportData(UserContext userContext, int reportId, NameValueCollection requestParms = null)
		{

			var svc = (IBamMetricService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IBamMetricService));
			var dataSvc = (IResposity)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IResposity));
			//var reportFormat = svc.GetMetricReportUsers().FirstOrDefault(t => t.UserID == userContext.ID && t.ReportID == reportId); Default Users
			var reportFormat = svc.GetMetricReportUsers().FirstOrDefault(t => t.ReportID == reportId);


			if (reportFormat == null || reportFormat.tbl_BAM_Metric_ReportList == null)
			{
				return null;
			}

			_reportService = ReportFactory.GetService(reportFormat.tbl_BAM_Metric_ReportList.ServiceName, new object[] { userContext, dataSvc });

			var dataResult = _reportService.GetReportData(reportFormat, requestParms);
            dataResult.DataSetName = reportFormat.tbl_BAM_Metric_ReportList.ReportResourceName;

			return new CustomReportModel()
			{
				ReportId = reportFormat.ReportID,
				ReportLocation = reportFormat.tbl_BAM_Metric_ReportList.ReportLocation,
				ReportName = reportFormat.tbl_BAM_Metric_ReportList.ReportName,
				ReportResourceName = reportFormat.tbl_BAM_Metric_ReportList.ReportResourceName,
				Data = dataResult
			};
		}
	}
}
