using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using System.Linq.Expressions;


namespace CMSWebApi.DataServices
{
	public partial class JobTitleService : ServiceBase, IJobTitleService
	{

		public JobTitleService(PACDMModel.Model.IResposity model) : base(model) { }

		public JobTitleService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		/// <summary>
		/// Insert Update Delete tCMSWeb_UserPosition
		/// </summary>

		public tCMSWeb_UserPosition Add(tCMSWeb_UserPosition jobTitle)
		{
			DBModel.Insert<tCMSWeb_UserPosition>(jobTitle);
			return DBModel.Save() > 0 ? jobTitle : null;
		}

		public tCMSWeb_UserPosition Update(tCMSWeb_UserPosition jobTitle)
		{
			DBModel.Update<tCMSWeb_UserPosition>(jobTitle);
			return DBModel.Save() >= 0 ? jobTitle : null;
		}

		public Tout Gets<Tout>(int ID, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout : class
		{
			return base.Query<tCMSWeb_UserPosition, Tout>(item => ID == item.PositionID, selector, null).FirstOrDefault();
		}
		public IQueryable<Tout> Gets<Tout>(string jobName, int CreatedBy, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout: class
		{
			return base.Query<tCMSWeb_UserPosition, Tout>( item => jobName.Equals(item.PositionName) && item.CreatedBy== CreatedBy, selector, null);
		}
		public IQueryable<Tout> Gets<Tout>(int? createby, string[] includes, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout : class
		{
			IQueryable<Tout> result = createby.HasValue ? base.Query<tCMSWeb_UserPosition, Tout>(user => user.CreatedBy == createby.Value,selector, includes): base.Query<tCMSWeb_UserPosition, Tout>(null, selector, includes);
			//!createby.HasValue ? DBModel.Query<tCMSWeb_UserPosition>(null, includes) : DBModel.Query<tCMSWeb_UserPosition>(user => user.CreatedBy == createby.Value, includes);
			return result;
		}

		public bool Delete(List<int> listJobID)
		{
			DBModel.DeleteWhere<tCMSWeb_UserPosition>(userpos => listJobID.Contains(userpos.PositionID));
			return DBModel.Save() > 0 ? true : false;
		}

		public bool Delete(tCMSWeb_UserPosition jobPos)
		{
			if( jobPos == null)
				return true;
			List<int> listJobID = new List<int>();
			listJobID.Add(jobPos.PositionID);
			return Delete(listJobID);
		}
	}
}
