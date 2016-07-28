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
	public class PeriodParam
	{
		public DateTime Date { get; set; }
		public PeriodType Date_Period { get; set; }
		public PeriodType Period { get; set; }
		public Int16 PeriodValue { get; set; }
		public IEnumerable<KeyValuePair<string, string>> Extend { get; set; }
	}

	public abstract class CompareModelBinderBase<T> : IModelBinder where T : PeriodParam
	{
		protected const byte Default_Period_Value = 24;
		protected const string date = "date";
		protected const string d = "d";
		protected const string Date_Format = "yyyyMMddHHmmss";
		protected readonly string[] ValidKeys = { date, d };
		protected List<string> Keys = new List<string>();
		protected const string Begin_Array = "[";
		protected const string End_Array = "]";

		public CompareModelBinderBase()
		{

			Keys.AddRange(ValidKeys);
		}

		protected void AddKey(string key)
		{
			if (string.IsNullOrEmpty(key))
				return;
			Keys.Add(key);
		}

		protected void RemoveKey(string key)
		{
			int index = Keys.FindIndex(
										delegate(string item)
										{
											return string.Compare(item, key, true) == 0;
										}
									);
			if (index >= 0)
				Keys.RemoveAt(index);

		}

		protected string GetQueryValue(IEnumerable<KeyValuePair<string, string>> queries, string key)
		{
			KeyValuePair<string, string> item = GetQueryPair(queries, key);
			return string.IsNullOrEmpty(item.Key) ? null : item.Value;
		}

		protected KeyValuePair<string, string> GetQueryPair(IEnumerable<KeyValuePair<string, string>> queries, string key)
		{
			KeyValuePair<string, string> item = queries.FirstOrDefault(q => string.Compare(q.Key, key, true) == 0);
			return item;
		}

		//protected bool isArray( string value)
		//{
		//	if( string.IsNullOrEmpty( value))
		//		return false;
		//	return  value.StartsWith(Begin_Array) && value.EndsWith(End_Array);

		//}

		protected List<int> ParserIntArray(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return new List<int>();

			//value = value.Remove(0, 1);
			//value = value.Remove(value.Length -1, 1);
			string[] arr = value.Split(new char[] { ',' });
			List<int> ret = new List<int>();
			int val = 0;
			foreach (string it in arr)
			{
				if (Int32.TryParse(it, out val))
					ret.Add(val);
			}
			return ret;
		}

		protected string GetQueryValue(IEnumerable<KeyValuePair<string, string>> queries, params string[] keys)
		{
			string value = string.Empty;
			foreach (string key in keys)
			{
				value = GetQueryValue(queries, key);
				if (!string.IsNullOrEmpty(value))
					break;
			}
			return value;
		}

		protected KeyValuePair<string, string> GetQueryPair(IEnumerable<KeyValuePair<string, string>> queries, params string[] keys)
		{
			KeyValuePair<string, string> item = new KeyValuePair<string, string>();
			foreach (string key in keys)
			{

				item = GetQueryPair(queries, key);
				if (!string.IsNullOrEmpty(item.Key))
					break;
			}
			return item;
		}

		public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(T))
			{
				return false;
			}
			T model = Commons.ObjectUtils.InitObject<T>();
			//CompareParam model = Tmodel; //new AlertCompareParam { Cmp = AlertCompareParam.CompareType.Invalid_compare };
			List<string> validkey = new List<string>();

			IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();
			string periodkey = UpdatePeriodmodel(queries, model);

			string queryvalue = GetQueryValue(queries, date, d);
			model.Date_Period = PeriodType.Invalid;
			if (!string.IsNullOrEmpty(queryvalue))
			{
				if (Consts.DateDefines.isValidDefine(queryvalue))
				{
					model.Date_Period = Utilities.Period(queryvalue);
					model.Date = Utilities.DefaultDate(model.Date_Period);
				}
				else
					model.Date = Utilities.DateTimeParseExact(queryvalue, queryvalue.Length != Date_Format.Length ? Date_Format.Substring(0, queryvalue.Length) : Date_Format, DateTime.MinValue);
			}

			//queryvalue = GetQueryValue(queries, cmp, c);
			//if (!string.IsNullOrEmpty(queryvalue))
			//	model.Cmp = Commons.Utils.GetEnum<AlertCompareParam.CompareType>(queryvalue);

			bindingContext.Model = model;
			model.Extend = queries.Where(item => !Keys.Contains(item.Key.ToLower()) && item.Key != periodkey);
			return true;
		}

		private string UpdatePeriodmodel(IEnumerable<KeyValuePair<string, string>> queries, PeriodParam model)
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

	}
}
