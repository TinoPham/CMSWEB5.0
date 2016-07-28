using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface ISynUserService
	{
		tCMSWeb_SynUser AddSynUser(tCMSWeb_SynUser SynUser);
		tCMSWeb_SynUser UpdateSynUser(tCMSWeb_SynUser SynUser);
		tCMSWeb_SynUser SelectSynUser(int jobID);
		bool DeleteSynUser(int SynID);

		IQueryable<Tout> SelectSynUser<Tout>(string ServerIP, int SynType, Expression<Func<tCMSWeb_SynUser, Tout>> selector, string [] includes) where Tout : class;
		//IQueryable<tCMSWeb_SynUser> SelectSynUser(string ServerIP, int SynType);
		//IQueryable<tCMSWeb_SynUser> SelectAllSynUser(int userID);
		IQueryable<Tout> SelectAllSynUser<Tout>(int userID, Expression<Func<tCMSWeb_SynUser, Tout>> selector, string [] includes) where Tout : class;

		IQueryable<Tout> SelectSynUserType<Tout>(Expression<Func<tCMSWeb_SynUser_Types, Tout>> selector, string [] includes) where Tout : class;
		//IQueryable<tCMSWeb_SynUser_Types> SelectSynUserType();
		//tCMSWeb_UserList SelectUser(int userID);
	}
}

