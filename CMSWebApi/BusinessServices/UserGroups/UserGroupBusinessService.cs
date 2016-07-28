using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.UserGroups
{
	public class UserGroupBusinessService : BusinessBase<IUserGroupService>
	{
		public IUsersService UserSvc { get; set; }

		#region Utility UserGroup

		private tCMSWeb_UserGroups ToCmsUserGroup(UserGroupModel userGroup)
		{
			return new tCMSWeb_UserGroups()
			{
				GroupID = userGroup.GroupId,
				GroupName = userGroup.GroupName,
				GroupDescription = userGroup.Description,
				CreateBy = userGroup.CreatedBy
			};
		}

		private Expression<Func<UserGroupModel,tCMSWeb_UserGroups>> Selector_ToCmsUserGroup = userGroup => new tCMSWeb_UserGroups{
																																	GroupID = userGroup.GroupId,
																																	GroupName = userGroup.GroupName,
																																	GroupDescription = userGroup.Description,
																																	CreateBy = userGroup.CreatedBy,
																																	GroupLevel= userGroup.GroupLevel
																																};

		private Expression<Func<tCMSWeb_Levels, FunctionLevelModel>> Selector_Level = funcLevel => new FunctionLevelModel
																									{
																										LevelID= funcLevel.LevelID,
																										LevelName= funcLevel.LevelName
																									};

		private Expression<Func<tCMSWeb_UserGroups, UserGroupModel>> Selector_UserGroupModel = item => new UserGroupModel
		{
			GroupId = item.GroupID,
			GroupName = item.GroupName,
			Description = item.GroupDescription,
			CreatedBy = item.CreateBy,
			GroupLevel = item.GroupLevel,
			NumberUser = item.tCMSWeb_UserList.Count(),
			Users = item.tCMSWeb_UserList.Select(i => i.UserID).Distinct(),
			FuncLevels = item.tCMSWeb_Function_Level.Select(x => new FuncLevel { FunctionID = x.FunctionID, LevelID = x.LevelID })
		};

		#endregion

		public IQueryable<UserSimple> GetUserList(UserContext user)
		{
			IQueryable<tCMSWeb_UserList> tUserLists = UserSvc.GetListUser(item => item.CreatedBy==user.ID);
			IQueryable<UserSimple> users = (from item in tUserLists
											select new UserSimple { UserID = item.UserID, FName = item.UFirstName, LName = item.ULastName, Status= false });
			users.Select(i => i.UserID);
			return users;
		}

		public IQueryable<UserGroupModel> GetUserGroups(UserContext user)
		{
			return DataService.Gets(user.ID);
		}

		public IQueryable<FunctionModel> GetFunctionList()
		{
			IQueryable<tCMSWeb_Functions> functions = DataService.GetFunctionList();
			IQueryable<FunctionModel> model = (from item in functions
											   select new FunctionModel { FunctionID = item.FunctionID, FunctionName = item.FunctionName, ModuleID = item.ModuleID, Status=false });
			return model;
		}

		public IQueryable<FunctionLevelModel> GetFuncLevel()
		{
			IQueryable<FunctionLevelModel> levels = DataService.Get<tCMSWeb_Levels, FunctionLevelModel>(item => item.LevelID == 5 || item.LevelID == 6, Selector_Level);
			return levels;
		}

		public TransactionalModel<UserGroupModel> UpdateUserGroup(UserGroupModel userGroup)
		{
			TransactionalModel<UserGroupModel> returnmodel = new TransactionalModel<UserGroupModel>();
			returnmodel.ReturnStatus = true;
			tCMSWeb_UserGroups tUserGroup = new tCMSWeb_UserGroups();
			if (isGroupExisted(userGroup.GroupName, userGroup.CreatedBy, userGroup.GroupId))
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(CMSWebError.USERGROUPS_NAME_EXIST.ToString());
				return returnmodel;
			}

			RemoveRelationshipBetweenUseAndGroup(userGroup);
			tUserGroup = SetEntity(userGroup);

			returnmodel = tUserGroup.GroupID == 0 ? AddUserGroup(userGroup, tUserGroup) : EditUserGroup(userGroup, tUserGroup);
			return returnmodel;
		}
		
		private TransactionalModel<UserGroupModel> AddUserGroup(UserGroupModel userGroupModel, tCMSWeb_UserGroups tUserGroup)
		{
			TransactionalModel<UserGroupModel> response = new TransactionalModel<UserGroupModel>();
			tUserGroup = DataService.Add(tUserGroup);
			if (tUserGroup != null)
			{
				tUserGroup = UpdateRelationshipOfUserGroup(userGroupModel, tUserGroup);
			}

			if (tUserGroup == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			userGroupModel.GroupId = tUserGroup.GroupID;
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			response.Data = userGroupModel;
			return response;
		}

		private TransactionalModel<UserGroupModel> EditUserGroup(UserGroupModel userGroupModel, tCMSWeb_UserGroups tUserGroup)
		{
			TransactionalModel<UserGroupModel> response = new TransactionalModel<UserGroupModel>();
			tUserGroup = DataService.Edit(tUserGroup);
			if (tUserGroup != null)
			{
				tUserGroup = UpdateRelationshipOfUserGroup(userGroupModel, tUserGroup);
			}

			if (tUserGroup == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return response;
			}

			userGroupModel.GroupId = tUserGroup.GroupID;
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			response.Data = userGroupModel;
			return response;
		}

		private void RemoveRelationshipBetweenUseAndGroup(UserGroupModel userGroup)
		{
			string itemChild = ServiceBase.ChildProperty(typeof(tCMSWeb_UserList), typeof(tCMSWeb_UserGroups));
			IQueryable<tCMSWeb_UserList> user_list = UserSvc.GetListUser(item => userGroup.Users.Contains(item.UserID), new string[] { itemChild });
			foreach (tCMSWeb_UserList dbmodel in user_list)
			{
				if (dbmodel.tCMSWeb_UserGroups == null)
					dbmodel.tCMSWeb_UserGroups = new List<tCMSWeb_UserGroups>();

				ICollection<tCMSWeb_UserGroups> old_groups = dbmodel.tCMSWeb_UserGroups;
				IQueryable<tCMSWeb_UserGroups> new_groups = DataService.Gets<tCMSWeb_UserGroups>(it => it.GroupID == userGroup.GroupId, it => it, null);
				IEnumerable<tCMSWeb_UserGroups> del_groups = old_groups.Where(it => it.GroupID != userGroup.GroupId);
				IEnumerable<int> list_old_group = old_groups.Select(item => item.GroupID);
				new_groups = new_groups.Where(item => !list_old_group.Contains(item.GroupID));
				if (del_groups.Any())
					UserSvc.Remove_Groups_fromUser(dbmodel, del_groups.ToArray());
				
				 UserSvc.Update(dbmodel);
			}		
		}

		private tCMSWeb_UserGroups SetEntity(UserGroupModel userGroup)
		{
			string[] include = ServiceBase.ChildProperties(typeof(tCMSWeb_UserGroups)).ToArray();
			tCMSWeb_UserGroups tUserGroup = userGroup.GroupId > 0 ?
												DataService.Get<tCMSWeb_UserGroups>(value => value.GroupID == userGroup.GroupId, item => item, include) : new tCMSWeb_UserGroups();
			if (userGroup == null)
				return tUserGroup;

			tUserGroup.GroupID = userGroup.GroupId;
			tUserGroup.GroupName = userGroup.GroupName;
			tUserGroup.GroupDescription = userGroup.Description; 
			tUserGroup.CreateBy = userGroup.CreatedBy;
			tUserGroup.GroupLevel = userGroup.GroupLevel;			
			return tUserGroup;		
		}

		private tCMSWeb_UserGroups UpdateRelationshipOfUserGroup(UserGroupModel userGroup, tCMSWeb_UserGroups tUserGroup)
		{
			//
			//Add data to table User_UserGroups
			//
			if (tUserGroup.tCMSWeb_UserList == null)
				tUserGroup.tCMSWeb_UserList = new List<tCMSWeb_UserList>();

			ICollection<tCMSWeb_UserList> old_users = tUserGroup.tCMSWeb_UserList;
			IQueryable<tCMSWeb_UserList> new_users = UserSvc.GetListUser(item => userGroup.Users.Contains(item.UserID), null);
			IEnumerable<int> list_old_userid = old_users.Select(i => i.UserID);
			IQueryable<int> list_new_userid = new_users.Select(i => i.UserID);
			IEnumerable<tCMSWeb_UserList> del_users = old_users.Where(item => !list_new_userid.Contains(item.UserID));
			new_users = new_users.Where(item => !list_old_userid.Contains(item.UserID));
			if (del_users.Any())
				DataService.Remove_Users_fromGroup(tUserGroup, del_users.ToArray());
			if (new_users.Any())
				DataService.Add_Users_toGroup(tUserGroup, new_users.ToArray());

			//
			//Add data to table UserGroup_Function
			//
			if (tUserGroup.tCMSWeb_UserList == null)
				tUserGroup.tCMSWeb_UserList = new List<tCMSWeb_UserList>();

			ICollection<tCMSWeb_Function_Level> old_funclvs = tUserGroup.tCMSWeb_Function_Level;
			ICollection<tCMSWeb_Function_Level> new_funclvs = new List<tCMSWeb_Function_Level>();
			foreach (FuncLevel level in userGroup.FuncLevels)
			{
				new_funclvs.Add(DataService.Get<tCMSWeb_Function_Level>(item => item.FunctionID == level.FunctionID && item.LevelID == level.LevelID, value => value, null));
			}
			IEnumerable<int> list_old_funclvid = old_funclvs.Select(i => i.FunclevelID);
			IEnumerable<int> list_new_funclvid = new_funclvs.Select(i => i.FunclevelID);
			IEnumerable<tCMSWeb_Function_Level> del_funclvs = old_funclvs.Where(item => !list_new_funclvid.Contains(item.FunclevelID));
			new_funclvs = new_funclvs.Where(item => !list_old_funclvid.Contains(item.FunclevelID)).ToArray();
			if (del_funclvs.Any())
				DataService.Remove_FuncLevels_fromGroup(tUserGroup, del_funclvs.ToArray());
			if (new_funclvs.Any())
				DataService.Add_FuncLevels_toGroup(tUserGroup, new_funclvs.ToArray());

			return DataService.Edit(tUserGroup);
		}

		private bool isGroupExisted(string groupName, Nullable<int> CreateBy, int groupID)
		{
			if (!string.IsNullOrEmpty(groupName))
			{
				IQueryable<tCMSWeb_UserGroups> tUserGroup = ServiceBase.Query<tCMSWeb_UserGroups, tCMSWeb_UserGroups>(item => item.GroupName == groupName && item.CreateBy== CreateBy, item => item, null);
				if (groupID == 0 && tUserGroup.Any())
					return true;
				if (groupID != 0 && tUserGroup.FirstOrDefault(i => i.GroupID != groupID) != null)
					return true;
			}			
			return false;
		}

		public TransactionalModel<UserGroupModel> DeleteUserGroup(userGroupDeleteModel model)
		{
			TransactionalModel<UserGroupModel> response = new TransactionalModel<UserGroupModel>();
			if (model.listUserGroupId.Count == 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			//Replace new user group for users who had be assigned to user groups that will be deleting.
			string[] include = ServiceBase.ChildProperties(typeof(tCMSWeb_UserGroups)).ToArray();
			if(model.userGroupIdReplace > 0)
			{
				tCMSWeb_UserGroups userGroupReplace = DataService.Gets<tCMSWeb_UserGroups>(uGroup => uGroup.GroupID == model.userGroupIdReplace, item => item, null).FirstOrDefault();
				IQueryable<tCMSWeb_UserGroups> oldUserGroup = DataService.Gets<tCMSWeb_UserGroups>(uGroup => model.listUserGroupId.Contains(uGroup.GroupID), item => item, include);
				foreach (tCMSWeb_UserGroups userGroup in oldUserGroup.ToList())
				{
					if (userGroup.tCMSWeb_UserList.Any())
					{
						DataService.Add_Users_toGroup(userGroupReplace, userGroup.tCMSWeb_UserList.ToArray());
						DataService.Remove_Users_fromGroup(userGroup, userGroup.tCMSWeb_UserList.ToArray());
					}
				}
			}
			
			//Delete user Group on DB
			IQueryable<tCMSWeb_UserGroups> userGroupDelete = DataService.Gets<tCMSWeb_UserGroups>(item => model.listUserGroupId.Contains(item.GroupID), value => value, include);
			if (!DataService.Delete<tCMSWeb_UserGroups>(userGroupDelete))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}
			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return response;
		}
	}
}
