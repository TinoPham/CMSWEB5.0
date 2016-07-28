using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Net.Http;

namespace CMSSVR.Models.Api.Configuration
{

	public class PagedFilter
	{
		private int _nPageSize;
		private int _nPageNumber;

		public int PageSize
		{
			get { return _nPageSize; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("PageSize must be greater then 0");
				_nPageSize = value;
			}
		}

		public int PageNumber
		{
			get { return _nPageNumber; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("PageNumber must be greater then 0");
				_nPageNumber = value;
			}
		}

		public int TotalItems { get; set; }

		public int TotalPages
		{
			get { return (int)Math.Ceiling((double)(TotalItems / PageSize)); }
		}

		public PagedFilter()
		{
			_nPageSize = 20;
			_nPageNumber = 1;
		}
	}

	//[ModelBinder(typeof(UriLogFilterBinderProvider))]
	public class LogFilter : PagedFilter
	{
		public const string Filter_date_format = "yyyyMMddHHmm";
		const string str_Year = "Year";
		const string str_Month = "Month";
		const string str_Day = "Day";
		const string str_Hour = "Hour";
		const string str_Min = "Min";
		public const string Filter_date_regex = @"^(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Hour>\d{2})?(?<Min>\d{2})?$";

		public DateTime?Date{ get;set;}
		public Commons.Programset? Programset { get; set; }
		public List<string> SortFields { get; set; }
		public List<string> SortDirections { get; set; }

		public void SetDateSearch( string date)
		{
			if( string.IsNullOrEmpty(date))
				return;
			Regex rx = new Regex( Filter_date_regex);
			Match match = rx.Match(date);
			if(!match.Success)
				return;
			int year = Int32.Parse( match.Groups[str_Year].Value);
			int month = Int32.Parse(match.Groups[str_Month].Value);
			int day = Int32.Parse(match.Groups[str_Day].Value);

			int hour;
			 Int32.TryParse(match.Groups[str_Hour].Value, out hour);
			int minu;
			 Int32.TryParse(match.Groups[str_Min].Value, out minu);
			Date = new DateTime(year, month, day, hour, minu, 0);
		}
	}

	public class UriLogFilterBinderProvider : ModelBinderProvider
	{
		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new UriLogFilterBinder();
		}
	}
	public class UriLogFilterBinder : IModelBinder
	{
		private const string RexChechNumeric = @"^\d+$";
		private const string RexBrackets = @"\[\d*\]";
		private const string RexSearchBracket = @"\[([^}])\]";

		////Define original source data list
		//private List<KeyValuePair<string, string>> kvps;

		////Set default maximum resursion limit
		//private int maxRecursionLimit = 100;
		//private int recursionCount = 0;
		public UriLogFilterBinder()
		{

		}
		//Implement base member
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			
			IEnumerable<KeyValuePair<string, string>> queries = actionContext.Request.GetQueryNameValuePairs();
			LogFilter filter = new LogFilter();
			string qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<Nullable<DateTime>>(() => filter.Date));
			filter.SetDateSearch(qvalue);
			qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<Nullable<Commons.Programset>>(() => filter.Programset ));
			if(!string.IsNullOrEmpty(qvalue) )
				filter.Programset = Commons.Utils.GetEnum<Commons.Programset>(qvalue);
			qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<int>(() => filter.PageNumber));
			if(Commons.Utils.ValidationString(RexChechNumeric, qvalue))
				filter.PageNumber = Convert.ToInt32(qvalue);
			qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<int>(() => filter.PageSize));
			if (Commons.Utils.ValidationString(RexChechNumeric, qvalue))
				filter.PageSize = Convert.ToInt32(qvalue);
			qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<List<string>>(() => filter.SortFields));
			if(!string.IsNullOrEmpty(qvalue))
				filter.SortFields = new List<string>( qvalue.Split( new char[]{','}));
			qvalue = GetQueryValue(queries, Commons.Utils.GetPropertyName<List<string>>(() => filter.SortDirections));
			if (!string.IsNullOrEmpty(qvalue))
				filter.SortDirections = new List<string>(qvalue.Split(new char[] { ',' }));
			bindingContext.Model = filter;
			return true;
		}

		private string GetQueryValue(IEnumerable<KeyValuePair<string, string>> queries, string key)
		{
			KeyValuePair<string, string> item = queries.FirstOrDefault( q => string.Compare( q.Key, key, true ) == 0);
			return string.IsNullOrEmpty(item.Key)? null : item.Value;
		}
	}
	
}