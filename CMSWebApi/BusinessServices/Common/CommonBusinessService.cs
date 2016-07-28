using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Common
{
	public class CommonBusinessService : BusinessBase<ICommonInfoService>
	{
		Expression<Func<countries, Country>> Selector_Country = item => new Country
		{
																							Id = item.id,
																							Code = item.CODE == null ? null : item.CODE.Trim(),
																							Name = item.name.Trim(),
																							Sort = item.sort
																						};
		Expression<Func<states, State>> Selector_State = item => new State{
																				Id = item.id,
																				Name = item.name,
																				CountryCode = item.country
																			};

		public IQueryable<Country> GetCountries()
		{
			return DataService.GetCountries<Country>(Selector_Country);
			
		}

		public IQueryable<State> GetStateses(string countryCode)
		{
			if(string.IsNullOrEmpty(countryCode))
				return new List<State>() as IQueryable<State>;

			return DataService.GetStateses<State>(countryCode, Selector_State);
		}
	}
}
