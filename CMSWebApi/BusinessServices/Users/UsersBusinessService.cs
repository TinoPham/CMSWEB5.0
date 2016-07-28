using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.Http;
using Cryptography;
using Extensions;
using CMSWebApi.DataServices;

namespace CMSWebApi.BusinessServices.Users
{
	public class UsersBusinessService : BusinessBase<IUsersService>
	{
		private int BORDER_LEFT_COLOR_DEFAULT = 13487308; // COLOR: #cdcccc
		public IUserGroupService UserGroupSvc { get; set; }
		public ISiteService SiteSvc { get; set; }
		public IBamMetricService BamMetricSvc { get; set; }
		
		public IQueryable<UserInfo> GetUsers()
		{
			return DataService.Gets<UserInfo>((Nullable<int>)null ,null,null, item => 
																				new UserInfo{ EmpID = item.UEmployeeID
																						, FName = item.UFirstName,LName = item.ULastName
																						,UID = item.UserID
																						,PosName = item.tCMSWeb_UserPosition.PositionName
																						} ); //null;//DataService.GetUsers();
		}

		public IQueryable<UserInfo> GetAllUsers(UserContext userLogin)
		{
			string file = string.Empty;
			int userLoginID = userLogin.ID;
			IQueryable<UserInfo> userList = DataService.Gets<UserInfo>(userLoginID, null, null, item =>
			new UserInfo
			{
				EmpID = item.UEmployeeID
				,FName = item.UFirstName
				,LName = item.ULastName
				,UID = item.UserID
				,PosName = item.tCMSWeb_UserPosition == null ? null : item.tCMSWeb_UserPosition.PositionName
				,Expired = item.UExpiredDate.HasValue ? DateTime.Compare(item.UExpiredDate.Value, DateTime.Now) < 0 ? true : false : false
				,ExDate = item.UExpiredDate
				,Email = item.UEmail
				,PosColor = item.tCMSWeb_UserPosition == null? BORDER_LEFT_COLOR_DEFAULT: item.tCMSWeb_UserPosition.Color.HasValue ? item.tCMSWeb_UserPosition.Color.Value: BORDER_LEFT_COLOR_DEFAULT
				,UPhoto = item.UPhoto
				,UserName= item.UUsername
			}
			);
			return userList;
			//List<UserInfo> data= new List<UserInfo>();

			//foreach (UserInfo value in userList)
			//{
			//	if (value.UPhoto != null && value.UPhoto != "")
			//	{
			//		file = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, value.UserName, value.UPhoto);
			//		if (File.Exists(file))
			//			value.ImageSrc = File.ReadAllBytes(file);
			//		else
			//			value.UPhoto = null;
			//	}
			//	data.Add(value);
			//}
			
			//return data;
		}

		public UserModel GetUserDetail(int userID)
		{
			string include = ServiceBase.ChildProperty(typeof(tCMSWeb_UserList), typeof(tCMSWeb_UserGroups));
			UserModel userModel = DataService.Get(userID, item =>
			new UserModel
			{
				 //get User Image
				 FName = item.UFirstName
				 ,LName = item.ULastName
				 ,Email = item.UEmail
				 ,UserName = item.UUsername
				 ,Password = string.Empty
				 ,EmailDaily = item.EmailDaily
				 ,IsAdmin = false
				 ,PositionID = item.PositionID
				 ,ExpiredDate = item.UExpiredDate
				 ,EmployeeID =item.UEmployeeID
				 ,Telephone = item.UTelephone
				 , UserID= item.UserID
				 , CreatedBy= item.CreatedBy
				 , CompanyID= item.CompanyID
				 ,Notes = item.UNote
				 , GroupID= item.tCMSWeb_UserGroups.Select(i=> i.GroupID).FirstOrDefault()
				 , UPhoto = item.UPhoto
			}, new string[]{include});

			//if (userModel.UPhoto != null && userModel.UPhoto != "")
			//{
			//	string file = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, userModel.UserName, userModel.UPhoto);
			//	if (FileManager.FileExist(file))
			//	{
			//		userModel.ImageSrc = FileManager.ReadFile(file);
			//	}
			//}

			List<int> sites = DataService.GetDvrbyUser<int>(userID, item => (int)item.siteKey).Distinct().ToList();
			userModel.SiteIDs = sites;
			return userModel;
		}

		public TransactionalModel<UserModel> AddUser(UserModel userModel, string SID)
		{
			TransactionalModel<UserModel> returnmodel = new TransactionalModel<UserModel>();
			returnmodel.ReturnStatus = true;
			byte[] bb = Cryptography.Utils.GetRandomBytes(32);
			string saltbase64 = Convert.ToBase64String(bb);
			userModel.UserName = OpenSSLSHA256.OpenSSLDecrypt(userModel.UserName, SID);
			if (!string.IsNullOrEmpty(userModel.Password))
			{
				userModel.Password = OpenSSLSHA256.OpenSSLDecrypt(userModel.Password, SID);
				userModel.Password = SHA.ComputeHash(userModel.Password, saltbase64);
			}

			CMSWebError userExisted = CheckUserExisted(userModel.UserName, userModel.EmployeeID, userModel.Email, userModel.UserID);
			if (userExisted != CMSWebError.DATA_NOT_FOUND)
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(userExisted.ToString());
				return returnmodel;
			}

			tCMSWeb_UserList dbuser = DataService.Get<tCMSWeb_UserList>(userModel.UserID, item => item);
			string old_avatar = null;
			dbuser = toDBModel(userModel, saltbase64, out old_avatar);

			if(!string.IsNullOrEmpty(userModel.UPhoto) && userModel.ImageSrc != null)
				dbuser.UPhoto =	FileManager.FileRandom() + FileManager.FileExtension( userModel.UPhoto);
			if( userModel.UserID == 0) //new user
			{
				return InsertUser(dbuser, userModel, old_avatar);
			}

			//update user
			return UpdateUser(dbuser, userModel, old_avatar);
		}

		public TransactionalModel<UserProfile> UpdateProfile(UserProfile userModel, string SID)
		{
			TransactionalModel<UserProfile> responseData = new TransactionalModel<UserProfile>();
			//check email existed.
			if (!string.IsNullOrEmpty(userModel.Email))
			{
				IQueryable<tCMSWeb_UserList> tcmswebUserList = ServiceBase.Query<tCMSWeb_UserList, tCMSWeb_UserList>(user => user.UEmail == userModel.Email, item => item, null);
				if (tcmswebUserList.FirstOrDefault(i => i.UserID != userModel.UserID) != null)
				{
					responseData.ReturnStatus = false;
					responseData.ReturnMessage.Add(CMSWebError.EMAIL_EXIST_MSG.ToString());
					return responseData;
				}
			}

			tCMSWeb_UserList dbuser = DataService.Get(userModel.UserID, item=> item);
			//userModel.UserName = OpenSSLSHA256.OpenSSLDecrypt(userModel.UserName, SID);
			string old_avatar = null, new_avatar = null;
			bool is_new_avatar = !string.IsNullOrEmpty(userModel.UPhoto) && userModel.ImageSrc != null && userModel.ImageSrc.Length > 0;
			if (is_new_avatar && !UpdateAvartar(userModel.ImageSrc, FileManager.FileExtension(userModel.UPhoto), userModel.UserID, out new_avatar))
				{
					responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return responseData;
				}

			if (!string.IsNullOrEmpty(dbuser.UPhoto) )
				old_avatar = AvartarPath( dbuser.UserID, dbuser.UPhoto);

			dbuser.ULastName = userModel.LName;
			dbuser.UFirstName = userModel.FName;
			dbuser.UEmail = userModel.Email;
			if(!string.IsNullOrEmpty( new_avatar))
				dbuser.UPhoto = FileManager.FileName(new_avatar);// userModel.UPhoto;
			//dbuser.UUsername = userModel.UserName;
			DataService.Update(dbuser);
			if (dbuser == null)
			{
				responseData.ReturnStatus = false;
				responseData.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				if ( is_new_avatar)//delete new avatar if cannot insert DB
					FileManager.FileDeleteAsync(new_avatar).Forget();
				return responseData;
			}
			if (!string.IsNullOrEmpty(old_avatar) && string.Compare(dbuser.UPhoto, FileManager.FileName(old_avatar), true) != 0)
				FileManager.FileDeleteAsync(old_avatar).Forget();
		
			userModel.ImageSrc = null;
			userModel.UPhoto = dbuser.UPhoto;

			responseData.ReturnStatus = true;
			responseData.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			responseData.Data = userModel;
			return responseData;
		}
		
		private bool SaveImageToFile(byte[] byteArray, string fileName, string username)
		{
			string pathFolder = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, username);
			if (!FileManager.DirExist(pathFolder))
			{
				FileManager.DirCreate(pathFolder);
				//FileManager.DirGrantAccess(pathFolder);
			}

			string pathFile = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, username, fileName);
			return FileManager.FileWrite(pathFile, byteArray, true);
		}

		private void DeleteUserImages(string username)
		{
			if (!string.IsNullOrEmpty(username))
			{
				string path = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, username);
				foreach (FileInfo file in FileManager.DirGetFileInfos(path))
				{
					FileManager.FileDeleteAsync(file.FullName);
				}
			}
		}

		public string ImageSrc(int UserID, string filename)
		{
            if (string.IsNullOrEmpty(filename))
            {
                var _filname = DataService.Get<tCMSWeb_UserList>(UserID, selector => selector);
                if (_filname != null)
                {
                    filename = _filname.UPhoto == null ?  Consts.Image_Default : _filname.UPhoto;
                }
            }
			string path = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, UserID.ToString(), filename);
            
			//UserInfo user = DataService.Get<UserInfo>(UserID, item => new UserInfo { UID = UserID, UserName = item.UUsername, UPhoto = item.UPhoto });

			if (!FileManager.FileExist(path))
			{
				path = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, Consts.Image_Default);
			}
			return path;
		}

		public TransactionalModel<UserModel> DeleteUser(List<int> userID, UserContext context)
		{
			TransactionalModel<UserModel> response = new TransactionalModel<UserModel>();
			string path = string.Empty;
			string[] includes = ServiceBase.ChildProperties(typeof (tCMSWeb_UserList)).ToArray();
			IQueryable<tCMSWeb_UserList> userlists = DataService.GetListUser(item => userID.Contains(item.UserID), includes);

			ReAssignReginsAndSitesToMaster(userID, context);
			if (SiteSvc.Save() < 0)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			foreach (tCMSWeb_UserList user in userlists.ToList())
			{
				DataService.Delete<tCMSWeb_DashBoardUsers>(x=>x.UserID == user.UserID, false);
			} //foreach

			if (!DataService.Delete<tCMSWeb_UserList>(userlists))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return response;
			}

			//delete folders contain users information wanting to delete
			List<Task<bool>> dels = new List<Task<bool>>();
			foreach (tCMSWeb_UserList user in userlists.ToList())
			{
				path = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, user.UserID.ToString());
				Task<bool> tdel = FileManager.DirDeleteAsync(path, true);
				dels.Add(tdel);
				//base.DeleteFolder(path);
			}
			Task.WhenAll(dels);

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return response;
		}

		private void ReAssignReginsAndSitesToMaster(List<int> userID, UserContext context)
		{
			AssignSitesToMaster(userID, context);

			AssignRegionsToMaster(userID, context);
		}

		private void AssignRegionsToMaster(List<int> userID, UserContext context)
		{
			var regionsCreatedByUser =
				SiteSvc.GetRegions<tCMSWebRegion>(t => t, null)
					.Where(t => t.UserKey != null && userID.Contains((int) t.UserKey))
					.ToList();
			regionsCreatedByUser.ForEach((r) =>
			{
				r.UserKey = context.ID;
				SiteSvc.UpdateRegion(r, false);
			});
		}

		private void AssignSitesToMaster(List<int> userID, UserContext context)
		{
			var sitesCreatedByUser =
				SiteSvc.GetSites<tCMSWebSites>(t => t.UserID != null && userID.Contains((int) t.UserID), t => t, null).ToList();

			sitesCreatedByUser.ForEach((s) =>
			{
				s.UserID = context.ID;
				SiteSvc.UpdateSite(s, false);
			});
		}

		private CMSWebError CheckUserExisted(string userName, int employeeID, string email, int userID)
		{
			IQueryable<tCMSWeb_UserList> tcmswebUserList;
			//check user name existed.
			if (!string.IsNullOrEmpty(userName))
			{
				tcmswebUserList = ServiceBase.Query<tCMSWeb_UserList, tCMSWeb_UserList>(user => user.UUsername.ToLower() == userName.ToLower(), item => item, null);
				if (userID == 0 && tcmswebUserList.Any())
				{
					return CMSWebError.USER_NAME_EXIST;
				}
				else if (tcmswebUserList.FirstOrDefault(i => i.UserID != userID) != null)
				{
					return CMSWebError.USER_NAME_EXIST;
				}
			}

			//check employee Id existed.
			if (employeeID != 0)
			{
				tcmswebUserList = ServiceBase.Query<tCMSWeb_UserList, tCMSWeb_UserList>(user => user.UEmployeeID == employeeID, item => item, null);
				if (userID == 0 && tcmswebUserList.Any())
				{
					return CMSWebError.USER_EMPLOYEE_EXIST;
				}
				else if (tcmswebUserList.FirstOrDefault(i => i.UserID != userID) != null)
				{
					return CMSWebError.USER_EMPLOYEE_EXIST;
				}
			}

			//check email existed.
			if (!string.IsNullOrEmpty(email))
			{
				tcmswebUserList = ServiceBase.Query<tCMSWeb_UserList, tCMSWeb_UserList>(user => user.UEmail == email, item => item, null);
				if (userID == 0 && tcmswebUserList.Any())
				{
					return CMSWebError.EMAIL_EXIST_MSG;
				}
				else if (tcmswebUserList.FirstOrDefault(i => i.UserID != userID) != null)
				{
					return CMSWebError.EMAIL_EXIST_MSG;
				}
			}

			return CMSWebError.DATA_NOT_FOUND;
		}

		private tCMSWeb_UserList toDBModel(UserModel model, string UID, out string current_avatar)
		{
			tCMSWeb_UserList dbmodel = null;
			current_avatar = null;
			if( model.UserID  == 0)
				dbmodel = new tCMSWeb_UserList();
			else
			{
				string[] u_groupinclude= new string[2];
				u_groupinclude[0]=ServiceBase.ChildProperty<tCMSWeb_UserList, tCMSWeb_UserGroups>();
				u_groupinclude[1]=ServiceBase.ChildProperty<tCMSWeb_UserList, tCMSWebSites>();
				dbmodel = DataService.Get<tCMSWeb_UserList>(model.UserID, item => item, u_groupinclude);
				if( dbmodel != null)
					current_avatar = AvartarPath(dbmodel.UserID, dbmodel.UPhoto);
			}
			if( model == null)
				return dbmodel;
			//dbmodel.UserID = model.UserID;
			dbmodel.UEmployeeID = model.EmployeeID;
			dbmodel.UUsername = model.UserName;
			dbmodel.UPassword = string.IsNullOrEmpty(model.Password)? dbmodel.UPassword: model.Password;
			dbmodel.UFirstName = model.FName;
			dbmodel.ULastName = model.LName;
			dbmodel.UEmail = model.Email;
			dbmodel.UTelephone = model.Telephone;
			dbmodel.PositionID = model.PositionID ==0? null: model.PositionID;
			dbmodel.UExpiredDate = model.ExpiredDate;
			dbmodel.UNote = model.Notes;
			dbmodel.EmailDaily = model.EmailDaily;
			dbmodel.isAdmin = model.IsAdmin;
			dbmodel.CompanyID = model.CompanyID;
			dbmodel.CreatedBy = model.CreatedBy;
			dbmodel.CreatedDate = model.CreatedDate;
			dbmodel.UPhoto = model.UPhoto;
			dbmodel.UID = string.IsNullOrEmpty(model.Password)? dbmodel.UID : UID;
			
			return dbmodel;
		}

        private tCMSWeb_UserList UpdateRelationshipOfUser(UserModel model, tCMSWeb_UserList dbmodel, bool isNew = true)
		{
			bool isModify = false;
			if (dbmodel.tCMSWeb_UserGroups == null)
				dbmodel.tCMSWeb_UserGroups = new List<tCMSWeb_UserGroups>();

			ICollection<tCMSWeb_UserGroups> old_groups = dbmodel.tCMSWeb_UserGroups;
			IQueryable<tCMSWeb_UserGroups> new_groups = UserGroupSvc.Gets<tCMSWeb_UserGroups>(it => it.GroupID == model.GroupID, it => it, null);
			IEnumerable<tCMSWeb_UserGroups> del_groups = old_groups.Where(it => it.GroupID != model.GroupID);
			IEnumerable<int> list_old_group = old_groups.Select(item => item.GroupID);
			new_groups = new_groups.Where(item => !list_old_group.Contains(item.GroupID));

			if (del_groups.Any() || new_groups.Any())
				isModify = true;
			if (del_groups.Any()) 
				DataService.Remove_Groups_fromUser(dbmodel, del_groups.ToArray());
			if (new_groups.Any())
				DataService.Add_Groups_toUser(dbmodel, new_groups.ToArray());


			if (dbmodel.tCMSWebSites == null)
				dbmodel.tCMSWebSites = new List<tCMSWebSites>();
			ICollection<tCMSWebSites> old_sites = dbmodel.tCMSWebSites;
			IQueryable<tCMSWebSites> new_sites = SiteSvc.GetSites<tCMSWebSites>(model.SiteIDs, item => item, null);
			IQueryable<int> list_new_sitekey= new_sites.Select(item => item.siteKey);
			IEnumerable<int> list_old_sitekey = old_sites.Select(item => item.siteKey);
			IEnumerable<tCMSWebSites> del_sites = old_sites.Where(item => !list_new_sitekey.Contains(item.siteKey));
			new_sites = new_sites.Where(item => !list_old_sitekey.Contains(item.siteKey));

			if (del_sites.Any() || new_sites.Any())
				isModify = true;
			if (del_sites.Any())
				DataService.Remove_Sites_fromUser(dbmodel, del_sites.ToArray());
			if(new_sites.Any())
				DataService.Add_Sites_toUser(dbmodel, new_sites.ToArray());

            if (isNew)
            {
                //if(dbmodel.tCMSWeb_UserGroups != null && dbmodel.tCMSWeb_UserGroups.)
                if (dbmodel.tbl_BAM_Metric_ReportUser == null || !dbmodel.tbl_BAM_Metric_ReportUser.Any())
                {
                    if (dbmodel.tbl_BAM_Metric_ReportUser == null)
                        dbmodel.tbl_BAM_Metric_ReportUser = new List<tbl_BAM_Metric_ReportUser>();

                    tbl_BAM_Metric traffic = BamMetricSvc.GetMetricDefaults().FirstOrDefault(x => x.MetricResourceName.Equals(METRIC_DEFAULT.METRIC_OPPORTUNITIES.ToString(), StringComparison.OrdinalIgnoreCase));
                    List<tbl_BAM_Metric_ReportList> rptDashboards = BamMetricSvc.GetReportLists().Where(x => x.MetricGroupID == Consts.BAM_RPT_DASHBOARD).ToList();

                    foreach (tbl_BAM_Metric_ReportList rpt in rptDashboards)
                    {
                        tbl_BAM_Metric_ReportUser defMetric = new tbl_BAM_Metric_ReportUser();
                        defMetric.Active = true;
                        defMetric.ReportID = rpt.ReportID;
                        //defMetric.tbl_BAM_Metric_ReportList = rpt;
                        defMetric.tCMSWeb_UserList = dbmodel;
                        defMetric.MetricID = traffic != null ? traffic.MetricID : (short)1;
                        //defMetric.tbl_BAM_Metric = traffic;
                        dbmodel.tbl_BAM_Metric_ReportUser.Add(defMetric);

                        if (!isModify)
                            isModify = true;
                    }
                }
            }
			
			return isModify ? DataService.Update(dbmodel): dbmodel;
		}
	
		public int GetMaxEmployeeID()
		{
			return DataService.GetMaxEmployeeID();
		}
		
		private string NewAvartarName(string extension)
		{
			string name = FileManager.FileRandom() + (extension == null ? string.Empty : extension);
			return name.ToUpper();
		}
		
		private string AvartarPath(int userid, string fname)
		{
			return string.IsNullOrEmpty(fname)? Path.Combine(AppSettings.AppSettings.Instance.UsersPath, userid.ToString()) :  Path.Combine(AppSettings.AppSettings.Instance.UsersPath, userid.ToString(), fname);
			

		}

		private bool UpdateAvartar(byte[]image, string extension, int userid, out string new_path)
		{
			new_path = null;
			if( image == null)
				return false;

			new_path = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, userid.ToString(), NewAvartarName(extension));
			if(FileManager.FileWrite( new_path, image, true))
				return true;
			new_path = null;
			return false;

		}
		
		private bool UpdateAvartar(byte [] image, string fname, int userid)
		{
			if (image == null)
				return false;

			string fpath = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, userid.ToString(), fname);
			return FileManager.FileWrite(fpath, image, true);

		}
		
		private TransactionalModel<UserModel> InsertUser(tCMSWeb_UserList dbUser, UserModel userModel, string oldUserImage)
		{
			TransactionalModel<UserModel> response = new TransactionalModel<UserModel>();
			dbUser = DataService.Add(dbUser);
			if (dbUser == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			UpdateAvartar(userModel.ImageSrc, dbUser.UPhoto, dbUser.UserID);
			if (!string.IsNullOrEmpty(oldUserImage))
				FileManager.FileDeleteAsync(oldUserImage).Forget();
			userModel.ImageSrc = null;
			userModel.UPhoto = dbUser.UPhoto;

			dbUser = UpdateRelationshipOfUser(userModel, dbUser);

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			userModel.UserID = dbUser.UserID;
			response.Data = userModel;
			return response;
		}

		private TransactionalModel<UserModel> UpdateUser(tCMSWeb_UserList dbUser, UserModel userModel, string oldUserImage)
		{
			TransactionalModel<UserModel> response = new TransactionalModel<UserModel>();
			dbUser = DataService.Update(dbUser);
			if (dbUser == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return response;
			}

			if (userModel.ImageSrc == null && string.IsNullOrEmpty(userModel.UPhoto))
			{
				//Remove user photo
				FileManager.FileDeleteAsync(oldUserImage).Forget();
			}
			else if (userModel.ImageSrc != null)
			{//change user photo
				UpdateAvartar(userModel.ImageSrc, dbUser.UPhoto, dbUser.UserID);
				FileManager.FileDeleteAsync(oldUserImage).Forget(); //remove user old photo
			userModel.ImageSrc = null;
			userModel.UPhoto = dbUser.UPhoto;
			}

			dbUser = UpdateRelationshipOfUser(userModel, dbUser, false);

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			response.Data = userModel;
			return response;
		}
	}
}
