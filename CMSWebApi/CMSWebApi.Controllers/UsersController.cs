//using System.Web.OData.Query;
using CMSWebApi.APIFilters;
using CMSWebApi.BusinessServices.Users;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.Utils;
using System.Web;
using System.IO;

namespace CMSWebApi.Controllers
{
	[WebApiAuthenication]
	public class UsersController : ApiControllerBase<IUsersService, UsersBusinessService>
	{
		
		[HttpGet]
		public HttpResponseMessage GetUserImage(int id, string name = "")
		{
			return ResponseFile(BusinessService.ImageSrc(id, name));
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage Users()
		{
			IQueryable<UserInfo> userAdmins = BusinessService.GetAllUsers(usercontext);
			return ResponseData<IQueryable<UserInfo>>(userAdmins);
		}

		[HttpGet]
		public HttpResponseMessage UserDetail(int userID)
		{
			UserModel userAdmins = BusinessService.GetUserDetail(userID);
			return ResponseData<UserModel>(userAdmins);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage UpdateProfile(UserProfile userModel)
		{
			TransactionalModel<UserProfile> model = BusinessService.UpdateProfile(userModel, base.SID);
			return ResponseData<TransactionalModel<UserProfile>>(model);
		}
		
		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage AddUser(UserModel userModel)
		{
			base.BusinessService.UserGroupSvc = base.DependencyResolver<IUserGroupService>();
			userModel.CreatedBy = usercontext.ID;
			TransactionalModel<UserModel> model = BusinessService.AddUser(userModel, base.SID);
			return ResponseData<TransactionalModel<UserModel>>(model);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteUser([FromBody] List<int> userID)
		{
			TransactionalModel<UserModel> response = BusinessService.DeleteUser(userID, base.usercontext);
			return ResponseData<TransactionalModel<UserModel>>(response);
		}

		[HttpGet]
		public HttpResponseMessage GetMaxEmployeeID()
		{
			int result = BusinessService.GetMaxEmployeeID();
			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
	}
}
