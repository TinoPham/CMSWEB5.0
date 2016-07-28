using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using CMSWebApi.Utils;
using System.Net.Http;

namespace CMSWebApi.DataModels.ModelBinderProvider
{
    
    public class DashboardBamModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
    {
        public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
        {
            return new DashboardBamModelBinder();
        }
    }

    public class DashboardBamModelBinder : DashboardBamCompareModelBinderBase<MetricParam>
    {
        protected const string DATE_FORMAT = "yyyyMMdd";
		const string sDate = "sDate";
		const string eDate = "eDate";
        const string ArrsiteKeys = "SitesKey";
		const string ReportId = "ReportId";
		const string ReportType = "ReportType";
        public DashboardBamModelBinder()
        {

        }

        public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(MetricParam))
            {
                return false;
            }

            MetricParam model = bindingContext.Model as MetricParam;
            if (model == null) { model = new MetricParam(); }
            IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();
			KeyValuePair<string, string> date;
			date = queries.FirstOrDefault( it => string.Compare(it.Key, sDate, true) == 0);
			if(!string.IsNullOrEmpty(date.Key))
				model.StartDate = model.WeekStartDate = Utilities.DateTimeParseExact(date.Value, date.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, date.Value.Length) : DATE_FORMAT, DateTime.MinValue);

			date = queries.FirstOrDefault(it => string.Compare(it.Key, eDate, true) == 0);
			if (!string.IsNullOrEmpty(date.Key))
				model.EndDate = model.WeekEndDate = Utilities.DateTimeParseExact(date.Value, date.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, date.Value.Length) : DATE_FORMAT, DateTime.MinValue);

            KeyValuePair<string, string> siteKeys;
            siteKeys = queries.FirstOrDefault(it => string.Compare(it.Key, ArrsiteKeys, true) == 0);
            if (!string.IsNullOrEmpty(siteKeys.Key))
			{
                model.SitesKey = siteKeys.Value.Split(',').Select(n => Convert.ToInt32(n)).ToList();
			}

			KeyValuePair<string, string> reportId;
			reportId = queries.FirstOrDefault(it => string.Compare(it.Key, ReportId, true) == 0);
			if (!string.IsNullOrEmpty(reportId.Key))
			{
				model.ReportId = reportId.Value != "" ?  Convert.ToInt32(reportId.Value): 0;
			}

			KeyValuePair<string, string> reportType;
			reportType = queries.FirstOrDefault(it => string.Compare(it.Key, ReportType, true) == 0);
			if (!string.IsNullOrEmpty(reportType.Key))
			{
				model.ReportType = reportType.Value != "" ? Convert.ToInt32(reportType.Value) : 0;
			}

			//foreach (KeyValuePair<string, string> query in queries)
			//{
			//	switch (query.Key)
			//	{
			//		case "sDate":
			//			model.StartDate = Utilities.DateTimeParseExact(query.Value, query.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, query.Value.Length) : DATE_FORMAT, DateTime.MinValue);
			//			break;
			//		case "eDate":
			//			model.EndDate = Utilities.DateTimeParseExact(query.Value, query.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, query.Value.Length) : DATE_FORMAT, DateTime.MinValue);
			//			break;
			//	}
			//}

            bindingContext.Model = model;
            return true;
        }
    }

    public abstract class DashboardBamCompareModelBinderBase<T> : IModelBinder where T : MetricParam
    {
        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(T))
            {
                return false;
            }
            T model = Commons.ObjectUtils.InitObject<T>();
            bindingContext.Model = model;
            return true;
        }
    }
}
