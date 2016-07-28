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

namespace CMSWebApi.BusinessServices.JobTitle
{
	public class JobTitleBusinessService: BusinessBase<IJobTitleService>
	{
		public IUsersService IUser{ get; set;}

		public IQueryable<JobTitleModel> GetAllJobTitle(UserContext Context)
		{
			int userID = Context.Createdby.HasValue? IUser.GetMasterID(Context.CompanyID) : Context.ID;
			string createdName =  IUser.Get<String>(userID, user => user == null ? null : string.Format("{0} {1}", user.UFirstName, user.ULastName));
			IQueryable<JobTitleModel> jobTitle = DataService.Gets<JobTitleModel>(userID, null, item => new JobTitleModel
			{ 
				PositionID= item.PositionID, PositionName= item.PositionName,
				Description= item.Description
				,Color = item.Color.HasValue ? item.Color.Value : Consts.JOBTITLE_DEFAULT_COLOR

			});
			return jobTitle;
			
		}

		public TransactionalModel<JobTitleModel> DeleteJobTitle(UserContext userLogin, List<int> listJobID)
		{
			TransactionalModel<JobTitleModel> result = new TransactionalModel<JobTitleModel>();
			string message = string.Empty;
			IQueryable<JobTitleModel> jobTitle = JobTitleUsed(userLogin.ID, listJobID);
			if (jobTitle.Count() > 0)
			{
				result.ReturnStatus = false;
				result.ReturnMessage.Add(CMSWebError.JOB_IS_USED.ToString());
				result.ReturnMessage.Add(string.Join(",", jobTitle.Select(s => s.PositionName).ToList()));
				return result;
			}

			if (!DataService.Delete(listJobID))
			{
				result.ReturnStatus = false;
				result.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
				return result;
			}

			result.ReturnStatus = true;
			result.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
			return result;
		}

		public TransactionalModel<JobTitleModel> UpdateJobTitle(JobTitleModel model)
		{
			TransactionalModel<JobTitleModel> response = new TransactionalModel<JobTitleModel>();
			JobtitleBusinessRules Rules = new JobtitleBusinessRules(Culture);
			Rules.ValidateInput(model.PositionName);
			if (!Rules.ValidationStatus)
			{
				Rules.SetTransactionInfomation(response);
				return response;
			}

			if (CheckRegistExist(model.PositionName, model.PositionID, model.CreatedBy))
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.JOB_NAME_EXIST_MSG.ToString());
				return response;
			}

			tCMSWeb_UserPosition tJobTitle = new tCMSWeb_UserPosition();
			if (model.PositionID != 0) //Update Job Title
			{
				tJobTitle = DataService.Gets<tCMSWeb_UserPosition>(model.PositionID, item => item);
				SetEntityJob(ref tJobTitle, model);
				if (DataService.Update(tJobTitle) == null)
				{
					response.ReturnStatus = false;
					response.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
					return response;
				}

				response.ReturnStatus = true;
				response.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
				response.Data = model;
				return response;
			}

			//Add Job Title
			model.CreatedName = IUser.Get<String>(model.CreatedBy, user => user == null ? null : string.Format("{0} {1}", user.UFirstName, user.ULastName));
			SetEntityJob(ref tJobTitle, model);
			if (DataService.Add(tJobTitle) == null)
			{
				response.ReturnStatus = false;
				response.ReturnMessage.Add(CMSWebError.ADD_FAIL_MSG.ToString());
				return response;
			}

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.ADD_SUCCESS_MSG.ToString());
			model.PositionID = tJobTitle.PositionID;
			response.Data = model;
			return response;
		}

		private bool CheckRegistExist(string jobName, int jobID, int CreatedBy)
		{
			IQueryable<tCMSWeb_UserPosition> tJobTitle = DataService.Gets<tCMSWeb_UserPosition>(jobName, CreatedBy, item => item);
			//check insert
			if (jobID == 0)
				return (tJobTitle.Count() > 0) ? true : false;
			//check update
			else
			{
				var name = from item in tJobTitle
						   where item.PositionID != jobID
						   select item;
				return name.Count() > 0 ? true : false;
			}
		}

		public void SetEntityJob(ref tCMSWeb_UserPosition tJobTitle, JobTitleModel model)
		{
			if (tJobTitle == null)
				tJobTitle = new tCMSWeb_UserPosition();

			tJobTitle.PositionID = model.PositionID;
			tJobTitle.PositionName = model.PositionName;
			tJobTitle.CreatedBy = model.CreatedBy;
			tJobTitle.CreatedDate = model.CreatedDate;
			tJobTitle.Description = model.Description;
			tJobTitle.Color = model.Color.HasValue ? model.Color.Value : Consts.JOBTITLE_DEFAULT_COLOR;
		}

		private IQueryable<JobTitleModel> JobTitleUsed(int createdBy, List<int> posIds)
		{
			IQueryable<JobTitleModel> jobTitle = IUser.Gets<UserInfoDetail>(createdBy, null, null,
				user => new UserInfoDetail()
				{
					PosID = user.PositionID,PosName = user.tCMSWeb_UserPosition == null ? "" : user.tCMSWeb_UserPosition.PositionName
				}).Where(w => posIds.Contains(w.PosID.HasValue ? w.PosID.Value : 0))
					.Select(s => new JobTitleModel() { 
						PositionID = s.PosID.Value,
						PositionName = s.PosName
					}).Distinct();
			return jobTitle;
		}
	}
}
