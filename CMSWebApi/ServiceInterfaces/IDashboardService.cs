using System;
using System.Linq;
using System.Linq.Expressions;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IDashboardService
	{
		tCMSWeb_DashBoardUsers GetDashbaoBoardUsers(int userId);
		tCMSWeb_DashBoardUserLevels GetDashbaoBoardUserLevel(int levelId);
		Tout GetDashbaoBoard<Tentity, Tout>(int levelId, Expression<Func<Tentity, bool>> filter, Func<Tentity, Tout> selecttor) where Tentity : class;//

		IQueryable<T> GetElementUsers<T>(int userId, Expression<Func<tCMSWeb_DashBoard_User_Element, T>> selector);
		IQueryable<T> GetElementUserLevels<T>(int levelId, Expression<Func<tCMSWeb_DashBoard_UserLevel_Element, T>> selector);

		IQueryable<T> GetWidgetGroups<T>(Expression<Func<tCMSWeb_DashBoard_WidgetGroupSize, T>> selector);
		IQueryable<tCMSWeb_DashBoardLayouts> GetDashboards();
		IQueryable<T> GetElemements<T>(Expression<Func<tCMSWeb_DashBoardElements, T>> selector);
		IQueryable<T> GetDashBoardStyles<T>(int styleId, Expression<Func<tCMSWeb_DashBoardStyles, T>> selector);
		void InsertElement(tCMSWeb_DashBoard_User_Element element);
		void DeleteElements(int userId);
		void InsertDashboard(tCMSWeb_DashBoardUsers dashUser);
		void EditDashboard(tCMSWeb_DashBoardUsers dashUser);
		void DeleteDashboard(tCMSWeb_DashBoardUsers dashUser);
		void SaveDashboard();
	}
}
