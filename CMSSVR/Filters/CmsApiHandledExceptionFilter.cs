using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Commons;
using ConvertMessage;
using SVRDatabase;

namespace CMSSVR.Filter
{
	public class CmsApiHandledExceptionFilter : System.Web.Http.Filters.ExceptionFilterAttribute//, System.Web.Mvc.IExceptionFilter
	{
		readonly int [] DB_Login_Failed_Error = new int [] { 18309, 18311, 18312, 18313, 18314, 18315, 18319, 18320, 1832, 18322, 18323, 18324, 18325, 18326, 18327, 18331, 18332, 18333, 18339, 18343, 18345, 18346, 18347, 18348, };
		public override System.Threading.Tasks.Task OnExceptionAsync(
			System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext,
			System.Threading.CancellationToken cancellationToken)
		{
			HttpException httpException = actionExecutedContext.Exception as HttpException ?? new HttpException(500, "Internal Server Error", actionExecutedContext.Exception);
			 MessageResult msgResult = null;
			if (httpException.GetHttpCode() != (int)HttpStatusCode.InternalServerError)
			{
				return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
			}
			if( actionExecutedContext.Exception != null && actionExecutedContext.Exception.InnerException != null && actionExecutedContext.Exception.InnerException is SqlException)
			{
				msgResult = SQLException(actionExecutedContext.Exception.InnerException as SqlException);
			}

			HttpStatusCode status = (HttpStatusCode)httpException.GetHttpCode();
			if( msgResult == null)
			{
				msgResult = new MessageResult()
				{
					Data = actionExecutedContext.Exception.Message,
					ErrorID = ERROR_CODE.SERVICE_EXCEPTION
				};
				msgResult.httpStatus = status;
			}

			var errorResponse = actionExecutedContext.Request.CreateResponse<MessageResult>(msgResult.httpStatus, msgResult);
			actionExecutedContext.Response = errorResponse;
			return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
		}
		private MessageResult SQLException(SqlException sqlException)
		{
			if( sqlException == null)
			{
				return null;
			}
			//Informational messages that return status information or report errors that are not severe. The Database Engine does not raise system errors with severities of 0 through 9.
			//Informational messages that return status information or report errors that are not severe. For compatibility reasons, the Database Engine converts severity 10 to severity 0 before returning the error information to the calling application.
			if( sqlException.Number >= 18301 &&  sqlException.Number <= 18489)
			{
				return new MessageResult{ ErrorID = ERROR_CODE.DB_CONNECTION_FAILED, httpStatus = HttpStatusCode.RequestTimeout};
			}
			return null;
		}

	}
	
}