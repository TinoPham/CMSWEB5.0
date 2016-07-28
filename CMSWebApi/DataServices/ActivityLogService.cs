using System.Linq;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMDB = PACDMModel.PACDMDB;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices
{
	public class ActivityLogService : ServiceBase, IActivityLogService
	{
		public ActivityLogService(PACDMModel.Model.IResposity model) : base(model) { }

		public ActivityLogService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		/// <summary>
		/// Insert Update Delete tCMSWeb_ActivityLog
		/// </summary>

		public tCMSWeb_ActivityLog Add(tCMSWeb_ActivityLog log)
		{
			DBModel.Insert<tCMSWeb_ActivityLog>(log);
			return DBModel.Save() > 0 ? log : null;
		}

		public tCMSWeb_ActivityLog Delete(tCMSWeb_ActivityLog log)
		{
			DBModel.Delete<tCMSWeb_ActivityLog>(log);
			return DBModel.Save() > 0 ? log : null;
		}
		
		// get activity log following condition create it
		public IQueryable<Tout> Gets<Tout>(Expression<Func<Tout, bool>> filter, string[] includes = null) where Tout: class
		{
			IQueryable<Tout> model = DBModel.Query<Tout>(filter, includes);
			return model;
		}

	}
}
