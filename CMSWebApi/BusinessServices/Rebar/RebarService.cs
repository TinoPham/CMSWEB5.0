using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using CMSWebApi.DataModels.ModelBinderProvider;
using Extensions.Linq;
using System.Linq.Expressions;

namespace CMSWebApi.BusinessServices.Rebar
{
	public class RebarService : BusinessBase<IRebarDataService>
	{
		#region Properties

		public IUsersService IUser { get; set; }
		public ICompanyService comSvc { get; set; }
		public ISiteService ISiteSvc { get; set; }

		#endregion

		public List<BoxRebarModel> GetMetricBox(UserContext usercontext, BoxRebarParamModel param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>();

			var totalTrans = DataService.GetPosTransactions()
				.Where(t => pacIds.Contains(t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate)
				.Select(t => t.TransID).Count();

			var tranTypes = DataService.GetTransExceptionType()
				.Include(t => t.tbl_Exception_Transact)
				.Include(t => t.tbl_Exception_Type)
				.Where(t => param.Types.Contains(t.TypeID) && pacIds.Contains(t.tbl_Exception_Transact.T_PACID) &&
						t.tbl_Exception_Transact.TransDate >= sDate && t.tbl_Exception_Transact.TransDate <= eDate)
				.Select(t => new { t.TypeID, t.tbl_Exception_Type.TypeName, t.tbl_Exception_Transact.T_6TotalAmount })
				.GroupBy(t => new { t.TypeID, t.TypeName })
				.Select(t => new BoxRebarModel()
				{
					Id = t.Key.TypeID,
					Name = t.Key.TypeName,
					Total = t.Count(),
					TotalTran = totalTrans
				});
			return tranTypes.ToList();
		}

		public tCMSWeb_Company GetCompanyLogo(UserContext userctx)
		{
			int? CompanyID = IUser.Get<int?>(userctx.ParentID, item => item.CompanyID); //DataService.SelectUser(userValue.ID);
			return comSvc.SelectCompanyInfo((int)CompanyID);
		}

		private List<UserSiteDvrChannel> getPacIds(UserContext usercontext, List<int> siteKeys)
		{
			IEnumerable<UserSiteDvrChannel> sites =
				Task.Run(() => base.UserSitesBySiteIDsAsync(IUser, usercontext, siteKeys)).Result;

			if (sites == null)
			{
				return new List<UserSiteDvrChannel>();
			}

			List<UserSiteDvrChannel> pacIds = sites.Where(t => t.PACID != null).ToList();
			return pacIds;
		}

		public IQueryable<TranPaymentChartModel> GetTransactPaymentTypes(UserContext usercontext, TranPaymentParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;
			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int>();

			var queryem = DataService.GetMetricRebar()
				.Where(
					t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
			var queryPaymentsTran = DataService.GetTransExceptionPayments();

			var query = from tr in queryem
						join ty in queryPaymentsTran on tr.TransID equals ty.TransID
						select new
						{
							tr.TransID,
							tr.T_6TotalAmount,
							ty.PaymentID,
							PaymentName = ty.tbl_POS_PaymentList != null ? ty.tbl_POS_PaymentList.PaymentName : ""
						}
							into g
							group g by new { g.PaymentID, g.PaymentName }
								into f
								select new TranPaymentChartModel
								{
									PaymentId = f.Key.PaymentID,
									PaymentName = f.Key.PaymentName,
									Total = f.Sum(g => g.T_6TotalAmount),
									TranCount = f.Count()
								};

			return query;
		}

		public EmployerModel GetEmployerRebarForChart(UserContext usercontext, EmployerParamModel param)
		{
			IQueryable<EmployerPagings> query;

			DateTime sDate = Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = Convert.ToDateTime(param.EndTranDate);

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>();


			var queryem = DataService.GetMetricRebar().Where(t => pacIds.Contains(t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);

			query = from tr in queryem
					select new { tr.T_PACID, tr.T_OperatorID }
						into g
						group g by new { g.T_PACID, g.T_OperatorID }
							into f
							select new EmployerPagings()
							{
								PACID = f.Key.T_PACID,
								Id = f.Key.T_OperatorID
							};

			var totalCount = query.Count();
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var results = query
				.OrderBy(x => x.Id)
				.Skip(param.PageSize * (param.PageNo - 1))
				.Take(param.PageSize).ToList();

			var resultData = GetDataforEmp(results, param, listPacSite);

			IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();

			results.ForEach(t =>
			{

				var site = listPacSite.FirstOrDefault(g => g.KDVR == t.PACID);
				if (site != null)
				{
					t.SiteKey = site.siteKey;
					var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
					t.SiteName = sitedb != null ? sitedb.Name : "";
				}

				t.Charts = resultData.Where(f => f.EmployerId == t.Id && f.PacId == t.PACID).ToList();
				t.TotalException = t.Charts.Any() ? t.Charts.Sum(g => g.Value) : 0;
				t.Name = t.Charts.Count > 0 ? t.Charts[0].EmployerName : "";
			});


			return new EmployerModel()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results.ToList()
			};

		}

		public EmployeeRiskSummaryPagings GetEmployeeRisks(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			IQueryable<EmployeeRiskSummary> query;

			DateTime sDate = param.StartTranDate;//Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = param.EndTranDate;//Convert.ToDateTime(param.EndTranDate);

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int>().ToList();

			IQueryable<tbl_Exception_Transact> queryem = DataService.GetMetricRebar().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
            if ( (param.GroupBy!= (int) GroupBy.EMPL))
            {
                 int? id;
                 if (param.Employees == null)
                     id = null;
                 else id = int.Parse(param.Employees);

                queryem = DataService.GetMetricRebar().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate && t.T_OperatorID == id);
            }

            IQueryable<tbl_POS_Transact> querytransac = DataService.GetPosTransactions().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
            if (param.GroupBy!= (int) GroupBy.EMPL)
            {
                int? id;
                if (param.Employees == null)
                    id = null;
                else id = int.Parse(param.Employees);
                querytransac = DataService.GetPosTransactions().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate && t.T_OperatorID == id );
            }
            if ((param.GroupBy== (int) GroupBy.EMPL))
            {
			query = from tr in queryem
					select
						new
						{
							tr.TransID,
							tr.T_PACID,
							tr.T_StoreID,
							Store_Name = tr.tbl_POS_StoreList.Store_Name,
							tr.T_OperatorID,
							EmpName = tr.tbl_POS_OperatorList.Operator_Name,
							tr.T_6TotalAmount
						}
						into g
						group g by
							new { g.T_PACID, g.T_StoreID, g.Store_Name, g.T_OperatorID, g.EmpName }
							into f
							select new EmployeeRiskSummary()
							{
								PacId = f.Key.T_PACID,
								EmployerId = f.Key.T_OperatorID,
								EmployerName = f.Key.EmpName,
								SiteKey = f.Key.T_PACID,
								StoreId = f.Key.T_StoreID,
								StoreName = f.Key.Store_Name,
								RiskFactor = f.Count(),
                                    TotalAmmount = (decimal)f.Sum(x => x.T_6TotalAmount),
                                };
            }
            else
            {
                query = from tr in queryem
                        select
                            new
                            {
                                tr.TransID,
                                tr.T_PACID,
                                tr.T_StoreID,
                                Store_Name = tr.tbl_POS_StoreList.Store_Name,
                                tr.T_OperatorID,
                                EmpName = tr.tbl_POS_OperatorList.Operator_Name,
                                tr.T_6TotalAmount,
                                Year = tr.TransDate.Value.Year,
                                Month = tr.TransDate.Value.Month,
                                Day = tr.TransDate.Value.Day,
                            }
                            into g
                            group g by
                                new { g.Year,g.Month,g.Day }
                                into f
                                select new EmployeeRiskSummary()
                                {
                                    PacId = f.FirstOrDefault().T_PACID,
                                    EmployerId = f.FirstOrDefault().T_OperatorID,
                                    SiteKey = f.FirstOrDefault().T_PACID,
                                    StoreId = f.FirstOrDefault().T_StoreID,
                                    StoreName = f.FirstOrDefault().Store_Name,
                                    RiskFactor = f.Count(),
                                    TotalAmmount = (decimal)f.Sum(x => x.T_6TotalAmount),
                                    Day =f.Key.Day,
                                    Month = f.Key.Month,
                                    Year = f.Key.Year
							};
            }

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};


            List<EmployeeRiskSummary> results;
            if ((param.GroupBy== (int) GroupBy.EMPL))
            {
                switch (param.Sort)
                {
                    default:
                    case (int)SortField.Employee:
                results = query
                              .OrderByDescending(x => x.EmployerId)
                              .Skip(param.PageSize * (param.PageNo - 1))
                              .Take(param.PageSize).ToList();
                        break;
                    case (int)SortField.RatioToSale:

                        var _obj = from p in querytransac
                                   group p by new { p.T_OperatorID, p.T_PACID } into g
                                   select new { T_PACID = g.Key.T_PACID, T_OperatorID = g.Key.T_OperatorID, Count = g.Count()};
                        var re = query.Join(_obj, l => new { A = l.EmployerId.Value, B = l.PacId.Value }, r => new { A = r.T_OperatorID.Value, B = r.T_PACID }, (l, r) => new
                        {
                            P = l,
                            M = r
                        }).OrderByDescending(x => (x.P.RiskFactor *100 / x.M.Count)).Skip(param.PageSize * (param.PageNo - 1)).Take(param.PageSize).ToList();

                        results = re.Select(w => new EmployeeRiskSummary() {
                            Day = w.P.Day,
                            EmployerId = w.P.EmployerId,
                            EmployerName = w.P.EmployerName,
                            Month = w.P.Month,
                            PacId = w.P.PacId,
                            PercentToSale = w.M.Count == 0 ? w.M.Count : w.P.RiskFactor *100 / w.M.Count,
                            RiskFactor = w.P.RiskFactor,
                            SiteKey = w.P.SiteKey,
                            SiteName = w.P.SiteName,
                            StoreId = w.P.StoreId,
                            StoreName = w.P.StoreName,
                            TotalAmmount = w.P.TotalAmmount,
                            TotalTran = w.M.Count,
                            Year = w.P.Year
                        }).ToList();
                        break;
                    case(int)SortField.TotalAmount:
                          results = query
                              .OrderByDescending(x => x.TotalAmmount)
                              .Skip(param.PageSize * (param.PageNo - 1))
                              .Take(param.PageSize).ToList();
                        break;
                    case (int)SortField.RiskFactor:
                        results = query
                            .OrderByDescending(x => x.RiskFactor)
				.Skip(param.PageSize * (param.PageNo - 1))
				.Take(param.PageSize).ToList();
                        break;

                }
              
            }
            else 
            {
                results = query
                .OrderBy(x => new { x.Year,x.Month,x.Day })
                .Skip(param.PageSize * (param.PageNo - 1))
                .Take(param.PageSize).ToList();
            }

			var listEmployee = results.Select(t => t.EmployerId).ToList();

			IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();

            if ((param.GroupBy== (int) GroupBy.EMPL))
            {
			var transSum = querytransac.Where(t => listEmployee.Contains(t.T_OperatorID))
					.Select(t => new { t.TransID, t.T_OperatorID, t.T_PACID, t.T_6TotalAmount })
					.GroupBy(t => new { t.T_OperatorID, t.T_PACID })
					.Select(t => new { Id = t.Key.T_OperatorID, PacID = t.Key.T_PACID, Count = t.Count() }).ToList();
                results.ForEach(t =>
                {
                    var site = listPacSite.FirstOrDefault(g => g.KDVR == t.PacId);

                    if (site != null)
                    {
                        t.SiteKey = site.siteKey;
                        var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
                        t.SiteName = sitedb != null ? sitedb.Name : "";
                    }

                    var totalTran = transSum.FirstOrDefault(g => g.Id == t.EmployerId && g.PacID == t.PacId);

                    if(param.Sort!= (int)SortField.RatioToSale)
                    {
                    if (totalTran != null && totalTran.Count > 0)
                    {
                        t.TotalTran = totalTran.Count;
                            t.PercentToSale = t.RiskFactor / totalTran.Count * 100;
                    }
                    else
                    {
                        t.TotalTran = 0;
                        t.PercentToSale = 0;
                    }
                    }
                });
            }
            else {

                if (param.GroupBy == (int)GroupBy.DATE)
                {

                    var transSum = querytransac
                    .Select(t => new { t.TransID, t.DVRDate.Value.Day, t.DVRDate.Value.Month, t.DVRDate.Value.Year, t.T_PACID, t.T_6TotalAmount })
                    .GroupBy(t => new { t.Day, t.Month, t.Year })
                    .Select(t => new { Id = t.Key,  Count = t.Count(), Month = t.Key.Month, Day = t.Key.Day, Year = t.Key.Year }).ToList();

                    results.ForEach(t =>
                    {
                        var site = listPacSite.FirstOrDefault(g => g.KDVR == t.PacId);

                        if (site != null)
                        {
                            t.SiteKey = site.siteKey;
                            var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
                            t.SiteName = sitedb != null ? sitedb.Name : "";
                        }

                        var totalTran = transSum.FirstOrDefault(g => g.Day == t.Day && g.Month == t.Month && g.Year == t.Year);

                        if (totalTran != null && totalTran.Count > 0)
                        {
                            t.TotalTran = totalTran.Count;
                            t.PercentToSale = (t.RiskFactor / totalTran.Count) * 100;
                        }
                        else
                        {
                            t.TotalTran = 0;
                            t.PercentToSale = 0;
                        }
                    });

                }
                else {

                var transSum = querytransac
                .Select(t => new { t.TransID, t.DVRDate.Value.Day, t.DVRDate.Value.Month,t.DVRDate.Value.Year, t.T_PACID, t.T_6TotalAmount })
                .GroupBy(t => new { t.Day, t.Month, t.Year, t.T_PACID })
                .Select(t => new { Id = t.Key, PacID = t.Key.T_PACID, Count = t.Count(),Month=t.Key.Month,Day =t.Key.Day,Year = t.Key.Year }).ToList();

			results.ForEach(t =>
			{
				var site = listPacSite.FirstOrDefault(g => g.KDVR == t.PacId);

				if (site != null)
				{
					t.SiteKey = site.siteKey;
					var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
					t.SiteName = sitedb != null ? sitedb.Name : "";
				}

				var totalTran = transSum.FirstOrDefault(g => g.Day == t.Day && g.Month==t.Month && g.Year == t.Year  && g.PacID == t.PacId);

				if (totalTran != null && totalTran.Count > 0)
				{
					t.TotalTran = totalTran.Count;
					t.PercentToSale = (t.RiskFactor / totalTran.Count) * 100;
				}
				else
				{
					t.TotalTran = 0;
					t.PercentToSale = 0;
				}
			});
                }


			}

			return new EmployeeRiskSummaryPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				SumRiskFactors = totalCount !=0 ? (int)query.Sum(item=>item.RiskFactor!=null ? (int) item.RiskFactor : 0 ) : 0,
				Data = results.ToList()
			};
		}

		public SiteRiskSummaryPagings GetSitesRisks(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			IQueryable<SiteRiskSummarySummary> query;

			DateTime sDate = param.StartTranDate;//Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = param.EndTranDate;//Convert.ToDateTime(param.EndTranDate);

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int>().ToList();

			var queryem = DataService.GetMetricRebar().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
			var querytransac = DataService.GetPosTransactions().Where(t => pacIds.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
			query = from tr in queryem
					select new
						{
							tr.TransID,
							tr.T_PACID,
							tr.T_6TotalAmount
						}
						into g
						group g by
							new { g.T_PACID }
							into f
							select new SiteRiskSummarySummary()
							{
								PacId = f.Key.T_PACID,
								RiskFactor = f.Count(),
								TotalAmmount = (decimal)f.Sum(x => x.T_6TotalAmount)
							};

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);



            var _obj = from p in querytransac
                       group p by  p.T_PACID into g
                       select new 
                       { T_PACID = g.Key, Count = g.Count() };
            var re = query.Join(_obj, l => l.PacId.Value, r => r.T_PACID, (l, r) => new
            {
                P = l,
                M = r
            });

            var res = re;
            switch (param.Sort)
            {
                case (int)SortField.Employee:
                    res = re.OrderByDescending(x => x.P.PacId).Skip(param.PageSize * (param.PageNo - 1)).Take(param.PageSize);
                    break;
                case (int)SortField.RatioToSale:
                    res = re.OrderByDescending(x => (x.P.RiskFactor * 100 / x.M.Count)).Skip(param.PageSize * (param.PageNo - 1)).Take(param.PageSize);
                    break;
                case (int)SortField.RiskFactor:
                    res = re.OrderByDescending(x => x.P.RiskFactor).Skip(param.PageSize * (param.PageNo - 1)).Take(param.PageSize);
                    break;
                case (int)SortField.TotalAmount:
                    res = re.OrderByDescending(x => x.P.TotalAmmount).Skip(param.PageSize * (param.PageNo - 1)).Take(param.PageSize);
                    break;
            }

            var results = res.ToList().Select(w => new SiteRiskSummarySummary()
            {
                PacId = w.P.PacId,
                PercentToSale = w.M.Count == 0 ? w.M.Count : w.P.RiskFactor * 100 / w.M.Count,
                RiskFactor = w.P.RiskFactor,
                SiteKey = w.P.SiteKey,
                SiteName = w.P.SiteName,
                TotalAmmount = w.P.TotalAmmount,
                TotalTran = w.M.Count
                   
            }).ToList();

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var listpac = results.Select(t => t.PacId).ToList();
			IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();
			results.ForEach(t =>
			{
				var site = listPacSite.FirstOrDefault(g => g.KDVR == t.PacId);

				if (site != null)
				{
					t.SiteKey = site.siteKey;
					var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
					t.SiteName = sitedb != null ? sitedb.Name : "";
				}
			});
			return new SiteRiskSummaryPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
                SumRiskFactors = totalCount != 0 ? query.Sum(item => item.RiskFactor!=null ? (int)item.RiskFactor : 0 ):0,
				Data = results.ToList()
			};
		}

		public SiteExceptionTransPagings GetTransWOCust(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			string strpacids = string.Join(",", pacIds);
			List<Proc_Exception_TransWOC_Result> queryem = DataService.GetTransWOCust(strpacids, sDate.StartOfDay(), eDate.EndOfDay()).ToList();
			bool incEmp = false;
			int? empid = null;
			if ((param.GroupBy != (int)GroupBy.SITE) && (param.GroupBy != (int)GroupBy.EMPL))
			{
				incEmp = true;
				if (param.Employees == null)
					empid = null;
				else 
					empid = int.Parse(param.Employees);

				queryem = queryem.Where(x => x.T_OperatorID == empid).ToList();
			}
			List<int> validPACID = queryem.Select(x => x.PACID_ID).Distinct().ToList();

			var kdvr_pacid = listPacSite.Where(x=>validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, PACID = x.PACID }).Distinct().ToList();
			List<int> lsKDVR = kdvr_pacid.Select(x => x.KDVR ?? 0).Distinct().ToList();
			List<tbl_POS_Transact> querytranno = incEmp ? DataService.GetPosTransactions().Where(t => lsKDVR.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate && t.T_OperatorID == empid).ToList()
														 : DataService.GetPosTransactions().Where(t => lsKDVR.Contains((int)t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate).ToList();

			var querytransac = querytranno.Join(kdvr_pacid, t => t.T_PACID, d => d.KDVR, (t, d) => new { T_PACID = d.PACID, T_OperatorID = t.T_OperatorID, TransDate = t.TransDate });

			IEnumerable<TransInfoSite> query = null;
			List<TransInfoSite> results = null;
			int totalCount = 0;
			int totalPages = 0;

			if (param.GroupBy == (int)GroupBy.SITE)
			{
				#region Group By Site
				var grData = queryem.Select(x => new { TransID = x.TransID, T_PACID = x.PACID_ID, T_6TotalAmount = x.T_6TotalAmount ?? 0 });
				query = grData.GroupBy(x => x.T_PACID).Select(gr => new TransInfoSite()
				{
					PacId = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0,
					TotalAmmount = gr.Any() ? (decimal)gr.Sum(x => x.T_6TotalAmount) : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.PacId)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltrans = querytransac.GroupBy(x => x.T_PACID).Select(gr => new { T_PACID = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltrans, l => l.PacId, r => r.T_PACID, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor,
					//SiteKey = l.SiteKey,
					//SiteName = l.SiteName,
					TotalAmmount = l.TotalAmmount
				});
				var kdvr_site = listPacSite.Where(x => validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, SiteKey = x.siteKey }).Distinct().ToList();
				var site_data = res_cmp.Join(kdvr_site, c => c.PacId, s => s.KDVR, (c, s) => new 
					{
						PacId = c.PacId,
						Count = c.Count,
						RiskFactor = c.RiskFactor,
						SiteKey = s.SiteKey,
						TotalAmmount = c.TotalAmmount
					});
				var group_site = site_data.GroupBy(x => x.SiteKey).Select(gr => new 
					{
						PacId = gr.Any() ? gr.FirstOrDefault().PacId : 0,
						Count = gr.Any() ? gr.Sum(x=>x.Count) : 0,
						RiskFactor = gr.Any() ? gr.Sum(x=>x.RiskFactor) : 0,
						SiteKey = gr.Key,
						TotalAmmount = gr.Any() ? gr.Sum(x=>x.TotalAmmount) : 0
					});

				results = group_site.Select(w => new TransInfoSite()
				{
					PacId = w.PacId,
					PercentToSale = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					SiteKey = w.SiteKey,
					//SiteName = w.SiteName,
					TotalAmmount = w.TotalAmmount,
					TotalTran = w.Count
				}).OrderByDescending(x=>x.RiskFactor).ToList();
				var listpac = results.Select(t => t.PacId).ToList();
				IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();
				results.ForEach(t =>
				{
					//var site = listPacSite.FirstOrDefault(g => g.PACID == t.PacId);
					var site = listPacSite.FirstOrDefault(g => g.siteKey == t.SiteKey);
					if (site != null)
					{
						t.SiteKey = site.siteKey;
						var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
						t.SiteName = sitedb != null ? sitedb.Name : "";
					}
				});
				#endregion
			}
			else if (param.GroupBy == (int)GroupBy.EMPL)
			{
				#region Group By Emp
				List<int> lsEmpIDs = queryem.Select(x => x.T_OperatorID ?? 0).ToList();
				List<tbl_POS_OperatorList> lsEmps = DataService.GetPOSOperatorList().Where(e => lsEmpIDs.Contains(e.Operator_ID)).ToList();

				var grData = queryem.Select(x => new 
					{
						TransID = x.TransID, 
						T_PACID = x.PACID_ID, 
						T_StoreID = x.T_StoreID ?? 0,
						//Store_Name = lsEmps.Any(e => e.Operator_ID == x.T_OperatorID) ? lsEmps.FirstOrDefault(e => e.Operator_ID == x.T_OperatorID).Operator_Name : string.Empty, 
						T_OperatorID = x.T_OperatorID,
						EmpName = lsEmps.Any(e => e.Operator_ID == x.T_OperatorID) ? lsEmps.FirstOrDefault(e => e.Operator_ID == x.T_OperatorID).Operator_Name : string.Empty,
						T_6TotalAmount = x.T_6TotalAmount ?? 0 
					});

				query = grData.GroupBy(x => new { x.T_PACID, x.T_StoreID, x.T_OperatorID }).Select(gr => new TransInfoSite()
				{
					PacId = gr.Key.T_PACID,
					EmployerId = gr.Key.T_OperatorID ?? 0,
					EmployerName = gr.Any() ? gr.FirstOrDefault().EmpName : string.Empty,
					RiskFactor = gr.Any() ? gr.Count() : 0,
					TotalAmmount = gr.Any() ? (decimal)gr.Sum(x => x.T_6TotalAmount) : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.EmployerId)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltrans = querytransac.GroupBy(x => x.T_OperatorID).Select(gr => new { EmpID = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltrans, l => l.EmployerId, r => r.EmpID, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor,
					EmployerId = l.EmployerId,
					EmployerName = l.EmployerName,
					//SiteKey = l.SiteKey,
					//SiteName = l.SiteName,
					TotalAmmount = l.TotalAmmount
				});

				results = res_cmp.Select(w => new TransInfoSite()
				{
					PacId = w.PacId,
					PercentToSale = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					EmployerId = w.EmployerId,
					EmployerName = w.EmployerName,
					TotalAmmount = w.TotalAmmount,
					TotalTran = w.Count
				}).OrderByDescending(x => x.RiskFactor).ToList();

				var listpac = results.Select(t => t.PacId).ToList();
				IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();
				results.ForEach(t =>
				{
					var site = listPacSite.FirstOrDefault(g => g.PACID == t.PacId);
					if (site != null)
					{
						t.SiteKey = site.siteKey;
						var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
						t.SiteName = sitedb != null ? sitedb.Name : "";
					}
				});
				#endregion
			}
			else
			{
				#region Group By Date
				var grData = queryem.Select(x => new
				{
					TransID = x.TransID,
					T_PACID = x.PACID_ID,
					T_StoreID = x.T_StoreID ?? 0,
					//Store_Name = x.tbl_POS_StoreList.Store_Name,
					T_OperatorID = x.T_OperatorID,
					//EmpName = x.tbl_POS_OperatorList.Operator_Name,
					T_6TotalAmount = x.T_6TotalAmount ?? 0,
					Date = x.TransDate.Value.Date
				});
				query = grData.GroupBy(x => x.Date).Select(gr => new TransInfoSite()
				{
					Date = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0,
					EmployerId = gr.Any() ? gr.FirstOrDefault().T_OperatorID ?? 0 : 0,
					TotalAmmount = gr.Any() ? (decimal)gr.Sum(x => x.T_6TotalAmount) : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.Date)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltrans = querytransac.GroupBy(x => x.TransDate.HasValue ? x.TransDate.Value.Date : DateTime.MinValue).Select(gr => new { TransDate = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltrans, l => l.Date, r => r.TransDate, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor,
					Date = l.Date,
					EmployerId = l.EmployerId,
					TotalAmmount = l.TotalAmmount
				});

				results = res_cmp.Select(w => new TransInfoSite()
				{
					PacId = w.PacId,
					PercentToSale = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					Date = w.Date,
					EmployerId = w.EmployerId,
					TotalAmmount = w.TotalAmmount,
					TotalTran = w.Count
				}).OrderBy(x=>x.Date).ToList();
				#endregion
			}

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			return new SiteExceptionTransPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				//SumRiskFactors = totalCount != 0 ? query.Sum(item => item.RiskFactor!=null ? (int)item.RiskFactor : 0 ) : 0,
				Data = results
			};
		}
		public SiteExceptionCustsPagings GetCustsWOTran(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			string strpacids = string.Join(",", pacIds);
			List<Proc_Exception_CustsWOT_Result> queryem = DataService.GetCustsWOTran(strpacids, sDate.StartOfDay(), eDate.EndOfDay()).ToList();
			List<int> validPACID = queryem.Select(x => x.PACID_ID).Distinct().ToList();

			var kdvr_pacid = listPacSite.Where(x => validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, PACID = x.PACID }).Distinct().ToList();
			List<int> lsKDVR = kdvr_pacid.Select(x => x.KDVR ?? 0).Distinct().ToList();
			var querytraffno = DataService.GetIOPCTraffics().Where(t => lsKDVR.Contains((int)t.tbl_IOPC_TrafficCountRegion.T_PACID) && t.RegionEnterTime >= sDate && t.RegionEnterTime <= eDate)
				.Select(x => new { T_PACID = x.tbl_IOPC_TrafficCountRegion.T_PACID, PersonID = x.PersonID, DVRDate = x.RegionEnterTime }).ToList();

			var querytraffic = querytraffno.Join(kdvr_pacid, t => t.T_PACID, d => d.KDVR, (t, d) => new { T_PACID = d.PACID, DVRDate = t.DVRDate });

			IEnumerable<CustomersInfoSite> query = null;
			List<CustomersInfoSite> results = null;
			int totalCount = 0;
			int totalPages = 0;

			if (param.GroupBy == (int)GroupBy.SITE)
			{
				#region Group By Site
				var grData = queryem.Select(x => new { PersonID = x.PersonID, T_PACID = x.PACID_ID });
				query = grData.GroupBy(x => x.T_PACID).Select(gr => new CustomersInfoSite()
				{
					PacId = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.PacId)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltraffics = querytraffic.GroupBy(x => x.T_PACID).Select(gr => new { T_PACID = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltraffics, l => l.PacId, r => r.T_PACID, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor
				});

				var kdvr_site = listPacSite.Where(x => validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, SiteKey = x.siteKey }).Distinct().ToList();
				var site_data = res_cmp.Join(kdvr_site, c => c.PacId, s => s.KDVR, (c, s) => new
				{
					PacId = c.PacId,
					Count = c.Count,
					RiskFactor = c.RiskFactor,
					SiteKey = s.SiteKey
				});
				var group_site = site_data.GroupBy(x => x.SiteKey).Select(gr => new
				{
					PacId = gr.Any() ? gr.FirstOrDefault().PacId : 0,
					Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
					RiskFactor = gr.Any() ? gr.Sum(x => x.RiskFactor) : 0,
					SiteKey = gr.Key
				});

				results = group_site.Select(w => new CustomersInfoSite()
				{
					PacId = w.PacId,
					Percent = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					Total = w.Count
				}).OrderByDescending(x=>x.RiskFactor).ToList();

				var listpac = results.Select(t => t.PacId).ToList();
				IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();
				results.ForEach(t =>
				{
					var site = listPacSite.FirstOrDefault(g => g.siteKey == t.SiteKey);
					if (site != null)
					{
						t.SiteKey = site.siteKey;
						var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
						t.SiteName = sitedb != null ? sitedb.Name : "";
					}
				});
				#endregion
			}
			else //if (param.GroupBy == (int)GroupBy.DATE)
			{
				#region Group By Date
				var grData = queryem.Select(x => new
				{
					PersonID = x.PersonID,
					T_PACID = x.PACID_ID,
					Date = x.RegionEnterTime ?? DateTime.MinValue
				});
				query = grData.GroupBy(x => x.Date.Date).Select(gr => new CustomersInfoSite()
				{
					Date = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.Date)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltrans = querytraffic.GroupBy(x => x.DVRDate.Date).Select(gr => new { Date = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltrans, l => l.Date, r => r.Date, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor,
					Date = l.Date
				});

				results = res_cmp.Select(w => new CustomersInfoSite()
				{
					PacId = w.PacId,
					Percent = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					Date = w.Date,
					Total = w.Count
				}).OrderBy(x=>x.Date).ToList();
				#endregion
			}

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			return new SiteExceptionCustsPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results
			};
		}
		public ExceptionCustomerPagings GetIOPCCustomerViewer(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			string strpacids = string.Join(",", pacIds);
			List<Proc_Exception_CustsWOT_Result> queryem = DataService.GetCustsWOTran(strpacids, sDate.StartOfDay(), eDate.EndOfDay()).ToList();

			int totalCount = 0;
			int totalPages = 0;

			totalCount = queryem.Count();
			var query = queryem.Take(totalCount);
			totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			query = query
					.OrderBy(x => x.RegionEnterTime)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

			List<int> lsRegIdxs = query.Select(x => x.RegionIndex ?? 0).Distinct().ToList();
			List<tbl_IOPC_TrafficCountRegion> regionName = DataService.GetRegionName(lsRegIdxs).ToList();

			List<ExceptionCustomerInfo> results = query.Join(regionName, x => x.RegionIndex, r => r.RegionIndex, (x, r) => new ExceptionCustomerInfo()
					{
						DVRDate = x.RegionEnterTime.HasValue ? x.RegionEnterTime.Value.Date : DateTime.MinValue,
						PACID = x.PACID_ID,
						PersonID = x.PersonID.ToString(),
						RegionIndex = x.RegionIndex ?? 0,
						RegionName = r.tbl_IOPC_TrafficCountRegionName == null ? string.Empty : r.tbl_IOPC_TrafficCountRegionName.RegionName,
						StartDate = x.RegionEnterTime ?? DateTime.MinValue,
						EndDate = x.RegionExitTime ?? DateTime.MinValue,
						ExternalCamera = x.ExternalChannel ?? -1
					}).OrderBy(x=>x.StartDate).ToList();
				
				//.Select(x => new ExceptionCustomerInfo() 
				//{
				//	DVRDate = x.RegionEnterTime.HasValue ? x.RegionEnterTime.Value.Date : DateTime.MinValue,
				//	PACID = x.PACID_ID,
				//	PersonID = x.PersonID.ToString(),
				//	RegionIndex = x.RegionIndex ?? 0,
				//	StartDate = x.RegionEnterTime ?? DateTime.MinValue,
				//	EndDate = x.RegionExitTime ?? DateTime.MinValue,
				//	ExternalCamera = x.ExternalChannel ?? -1
				//}).OrderBy(x=>x.StartDate).ToList();

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			return new ExceptionCustomerPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results
			};
		}

		public SiteExceptionCustsPagings GetCarsWOTran(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			string strpacids = string.Join(",", pacIds);
			var queryem = DataService.GetCarsWOTran(strpacids, sDate.StartOfDay(), eDate.EndOfDay()).ToList();
			List<int> validPACID = queryem.Select(x => x.PACID_ID).Distinct().ToList();

			var kdvr_pacid = listPacSite.Where(x=>validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, PACID = x.PACID }).Distinct().ToList();
			List<int> lsKDVR = kdvr_pacid.Select(x => x.KDVR ?? 0).Distinct().ToList();
			List<tbl_IOPC_DriveThrough> querydtno = DataService.GetIOPCDriveThroughs().Where(t => lsKDVR.Contains((int)t.T_PACID) && t.StartDate >= sDate && t.StartDate <= eDate).ToList();
			var querydt = querydtno.Join(kdvr_pacid, t => t.T_PACID, d => d.KDVR, (t, d) => new { T_PACID = d.PACID, StartDate = t.StartDate });

			IEnumerable<CustomersInfoSite> query = null;
			List<CustomersInfoSite> results = null;
			int totalCount = 0;
			int totalPages = 0;

			if (param.GroupBy == (int)GroupBy.SITE)
			{
				#region Group By Site
				var grData = queryem.Select(x => new { StartDate = x.StartDate, T_PACID = x.PACID_ID });
				query = grData.GroupBy(x => x.T_PACID).Select(gr => new CustomersInfoSite()
				{
					PacId = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.PacId)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltraffics = querydt.GroupBy(x => x.T_PACID).Select(gr => new { T_PACID = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltraffics, l => l.PacId, r => r.T_PACID, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor
				});

				var kdvr_site = listPacSite.Where(x => validPACID.Contains(x.PACID ?? 0)).Select(x => new { KDVR = x.KDVR, SiteKey = x.siteKey }).Distinct().ToList();
				var site_data = res_cmp.Join(kdvr_site, c => c.PacId, s => s.KDVR, (c, s) => new
				{
					PacId = c.PacId,
					Count = c.Count,
					RiskFactor = c.RiskFactor,
					SiteKey = s.SiteKey
				});
				var group_site = site_data.GroupBy(x => x.SiteKey).Select(gr => new
				{
					PacId = gr.Any() ? gr.FirstOrDefault().PacId : 0,
					Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
					RiskFactor = gr.Any() ? gr.Sum(x => x.RiskFactor) : 0,
					SiteKey = gr.Key
				});

				results = group_site.Select(w => new CustomersInfoSite()
				{
					PacId = w.PacId,
					Percent = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					Total = w.Count
				}).OrderByDescending(x=>x.RiskFactor).ToList();
				var listpac = results.Select(t => t.PacId).ToList();
				IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();
				results.ForEach(t =>
				{
					var site = listPacSite.FirstOrDefault(g => g.siteKey == t.SiteKey);
					if (site != null)
					{
						t.SiteKey = site.siteKey;
						var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
						t.SiteName = sitedb != null ? sitedb.Name : "";
					}
				});
				#endregion
			}
			else //if (param.GroupBy == (int)GroupBy.DATE)
			{
				#region Group By Date
				var grData = queryem.Select(x => new
				{
					StartDate = x.StartDate,
					T_PACID = x.PACID_ID,
					Date = x.StartDate.Value.Date
				});
				query = grData.GroupBy(x => x.Date).Select(gr => new CustomersInfoSite()
				{
					Date = gr.Key,
					RiskFactor = gr.Any() ? gr.Count() : 0
				});
				totalCount = query.Count();
				query = query.Take(totalCount);
				totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

				query = query
					.OrderBy(x => x.Date)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

				var alltrans = querydt.GroupBy(x => x.StartDate.HasValue ? x.StartDate.Value.Date : DateTime.MinValue).Select(gr => new { Date = gr.Key, Count = gr.Count() });
				var res_cmp = query.Join(alltrans, l => l.Date, r => r.Date, (l, r) => new
				{
					PacId = l.PacId,
					Count = r.Count,
					RiskFactor = l.RiskFactor,
					Date = l.Date
				});

				results = res_cmp.Select(w => new CustomersInfoSite()
				{
					PacId = w.PacId,
					Percent = w.Count == 0 ? w.Count : w.RiskFactor * 100 / w.Count,
					RiskFactor = w.RiskFactor,
					Date = w.Date,
					Total = w.Count
				}).OrderBy(x => x.Date).ToList();
				#endregion
			}

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			return new SiteExceptionCustsPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results.ToList()
			};
		}
		public ExceptionCarPagings GetIOPCCarViewer(UserContext usercontext, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate;
			DateTime eDate = param.EndTranDate;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			string strpacids = string.Join(",", pacIds);
			List<Proc_Exception_CarsWOT_Result> queryem = DataService.GetCarsWOTran(strpacids, sDate.StartOfDay(), eDate.EndOfDay()).ToList();

			int totalCount = 0;
			int totalPages = 0;

			totalCount = queryem.Count();
			var query = queryem.Take(totalCount);
			totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			query = query
					.OrderBy(x => x.TD_ID)
					.Skip(param.PageSize * (param.PageNo - 1))
					.Take(param.PageSize);

			List<ExceptionCarInfo> results = query.Select(x => new ExceptionCarInfo()
			{
				PACID = x.PACID_ID,
				ExternalCamera = x.ExternalChannel ?? -1,
				//InternalCamera = x.RegionIndex ?? 0,
				StartDate = x.StartDate ?? DateTime.MinValue,
				EndDate = x.EndDate ?? DateTime.MinValue
			}).OrderBy(x=>x.StartDate).ToList();

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			return new ExceptionCarPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results
			};
		}

		public List<EmployerRebarModel> GetDataforEmp(List<EmployerPagings> emps, EmployerParamModel param, List<UserSiteDvrChannel> listPacSite)
		{

			var empids = emps.Select(h => h.Id).Distinct().Cast<int?>().ToList();
			var pacids = emps.Select(h => h.PACID).Distinct().Cast<int?>().ToList();

			DateTime sDate = Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = Convert.ToDateTime(param.EndTranDate);

			var queryem = DataService.GetMetricRebar()
				.Include(t => t.tbl_POS_OperatorList)
				.Include(t => t.tbl_POS_StoreList)
				.AsNoTracking()
				.Where(t => pacids.Contains(t.T_PACID) && empids.Contains(t.T_OperatorID) && t.TransDate >= sDate && t.TransDate <= eDate);
			var querytypetran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type).AsNoTracking().Where(t => param.Types.Contains(t.TypeID));
			var query = from tr in queryem
						join ty in querytypetran on tr.TransID equals ty.TransID
						select
							new
							{
								tr.TransID,
								tr.T_PACID,
								tr.T_StoreID,
								Store_Name = tr.tbl_POS_StoreList.Store_Name,
								tr.T_OperatorID,
								EmpName = tr.tbl_POS_OperatorList.Operator_Name,
								tr.T_6TotalAmount,
								ty.TypeID,
								ty.tbl_Exception_Type.TypeName,
								ty.tbl_Exception_Type.Color,
								ty.tbl_Exception_Type.TypeWeight
							}
							into g
							group g by
								new { g.T_PACID, g.T_StoreID, g.Store_Name, g.T_OperatorID, g.EmpName, g.TypeID, g.TypeName, g.Color, g.TypeWeight }
								into f
								select new EmployerRebarModel()
								{
									EmployerId = f.Key.T_OperatorID,
									EmployerName = f.Key.EmpName,
									PacId = f.Key.T_PACID,
									StoreId = f.Key.T_StoreID,
									StoreName = f.Key.Store_Name,
									TypeName = f.Key.TypeName,
									TypeId = f.Key.TypeID,
									Color = f.Key.Color,
									Weight = f.Key.TypeWeight,
									Value = f.Count() // (decimal)f.Sum(x => x.T_6TotalAmount)
								};
			var results = query.ToList();

			//IEnumerable<CMSWebSiteModel> siteModel = ISiteSvc.GetSites<CMSWebSiteModel>(param.SiteKeys, item => new CMSWebSiteModel { ID = item.siteKey, Name = item.ServerID }, null).Distinct().ToList();

			//results.ForEach(t =>
			//{
			//	var site = listPacSite.FirstOrDefault(g => g.PACID == t.PacId);
			//	if (site != null)
			//	{
			//		t.SiteKey = site.siteKey;
			//		var sitedb = siteModel.FirstOrDefault(g => g.ID == t.SiteKey);
			//		t.SiteName = sitedb != null ? sitedb.Name : "";
			//	}
			//});

			return results;
		}

		public IQueryable<TransactionViewerModel> GetPOSTransactionViewer(HttpRequestMessage request, UserContext usercontext, List<int> keys = null, List<int> flags = null, List<int> pays = null, List<int> taxs = null)
		{
			var queryem = DataService.GetPosTransactions()
				.Include(t => t.tbl_POS_CameraNBList)
				.Include(t => t.tbl_POS_CardIDList)
				.Include(t => t.tbl_POS_CheckIDList)
				.Include(t => t.tbl_POS_OperatorList)
				.Include(t => t.tbl_POS_RegisterList)
				.Include(t => t.tbl_POS_ShiftList)
				.Include(t => t.tbl_POS_StoreList)
				.Include(t => t.tbl_POS_TerminalList);

			IQueryable<tbl_POS_Transact> query = queryem;

			var queryemflagtran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type);

			if (flags != null)
			{
				queryemflagtran = queryemflagtran.Where(t => flags.Contains(t.TypeID));
				query = from tran in query
						join flag in queryemflagtran on tran.TransID equals flag.TransID
						group tran by tran into tra
						select tra.Key;
			}
			
			var querypayment = DataService.GetPosTransactionPayments().Include(t => t.tbl_POS_PaymentList);
			var filterPaymentAmmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterPaymentAmount");
			var hasPaymentfilter = false;
			if (!string.IsNullOrEmpty(filterPaymentAmmount))
			{
				querypayment = querypayment.Where(filterPaymentAmmount);
				hasPaymentfilter = true;
			}

			if (pays != null)
			{
				querypayment = querypayment.Where(f => pays.Contains(f.PaymentID));
				hasPaymentfilter = true;
			}

			if (hasPaymentfilter)
			{
				query = from tran in query
						join pay in querypayment on tran.TransID equals pay.TransID
						group tran by tran into tra
						select tra.Key;
			}


			var filterTaxAmmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterTaxAmount");
			var querytaxts = DataService.GetPosTransactionTaxes().Include(t => t.tbl_POS_TaxesList);
			var hasTaxfilter = false;
			if (!string.IsNullOrEmpty(filterTaxAmmount))
			{
				querytaxts = querytaxts.Where(filterTaxAmmount);
				hasTaxfilter = true;
			}
			
			if (taxs != null)
			{
				querytaxts = querytaxts.Where(f => taxs.Contains(f.TaxID));
				hasTaxfilter = true;
				
			}
			if (hasTaxfilter)
			{
				query = from tran in query
						join tax in querytaxts on tran.TransID equals tax.TransID
						group tran by tran into tra
						select tra.Key;
			}

			var itemCodeId = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("ItemCodeId");
			var descId = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("DescId");
			var filterQty = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterQty");
			var filterAmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterAmount");
			var detailfilter = false;
			var tranDetails = DataService.GetPosTransactionsDetail().Select(t => new { ItemCodeId = t.R_ItemCode, Qty = t.R_1Qty, Amount = t.R_0Amount, DescId = t.R_Description, TransID = t.TransID });
			if (!string.IsNullOrEmpty(filterQty))
			{
				tranDetails = tranDetails.Where(filterQty);
				detailfilter = true;
			}

			if (!string.IsNullOrEmpty(filterAmount))
			{
				tranDetails = tranDetails.Where(filterAmount);
				detailfilter = true;
			}

			if (!string.IsNullOrEmpty(itemCodeId))
			{
				var listItemCode = itemCodeId.Split(',').Select(Int32.Parse).ToList();
				if (listItemCode.Any())
				{
					tranDetails = tranDetails.Where(t => listItemCode.Contains((int)t.ItemCodeId));
					detailfilter = true;
				}
			}

			if (!string.IsNullOrEmpty(descId))
			{
				var listItem = descId.Split(',').Select(Int32.Parse).ToList();
				if (listItem.Any())
				{
					tranDetails = tranDetails.Where(t => listItem.Contains((int)t.DescId));
					detailfilter = true;
				}
			}

			if (detailfilter)
			{
				query = from tran in query
						join detail in tranDetails on tran.TransID equals detail.TransID
						group tran by tran into tra
						select tra.Key;
			}

			var queryemnotetran = DataService.GetTransExceptionNotes();

			if (keys != null && keys.Count > 0)
			{
				var listPacSite = getPacIds(usercontext, keys);
				var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>().ToList();
				query = query.Where(t => pacIds.Contains(t.T_PACID));
			}

			return query.Select(t => new TransactionViewerModel()
			{
				CamId = t.T_CameraNB,
				CamName = t.tbl_POS_CameraNBList != null ? t.tbl_POS_CameraNBList.CameraNB_Name : "",
				CardId = t.T_CardID,
				CardName = t.tbl_POS_CardIDList != null ? t.tbl_POS_CardIDList.CardID_Name : "",
				ChangeAmount = t.T_8ChangeAmount,
				CheckId = t.T_CheckID,
				CheckName = t.tbl_POS_CheckIDList != null ? t.tbl_POS_CheckIDList.CheckID_Name : "",
				DvrDate = t.DVRDate.HasValue ? t.DVRDate.Value : new DateTime(),
				Year = t.TransDate.HasValue ? SqlFunctions.DatePart("year", t.TransDate.Value) : -1,
				Quarter = t.TransDate.HasValue ? SqlFunctions.DatePart("quarter", t.TransDate.Value) : -1,
				Month = t.TransDate.HasValue ? SqlFunctions.DatePart("month", t.TransDate.Value) : -1,
				Day = t.TransDate.HasValue ? SqlFunctions.DatePart("day", t.TransDate.Value) : -1,
				Week = t.TransDate.HasValue ? SqlFunctions.DatePart("week", t.TransDate.Value) : -1,
				Hour = t.TransDate.HasValue ? SqlFunctions.DatePart("hour", t.TransDate) : -1,
				EmployeeId = t.T_OperatorID,
				EmployeeName = t.tbl_POS_OperatorList != null ? t.tbl_POS_OperatorList.Operator_Name : "",
				ExceptionTypes = queryemflagtran.Where(f => f.TransID == t.TransID).Select(f => new TranExceptionType()
				{
					Id = f.tbl_Exception_Type.TypeID,
					Color = f.tbl_Exception_Type.Color,
					TypeWeight = f.tbl_Exception_Type.TypeWeight,
					Name = f.tbl_Exception_Type.TypeName
				}),
				Notes = queryemnotetran.Where(f => f.TransID == t.TransID).Select(f => new ExceptionNotes()
				{
					TranId = f.TransID,
					DateNotes = f.CreatedDate,
					UserId = f.UserID,
					Note = f.TransNote
				}),
				PacId = t.T_PACID,
				Payments = querypayment.Where(f => f.TransID == t.TransID && f.tbl_POS_PaymentList != null).Select(f => new TranPayment()
				{
					Id = f.tbl_POS_PaymentList.PaymentID,
					Name = f.tbl_POS_PaymentList.PaymentName,
					Ammount = f.PaymentAmount
				}),
				RegisterId = t.T_RegisterID,
				RegisterName = t.tbl_POS_RegisterList != null ? t.tbl_POS_RegisterList.Register_Name : "",
				ShiftId = t.T_ShiftID,
				ShiftName = t.tbl_POS_ShiftList != null ? t.tbl_POS_ShiftList.Shift_Name : "",
				StoreId = t.T_StoreID,
				StoreName = t.tbl_POS_StoreList != null ? t.tbl_POS_StoreList.Store_Name : "",
				SubTotal = t.T_1SubTotal,
				Taxs = querytaxts.Where(f => f.TransID == t.TransID && f.tbl_POS_TaxesList != null).Select(f => new TranTax()
				{
					Id = f.tbl_POS_TaxesList.TaxID,
					Name = f.tbl_POS_TaxesList.TaxName,
					Ammount = f.TaxAmount
				}),
				TerminalId = t.T_TerminalID,
				TerminalName = t.tbl_POS_TerminalList != null ? t.tbl_POS_TerminalList.Terminal_Name : "",
				Total = t.T_6TotalAmount ?? 0,
				TranDate = t.TransDate,
				TranId = t.TransID,
				TranNo = t.T_0TransNB
			});
		}

		public IQueryable<TransactionDetailViewerModel> GetAhocViewer(UserContext usercontext, HttpRequestMessage request, List<int> keys = null)
		{
			List<int> flags = null, taxs = null, pays = null;
			var flaglist = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("flags");
			if (!string.IsNullOrEmpty(flaglist))
			{
				flags = flaglist.Split(',').Select(Int32.Parse).ToList();
			}

			var taxslist = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("taxs");
			if (!string.IsNullOrEmpty(taxslist))
			{
				taxs = taxslist.Split(',').Select(Int32.Parse).ToList();
			}

			var payslist = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("pays");
			if (!string.IsNullOrEmpty(payslist))
			{
				pays = payslist.Split(',').Select(Int32.Parse).ToList();
			}

			var itemCodeId = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("ItemCodeId");
			var descId = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("DescId");
			var filterQty = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterQty");
			var filterAmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterAmount");

			var queryem = DataService.GetPosTransactionsDetail()
				//.Include(t => t.tbl_POS_Transact)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_CameraNBList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_CardIDList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_CheckIDList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_OperatorList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_RegisterList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_ShiftList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_StoreList)
				//.Include(t => t.tbl_POS_Transact.tbl_POS_TerminalList)
				.Include(t => t.tbl_POS_DescriptionList)
				.Include(t => t.tbl_POS_ItemCodeList)
				.Include(t => t.tbl_POS_RetailExtraNumber)
				.Include(t => t.tbl_POS_RetailExtraString);

			IQueryable<tbl_POS_Retail> query = queryem;

			var queryemflagtran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type);

			if (flags != null)
			{
				queryemflagtran = queryemflagtran.Where(t => flags.Contains(t.TypeID));
				query = from tran in query
						join flag in queryemflagtran on tran.TransID equals flag.TransID
						group tran by tran into tra
						select tra.Key;
			}

			var querypayment = DataService.GetPosTransactionPayments().Include(t => t.tbl_POS_PaymentList);
			var filterPaymentAmmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterPaymentAmount");
			var hasPaymentfilter = false;
			if (!string.IsNullOrEmpty(filterPaymentAmmount))
			{
				querypayment = querypayment.Where(filterPaymentAmmount);
				hasPaymentfilter = true;
			}

			if (pays != null)
			{
				querypayment = querypayment.Where(f => pays.Contains(f.PaymentID));
				hasPaymentfilter = true;
			}

			if (hasPaymentfilter)
			{
				query = from tran in query
						join pay in querypayment on tran.TransID equals pay.TransID
						group tran by tran into tra
						select tra.Key;
			}

			var filterTaxAmmount = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("FilterTaxAmount");
			var querytaxts = DataService.GetPosTransactionTaxes().Include(t => t.tbl_POS_TaxesList);
			var hasTaxfilter = false;
			if (!string.IsNullOrEmpty(filterTaxAmmount))
			{
				querytaxts = querytaxts.Where(filterTaxAmmount);
				hasTaxfilter = true;
			}
			
			if (taxs != null)
			{
				querytaxts = querytaxts.Where(f => taxs.Contains(f.TaxID));
				hasTaxfilter = true;
				
			}
			if (hasTaxfilter)
			{
				query = from tran in query
						join tax in querytaxts on tran.TransID equals tax.TransID
						group tran by tran into tra
						select tra.Key;
			}

			var queryemnotetran = DataService.GetTransExceptionNotes();

			if (keys != null && keys.Count > 0)
			{
				var listPacSite = getPacIds(usercontext, keys);
				var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>().ToList();
				query = query.Where(t => pacIds.Contains(t.tbl_POS_Transact.T_PACID));
			}

			if (!string.IsNullOrEmpty(itemCodeId))
			{
				var listItemCode = itemCodeId.Split(',').Select(Int32.Parse).ToList();
				if (listItemCode.Any())
				{
					query = query.Where(t => listItemCode.Contains((int)t.R_ItemCode));
				}
			}

			if (!string.IsNullOrEmpty(descId))
			{
				var listItem = descId.Split(',').Select(Int32.Parse).ToList();
				if (listItem.Any())
				{
					query = query.Where(t => listItem.Contains((int)t.R_Description));
				}
			}

			var resultquery = query.Select(t => new TransactionDetailViewerModel()
			{
				CamId = t.tbl_POS_Transact.T_CameraNB,
				CamName = t.tbl_POS_Transact.tbl_POS_CameraNBList != null ? t.tbl_POS_Transact.tbl_POS_CameraNBList.CameraNB_Name : "",
				CardId = t.tbl_POS_Transact.T_CardID,
				CardName = t.tbl_POS_Transact.tbl_POS_CardIDList != null ? t.tbl_POS_Transact.tbl_POS_CardIDList.CardID_Name : "",
				ChangeAmount = t.tbl_POS_Transact.T_8ChangeAmount,
				CheckId = t.tbl_POS_Transact.T_CheckID,
				CheckName = t.tbl_POS_Transact.tbl_POS_CheckIDList != null ? t.tbl_POS_Transact.tbl_POS_CheckIDList.CheckID_Name : "",
				DvrDate = t.tbl_POS_Transact.DVRDate.HasValue ? t.tbl_POS_Transact.DVRDate.Value : new DateTime(),
				Year = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("year", t.tbl_POS_Transact.TransDate.Value) : -1,
				Quarter = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("quarter", t.tbl_POS_Transact.TransDate.Value) : -1,
				Month = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("month", t.tbl_POS_Transact.TransDate.Value) : -1,
				Day = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("day", t.tbl_POS_Transact.TransDate.Value) : -1,
				Week = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("week", t.tbl_POS_Transact.TransDate.Value) : -1,
				Hour = t.tbl_POS_Transact.TransDate.HasValue ? SqlFunctions.DatePart("hour", t.tbl_POS_Transact.TransDate) : -1,
				EmployeeId = t.tbl_POS_Transact.T_OperatorID,
				EmployeeName = t.tbl_POS_Transact.tbl_POS_OperatorList != null ? t.tbl_POS_Transact.tbl_POS_OperatorList.Operator_Name : "",
				ExceptionTypes = queryemflagtran.Where(f => f.TransID == t.TransID).Select(f => new TranExceptionType()
				{
					Id = f.tbl_Exception_Type.TypeID,
					Color = f.tbl_Exception_Type.Color,
					TypeWeight = f.tbl_Exception_Type.TypeWeight,
					Name = f.tbl_Exception_Type.TypeName
				}),
				Notes = queryemnotetran.Where(f => f.TransID == t.TransID).Select(f => new ExceptionNotes()
				{
					TranId = f.TransID,
					DateNotes = f.CreatedDate,
					UserId = f.UserID,
					Note = f.TransNote
				}),
				PacId = t.tbl_POS_Transact.T_PACID,
				Payments = querypayment.Where(f => f.TransID == t.TransID && f.tbl_POS_PaymentList != null).Select(f => new TranPayment()
				{
					Id = f.tbl_POS_PaymentList.PaymentID,
					Name = f.tbl_POS_PaymentList.PaymentName,
					Ammount = f.PaymentAmount
				}),
				RegisterId = t.tbl_POS_Transact.T_RegisterID,
				RegisterName = t.tbl_POS_Transact.tbl_POS_RegisterList != null ? t.tbl_POS_Transact.tbl_POS_RegisterList.Register_Name : "",
				ShiftId = t.tbl_POS_Transact.T_ShiftID,
				ShiftName = t.tbl_POS_Transact.tbl_POS_ShiftList != null ? t.tbl_POS_Transact.tbl_POS_ShiftList.Shift_Name : "",
				StoreId = t.tbl_POS_Transact.T_StoreID,
				StoreName = t.tbl_POS_Transact.tbl_POS_StoreList != null ? t.tbl_POS_Transact.tbl_POS_StoreList.Store_Name : "",
				SubTotal = t.tbl_POS_Transact.T_1SubTotal,
				Taxs = querytaxts.Where(f => f.TransID == t.TransID && f.tbl_POS_TaxesList != null).Select(f => new TranTax()
				{
					Id = f.tbl_POS_TaxesList.TaxID,
					Name = f.tbl_POS_TaxesList.TaxName,
					Ammount = f.TaxAmount
				}),
				TerminalId = t.tbl_POS_Transact.T_TerminalID,
				TerminalName = t.tbl_POS_Transact.tbl_POS_TerminalList != null ? t.tbl_POS_Transact.tbl_POS_TerminalList.Terminal_Name : "",
				Total = t.tbl_POS_Transact.T_6TotalAmount ?? 0,
				TranDate = t.tbl_POS_Transact.TransDate,
				TranId = t.tbl_POS_Transact.TransID,
				TranNo = t.tbl_POS_Transact.T_0TransNB,
				Amount = t.R_0Amount,
				DescId = t.R_Description,
				DescName = t.tbl_POS_DescriptionList != null ? t.tbl_POS_DescriptionList.Description_Name : "",
				ItemCodeId = t.R_ItemCode,
				ItemCode = t.tbl_POS_ItemCodeList != null? t.tbl_POS_ItemCodeList.ItemCode_Name: "",
				LineNo = t.R_2ItemLineNb,
				Qty = t.R_1Qty,
				RetailId = t.RetailID,
				TOBox = t.R_TOBox
			});

		
			if (!string.IsNullOrEmpty(filterQty))
			{
				resultquery = resultquery.Where(filterQty);
			}

			if (!string.IsNullOrEmpty(filterAmount))
			{
				resultquery = resultquery.Where(filterAmount);
			}

			return resultquery;
		}

		public Expression<Func<TItem, dynamic>> GroupByExpression<TItem>(string[] propertyNames)
		{
			var properties = propertyNames.Select(name => typeof(TItem).GetProperty(name)).ToArray();
			var propertyTypes = properties.Select(p => p.PropertyType).ToArray();
			var tupleTypeDefinition = typeof(Tuple).Assembly.GetType("System.Tuple`" + properties.Length);
			var tupleType = tupleTypeDefinition.MakeGenericType(propertyTypes);
			var constructor = tupleType.GetConstructor(propertyTypes);
			var param = Expression.Parameter(typeof(TItem), "item");
			var body = Expression.New(constructor, properties.Select(p => Expression.Property(param, p)));
			var expr = Expression.Lambda<Func<TItem, dynamic>>(body, param);
			return expr;
		}  

		private static Expression<Func<T, string>> GetGroupKey<T>(string property)
		{
			var parameter = Expression.Parameter(typeof(T));
			var body = Expression.Property(parameter, property);
			return Expression.Lambda<Func<T, string>>(body, parameter);
		}

		public static Expression<Func<TElement, object>> GetColumnName<TElement>(string property)
		{
			var menu = Expression.Parameter(typeof(TElement), "groupCol");
			var menuProperty = Expression.PropertyOrField(menu, property);
			var lambda = Expression.Lambda<Func<TElement, object>>(menuProperty, menu);
			return lambda;
		}

		public IQueryable<TransactionViewerModel> GetTransactionViewer(UserContext usercontext, List<int> keys = null)
		{

			var queryem = DataService.GetMetricRebar()
				.Include(t => t.tbl_POS_CameraNBList)
				.Include(t => t.tbl_POS_CardIDList)
				.Include(t => t.tbl_POS_CheckIDList)
				.Include(t => t.tbl_POS_OperatorList)
				.Include(t => t.tbl_POS_RegisterList)
				.Include(t => t.tbl_POS_ShiftList)
				.Include(t => t.tbl_POS_StoreList)
				.Include(t => t.tbl_POS_TerminalList)
				.Include(t => t.tbl_Exception_FlaggedTrans)
				.Include(t => t.tbl_Exception_FlaggedTrans.Select(f => f.tbl_Exception_Type))
				.Include(t => t.tbl_Exception_Notes)
				.Include(t => t.tbl_Exception_TransPayment)
				.Include(t => t.tbl_Exception_TransPayment.Select(f => f.tbl_POS_PaymentList))
				.Include(t => t.tbl_Exception_TransTaxes)
				.Include(t => t.tbl_Exception_TransTaxes.Select(f => f.tbl_POS_TaxesList));

			if (keys != null && keys.Count > 0)
			{
				var listPacSite = getPacIds(usercontext, keys);
				var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>().ToList();
				queryem = queryem.Where(t => pacIds.Contains(t.T_PACID));
			}

			return queryem.Select(t => new TransactionViewerModel()
			{
				CamId = t.T_CameraNB,
				CamName = t.tbl_POS_CameraNBList != null ? t.tbl_POS_CameraNBList.CameraNB_Name : "",
				CardId = t.T_CardID,
				CardName = t.tbl_POS_CardIDList != null ? t.tbl_POS_CardIDList.CardID_Name : "",
				ChangeAmount = t.T_8ChangeAmount,
				CheckId = t.T_CheckID,
				CheckName = t.tbl_POS_CheckIDList != null ? t.tbl_POS_CheckIDList.CheckID_Name : "",
				DvrDate = t.DVRDate.HasValue ? t.DVRDate.Value : new DateTime(),
				Year = t.TransDate.HasValue ? SqlFunctions.DatePart("year", t.TransDate.Value) : -1,
				Quarter = t.TransDate.HasValue ? SqlFunctions.DatePart("quarter", t.TransDate.Value) : -1,
				Month = t.TransDate.HasValue ? SqlFunctions.DatePart("month", t.TransDate.Value) : -1,
				Day = t.TransDate.HasValue ? SqlFunctions.DatePart("day", t.TransDate.Value) : -1,
				Week = t.TransDate.HasValue ? SqlFunctions.DatePart("week", t.TransDate.Value) : -1,
				Hour = t.TransDate.HasValue ? SqlFunctions.DatePart("hour", t.TransDate) : -1,
				EmployeeId = t.T_OperatorID,
				EmployeeName = t.tbl_POS_OperatorList != null ? t.tbl_POS_OperatorList.Operator_Name : "",
				ExceptionTypes = t.tbl_Exception_FlaggedTrans.Where(f => f.tbl_Exception_Type != null).Select(f => new TranExceptionType()
				{
					Id = f.tbl_Exception_Type.TypeID,
					Color = f.tbl_Exception_Type.Color,
					TypeWeight = f.tbl_Exception_Type.TypeWeight,
					Name = f.tbl_Exception_Type.TypeName
				}),
				Notes = t.tbl_Exception_Notes.Select(f => new ExceptionNotes()
				{
					TranId = f.TransID,
					DateNotes = f.CreatedDate,
					UserId = f.UserID,
					Note = f.TransNote
				}),
				PacId = t.T_PACID,
				Payments = t.tbl_Exception_TransPayment.Where(f => f.tbl_POS_PaymentList != null).Select(f => new TranPayment()
				{
					Id = f.tbl_POS_PaymentList.PaymentID,
					Name = f.tbl_POS_PaymentList.PaymentName,
					Ammount = f.PaymentAmount
				}),
				RegisterId = t.T_RegisterID,
				RegisterName = t.tbl_POS_RegisterList != null ? t.tbl_POS_RegisterList.Register_Name : "",
				ShiftId = t.T_ShiftID,
				ShiftName = t.tbl_POS_ShiftList != null ? t.tbl_POS_ShiftList.Shift_Name : "",
				StoreId = t.T_StoreID,
				StoreName = t.tbl_POS_StoreList != null ? t.tbl_POS_StoreList.Store_Name : "",
				SubTotal = t.T_1SubTotal,
				Taxs = t.tbl_Exception_TransTaxes.Where(f => f.tbl_POS_TaxesList != null).Select(f => new TranTax()
				{
					Id = f.tbl_POS_TaxesList.TaxID,
					Name = f.tbl_POS_TaxesList.TaxName,
					Ammount = f.TaxAmount
				}),
				TerminalId = t.T_TerminalID,
				TerminalName = t.tbl_POS_TerminalList != null ? t.tbl_POS_TerminalList.Terminal_Name : "",
				Total = t.T_6TotalAmount ?? 0,
				TranDate = t.TransDate,
				TranId = t.TransID,
				TranNo = t.T_0TransNB
			});
		}

		public EmplTranctionPagings GetTransacByEmployee(UserContext usercontext, EmplTransacParam param)
		{
			IQueryable<EmplTranction> query;

			DateTime sDate = Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = Convert.ToDateTime(param.EndTranDate);

			var queryem =
				DataService.GetMetricRebar().Where(t => t.T_PACID == param.PacId && t.T_OperatorID == param.EmployerId && t.TransDate >= sDate && t.TransDate <= eDate);
			var querytypetran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type).Where(t => param.Types.Contains(t.TypeID));
			var queryPaymentsTran = DataService.GetTransExceptionPayments();

			var queryFilter = from tr in queryem
							  join pt in queryPaymentsTran on tr.TransID equals pt.TransID into py
							  from p in py.DefaultIfEmpty()
							  join ty in querytypetran on tr.TransID equals ty.TransID
							  select new
							  {
								  TranId = tr.TransID,
								  TranNo = tr.T_0TransNB,
								  PacId = tr.T_PACID,
								  EmpId = tr.T_OperatorID,
								  DateTran = tr.TransDate,
								  Total = tr.T_6TotalAmount,
								  ExcTypeId = ty != null ? ty.TypeID : 0,
								  TypeName = ty != null && ty.tbl_Exception_Type != null ? ty.tbl_Exception_Type.TypeName : "",
								  Color = ty != null && ty.tbl_Exception_Type != null ? ty.tbl_Exception_Type.Color : "",
								  TypeWeight = ty != null && ty.tbl_Exception_Type != null ? ty.tbl_Exception_Type.TypeWeight : 0,
								  PaymentId = p != null ? p.PaymentID : 0,
								  PaymentMethod = p != null && p.tbl_POS_PaymentList != null ? p.tbl_POS_PaymentList.PaymentName : ""
							  };

			if (param.FilterPayments.Any())
			{
				queryFilter = queryFilter.Where(t => param.FilterPayments.Contains(t.PaymentId));
			}

			query = from bd in queryFilter
				group bd by new
				{
					bd.TranId,
					bd.TranNo,
					bd.PacId,
					bd.EmpId,
					bd.DateTran,
					bd.Total
				}
				into g
				select new EmplTranction
				{
					TranId = g.Key.TranId,
					TranNo = g.Key.TranNo,
					PacId = g.Key.PacId,
					EmpId = g.Key.EmpId,
					DateTran = g.Key.DateTran,
					Total = g.Key.Total,
					Payments = g.Select(t => new TranPayment()
					{
						Id = t.PaymentId,
						Name = t.PaymentMethod
					}).Distinct(),
					ExceptionTypes = g.Select(t => new TranExceptionType()
					{
						Id = t.ExcTypeId,
						Color = t.Color,
						TypeWeight = t.TypeWeight,
						Name = t.TypeName
					})
				};


			var totalCount = query.Count();

			query = query.Take(totalCount);

			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			query = query
				.OrderBy(x => x.TranId)
				.Skip(param.PageSize * (param.PageNo - 1))
				.Take(param.PageSize);

			var result = query.ToList();

			var listTran = result.Select(t => t.TranId).Distinct().ToList();

			var notedb = DataService.GetTransExceptionNotes()
				.Where(t => t.UserID == usercontext.ID && listTran.Contains(t.TransID))
				.Select(t => new ExceptionNotes()
					{
						TranId = t.TransID,
						DateNotes = t.CreatedDate,
						UserId = t.UserID,
						Note = t.TransNote
					}).ToList();

			result.ForEach(t =>
			{
				t.Notes = notedb.Where(g => g.TranId == t.TranId);
			});


			return new EmplTranctionPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = result
			};

		}

		public PaymTranctionPagings GetTransacDetailByPaymentType(UserContext usercontext, TranPaymentDetailParam param)
		{
			IQueryable<PaymTranction> query;
			DateTime sDate = Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = Convert.ToDateTime(param.EndTranDate);
			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int?>();

			var queryem =
				DataService.GetMetricRebar()
					.Where(t => pacIds.Contains(t.T_PACID) && t.TransDate >= sDate && t.TransDate <= eDate);
			var queryPaymentsTran = DataService.GetTransExceptionPayments().Where(t => t.PaymentID == param.Type);

			query = from tr in queryem
					join pt in queryPaymentsTran on tr.TransID equals pt.TransID
					select new PaymTranction
					{
						TranId = tr.TransID,
						TranNo = tr.T_0TransNB,
						PacId = tr.T_PACID,
						EmpId = tr.T_OperatorID,
						DateTran = tr.TransDate,
						Total = tr.T_6TotalAmount,
						PaymentId = pt != null ? pt.PaymentID : 0,
						PaymentMethod = pt != null && pt.tbl_POS_PaymentList != null ? pt.tbl_POS_PaymentList.PaymentName : ""
					};

			var totalCount = query.Count();

			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};



			query = query
				.OrderBy(x => x.TranId)
				.Skip(param.PageSize * (param.PageNo - 1))
				.Take(param.PageSize);

			var result = query.ToList();
			var listTran = result.Select(t => t.TranId).Distinct().ToList();
			var notedb = DataService.GetTransExceptionNotes()
				.Where(t => t.UserID == usercontext.ID && listTran.Contains(t.TransID))
				.Select(t => new ExceptionNotes()
				{
					TranId = t.TransID,
					DateNotes = t.CreatedDate,
					UserId = t.UserID,
					Note = t.TransNote
				}).ToList();

			var querytypetran =
				DataService.GetTransExceptionType()
					.Include(t => t.tbl_Exception_Type)
					.Where(t => t.UserID == usercontext.ID && listTran.Contains(t.TransID)).ToList();


			result.ForEach(t =>
			{
				t.Notes = notedb.Where(g => g.TranId == t.TranId);
				t.ExceptionTypes = querytypetran.Where(g => g.TransID == t.TranId).Select(g => new TranExceptionType
				{
					Id = g.TypeID,
					Color = g.tbl_Exception_Type != null ? g.tbl_Exception_Type.Color : "",
					Name = g.tbl_Exception_Type != null ? g.tbl_Exception_Type.TypeName : "",
					TypeWeight = g.tbl_Exception_Type != null ? g.tbl_Exception_Type.TypeWeight : 0
				});
			});

			return new PaymTranctionPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = result
			};

		}

		public TransactionFilterPagings GeTransactionFilterPagings(UserContext usercontext, TransactionFilterParam param)
		{
			DateTime sDate = Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = Convert.ToDateTime(param.EndTranDate);

            BusinessServices.Sites.SitesBusinessService siteSvc = new Sites.SitesBusinessService();
            siteSvc.IUser = IUser;
            siteSvc.DataService = ISiteSvc;
            IEnumerable <CMSPACSiteModel> pacids =  siteSvc.GetPacInfoBySites(usercontext, String.Join(",", param.Sites.ToArray()));
            if (!pacids.Any()) return null;
            IEnumerable<int> kdvr = pacids.Select(i=>i.KDVR);
			var query = DataService.GetPosTransactions()
				.Include(t => t.tbl_POS_StoreList)
				.Include(t => t.tbl_POS_OperatorList)
				.Include(t => t.tbl_POS_RegisterList)
                .Where(t => t.TransDate >= sDate && t.TransDate <= eDate && kdvr.Contains(t.T_PACID));

			if (param.TranNo > 0)
			{
				query = query.Where(t => t.T_0TransNB == param.TranNo);
			}

			if (!string.IsNullOrEmpty(param.EmployeeId))
			{
				query =	query.Where(t => t.tbl_POS_OperatorList != null && t.tbl_POS_OperatorList.Operator_Name.Contains(param.EmployeeId));
			}

			var totalCount = query.Count();
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};


			var querycompare = query.Select(t => new TransactionCompare()
			{
				EmployeeId = t.T_OperatorID,
				EmployeeName = t.tbl_POS_OperatorList != null ? t.tbl_POS_OperatorList.Operator_Name : "",
				PacId = t.T_PACID,
				RegisterId = t.T_RegisterID,
				StoreId = t.T_StoreID,
				StoreName = t.tbl_POS_StoreList != null ? t.tbl_POS_StoreList.Store_Name : "",
				TranId = t.TransID,
				TranNo = t.T_0TransNB,
				TranDate = t.TransDate
			});

			querycompare = querycompare
				.OrderBy(x => x.TranId)
                .Skip(param.PageSize * (param.PageNo - 1))
				.Take(param.PageSize);


			return new TransactionFilterPagings()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = querycompare.ToList()
			};
		}

		public bool AddTransactionFlagType(UserContext usercontext, TranExceptionType flag)
		{
			var newflag = new tbl_Exception_Type
			{
				Color = flag.Color,
				TypeDesc = flag.Desc,
				TypeName = flag.Name,
				UserID = usercontext.ID,
				TypeWeight = flag.TypeWeight,
				Fixed = false
			};
			DataService.InsertExcepFlagType(newflag);
			return DataService.Save() >= 0;
		}

		public bool UpdateTransactionFlagType(UserContext usercontext, TranExceptionType flag)
		{
			var exceptionTypedb = DataService.GetExcepFlagTypes().FirstOrDefault(t => t.TypeID == flag.Id && t.UserID == usercontext.ID);

			if (exceptionTypedb == null) return false;

			if (usercontext.ID == usercontext.ParentID)
			{
				if (exceptionTypedb.Fixed == true)
				{
					exceptionTypedb.TypeWeight = flag.TypeWeight;
					exceptionTypedb.Color = flag.Color;
					DataService.UpdateExcepFlagType(exceptionTypedb);

					return DataService.Save() >= 0;
				}
				else
				{
					exceptionTypedb.Color = flag.Color;
					exceptionTypedb.TypeName = flag.Name;
					exceptionTypedb.TypeWeight = flag.TypeWeight;
					exceptionTypedb.TypeDesc = flag.Desc;
					exceptionTypedb.Fixed = false;
					DataService.UpdateExcepFlagType(exceptionTypedb);

					return DataService.Save() >= 0;
				}
				
			}
			else
			{
				exceptionTypedb.Color = flag.Color;
				exceptionTypedb.TypeName = flag.Name;
				exceptionTypedb.TypeWeight = flag.TypeWeight;
				exceptionTypedb.TypeDesc = flag.Desc;
				exceptionTypedb.Fixed = false;
				DataService.UpdateExcepFlagType(exceptionTypedb);

				return DataService.Save() >= 0;
			}
			
		}

		public bool DelTransactionFlagType(UserContext usercontext, TranExceptionType flag)
		{
			var exceptionTypedb = DataService.GetExcepFlagTypes().FirstOrDefault(t => t.TypeID == flag.Id && t.UserID == usercontext.ID);

			if (exceptionTypedb == null || exceptionTypedb.Fixed == true) return false;

			var transTypes = DataService.GetTransExceptionType();
			if (transTypes.Any(g => g.TransID == exceptionTypedb.TypeID))
			{
				throw new CmsErrorException(CMSWebError.DELETE_FAIL_MSG.ToString(), flag);
			}

			DataService.DeleteExcepFlagType(exceptionTypedb);
			return DataService.Save() >= 0;
		}

		public bool SaveTransactionFlagTypes(UserContext usercontext, List<TranExceptionType> flags)
		{
			var exceptionSystemFlag = DataService.GetExcepFlagTypes().Where(t => t.Fixed == true).Select(t => t.TypeID).ToList();
			var transTypes = DataService.GetTransExceptionType();

			var exceptionTypedbs = DataService.GetExcepFlagTypes().Where(t => exceptionSystemFlag.Contains(t.TypeID) || t.UserID == usercontext.ID).ToList();
			var flagIds = flags.Select(t => t.Id).ToList();

			if (usercontext.ID == usercontext.ParentID)
			{
				var updateSystemFlags = exceptionTypedbs.Where(t => t.Fixed == true && !flagIds.Contains(t.TypeID)).ToList();
				if (updateSystemFlags.Any())
				{
					updateSystemFlags.ForEach(t =>
					{
						var updateitem = flags.FirstOrDefault(g => g.Id == t.TypeID);
						if (updateitem == null) return;
						t.Color = updateitem.Color;
						t.TypeWeight = updateitem.TypeWeight;
						DataService.UpdateExcepFlagType(t);
					});
				}
			}

			var removeList = exceptionTypedbs.Where(t => t.Fixed == false && !flagIds.Contains(t.TypeID)).ToList();
			if (removeList.Any())
			{
				removeList.ForEach(t =>
				{
					if (transTypes.Any(g => g.TransID == t.TypeID))
					{
						throw new CmsErrorException(CMSWebError.DELETE_FAIL_MSG.ToString(), t);
					}
					DataService.DeleteExcepFlagType(t);
				});
			}

			var updateList = exceptionTypedbs.Where(t => t.Fixed == false && !flagIds.Contains(t.TypeID)).ToList();
			if (updateList.Any())
			{
				updateList.ForEach(t =>
				{
					var updateitem = flags.FirstOrDefault(g => g.Id == t.TypeID);
					if (updateitem == null) return;
					t.Color = updateitem.Color;
					t.TypeName = updateitem.Name;
					t.TypeWeight = updateitem.TypeWeight;
					t.TypeDesc = updateitem.Desc;
					DataService.UpdateExcepFlagType(t);
				});
			}

			var insert = flags.Where(t => !exceptionTypedbs.Select(g => g.TypeID).Contains(t.Id) && !exceptionSystemFlag.Contains(t.Id)).ToList();
			if (insert.Any())
			{
				insert.ForEach(t =>
				{
					var flag = new tbl_Exception_Type
					{
						Color = t.Color,
						TypeDesc = t.Desc,
						TypeName = t.Name,
						UserID = usercontext.ID,
						TypeWeight = t.TypeWeight,
						Fixed = false
					};
					DataService.InsertExcepFlagType(flag);
				});
			}
			return DataService.Save() >= 0;
		}

		public bool SaveTransactionNotes(UserContext userContext, ExceptionModel note)
		{

			tbl_Exception_Transact tran = DataService.GetExceptionTransactions()
				.Include(t => t.tbl_Exception_FlaggedTrans)
				.Include(t => t.tbl_Exception_Notes)
				.FirstOrDefault(t => t.TransID == note.TranId);
			var exceptionSystemFlag = DataService.GetExcepFlagTypes().Where(t => t.Fixed == true).Select(t => t.TypeID).ToList();

			if (tran != null && (note.Note == null || string.IsNullOrEmpty(note.Note.Note)) && !note.ExceptionTypes.Any() && !tran.tbl_Exception_FlaggedTrans.Any(t => exceptionSystemFlag.Contains(t.TypeID)))
			{
				if (!tran.tbl_Exception_Notes.Any() || tran.tbl_Exception_Notes.All(t => t.UserID == userContext.ID))
				{
					DataService.DeleteExcepTranByTranId(tran.TransID);
					DataService.DeleteExcepFlagByTranId(tran.TransID);
					DataService.DeleteExcepNoteByTranId(tran.TransID);
					tran.tbl_Exception_Retail.ToList().ForEach(t => DataService.DeleteExcepTranSubDetailByTranId(t.RetailID));
					DataService.DeleteExcepTranDetailByTranId(tran.TransID);
					DataService.DeleteExcepTranPaymentByTranId(tran.TransID);
					DataService.DeleteExcepTranTaxByTranId(tran.TransID);
					return DataService.Save() >= 0;
				}
			}
			


			if (tran == null)
			{
				var getTran = DataService.GetPosTransactions()
					.Include(t => t.tbl_POS_Retail)
					.Include(t => t.tbl_POS_Retail.Select(d => d.tbl_POS_SubRetail))
					.Include(t => t.tbl_POS_TransPayment)
					.Include(t => t.tbl_POS_TransTaxes)
					.FirstOrDefault(t => t.TransID == note.TranId);

				if (getTran == null) return false;

				tran = new tbl_Exception_Transact()
				{
					TransID = getTran.TransID,
					TransactKey = getTran.TransactKey,
					T_0TransNB = getTran.T_0TransNB,
					T_6TotalAmount = getTran.T_6TotalAmount,
					T_1SubTotal = getTran.T_1SubTotal,
					T_8ChangeAmount = getTran.T_8ChangeAmount,
					TransDate = getTran.TransDate,
					DVRDate = getTran.DVRDate,
					T_9RecItemCount = getTran.T_9RecItemCount,
					T_CameraNB = getTran.T_CameraNB,
					T_OperatorID = getTran.T_OperatorID,
					T_StoreID = getTran.T_StoreID,
					T_TerminalID = getTran.T_TerminalID,
					T_RegisterID = getTran.T_RegisterID,
					T_ShiftID = getTran.T_ShiftID,
					T_CheckID = getTran.T_CheckID,
					T_CardID = getTran.T_CardID,
					T_TOBox = getTran.T_TOBox,
					T_00TransNBText = getTran.T_00TransNBText,
					T_PACID = getTran.T_PACID
				};

				tran.tbl_Exception_Retail = getTran.tbl_POS_Retail.Select(t => new tbl_Exception_Retail()
				{


					RetailID = t.RetailID,
					RetailKey = t.RetailKey,
					R_2ItemLineNb = t.R_2ItemLineNb,
					R_1Qty = t.R_1Qty,
					R_0Amount = t.R_0Amount,
					R_Description = t.R_Description,
					R_ItemCode = t.R_ItemCode,
					R_DVRDate = t.R_DVRDate,
					TransID = t.TransID,
					R_TOBox = t.R_TOBox,

					tbl_Exception_SubRetail = t.tbl_POS_SubRetail.Select(s => new tbl_Exception_SubRetail()
					{
						SubRetailID = s.SubRetailID,
						RetailID = s.RetailID,
						SR_2SubItemLineNb = s.SR_2SubItemLineNb,
						SR_1Qty = s.SR_1Qty,
						SR_0Amount = s.SR_0Amount,
						SR_Description = s.SR_Description
					}).ToList()
				}).ToList();



				tran.tbl_Exception_TransPayment = getTran.tbl_POS_TransPayment.Select(t => new tbl_Exception_TransPayment()
				{
					TransID = t.TransID,
					PaymentID = t.PaymentID,
					PaymentAmount = t.PaymentAmount
				}).ToList();

				tran.tbl_Exception_TransTaxes = getTran.tbl_POS_TransTaxes.Select(t => new tbl_Exception_TransTaxes()
				{
					TransID = t.TransID,
					TaxID = t.TaxID,
					TaxAmount = t.TaxAmount
				}).ToList();

				if (note.Note != null)
				{
					var notedb = new tbl_Exception_Notes()
					{
						CreatedDate = DateTime.Now,
						TransID = note.TranId,
						TransNote = note.Note.Note,
						UserID = userContext.ID
					};
					tran.tbl_Exception_Notes.Add(notedb);
				}

				
				if (note.ExceptionTypes.Any())
				{
					tran.tbl_Exception_FlaggedTrans = note.ExceptionTypes.Where(t => !exceptionSystemFlag.Contains(t.Id)).Select(t => new tbl_Exception_FlaggedTrans()
					{
						UserID = userContext.ID,
						TransID = tran.TransID,
						TypeID = t.Id,
						FlaggedDate = DateTime.Now
					}).ToList();
				}
				DataService.InsertExcepTransaction(tran);
				return DataService.Save() >= 0;
			}

			//Update Exception

			if (note.ExceptionTypes.Any())
			{
				var removeList = tran.tbl_Exception_FlaggedTrans.Where(t => !exceptionSystemFlag.Contains(t.TypeID) && !note.ExceptionTypes.Select(s => s.Id).Contains(t.TypeID)).ToList();
				if (removeList.Any())
				{
					removeList.ForEach(t => DataService.DeleteExcepFlag(t));
				}

				var insert = note.ExceptionTypes.Where(t => !exceptionSystemFlag.Contains(t.Id) && !tran.tbl_Exception_FlaggedTrans.Select(s => s.TypeID).Contains(t.Id)).ToList();
				if (insert.Any())
				{
					insert.ForEach(t =>
					{
						var flag = new tbl_Exception_FlaggedTrans
						{
							UserID = userContext.ID,
							TransID = tran.TransID,
							TypeID = t.Id,
							FlaggedDate = DateTime.Now
						};
						DataService.InsertExcepFlag(flag);
					});
				}
			}
			else
			{
				var removeList = tran.tbl_Exception_FlaggedTrans.Where(t => !exceptionSystemFlag.Contains(t.TypeID)).ToList();
				if (removeList.Any())
				{
					removeList.ForEach(t => DataService.DeleteExcepFlag(t));
				}
			}



			if (note.Note != null)
			{
				var exNote = tran.tbl_Exception_Notes.FirstOrDefault(t => t.UserID == userContext.ID);
				if (exNote != null)
				{
					exNote.TransNote = note.Note.Note;
					exNote.CreatedDate = DateTime.Now;

					DataService.UpdateTransactionNotes(exNote);
				}
				else
				{
					var notedb = new tbl_Exception_Notes()
					{
						CreatedDate = DateTime.Now,
						TransID = note.TranId,
						TransNote = note.Note.Note,
						UserID = userContext.ID
					};
					DataService.SaveTransactionNotes(notedb);
				}

			}

			return DataService.Save() >= 0;
		}

		public Transaction GetPosTransactionInfo(UserContext usercontext, long tranId, int pacId, int? registerId = null, int nextOrPrev = 0)
		{
			var query = DataService.GetPosTransactions()
				.Include(t => t.tbl_POS_StoreList)
				.Include(t => t.tbl_POS_CameraNBList)
				.Include(t => t.tbl_POS_RegisterList)
				.Select(t => new Transaction()
				{
					TranId = t.TransID,
					TranNo = t.T_0TransNB,
					PacId = t.T_PACID,
					CamId = t.T_CardID,
					CamName = t.tbl_POS_CameraNBList != null ? t.tbl_POS_CameraNBList.CameraNB_Name : "",
					ChangeAmount = t.T_8ChangeAmount,
					DvrDate = t.DVRDate,
					EmployeeId = t.T_OperatorID,
					EmployeeName = t.tbl_POS_OperatorList != null ? t.tbl_POS_OperatorList.Operator_Name : "",
					RegisterId = t.T_RegisterID,
					RegisterName = t.tbl_POS_RegisterList != null ? t.tbl_POS_RegisterList.Register_Name : "",
					StoreId = t.T_StoreID,
					StoreName = t.tbl_POS_StoreList != null ? t.tbl_POS_StoreList.Store_Name : "",
					SubTotal = t.T_1SubTotal,
					Total = t.T_6TotalAmount,
					TranDate = t.TransDate
				});

			Transaction tran = null;

			if (nextOrPrev == 0)
			{
				tran = query.FirstOrDefault(t => t.TranId == tranId);
			}

			if (nextOrPrev == 1)
			{
				tran =
					query.Where(t => t.TranId > tranId && t.PacId == pacId && t.RegisterId == registerId)
						.OrderBy(e => e.TranId)
						.FirstOrDefault();
			}

			if (nextOrPrev == -1)
			{
				tran =
					query.Where(t => t.TranId < tranId && t.PacId == pacId && t.RegisterId == registerId)
						.OrderByDescending(e => e.TranId)
						.FirstOrDefault();
			}


			if (tran == null) return null;

			tran.Note = DataService.GetTransExceptionNotes().Select(n => new ExceptionNotes()
			{
				TranId = n.TransID,
				UserId = n.UserID,
				Note = n.TransNote,
				DateNotes = n.CreatedDate
			}).FirstOrDefault(n => n.UserId == usercontext.ID && n.TranId == tran.TranId);

			var tranDetail = DataService.GetPosTransactionsDetail().Where(t => t.TransID == tran.TranId)
				.Include(t => t.tbl_POS_SubRetail)
				.Include(t => t.tbl_POS_DescriptionList)
				.Include(t => t.tbl_POS_ItemCodeList)
				.Select(t => new TransactionDetail()
				{
					Id = t.RetailID,
					DescriptionName = t.tbl_POS_DescriptionList != null ? t.tbl_POS_DescriptionList.Description_Name : "",
					ItemCodeName = t.tbl_POS_ItemCodeList != null ? t.tbl_POS_ItemCodeList.ItemCode_Name : "",
					ItemLine = t.R_2ItemLineNb,
					Qty = t.R_1Qty,
					Total = t.R_0Amount,
					TranId = t.TransID,
					SubDetails = t.tbl_POS_SubRetail.Select(s => new TransactionSubDetail()
					{
						Id = s.SubRetailID,
						DetailId = s.RetailID,
						ItemLine = s.SR_2SubItemLineNb,
						Qty = s.SR_1Qty,
						Total = s.SR_0Amount
					})
				});
			tran.Details = tranDetail;


			var querytypetran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type)
                .Where(t => (t.TransID == tran.TranId && t.UserID == usercontext.ID) || (t.TransID == tran.TranId && t.tbl_Exception_Type.Fixed == true))
				.Select(t => new TranExceptionType()
				{
					Id = t.TypeID,
					Desc = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.TypeDesc : "",
					Name = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.TypeName : "",
					FlagTime = t.FlaggedDate,
					Color = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.Color : null,
					TypeWeight = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.TypeWeight : 0
				});

			tran.ExceptionTypes = querytypetran;

			var querytaxs = DataService.GetPosTransactionTaxes()
				.Where(t => t.TransID == tran.TranId)
				.Select(t => new TranTax()
				{
					Id = t.TaxID,
					Name = t.tbl_POS_TaxesList != null ? t.tbl_POS_TaxesList.TaxName : "",
					Ammount = t.TaxAmount
				});

			tran.Taxs = querytaxs;

			var payments = DataService.GetPosTransactionPayments().Include(t => t.tbl_POS_PaymentList)
				.Where(t => t.TransID == tran.TranId)
				.Select(t => new TranPayment()
				{
					Id = t.PaymentID,
					Ammount = t.PaymentAmount,
					Name = t.tbl_POS_PaymentList != null ? t.tbl_POS_PaymentList.PaymentName : ""
				});

			tran.Payments = payments;

			var company = GetCompanyLogo(usercontext);

			tran.CompanyLogo = company != null ? company.CompanyLogo : null;

			return tran;
		}

		public IQueryable<TranExceptionType> GetTransactionTypes(UserContext usercontext)
		{
			var exceptionSystemFlag = DataService.GetExcepFlagTypes().Where(t => t.Fixed == true).Select(t => t.TypeID).ToList();
			return DataService.GetTransactionTypes()
				.Where(t => exceptionSystemFlag.Contains(t.TypeID) || t.UserID == usercontext.ID)
				.Select(t => new TranExceptionType()
				{
					Id = t.TypeID,
					Name = t.TypeName,
					TypeWeight = t.TypeWeight,
					Color = t.Color,
					IsSystem = t.Fixed,
					Desc = t.TypeDesc
				});
		}

		public IQueryable<TranTax> GetTaxsList(UserContext usercontext)
		{
			return DataService.GetTaxtLists()
				.Select(t => new TranTax()
				{
					Id = t.TaxID,
					Name = t.TaxName
				});
		}

		public Transaction GetTransactionInfo(UserContext usercontext, long tranId)
		{
			var tran = DataService.GetMetricRebar()
				.Include(t => t.tbl_Exception_Notes)
				.Include(t => t.tbl_POS_StoreList)
				.Include(t => t.tbl_POS_CameraNBList)
				.Include(t => t.tbl_POS_RegisterList)
				.Select(t => new Transaction()
				{
					TranId = t.TransID,
					TranNo = t.T_0TransNB,
					PacId = t.T_PACID,
					CamId = t.T_CardID,
					CamName = t.tbl_POS_CameraNBList != null ? t.tbl_POS_CameraNBList.CameraNB_Name : "",
					ChangeAmount = t.T_8ChangeAmount,
					DvrDate = t.DVRDate,
					EmployeeId = t.T_OperatorID,
					EmployeeName = t.tbl_POS_OperatorList != null ? t.tbl_POS_OperatorList.Operator_Name : "",
					RegisterId = t.T_RegisterID,
					RegisterName = t.tbl_POS_RegisterList != null ? t.tbl_POS_RegisterList.Register_Name : "",
					StoreId = t.T_StoreID,
					StoreName = t.tbl_POS_StoreList != null ? t.tbl_POS_StoreList.Store_Name : "",
					SubTotal = t.T_1SubTotal,
					Total = t.T_6TotalAmount,
					TranDate = t.TransDate,
					Note = t.tbl_Exception_Notes.Select(n => new ExceptionNotes()
					{
						TranId = t.TransID,
						UserId = n.UserID,
						Note = n.TransNote,
						DateNotes = n.CreatedDate
					}).FirstOrDefault(n => n.UserId == usercontext.ID)
				})
				.FirstOrDefault(t => t.TranId == tranId);


			if (tran == null) return null;

			var tranDetail = DataService.GetTransExceptionDetail().Where(t => t.TransID == tranId)
				.Include(t => t.tbl_Exception_SubRetail)
				.Include(t => t.tbl_POS_DescriptionList)
				.Include(t => t.tbl_POS_ItemCodeList)
				.Select(t => new TransactionDetail()
				{
					Id = t.RetailID,
					DescriptionName = t.tbl_POS_DescriptionList != null ? t.tbl_POS_DescriptionList.Description_Name : "",
					ItemCodeName = t.tbl_POS_ItemCodeList != null ? t.tbl_POS_ItemCodeList.ItemCode_Name : "",
					ItemLine = t.R_2ItemLineNb,
					Qty = t.R_1Qty,
					Total = t.R_0Amount,
					TranId = t.TransID,
					SubDetails = t.tbl_Exception_SubRetail.Select(s => new TransactionSubDetail()
					{
						Id = s.SubRetailID,
						DetailId = s.RetailID,
						ItemLine = s.SR_2SubItemLineNb,
						Qty = s.SR_1Qty,
						Total = s.SR_0Amount
					})
				});
			tran.Details = tranDetail;


			var querytypetran = DataService.GetTransExceptionType().Include(t => t.tbl_Exception_Type)
                .Where(t => t.TransID == tranId && t.UserID == usercontext.ID || (t.TransID == tran.TranId && t.tbl_Exception_Type.Fixed == true))
				.Select(t => new TranExceptionType()
				{
					Id = t.TypeID,
					Desc = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.TypeDesc : "",
					Name = t.tbl_Exception_Type != null ? t.tbl_Exception_Type.TypeName : "",
					FlagTime = t.FlaggedDate
				});

			tran.ExceptionTypes = querytypetran;

			var querytaxs = DataService.GetTransExceptionTaxes()
				.Where(t => t.TransID == tranId)
				.Select(t => new TranTax()
				{
					Id = t.TaxID,
					Name = t.tbl_POS_TaxesList != null ? t.tbl_POS_TaxesList.TaxName : "",
					Ammount = t.TaxAmount
				});

			tran.Taxs = querytaxs;

			var payments = DataService.GetTransExceptionPayments().Include(t => t.tbl_POS_PaymentList)
				.Where(t => t.TransID == tranId)
				.Select(t => new TranPayment()
				{
					Id = t.PaymentID,
					Ammount = t.PaymentAmount,
					Name = t.tbl_POS_PaymentList != null ? t.tbl_POS_PaymentList.PaymentName : ""
				});

			tran.Payments = payments;

			return tran;
		}

		public IQueryable<SummaryWeekAtGlanceModel> GetWeekAtGlanceSummary(UserContext userLogin, RebarWeekAtAGlanceParam param)
		{
			DateTime sDate = param.StartTranDate; //Convert.ToDateTime(param.StartTranDate);
			DateTime eDate = param.EndTranDate;//Convert.ToDateTime(param.EndTranDate);

			var listPacSite = getPacIds(userLogin, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.PACID).Distinct().Cast<int>().ToList();

			List<int> lsKDVR = listPacSite.Select(x=>x.KDVR ?? 0).Distinct().ToList();
			string KDVRs = string.Join(",", lsKDVR);

			string pacids = string.Join(",", pacIds);
			var data = DataService.GetWeekAtGlanceSummary(pacids, sDate.StartOfDay(), eDate.EndOfDay());
			IQueryable<SummaryWeekAtGlanceModel> retData = data.Select(s => new SummaryWeekAtGlanceModel()
			{
				Id = s.TypeID,
				Name = s.TypeName,
				TotalAmmount = s.TotalAmount.HasValue ? s.TotalAmount.Value : 0,
				TotalTrans = s.Count.HasValue ? s.Count.Value : 0,
				KDVRs = KDVRs
			});

			return retData;
		}

		public EmployerPagingModel GetEmployerRebar(UserContext usercontext, EmployerParamModel param)
		{
			IQueryable<EmployerRebarModel> query;

			var listPacSite = getPacIds(usercontext, param.SiteKeys);
			var pacIds = listPacSite.Select(t => t.KDVR).Distinct().Cast<int>().ToList();


			var queryem = DataService.GetMetricRebar().Where(t => pacIds.Contains((int)t.T_PACID));
			var querytypetran = DataService.GetTransExceptionType().Where(t => param.Types.Contains(t.TypeID));

			query = from tr in queryem
					join ty in querytypetran on tr.TransID equals ty.TransID
					select
						new
						{
							tr.TransID,
							tr.T_PACID,
							tr.T_StoreID,
							Store_Name = tr.tbl_POS_StoreList.Store_Name,
							tr.T_OperatorID,
							EmpName = tr.tbl_POS_OperatorList.Operator_Name,
							tr.T_6TotalAmount,
							ty.TypeID,
							ty.tbl_Exception_Type.TypeName
						}
						into g
						group g by
							new { g.T_PACID, g.T_StoreID, g.Store_Name, g.T_OperatorID, g.EmpName, g.TypeID, g.TypeName, g.T_6TotalAmount }
							into f
							select new EmployerRebarModel()
							{
								EmployerId = f.Key.T_OperatorID,
								EmployerName = f.Key.EmpName,
								PacId = f.Key.T_PACID,
								StoreId = f.Key.T_StoreID,
								StoreName = f.Key.Store_Name,
								TypeName = f.Key.TypeName,
								TypeId = f.Key.TypeID,
								Value = f.Count() // (decimal) f.Sum(x => x.T_6TotalAmount)
							};

			var totalCount = query.Count();
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);


			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};


			var results = query
				.OrderBy(x => x.EmployerId)
				.Skip(param.PageSize * param.PageNo - 1)
				.Take(param.PageSize).ToList();

			results.ForEach(t =>
			{
				var site = listPacSite.FirstOrDefault(g => g.PACID == t.PacId);
				t.SiteKey = site != null ? site.siteKey : 0;
			});

			return new EmployerPagingModel()
			{
				CurrentPage = param.PageNo,
				TotalPages = paginationHeader.TotalPages,
				Data = results.ToList()
			};
		}

		public ModelPaging GetPaymentList(PagingParam param)
		{
			var query = DataService.GetPOSPaymentList();

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var results = query
				.OrderBy(x => x.PaymentID)
				.Skip(param.PageSize * (param.PageNumber - 1))
				.Take(param.PageSize);

			
			var ret = new ModelPaging()
			{
				CurrentPage = param.PageNumber,
				TotalPage = paginationHeader.TotalPages,
				Data = results.Select(s => new PaymentModel()
				{
					ID = s.PaymentID,
					Name = s.PaymentName
				})
			};

			return ret;
		}

		public ModelPaging GetRegisterList(PagingParam param)
		{
			var query = DataService.GetPOSRegisterList();

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			var paginationHeader = new
		{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var results = query
				.OrderBy(x => x.Register_ID)
				.Skip(param.PageSize * (param.PageNumber - 1))
				.Take(param.PageSize);


			var ret = new ModelPaging()
			{
				CurrentPage = param.PageNumber,
				TotalPage = paginationHeader.TotalPages,
				Data = results.Select(s => new RegisterModel()
				{
					ID = s.Register_ID,
					Name = s.Register_Name
				})
			};

			return ret;
		}

		public ModelPaging GetOperatorList(PagingParam param)
		{
			var query = DataService.GetPOSOperatorList();

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var results = query
				.OrderBy(x => x.Operator_ID)
				.Skip(param.PageSize * (param.PageNumber - 1))
				.Take(param.PageSize);


			var ret = new ModelPaging()
			{
				CurrentPage = param.PageNumber,
				TotalPage = paginationHeader.TotalPages,
				Data = results.Select(s => new OperatorModel()
				{
					ID = s.Operator_ID,
					Name = s.Operator_Name
				})
			};

			return ret;
		}

		public ModelPaging GetDescriptionList(PagingParam param)
		{
			var query = DataService.GetPOSDescriptionList();

			var totalCount = query.Count();
			query = query.Take(totalCount);
			var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);

			var paginationHeader = new
			{
				TotalCount = totalCount,
				TotalPages = totalPages
			};

			var results = query
				.OrderBy(x => x.Description_ID)
				.Skip(param.PageSize * (param.PageNumber - 1))
				.Take(param.PageSize);


			var ret = new ModelPaging()
		{
				CurrentPage = param.PageNumber,
				TotalPage = paginationHeader.TotalPages,
				Data = results.Select(s => new DescriptionModel()
				{
					ID = s.Description_ID,
					Name = s.Description_Name
				})
			};

			return ret;
		}

		public IQueryable<PaymentModel> FilterPayment(string FilterText)
		{
			if (string.IsNullOrEmpty(FilterText)) { return null; }
			var ret = DataService.GetPOSPaymentList()
				.Where(w => w.PaymentName.ToLower().Contains(FilterText.ToLower()))
				.Select(s => new PaymentModel() { ID = s.PaymentID, Name = s.PaymentName });
			return ret;
		}
		public IQueryable<RegisterModel> FilterRegister(string FilterText)
		{
			if (string.IsNullOrEmpty(FilterText)) { return null; }
			return DataService.GetPOSRegisterList()
				.Where(w => w.Register_Name.ToLower().Contains(FilterText.ToLower()))
				.Select(s => new RegisterModel() { ID = s.Register_ID, Name = s.Register_Name });
		}
		public IQueryable<OperatorModel> FilterOperator(string FilterText)
		{
			if (string.IsNullOrEmpty(FilterText)) { return null; }
			return DataService.GetPOSOperatorList()
				.Where(w => w.Operator_Name.ToLower().Contains(FilterText.ToLower()))
				.Select(s => new OperatorModel() { ID = s.Operator_ID, Name = s.Operator_Name });
		}
		public IQueryable<DescriptionModel> FilterDescription(string FilterText)
		{
			if (string.IsNullOrEmpty(FilterText)) { return null; }
			return DataService.GetPOSDescriptionList()
				.Where(w => w.Description_Name.ToLower().Contains(FilterText.ToLower()))
				.Select(s => new DescriptionModel() { ID = s.Description_ID, Name = s.Description_Name });
		}

		public IQueryable<DescriptionTransModel> GetDescriptionById(string keys)
		{
			if (string.IsNullOrEmpty(keys)) { return null; }

			var descIds = keys.Split(new char[] { ',' }).Select(int.Parse).ToList();
			var ret = DataService.GetPOSDescriptionList()
				.Where(w => descIds.Contains(w.Description_ID))
				.SelectMany(s => s.tbl_POS_Retail, (parent, child) => new DescriptionTransModel
				{
					ID = parent.Description_ID,
					Name = parent.Description_Name,
					TransID = child.TransID.Value
				}).Distinct();
			return ret;
		}
		public List<ColumnOptionModel> GetColumnOption(List<ColumnOptionParams> param)
		{
			IQueryable<ColumnModel> columnData;
			ColumnOptionModel ColumnOptionData = new ColumnOptionModel();
			List<ColumnOptionModel> cols = new List<ColumnOptionModel>();
			foreach (ColumnOptionParams p in param)
			{
				var keys = p.Keys.Split(new char[] { ',' }).Select(int.Parse).ToList();
				switch (p.Name)
				{
					case "TypeName":
						columnData = DataService.GetTransactionTypes().Where(w => keys.Contains(w.TypeID))
							.Select(s => new ColumnModel() { ID = s.TypeID, Name= s.TypeName });
						ColumnOptionData = new ColumnOptionModel() { Name = p.Name, Data = columnData, PrimaryField = p.PrimaryField };
						break;
					case "DescriptionName":
						columnData = DataService.GetPOSDescriptionList().Where(w => keys.Contains(w.Description_ID))
							.Select(s => new ColumnModel() { ID = s.Description_ID, Name = s.Description_Name });
						ColumnOptionData = new ColumnOptionModel() { Name = p.Name, Data = columnData, PrimaryField = p.PrimaryField };
						break;
					case "TerminalName":
						columnData = DataService.GetPOSTerminalList().Where(w => keys.Contains(w.Terminal_ID))
							.Select(s => new ColumnModel() { ID = s.Terminal_ID, Name = s.Terminal_Name });
						ColumnOptionData = new ColumnOptionModel() { Name = p.Name, Data = columnData, PrimaryField = p.PrimaryField };
						break;
					case "StoreName":
						columnData = DataService.GetPOSStoreList().Where(w => keys.Contains(w.Store_ID))
							.Select(s => new ColumnModel() { ID = s.Store_ID, Name = s.Store_Name });
						ColumnOptionData = new ColumnOptionModel() { Name = p.Name, Data = columnData, PrimaryField = p.PrimaryField };
						break;
					case "CheckName":
						columnData = DataService.GetPOSCheckIDList().Where(w => keys.Contains(w.CheckID_ID))
							.Select(s => new ColumnModel() { ID = s.CheckID_ID, Name = s.CheckID_Name });
						ColumnOptionData = new ColumnOptionModel() { Name = p.Name, Data = columnData, PrimaryField = p.PrimaryField };
						break;
				}
				cols.Add(ColumnOptionData);
			}
			return cols;
		}
		public IQueryable<ColumnModel> GetTerminal()
		{
			var ret = DataService.GetPOSTerminalList().Select(s => new ColumnModel() { ID = s.Terminal_ID, Name = s.Terminal_Name });
			return ret;
		}
		public IQueryable<ColumnModel> GetStore()
		{
			var ret = DataService.GetPOSStoreList().Select(s => new ColumnModel() { ID = s.Store_ID, Name = s.Store_Name });
			return ret;
		}
		public IQueryable<ColumnModel> GetCheckID()
		{
			var ret = DataService.GetPOSCheckIDList().Select(s => new ColumnModel() { ID = s.CheckID_ID, Name = s.CheckID_Name });
			return ret;
		}

	}
}
