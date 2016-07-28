using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.Utils;
using System.IO;

namespace CMSWebApi.DataServices
{
	public partial class AccountService : ServiceBase, IAccountService
	{
		private IQueryable<tCMSWeb_Function_Level> Fucntion_Levels{ get{ return DBModel.Query<tCMSWeb_Function_Level>();}}

		public AccountService(PACDMModel.Model.IResposity model) : base(model) { }

		public AccountService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWeb_UserList Login(string username, string password)
		{

			IQueryable<tCMSWeb_UserList> users = DBModel.Query<tCMSWeb_UserList>(user => string.Compare(user.UUsername, username, false) == 0);
			tCMSWeb_UserList model = null;
			foreach(tCMSWeb_UserList user in users)
			{	if( ComparePassword(user, password))
				{
					model = user;
					break;
				}
			}

			if (model != null)
				DBModel.Include<tCMSWeb_UserList, tCMSWeb_UserGroups>(model , item => item.tCMSWeb_UserGroups);
			return model;
		}

		//public IQueryable<UserModel> GetUsers()
		//{
		//	var usertitlte = DBModel.Query<tCMSWeb_UserPosition>();

		//	var result = DBModel.Query<tCMSWeb_UserList>()
		//		.GroupJoin(usertitlte, t => t.PositionID, p => p.PositionID, (t, p) => new {T = t, P = p})

		//		.SelectMany(x => x.P.DefaultIfEmpty(),
		//		  (t, p) => new UserModel
		//		  {
		//			  UserID = t.T.UserID,
		//			  Email = t.T.UEmail,
		//			  EmployeeID = t.T.UEmployeeID,
		//			  FName = t.T.UFirstName,
		//			  LName = t.T.ULastName,
		//			  PositionID = p.PositionID,
		//			  PositionName = p.PositionName,
		//			  UPhoto = t.T.UPhoto== null? t.T.UPhoto : Consts.Path_Image + "/" + t.T.UUsername + "/" + t.T.UPhoto
		//		  }
		//		);

		//	return result;
		//	//return id != null ? DBModel.Query<tCMSWeb_UserList>(t => t.GroupID == id) : DBModel.Query<tCMSWeb_UserList>();
		//}

		public tCMSWeb_UserList Edit(tCMSWeb_UserList user)
		{
			DBModel.Update<tCMSWeb_UserList>(user);
			return DBModel.Save() >= 0 ? user : null;
		}

		public tCMSWeb_UserList Add(tCMSWeb_UserList user)
		{
			DBModel.Insert<tCMSWeb_UserList>(user);
			return DBModel.Save() > 0? user : null;
		}

		public bool Delete(int userID)
		{
			DBModel.DeleteWhere<tCMSWeb_UserList>(user => user.UserID == userID);
			return DBModel.Save()> 0? true : false;
		}

		public IQueryable<tCMSWeb_Modules> Modules(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false)
		{
			IQueryable<tCMSWeb_Modules> modules = DBModel.Query<tCMSWeb_Modules>();
			if(admin)
				return modules;
			IQueryable<tCMSWeb_Functions> functions = Functions(ugroup, admin);
			return modules.Join( functions, m => m.ModuleID, f=> f.ModuleID, (m,f) => m ).Distinct();
		}

		public IQueryable<tCMSWeb_Functions> Functions(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false)
		{
			IQueryable<tCMSWeb_Functions> fucntions = DBModel.Query<tCMSWeb_Functions>(it => it.Displayed == true);
			if (admin)
				return fucntions;
			LoadUsergroupFunctions(ugroup);
			List<tCMSWeb_Function_Level> Ufunctionlevel = ugroup.SelectMany(item => item.tCMSWeb_Function_Level).ToList();
			List<int> func_levels = Fucntion_Levels.ToList().Join(Ufunctionlevel, funclevel => funclevel.FunclevelID, ufunc => ufunc.FunclevelID, (func_level, gfunc) => func_level.FunctionID).Distinct().ToList();
			return fucntions.Where(it => func_levels.Contains(it.FunctionID));

		}

		public IQueryable<tCMSWeb_Levels> Levels(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false)
		{
			IQueryable<tCMSWeb_Levels> levels = base.DBModel.Query<tCMSWeb_Levels>();
			if (admin)
				return levels;

			LoadUsergroupFunctions(ugroup);
			List<tCMSWeb_Function_Level> Ufunctionlevel = ugroup.SelectMany(item => item.tCMSWeb_Function_Level).ToList();

			List<int?> func_levels = Fucntion_Levels.ToList().Join(Ufunctionlevel, funclevel => funclevel.FunclevelID, ufunc => ufunc.FunclevelID, (func_level, gfunc) => func_level.LevelID).Distinct().ToList();

			return levels.Where(it => func_levels.Contains(it.LevelID));
		}

		private void LoadUsergroupFunctions(ICollection<tCMSWeb_UserGroups> ugroup)
		{
			if (ugroup == null)
				return;
			foreach (tCMSWeb_UserGroups value in ugroup)
			{
				DBModel.Include<tCMSWeb_UserGroups, tCMSWeb_Function_Level>(value, item => item.tCMSWeb_Function_Level);
			}
		}

		private bool ComparePassword( tCMSWeb_UserList user, string pass)
		{
			if (string.IsNullOrEmpty(pass) || user == null || string.IsNullOrEmpty(user.UID))
				return false;
				byte[] salt = Convert.FromBase64String( user.UID);
				byte[] plainpass = Encoding.UTF8.GetBytes(pass);
				byte [] passwithsalt = Convert.FromBase64String(user.UPassword);
				return Cryptography.SHA.VerifyHash(plainpass, salt, passwithsalt);
		}

    }
}
