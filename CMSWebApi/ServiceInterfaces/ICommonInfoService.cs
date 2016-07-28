using System;
using System.Linq;
using System.Linq.Expressions;
using PACDMModel.Model;
using System.Collections;
using System.Collections.Generic;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ICommonInfoService
	{
		IQueryable<Tout> GetCountries<Tout>( Expression<Func<countries, Tout>> selector ) where Tout: class;
		IQueryable<Tout> GetStateses<Tout>( string countrycode, Expression<Func<states, Tout>> selector) where Tout : class;
		IQueryable<Tout> GetStateses<Tout>(IEnumerable<int> states, Expression<Func<states, Tout>> selector);
		//IQueryable<states> GetStateses();
		//IQueryable<tCMSWeb_Metric_List> GetMetrics();
	}
}
