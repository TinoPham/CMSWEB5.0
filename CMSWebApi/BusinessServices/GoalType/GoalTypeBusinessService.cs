using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Globalization;
using PACDMModel.Model;
using System.IO;
using CMSWebApi.APIFilters;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.GoalType
{
	public class GoalTypeBusinessService: BusinessBase<IGoalTypeService>
	{
		public IUsersService IUser{ get ;set;}
		public ISiteService ISite { get; set;}
		Expression<Func<tCMSWeb_Goal_Types, GoalTypeModel>> Selector_GoalTypeModel = item => new GoalTypeModel { GoalTypeID = item.GoalTypeID, GoalTypeName = item.GoalTypeName };
		Expression<Func<tCMSWeb_Goals, GoalSimple>> Selector_GoalModel = item => new GoalSimple { GoalID = item.GoalID, GoalName = item.GoalName };

		public IQueryable<GoalTypeModel> GetAllGoalType()
		{
			IQueryable<GoalTypeModel> models = DataService.GetGoalTypes<GoalTypeModel>( Selector_GoalTypeModel, null);
			return models;

			//IQueryable<tCMSWeb_Goal_Types> goalTypes = DataService.GetAllGoalType();
			//GoalTypeModel[] typeModel= (from item in goalTypes select new GoalTypeModel{GoalTypeID=item.GoalTypeID, GoalTypeName= item.GoalTypeName}).ToArray();
			//return typeModel;
		}

		public IQueryable<GoalSimple> GetGoals(UserContext userValue)
		{
			int userId = userValue.Createdby.HasValue? IUser.GetMasterID(userValue.CompanyID) : userValue.ID;
			IQueryable<GoalSimple> models = DataService.Gets<GoalSimple>(userId, Selector_GoalModel, null);
			return models;
		}

		public IEnumerable<GoalModel> GetAllGoal(UserContext userValue)
		{
			int userID = userValue.ID;
			IQueryable<tCMSWeb_Goals> tGoal = DataService.Gets<tCMSWeb_Goals>(userID, item=> item, null); //.SelectAllGoal(userID);
			IQueryable<tCMSWeb_Goal_Types> tGoalType = DataService.GetGoalTypes<tCMSWeb_Goal_Types>( item=> item, null); //.GetAllGoalType();
			IQueryable<tCMSWeb_GoalType_Map> tGoalMap = DataService.GetGoalMaps<tCMSWeb_GoalType_Map>( item=> item, null);//.SelectGoalMap();
			//string tUser = IUser.Get<string>(userID, item => string.Format( "{0} {1}", item.UFirstName, item.ULastName));//  DataService.SelectUser(userID);

			IEnumerable<GoalModel> goalModel = (from value in tGoal//.ToArray()
										 select new GoalModel
										 {
											 GoalID = value.GoalID,
											 GoalName = value.GoalName,
											 GoalCreateBy = value.GoalCreateBy,
											 GoalLastUpdated = value.GoalLastUpdated,
											 UUsername = userValue.Name,
											 MapValue = (from map in tGoalMap//.ToArray()
														 join type in tGoalType//.ToArray() 
															on map.GoalTypeID equals type.GoalTypeID
														 where map.GoalID == value.GoalID
														 select new GoalMap
														 {
															 GoalID = map.GoalID,
															 GoalTypeID = map.GoalTypeID,
															 GoalTypeName = type.GoalTypeName,
															 MinValue = map.MinValue,
															 MaxValue = map.MaxValue
														 })//.ToArray()
										 });//.ToArray();


			return goalModel;
		}

		public TransactionalModel<GoalModel> DeleteGoal(List<int> goalIDs)
		{
			TransactionalModel<GoalModel> model = new TransactionalModel<GoalModel>();
			string goalNameUsed = string.Empty;
			foreach (int id in goalIDs)
			{
				var goalName = CheckGoalUsed(id);
				if (!string.IsNullOrEmpty(goalName))
				{
					goalNameUsed += CheckGoalUsed(id) + ",";
				}
			}

			if (!string.IsNullOrEmpty(goalNameUsed))
			{ 
				goalNameUsed = goalNameUsed.Substring(0, goalNameUsed.Length - 1);
				model.ReturnStatus = false;
				model.ReturnMessage.Add(CMSWebError.GOALTYPE_IS_USED.ToString());
				model.ReturnMessage.Add(goalNameUsed);
				return model;
			}

			if (DataService.DeleteGoalMap(goalIDs))
			{
				if (DataService.DeleteGoal(goalIDs))
				{
					model.ReturnStatus = true;
					model.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
					return model;
				}
			}
			else if (DataService.DeleteGoal(goalIDs))
			{
				model.ReturnStatus = true;
				model.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
				return model;
			}

			model.ReturnStatus = false;
			model.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
			return model;
		}

		private void SetEntityGoalMap(ref tCMSWeb_GoalType_Map tGoalMap, GoalMap goalMap)
		{
			if (goalMap == null)
				goalMap = new GoalMap();

			tGoalMap.GoalID = goalMap.GoalID;
			tGoalMap.GoalTypeID = goalMap.GoalTypeID;
			tGoalMap.MaxValue = goalMap.MaxValue;
			tGoalMap.MinValue = goalMap.MinValue;
		}

		private void SetEntityGoal(ref tCMSWeb_Goals tGoal, GoalModel model)
		{
			if (model == null)
				model = new GoalModel();

			tGoal.GoalID = model.GoalID;
			tGoal.GoalName = model.GoalName;
			tGoal.GoalCreateBy = model.GoalCreateBy;
			tGoal.GoalLastUpdated = model.GoalLastUpdated;
		}

		public TransactionalModel<GoalModel> UpdateGoal(GoalModel model)
		{
			TransactionalModel<GoalModel> response = new TransactionalModel<GoalModel>();
			GoalTypeBusinessRules Rules = new GoalTypeBusinessRules(Culture);
			Rules.ValidateInput(model.GoalName);
			if (!Rules.ValidationStatus)
			{
				Rules.SetTransactionInfomation(response);
				return response;
			}

			if (CheckRegistExist(model.GoalName, model.GoalID))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.GOALTYPE_NAME_EXIST.ToString());
				response.Data = model;
				return response;
			}

			tCMSWeb_Goals tGoal = new tCMSWeb_Goals();
			if (model.GoalID != 0) //Update Goal Type
			{
				tGoal = DataService.Get<tCMSWeb_Goals>(model.GoalID, item => item, null);//.SelectGoal(model.GoalID);
				SetEntityGoal(ref tGoal, model);
				if (DataService.EditGoal(tGoal) == null)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					return response;
				}

				if (!UpdateGoalTypeMap(model.MapValue.ToArray(), tGoal.GoalID))
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
					return response;
				}

				response.ReturnStatus = true;
				response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
				response.Data = model;
				return response;
			}
			
			//Add Goal Type
			SetEntityGoal(ref tGoal, model);
			if (DataService.AddGoal(tGoal) == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			if (!UpdateGoalTypeMap(model.MapValue.ToArray(), tGoal.GoalID))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			model.GoalID = tGoal.GoalID;
			response.Data = model;
			return response;
		}

		private bool UpdateGoalTypeMap(GoalMap[] goalMaps, int goalID)
		{
			List<int> goalIDs = new List<int>();
			goalIDs.Add(goalID);
			DataService.DeleteGoalMap(goalIDs);
			
			foreach (GoalMap item in goalMaps)
			{
				item.GoalID = goalID;
				tCMSWeb_GoalType_Map tGoalMap = new tCMSWeb_GoalType_Map();
				SetEntityGoalMap(ref tGoalMap, item);
				if (DataService.AddGoalMap(tGoalMap) == null)
					return false;
			}
			return true;
		}
		
		private bool CheckRegistExist(string goalName, int goalID)
		{
			IQueryable<tCMSWeb_Goals> goals = DataService.Gets<tCMSWeb_Goals>(goalName, item => item, null);
			if(goals == null)
				return false;
			if (goalID == 0)
			{
				//Insert Case
				return goals.Any();
			}
			else
			{
				//Update Case
				return goals.Where(w => w.GoalID != goalID).Any();
			}
		}

		private string CheckGoalUsed(int GoalId)
		{
			string[] include = ServiceBase.ChildProperties(typeof(tCMSWeb_Goals)).ToArray();
			tCMSWeb_Goals tGoalDB = DataService.Get<tCMSWeb_Goals>(GoalId, item => item, include);
			if (tGoalDB != null)
			{
				if (tGoalDB.tCMSWebSites != null && tGoalDB.tCMSWebSites.Count() > 0)
				{
					return tGoalDB.GoalName;
				}
			}
			return string.Empty;
		}
	}
}
