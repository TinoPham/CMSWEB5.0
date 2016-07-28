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
	public partial class SynUserService : ServiceBase, ISynUserService
	{
		public SynUserService(PACDMModel.Model.IResposity model) : base(model) { }

		public SynUserService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		/// <summary>
		/// Insert Update Delete tCMSWeb_UserPosition
		/// </summary>

		public tCMSWeb_SynUser AddSynUser(tCMSWeb_SynUser SynUser)
		{
			DBModel.Insert<tCMSWeb_SynUser>(SynUser);
			return DBModel.Save() > 0 ? SynUser : null;
		}

		public tCMSWeb_SynUser UpdateSynUser(tCMSWeb_SynUser SynUser)
		{
			DBModel.Update<tCMSWeb_SynUser>(SynUser);
			return DBModel.Save() >= 0 ? SynUser : null;
		}

		public bool DeleteSynUser(int SynID)
		{
			DBModel.DeleteWhere<tCMSWeb_SynUser>(id => id.SynID == SynID);
			return DBModel.Save() > 0 ? true : false;
		}

		//public IQueryable<tCMSWeb_SynUser> SelectAllSynUser(int userID)
		//{
		//	IQueryable<tCMSWeb_SynUser> model = DBModel.Query<tCMSWeb_SynUser>(i => i.CreateBy == userID);
		//	return model;
		//}

		public IQueryable<Tout> SelectSynUser<Tout>(string ServerIP, int SynType, Expression<Func<tCMSWeb_SynUser, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> models = Query<tCMSWeb_SynUser, Tout>(i => i.ServerIP == ServerIP && i.SynType == SynType, selector, includes);

			return models;
		}
		//public IQueryable<tCMSWeb_SynUser> SelectSynUser(string ServerIP, int SynType)
		//{
		//	IQueryable<tCMSWeb_SynUser > models = DBModel.Query<tCMSWeb_SynUser>(i => i.ServerIP == ServerIP && i.SynType == SynType);
		//	return models;
		//}

		public tCMSWeb_SynUser SelectSynUser(int SynID)
		{
			tCMSWeb_SynUser model = DBModel.FirstOrDefault<tCMSWeb_SynUser>(i => i.SynID == SynID); //DBModel.Query<tCMSWeb_SynUser>(i => i.SynID == SynID).FirstOrDefault();
			return model;
		}

		public IQueryable<Tout> SelectAllSynUser<Tout>(int userID, Expression<Func<tCMSWeb_SynUser, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> model = Query<tCMSWeb_SynUser, Tout>(i => i.CreateBy == userID, selector, includes);
			return model;
		}

		public IQueryable<Tout> SelectSynUserType<Tout>(Expression<Func<tCMSWeb_SynUser_Types, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> model = Query<tCMSWeb_SynUser_Types, Tout>(null , selector, includes);
			return model;
		}

		//public IQueryable<tCMSWeb_SynUser_Types> SelectSynUserType()
		//{
		//	IQueryable<tCMSWeb_SynUser_Types> model = DBModel.Query<tCMSWeb_SynUser_Types>();
		//	return model;
		//}

		///
		///User
		///
		//public tCMSWeb_UserList SelectUser(int userID)
		//{
		//	tCMSWeb_UserList model = DBModel.FirstOrDefault<tCMSWeb_UserList>(i=> i.UserID==userID);// DBModel.Query<tCMSWeb_UserList>(i=> i.UserID==userID).FirstOrDefault();
		//	return model;
		//}
	}
}
