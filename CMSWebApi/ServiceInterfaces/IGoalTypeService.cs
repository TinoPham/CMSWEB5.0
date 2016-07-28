using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IGoalTypeService
	{
		#region Goals

		tCMSWeb_Goals EditGoal(tCMSWeb_Goals goal);

		tCMSWeb_Goals AddGoal(tCMSWeb_Goals goal);

		bool DeleteGoal(List<int> goalIDs);

		IQueryable<Tout> Gets<Tout>(string Name, Expression<Func<tCMSWeb_Goals, Tout>> selector, string[] includes) where Tout : class;

		IQueryable<Tout> Gets<Tout>(int UserID, Expression<Func<tCMSWeb_Goals, Tout>> selector, string [] includes) where Tout : class;

		Tout Get<Tout>(int ID, Expression<Func<tCMSWeb_Goals, Tout>> selector, string [] includes) where Tout : class;

		#endregion
		#region Gola Maps
		tCMSWeb_GoalType_Map EditGoalMap(tCMSWeb_GoalType_Map goalMap);

		tCMSWeb_GoalType_Map AddGoalMap(tCMSWeb_GoalType_Map goalMap);

		bool DeleteGoalMap(List<int> goalIDs);

		IQueryable<Tout> GetGoalMaps<Tout>( Expression<Func<tCMSWeb_GoalType_Map, Tout>> selecttor, string[]include ) where Tout : class;

		IQueryable<Tout> GetGoalTypes<Tout>(Expression<Func<tCMSWeb_Goal_Types, Tout>> selecttor, string [] include) where Tout : class;
		#endregion

	}
}

