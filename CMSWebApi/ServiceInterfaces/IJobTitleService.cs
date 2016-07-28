using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IJobTitleService
	{
		tCMSWeb_UserPosition Add(tCMSWeb_UserPosition jobTitle);
		tCMSWeb_UserPosition Update(tCMSWeb_UserPosition jobTitle);

		IQueryable<Tout> Gets<Tout>(int? createby, string [] includes, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout : class;
		bool Delete(List<int> listJobID);
		bool Delete(tCMSWeb_UserPosition jobPos);

		Tout Gets<Tout>(int ID, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout : class;
		IQueryable<Tout> Gets<Tout>(string jobName, int CreatedBy, Expression<Func<tCMSWeb_UserPosition, Tout>> selector) where Tout : class;
		//IQueryable<tCMSWeb_UserPosition> SelectAllJobTitle(int userID);
		
		//tCMSWeb_UserList SelectUser(int userID);
	}
}

