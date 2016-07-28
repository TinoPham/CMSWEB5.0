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
	public class BAMHeatMapParam
	{
		public BAMReportType rptDataType { get; set; }
		public int userID { get; set; }
		public DateTime sDate { get; set; }
		public DateTime eDate { get; set; }
		public int kDVR { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
	}


	public class BAMHeatMapModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
	{
		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new BAMHeatMapModelBinder();
		}
	}

	public class BAMHeatMapModelBinder : BAMHeatMapCompareModelBinderBase<BAMHeatMapParam>
	{
		protected const string DATE_FORMAT = "yyyyMMddHHmmss";
		protected const string START_DATE_KEY = "sDate";
		protected const string END_DATE_KEY = "eDate";
		protected const string REPORT_TYPE_KEY = "rptDataType";
		protected const string KDVR = "KDVR";
        protected const string PageNo = "PageNo";
        protected const string PageSize = "PageSize";

		public BAMHeatMapModelBinder()
		{

		}

		public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(BAMHeatMapParam))
			{
				return false;
			}

            BAMHeatMapParam model = bindingContext.Model as BAMHeatMapParam;
            if (model == null) { model = new BAMHeatMapParam(); }
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
                    case KDVR:
						if (!string.IsNullOrEmpty(query.Value))
						{
							model.kDVR = int.Parse(query.Value);
						}
						break;
                    case PageNo:
                        if (!string.IsNullOrEmpty(query.Value))
                        {
                            model.PageNo = int.Parse(query.Value);
                        }
                        break;
                    case PageSize:
                        if (!string.IsNullOrEmpty(query.Value))
                        {
                            model.PageSize = int.Parse(query.Value);
                        }
                        break;
				}
			}

			bindingContext.Model = model;
			return true;
		}
	}

	public abstract class BAMHeatMapCompareModelBinderBase<T> : IModelBinder where T : BAMHeatMapParam
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
