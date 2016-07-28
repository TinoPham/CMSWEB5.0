using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Net.Http;
using CMSWebApi.Utils;

namespace CMSWebApi.DataModels.ModelBinderProvider
{
	public class RptChartParam : PeriodParam
	{
		public enum DataType :int
		{
			AlertCount,
			DVRCount,
			DVRMostAlert,
			Traffic,
			Conversion,
			ConversionMap,
		}
		public DataType rptDataType{ get ;set;}
		public List<int>Values{ get ;set;}
		//public PeriodType Period { get; set; }
		//public Int16 PeriodValue { get; set; }

	}

	public class AlertChartModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
	{
		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new AlertChartModelBinder();
		}
	}


	public class AlertChartModelBinder : CompareModelBinderBase<RptChartParam>
	{
		protected const string value = "value";
		protected const string v = "v";
		protected const string cmp = "cmp";
		protected const string c = "c";
		public AlertChartModelBinder(): base()
		{
			base.AddKey(value);
			base.AddKey(v);

		}
		public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(RptChartParam) || !base.BindModel(actionContext, bindingContext))
			{
				return false;
			}

			RptChartParam model = bindingContext.Model as RptChartParam;
			IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();

			KeyValuePair<string, string> querypair = GetQueryPair(queries, value, v);
			if(!string.IsNullOrWhiteSpace(querypair.Key) && !string.IsNullOrWhiteSpace(querypair.Value))
			{
				string val = Uri.UnescapeDataString( querypair.Value);
				model.Values = base.ParserIntArray(val);
			}
			querypair = GetQueryPair(queries, cmp, c);
			if (!string.IsNullOrEmpty(querypair.Value))
				model.rptDataType = Commons.Utils.GetEnum<RptChartParam.DataType>(querypair.Value);

			bindingContext.Model = model;
			return true;
		}
		/*
		private string UpdatePeriodmodel(IEnumerable<KeyValuePair<string, string>> queries, AlertCompareParam model)
		{
			int index = Consts.DateDefines.ValidDefineIndex(true, queries.Select(item => item.Key).ToArray());
			if (index < 0)
			{
				model.Period = PeriodType.Invalid;
				model.PeriodValue = Default_Period_Value;
				return null;
			}

			KeyValuePair<string, string> key = queries.ElementAt(index);
			model.Period = CMSWebApi.Utils.Utilities.Period(key.Key);
			Int16 per = Default_Period_Value;
			if (Int16.TryParse(key.Value, out per))
				model.PeriodValue = per;
			return key.Key;
		}
		*/
	}
}
