using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.BusinessServices.UserGroups;

namespace CMSWebApi.Controllers
{
	[RoutePrefix("UserGroup")]
	[WebApiAuthenication]
	public class UserGroupController: ApiControllerBase<IUserGroupService, UserGroupBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage GetUserGroup()
		{
			var result = BusinessService.GetUserGroups(base.usercontext);
			return ResponseData<IQueryable<UserGroupModel>>(result);
		}

		[HttpGet]
		public HttpResponseMessage GetFunctionList()
		{
			IQueryable<FunctionModel> result = BusinessService.GetFunctionList();
			return ResponseData<IQueryable<FunctionModel>>(result);
		}

		[HttpGet]
		public HttpResponseMessage GetFuncLevel()
		{
			IQueryable<FunctionLevelModel> result = BusinessService.GetFuncLevel();
			return ResponseData<IQueryable<FunctionLevelModel>>(result);
		}

		[HttpGet]
		public HttpResponseMessage GetUserList()
		{
			base.BusinessService.UserSvc = base.DependencyResolver<IUsersService>();
			var result = BusinessService.GetUserList(base.usercontext);
			return ResponseData<IQueryable<UserSimple>>(result);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage UpdateUserGroup(UserGroupModel model)
		{
			var userGroup = BusinessService.UpdateUserGroup(model);
			return ResponseData<TransactionalModel<UserGroupModel>>(userGroup);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteUserGroup(userGroupDeleteModel model) // List<int> userGroups, [FromBody] int userGroupIdToReplace)
		{
			TransactionalModel<UserGroupModel> result = BusinessService.DeleteUserGroup(model);
			return ResponseData<TransactionalModel<UserGroupModel>>(result);
		}
	}
}
