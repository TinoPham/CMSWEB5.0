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
	public class UsersService : ServiceBase, IUsersService
	{
		public UsersService(PACDMModel.Model.IResposity model) : base(model) { }
		public UsersService(ServiceBase svrbase) : base(svrbase.DBModel) { }

		/// <summary>
		/// Insert Update Delete tCMSWeb_UserList
		/// </summary>

		public tCMSWeb_UserList Add(tCMSWeb_UserList user)
		{
			DBModel.Insert<tCMSWeb_UserList>(user);
			return DBModel.Save() > 0 ? user : null;
		}

		public int GetMasterID(int CompanyID)
		{
			return base.FirstOrDefault<tCMSWeb_UserList, int>(item => item.CompanyID == CompanyID && item.isAdmin == true, item => item.UserID);
		}

		// get user list following user create it
		public IQueryable<tCMSWeb_UserList> GetListUser(Expression<Func<tCMSWeb_UserList, bool>> filter, string[] includes = null)
		{
			IQueryable<tCMSWeb_UserList> model = DBModel.Query<tCMSWeb_UserList>(filter, includes);
			return model;
		}

		public Tout Get<Tout>(int userID, Expression<Func<tCMSWeb_UserList, Tout>> selector)
		{
			return base.FirstOrDefault<tCMSWeb_UserList, Tout>(item => item.UserID == userID, selector);
		}

		public Tout Get<Tout>(int userID, Expression<Func<tCMSWeb_UserList, Tout>> selector, string[] include)
		{
			return base.FirstOrDefault<tCMSWeb_UserList, Tout>(item => item.UserID == userID, selector, include);
		}

		public void Remove_Groups_fromUser(tCMSWeb_UserList user, params tCMSWeb_UserGroups[] groups)
		{
			DBModel.DeleteItemRelation<tCMSWeb_UserList, tCMSWeb_UserGroups>( user, u => u.tCMSWeb_UserGroups,groups);
		}
		public void Add_Groups_toUser(tCMSWeb_UserList user, params tCMSWeb_UserGroups[] groups)
		{
			DBModel.AddItemRelation<tCMSWeb_UserList, tCMSWeb_UserGroups>(user, u => u.tCMSWeb_UserGroups, groups);
		}
		public void Remove_Sites_fromUser(tCMSWeb_UserList user, params tCMSWebSites[] sites)
		{
			DBModel.DeleteItemRelation<tCMSWeb_UserList, tCMSWebSites>(user, u => u.tCMSWebSites, sites);
		}
		public void Add_Sites_toUser(tCMSWeb_UserList user, params tCMSWebSites[] sites)
		{
			DBModel.AddItemRelation<tCMSWeb_UserList, tCMSWebSites>(user, u => u.tCMSWebSites, sites);
		}
		public IQueryable<Tout> Gets<Tout>(int? createby, IEnumerable<int> Contains, string[]includes, Expression<Func<tCMSWeb_UserList,Tout>> selector)
		{
			IQueryable<tCMSWeb_UserList> result = !createby.HasValue? DBModel.Query<tCMSWeb_UserList>( null, includes) : DBModel.Query<tCMSWeb_UserList>( user => user.CreatedBy == createby.Value, includes);
			return ( Contains == null || Contains.Count() == 0)? result.Select(selector) :  result.Where( item => Contains.Contains(item.UserID)).Select(selector);
		}

		public tCMSWeb_UserList Update(tCMSWeb_UserList user)
		{
			DBModel.Update<tCMSWeb_UserList>(user);			
			return DBModel.Save() >= 0 ? user : null;
		}

		public bool Delete<Tout>(Expression<Func<Tout, bool>> predicate, bool isSave = true) where Tout : class
		{
			DBModel.DeleteWhere<Tout>(predicate);
			if (isSave)
				return DBModel.Save() > 0;
			else
				return true;
		}

		public bool Delete<Tout>(IQueryable<Tout> value) where Tout : class
		{
			value.ToList().ForEach(item=> DBModel.Delete<Tout>(item));
			return DBModel.Save() > 0;
		}

		public tCMSWeb_UserList Get(string userName)
		{
			tCMSWeb_UserList model = DBModel.FirstOrDefault<tCMSWeb_UserList>(user => user.UUsername.Equals(userName));
			return model;
		}

		public IQueryable<Tout> SelectUserPositions<Tout>(int userID, Expression<Func<tCMSWeb_UserPosition, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> model = Query<tCMSWeb_UserPosition, Tout>(i => i.CreatedBy == userID, selector, includes);
			return model;
		}

		public IQueryable<Tout> SelectUserGroups<Tout>(int userID, Expression<Func<tCMSWeb_UserGroups, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> model = Query<tCMSWeb_UserGroups, Tout>(i => i.CreateBy == userID, selector, includes);
			return model;
		}

		public IEnumerable<Tout> GetDvrbyUser<Tout>(int userid, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector)
		{
			SqlParameter puserid = GetDvrbyUser_Params(userid);
			string sql = string.Format(SQLFunctions.Func_CMSWeb_DVRFollowUSer, puserid.ParameterName );// "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@UserID)";
			IEnumerable<Func_CMSWeb_DVRFollowUSer_Result> result = DBModel.ExecWithStoreProcedure<Func_CMSWeb_DVRFollowUSer_Result>(sql, puserid);
			return result.Select<Func_CMSWeb_DVRFollowUSer_Result, Tout>(selector.Compile());
		}

		public async Task<IEnumerable<Tout>> GetDvrbyUserAsync<Tout>(int userid, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector)
		{
			SqlParameter puserid = GetDvrbyUser_Params(userid);
			string sql = string.Format(SQLFunctions.Func_CMSWeb_DVRFollowUSer, puserid.ParameterName);// "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@UserID)";
			Task<List<Func_CMSWeb_DVRFollowUSer_Result>> Tresult = DBModel.ExecWithStoreProcedureAsync<Func_CMSWeb_DVRFollowUSer_Result>(sql, puserid);
			List<Func_CMSWeb_DVRFollowUSer_Result> result = await Tresult;
			return result.Select<Func_CMSWeb_DVRFollowUSer_Result, Tout>(selector.Compile());
		}
		public async Task<IEnumerable<Tout>> GetDvrbyUserAsync<Tout>(int userid, IEnumerable<int> lsSiteIDs, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector)
		{
			SqlParameter puserid = GetDvrbyUser_Params(userid);
			string sql = string.Format(SQLFunctions.Func_CMSWeb_DVRFollowUSer_Filter, puserid.ParameterName, "siteKey", (lsSiteIDs == null || !lsSiteIDs.Any()) ? "0" : string.Join(",", lsSiteIDs));// "SELECT * FROM Func_CMSWeb_DVRFollowUSer(@UserID)";
			Task<List<Func_CMSWeb_DVRFollowUSer_Result>> Tresult = DBModel.ExecWithStoreProcedureAsync<Func_CMSWeb_DVRFollowUSer_Result>(sql, puserid);
			List<Func_CMSWeb_DVRFollowUSer_Result> result = await Tresult;
			return result.Select<Func_CMSWeb_DVRFollowUSer_Result, Tout>(selector.Compile());
		}

		public int GetMaxEmployeeID()
		{
			var model = DBModel.Query<tCMSWeb_UserList>().Select(s => s.UEmployeeID);
			return model != null ? model.Max() + 1 : 0;
		}

		private SqlParameter GetDvrbyUser_Params( int userid)
		{
			SqlParameter puserid = new SqlParameter("UserID", System.Data.SqlDbType.Int){ Value = userid, Direction = System.Data.ParameterDirection.Input};
			return puserid;
			
		}
	}
}
