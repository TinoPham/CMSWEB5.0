using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	
    public interface IAccountService
    {
		tCMSWeb_UserList Login(string username, string password);
	    //IQueryable<UserModel> GetUsers();
		tCMSWeb_UserList Edit(tCMSWeb_UserList user);
		tCMSWeb_UserList Add(tCMSWeb_UserList user);
		bool Delete( int userID);
		IQueryable<tCMSWeb_Modules> Modules(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false);
		IQueryable<tCMSWeb_Functions> Functions(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false);
		IQueryable<tCMSWeb_Levels> Levels(ICollection<tCMSWeb_UserGroups> ugroup, bool admin = false);		
    }
}
