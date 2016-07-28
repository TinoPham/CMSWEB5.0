using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;
using CMSWebApi.BusinessServices.ReportBusiness;

namespace CMSWebApi.BusinessServices.ConversionRate
{
	public class SaleReportsBusiness : ReportBusinessService //BusinessBase<ISaleReportsService>
	{
		#region properties
		//public IUsersService IUser { get; set; }
		public ISiteService ISite { get; set; }
		public IFiscalYearServices IFiscalYear { get; set; }

		#endregion

		public async Task<SaleReportDataAll> GetDataReport(UserContext userLogin, BAMRptParam pram)
		{
			switch (pram.rptDataType)
			{
				case Utils.BAMReportType.Hourly:
				return await GetSaleReportHourly(userLogin, pram);
				case Utils.BAMReportType.Daily:
				case Utils.BAMReportType.WTD:
				case Utils.BAMReportType.PTD:
					return await GetSaleReportDaily(userLogin, pram);
				case Utils.BAMReportType.Weekly:
					return await GetSaleReportWeekly(userLogin, pram);
				case Utils.BAMReportType.YTD:
					return await GetSaleReportMonthly(userLogin, pram);
				default:
					return null;
			}
		}

		private async Task<SaleReportDataAll> GetSaleReportHourly(UserContext userLogin, BAMRptParam pram)
		{
			List<SaleReportSummary> resultData = new List<SaleReportSummary>();
			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, userLogin, selectedSites);
			if (sites == null || !sites.Any())
			{
				return null;
			}

			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());
			IEnumerable<int> siteKeys = sites.Select(x => x.siteKey.HasValue ? x.siteKey.Value : 0).Distinct();

			IEnumerable<Proc_DashBoard_Conversion_Hourly_Result> posConversionData = await base.IPOSBusinessService.GetConversionHourlyAsync(pram.sDate, pram.eDate, pacIds);//await DataService.GetPOSConversionHourly(pacIds, pram.sDate, pram.eDate);

			//Get Region List by site id list
			string[] includes = { typeof(tCMSWebRegion).Name };
			IQueryable<tCMSWebSites> dbSites = ISite.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
			IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

			var sitepacId = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new { pacid = si.PACID.Value, siteKey = si.siteKey.Value }).Distinct();
			var pacInSiteCount = sitepacId.GroupJoin(dbSites, s => s.siteKey, sc => sc.siteKey, (s, sc) => new { pacid = s.pacid, sitekey = s.siteKey, siteName = sc.Any() ? sc.FirstOrDefault().ServerID : string.Empty, count = sc.Count() });

			var siteregionId = pacInSiteCount.Join(dbSites, si => si.sitekey, rg => rg.siteKey, (si, rg) => new { site = si, region = rg });
			var siteregionName = siteregionId.Join(dbRegions, si => si.region.RegionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.site.pacid, siteKey = si.site.sitekey, siteName = si.site.siteName, count = si.site.count, regionKey = rg.RegionKey, regionName = rg.RegionName });
			//var countSiteInRegion = dbRegions.GroupJoin(siteregionName, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();
			var countSiteInRegion = siteregionName.GroupBy(x => x.regionKey).Select(gr => new { regionKey = gr.Key, count = gr.Select(x => x.siteKey).Distinct().Count() });

			var siteConv = siteregionName.Join(posConversionData, si => si.pacid, pos => pos.PACID, (si, pos) =>
				new
				{
					pacid = si.pacid,
					siteKey = si.siteKey,
					siteName = si.siteName,
					regionKey = si.regionKey,
					regionName = si.regionName,
					//dvrDate = pos.DVRDateKey, 
					tHour = pos.tHour,
					//count = si.count,
					trafficIn = pos.TrafficIn,
					trafficOut = pos.TrafficOut,
					totalAmout = pos.TotalAmount,
					countTrans = pos.CountTrans//,
					//conv = !pos.CountTrans.HasValue || !pos.TrafficIn.HasValue || pos.TrafficIn.Value == 0 ? 0 : ((decimal)pos.CountTrans.Value * 100) / (Math.Max(1, (decimal)pos.TrafficIn.Value))
				}).Distinct();

			var siteConvGroupNC = siteConv.GroupBy(cs => new { cs.siteKey, cs.tHour })//, cs.regionKey
				.Select(s => new
				{
					siteKey = s.Key.siteKey,
					//dvrDate = s.Key.dvrDate.HasValue ? s.Key.dvrDate.Value : DateTime.MinValue,
					tHour = s.Key.tHour.HasValue ? s.Key.tHour.Value : 0,
					siteName = s.Any() ? s.FirstOrDefault().siteName : string.Empty,
					regionKey = s.Any() ? s.FirstOrDefault().regionKey : 0,//s.Key.regionKey,
					regionName = s.Any() ? s.FirstOrDefault().regionName : string.Empty,
					trafficIn = s.Any() ? s.Sum(x => x.trafficIn.HasValue ? x.trafficIn.Value : 0) : 0,
					trafficOut = s.Any() ? s.Sum(x => x.trafficOut.HasValue ? x.trafficOut.Value : 0) : 0,
					countTran = s.Any() ? s.Sum(x => x.countTrans.HasValue ? x.countTrans.Value : 0) : 0,
					totalAmount = s.Any() ? s.Sum(x => x.totalAmout.HasValue ? x.totalAmout.Value : 0) : 0//,
					//conv = (s.Any() && s.FirstOrDefault() != null && s.FirstOrDefault().count > 0) ? (s.Sum(c => (c.conv > 150 ? 0 : c.conv)) / s.FirstOrDefault().count) : 0
				});

			//var siteConvGroup = siteConvGroupNC.Select(s => new
			//	{
			//		siteKey = s.siteKey,
			//		tHour = s.tHour,
			//		siteName = s.siteName,
			//		regionKey = s.regionKey,
			//		regionName = s.regionName,
			//		trafficIn = (decimal)s.trafficIn,
			//		trafficOut = (decimal)s.trafficOut,
			//		countTran = (decimal)s.countTran,
			//		totalAmount = s.totalAmount,
			//		conv = (decimal)((s.trafficIn == 0 || s.countTran == 0) ? 0 : (((decimal)s.countTran * 100) / s.trafficIn) > 150 ? 0 : ((decimal)s.countTran * 100) / s.trafficIn)
			//	});
			var siteConvGroup = siteConvGroupNC.Join(countSiteInRegion, s => s.regionKey, reg => reg.regionKey, (s, rg) => new 
			{
				Hour = s.tHour,
				SiteKey = s.siteKey,
				SiteName = s.siteName,
				RegionKey = s.regionKey,
				RegionName = s.regionName,
				CountSite = Math.Max(rg.count, 1),
				TrafficIn = s.trafficIn,
				TrafficOut = s.trafficOut,
				CountTrans = s.countTran,
				TotalAmount = s.totalAmount,
				Conversion = (s.trafficIn == 0 || s.countTran == 0) ? 0 : ((s.countTran * 100) / (decimal)s.trafficIn) > 150 ? 0 : (((decimal)s.countTran * 100) / (decimal)s.trafficIn)
			}).Distinct();

			var Hours = siteConvGroup.Select(s => s.Hour).OrderBy(x => x).Distinct();
			var SiteID = siteConvGroup.Select(s => new { ID = s.SiteKey, Name = s.SiteName, ParentKey = s.RegionKey }).Distinct();
			var siteHourList = (from hour in Hours
						   from site in SiteID
						   select new { Hour = hour, ID = site.ID, Name = site.Name, ParentKey = site.ParentKey}).ToList();
			
			#region Site Data
			IEnumerable<SaleReportData> convBySites = siteConvGroup.Select(x => new SaleReportData()
			{
				ID = x.SiteKey,
				Name = x.SiteName,
				//Date = x.dvrDate,
				Hour = x.Hour,
				Conversion = x.Conversion,
				CountTrans = x.CountTrans,
				TrafficIn = x.TrafficIn,
				TrafficOut = x.TrafficOut,
				TotalAmount = x.TotalAmount,
				isRegion = false,
				ParentKey = x.RegionKey
			}).OrderBy(x => x.Hour);

			IEnumerable<SaleReportData> convSites = (from si in siteHourList
													join posdata in convBySites
													on new { si.ID, si.Hour } equals new { posdata.ID, posdata.Hour } into tbPosData
													from conv in tbPosData.DefaultIfEmpty()
													select new SaleReportData()
													{
														ID = si.ID,
														Name = si.Name,
														Hour = si.Hour,
														Conversion = conv == null ? 0 : conv.Conversion,
														CountTrans = conv == null ? 0 : conv.CountTrans,
														TrafficIn = conv == null ? 0 : conv.TrafficIn,
														TrafficOut = conv == null ? 0 : conv.TrafficOut,
														TotalAmount = conv == null ? 0 : conv.TotalAmount,
														isRegion = false,
														ParentKey = si.ParentKey,
														TimeIndex = si.Hour
													}).ToList();

			IEnumerable<SaleReportSummary> convBySiteSum = convBySites.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x=>x.Conversion > 0).Any() ? s.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0,
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convSites.Where(x => x.ID == s.Key).Distinct().OrderBy(x=>x.Hour)
				}).ToList();
			#endregion

			#region Region Data
			var regionGroup = siteConvGroup.GroupBy(x => new { x.RegionKey, x.Hour }).
				Select(s => new
				{
					ID = s.Key.RegionKey,
					Name = s.Any() ? s.FirstOrDefault().RegionName : string.Empty,
					Hour = s.Key.Hour,//s.Any() ? s.FirstOrDefault().tHour : 0,
					Conversion = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.Conversion) / s.FirstOrDefault().CountSite : 0, //s.Average(x => x.Conversion)
					Transaction = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.CountTrans) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x=>x.CountTrans) : 0,
					TrafficIn = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficIn) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x=>x.TrafficIn) : 0,
					TrafficOut = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficOut) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.TrafficOut) : 0,
					Amount = s.Any() ? s.Sum(x=>x.TotalAmount) : 0,
					isRegion = true,
					ParentKey = 0
				});

			IEnumerable<SaleReportData> convByRegions = regionGroup.Select( s => new SaleReportData()
				{
					ID = s.ID,
					Name = s.Name,
					Hour = s.Hour,
					Conversion = s.Conversion,
					CountTrans = s.Transaction,
					TrafficIn = s.TrafficIn,
					TrafficOut = s.TrafficOut,
					TotalAmount = s.Amount,
					isRegion = true,
					ParentKey = 0
				}).ToList();

			var RegionID = siteConvGroup.Select(s => new { ID = s.RegionKey, Name = s.RegionName }).Distinct();
			var regionHourList = (from hour in Hours
								  from region in RegionID
								  select new { Hour = hour, ID = region.ID, Name = region.Name }).ToList();


			IEnumerable<SaleReportData> convRegionAll = (from rg in regionHourList
														 join conv in convByRegions
														 on new { rg.Hour, rg.ID } equals new { conv.Hour, conv.ID } into dbConvRegions
														 from posRegion in dbConvRegions.DefaultIfEmpty()
														 select new SaleReportData()
														 {
															 ID = rg.ID,
															 Name = rg.Name,
															 Hour = rg.Hour,
															 Conversion = posRegion == null ? 0 : posRegion.Conversion,
															 CountTrans = posRegion == null ? 0 : posRegion.CountTrans,
															 TrafficIn = posRegion == null ? 0 : posRegion.TrafficIn,
															 TrafficOut = posRegion == null ? 0 : posRegion.TrafficOut,
															 TotalAmount = posRegion == null ? 0 : posRegion.TotalAmount,
															 isRegion = false,
															 ParentKey = posRegion == null ? 0 : posRegion.ParentKey,
															 TimeIndex = rg.Hour
														 }).ToList();

			IEnumerable<SaleReportSummary> convByRegionSum = convByRegions.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x=>x.Conversion > 0).Any() ? s.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0,
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.Hour)
				}).ToList();

			foreach (SaleReportSummary data in convByRegionSum)
			{
				data.Sites = convBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			resultData.AddRange(convByRegionSum);
			#endregion

			#region Total Sum Data
			SaleRptTotalSumBase totalSumSite = convBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSumBase totalSumRegion = convByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSum totalSum = new SaleRptTotalSum()
			{
				TotalRegion = totalSumRegion,
				TotalSite = totalSumSite
			};
			#endregion

			#region Chart Data
			int minTime = siteConvGroup.Any() ? siteConvGroup.Min(x => x.Hour) : -1;
			int maxTime = siteConvGroup.Any() ? siteConvGroup.Max(x => x.Hour) : -1;
			IEnumerable<int> fullHours = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullHours = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<SaleReportChart> chartByRegs = null;
			IEnumerable<SaleReportChart> chartBySites = null;
			if (fullHours != null && fullHours.Any())
			{
				var regionHourFull = (from hour in fullHours
									  from region in RegionID
									  select new { Hour = hour, ID = region.ID, Name = region.Name }).ToList();
				IEnumerable<SaleReportData> chartRegionAll = (from rg in regionHourFull
															  join conv in convByRegions
															  on new { rg.Hour, rg.ID } equals new { conv.Hour, conv.ID } into dbConvRegions
															  from posRegion in dbConvRegions.DefaultIfEmpty()
															  select new SaleReportData()
															  {
																  ID = rg.ID,
																  Name = rg.Name,
																  Hour = rg.Hour,
																  TimeIndex = rg.Hour,
																  Conversion = posRegion == null ? 0 : posRegion.Conversion,
																  CountTrans = posRegion == null ? 0 : posRegion.CountTrans,
																  TrafficIn = posRegion == null ? 0 : posRegion.TrafficIn,
																  TrafficOut = posRegion == null ? 0 : posRegion.TrafficOut,
																  TotalAmount = posRegion == null ? 0 : posRegion.TotalAmount,
																  isRegion = false,
																  ParentKey = posRegion == null ? 0 : posRegion.ParentKey
															  }).ToList();

				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
					gr => new SaleReportChart()
					{
						ID = gr.Key,
						Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
						Details = chartRegionAll.Where(x => x.ID == gr.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format(Consts.CHART_LEGEND_HOUR_FORMAT, x.Hour, x.Hour + 1),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.Hour
						}).OrderBy(x => x.TimeIndex)
					});

				var siteHourFull = (from hour in fullHours
									from site in SiteID
									select new { Hour = hour, ID = site.ID, Name = site.Name, ParentKey = site.ParentKey });
				IEnumerable<SaleReportData> chartSiteAll = (from si in siteHourFull
															join posdata in convBySites
															on new { si.ID, si.Hour } equals new { posdata.ID, posdata.Hour } into tbPosData
															from conv in tbPosData.DefaultIfEmpty()
															select new SaleReportData()
															{
																ID = si.ID,
																Name = si.Name,
																Hour = si.Hour,
																Conversion = conv == null ? 0 : conv.Conversion,
																CountTrans = conv == null ? 0 : conv.CountTrans,
																TrafficIn = conv == null ? 0 : conv.TrafficIn,
																TrafficOut = conv == null ? 0 : conv.TrafficOut,
																TotalAmount = conv == null ? 0 : conv.TotalAmount,
																isRegion = false,
																ParentKey = si.ParentKey
															}).ToList();

				chartBySites = convBySites.GroupBy(x => x.ID).Select(
					sg => new SaleReportChart()
					{
						ID = sg.Key,
						Name = sg.Any() ? sg.FirstOrDefault().Name : string.Empty,
						Details = chartSiteAll.Where(x => x.ID == sg.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format(Consts.CHART_LEGEND_HOUR_FORMAT, x.Hour, x.Hour + 1),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.Hour
						}).OrderBy(x => x.TimeIndex)
					});
			}
			//var convByDate = convBySites.GroupBy(x => x.Hour).Select(
			//	gr => new SaleReportChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = string.Format(Consts.CHART_LEGEND_HOUR_FORMAT, gr.Key, gr.Key + 1),//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
			//		Conversion = gr.Any() ? gr.Average(x => x.Conversion) : 0,
			//		CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
			//		TotalAmount = gr.Any() ? gr.Sum(x => x.TotalAmount) : 0,
			//		TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
			//		TrafficOut = gr.Any() ? gr.Sum(x => x.TrafficOut) : 0,
			//		Regions = convByRegions.Where(x => x.Hour == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.Hour,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		}),
			//		Sites = convSites.Where(x => x.Hour == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			Date = x.Date,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			SaleReportDataAll retData = new SaleReportDataAll();
			retData.SummaryData = resultData;
			retData.TotalSum = totalSum;
			retData.ChartData = new SaleReportChartAll();//convByDate;
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<SaleReportDataAll> GetSaleReportDaily(UserContext userLogin, BAMRptParam pram)
		{
			List<SaleReportSummary> resultData = new List<SaleReportSummary>();

			if (pram.rptDataType == Utils.BAMReportType.Daily)
			{
				pram.sFYDate = pram.sDate;
				pram.eFYDate = pram.eDate;
			}
			else
			{
				tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
				if (pram.rptDataType == Utils.BAMReportType.WTD)
				{
					FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, pram.sDate, fyInfo.FYDateStart.Value);
					pram.sFYDate = fyWeekInfo.StartDate;
					pram.eFYDate = pram.sDate;
				}
				else if (pram.rptDataType == Utils.BAMReportType.PTD)
				{
					List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, pram.eDate, fyInfo.FYDateStart.Value);
					FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x => x.StartDate <= pram.sDate && x.EndDate >= pram.sDate);
					pram.sFYDate = fyPeriod != null ? fyPeriod.StartDate : pram.sDate;
					pram.eFYDate = pram.sDate;
				}
				else if (pram.rptDataType == Utils.BAMReportType.YTD)
				{
					pram.sFYDate = fyInfo.FYDateStart.HasValue ? fyInfo.FYDateStart.Value : pram.sDate;//fyPeriods[0].StartDate;
					pram.eFYDate = fyInfo.FYDateEnd.HasValue && fyInfo.FYDateEnd.Value <= pram.sDate ? fyInfo.FYDateEnd.Value : pram.sDate;//fyPeriods[fyPeriods.Count()-1].EndDate;
				}
			}
			

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await UserSitesBySiteIDsAsync(base.DataService, userLogin, selectedSites);
			var siteConvGroup = await GetDataBySites(sites, pram.sFYDate, pram.eFYDate);
			if (siteConvGroup == null || !siteConvGroup.Any())
			{
				return null;
			}

			List<DateTime> dateSearchList = siteConvGroup.Select(x=>x.DVRDate).Distinct().ToList();
			//var curDate = pram.sFYDate;
			//while (curDate <= pram.eFYDate)
			//{
			//	dateSearchList.Add(curDate);
			//	curDate = curDate.AddDays(1);
			//};

			#region Site Data
			var hasDataSites = siteConvGroup.Select(x => new { x.SiteKey, x.SiteName }).Distinct().ToList();
			var SiteWithDate = (from reg in hasDataSites
								from dd in dateSearchList
								select new { ID = reg.SiteKey, Name = reg.SiteName, Date = dd }).ToList();

			IEnumerable<SaleReportData> convBySites = siteConvGroup.Select(x => new SaleReportData()
			{
				ID = x.SiteKey,
				Name = x.SiteName,
				Date = x.DVRDate,
				Conversion = x.Conversion,
				CountTrans = x.CountTrans,
				TrafficIn = x.TrafficIn,
				TrafficOut = x.TrafficOut,
				TotalAmount = x.TotalAmount,
				isRegion = false,
				ParentKey = x.RegionKey
			});

			IEnumerable<SaleReportData> convSiteDailyAll = (from si in SiteWithDate
															 join cv in convBySites on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
															 from it in dat.DefaultIfEmpty()
															 select new SaleReportData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = si.Date,
																 Conversion = it != null ? it.Conversion : 0,
																 CountTrans = it != null ? it.CountTrans : 0,
																 TrafficIn = it != null ? it.TrafficIn : 0,
																 TrafficOut = it != null ? it.TrafficOut : 0,
																 TotalAmount = it != null ? it.TotalAmount : 0,
																 isRegion = false,
																 ParentKey = it != null ? it.ParentKey : 0
															 }).ToList();

			IEnumerable<SaleReportSummary> convBySiteSum = convBySites.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convSiteDailyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.Date)
				}).ToList();
			#endregion

			#region Region Data
			var hasDataRegions = siteConvGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithDate = (from reg in hasDataRegions
								  from d in dateSearchList
								  select new { ID = reg.RegionKey, Name = reg.RegionName, Date = d.Date }).ToList();

			var regionGroup = siteConvGroup.GroupBy(x => new { x.RegionKey, x.DVRDate }).
				Select(s => new
				{
					ID = s.Key.RegionKey,
					Name = s.Any() ? s.FirstOrDefault().RegionName : string.Empty,
					Date = s.Key.DVRDate,//s.Any() ? s.FirstOrDefault().DVRDate : DateTime.MinValue,
					Conversion = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.Conversion) / s.FirstOrDefault().CountSite : 0, //s.Average(x => x.Conversion)
					CountTrans = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.CountTrans) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.CountTrans) : 0,
					TrafficIn = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficIn) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficOut) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.TrafficOut) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					isRegion = true,
					ParentKey = 0
			});

			IEnumerable<SaleReportData> convByRegionAll = (from si in RegionWithDate
															join cv in regionGroup on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
															 from it in dat.DefaultIfEmpty()
															 select new SaleReportData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = si.Date,
																 Conversion = it != null ? it.Conversion : 0,
																 CountTrans = it != null ? it.CountTrans : 0,
																 TrafficIn = it != null ? it.TrafficIn : 0,
																 TrafficOut = it != null ? it.TrafficOut : 0,
																 TotalAmount = it != null ? it.TotalAmount : 0,
																 isRegion = false,
																 ParentKey = it != null ? it.ParentKey : 0
															 }).ToList();

			IEnumerable<SaleReportSummary> convByRegionSum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x=>x.Conversion > 0).Any() ? s.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convByRegionAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x => x.Date)
				}).ToList();

			foreach(SaleReportSummary data in convByRegionSum)
			{
				data.Sites = convBySiteSum.Where(w => w.ParentKey == data.ID);
			}
			resultData.AddRange(convByRegionSum);
			#endregion

			#region Total Sum Data
			SaleRptTotalSumBase totalSumSite = convBySiteSum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSumBase totalSumRegion = convByRegionSum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSum totalSum = new SaleRptTotalSum()
			{
				TotalRegion = totalSumRegion,
				TotalSite = totalSumSite
			};
			#endregion

			#region Chart Data
			DateTime minDate = siteConvGroup.Any() ? siteConvGroup.Min(x => x.DVRDate) : DateTime.MinValue;
			DateTime maxDate = siteConvGroup.Any() ? siteConvGroup.Max(x => x.DVRDate) : DateTime.MinValue;
			IEnumerable<DateTime> fullDates = null;
			if (minDate > DateTime.MinValue && maxDate >= minDate)
			{
				int iCount = Convert.ToInt32((maxDate - minDate).TotalDays);
				fullDates = ArrayUtilities.SequenceDate(minDate, iCount);
			}
			IEnumerable<SaleReportChart> chartByRegs = null;
			IEnumerable<SaleReportChart> chartBySites = null;
			if (fullDates != null && fullDates.Any())
			{
				var RegionFullDate = (from reg in hasDataRegions
									  from d in fullDates
									  select new { ID = reg.RegionKey, Name = reg.RegionName, Date = d.Date }).ToList();
				IEnumerable<SaleReportData> chartByRegionAll = (from si in RegionFullDate
																join cv in regionGroup on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																from it in dat.DefaultIfEmpty()
																select new SaleReportData()
																{
																	ID = si.ID,
																	Name = si.Name,
																	Date = si.Date,
																	Conversion = it != null ? it.Conversion : 0,
																	CountTrans = it != null ? it.CountTrans : 0,
																	TrafficIn = it != null ? it.TrafficIn : 0,
																	TrafficOut = it != null ? it.TrafficOut : 0,
																	TotalAmount = it != null ? it.TotalAmount : 0,
																	isRegion = false,
																	ParentKey = it != null ? it.ParentKey : 0
																}).ToList();
				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
					gr => new SaleReportChart()
					{
						ID = gr.Key,
						Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
						Details = chartByRegionAll.Where(x => x.ID == gr.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = x.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut
						}).OrderBy(x => x.Date)
					});

				var SiteFullDates = (from reg in hasDataSites
									 from dd in fullDates
									 select new { ID = reg.SiteKey, Name = reg.SiteName, Date = dd }).ToList();
				IEnumerable<SaleReportData> chartSiteDailyAll = (from si in SiteFullDates
																 join cv in convBySites on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																 from it in dat.DefaultIfEmpty()
																 select new SaleReportData()
																 {
																	 ID = si.ID,
																	 Name = si.Name,
																	 Date = si.Date,
																	 Conversion = it != null ? it.Conversion : 0,
																	 CountTrans = it != null ? it.CountTrans : 0,
																	 TrafficIn = it != null ? it.TrafficIn : 0,
																	 TrafficOut = it != null ? it.TrafficOut : 0,
																	 TotalAmount = it != null ? it.TotalAmount : 0,
																	 isRegion = false,
																	 ParentKey = it != null ? it.ParentKey : 0
																 }).ToList();
				chartBySites = convBySites.GroupBy(x => x.ID).Select(
					sg => new SaleReportChart()
					{
						ID = sg.Key,
						Name = sg.Any() ? sg.FirstOrDefault().Name : string.Empty,
						Details = chartSiteDailyAll.Where(x => x.ID == sg.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = x.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut
						}).OrderBy(x => x.Date)
					});
			}
			/*
			var convByDate = convBySites.GroupBy(x => x.Date).Select(
				gr => new SaleReportChart()
				{
					Date = gr.Key,
					Name = gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
					Conversion = gr.Any() ? gr.Average(x => x.Conversion) : 0,
					CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
					TotalAmount = gr.Any() ? gr.Sum(x => x.TotalAmount) : 0,
					TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
					TrafficOut = gr.Any() ? gr.Sum(x => x.TrafficOut) : 0,
					Regions = convByRegionAll.Where(x => x.Date == gr.Key).Select(x => new SaleReportChart()
					{
						ID = x.ID,
						Name = x.Name,
						Date = x.Date,
						Conversion = x.Conversion,
						CountTrans = x.CountTrans,
						TotalAmount = x.TotalAmount,
						TrafficIn = x.TrafficIn,
						TrafficOut = x.TrafficOut
					}),
					Sites = convSiteDailyAll.Where(x => x.Date == gr.Key).Select(x => new SaleReportChart()
					{
						ID = x.ID,
						Name = x.Name,
						Date = x.Date,
						Conversion = x.Conversion,
						CountTrans = x.CountTrans,
						TotalAmount = x.TotalAmount,
						TrafficIn = x.TrafficIn,
						TrafficOut = x.TrafficOut
					})
				}).OrderBy(x=>x.Date);
			*/
			#endregion

			SaleReportDataAll retData = new SaleReportDataAll();
			retData.SummaryData = resultData;
			retData.TotalSum = totalSum;
			retData.ChartData = new SaleReportChartAll();//convByDate;
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<SaleReportDataAll> GetSaleReportWeekly(UserContext userLogin, BAMRptParam pram)
		{
			List<SaleReportSummary> resultData = new List<SaleReportSummary>();
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, pram.eDate, fyInfo.FYDateStart.Value);
			//FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, pram.sDate, fyInfo.FYDateStart.Value);
			FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x=>x.StartDate <= pram.sDate && x.EndDate >= pram.sDate); //Get data Pos by fyPeriod start date and end date
			DateTime startDate = fyPeriod != null ? fyPeriod.StartDate : pram.sDate;
			DateTime endDate = (fyPeriod != null && fyPeriod.EndDate <= pram.eDate) ? fyPeriod.EndDate : pram.eDate;

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, userLogin, selectedSites);
			var siteConvGroup = await GetDataBySites(sites, startDate, endDate);
			if (siteConvGroup == null || !siteConvGroup.Any())
			{
				return null;
			}

			var convSiteWeek = (from fyweek in fyPeriod.Weeks
								from convSite in siteConvGroup
								where convSite.DVRDate >= fyweek.StartDate && convSite.DVRDate <= fyweek.EndDate
								select new
								{
									ID = convSite.SiteKey,
									Name = convSite.SiteName,
									Date = convSite.DVRDate,
									Conversion = convSite.Conversion,
									CountTrans = convSite.CountTrans,
									TrafficIn = convSite.TrafficIn,
									TrafficOut = convSite.TrafficOut,
									TotalAmount = convSite.TotalAmount,
									isRegion = false,
									ParentKey = convSite.RegionKey,
									ParentName = convSite.RegionName,
									CountSite = convSite.CountSite,
									Week = fyweek
								}).ToList();
			List<FiscalWeek> weekSearchList = convSiteWeek.Select(x => x.Week).Distinct().ToList();
			#region Site Data
			var hasDataSites = siteConvGroup.Select(x => new { x.SiteKey, x.SiteName }).Distinct().ToList();
			var SiteWithWeek = (from reg in hasDataSites
								from p in weekSearchList//fyPeriod.Weeks
								select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p.WeekIndex, week = p }).ToList();

			var convSiteWeekly = convSiteWeek.GroupBy(x => new { x.ID, x.Week.WeekIndex })
				.Select(s => new 
				{
					ID = s.Key.ID,
					Name = s.Any()? s.FirstOrDefault().Name: string.Empty,
					Date = s.Any()? s.FirstOrDefault().Date: DateTime.MinValue,
					CountSite = s.Any()? s.FirstOrDefault().CountSite: 0,
					Conversion = s.Any() && s.Where(x=>x.Conversion > 0).Any() ? s.Where(x=>x.Conversion > 0).Average(x => x.Conversion) : 0, //s.Average(x => x.Conversion)
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					isRegion = false,
					ParentKey = s.Any()? s.FirstOrDefault().ParentKey: 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Title = s.Any() ? string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", s.FirstOrDefault().Week.StartDate, s.FirstOrDefault().Week.EndDate, s.FirstOrDefault().Week.WeekIndex) : string.Empty,
					TimeIndex = s.Key.WeekIndex
				}).ToList();

			IEnumerable<SaleReportData> convSiteWeeklyAll = (from si in SiteWithWeek
															 join cv in convSiteWeekly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															 from it in dat.DefaultIfEmpty()
															 select new SaleReportData()
															 {
																 ID = si.ID,
																 Name = si.Name,
																 Date = it != null ? it.Date : DateTime.MinValue,
																 Conversion = it != null ? it.Conversion : 0,
																 CountTrans = it != null ? it.CountTrans : 0,
																 TrafficIn = it != null ? it.TrafficIn : 0,
																 TrafficOut = it != null ? it.TrafficOut : 0,
																 TotalAmount = it != null ? it.TotalAmount : 0,
																 isRegion = false,
																 ParentKey = it != null ? it.ParentKey : 0,
																 Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.week.StartDate, si.week.EndDate, si.week.WeekIndex),
																 TimeIndex = si.TimeIndex
															 }).ToList();

			IEnumerable<SaleReportSummary> convBySiteWeeklySum = convSiteWeekly.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convSiteWeeklyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x=>x.TimeIndex)
				}).ToList();
			#endregion

			#region Region Data
			var hasDataRegions = siteConvGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithWeek = (from reg in hasDataRegions //countSiteInRegion.ToList()
								  from p in weekSearchList//fyPeriod.Weeks
								  select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p.WeekIndex, Week = p }).ToList();

			var regionGroup = convSiteWeekly.GroupBy(x => new { x.ParentKey, x.TimeIndex }).
				Select(s => new
				{
					ID = s.Key.ParentKey,
					Name = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Conversion = s.Any() && s.FirstOrDefault().CountSite > 0 ? (s.Sum(x => x.Conversion) / s.FirstOrDefault().CountSite) : 0, //s.Average(x => x.Conversion)
					Transaction = s.Any() && s.FirstOrDefault().CountSite > 0 ? (s.Sum(x => x.CountTrans) / s.FirstOrDefault().CountSite) : 0, // ? s.Sum(x => x.CountTrans) : 0,
					TrafficIn = s.Any() && s.FirstOrDefault().CountSite > 0 ? (s.Sum(x => x.TrafficIn) / s.FirstOrDefault().CountSite) : 0, // ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() && s.FirstOrDefault().CountSite > 0 ? (s.Sum(x => x.TrafficOut) / s.FirstOrDefault().CountSite) : 0, // ? s.Sum(x => x.TrafficOut) : 0,
					Amount = s.Any() ? s.Sum(x=>x.TotalAmount) : 0,
					isRegion = true,
					ParentKey = 0,
					TimeIndex = s.Key.TimeIndex
				});
			
			IEnumerable<SaleReportData> convRegionWeekly = (from reg in RegionWithWeek
															join cv in regionGroup on new { reg.ID, reg.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															from it in dat.DefaultIfEmpty()
															select new SaleReportData()
															{
																ID = reg.ID,
																Name = reg.Name,
																Date = it != null ? it.Date : DateTime.MinValue,
																Conversion = it != null ? it.Conversion : 0,
																CountTrans = it != null ? it.Transaction : 0,
																TrafficIn = it != null ? it.TrafficIn : 0,
																TrafficOut = it != null ? it.TrafficOut : 0,
																TotalAmount = it != null ? it.Amount : 0,
																isRegion = false,
																ParentKey = 0,
																Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", reg.Week.StartDate, reg.Week.EndDate, reg.Week.WeekIndex),
																TimeIndex = reg.TimeIndex
															}).ToList();

			IEnumerable<SaleReportSummary> convByRegionWeeklySum = convRegionWeekly.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convRegionWeekly.Where(x => x.ID == s.Key).Distinct().OrderBy(x=>x.TimeIndex)
				}).ToList();

			foreach (SaleReportSummary data in convByRegionWeeklySum)
			{
				data.Sites = convBySiteWeeklySum.Where(w => w.ParentKey == data.ID);
			}

			resultData.AddRange(convByRegionWeeklySum);
			#endregion

			#region Total Sum Data
			SaleRptTotalSumBase totalSumSite = convBySiteWeeklySum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSumBase totalSumRegion = convByRegionWeeklySum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSum totalSum = new SaleRptTotalSum()
			{
				TotalRegion = totalSumRegion,
				TotalSite = totalSumSite
			};
			#endregion

			#region Chart Data
			int minTime = convSiteWeekly.Any() ? convSiteWeekly.Min(x => x.TimeIndex) : -1;
			int maxTime = convSiteWeekly.Any() ? convSiteWeekly.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullWeeks = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullWeeks = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<SaleReportChart> chartByRegs = null;
			IEnumerable<SaleReportChart> chartBySites = null;
			if (fullWeeks != null && fullWeeks.Any())
			{
				var RegionFullWeek = (from reg in hasDataRegions //countSiteInRegion.ToList()
									  from p in fullWeeks
									  select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p }).ToList();
				IEnumerable<SaleReportData> chartRegionWeekly = (from reg in RegionFullWeek
																join cv in regionGroup on new { reg.ID, reg.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																from it in dat.DefaultIfEmpty()
																select new SaleReportData()
																{
																	ID = reg.ID,
																	Name = reg.Name,
																	Date = it != null ? it.Date : DateTime.MinValue,
																	Conversion = it != null ? it.Conversion : 0,
																	CountTrans = it != null ? it.Transaction : 0,
																	TrafficIn = it != null ? it.TrafficIn : 0,
																	TrafficOut = it != null ? it.TrafficOut : 0,
																	TotalAmount = it != null ? it.Amount : 0,
																	isRegion = false,
																	ParentKey = 0,
																	Title = string.Format("{0}", reg.TimeIndex),
																	TimeIndex = reg.TimeIndex
																}).ToList();

				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
					gr => new SaleReportChart()
					{
						ID = gr.Key,
						Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
						Details = chartRegionWeekly.Where(x => x.ID == gr.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format("Week {0}", x.TimeIndex),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.TimeIndex
						}).OrderBy(x => x.TimeIndex)
					});

				var SiteFullWeek = (from reg in hasDataSites
									from p in fullWeeks
									select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p }).ToList();
				IEnumerable<SaleReportData> chartSiteWeeklyAll = (from si in SiteFullWeek
																 join cv in convSiteWeekly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																 from it in dat.DefaultIfEmpty()
																 select new SaleReportData()
																 {
																	 ID = si.ID,
																	 Name = si.Name,
																	 Date = it != null ? it.Date : DateTime.MinValue,
																	 Conversion = it != null ? it.Conversion : 0,
																	 CountTrans = it != null ? it.CountTrans : 0,
																	 TrafficIn = it != null ? it.TrafficIn : 0,
																	 TrafficOut = it != null ? it.TrafficOut : 0,
																	 TotalAmount = it != null ? it.TotalAmount : 0,
																	 isRegion = false,
																	 ParentKey = it != null ? it.ParentKey : 0,
																	 Title = string.Format("{0}", si.TimeIndex),
																	 TimeIndex = si.TimeIndex
																 }).ToList();
				chartBySites = convSiteWeekly.GroupBy(x => x.ID).Select(
					sg => new SaleReportChart()
					{
						ID = sg.Key,
						Name = sg.Any() ? sg.FirstOrDefault().Name : string.Empty,
						Details = chartSiteWeeklyAll.Where(x => x.ID == sg.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format("Week {0}", x.TimeIndex),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.TimeIndex
						}).OrderBy(x => x.TimeIndex)
					});
			}
			//var convByWeekChart = convSiteWeekly.GroupBy(x => x.TimeIndex).Select(
			//	gr => new SaleReportChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = string.Format("Week {0}", gr.Key),//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
			//		Conversion = gr.Any() ? gr.Average(x => x.Conversion) : 0,
			//		CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
			//		TotalAmount = gr.Any() ? gr.Sum(x => x.TotalAmount) : 0,
			//		TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
			//		TrafficOut = gr.Any() ? gr.Sum(x => x.TrafficOut) : 0,
			//		Regions = convRegionWeekly.Where(x => x.TimeIndex == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.Hour,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		}),
			//		Sites = convSiteWeeklyAll.Where(x => x.TimeIndex == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			Date = x.Date,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			SaleReportDataAll retData = new SaleReportDataAll();
			retData.SummaryData = resultData;
			retData.TotalSum = totalSum;
			retData.ChartData = new SaleReportChartAll();//convByWeekChart;
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<SaleReportDataAll> GetSaleReportMonthly(UserContext userLogin, BAMRptParam pram)
		{
			List<SaleReportSummary> resultData = new List<SaleReportSummary>();
			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, pram.eDate, fyInfo.FYDateStart.Value);
			fyPeriods = fyPeriods.Where(x=>x.StartDate < pram.eDate).ToList();

			DateTime sdate = fyInfo.FYDateStart.HasValue ? fyInfo.FYDateStart.Value : pram.sDate;
			DateTime edate = (fyInfo.FYDateEnd.HasValue && fyInfo.FYDateEnd.Value <= pram.eDate) ? fyInfo.FYDateEnd.Value : pram.eDate;

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(base.DataService, userLogin, selectedSites);
			var siteConvGroup = await GetDataBySites(sites, sdate, edate);
			if (siteConvGroup == null || !siteConvGroup.Any())
			{
				return null;
			}

			var convSitePeriod = (from period in fyPeriods
								  from convSite in siteConvGroup
								  where convSite.DVRDate >= period.StartDate && convSite.DVRDate <= period.EndDate
								  select new
								  {
									  ID = convSite.SiteKey,
									  Name = convSite.SiteName,
									  Date = convSite.DVRDate,
									  Conversion = convSite.Conversion,
									  CountTrans = convSite.CountTrans,
									  TrafficIn = convSite.TrafficIn,
									  TrafficOut = convSite.TrafficOut,
									  TotalAmount = convSite.TotalAmount,
									  isRegion = false,
									  ParentKey = convSite.RegionKey,
									  ParentName = convSite.RegionName,
									  CountSite = convSite.CountSite,
									  Period = period
								  }).ToList();

			List<FiscalPeriod> periodSearchList = convSitePeriod.Select(x => x.Period).Distinct().ToList();
			#region Site Data
			var hasDataSites = siteConvGroup.Select(x => new {x.SiteKey, x.SiteName} ).Distinct().ToList();
			var SiteWithPeriod = (from reg in hasDataSites
								  from p in periodSearchList//fyPeriods
								  select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p.Period, Period = p }).ToList();

			var convSitePeriodly = convSitePeriod.GroupBy(x => new { x.ID, x.Period.Period })
				.Select(s => new 
				{
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					CountSite = s.Any() ? s.FirstOrDefault().CountSite : 0,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					isRegion = false,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Title = s.Any() ? string.Format("{0}", s.FirstOrDefault().Period.Period) : string.Empty,
					TimeIndex = s.Key.Period
				});

			IEnumerable<SaleReportData> convSitePeriodlyAll = (from si in SiteWithPeriod
															   join cv in convSitePeriodly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															   from it in dat.DefaultIfEmpty()
															   select new SaleReportData()
															   {
																   ID = si.ID,
																   Name = si.Name,
																   Date = it != null ? it.Date : DateTime.MinValue,
																   Conversion = it != null ? it.Conversion : 0,
																   CountTrans = it != null ? it.CountTrans : 0,
																   TrafficIn = it != null ? it.TrafficIn : 0,
																   TrafficOut = it != null ? it.TrafficOut : 0,
																   TotalAmount = it != null ? it.TotalAmount : 0,
																   isRegion = false,
																   ParentKey = it != null ? it.ParentKey : 0,
																   Title = string.Format("{0}", si.TimeIndex),
																   TimeIndex = si.TimeIndex
															   }).ToList();

			IEnumerable<SaleReportSummary> convBySiteWeeklySum = convSitePeriodly.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.CountTrans) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.TotalAmount) : 0,
					DataDetail = convSitePeriodlyAll.Where(x => x.ID == s.Key).Distinct().OrderBy(x=>x.TimeIndex)
				}).ToList();
			#endregion

			#region Region Data
			var hasDataRegions = siteConvGroup.Select(x => new { x.RegionKey, x.RegionName }).Distinct().ToList();
			var RegionWithPeriod = (from reg in hasDataRegions //countSiteInRegion.ToList()
									from p in periodSearchList//fyPeriods
									select new { ID = reg.RegionKey, Name = reg.RegionName, TimeIndex = p.Period, Period = p }).ToList();

			var regionGroup = convSitePeriodly.GroupBy(x => new { x.ParentKey, x.TimeIndex }).
				Select(s => new
				{
					ID = s.Key.ParentKey,
					Name = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					Conversion = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.Conversion) / s.FirstOrDefault().CountSite : 0, //s.Average(x => x.Conversion)
					Transaction = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.CountTrans) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.CountTrans) : 0,
					TrafficIn = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficIn) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() && s.FirstOrDefault().CountSite > 0 ? s.Sum(x => x.TrafficOut) / s.FirstOrDefault().CountSite : 0, // ? s.Sum(x => x.TrafficOut) : 0,
					Amount = s.Any() ? s.Sum(x=>x.TotalAmount) : 0,
					isRegion = true,
					ParentKey = 0,
					TimeIndex = s.Key.TimeIndex
				});

			IEnumerable<SaleReportData> convRegionMonthly = (from reg in RegionWithPeriod
															 join cv in regionGroup on new { reg.ID, reg.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															 from it in dat.DefaultIfEmpty()
															 select new SaleReportData()
															 {
																 ID = reg.ID,
																 Name = reg.Name,
																 Date = it != null ? it.Date : DateTime.MinValue,
																 Conversion = it != null ? it.Conversion : 0,
																 CountTrans = it != null ? it.Transaction : 0,
																 TrafficIn = it != null ? it.TrafficIn : 0,
																 TrafficOut = it != null ? it.TrafficOut : 0,
																 TotalAmount = it != null ? it.Amount : 0,
																 isRegion = false,
																 ParentKey = 0,
																 Title = String.Format("{0}", reg.TimeIndex),
																 TimeIndex = reg.TimeIndex
															 }).ToList();

			IEnumerable<SaleReportSummary> convByRegionWeeklySum = regionGroup.GroupBy(x => x.ID)
				.Select(s => new SaleReportSummary()
				{
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentKey = s.Any() ? s.FirstOrDefault().ParentKey : 0,
					isRegion = s.Any() ? s.FirstOrDefault().isRegion : false,
					Conversion = s.Any() && s.Where(x => x.Conversion > 0).Any() ? s.Where(x => x.Conversion > 0).Average(x => x.Conversion) : 0,//s.Average(x => x.Conversion)
					TrafficIn = s.Any() ? s.Sum(x => x.TrafficIn) : 0,
					TrafficOut = s.Any() ? s.Sum(x => x.TrafficOut) : 0,
					CountTrans = s.Any() ? s.Sum(x => x.Transaction) : 0,
					TotalAmount = s.Any() ? s.Sum(x => x.Amount) : 0,
					DataDetail = convRegionMonthly.Where(x => x.ID == s.Key).Distinct().OrderBy(x=>x.TimeIndex)
				}).ToList();

			foreach (SaleReportSummary data in convByRegionWeeklySum)
			{
				data.Sites = convBySiteWeeklySum.Where(w => w.ParentKey == data.ID);
			}

			resultData.AddRange(convByRegionWeeklySum);
			#endregion

			#region Total Sum Data
			SaleRptTotalSumBase totalSumSite = convBySiteWeeklySum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSumBase totalSumRegion = convByRegionWeeklySum.GroupBy(x => x.TimeIndex)
				.Select(s => new SaleRptTotalSumBase()
				{
					TotalConv = s.Any() ? s.Sum(i => i.Conversion) : 0,
					TotalTrans = s.Any() ? s.Sum(i => i.CountTrans) : 0,
					TotalIn = s.Any() ? s.Sum(i => i.TrafficIn) : 0,
					TotalOut = s.Any() ? s.Sum(i => i.TrafficOut) : 0,
					TotalAmout = s.Any() ? s.Sum(i => i.TotalAmount) : 0
				}).FirstOrDefault();

			SaleRptTotalSum totalSum = new SaleRptTotalSum()
			{
				TotalRegion = totalSumRegion,
				TotalSite = totalSumSite
			};
			#endregion

			#region Chart Data
			int minTime = convSitePeriodly.Any() ? convSitePeriodly.Min(x => x.TimeIndex) : -1;
			int maxTime = convSitePeriodly.Any() ? convSitePeriodly.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullPeriods = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullPeriods = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			IEnumerable<SaleReportChart> chartByRegs = null;
			IEnumerable<SaleReportChart> chartBySites = null;
			if (fullPeriods != null && fullPeriods.Any())
			{
				chartByRegs = regionGroup.GroupBy(x => x.ID).Select(
					gr => new SaleReportChart()
					{
						ID = gr.Key,
						Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
						Details = convRegionMonthly.Where(x => x.ID == gr.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format("Period {0}", x.TimeIndex),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.TimeIndex
						}).OrderBy(x => x.TimeIndex)
					});

				var SiteFullPeriod = (from reg in hasDataSites
									  from p in fullPeriods
									  select new { ID = reg.SiteKey, Name = reg.SiteName, TimeIndex = p }).ToList();
				IEnumerable<SaleReportData> chartSitePeriodlyAll = (from si in SiteFullPeriod
																	join cv in convSitePeriodly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	from it in dat.DefaultIfEmpty()
																	select new SaleReportData()
																	{
																		ID = si.ID,
																		Name = si.Name,
																		Date = it != null ? it.Date : DateTime.MinValue,
																		Conversion = it != null ? it.Conversion : 0,
																		CountTrans = it != null ? it.CountTrans : 0,
																		TrafficIn = it != null ? it.TrafficIn : 0,
																		TrafficOut = it != null ? it.TrafficOut : 0,
																		TotalAmount = it != null ? it.TotalAmount : 0,
																		isRegion = false,
																		ParentKey = it != null ? it.ParentKey : 0,
																		Title = string.Format("{0}", si.TimeIndex),
																		TimeIndex = si.TimeIndex
																	}).ToList();
				chartBySites = convSitePeriodly.GroupBy(x => x.ID).Select(
					sg => new SaleReportChart()
					{
						ID = sg.Key,
						Name = sg.Any() ? sg.FirstOrDefault().Name : string.Empty,
						Details = chartSitePeriodlyAll.Where(x => x.ID == sg.Key).Select(x => new SaleReportChart()
						{
							ID = x.ID,
							Name = string.Format("Period {0}", x.TimeIndex),
							Date = x.Date,
							Conversion = x.Conversion,
							CountTrans = x.CountTrans,
							TotalAmount = x.TotalAmount,
							TrafficIn = x.TrafficIn,
							TrafficOut = x.TrafficOut,
							TimeIndex = x.TimeIndex
						}).OrderBy(x => x.TimeIndex)
					});
			}
			//var convByWeekChart = convSitePeriodly.GroupBy(x => x.TimeIndex).Select(
			//	gr => new SaleReportChart()
			//	{
			//		TimeIndex = gr.Key,
			//		Name = string.Format("Period {0}", gr.Key),//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
			//		Conversion = gr.Any() ? gr.Average(x => x.Conversion) : 0,
			//		CountTrans = gr.Any() ? gr.Sum(x => x.CountTrans) : 0,
			//		TotalAmount = gr.Any() ? gr.Sum(x => x.TotalAmount) : 0,
			//		TrafficIn = gr.Any() ? gr.Sum(x => x.TrafficIn) : 0,
			//		TrafficOut = gr.Any() ? gr.Sum(x => x.TrafficOut) : 0,
			//		Regions = convRegionMonthly.Where(x => x.TimeIndex == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			TimeIndex = x.Hour,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		}),
			//		Sites = convSitePeriodlyAll.Where(x => x.TimeIndex == gr.Key).Select(x => new SaleReportChart()
			//		{
			//			ID = x.ID,
			//			Name = x.Name,
			//			Date = x.Date,
			//			Conversion = x.Conversion,
			//			CountTrans = x.CountTrans,
			//			TotalAmount = x.TotalAmount,
			//			TrafficIn = x.TrafficIn,
			//			TrafficOut = x.TrafficOut
			//		})
			//	}).OrderBy(x=>x.TimeIndex);
			#endregion

			SaleReportDataAll retData = new SaleReportDataAll();
			retData.SummaryData = resultData;
			retData.TotalSum = totalSum;
			retData.ChartData = new SaleReportChartAll();//convByWeekChart;
			retData.ChartData.Regions = chartByRegs;
			retData.ChartData.Sites = chartBySites;

			return retData;
		}

		private async Task<IEnumerable<BamDataBySite>> GetDataBySites(IEnumerable<UserSiteDvrChannel> sites, DateTime startDate, DateTime endDate)
		{
			if (sites == null || !sites.Any())
			{
				return null;
			}
			List<int> lsPacids = sites.Where(x=>x.PACID.HasValue).Select(s => s.PACID.Value).Distinct().ToList();
			string pacIds = string.Join(",", lsPacids);
			IEnumerable<int> siteKeys = sites.Select(x => x.siteKey.HasValue ? x.siteKey.Value : 0).Distinct();

			IEnumerable<Proc_DashBoard_Conversion_Result> posConversionData = await base.IPOSBusinessService.GetConversionAsync(startDate, endDate, lsPacids);//await DataService.GetPOSConversion(pacIds, startDate, endDate);

			//Get Region List by site id list
			string[] includes = { typeof(tCMSWebRegion).Name };
			IQueryable<tCMSWebSites> dbSites = ISite.GetSites<tCMSWebSites>(siteKeys, item => item, includes).Where(x => x.tCMSWebRegion != null);
			IEnumerable<tCMSWebRegion> dbRegions = dbSites.Select(s => s.tCMSWebRegion);

			var sitepacId = sites.Where(it => it.siteKey.HasValue && it.PACID.HasValue && it.siteKey.Value > 0 && it.PACID.Value > 0).Select(si => new { pacid = si.PACID.Value, siteKey = si.siteKey.Value }).Distinct();
			var pacInSiteCount = sitepacId.GroupJoin(dbSites, s => s.siteKey, sc => sc.siteKey, (s, sc) => new { pacid = s.pacid, sitekey = s.siteKey, siteName = sc.Any() ? sc.FirstOrDefault().ServerID : string.Empty, count = sc.Count() });

			var siteregionId = pacInSiteCount.Join(dbSites, si => si.sitekey, rg => rg.siteKey, (si, rg) => new { site = si, region = rg });
			var siteregionName = siteregionId.Join(dbRegions, si => si.region.RegionKey, rg => rg.RegionKey, (si, rg) => new { pacid = si.site.pacid, siteKey = si.site.sitekey, siteName = si.site.siteName, countpac = si.site.count, regionKey = rg.RegionKey, regionName = rg.RegionName });
			//var countSiteInRegion = dbRegions.GroupJoin(siteregionName, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();
			var countSiteInRegion = siteregionName.GroupBy(x => x.regionKey).Select(gr => new { regionKey = gr.Key, count = gr.Where(x => siteKeys.Contains(x.siteKey)).Select(x => x.siteKey).Distinct().Count() }); //siteregionName, rg => rg.RegionKey, si => si.regionKey, (rg, si) => new { regionKey = rg.RegionKey, regionName = rg.RegionName, count = si.Select(x => x.siteKey).Distinct().Count() }).Distinct();

			var siteConv = posConversionData.Join(siteregionName, pos => pos.PACID, si => si.pacid, (pos, si) =>
				new
				{
					pacid = pos.PACID,
					siteKey = si.siteKey,
					siteName = si.siteName,
					regionKey = si.regionKey,
					regionName = si.regionName,
					dvrDate = pos.DVRDate,
					countpac = si.countpac,
					trafficIn = pos.TrafficIn,
					trafficOut = pos.TrafficOut,
					totalAmout = pos.TotalAmount,
					countTrans = pos.CountTrans//,
					//conv = !pos.CountTrans.HasValue || !pos.TrafficIn.HasValue || pos.TrafficIn.Value == 0 ? 0 : ((decimal)pos.CountTrans.Value * 100) / (Math.Max(1, (decimal)pos.TrafficIn.Value))
				}).Distinct();

			var siteConvGroupNoConv = siteConv.GroupBy(cs => new { cs.siteKey, cs.dvrDate })//, cs.regionKey
				.Select(s => new
				{
					siteKey = s.Key.siteKey,
					dvrDate = s.Key.dvrDate.HasValue ? s.Key.dvrDate.Value : DateTime.MinValue,
					siteName = s.Any() ? s.FirstOrDefault().siteName : string.Empty,
					regionKey = s.Any() ? s.FirstOrDefault().regionKey : 0,
					regionName = s.Any() ? s.FirstOrDefault().regionName : string.Empty,
					trafficIn = (decimal)(s.Any() ? s.Sum(x => x.trafficIn.HasValue ? x.trafficIn.Value : 0) : 0),
					trafficOut = (decimal)(s.Any() ? s.Sum(x => x.trafficOut.HasValue ? x.trafficOut.Value : 0) : 0),
					countTran = (decimal)(s.Any() ? s.Sum(x => x.countTrans.HasValue ? x.countTrans.Value : 0) : 0),
					totalAmount = s.Any() ? s.Sum(x => x.totalAmout.HasValue ? x.totalAmout.Value : 0) : 0
					//conv = (s.Any() && s.FirstOrDefault() != null && s.FirstOrDefault().count > 0) ? (s.Sum(c => (c.conv > 150 ? 0 : c.conv)) / s.FirstOrDefault().count) : 0
				});

			var ret = siteConvGroupNoConv.Join(countSiteInRegion, s => s.regionKey, reg => reg.regionKey, (s, rg) => new BamDataBySite() 
				{
					DVRDate = s.dvrDate,
					SiteKey = s.siteKey,
					SiteName = s.siteName,
					RegionKey = s.regionKey,
					RegionName = s.regionName,
					CountSite = Math.Max(rg.count, 1),
					TrafficIn = s.trafficIn,
					TrafficOut = s.trafficOut,
					CountTrans = s.countTran,
					TotalAmount = s.totalAmount,
					Conversion = (s.trafficIn == 0 || s.countTran == 0) ? 0 : ((s.countTran * 100) / s.trafficIn) > 150 ? 0 : ((s.countTran * 100) / s.trafficIn)
				}).Distinct();
			//var siteConvGroup = siteConvGroupNoConv.Select(s => new BamDataBySite()
			//{
			//	DVRDate = s.dvrDate,
			//	SiteKey = s.siteKey,
			//	SiteName = s.siteName,
			//	RegionKey = s.regionKey,
			//	RegionName = s.regionName,
			//	TrafficIn = s.trafficIn,
			//	TrafficOut = s.trafficOut,
			//	CountTrans = s.countTran,
			//	TotalAmount = s.totalAmount,
			//	Conversion = (s.trafficIn == 0 || s.countTran == 0) ? 0 : ((s.countTran * 100) / s.trafficIn) > 150 ? 0 : ((s.countTran * 100) / s.trafficIn)
			//}).ToList();

			return ret;
		}
	}
}
