using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.GoalType;
using PACDMModel;
using System.Net.Http;
using System.Net;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using CMSWebApi.APIFilters;
using System.Net.Http.Headers;
using System.Web.Security;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.Controllers
{
	[RoutePrefix("goaltype")]
	[WebApiAuthenication]
	public class GoalTypeController : ApiControllerBase<IGoalTypeService, GoalTypeBusinessService>
	{
		[HttpGet]
		public HttpResponseMessage GoalTypes()
		{
			IQueryable<GoalTypeModel> goalTypes = base.BusinessService.GetAllGoalType();
			return ResponseData<IQueryable<GoalTypeModel>>( goalTypes);
		}

		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage Goals( bool? detail = null)
		{
			if( detail.HasValue && detail.Value == true)
			{
				IEnumerable<GoalModel> goals = base.BusinessService.GetAllGoal(base.usercontext);
				return ResponseData<IEnumerable<GoalModel>>( goals);
			}
			else
			{
				return ExecuteBusiness<IQueryable<GoalSimple>>(() =>
				{
					IQueryable<GoalSimple> goals = base.BusinessService.GetGoals(base.usercontext);
					return goals;
				});
			}
		}

		//[HttpGet]
		//public HttpResponseMessage GetGoals()
		//{
		//	return ExecuteBusiness<IQueryable<GoalSimple>>(() =>
		//	{
		//		IQueryable<GoalSimple> goals = base.BusinessService.GetGoals(base.usercontext);
		//		return goals;
		//	});
		//}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteGoalType([FromBody] List<int> goalIDs)
		{
			TransactionalModel<GoalModel> result = base.BusinessService.DeleteGoal(goalIDs);
			return ResponseData<TransactionalModel<GoalModel>>(result);
			//return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage AddGoalType(GoalModel goalType)
		{
			TransactionalModel<GoalModel> goal = base.BusinessService.UpdateGoal(goalType);
			return ResponseData<TransactionalModel<GoalModel>>(goal);
			//return Request.CreateResponse(HttpStatusCode.OK, goal);
		}
		
	}
}
