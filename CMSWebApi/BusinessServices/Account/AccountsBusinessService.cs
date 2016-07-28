using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
//using CMSWebApi.Configurations;
using CMSWebApi.Email;
using CMSWebApi.Email.SMTP;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Globalization;
using PACDMModel.Model;
using System.IO;
using CMSWebApi.APIFilters;
using Cryptography;
using AppSettings;
using CMSWebApi.Utils;
using CMSWebApi.Configurations;
using System.Text.RegularExpressions;
using CMSWebApi.BusinessServices.Users;
using CMSWebApi.DataServices;

namespace CMSWebApi.BusinessServices.Account
{
	public partial class AccountsBusinessService : BusinessBase<IAccountService>
	{
		public ICompanyService ICompanyService { get; set; }
		public IUsersService User { get; set; }
		
		public UserModel GetUserByID(int userid)
		{
			if(userid == 0)
				return null;
			UsersBusinessService user_svr = new UsersBusinessService();
			user_svr.DataService = new CMSWebApi.DataServices.UsersService(base.ServiceBase);
			user_svr.BamMetricSvc = new CMSWebApi.DataServices.BamMetricService(base.ServiceBase);
			user_svr.UserGroupSvc = new CMSWebApi.DataServices.UserGroupService(base.ServiceBase);
			user_svr.SiteSvc = new SiteService(base.ServiceBase);
			return user_svr.GetUserDetail(userid);
			

		}
		public TransactionalModel<UserModel> Login(LoginModel loginmodel, bool encrypt = false)
		{
			TransactionalModel<UserModel> returnmodel = new TransactionalModel<UserModel>();
			try
			{
				string uid =  encrypt == false? loginmodel.UserName : OpenSSLSHA256.OpenSSLDecrypt(loginmodel.UserName, loginmodel.SID);
				string pwd = encrypt == false? loginmodel.Password : OpenSSLSHA256.OpenSSLDecrypt(loginmodel.Password, loginmodel.SID);
				loginmodel.Password = pwd;
				loginmodel.UserName = uid;

				UserModel model = new UserModel();
				AccountsBusinessRules Rules = new AccountsBusinessRules( Culture);
				Rules.ValidateLogin( uid, pwd);
				if(!Rules.ValidationStatus)
				{
					Rules.SetTransactionInfomation(returnmodel);
					returnmodel.IsAuthenicated = false;
					returnmodel.ReturnStatus = false;
					returnmodel.Data = model;
					return returnmodel;
				}
				returnmodel.ReturnStatus = false;
			
				tCMSWeb_UserList user = DataService.Login(uid, pwd);
				if( user == null)
					return returnmodel;
				loginmodel.Createdby = user.CreatedBy;
				loginmodel.CompanyID = (int)user.CompanyID;
				returnmodel.IsAuthenicated = true;
				UpdateUserInfo( ref model, user);
				model.Menus = GetMenu(DataService.Modules(user.tCMSWeb_UserGroups, user.isAdmin), user.isAdmin);
				returnmodel.Data = model;
				return returnmodel;
			}
			catch(Exception)
			{
				return null;
			}
		}

		protected T FromXml<T>(string path)
		{
			T returnedXmlClass = default(T);

			try
			{
				using (var reader = new FileStream(path, FileMode.Open))//using (TextReader reader = new StringReader(xml))
				{
					try
					{
						returnedXmlClass = (T)new XmlSerializer(typeof(T)).Deserialize(reader);
					}
					catch (InvalidOperationException)
					{
						
					}
				}
			}
			catch (Exception ex)
			{
			}

			return returnedXmlClass;
		}

		public async Task<bool> ResetPassword(ForgotPasswordModel lostpassModel)
		{
			var emailformatPath = Path.Combine(AppSettings.AppSettings.Instance.EmailForgotSettingPath, lostpassModel.LanguageKey + ".xml");
			EmailSetting emailformat = FromXml<EmailSetting>(emailformatPath);

			if (emailformat == null) return false;

			var subject = emailformat.Subject;
			var content = emailformat.Content;

			var dbbuser = User.GetListUser(t => t.UEmail.Equals(lostpassModel.Email,StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (dbbuser == null) return false;

			if (!dbbuser.UUsername.Equals(lostpassModel.UserName,StringComparison.OrdinalIgnoreCase)) return false;

			var newpass = Cryptography.Utils.GeneratePassword(emailformat.PasswordLength, emailformat.NumberNonOfAlpha);
            //newpass = Regex.Replace(newpass, @"[^a-zA-Z0-9]", m => "9");
			dbbuser.UPassword = SHA.ComputeHash(newpass, dbbuser.UID);

			var result = User.Update(dbbuser);
			if (result != null)
			{

				EmailSettingSection EmailSetting = AppSettings.AppSettings.Instance.EmailSetting;//ConfigurationManager.GetSection(EmailSettingSection.EmailSettingsSection_Name) as EmailSettingSection;
				EmailService email = CMSWebApi.Email.EmailService.Create(EmailSetting);
				SendMailResult resultSend = await email.SendMessage(dbbuser.UEmail, subject, string.Format(content, dbbuser.UUsername, dbbuser.UUsername, newpass), emailformat.IsHtml, null);
				email.Dispose();
				return true;
				

			}
			return false;
		}

		public TransactionalModel<ChangePasswordModel> ChangePassword(UserContext context, ChangePasswordModel passwordModel,
			string SID)
		{
			var returnmodel = new TransactionalModel<ChangePasswordModel> { ReturnStatus = true };

			var dbbuser = User.Get<tCMSWeb_UserList>(context.ID, item => item);
			if (dbbuser == null)
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(CMSWebError.DATA_NOT_FOUND.ToString());
				return returnmodel;
			}

			//byte[] bb = Cryptography.Utils.GetRandomBytes(32);
			string saltbase64 = dbbuser.UID;

			var currentpasswordDecrypt = OpenSSLSHA256.OpenSSLDecrypt(passwordModel.CurrentPassword, SID);
			var currentpassword = SHA.ComputeHash(currentpasswordDecrypt, saltbase64);
			if (dbbuser.UPassword != currentpassword)
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(CMSWebError.PASSWORD_INCORRECT_MSG.ToString());
				return returnmodel;
			}

			var newpassword = OpenSSLSHA256.OpenSSLDecrypt(passwordModel.NewPassword, SID);
			dbbuser.UPassword = SHA.ComputeHash(newpassword, saltbase64);

			var result = User.Update(dbbuser);

			if (result == null)
			{
				returnmodel.ReturnStatus = false;
				returnmodel.ReturnMessage.Add(CMSWebError.SERVER_ERROR_MSG.ToString());
				return returnmodel;
			}

			returnmodel.Data = passwordModel;

			return returnmodel;
		}

		//public IQueryable<UserModel> GetUsers()
		//{
		//	return DataService.GetUsers();
		//}

		private void UpdateUserInfo(ref UserModel model, tCMSWeb_UserList user)
		{
			if( model == null)
				model = new UserModel();
			model.Accepted = user.Accepted.HasValue? (user.Accepted.Value > 0? true : false) : false;
			model.CompanyID = user.CompanyID;
			model.FName = user.UFirstName;
			model.LName = user.ULastName;
			model.Email = user.UEmail;
			model.PositionID = user.PositionID;
			model.IsAdmin = user.isAdmin;
			model.UserID = user.UserID;
			model.Functions = DataService.Functions(user.tCMSWeb_UserGroups, user.isAdmin).Select(item => new CMSWebFunctionModel { FunctionID = item.FunctionID, ModuleID = item.ModuleID, FunctionName = item.FunctionName });
			model.Levels = DataService.Levels(user.tCMSWeb_UserGroups, user.isAdmin).Select(item => new CMSWebLevelModel { LevelID = item.LevelID, LevelName = item.LevelName });
			model.Settings = GetAppSettings();
			model.UPhoto = user.UPhoto;
			model.UserName = user.UUsername;
            model.CreatedBy = user.CreatedBy;
			model.IdleTimeout = AppSettings.AppSettings.Instance.IdleTimeout;
			model.isExpired = user.UExpiredDate.HasValue ? DateTime.Compare(user.UExpiredDate.Value, DateTime.Now) < 0 ? true : false : false;
			if (!string.IsNullOrEmpty(model.UPhoto))
			{
				string file = Path.Combine(AppSettings.AppSettings.Instance.UsersPath, user.UUsername, model.UPhoto);
				if (FileManager.FileExist(file))
				{
					model.ImageSrc = FileManager.ReadFile(file);
				}
			}

			tCMSSystemConfig sysConfig = (user.CompanyID.HasValue && user.CompanyID.Value > 0) ? ICompanyService.SelectRecordingDay(user.CompanyID.Value) : null;
			model.RecordingDays = (sysConfig != null) ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;
		}

		private AppSetting GetAppSettings()
		{
			AppSetting app = new AppSetting();
			app.CompImgSize = AppSettings.AppSettings.Instance.SnapShotSize;
			app.UserImgSize = AppSettings.AppSettings.Instance.UserImageSize;
			app.EncryptMode = AppSettings.AppSettings.Instance.MessageEncrypt;
			app.ImageUrl = AppSettings.AppSettings.Instance.UsersPath; //Consts.Path_Image + "/" ;
			return app;
		}


		private List<ApplicationMenu> GetMenu(IQueryable<tCMSWeb_Modules> modules, bool isMaster)
		{
			List<ApplicationMenu> menus = new List<ApplicationMenu>();
			ApplicationMenu menu = null;
			IEnumerable<tCMSWeb_Modules> omodules = modules.OrderBy( it => it.ModuleOrder);
			foreach (var item in omodules)
			{
				menu = RouteData.Instance.GetRoute(item.ModuleName, isMaster);
				//if( menu == null)
				if( menu == null || !IsValidLicense( item.ModuleName)  )
					continue;
				menu.ID = item.ModuleID;
				menus.Add(menu);
			}
			return menus;
		}
		private bool IsValidLicense( string modulename)
		{
#if License
			if( string.IsNullOrEmpty( modulename))
				return false;
			LicenseInfo.Models.LicenseModel licens = AppSettings.AppSettings.Instance.Licenseinfo;
			LicenseInfo.Models.CMSWebModule mdule = licens.CMSWebModules.FirstOrDefault(it => string.Compare(it.Name, modulename, true) == 0);
			if( mdule == null || mdule.Enable == false)
				return false;
			DateTime utcnow = DateTime.UtcNow.Date;
			return mdule.From.Date <= utcnow && utcnow <= mdule.To.Date; 
#else
		return true;
#endif
		}

	}
}
