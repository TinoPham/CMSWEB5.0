using System.Collections.Generic;
using System.Linq;
using CMSWebApi.DataModels;
using PACDMModel.Model;
using PACDMDB = PACDMModel.PACDMDB;
using CMSWebApi.ServiceInterfaces;
using System.Linq.Expressions;
using System;

namespace CMSWebApi.DataServices
{
	public partial class UserGroupService : ServiceBase, IUserGroupService
	{
		public UserGroupService(PACDMModel.Model.IResposity model)
			: base(model)
		{
		}

		public UserGroupService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public Tout Edit<Tout>(Tout data) where Tout : class
		{
			DBModel.Update<Tout>(data);
			return DBModel.Save() >= 0 ? data : null;
		}

		public Tout Add<Tout>(Tout data) where Tout : class
		{
			DBModel.Insert<Tout>(data);
			return DBModel.Save() > 0 ? data : null;
		}

		public bool Delete<Tout>(Tout data) where Tout : class
		{
			DBModel.Delete<Tout>(data);
			return DBModel.Save() >= 0;
		}

		public bool Delete<Tout>(IQueryable<Tout> value) where Tout : class
		{
			value.ToList().ForEach(item => DBModel.Delete<Tout>(item));
			return DBModel.Save() >= 0;
		}

		public Tout Get<Tout>(int id, Expression<Func<tCMSWeb_UserGroups, Tout>> selector) where Tout : class
		{
			return FirstOrDefault<tCMSWeb_UserGroups, Tout>(item => item.GroupID == id, selector);
		}

		public  IQueryable<Tout> Gets<Tout>(Expression<Func<tCMSWeb_UserGroups, bool>> filters, Expression<Func<tCMSWeb_UserGroups, Tout>> selector, string[] includes)
		{
			return base.Query<tCMSWeb_UserGroups, Tout>( filters, selector, includes);  //FirstOrDefault<tCMSWeb_UserGroups, Tout>(item => item.GroupID == id, selector);
		}

		public IQueryable<Tout> Get<Tin, Tout>(Expression<Func<Tin, bool>> filter, Expression<Func<Tin, Tout>> selector) where Tin : class
		{
			return Query<Tin, Tout>(filter, selector);
		}

		public IQueryable<Tout> Gets<Tout>(Expression<Func<Tout, bool>> filter, Expression<Func<Tout, Tout>> selector, string[] includes = null) where Tout : class
		{
			return Query<Tout, Tout>(filter, selector, includes);
		}

		public Tout Get<Tout>(Expression<Func<Tout, bool>> filter, Expression<Func<Tout, Tout>> selector, string[] inludes) where Tout : class
		{
			return FirstOrDefault<Tout, Tout>(filter, selector, inludes);
		}

		public IQueryable<UserGroupModel> Gets(int? CreatedBy)
		{
			IQueryable<tCMSWeb_UserGroups> groups = CreatedBy.HasValue ? DBModel.Query<tCMSWeb_UserGroups>(t => t.CreateBy == CreatedBy) : DBModel.Query<tCMSWeb_UserGroups>();
			IQueryable<UserGroupModel> result = groups
				.Select(
					(x) =>
						new UserGroupModel()
						{
							GroupId = x.GroupID,
							GroupName = x.GroupName,
							Description = x.GroupDescription,
							CreatedBy = x.CreateBy,
							GroupLevel = x.GroupLevel,
							NumberUser = x.tCMSWeb_UserList.Count(),
							Users = x.tCMSWeb_UserList.Select(i => i.UserID).Distinct(),
							FuncLevels = x.tCMSWeb_Function_Level.Select(i => new FuncLevel { FunctionID = i.FunctionID, LevelID = i.LevelID })
						});

			return result;
		}

		public IQueryable<tCMSWeb_Functions> GetFunctionList()
		{
			return DBModel.Query<tCMSWeb_Functions>(item => item.Displayed == true);
		}

		public IQueryable<FunctionModel> GetFuncLevelModel()
		{
			var webFunction = DBModel.Query<tCMSWeb_Functions>();
			var webFuncLevel = DBModel.Query<tCMSWeb_Function_Level>();

			var result = webFunction
				.Join(webFuncLevel.Select(x=> new {x.FunclevelID, x.FunctionID, x.LevelID}), p => p.FunctionID, v => v.FunctionID, (p, v) => new {WebFn = p, WebFnLv = v})
				.Select(
					(x) =>
						new FunctionModel()
						{
							FunctionID = x.WebFn.FunctionID,
							FunctionName = x.WebFn.FunctionName,
							ModuleID = x.WebFn.ModuleID ?? 0,
						});
			return result;
		}

		public void Remove_Groups_fromUsers(tCMSWeb_UserList user, params tCMSWeb_UserGroups[] groups)
		{
			DBModel.DeleteItemRelation<tCMSWeb_UserList, tCMSWeb_UserGroups>(user, u => u.tCMSWeb_UserGroups, groups);
		}

		public void Remove_Users_fromGroup(tCMSWeb_UserGroups user, params tCMSWeb_UserList[] users)
		{
			DBModel.DeleteItemRelation<tCMSWeb_UserGroups, tCMSWeb_UserList>(user, u => u.tCMSWeb_UserList, users);
		}
		public void Add_Users_toGroup(tCMSWeb_UserGroups group, params tCMSWeb_UserList[] users)
		{
			DBModel.AddItemRelation<tCMSWeb_UserGroups, tCMSWeb_UserList>(group, u => u.tCMSWeb_UserList, users);
		}
		public void Remove_FuncLevels_fromGroup(tCMSWeb_UserGroups group, params tCMSWeb_Function_Level[] funcLVs)
		{
			DBModel.DeleteItemRelation<tCMSWeb_UserGroups, tCMSWeb_Function_Level>(group, u => u.tCMSWeb_Function_Level, funcLVs);
		}
		public void Add_FuncLevels_toGroup(tCMSWeb_UserGroups group, params tCMSWeb_Function_Level[] funcLVs)
		{
			DBModel.AddItemRelation<tCMSWeb_UserGroups, tCMSWeb_Function_Level>(group, u => u.tCMSWeb_Function_Level, funcLVs);
		}

	}
}
