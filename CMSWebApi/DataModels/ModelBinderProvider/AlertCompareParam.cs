using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web;
using System.Web.Http.ModelBinding;
using System.Net.Http;
using CMSWebApi.Utils;
namespace CMSWebApi.DataModels.ModelBinderProvider
{

	public class AlertCompareParam : PeriodParam
	{
		public enum CompareType : int
		{
			Invalid_compare = 0
				//"Number of new Alerts"
			,
			AlertType
				,
			AlertSeverity
				//Total conversion rate
			,
			RecordLess
			   ,
			DVROffline,
			DVROnline
			,Conversion
				//Total traffic
				,
			Traffic
				//Total sales
				,
			Sales
				//Total number of transactions
				,
			Transaction
				//Total number of POS exception,
				,
			Exception
		}

		public CompareType Cmp { get; set; }
		public List<int> Values{ get ;set;}
		int _interval = 24;

		public int interval{ get{ return _interval;} set{ _interval = value;}}
	}

	public class AlertCompareModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
	{

		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new AlertCompareModelBinder();
		}
	}

	public class AlertCompareModelBinder : CompareModelBinderBase<AlertCompareParam>
	{
		protected const string value = "value";
		protected const string v = "v";
		protected const string cmp = "cmp";
		protected const string c = "c";
		protected const string interval = "int";

		public  AlertCompareModelBinder(): base()
		{
			base.AddKey( value);
			base.AddKey(v);
			base.AddKey(cmp);
			base.AddKey(interval);
		}
	
		public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if( !base.BindModel(actionContext, bindingContext))
				return false;

			AlertCompareParam model = bindingContext.Model as AlertCompareParam;
			
			
			IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();

			KeyValuePair<string, string> querypair = GetQueryPair(queries, value, v);

			if (!string.IsNullOrWhiteSpace(querypair.Key) && !string.IsNullOrWhiteSpace(querypair.Value))
			{
				string val = Uri.UnescapeDataString(querypair.Value);
				model.Values = base.ParserIntArray(val);
			}

			//int val = 0;
			//if (Int32.TryParse(querypair.Value, out val))
			//	model.value = val;

			querypair = GetQueryPair(queries, cmp, c);
			if (!string.IsNullOrEmpty(querypair.Value))
				model.Cmp = Commons.Utils.GetEnum<AlertCompareParam.CompareType>(querypair.Value);
			querypair = GetQueryPair(queries, interval);

			if(!string.IsNullOrEmpty(querypair.Value))
			{
				int intt = 0;
				if(Int32.TryParse(querypair.Value, out intt))
					model.interval = intt;
			}


			bindingContext.Model = model;

			return true;
		}

	}

}
