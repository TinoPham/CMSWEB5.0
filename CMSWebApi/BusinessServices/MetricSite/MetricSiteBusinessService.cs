using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataModels;
using System.Globalization;
using PACDMModel.Model;
using System.IO;
using CMSWebApi.APIFilters;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.MetricSite
{
	public class MetricSiteBusinessService : BusinessBase<IMetricSiteService>
	{
		public IUsersService IUser { get; set; }
		public IEnumerable<MetricModel> GetAllMetric(UserContext userLogin)
		{
			int userID = userLogin.Createdby.HasValue ? IUser.GetMasterID(userLogin.CompanyID) : userLogin.ID;
			//string userFullName = IUser.Get<string>(userLogin.ID, item => item == null ? string.Empty : string.Format("{0} {1}", item.UFirstName, item.ULastName));
			IEnumerable<MetricModel> metricList = DataService.GetMetricsRoot<tCMSWeb_Metric_List>(userID, item => item, null)
																.Select(metric => new MetricModel()
																{
																	MListID = metric.MListID,
																	MetricName = metric.MetricName,
																	MetricMeasure = metric.MetricMeasure,
																	ParentID = metric.ParentID,
																	MListEditedDate = metric.MListEditedDate,
																	CreateBy = metric.CreateBy,
																	isDefault = metric.isDefault,
																	UUsername = userLogin.Name
																});

			return metricList;
		}

		public IEnumerable<MetricModel> GetMetricChild(UserContext userLogin, int parentID)
		{
			string userFullName = IUser.Get<string>(userLogin.ID, item => item == null ? string.Empty : string.Format("{0} {1}", item.UFirstName, item.ULastName));
			IEnumerable<MetricModel> metricList = DataService.GetMetricByParentID<tCMSWeb_Metric_List>(userLogin, parentID, false, item => item, null)
																.Select(metric => new MetricModel()
																{
																	MListID = metric.MListID,
																	MetricName = metric.MetricName,
																	MetricMeasure = metric.MetricMeasure,
																	ParentID = metric.ParentID,
																	MListEditedDate = metric.MListEditedDate,
																	CreateBy = metric.CreateBy,
																	isDefault = metric.isDefault,
																	UUsername = userFullName
																});

			return metricList;
		}

		/// <summary>
		/// Delete site metric
		/// </summary>
		/// <param name="metricID"></param>
		/// <returns></returns>
		public TransactionalModel<List<MetricModel>> DeleteMetrics(UserContext userLogin, List<int> metricIdModel)
		{
			TransactionalModel<List<MetricModel>> retMetrics = new TransactionalModel<List<MetricModel>>();
			var metricsDB = DataService.GetMetricByID(userLogin, metricIdModel, item => item, null);
			if (metricsDB != null)
			{
				IEnumerable<tCMSWeb_Metric_List> parentsDB = metricsDB.Where(w => w.isDefault == true);
				if (parentsDB != null)
				{
					List<int> parentIds = parentsDB.Select(s => s.MListID).ToList();
					foreach (int id in parentIds)
					{
						metricsDB = DataService.GetMetricByParentID(userLogin, id, false, item => item, null);
						if (metricsDB != null)
						{
							metricIdModel.AddRange(metricsDB.Select(s => s.MListID).ToList());
						}
					}
				}
			}

			List<MetricSiteListModel> metricUsedList = checkMetricUsed(userLogin, metricIdModel);
			if (metricUsedList.Count > 0)
			{
				var metricIdUseds = metricUsedList.Select(s => s.MListID).ToList();
				IQueryable<MetricModel> metricUseds = DataService.GetMetricByID<MetricModel>(userLogin, metricIdUseds,
														s => new MetricModel()
														{
															MListID = s.MListID,
															MetricName = s.MetricName
														}, null);
				retMetrics.ReturnStatus = false;
				retMetrics.ReturnMessage.Add(CMSWebError.SITEMETRIC_IS_USED.ToString());
				retMetrics.Data = metricUseds.ToList();
				return retMetrics;
			}

			//Delete metric list
			if (!DataService.DeleteMetrics(metricIdModel))
			{
				retMetrics.ReturnStatus = false;
				retMetrics.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return retMetrics;
			}

			retMetrics.ReturnStatus = true;
			retMetrics.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return retMetrics;
		}

		/// <summary>
		/// Add metric site ( both insert and update)
		/// </summary>
		/// <param name="metricModel"></param>
		/// <returns></returns>
		public TransactionalModel<List<MetricModel>> AddMetricSite(UserContext userLogin, List<MetricModel> metricModel)
		{
			TransactionalModel<List<MetricModel>> returnmodel = new TransactionalModel<List<MetricModel>>();
			returnmodel.ReturnStatus = true;
			string message = string.Empty;
			MetricModel parent = metricModel.FirstOrDefault(i => i.isDefault == true);
			if (parent.MListID == 0)
			{
				//Add metrics
				returnmodel = InsertMetricList(metricModel, userLogin);
			}
			else
			{
				IQueryable<tCMSWeb_Metric_List> metricDB = DataService.GetMetricByParentID<tCMSWeb_Metric_List>(userLogin, parent.MListID, true, item => item, null);
				List<int> metricIds = metricModel.Select(s => s.MListID).ToList();
				IQueryable<tCMSWeb_Metric_List> metricUpdate = metricDB.Where(w => metricIds.Contains(w.MListID));
				//Remove metrics, if user removed child metrics.
				IQueryable<tCMSWeb_Metric_List> metricRemovesDB = metricDB.Except(metricUpdate);
				if (metricRemovesDB.Count() > 0)
				{
					List<int> metricRemoves = metricRemovesDB.Select(s => s.MListID).ToList();
					returnmodel = DeleteMetrics(userLogin, metricRemoves);
					if (!returnmodel.ReturnStatus)
					{
						return returnmodel;
					}
				}

				//Update metrics
				List<MetricModel> modelUpdate = new List<MetricModel>();
				if (metricUpdate.Count() > 0)
				{
					metricIds = metricUpdate.Select(s => s.MListID).ToList();
					modelUpdate = metricModel.Where(w => metricIds.Contains(w.MListID)).ToList();
					returnmodel = UpdateMetricList(modelUpdate, userLogin);
					if (!returnmodel.ReturnStatus)
						return returnmodel;
				}

				//Insert metrics, If user add new child metrics.
				List<MetricModel> metricsInsert = metricModel.Except(modelUpdate).ToList();
				if (metricsInsert.Count > 0)
				{
					foreach (MetricModel metric in metricsInsert)
					{
						metric.ParentID = parent.MListID;
					}
					returnmodel = InsertMetricList(metricsInsert, userLogin);
				}

			}

			returnmodel.Data = metricModel;
			return returnmodel;
		}

		/// <summary>
		/// Update metric site 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private TransactionalModel<List<MetricModel>> UpdateMetricList(List<MetricModel> model, UserContext userLogin)
		{
			TransactionalModel<List<MetricModel>> retMetricList = new TransactionalModel<List<MetricModel>>();
			retMetricList.Data = new List<MetricModel>();
			foreach (MetricModel metric in model)
			{
				TransactionalModel<MetricModel> retMetric = new TransactionalModel<MetricModel>();
				retMetric = UpdateMetric(metric, userLogin);
				if (!retMetric.ReturnStatus)
				{
					retMetricList.ReturnStatus = retMetric.ReturnStatus;
					retMetricList.ReturnMessage = retMetric.ReturnMessage;
					if (retMetric.Data != null)
						retMetricList.Data.Add(retMetric.Data);
					return retMetricList;
				}
			}
			retMetricList.ReturnStatus = true;
			retMetricList.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			return retMetricList;
		}

		private TransactionalModel<MetricModel> UpdateMetric(MetricModel metricModel, UserContext userLogin)
		{
			TransactionalModel<MetricModel> retMetric = new TransactionalModel<MetricModel>();
			MetricSiteBusinessRules Rules = new MetricSiteBusinessRules(Culture);
			Rules.ValidateInput(metricModel.MetricName);
			if (!Rules.ValidationStatus)
			{
				retMetric.ReturnStatus = false;
				retMetric.ReturnMessage.Add(CMSWebError.SITEMETRIC_NAME_REQUIRED.ToString());
			}

			List<int> metricIds = new List<int>();
			metricIds.Add(metricModel.MListID);
			tCMSWeb_Metric_List tMetric = DataService.GetMetricByID<tCMSWeb_Metric_List>(userLogin, metricIds, metric => metric, null).FirstOrDefault();
			SetEntity(ref tMetric, metricModel);
			if (CheckRegistExist(tMetric, userLogin))
			{
				retMetric.ReturnStatus = false;
				retMetric.ReturnMessage.Add(CMSWebError.SITEMETRIC_NAME_EXIST.ToString());
				//retMetric.ReturnMessage.Add(tMetric.MetricName);
				return retMetric;
			}

			//try
			//{
			//	DataService.UpdateMetric(tMetric);
			//	retMetric.ReturnStatus = true;
			//	retMetric.ReturnMessage.Add(CMSWebError.re.ToString());
			//}
			//catch (Exception ex)
			//{
			//	retMetric.ReturnStatus = false;
			//	retMetric.ReturnMessage.Add(CMSWebError.SITEMETRIC_REGIST_FAIL.ToString());
			//}
			tMetric = DataService.UpdateMetric(tMetric);
			if (tMetric == null)
			{
				retMetric.ReturnStatus = false;
				retMetric.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
				return retMetric;
			}

			retMetric.ReturnStatus = true;
			retMetric.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
			retMetric.Data = metricModel;
			return retMetric;
		}

		private TransactionalModel<List<MetricModel>> InsertMetricList(List<MetricModel> model, UserContext userLogin)
		{
			TransactionalModel<List<MetricModel>> retMetricList = new TransactionalModel<List<MetricModel>>();
			MetricModel parent = model.FirstOrDefault(i => i.isDefault == true);
			MetricModel[] childMetricList = model.Where(w => w.isDefault == false).ToArray();
			TransactionalModel<MetricModel> retMetric = new TransactionalModel<MetricModel>();
			int parentId = 0;
			if (parent != null)
			{
				retMetric = InserMetric(parent, userLogin);
				retMetricList.ReturnStatus = retMetric.ReturnStatus;
				retMetricList.ReturnMessage.AddRange(retMetric.ReturnMessage);
				retMetricList.ReturnMessage = retMetricList.ReturnMessage.Distinct().ToList();
				if (!retMetricList.ReturnStatus)
					return retMetricList;

				if (retMetric.Data != null)
					parentId = retMetric.Data.MListID;
			}

			if (childMetricList != null)
			{
				foreach (MetricModel metric in childMetricList)
				{
					if (parentId != 0)
						metric.ParentID = parentId;
					retMetric = new TransactionalModel<MetricModel>();
					if (retMetricList.Data == null)
						retMetricList.Data = new List<MetricModel>();
					retMetric = InserMetric(metric, userLogin);
					retMetricList.ReturnStatus = retMetric.ReturnStatus;
					retMetricList.ReturnMessage.AddRange(retMetric.ReturnMessage);
					retMetricList.ReturnMessage = retMetricList.ReturnMessage.Distinct().ToList();
					if (retMetric.Data != null)
						retMetricList.Data.Add(retMetric.Data);
					if (!retMetricList.ReturnStatus)
						return retMetricList;
				}
			}
			return retMetricList;
		}

		private TransactionalModel<MetricModel> InserMetric(MetricModel metric, UserContext userLogin)
		{
			TransactionalModel<MetricModel> result = new TransactionalModel<MetricModel>();
			MetricSiteBusinessRules Rules = new MetricSiteBusinessRules(Culture);
			Rules.ValidateInput(metric.MetricName);
			if (!Rules.ValidationStatus)
			{
				result.ReturnStatus = false;
				result.ReturnMessage.Add(CMSWebError.SITEMETRIC_NAME_REQUIRED.ToString());
				return result;
			}

			tCMSWeb_Metric_List tMetric = new tCMSWeb_Metric_List();
			SetEntity(ref tMetric, metric);
			if (CheckRegistExist(tMetric, userLogin))
			{
				result.ReturnStatus = false;
				result.ReturnMessage.Add(CMSWebError.SITEMETRIC_NAME_EXIST.ToString());
				//result.ReturnMessage.Add(tMetric.MetricName);
				return result;
			}

			tMetric = DataService.AddMetric(tMetric);
			if (tMetric == null)
			{
				result.ReturnStatus = false;
				result.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return result;
			}

			result.ReturnStatus = true;
			result.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			result.Data = new MetricModel()
			{
				MListID = tMetric.MListID,
				MetricName = tMetric.MetricName,
				ParentID = tMetric.ParentID,
				CreateBy = tMetric.CreateBy,
				isDefault = tMetric.isDefault,
				MListEditedDate = tMetric.MListEditedDate
			};
			return result;
		}

		private void SetEntity(ref tCMSWeb_Metric_List tMetric, MetricModel model)
		{
			if (tMetric == null)
				tMetric = new tCMSWeb_Metric_List();

			tMetric.MListID = model.MListID;
			tMetric.MetricName = model.MetricName;
			tMetric.isDefault = model.isDefault;
			tMetric.MetricMeasure = model.MetricMeasure;
			tMetric.MListEditedDate = model.MListEditedDate;
			tMetric.ParentID = model.ParentID;
			tMetric.CreateBy = model.CreateBy;
		}

		private bool CheckRegistExist(tCMSWeb_Metric_List metricModel, UserContext userLogin)
		{
			if ((metricModel.isDefault.HasValue && metricModel.isDefault.Value) || metricModel.ParentID == null)
			{
				IQueryable<tCMSWeb_Metric_List> tMetric = DataService.GetMetricByName(userLogin, metricModel.MetricName, metric => metric, null);
				if (tMetric == null) return false;
				//update itself
				if (tMetric.Any(w => w.MListID == metricModel.MListID))
					return false;
				return tMetric.Where(w => w.isDefault.HasValue && w.isDefault.Value == true || w.ParentID.HasValue == null).Count() > 0 ? true : false; //add new metric parent.
			}
			else
			{
				IQueryable<tCMSWeb_Metric_List> tMetric = DataService.GetMetricByParentID<tCMSWeb_Metric_List>(userLogin, metricModel.ParentID.Value, false, metric => metric, null)
																		.Where(w => w.MListID != metricModel.MListID);
				foreach (tCMSWeb_Metric_List metric in tMetric.ToList())
				{
					if (metric.MetricName == metricModel.MetricName)
						return true;
				}
			}
			return false;
		}

		private List<MetricSiteListModel> checkMetricUsed(UserContext userLogin, List<int> metricIds)
		{
			List<MetricSiteListModel> retMetrics = new List<MetricSiteListModel>();
			if (metricIds == null)
				return retMetrics;

			retMetrics = DataService.GetMetricSiteList<MetricSiteListModel>(metricIds, null, metric => new MetricSiteListModel()
				{
					MListID = metric.MListID,
					SiteID = metric.SiteID,
					CreatedDate = metric.CreateDate.HasValue ? metric.CreateDate.Value : DateTime.MinValue
				}, null).ToList();
			return retMetrics;
		}
	}
}
