using System.Linq;
using System.Linq.Expressions;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using System;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IUserGroupService
	{
		Tout Edit<Tout>(Tout data) where Tout : class;
		Tout Add<Tout>(Tout data) where Tout : class;
		bool Delete<Tout>(Tout data) where Tout: class;
		bool Delete<Tout>(IQueryable<Tout> value) where Tout : class;	

		IQueryable<UserGroupModel> Gets(int? CreatedBy);
		Tout Get<Tout>(Expression<Func<Tout, bool>> filter, Expression<Func<Tout, Tout>> selector, string[] includes) where Tout : class;
		//IQueryable<Tout> Gets<Tout>(Expression<Func<tCMSWeb_UserGroups, bool>> filters, Expression<Func<tCMSWeb_UserGroups, Tout>> selector, string[] includes);

		IQueryable<Tout> Get<Tin, Tout>(Expression<Func<Tin, bool>> filter, Expression<Func<Tin, Tout>> selector) where Tin : class;
		IQueryable<Tout> Gets<Tout>(Expression<Func<Tout, bool>> filter, Expression<Func<Tout, Tout>> selector, string[] includes = null) where Tout : class;
		IQueryable<tCMSWeb_Functions> GetFunctionList();

		void Remove_Users_fromGroup(tCMSWeb_UserGroups group, params tCMSWeb_UserList[] users);
		void Add_Users_toGroup(tCMSWeb_UserGroups group, params tCMSWeb_UserList[] users);
		void Remove_FuncLevels_fromGroup(tCMSWeb_UserGroups group, params tCMSWeb_Function_Level[] funcLVs);
		void Add_FuncLevels_toGroup(tCMSWeb_UserGroups group, params tCMSWeb_Function_Level[] funcLVs);
	}
}