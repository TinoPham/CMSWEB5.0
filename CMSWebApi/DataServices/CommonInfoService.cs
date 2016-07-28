using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.DataServices
{
	public partial class CommonInfoService : ServiceBase, ICommonInfoService
	{
		public CommonInfoService(PACDMModel.Model.IResposity model)
			: base(model)
		{
		}

		public CommonInfoService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<Tout> GetCountries<Tout>(Expression<Func<countries, Tout>> selector) where Tout : class
		{
			return Query<countries, Tout>(null, selector);
		}
		public IQueryable<Tout> GetStateses<Tout>(string countrycode,Expression<Func<states, Tout>> selector) where Tout : class
		{
			return Query<states, Tout>(item => item.country.Trim() == countrycode, selector);
		}

		public IQueryable<Tout> GetStateses<Tout>(IEnumerable<int> states, Expression<Func<states, Tout>> selector)
		{
			return base.Query<states, Tout>( item => states.Contains(item.id), selector, null);
		}
		//public IQueryable<countries> GetCountries()
		//{
		//	return DBModel.Query<countries>();
		//}

		//public IQueryable<states> GetStateses()
		//{
		//	return DBModel.Query<states>();
		//}

		//public IQueryable<tCMSWeb_Metric_List> GetMetrics()
		//{
		//	return DBModel.Query<tCMSWeb_Metric_List>();
		//}
	}
}
