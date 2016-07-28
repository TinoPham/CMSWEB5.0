using CMSWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Net.Http;

namespace CMSWebApi.DataModels.ModelBinderProvider
{
	public class BAMRptParam
	{
		public BAMReportType rptDataType { get; set; }
		public int userID { get; set; }
		public DateTime sDate { get; set; }
		public DateTime eDate { get; set; }
		public string siteKeys { get; set; }
		public DateTime sFYDate { get; set; }
		public DateTime eFYDate { get; set; }
	}

	public class RegionDate
	{
		public int RegionKey { get; set; }
		public int RegionName { get; set; }
		public DateTime Date { get; set; }
	}

	public class BAMRptModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
	{
		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new BAMRptModelBinder();
		}
	}

	public class BAMRptModelBinder : BAMRptCompareModelBinderBase<BAMRptParam>
	{
		protected const string DATE_FORMAT = "yyyyMMddHHmmss";
		protected const string START_DATE_KEY = "sDate";
		protected const string END_DATE_KEY = "eDate";
		protected const string REPORT_TYPE_KEY = "rptDataType";
		protected const string SITE_KEY = "siteKeys";

		public BAMRptModelBinder()
		{

		}

		public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(BAMRptParam))
			{
				return false;
			}

			BAMRptParam model = bindingContext.Model as BAMRptParam;
			if (model == null) { model = new BAMRptParam(); }
			IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();
			foreach (KeyValuePair<string, string> query in queries)
			{
				switch (query.Key)
				{ 
					case START_DATE_KEY:
						model.sDate = Utilities.DateTimeParseExact(query.Value, query.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, query.Value.Length) : DATE_FORMAT, DateTime.MinValue);
						break;
					case END_DATE_KEY:
						model.eDate = Utilities.DateTimeParseExact(query.Value, query.Value.Length != DATE_FORMAT.Length ? DATE_FORMAT.Substring(0, query.Value.Length) : DATE_FORMAT, DateTime.MinValue);
						break;
					case REPORT_TYPE_KEY:
						model.rptDataType = Commons.Utils.GetEnum<BAMReportType>(query.Value);
						break;
					case SITE_KEY:
						if (!string.IsNullOrEmpty(query.Value))
						{
							model.siteKeys = query.Value;
						}
						break;
				}
			}

			bindingContext.Model = model;
			return true;
		}
	}

	public abstract class BAMRptCompareModelBinderBase<T> : IModelBinder where T : BAMRptParam
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
