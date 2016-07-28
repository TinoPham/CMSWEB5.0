using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CMSWebApi.BusinessServices.JobTitle;
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
	//[RoutePrefix("jobtitle")]
	[WebApiAuthenication]
	public class JobTitleController : ApiControllerBase<IJobTitleService, JobTitleBusinessService>
	{
		[HttpGet]
		[ActivityLog]
		public HttpResponseMessage JobTitle()
		{
			//BusinessService.IUser = base.DependencyResolver<IUsersService>();
			IQueryable<JobTitleModel> jobTitles = base.BusinessService.GetAllJobTitle(base.usercontext);
			return ResponseData< IQueryable<JobTitleModel>>(jobTitles);
			//return Request.CreateResponse(HttpStatusCode.OK, TransactionalModel<IQueryable<JobTitleModel>>.CreateModel( jobTitles));
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage JobTitle(JobTitleModel jobModel)
		{
			//BusinessService.IUser = base.DependencyResolver<IUsersService>();
			TransactionalModel<JobTitleModel> jobTitle = base.BusinessService.UpdateJobTitle(jobModel);
			return ResponseData<TransactionalModel<JobTitleModel>>(jobTitle);
			//return Request.CreateResponse(HttpStatusCode.OK, jobTitle);
		}

		[HttpPost]
		[ActivityLog]
		public HttpResponseMessage DeleteJobTitle([FromBody] List<int> listJobID)
		{
			//BusinessService.IUser = base.DependencyResolver<IUsersService>();
			TransactionalModel<JobTitleModel> result = base.BusinessService.DeleteJobTitle(base.usercontext, listJobID);
			return ResponseData<TransactionalModel<JobTitleModel>>(result);
		}
		
	}
}
