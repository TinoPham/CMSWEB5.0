using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using System.Linq.Expressions;


namespace CMSWebApi.DataServices
{
	public partial class GoalTypeService : ServiceBase, IGoalTypeService
	{
		public GoalTypeService(PACDMModel.Model.IResposity model) : base(model) { }

		public GoalTypeService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		#region Goals
		public tCMSWeb_Goals AddGoal(tCMSWeb_Goals goal)
		{
			DBModel.Insert<tCMSWeb_Goals>(goal);
			return DBModel.Save() > 0 ? goal : null;
		}

		public bool DeleteGoal(List<int> goalIDs)
		{
			DBModel.DeleteWhere<tCMSWeb_Goals>(id => goalIDs.Contains(id.GoalID));
			return DBModel.Save() > 0 ? true : false;
		}

		public tCMSWeb_Goals EditGoal(tCMSWeb_Goals goal)
		{
			DBModel.Update<tCMSWeb_Goals>(goal);
			return DBModel.Save() >= 0 ? goal : null;
		}

		public IQueryable<Tout> Gets<Tout>(string Name, Expression<Func<tCMSWeb_Goals, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> result = base.Query<tCMSWeb_Goals, Tout>( item =>item.GoalName.Equals(Name), selector, includes);
			return result;
		}
	
		public IQueryable<Tout> Gets<Tout>(int UserID, Expression<Func<tCMSWeb_Goals, Tout>> selector, string [] includes) where Tout : class
		{
			IQueryable<Tout> result = base.Query<tCMSWeb_Goals, Tout>(item => UserID == item.GoalCreateBy, selector, includes);
			return result;
		}
		
		public Tout Get<Tout>(int ID, Expression<Func<tCMSWeb_Goals, Tout>> selector, string [] includes) where Tout : class
		{
			Tout result = base.FirstOrDefault<tCMSWeb_Goals, Tout>(item => item.GoalID == ID, selector, includes);
			return result;
		}
		#endregion
		
		/// <summary>
		/// Insert Update Delete tCMSWeb_GoalType_Map
		/// </summary>

		public tCMSWeb_GoalType_Map AddGoalMap(tCMSWeb_GoalType_Map goalMap)
		{
			DBModel.Insert<tCMSWeb_GoalType_Map>(goalMap);
			return DBModel.Save() > 0 ? goalMap : null;
		}

		public bool DeleteGoalMap(List<int> goalIDs)
		{
			DBModel.DeleteWhere<tCMSWeb_GoalType_Map>(id => goalIDs.Contains(id.GoalID));
			return DBModel.Save() > 0 ? true : false;
		}

		public tCMSWeb_GoalType_Map EditGoalMap(tCMSWeb_GoalType_Map goalMap)
		{
			DBModel.Update<tCMSWeb_GoalType_Map>(goalMap);
			return DBModel.Save() >= 0 ? goalMap : null;
		}

		
		public IQueryable<Tout> GetGoalMaps<Tout>(Expression<Func<tCMSWeb_GoalType_Map, Tout>> selecttor, string [] include) where Tout : class
		{
			IQueryable<Tout> result = base.Query<tCMSWeb_GoalType_Map, Tout>( null, selecttor, include);
			return result;
		}

		public IQueryable<Tout> GetGoalTypes<Tout>(Expression<Func<tCMSWeb_Goal_Types, Tout>> selecttor, string [] include) where Tout : class
		{
			IQueryable<Tout> result = base.Query<tCMSWeb_Goal_Types, Tout>(null, selecttor, include);
			return result;
		}


	}
}
