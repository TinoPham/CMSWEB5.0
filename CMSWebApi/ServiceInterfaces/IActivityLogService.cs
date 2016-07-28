using System.Linq;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IActivityLogService
	{
		tCMSWeb_ActivityLog Add(tCMSWeb_ActivityLog logmodel);
		IQueryable<Tout> Gets<Tout>(Expression<Func<Tout, bool>> filter, string[] includes = null) where Tout: class;
	}
}
