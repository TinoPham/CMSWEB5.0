using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Linq;

namespace CMSWebApi.DataServices
{
	public class DashboardService : ServiceBase, IDashboardService
	{
		public DashboardService(IResposity model) : base(model)
		{
		}

		public DashboardService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWeb_DashBoardUsers GetDashbaoBoardUsers(int userId)
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoardLayouts).Name, 
				typeof (tCMSWeb_DashBoardStyles).Name,
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name),
				string.Format("{0}.{1}.{2}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name,typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};

			return DBModel.FirstOrDefault<tCMSWeb_DashBoardUsers>(t => t.UserID == userId, includes);
		}
		public tCMSWeb_DashBoardUserLevels GetDashbaoBoardUserLevel(int levelId)
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoardLayouts).Name, 
				typeof (tCMSWeb_DashBoardStyles).Name,
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name),
				string.Format("{0}.{1}.{2}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name,typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};

			return DBModel.FirstOrDefault<tCMSWeb_DashBoardUserLevels>(t => t.LevelID == levelId, includes);
		}

		public Tout GetDashbaoBoard<Tentity, Tout>(int levelId, Expression<Func<Tentity, bool>> filter, Func<Tentity, Tout> selecttor) where Tentity : class
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoardLayouts).Name, 
				typeof (tCMSWeb_DashBoardStyles).Name,
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name),
				string.Format("{0}.{1}.{2}", typeof (tCMSWeb_DashBoardLayouts).Name, typeof (tCMSWeb_DashBoard_WidgetPositions).Name,typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};

			Tentity en = DBModel.FirstOrDefault<Tentity>(filter, includes);

			return en == null ? default(Tout) : selecttor.Invoke(en); 

		}

		public IQueryable<T> GetElementUsers<T>(int userId, Expression<Func<tCMSWeb_DashBoard_User_Element, T>> selector)
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoardElements).Name, 
				typeof (tCMSWeb_DashBoardStyles).Name,
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoardElements).Name, typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};
			return DBModel.Query<tCMSWeb_DashBoard_User_Element>(t => t.UserID == userId, includes).Select(selector);
		}

		public IQueryable<T> GetElementUserLevels<T>(int levelId, Expression<Func<tCMSWeb_DashBoard_UserLevel_Element, T>> selector)
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoardElements).Name, 
				typeof (tCMSWeb_DashBoardStyles).Name,
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoardElements).Name, typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};
			return DBModel.Query<tCMSWeb_DashBoard_UserLevel_Element>(t => t.LevelID == levelId, includes).Select(selector);
		}

		public IQueryable<T> GetWidgetGroups<T>(Expression<Func<tCMSWeb_DashBoard_WidgetGroupSize, T>> selector)
		{
			return Query<tCMSWeb_DashBoard_WidgetGroupSize, T>(null, selector);
		}

		public IQueryable<tCMSWeb_DashBoardLayouts> GetDashboards()
		{
			var includes = new string[]
			{
				typeof (tCMSWeb_DashBoard_WidgetPositions).Name, 
				string.Format("{0}.{1}", typeof (tCMSWeb_DashBoard_WidgetPositions).Name, typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name)
			};
			return DBModel.Query<tCMSWeb_DashBoardLayouts>(null, includes);
		}

		public IQueryable<T> GetElemements<T>(Expression<Func<tCMSWeb_DashBoardElements, T>> selector)
		{
			var include = new string[] {typeof (tCMSWeb_DashBoard_WidgetGroupSize).Name};
			return DBModel.Query<tCMSWeb_DashBoardElements>(null, include).Select(selector);
		}

		public IQueryable<T> GetDashBoardStyles<T>(int styleId, Expression<Func<tCMSWeb_DashBoardStyles, T>> selector)
		{
			return Query<tCMSWeb_DashBoardStyles, T>(t => t.StyleID == styleId, selector);
		}

		public void InsertElement(tCMSWeb_DashBoard_User_Element element)
		{
			DBModel.Insert<tCMSWeb_DashBoard_User_Element>(element);
		}

		public void DeleteElements(int userId)
		{
			DBModel.DeleteWhere<tCMSWeb_DashBoard_User_Element>(t => t.UserID == userId);
		}

		public void InsertDashboard(tCMSWeb_DashBoardUsers dashUser)
		{
			DBModel.Insert<tCMSWeb_DashBoardUsers>(dashUser);
		}

		public void EditDashboard(tCMSWeb_DashBoardUsers dashUser)
		{
			DBModel.Update<tCMSWeb_DashBoardUsers>(dashUser);
		}

		public void DeleteDashboard(tCMSWeb_DashBoardUsers dashUser)
		{
			DBModel.DeleteWhere<tCMSWeb_DashBoardUsers>(t=>t.UserID == dashUser.UserID);
		}

		public void SaveDashboard()
		{
			DBModel.Save();
		}
	}
}
