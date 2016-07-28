using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using CMSWebApi.DataModels;
using Extensions.Linq;

namespace CMSWebApi.APIFilters
{
	public class CmsQueryableAttribute : EnableQueryAttribute
	{
		private bool _isAggresion = false;
		public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
		{
			IQueryable result = default(IQueryable);

			HttpRequestMessage originalRequest = queryOptions.Request;

			string url = originalRequest.RequestUri.AbsoluteUri;

			if (queryOptions.Count == null && !url.Contains("&$count=true"))
			{
				url = url.Insert(url.Count(), "&$count=true");
				var req = new HttpRequestMessage(HttpMethod.Get, url);
				queryOptions = new ODataQueryOptions(queryOptions.Context, req);
			}

			if (queryOptions.Apply == null)
			{
				result = queryOptions.ApplyTo(queryable);

				// add the NextLink if one exists
				if (queryOptions.Request.ODataProperties().NextLink != null)
				{
					originalRequest.ODataProperties().NextLink = queryOptions.Request.ODataProperties().NextLink;
				}
				// add the TotalCount if one exists
				if (queryOptions.Request.ODataProperties().TotalCount != null)
				{
					originalRequest.ODataProperties().TotalCount = queryOptions.Request.ODataProperties().TotalCount;
				}
				_isAggresion = false;
			}
			else
			{
				result = queryOptions.ApplyTo(queryable);
				_isAggresion = true;
			}

			// return all results
			return result;
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			long? orgsize = null;
			object responseObj;
			//actionExecutedContext.Response.TryGetContentValue(out responseObj);
			//var orgquery = responseObj as IQueryable<object>;

			base.OnActionExecuted(actionExecutedContext);

			if (ResponseIsValid(actionExecutedContext.Response))
			{
				var topgroup = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("topgroup");
				var skipgroup = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("skipgroup");
				var ordergroup = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("ordergroup");
				actionExecutedContext.Response.TryGetContentValue(out responseObj);

				if (responseObj is IQueryable)
				{
					var robj = responseObj as IQueryable<object>;

					var count = actionExecutedContext.Request.ODataProperties().TotalCount;
					if (count != null)
					{
						orgsize = count;
					}
					else
					{
						orgsize = robj.Count();
					}


					if (_isAggresion)
					{
						if (skipgroup != null)
						{
							var skip = Convert.ToInt32(skipgroup);
							robj = robj.OrderBy(ordergroup);
							robj = robj.Skip(skip);
						}

						if (topgroup != null)
						{
							var top = Convert.ToInt32(topgroup);
							robj = robj.Take(top);
						}
					}

					var group = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("groupPayment");
					if (group != null)
					{
						var paymentGroup = robj as IQueryable<TransactionViewerModel>;

						if (paymentGroup == null)
						{
							var paymentdetailGroup = robj as IQueryable<TransactionDetailViewerModel>;
							var result = paymentdetailGroup.Where(p=>p.Payments.Any())
							.SelectMany(p => p.Payments, (l, r) => new { Tran = new{ l.PacId , l.Total}, Id = r.Id, Name = r.Name })
							.GroupBy(t => new { t.Id, t.Name, t.Tran.PacId })
							.Select(t => new
							{
								t.Key.PacId,
								t.Key.Id,
								Name = t.Key.Name,
								TotalTran = t.Sum(g => g.Tran.Total)
							}).ToList();
							actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, new CmsMetadata<object>(result.Count, result));
						}
						else
						{
							var result = paymentGroup.Where(p=>p.Payments.Any())
							.SelectMany(p => p.Payments, (l, r) => new { Tran = new { l.PacId, l.Total }, Id = r.Id, Name = r.Name })
							.GroupBy(t => new { t.Id, t.Name, t.Tran.PacId })
							.Select(t => new
							{
								t.Key.PacId,
								t.Key.Id,
								Name = t.Key.Name,
								TotalTran = t.Sum(g => g.Tran.Total)
							}).ToList();							
							actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, new CmsMetadata<object>(result.Count, result));
						}
					}
					else
					{
						actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, new CmsMetadata<object>(orgsize, robj));
					}
				}
			}
		}



		private bool ResponseIsValid(HttpResponseMessage response)
		{
			if (response == null || response.StatusCode != HttpStatusCode.OK || !(response.Content is ObjectContent))
			{
				return false;
			}

			return true;
		}
	}
}
