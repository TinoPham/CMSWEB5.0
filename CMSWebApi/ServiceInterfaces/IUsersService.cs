using System.Linq;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IUsersService
	{
		tCMSWeb_UserList Update(tCMSWeb_UserList user);
		tCMSWeb_UserList Add(tCMSWeb_UserList user);
		IQueryable<tCMSWeb_UserList> GetListUser(Expression<Func<tCMSWeb_UserList, bool>> filter, string[] includes = null);
		IQueryable<Tout> Gets<Tout>(int? createby, IEnumerable<int> Contains, string[] includes, Expression<Func<tCMSWeb_UserList, Tout>> selector);
		Tout Get<Tout>(int userID, Expression<Func<tCMSWeb_UserList, Tout>> selector, string[] includes);
		Tout Get<Tout>(int userID, Expression<Func<tCMSWeb_UserList, Tout>> selector);
		int GetMasterID(int CompanyID);
		bool Delete<Tout>(Expression<Func<Tout, bool>> predicate, bool isSave = true) where Tout : class;
		bool Delete<Tout>(IQueryable<Tout> value) where Tout : class;

		IQueryable<Tout> SelectUserPositions<Tout>(int userID, Expression<Func<tCMSWeb_UserPosition, Tout>> selector, string[]includes ) where Tout : class;
		IQueryable<Tout> SelectUserGroups<Tout>(int userID, Expression<Func<tCMSWeb_UserGroups, Tout>> selector, string [] includes) where Tout : class;
		IEnumerable<Tout> GetDvrbyUser<Tout>(int userid, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector);
		Task<IEnumerable<Tout>> GetDvrbyUserAsync<Tout>(int userid, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector);
		Task<IEnumerable<Tout>> GetDvrbyUserAsync<Tout>(int userid, IEnumerable<int> lsSiteIDs, Expression<Func<Func_CMSWeb_DVRFollowUSer_Result, Tout>> selector);
		//IQueryable<tCMSWeb_UserPosition> SelectUserPositions(int userID);
		//IQueryable<tCMSWeb_UserGroups> SelectUserGroups(int userID);
		//IQueryable<tCMSWeb_User_Sites> SelectUserSites(int userID);
		int GetMaxEmployeeID();
		void Remove_Groups_fromUser( tCMSWeb_UserList user, params tCMSWeb_UserGroups[] groups);
		void Add_Groups_toUser(tCMSWeb_UserList user, params tCMSWeb_UserGroups[] groups);
		void Remove_Sites_fromUser(tCMSWeb_UserList user, params tCMSWebSites[] sites);
		void Add_Sites_toUser(tCMSWeb_UserList user, params tCMSWebSites[] sites);
	}

}
