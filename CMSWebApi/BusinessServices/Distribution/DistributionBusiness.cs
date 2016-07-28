using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Distribution
{
	public class DistributionBusiness : BusinessBase<IDistributionService>
	{
		#region properties
		public IUsersService IUser { get; set; }
		public ISiteService ISite { get; set; }
		public IFiscalYearServices IFiscalYear { get; set; }

		Commons.TEqualityComparer<DistributionModel> distcomparer = new Commons.TEqualityComparer<DistributionModel>((a, b) => { 
			return a.PACID == b.PACID 
				& a.Date == b.Date
				& a.ID == b.ID
				& a.Count == b.Count
				& a.DWell == b.DWell
				& a.TimeIndex == b.TimeIndex
				& a.ParentID == b.ParentID
				& a.KDVR == b.KDVR
				& a.ChannelNo == b.ChannelNo; 
		});
		#endregion

		public async Task<DistributionDataAll> GetDataReport(UserContext userLogin, BAMRptParam pram)
		{
			switch (pram.rptDataType)
			{
				case Utils.BAMReportType.Hourly:
					return await GetDistributionRptHourly(userLogin, pram);
				case Utils.BAMReportType.Daily:
				case Utils.BAMReportType.WTD:
				case Utils.BAMReportType.PTD:
					return await GetDistributionRptDaily(userLogin, pram);
				case Utils.BAMReportType.Weekly:
					return await GetDistributionRptWeekly(userLogin, pram);
				case Utils.BAMReportType.YTD:
					return await GetDistributionRptMonthly(userLogin, pram);
				default:
					return null;
			}
		}

		public string GetHeatMapImages(int img)
		{
			var image = DataService.GetImagebyID(img);

			string folderTime = "";
			switch ((int)image.tbl_HM_TaskChannel.tbl_HM_ScheduledTasks.ScheduleType.Value)
			{
				case (int)Utils.BAMReportType.Hourly:
					folderTime = Utils.BAMReportType.Hourly.ToString();
					break;

				case (int)Utils.BAMReportType.Daily:
					folderTime = Utils.BAMReportType.Daily.ToString();
					break;
				case (int)Utils.BAMReportType.Weekly:
					folderTime = Utils.BAMReportType.Weekly.ToString();
					break;

				default:
					folderTime = string.Empty;
					break;
			}

			string root = string.Empty;
			if(image.UploadedBy == null)
			{
				root = Consts.ImagesHeatMap_Schedules;
			}
			else
			{
				root = Consts.ImagesHeatMap_Manual;
			}

			string path = Path.Combine(AppSettings.AppSettings.Instance.AppData, root, image.tbl_HM_TaskChannel.tDVRChannels.KDVR.ToString(), folderTime);//image.tbl_HM_TaskChannel.KChannel.ToString()

			path = Path.Combine(path, image.ImgName);
			return path;
		}

		private async Task<DistributionDataAll> GetDistributionRptHourly(UserContext userLogin, BAMRptParam pram)
		{
			List<DistributionSummary> resultData = new List<DistributionSummary>();

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(IUser, userLogin, selectedSites);
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());

			var siteTrafficCountPre = await DataService.GetTrafficCountHourly(pacIds, pram.sDate, pram.eDate, userLogin.ID);

			var kdvr_pacid = sites.Select(x => new { x.KDVR, x.PACID }).Distinct();
			var siteTrafficCount = siteTrafficCountPre.Join(kdvr_pacid, tr => tr.PACID, dvr => dvr.PACID, (tr, dvr) => new DistributionModel() 
				{
					Count = tr.Count ?? 0,
					Date = tr.DVRDate ?? DateTime.MinValue,
					TimeIndex = tr.DVRHour ?? 0,
					DWell = tr.DWellTime ?? 0,
					PACID = tr.PACID ?? 0,
					ID = tr.RegionID ?? 0,
					Name = tr.RegionName,
					ParentID = tr.QueueID ?? 0,
					ParentName = tr.QueueName,
					KDVR = dvr.KDVR ?? 0,
					ChannelNo = tr.ChannelNo ?? -1
				});

			//var trafficByQueueNC = siteTrafficCount.Where(x => x.QueueID.HasValue && x.QueueID.Value > 0).Select(s => new DistributionModel() 
			//	{
			//		Count = s.Count ?? 0,
			//		Date = s.DVRDate ?? DateTime.MinValue,
			//		DWell = s.DWellTime ?? 0,
			//		PACID = s.PACID ?? 0,
			//		ID = s.RegionID ?? 0,
			//		Name = s.RegionName,
			//		ParentID = s.QueueID ?? 0,
			//		ParentName = s.QueueName,
			//		TimeIndex = s.DVRHour ?? 0
			//	});
			var trafficByQueueNC = siteTrafficCount.Where(x=>x.ParentID > 0);
			//var countRegion = trafficByQueueNC.GroupBy(x => x.ParentID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x=>x.ID).Distinct().Count() });
			List<TrafficCountRegionInQueue_Result> regionInQueue = await DataService.GetTrafficCountRegionInQueue(pacIds, userLogin.ID);
			var countRegion = regionInQueue.GroupBy(x => x.QueueID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.RegionIndex).Distinct().Count() });
			var trafficByQueue = trafficByQueueNC.Join(countRegion, tr=>tr.ParentID, cn => cn.ParentID, (tr, cn) => new DistributionModel()
				{
					Count = tr.Count,
					Date = tr.Date,
					DWell = tr.DWell,
					PACID = tr.PACID,
					ID = tr.ID,
					Name = tr.Name,
					ParentID = tr.ParentID,
					ParentName = tr.ParentName,
					CountRegion = Math.Max(cn.NumReg, 1),
					TimeIndex = tr.TimeIndex,
					KDVR = tr.KDVR,
					ChannelNo = tr.ChannelNo
				}).Distinct();

			var trafficByRegion = siteTrafficCount.Where(x=>x.ParentID == 0);//.Where(x => !x.QueueID.HasValue || x.QueueID.Value == 0);

			IEnumerable<int> Hours = siteTrafficCount.Select(s => s.TimeIndex).OrderBy(x => x).Distinct();

			DistributionTotalSum totalSum = null;

			#region Region data
			IEnumerable<DistributionSummary> trafficSumBySites = null;
			IEnumerable<DistributionModel> trafficByRegionHour = null;
			if (trafficByRegion != null && trafficByRegion.Any())
			{
				var pacHasData = trafficByRegion.Select(s => new { RegionID = s.ID, RegionName = s.Name }).Distinct().ToList();
				var HourRegionList = from h in Hours
									 from pac in pacHasData
									 select new { TimeIndex = h, ID = pac.RegionID, Name = pac.RegionName };

				trafficByRegionHour = trafficByRegion.GroupBy(x => new { x.ID, x.TimeIndex })
					.Select(gr => new DistributionModel()
					{
						Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
						TimeIndex = gr.Key.TimeIndex,
						Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
						DWell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
						ID = gr.Key.ID,
						Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
						ParentID = gr.Any() ? gr.FirstOrDefault().ParentID : 0,
						ParentName = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
						Title = gr.Any() ? gr.FirstOrDefault().Title : string.Empty,
						KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
						ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0
					});

				var trafficSiteHour = from h in HourRegionList
									  join t in trafficByRegionHour
									  on new { h.TimeIndex, h.ID } equals new { t.TimeIndex, t.ID } into posData
									  from pos in posData.DefaultIfEmpty()
									  select new DistributionModel()
									  {
										  Count = pos == null ? 0 : pos.Count,
										  Date = pos == null ? DateTime.MinValue : pos.Date,
										  TimeIndex = h.TimeIndex,
										  DWell = pos == null ? 0 : Convert.ToInt32(pos.DWell),
										  ID = h.ID,
										  Name = h.Name,
										  Title = (pos == null) ? String.Format("{0}:00 : {1}:00", h.TimeIndex, h.TimeIndex + 1) : pos.Title,
										  KDVR = (pos == null) ? 0 : pos.KDVR,
										  ChannelNo = (pos == null) ? 0 : pos.ChannelNo
									  };

				//var trafficBySite = trafficSiteHour.Select(s => new DistributionModel()
				//{
				//	Count = s.Count ?? 0,
				//	Date = s.DVRDate ?? DateTime.MinValue,
				//	Title = s.HourCol,
				//	DWell = s.DWellTime,
				//	ID = s.RegionID ?? 0,
				//	Name = s.RegionName,
				//	TimeIndex = s.DVRHour
				//});

				trafficSumBySites = trafficByRegionHour.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						DataDetail = trafficSiteHour.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					});

				totalSum = trafficSumBySites.GroupBy(x => x.TimeIndex)
					.Select(s => new DistributionTotalSum()
					{
						TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
						TotalDwell = s.Any() ? s.Sum(i => i.DWell) : 0
					}).FirstOrDefault();
			}
			#endregion

			#region Queue data
			IEnumerable<DistributionSummary> trafficSumByQueue = null;
			IEnumerable<DistributionModel> trafficByQueueHour = null;
			if (trafficByQueue != null && trafficByQueue.Any())
			{
				var queueHasData = trafficByQueue.Select(s => new { QueueID = s.ParentID, QueueName = s.ParentName, CountRegion = s.CountRegion }).Distinct().ToList();
				var HourRegionList = from h in Hours
									 from pac in queueHasData
									 select new { TimeIndex = h, ID = pac.QueueID, Name = pac.QueueName, CountRegion = pac.CountRegion };

				trafficByQueueHour = trafficByQueue.GroupBy(x => new { x.ParentID, x.TimeIndex })
					.Select(gr => new DistributionModel()
					{
						Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
						TimeIndex = gr.Key.TimeIndex,
						Count = !gr.Any() ? 0 : gr.Sum(x => x.Count),// / gr.FirstOrDefault().CountRegion,//gr.Sum(x => x.Count),
						DWell = !gr.Any() ? 0 : gr.Sum(x => x.DWell) / gr.FirstOrDefault().CountRegion,//Convert.ToInt32(gr.Average(x => x.DWell)),
						ID = gr.Key.ParentID,
						Name = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
						CountRegion = gr.Any() ? gr.FirstOrDefault().CountRegion : 0,
						KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
						ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
						Title = gr.Key.TimeIndex.ToString()
					});

				var trafficQueueHourAll = from h in HourRegionList
										  join t in trafficByQueueHour
										  on new { h.TimeIndex, h.ID } equals new { t.TimeIndex, t.ID } into posData
										  from pos in posData.DefaultIfEmpty()
										  select new DistributionModel()
										  {
											  Count = pos == null ? 0 : pos.Count,
											  Date = pos == null ? DateTime.MinValue : pos.Date,
											  TimeIndex = h.TimeIndex,
											  DWell = pos == null ? 0 : pos.DWell,
											  ID = h.ID,
											  Name = pos == null ? string.Empty : pos.Name,
											  CountRegion = h.CountRegion,
											  KDVR = pos == null ? 0 : pos.KDVR,
											  ChannelNo = pos == null ? 0 : pos.ChannelNo,
											  Title = (pos == null) ? String.Format("{0}:00 : {1}:00", h.TimeIndex, h.TimeIndex + 1) : pos.Title
										  };

				trafficSumByQueue = trafficByQueueHour.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						DataDetail = trafficQueueHourAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					}).ToList();

				foreach (DistributionSummary data in trafficSumByQueue)
				{
					var Regions = trafficByQueue.Where(w => w.ParentID == data.ID).Select(x => new DistributionModel()
					{
						Count = x.Count,
						Date = x.Date,
						DWell = x.DWell,
						ID = x.ID,
						Name = x.Name,
						ParentID = x.ParentID,
						ParentName = x.ParentName,
						TimeIndex = x.TimeIndex,
						KDVR = x.KDVR,
						ChannelNo = x.ChannelNo
					});
					data.Regions = CreateRegionData(Regions, Hours);
				}
			}
			#endregion
			List<DistributionSummary> lsContributionDataAll = new List<DistributionSummary>();
			if (trafficSumByQueue != null && trafficSumByQueue.Any())
			{
				lsContributionDataAll.AddRange(trafficSumByQueue);
			}
			if (trafficSumBySites != null && trafficSumBySites.Any())
			{
				lsContributionDataAll.AddRange(trafficSumBySites);
			}

			#region Chart Data
			int minTime = siteTrafficCount.Any() ? siteTrafficCount.Min(x => x.TimeIndex) : -1;
			int maxTime = siteTrafficCount.Any() ? siteTrafficCount.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullHours = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullHours = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			List<DistChartData> chartByRegs = new List<DistChartData>();
			if (fullHours != null && fullHours.Any())
			{
				tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
				DateTime fyStartDate = (fyInfo != null && fyInfo.FYDateStart.HasValue) ? fyInfo.FYDateStart.Value : new DateTime(DateTime.Now.Year, 1, 1);
				List<Func_BAM_TrafficCountReportHourly_Result> trafficCounYTD = await DataService.GetTrafficCountHourly(pacIds, fyStartDate, pram.eDate, userLogin.ID);

				#region Region without queue
				IEnumerable<DistChartData> regionChartData = null;
				if (trafficByRegionHour != null && trafficByRegionHour.Any())
				{
					var regionHasData = trafficByRegionHour.Select(x => new { x.ID, x.Name }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullHours
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p }).ToList();
					IEnumerable<DistributionModel> chartHourlyAll = (from si in RegionFullWeek
																	 join cv in trafficByRegionHour on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 KDVR = (it != null) ? it.KDVR : 0,
																		 ChannelNo = (it != null) ? it.ChannelNo : 0,
																		 Title = string.Format("{0}", si.TimeIndex)
																	 }).ToList();

					regionChartData = trafficByRegionHour.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = gr.Key,
							Details = chartHourlyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									DataYTD = GetDistYTDData(trafficCounYTD, x.ID, x.TimeIndex)
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion

				#region Queue with region
				IEnumerable<DistChartData> queueChartData = null;
				if (trafficByQueueHour != null && trafficByQueueHour.Any())
				{
					var regionHasData = trafficByQueueHour.Select(x => new { x.ID, x.Name, x.CountRegion }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullHours
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p, CountRegion = reg.CountRegion }).ToList();
					IEnumerable<DistributionModel> chartHourlyAll = (from si in RegionFullWeek
																	 join cv in trafficByQueueHour on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 CountRegion = si.CountRegion,
																		 Title = string.Format("{0}", si.TimeIndex)
																	 }).ToList();

					queueChartData = trafficByQueueHour.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = 0,
							Details = chartHourlyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									ItemCount = x.CountRegion,
									//DataYTD = new DriveThroughBase() { Count = x.CountRegion, Dwell = 0 },
									Details = CreateChartDetail(trafficByQueue, x.ID, DateTime.MinValue, x.TimeIndex, trafficCounYTD)//trafficCounYTD
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion
				if (regionChartData != null && regionChartData.Any())
				{
					chartByRegs.AddRange(regionChartData);
				}
				if (queueChartData != null && queueChartData.Any())
				{
					chartByRegs.AddRange(queueChartData);
				}
			}
			//{
			//	var HourRegionFull = from h in fullHours
			//						 from pac in pacHasData
			//						 select new { DVRHour = h, PACID = pac.PACID, RegionID = pac.RegionID, RegionName = pac.RegionName };
			//	var chartRegionHour = from h in HourRegionList
			//						  join t in trafficByRegionHour
			//						  on new { h.DVRHour, h.RegionID } equals new { t.DVRHour, t.RegionID } into posData
			//						  from pos in posData.DefaultIfEmpty()
			//						  select new DistributionModel()
			//						  {
			//							  Count = pos == null ? 0 : pos.Count ?? 0,
			//							  Date = pos == null ? DateTime.MinValue : pos.DVRDate ?? DateTime.MinValue,
			//							  Title = (pos == null) ? String.Format("{0}:00 : {1}:00", h.DVRHour, h.DVRHour + 1) : pos.HourCol,
			//							  DWell = pos == null ? 0 : Convert.ToInt32(pos.DWellTime),
			//							  PACID = h.PACID ?? 0,
			//							  ID = h.RegionID ?? 0,
			//							  Name = h.RegionName,
			//							  TimeIndex = h.DVRHour
			//						  };
			//	chartByRegs = trafficBySite.GroupBy(x => x.ID)
			//		.Select(gr => new DistChartData()
			//		{
			//			Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format(Consts.CHART_LEGEND_HOUR_FORMAT, gr.Key, gr.Key + 1),
			//			ID = gr.Key,
			//			Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
			//			Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
			//			TimeIndex = gr.Any() ? gr.FirstOrDefault().TimeIndex : 0,
			//			Details = chartRegionHour.Where(x => x.ID == gr.Key)
			//				.Select(x => new DistChartData()
			//				{
			//					ID = x.ID,
			//					Name = x.Name,
			//					Count = x.Count,
			//					Dwell = x.DWell,
			//					TimeIndex = x.TimeIndex
			//				}).OrderBy(x => x.TimeIndex)
			//		});//.OrderBy(x=>x.TimeIndex);
			//}
			#endregion

			DistributionDataAll retData = new DistributionDataAll();
			retData.SummaryData = lsContributionDataAll;// trafficSumBySites;
			retData.TotalSum = totalSum;
			retData.ChartData = new DistributionChartAll();
			retData.ChartData.Regions = chartByRegs;

			return retData;
		}

		private async Task<DistributionDataAll> GetDistributionRptDaily(UserContext userLogin, BAMRptParam pram)
		{
			List<DistributionSummary> resultData = new List<DistributionSummary>();

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
				FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x => x.StartDate <= pram.sDate.Date && x.EndDate >= pram.sDate.Date);
				pram.sFYDate = fyPeriod != null ? fyPeriod.StartDate : pram.sDate;
				pram.eFYDate = pram.sDate;
			}
			else
			{
				pram.sFYDate = pram.sDate;
				pram.eFYDate = pram.eDate;
			}

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(IUser, userLogin, selectedSites);
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());
			#region Old
			///*
			var siteTrafficCount = await DataService.GetTrafficCount(pacIds, pram.sFYDate, pram.eFYDate, userLogin.ID);

			var kdvr_pacid = sites.Select(x => new { x.KDVR, x.PACID }).Distinct();
			var trafficBySite = siteTrafficCount.Join(kdvr_pacid, tr => tr.PACID, dvr => dvr.PACID, (tr, dvr) => new DistributionModel()
				{
					Count = tr.Count ?? 0,
					Date = tr.DVRDate ?? DateTime.MinValue,
					DWell = tr.DWellTime ?? 0,
					PACID = tr.PACID ?? 0,
					ID = tr.RegionID ?? 0,
					Name = tr.RegionName,
					ParentID = tr.QueueID ?? 0,
					ParentName = tr.QueueName,
					KDVR = dvr.KDVR ?? 0,
					ChannelNo = tr.ChannelNo ?? -1
				}).Distinct(distcomparer);
			/*
			.Select(s => new DistributionModel()
			{
				Count = s.Count ?? 0,
				Date = s.DVRDate ?? DateTime.MinValue,
				DWell = s.DWellTime ?? 0,
				PACID = s.PACID ?? 0,
				ID = s.RegionID ?? 0,
				Name = s.RegionName,
				ParentID = s.QueueID ?? 0,
				ParentName = s.QueueName
			});*/

			var trafficByQueueNC = trafficBySite.Where(x => x.ParentID > 0);
			//var countRegion = trafficByQueueNC.GroupBy(x => x.ParentID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.ID).Distinct().Count() });
			List<TrafficCountRegionInQueue_Result> regionInQueue = await DataService.GetTrafficCountRegionInQueue(pacIds, userLogin.ID);
			var countRegion = regionInQueue.GroupBy(x => x.QueueID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.RegionIndex).Distinct().Count() });
			var trafficByQueue = trafficByQueueNC.Join(countRegion, tr => tr.ParentID, cn => cn.ParentID, (tr, cn) => new DistributionModel()
			{
				Count = tr.Count,
				Date = tr.Date,
				DWell = tr.DWell,
				PACID = tr.PACID,
				ID = tr.ID,
				Name = tr.Name,
				ParentID = tr.ParentID,
				ParentName = tr.ParentName,
				CountRegion = Math.Max(cn.NumReg, 1),
				TimeIndex = tr.TimeIndex,
				KDVR = tr.KDVR,
				ChannelNo = tr.ChannelNo
			}).Distinct(distcomparer);

			var trafficByRegion = trafficBySite.Where(x => x.ParentID == 0);
			//* /
			#endregion
			//DistributionDBResult trafficData = await GetTrafficCountData(pacIds, pram.sFYDate, pram.eFYDate, userLogin.ID, (int)Utils.BAMReportType.Daily, null);
			//var trafficByQueue = trafficData.TrafficByQueue;
			//var trafficByRegion = trafficData.TrafficByRegion;
			DistributionTotalSum totalSum = null;
			#region Region data
			IEnumerable<DistributionSummary> trafficSumBySites = null;
			IEnumerable<DistributionModel> trafficByRegionDate = null;
			List<DateTime> lsDateHasData = siteTrafficCount.Select(x => x.DVRDate.HasValue ? x.DVRDate.Value : DateTime.MinValue).Distinct().ToList();
			if (trafficByRegion != null && trafficByRegion.Any())
			{
				trafficByRegionDate = trafficByRegion.GroupBy(x => new { x.Date, x.ID }).Select(gr => new DistributionModel()
				{
					Date = gr.Key.Date,
					PACID = gr.Any() ? gr.FirstOrDefault().PACID : 0,
					ID = gr.Key.ID,
					DWell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
					Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
					Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
					ParentID = gr.Any() ? gr.FirstOrDefault().ParentID : 0,
					ParentName = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
					KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
					ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0
				});

				var RegionHasData = trafficByRegionDate.Select(x => new { x.ID, x.Name }).Distinct().ToList();
				var RegionWithDate = (from reg in RegionHasData
									  from dd in lsDateHasData
									  select new { ID = reg.ID, Name = reg.Name, Date = dd }).ToList();

				IEnumerable<DistributionModel> trafficDailyAll = (from si in RegionWithDate
																  join cv in trafficByRegionDate on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																  from it in dat.DefaultIfEmpty()
																  select new DistributionModel()
																  {
																	  Date = si.Date,
																	  Count = (it != null) ? it.Count : 0,
																	  DWell = (it != null) ? it.DWell : 0,
																	  PACID = (it != null) ? it.PACID : 0,
																	  ID = si.ID,
																	  Name = si.Name,
																	  TimeIndex = 0,
																	  Title = si.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
																	  ParentID = (it != null) ? it.ParentID : 0,
																	  ParentName = (it != null) ? it.ParentName : string.Empty,
																	  KDVR = (it != null) ? it.KDVR : 0,
																	  ChannelNo = (it != null) ? it.ChannelNo : 0
																  }).ToList();

				trafficSumBySites = trafficByRegionDate.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
						ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
						DataDetail = trafficDailyAll.Where(w => w.ID == s.FirstOrDefault().ID).OrderBy(x => x.Date)
					});

				totalSum = trafficSumBySites.GroupBy(x => x.TimeIndex)
					.Select(s => new DistributionTotalSum()
					{
						TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
						TotalDwell = s.Any() ? s.Sum(i => i.DWell) : 0
					}).FirstOrDefault();
			}
			#endregion

			#region Queue data
			IEnumerable<DistributionSummary> trafficSumByQueue = null;
			IEnumerable<DistributionModel> queueDataDaily = null;
			if (trafficByQueue != null && trafficByQueue.Any())
			{
				var QueueHasData = trafficByQueue.Select(x => new { ID = x.ParentID, Name = x.ParentName }).Distinct().ToList();
				var QueueWithDate = (from reg in QueueHasData
									 from dd in lsDateHasData
									 select new { ID = reg.ID, Name = reg.Name, Date = dd }).ToList();

				queueDataDaily = trafficByQueue.GroupBy(x => new { x.ParentID, x.Date }).Select(gr => new DistributionModel()
					{
						ID = gr.Key.ParentID,
						Name = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
						Date = gr.Key.Date,
						Count = !gr.Any() ? 0 : gr.Sum(x => x.Count), ///gr.FirstOrDefault().CountRegion,//gr.Sum(x => x.Count),
						DWell = !gr.Any() ? 0 : gr.Sum(x => x.DWell)/gr.FirstOrDefault().CountRegion, //Convert.ToInt32(gr.Average(x => x.DWell))
						CountRegion = gr.Any() ? gr.FirstOrDefault().CountRegion : 0
					});

				IEnumerable<DistributionModel> queueDailyAll = (from si in QueueWithDate
																join cv in queueDataDaily on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																from it in dat.DefaultIfEmpty()
																select new DistributionModel()
																{
																	Date = si.Date,
																	Count = (it != null) ? it.Count : 0,
																	DWell = (it != null) ? it.DWell : 0,
																	ID = si.ID,
																	Name = si.Name,
																	TimeIndex = 0,
																	Title = si.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT)
																}).ToList();

				trafficSumByQueue = queueDataDaily.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						DataDetail = queueDailyAll.Where(w => w.ID == s.Key).OrderBy(x => x.Date)
					}).ToList();

				foreach (DistributionSummary data in trafficSumByQueue)
				{
					data.Regions = CreateRegionData(trafficByQueue.Where(w => w.ParentID == data.ID), lsDateHasData);
				}
			}
			#endregion
			List<DistributionSummary> lsContributionDataAll = new List<DistributionSummary>();
			if (trafficSumByQueue != null && trafficSumByQueue.Any())
			{
				lsContributionDataAll.AddRange(trafficSumByQueue);
			}
			if (trafficSumBySites != null && trafficSumBySites.Any())
			{
				lsContributionDataAll.AddRange(trafficSumBySites);
			}

			#region Chart Data
			DateTime minDate = trafficBySite != null && trafficBySite.Any() ? trafficBySite.Min(x => x.Date) : DateTime.MinValue;
			DateTime maxDate = trafficBySite != null && trafficBySite.Any() ? trafficBySite.Max(x => x.Date) : DateTime.MinValue;
			IEnumerable<DateTime> fullDates = null;
			if (!(minDate == DateTime.MinValue && maxDate == DateTime.MinValue))
			{
				int iCount = Convert.ToInt32((maxDate - minDate).TotalDays);
				fullDates = ArrayUtilities.SequenceDate(minDate, iCount);
			}
			List<DistChartData> datCharts = new List<DistChartData>();
			if (fullDates != null && fullDates.Any())
			{
				DateTime fyStartDate = (fyInfo != null && fyInfo.FYDateStart.HasValue) ? fyInfo.FYDateStart.Value : new DateTime(DateTime.Now.Year, 1, 1);
				List<Func_BAM_TrafficCountReportMonthly_Result> trafficCounYTD = await DataService.GetTrafficCount(pacIds, fyStartDate, pram.eFYDate, userLogin.ID);

				#region Region without queue
				IEnumerable<DistChartData> regionChartData = null;
				if (trafficByRegionDate != null && trafficByRegionDate.Any())
				{
					var RegionHasData = trafficByRegionDate.Select(x => new { x.ID, x.Name }).Distinct().ToList();
					var RegionFullDate = (from reg in RegionHasData
										  from dd in fullDates
										  select new { ID = reg.ID, Name = reg.Name, Date = dd }).ToList();
					IEnumerable<DistributionModel> chartDailyAll = (from si in RegionFullDate
																	join cv in trafficByRegionDate on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																	from it in dat.DefaultIfEmpty()
																	select new DistributionModel()
																	{
																		Date = si.Date,
																		Count = (it != null) ? it.Count : 0,
																		DWell = (it != null) ? it.DWell : 0,
																		PACID = (it != null) ? it.PACID : 0,
																		ID = si.ID,
																		Name = si.Name,
																		TimeIndex = 0,
																		Title = si.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
																		KDVR = (it != null) ? it.KDVR : 0,
																		ChannelNo = (it != null) ? it.ChannelNo : 0
																	}).ToList();

					regionChartData = trafficByRegionDate.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
							ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
							//DataYTD = GetDistYTDData(trafficCounYTD, gr.Key, gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue),
							Details = chartDailyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									Date = x.Date,
									KDVR = x.KDVR,
									ChannelNo = x.ChannelNo,
									DataYTD = GetDistYTDData(trafficCounYTD, x.ID, x.Date)
								}).OrderBy(x => x.Date)
						});
				}
				#endregion
				#region Queue with region
				IEnumerable<DistChartData> queueChartData = null;
				if (queueDataDaily != null && queueDataDaily.Any())
				{
					var RegionHasData = queueDataDaily.Select(x => new { x.ID, x.Name, x.CountRegion }).Distinct().ToList();
					var RegionFullDate = (from reg in RegionHasData
										  from dd in fullDates
										  select new { ID = reg.ID, Name = reg.Name, Date = dd, CountRegion = reg.CountRegion }).ToList();
					IEnumerable<DistributionModel> chartDailyAll = (from si in RegionFullDate
																	join cv in queueDataDaily on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
																	from it in dat.DefaultIfEmpty()
																	select new DistributionModel()
																	{
																		Date = si.Date,
																		Count = (it != null) ? it.Count : 0,
																		DWell = (it != null) ? it.DWell : 0,
																		PACID = (it != null) ? it.PACID : 0,
																		ID = si.ID,
																		Name = si.Name,
																		TimeIndex = 0,
																		CountRegion = si.CountRegion,
																		Title = si.Date.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
																		KDVR = (it != null) ? it.KDVR : 0,
																		ChannelNo = (it != null) ? it.ChannelNo : 0
																	}).ToList();

					//List<DistChartData> testData = new List<DistChartData>();
					//testData.Add(new DistChartData() { Count = 23, Name = "Region 1" });
					//testData.Add(new DistChartData() { Count = 33, Name = "Region 222" });
					//testData.Add(new DistChartData() { Count = 12, Name = "Region 333" });

					queueChartData = queueDataDaily.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//gr.Key.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
							KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
							ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
							Details = chartDailyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									Date = x.Date,
									ItemCount = x.CountRegion,
									//DataYTD = new DriveThroughBase() { Count = x.CountRegion, Dwell = 0 },
									Details = CreateChartDetail(trafficByQueue, x.ID, x.Date, -1, trafficCounYTD)
								}).OrderBy(x => x.Date)
						});
				}
				#endregion
				if (regionChartData != null && regionChartData.Any())
				{
					datCharts.AddRange(regionChartData);
				}
				if (queueChartData != null && queueChartData.Any())
				{
					datCharts.AddRange(queueChartData);
				}
			}
			#endregion

			DistributionDataAll retData = new DistributionDataAll();
			retData.SummaryData = lsContributionDataAll;// trafficSumByQueue;
			retData.TotalSum = totalSum;
			retData.ChartData = new DistributionChartAll();
			retData.ChartData.Regions = datCharts;

			return retData;
		}

		private async Task<DistributionDataAll> GetDistributionRptWeekly(UserContext userLogin, BAMRptParam pram)
		{
			List<DistributionSummary> resultData = new List<DistributionSummary>();

			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, pram.eDate, fyInfo.FYDateStart.Value);
			FiscalPeriod fyPeriod = fyPeriods.FirstOrDefault(x => x.StartDate <= pram.sDate.Date && x.EndDate >= pram.sDate.Date);

			DateTime startDate = fyPeriod != null ? fyPeriod.StartDate : pram.sDate;
			DateTime endDate = (fyPeriod != null && fyPeriod.EndDate <= pram.eDate) ? fyPeriod.EndDate : pram.eDate;

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(IUser, userLogin, selectedSites);
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());

			var siteTrafficCount = await DataService.GetTrafficCount(pacIds, startDate, endDate, userLogin.ID);

			var siteTrafficWithWeek = (from week in fyPeriod.Weeks
								   from traffic in siteTrafficCount
								   where traffic.DVRDate >= week.StartDate && traffic.DVRDate <= week.EndDate
								   select new
								   {
									   ID = traffic.RegionID ?? 0,
									   Name = traffic.RegionName,
									   Date = traffic.DVRDate ?? DateTime.MinValue,
									   Count = traffic.Count ?? 0,
									   DWell = traffic.DWellTime ?? 0,
									   PACID = traffic.PACID ?? 0,
									   ParentID = traffic.QueueID ?? 0,
									   ParentName = traffic.QueueName,
									   Week = week,
									   ChannelNo = traffic.ChannelNo ?? 0
								   }).Distinct().ToList();

			var siteTrafficWeekPre = siteTrafficWithWeek.GroupBy(x => new { x.ID, x.Week.WeekIndex })
				.Select(gr => new {
					ID = gr.Key.ID,
					Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
					Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
					Count = gr.Any() ? gr.Sum(x=>x.Count) : 0,
					DWell = gr.Any() ? Convert.ToInt32(gr.Average(x=>x.DWell)) : 0,
					PACID = gr.Any() ? gr.FirstOrDefault().PACID : 0,
					ParentID = gr.Any() ? gr.FirstOrDefault().ParentID : 0,
					ParentName = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
					TimeIndex = gr.Key.WeekIndex,
					ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0
				});

			var kdvr_pacid = sites.Select(x => new { x.KDVR, x.PACID }).Distinct();
			var siteTrafficWeek = siteTrafficWeekPre.Join(kdvr_pacid, tr => tr.PACID, dvr => dvr.PACID, (tr, dvr) => new
				{
					ID = tr.ID,
					Name = tr.Name,
					Date = tr.Date,
					Count = tr.Count,
					DWell = tr.DWell,
					PACID = tr.PACID,
					ParentID = tr.ParentID,
					ParentName = tr.ParentName,
					TimeIndex = tr.TimeIndex,
					KDVR = dvr.KDVR ?? 0,
					ChannelNo = tr.ChannelNo
				});

			var trafficByQueueNC = siteTrafficWeek.Where(x => x.ParentID > 0).Select(s => new DistributionModel()
				{
					Count = s.Count,
					Date = s.Date,
					DWell = s.DWell,
					PACID = s.PACID,
					ID = s.ID,
					Name = s.Name,
					ParentID = s.ParentID,
					ParentName = s.ParentName,
					TimeIndex = s.TimeIndex,//Week.WeekIndex
					KDVR = s.KDVR,
					ChannelNo = s.ChannelNo
				});
			//var countRegion = trafficByQueueNC.GroupBy(x => x.ParentID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.ID).Distinct().Count() });
			List<TrafficCountRegionInQueue_Result> regionInQueue = await DataService.GetTrafficCountRegionInQueue(pacIds, userLogin.ID);
			var countRegion = regionInQueue.GroupBy(x => x.QueueID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.RegionIndex).Distinct().Count() });
			var trafficByQueue = trafficByQueueNC.Join(countRegion, tr => tr.ParentID, cn => cn.ParentID, (tr, cn) => new DistributionModel()
			{
				Count = tr.Count,
				Date = tr.Date,
				DWell = tr.DWell,
				PACID = tr.PACID,
				ID = tr.ID,
				Name = tr.Name,
				ParentID = tr.ParentID,
				ParentName = tr.ParentName,
				CountRegion = Math.Max(cn.NumReg, 1),
				TimeIndex = tr.TimeIndex,
				KDVR = tr.KDVR,
				ChannelNo = tr.ChannelNo
			}).Distinct();

			var trafficByRegion = siteTrafficWeek.Where(x => x.ParentID == 0);
			List<FiscalWeek> weekSearchList = siteTrafficWithWeek.Select(x => x.Week).Distinct().ToList();

			DistributionTotalSum totalSum = null;
			#region Region data
			IEnumerable<DistributionSummary> trafficSumBySites = null;
			IEnumerable<DistributionModel> trafficByRegionWeek = null;
			if (trafficByRegion != null && trafficByRegion.Any())
			{
				//trafficByRegionWeek = trafficByRegion.GroupBy(x => new { x.ID, x.Week.WeekIndex })
				//.Select(s => new DistributionModel()
				//{
				//	Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
				//	Count = s.Any() ? s.Sum(x => x.Count) : 0,
				//	DWell = s.Any() ? Convert.ToInt32(s.Average(x=>x.DWell)) : 0,
				//	PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
				//	ID = s.Any() ? s.FirstOrDefault().ID : 0,
				//	Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
				//	TimeIndex = s.Key.WeekIndex,
				//	ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
				//	ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
				//	Title = s.Any() ? s.FirstOrDefault().Week.ToString() : string.Empty
				//});
				trafficByRegionWeek = trafficByRegion.Select(x => new DistributionModel()
					{
						Date = x.Date,
						Count = x.Count,
						DWell = x.DWell,
						PACID = x.PACID,
						ID = x.ID,
						Name = x.Name,
						TimeIndex = x.TimeIndex,
						ParentID = x.ParentID,
						ParentName = x.ParentName,
						Title = string.Empty,
						KDVR = x.KDVR,
						ChannelNo = x.ChannelNo
					});

				var regionHasData = trafficByRegionWeek.Select(x => new { x.ID, x.Name }).Distinct().ToList();
				var RegionWithWeek = (from reg in regionHasData
									  from p in weekSearchList//fyPeriod.Weeks
									  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.WeekIndex, week = p }).ToList();

				IEnumerable<DistributionModel> trafficWeeklyAll = (from si in RegionWithWeek
																   join cv in trafficByRegionWeek on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																   from it in dat.DefaultIfEmpty()
																   select new DistributionModel()
																   {
																	   Count = (it != null) ? it.Count : 0,
																	   Date = (it != null) ? it.Date : DateTime.MinValue,
																	   DWell = (it != null) ? it.DWell : 0,
																	   PACID = (it != null) ? it.PACID : 0,
																	   ID = si.ID,
																	   Name = si.Name,
																	   TimeIndex = si.TimeIndex,
																	   Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.week.StartDate, si.week.EndDate, si.week.WeekIndex),
																	   ParentID = (it != null) ? it.ParentID : 0,
																	   ParentName = (it != null) ? it.ParentName : string.Empty,
																	   KDVR = (it != null) ? it.KDVR : 0,
																	   ChannelNo = (it != null) ? it.ChannelNo : 0
																   }).ToList();

				trafficSumBySites = trafficByRegionWeek.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
						ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
						DataDetail = trafficWeeklyAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					});

				totalSum = trafficSumBySites.GroupBy(x => x.TimeIndex)
					.Select(s => new DistributionTotalSum()
					{
						TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
						TotalDwell = s.Any() ? s.Sum(i => i.DWell) : 0
					}).FirstOrDefault();
			}
			#endregion

			#region Queue data
			IEnumerable<DistributionSummary> trafficSumByQueue = null;
			IEnumerable<DistributionModel> trafficByQueueWeek = null;
			if (trafficByQueue != null && trafficByQueue.Any())
			{
				var queueHasData = trafficByQueue.Select(x => new { ID = x.ParentID, Name = x.ParentName }).Distinct().ToList();
				var QueueWithWeek = (from reg in queueHasData
									 from p in weekSearchList//fyPeriod.Weeks
									 select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.WeekIndex, week = p }).ToList();
				trafficByQueueWeek = trafficByQueue.GroupBy(x => new { x.ParentID, x.TimeIndex })
					.Select(gr => new DistributionModel()
					{
						Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
						Count = !gr.Any() ? 0 : gr.Sum(x => x.Count),// / gr.FirstOrDefault().CountRegion,//gr.Sum(x => x.Count),
						DWell = !gr.Any() ? 0 : gr.Sum(x => x.DWell) / gr.FirstOrDefault().CountRegion,//Convert.ToInt32(gr.Average(x => x.DWell)),
						ID = gr.Key.ParentID,
						Name = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
						TimeIndex = gr.Key.TimeIndex,
						CountRegion = gr.Any() ? gr.FirstOrDefault().CountRegion : 0,
						Title = gr.Key.TimeIndex.ToString(),
						KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
						ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
					});
				IEnumerable<DistributionModel> queueWeeklyAll = (from si in QueueWithWeek
																 join cv in trafficByQueueWeek on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																 from it in dat.DefaultIfEmpty()
																 select new DistributionModel()
																 {
																	 Count = (it != null) ? it.Count : 0,
																	 Date = (it != null) ? it.Date : DateTime.MinValue,
																	 DWell = (it != null) ? it.DWell : 0,
																	 PACID = (it != null) ? it.PACID : 0,
																	 ID = si.ID,
																	 Name = si.Name,
																	 TimeIndex = si.TimeIndex,
																	 Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.week.StartDate, si.week.EndDate, si.week.WeekIndex),
																	 KDVR = (it != null) ? it.KDVR : 0,
																	 ChannelNo = (it != null) ? it.ChannelNo : 0
																 }).ToList();

				trafficSumByQueue = trafficByQueueWeek.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						DataDetail = queueWeeklyAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					}).ToList();

				foreach (DistributionSummary data in trafficSumByQueue)
				{
					var Regions = trafficByQueue.Where(w => w.ParentID == data.ID).Select(x => new DistributionModel() {
						Count = x.Count,
						Date = x.Date,
						DWell = x.DWell,
						ID = x.ID,
						Name = x.Name,
						ParentID = x.ParentID,
						ParentName = x.ParentName,
						TimeIndex = x.TimeIndex
					});
					data.Regions = CreateRegionData(Regions, weekSearchList);
				}
			}
			#endregion

			List<DistributionSummary> lsContributionDataAll = new List<DistributionSummary>();
			if (trafficSumByQueue != null && trafficSumByQueue.Any())
			{
				lsContributionDataAll.AddRange(trafficSumByQueue);
			}
			if (trafficSumBySites != null && trafficSumBySites.Any())
			{
				lsContributionDataAll.AddRange(trafficSumBySites);
			}

			#region Chart Data
			int minTime = siteTrafficWeek != null && siteTrafficWeek.Any() ? siteTrafficWeek.Min(x => x.TimeIndex) : -1;
			int maxTime = siteTrafficWeek != null && siteTrafficWeek.Any() ? siteTrafficWeek.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullWeeks = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullWeeks = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			List<DistChartData> datCharts = new List<DistChartData>();
			if (fullWeeks != null && fullWeeks.Any())
			{
				DateTime fyStartDate = (fyInfo != null && fyInfo.FYDateStart.HasValue) ? fyInfo.FYDateStart.Value : new DateTime(DateTime.Now.Year, 1, 1);
				List<Func_BAM_TrafficCountReportMonthly_Result> trafficCounYTD = await DataService.GetTrafficCount(pacIds, fyStartDate, endDate, userLogin.ID);

				#region Region without queue
				IEnumerable<DistChartData> regionChartData = null;
				if (trafficByRegionWeek != null && trafficByRegionWeek.Any())
				{
					var regionHasData = trafficByRegionWeek.Select(x => new { x.ID, x.Name }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullWeeks
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p }).ToList();
					IEnumerable<DistributionModel> chartWeeklyAll = (from si in RegionFullWeek
																	 join cv in trafficByRegionWeek on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 Title = string.Format("{0}", si.TimeIndex),
																		 KDVR = (it != null) ? it.KDVR : 0,
																		 ChannelNo = (it != null) ? it.ChannelNo : 0,
																	 }).ToList();

					regionChartData = trafficByRegionWeek.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = gr.Key,
							Details = chartWeeklyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									KDVR = x.KDVR,
									ChannelNo = x.ChannelNo,
									DataYTD = GetDistYTDData(trafficCounYTD, x.ID, GetDateByWeek(fyPeriod.Weeks, x.TimeIndex))//pram.eDate
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion

				#region Queue with region
				IEnumerable<DistChartData> queueChartData = null;
				if (trafficByQueueWeek != null && trafficByQueueWeek.Any())
				{
					var regionHasData = trafficByQueueWeek.Select(x => new { x.ID, x.Name, x.CountRegion }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullWeeks
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p, CountRegion = reg.CountRegion }).ToList();
					IEnumerable<DistributionModel> chartWeeklyAll = (from si in RegionFullWeek
																	 join cv in trafficByQueueWeek on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 CountRegion = si.CountRegion,
																		 Title = string.Format("{0}", si.TimeIndex),
																		 KDVR = (it != null) ? it.KDVR : 0,
																		 ChannelNo = (it != null) ? it.ChannelNo : 0
																	 }).ToList();

					queueChartData = trafficByQueueWeek.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = gr.Key,
							Details = chartWeeklyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									ItemCount = x.CountRegion,
									KDVR = x.KDVR,
									ChannelNo = x.ChannelNo,
									//DataYTD = GetDistYTDData(trafficCounYTD, x.ID, pram.eDate),
									Details = CreateChartDetail(trafficByQueue, x.ID, GetDateByWeek(fyPeriod.Weeks, x.TimeIndex), x.TimeIndex, trafficCounYTD)//pram.eDate
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion
				if (regionChartData != null && regionChartData.Any())
				{
					datCharts.AddRange(regionChartData);
				}
				if (queueChartData != null && queueChartData.Any())
				{
					datCharts.AddRange(queueChartData);
				}
			}
			#endregion

			DistributionDataAll retData = new DistributionDataAll();
			retData.SummaryData = lsContributionDataAll;// trafficSumByQueue;
			retData.TotalSum = totalSum;
			retData.ChartData = new DistributionChartAll();
			retData.ChartData.Regions = datCharts;

			return retData;
		}

		private async Task<DistributionDataAll> GetDistributionRptMonthly(UserContext userLogin, BAMRptParam pram)
		{
			List<DistributionSummary> resultData = new List<DistributionSummary>();

			tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, pram.sDate);
			List<FiscalPeriod> fyPeriods = IFiscalYear.GetFiscalPeriods(fyInfo, pram.eDate, fyInfo.FYDateStart.Value);
			fyPeriods = fyPeriods.Where(x => x.StartDate < pram.eDate).ToList();
			pram.sFYDate = fyInfo.FYDateStart.HasValue ? fyInfo.FYDateStart.Value : pram.sDate;
			pram.eFYDate = fyInfo.FYDateEnd.HasValue && fyInfo.FYDateEnd.Value <= pram.sDate.Date ? fyInfo.FYDateEnd.Value : pram.sDate;

			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(IUser, userLogin, selectedSites);
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());

			var siteTrafficCount = await DataService.GetTrafficCount(pacIds, pram.sFYDate, pram.eFYDate, userLogin.ID);

			var siteTrafficWithPeriod = (from period in fyPeriods
										 from traffic in siteTrafficCount
										 where traffic.DVRDate >= period.StartDate && traffic.DVRDate <= period.EndDate
										 select new
										 {
											ID = traffic.RegionID?? 0,
											Name = traffic.RegionName,
											Date = traffic.DVRDate?? DateTime.MinValue,
											Count = traffic.Count?? 0,
											DWell = traffic.DWellTime?? 0,
											PACID = traffic.PACID?? 0,
											ParentID = traffic.QueueID ?? 0,
											ParentName = traffic.QueueName,
											ChannelNo = traffic.ChannelNo ?? 0,
											Period = period
										 }).Distinct().ToList();

			var siteTrafficPeriodPre = siteTrafficWithPeriod.GroupBy(x => new { x.ID, x.Period.Period })
				.Select(gr => new 
				{
					ID = gr.Key.ID,
					Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
					Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
					Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
					DWell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
					PACID = gr.Any() ? gr.FirstOrDefault().PACID : 0,
					ParentID = gr.Any() ? gr.FirstOrDefault().ParentID : 0,
					ParentName = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
					TimeIndex = gr.Key.Period,
					ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0
				});

			var kdvr_pacid = sites.Select(x => new { x.KDVR, x.PACID }).Distinct();
			var siteTrafficPeriod = siteTrafficPeriodPre.Join(kdvr_pacid, tr => tr.PACID, dvr => dvr.PACID, (tr, dvr) => new 
					{
						ID = tr.ID,
						Name = tr.Name,
						Date = tr.Date,
						Count = tr.Count,
						DWell = tr.DWell,
						PACID = tr.PACID,
						ParentID = tr.ParentID,
						ParentName = tr.ParentName,
						TimeIndex = tr.TimeIndex,
						KDVR = dvr.KDVR ?? 0,
						ChannelNo = tr.ChannelNo
					});

			List<FiscalPeriod> periodSearchList = siteTrafficWithPeriod.Select(x => x.Period).Distinct().ToList();

			var trafficByQueueNC = siteTrafficPeriod.Where(x => x.ParentID > 0).Select(s => new DistributionModel()
				{
					Count = s.Count,
					Date = s.Date,
					DWell = s.DWell,
					PACID = s.PACID,
					ID = s.ID,
					Name = s.Name,
					ParentID = s.ParentID,
					ParentName = s.ParentName,
					TimeIndex = s.TimeIndex,
					KDVR = s.KDVR,
					ChannelNo = s.ChannelNo
				});
			//var countRegion = trafficByQueueNC.GroupBy(x => x.ParentID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.ID).Distinct().Count() });
			List<TrafficCountRegionInQueue_Result> regionInQueue = await DataService.GetTrafficCountRegionInQueue(pacIds, userLogin.ID);
			var countRegion = regionInQueue.GroupBy(x => x.QueueID).Select(gr => new { ParentID = gr.Key, NumReg = !gr.Any() ? 0 : gr.Select(x => x.RegionIndex).Distinct().Count() });
			var trafficByQueue = trafficByQueueNC.Join(countRegion, tr => tr.ParentID, cn => cn.ParentID, (tr, cn) => new DistributionModel()
			{
				Count = tr.Count,
				Date = tr.Date,
				DWell = tr.DWell,
				PACID = tr.PACID,
				ID = tr.ID,
				Name = tr.Name,
				ParentID = tr.ParentID,
				ParentName = tr.ParentName,
				CountRegion = Math.Max(cn.NumReg, 1),
				TimeIndex = tr.TimeIndex,
				KDVR = tr.KDVR,
				ChannelNo = tr.ChannelNo
			}).Distinct(distcomparer);

			var trafficByRegion = siteTrafficPeriod.Where(x => x.ParentID == 0);

			DistributionTotalSum totalSum = null;
			#region Region data
			IEnumerable<DistributionSummary> trafficSumBySites = null;
			IEnumerable<DistributionModel> trafficBySite = null;
			if (trafficByRegion != null && trafficByRegion.Any())
			{
				//trafficBySite = trafficByRegion.GroupBy(x => new { x.ID, x.Period.Period })
				//.Select(s => new DistributionModel()
				//{
				//	Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
				//	Count = s.Any() ? s.Sum(x => x.Count) : 0,
				//	DWell = s.Any() ? Convert.ToInt32(s.Average(x=>x.DWell)) : 0,
				//	PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
				//	ID = s.Any() ? s.Key.ID : 0,
				//	Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
				//	ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
				//	ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
				//	TimeIndex = s.Key.Period,
				//	Title = s.Any() ? s.FirstOrDefault().Period.ToString() : string.Empty
				//});
				trafficBySite = trafficByRegion.Select(x => new DistributionModel()
					{
						Date = x.Date,
						Count = x.Count,
						DWell = x.DWell,
						PACID = x.PACID,
						ID = x.ID,
						Name = x.Name,
						TimeIndex = x.TimeIndex,
						ParentID = x.ParentID,
						ParentName = x.ParentName,
						Title = string.Empty,
						KDVR = x.KDVR,
						ChannelNo = x.ChannelNo
					});

				var regionHasData = trafficBySite.Select(x => new { x.ID, x.Name }).Distinct().ToList();
				var RegionWithPeriod = (from reg in regionHasData
										from p in periodSearchList//fyPeriods
										select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.Period, Period = p }).ToList();

				IEnumerable<DistributionModel> trafficPeriodAll = (from si in RegionWithPeriod
																   join cv in trafficBySite on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																   from it in dat.DefaultIfEmpty()
																   select new DistributionModel()
																   {
																	   Count = (it != null) ? it.Count : 0,
																	   Date = (it != null) ? it.Date : DateTime.MinValue,
																	   DWell = (it != null) ? it.DWell : 0,
																	   PACID = (it != null) ? it.PACID : 0,
																	   ID = si.ID,
																	   Name = si.Name,
																	   TimeIndex = si.TimeIndex,
																	   ParentID = (it != null) ? it.ParentID : 0,
																	   ParentName = (it != null) ? it.ParentName : string.Empty,
																	   Title = string.Format("{0}", si.TimeIndex),
																	   KDVR = (it != null) ? it.KDVR : 0,
																	   ChannelNo = (it != null) ? it.ChannelNo : 0
																   }).ToList();

				trafficSumBySites = trafficBySite.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
						ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
						DataDetail = trafficPeriodAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					});

				totalSum = trafficSumBySites.GroupBy(x => x.TimeIndex)
					.Select(s => new DistributionTotalSum()
					{
						TotalCount = s.Any() ? s.Sum(i => i.Count) : 0,
						TotalDwell = s.Any() ? s.Sum(i => i.DWell) : 0
					}).FirstOrDefault();
			}
			#endregion

			#region Queue data
			IEnumerable<DistributionSummary> trafficSumByQueue = null;
			IEnumerable<DistributionModel> queueDataPeriodly = null;
			if (trafficByQueue != null && trafficByQueue.Any())
			{
				var queueHasData = trafficByQueue.Select(x => new { ID = x.ParentID, Name = x.ParentName }).Distinct().ToList();
				var QueueWithPeriod = (from reg in queueHasData
									   from p in periodSearchList//fyPeriods
									   select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.Period, Period = p }).ToList();
				queueDataPeriodly = trafficByQueue.GroupBy(x => new { x.ParentID, x.TimeIndex }).Select(gr => new DistributionModel()
					{
						Date = gr.Any() ? gr.FirstOrDefault().Date : DateTime.MinValue,
						Count = !gr.Any() ? 0 : gr.Sum(x => x.Count),// / gr.FirstOrDefault().CountRegion,//gr.Sum(x => x.Count),
						DWell = !gr.Any() ? 0 : gr.Sum(x => x.DWell) / gr.FirstOrDefault().CountRegion,//Convert.ToInt32(gr.Average(x => x.DWell)),
						ID = gr.Key.ParentID,
						Name = gr.Any() ? gr.FirstOrDefault().ParentName : string.Empty,
						TimeIndex = gr.Key.TimeIndex,
						CountRegion = gr.Any() ? gr.FirstOrDefault().CountRegion : 0,
						Title = gr.Key.TimeIndex.ToString(),
						KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
						ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0
					});
				IEnumerable<DistributionModel> queuePeriodAll = (from si in QueueWithPeriod
																 join cv in queueDataPeriodly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																 from it in dat.DefaultIfEmpty()
																 select new DistributionModel()
																 {
																	 Count = (it != null) ? it.Count : 0,
																	 Date = (it != null) ? it.Date : DateTime.MinValue,
																	 DWell = (it != null) ? it.DWell : 0,
																	 PACID = (it != null) ? it.PACID : 0,
																	 ID = si.ID,
																	 Name = si.Name,
																	 TimeIndex = si.TimeIndex,
																	 Title = string.Format("{0}", si.TimeIndex),
																	 KDVR = (it != null) ? it.KDVR : 0,
																	 ChannelNo = (it != null) ? it.ChannelNo : 0
																 }).ToList();
				trafficSumByQueue = queueDataPeriodly.GroupBy(x => x.ID)
					.Select(s => new DistributionSummary()
					{
						Count = s.Any() ? s.Sum(i => i.Count) : 0,
						DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
						ID = s.Key,
						Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
						DataDetail = queuePeriodAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
					}).ToList();

				foreach (DistributionSummary data in trafficSumByQueue)
				{
					var Regions = trafficByQueue.Where(w => w.ParentID == data.ID).Select(x => new DistributionModel()
					{
						Count = x.Count,
						Date = x.Date,
						DWell = x.DWell,
						ID = x.ID,
						Name = x.Name,
						ParentID = x.ParentID,
						ParentName = x.ParentName,
						TimeIndex = x.TimeIndex
					});
					data.Regions = CreateRegionData(Regions, periodSearchList);
				}
			}
			#endregion

			List<DistributionSummary> lsContributionDataAll = new List<DistributionSummary>();
			if (trafficSumByQueue != null && trafficSumByQueue.Any())
			{
				lsContributionDataAll.AddRange(trafficSumByQueue);
			}
			if (trafficSumBySites != null && trafficSumBySites.Any())
			{
				lsContributionDataAll.AddRange(trafficSumBySites);
			}

			#region Chart Data
			int minTime = trafficBySite != null && trafficBySite.Any() ? trafficBySite.Min(x => x.TimeIndex) : -1;
			int maxTime = trafficBySite != null && trafficBySite.Any() ? trafficBySite.Max(x => x.TimeIndex) : -1;
			IEnumerable<int> fullPeriods = null;
			if (minTime >= 0 && maxTime >= minTime)
			{
				fullPeriods = ArrayUtilities.SequenceNumber(minTime, maxTime - minTime + 1);
			}
			List<DistChartData> datCharts = new List<DistChartData>();
			if (fullPeriods != null && fullPeriods.Any())
			{
				DateTime fyStartDate = (fyInfo != null && fyInfo.FYDateStart.HasValue) ? fyInfo.FYDateStart.Value : new DateTime(DateTime.Now.Year, 1, 1);
				List<Func_BAM_TrafficCountReportMonthly_Result> trafficCounYTD = await DataService.GetTrafficCount(pacIds, fyStartDate, pram.eFYDate, userLogin.ID);

				#region Region without queue
				IEnumerable<DistChartData> regionChartData = null;
				if (trafficBySite != null && trafficBySite.Any())
				{
					var regionHasData = trafficBySite.Select(x => new { x.ID, x.Name }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullPeriods
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p }).ToList();
					IEnumerable<DistributionModel> chartWeeklyAll = (from si in RegionFullWeek
																	 join cv in trafficBySite on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 Title = string.Format("{0}", si.TimeIndex),
																		 KDVR = (it != null) ? it.KDVR : 0,
																		 ChannelNo = (it != null) ? it.ChannelNo : 0
																	 }).ToList();

					regionChartData = trafficBySite.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = gr.Key,
							Details = chartWeeklyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									KDVR = x.KDVR,
									ChannelNo = x.ChannelNo,
									DataYTD = GetDistYTDData(trafficCounYTD, x.ID, GetDateByPeriod(fyPeriods, x.TimeIndex))//pram.eDate
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion

				#region Queue with region
				IEnumerable<DistChartData> queueChartData = null;
				if (queueDataPeriodly != null && queueDataPeriodly.Any())
				{
					var regionHasData = queueDataPeriodly.Select(x => new { x.ID, x.Name, x.CountRegion }).Distinct().ToList();
					var RegionFullWeek = (from reg in regionHasData
										  from p in fullPeriods
										  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p, CountRegion = reg.CountRegion }).ToList();
					IEnumerable<DistributionModel> chartWeeklyAll = (from si in RegionFullWeek
																	 join cv in queueDataPeriodly on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
																	 from it in dat.DefaultIfEmpty()
																	 select new DistributionModel()
																	 {
																		 Count = (it != null) ? it.Count : 0,
																		 Date = (it != null) ? it.Date : DateTime.MinValue,
																		 DWell = (it != null) ? it.DWell : 0,
																		 PACID = (it != null) ? it.PACID : 0,
																		 ID = si.ID,
																		 Name = si.Name,
																		 TimeIndex = si.TimeIndex,
																		 CountRegion = si.CountRegion,
																		 Title = string.Format("{0}", si.TimeIndex),
																		 KDVR = (it != null) ? it.KDVR : 0,
																		 ChannelNo = (it != null) ? it.ChannelNo : 0
																	 }).ToList();

					queueChartData = queueDataPeriodly.GroupBy(x => x.ID)
						.Select(gr => new DistChartData()
						{
							ID = gr.Key,
							Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format("Week {0}", gr.Key),
							Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
							Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
							TimeIndex = gr.Key,
							Details = chartWeeklyAll.Where(x => x.ID == gr.Key)
								.Select(x => new DistChartData()
								{
									ID = x.ID,
									Name = x.Name,
									Count = x.Count,
									Dwell = x.DWell,
									TimeIndex = x.TimeIndex,
									ItemCount = x.CountRegion,
									KDVR = x.KDVR,
									ChannelNo = x.ChannelNo,
									//DataYTD = GetDistYTDData(trafficCounYTD, x.ID, pram.eDate),
									Details = CreateChartDetail(trafficByQueue, x.ID, GetDateByPeriod(fyPeriods, x.TimeIndex), x.TimeIndex, trafficCounYTD)//pram.eDate
								}).OrderBy(x => x.TimeIndex)
						});
				}
				#endregion
				if (regionChartData != null && regionChartData.Any())
				{
					datCharts.AddRange(regionChartData);
				}
				if (queueChartData != null && queueChartData.Any())
				{
					datCharts.AddRange(queueChartData);
				}
				//var regionHasData = trafficBySite.Select(x => new { x.ID, x.Name }).Distinct().ToList();
				//var RegionFullPeriod = (from reg in regionHasData
				//						from p in fullPeriods
				//						select new { ID = reg.ID, Name = reg.Name, TimeIndex = p }).ToList();
				//IEnumerable<DistributionModel> chartPeriodAll = (from si in RegionFullPeriod
				//												   join cv in trafficBySite on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
				//												   from it in dat.DefaultIfEmpty()
				//												   select new DistributionModel()
				//												   {
				//													   Count = (it != null) ? it.Count : 0,
				//													   Date = (it != null) ? it.Date : DateTime.MinValue,
				//													   DWell = (it != null) ? it.DWell : 0,
				//													   PACID = (it != null) ? it.PACID : 0,
				//													   ID = si.ID,
				//													   Name = si.Name,
				//													   TimeIndex = si.TimeIndex,
				//													   Title = string.Format("{0}", si.TimeIndex)
				//												   }).ToList();

				//datCharts = trafficBySite.GroupBy(x => x.ID)
				//	.Select(gr => new DistChartData()
				//	{
				//		Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,//String.Format(Consts.CHART_LEGEND_MONTH_FORMAT, gr.Key),
				//		ID = gr.Key,
				//		Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
				//		Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
				//		TimeIndex = gr.Any() ? gr.FirstOrDefault().TimeIndex : 0,
				//		Details = chartPeriodAll.Where(x => x.TimeIndex == gr.Key)
				//			.Select(x => new DistChartData()
				//			{
				//				ID = x.ID,
				//				Name = x.Name,
				//				Count = x.Count,
				//				Dwell = x.DWell,
				//				TimeIndex = x.TimeIndex
				//			}).OrderBy(x => x.TimeIndex)
				//	});
			}
			#endregion

			DistributionDataAll retData = new DistributionDataAll();
			retData.SummaryData = lsContributionDataAll;// trafficSumByQueue;
			retData.TotalSum = totalSum;
			retData.ChartData = new DistributionChartAll();
			retData.ChartData.Regions = datCharts;

			return retData;
		}

		private IEnumerable<DistributionSummary> CreateRegionData(IEnumerable<DistributionModel> regInQueue, IEnumerable<int> hourList)
		{
			var trafficByRegionDate = regInQueue.GroupBy(x => new { x.ID, x.TimeIndex })
				.Select(s => new DistributionModel()
				{
					Count = s.Any() ? s.Sum(x=>x.Count) : 0,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					DWell = s.Any() ? Convert.ToInt32(s.Average(x=>x.DWell)) : 0,
					PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					TimeIndex = s.Key.TimeIndex,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					KDVR = s.Any() ? s.FirstOrDefault().KDVR : 0,
					ChannelNo = s.Any() ? s.FirstOrDefault().ChannelNo : 0,
					Title = s.Key.ToString()
				});
			var RegionHasData = trafficByRegionDate.Select(x => new { x.ID, x.Name }).Distinct().ToList();
			var RegionWithDate = (from reg in RegionHasData
								  from dd in hourList
								  select new { ID = reg.ID, Name = reg.Name, TimeIndex = dd }).ToList();

			IEnumerable<DistributionModel> trafficDailyAll = (from si in RegionWithDate
															  join cv in trafficByRegionDate on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															  from it in dat.DefaultIfEmpty()
															  select new DistributionModel()
															  {
																  Count = (it != null) ? it.Count : 0,
																  Date = (it != null) ? it.Date : DateTime.MinValue,
																  DWell = (it != null) ? it.DWell : 0,
																  PACID = (it != null) ? it.PACID : 0,
																  ID = si.ID,
																  Name = si.Name,
																  TimeIndex = si.TimeIndex,
																  Title = string.Empty,
																  ParentID = (it != null) ? it.ParentID : 0,
																  ParentName = (it != null) ? it.ParentName : string.Empty,
																  KDVR = (it != null) ? it.KDVR : 0,
																  ChannelNo = (it != null) ? it.ChannelNo : 0
															  }).ToList();

			IEnumerable<DistributionSummary> trafficSumBySites = trafficByRegionDate.GroupBy(x => x.ID)
				.Select(s => new DistributionSummary()
				{
					Count = s.Any() ? s.Sum(i => i.Count) : 0,
					DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					DataDetail = trafficDailyAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
				}).ToList();
			return trafficSumBySites;
		}

		private IEnumerable<DistributionSummary> CreateRegionData(IEnumerable<DistributionModel> regInQueue, IEnumerable<DateTime> dateSearchList)
		{
			DistributionModel firstItem = regInQueue.Any() ? regInQueue.FirstOrDefault() : null;
			if (firstItem == null)
				return new List<DistributionSummary>();
			string sParentName = firstItem.ParentName;
			int parentID = firstItem.ParentID;

			var trafficByRegionDate = regInQueue.GroupBy(x => new { x.ID, x.Date })
				.Select(s => new DistributionModel()
				{
					Count = s.Any() ? s.Sum(x=>x.Count) : 0,
					Date = s.Key.Date,
					DWell = s.Any() ? Convert.ToInt32(s.Average(x => x.DWell)) : 0,
					PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					TimeIndex = 0,
					ParentID = parentID,//s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = sParentName,//s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Title = s.Key.Date.ToString(),
					KDVR = s.Any() ? s.FirstOrDefault().KDVR : 0,
					ChannelNo = s.Any() ? s.FirstOrDefault().ChannelNo : 0
				});
			var RegionHasData = trafficByRegionDate.Select(x => new { x.ID, x.Name }).Distinct().ToList();
			var RegionWithDate = (from reg in RegionHasData
								  from dd in dateSearchList
								  select new { ID = reg.ID, Name = reg.Name, Date = dd }).ToList();

			IEnumerable<DistributionModel> trafficDailyAll = (from si in RegionWithDate
															   join cv in trafficByRegionDate on new { si.ID, si.Date } equals new { cv.ID, cv.Date } into dat
															   from it in dat.DefaultIfEmpty()
															   select new DistributionModel()
															   {
																   Count = (it != null) ? it.Count : 0,
																   Date = si.Date,//(it != null) ? it.Date : DateTime.MinValue,
																   DWell = (it != null) ? it.DWell : 0,
																   PACID = (it != null) ? it.PACID : 0,
																   ID = si.ID,
																   Name = si.Name,
																   TimeIndex = 0,
																   Title = string.Empty,
																   ParentID = parentID,//(it != null) ? it.ParentID : 0,
																   ParentName = sParentName,//(it != null) ? it.ParentName : string.Empty
																   KDVR = (it != null) ? it.KDVR : 0,
																   ChannelNo = (it != null) ? it.ChannelNo : 0,
															   }).ToList();

			IEnumerable<DistributionSummary> trafficSumBySites = trafficByRegionDate.GroupBy(x => x.ID)
				.Select(s => new DistributionSummary()
				{
					Count = s.Any() ? s.Sum(i => i.Count) : 0,
					DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentID = parentID,//s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = sParentName,//s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					DataDetail = trafficDailyAll.Where(w => w.ID == s.Key).OrderBy(x => x.Date)
				}).ToList();
			return trafficSumBySites;
		}

		private IEnumerable<DistributionSummary> CreateRegionData(IEnumerable<DistributionModel> regInQueue, IEnumerable<FiscalWeek> weekSearchList)
		{
			var trafficByRegionWeek = regInQueue.GroupBy(x => new { x.ID, x.TimeIndex })
				.Select(s => new DistributionModel()
				{
					Count = s.Any() ? s.Sum(x=>x.Count) : 0,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					DWell = s.Any() ? Convert.ToInt32(s.Average(x => x.DWell)) : 0,
					PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					TimeIndex = s.Key.TimeIndex,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Title = s.Key.TimeIndex.ToString(),
					KDVR = s.Any() ? s.FirstOrDefault().KDVR : 0,
					ChannelNo = s.Any() ? s.FirstOrDefault().ChannelNo : 0
				});
			var regionHasData = trafficByRegionWeek.Select(x => new { x.ID, x.Name }).Distinct().ToList();
			var RegionWithWeek = (from reg in regionHasData
								  from p in weekSearchList
								  select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.WeekIndex, week = p }).ToList();

			IEnumerable<DistributionModel> trafficWeeklyAll = (from si in RegionWithWeek
															   join cv in trafficByRegionWeek on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															   from it in dat.DefaultIfEmpty()
															   select new DistributionModel()
															   {
																   Count = (it != null) ? it.Count : 0,
																   Date = (it != null) ? it.Date : DateTime.MinValue,
																   DWell = (it != null) ? it.DWell : 0,
																   PACID = (it != null) ? it.PACID : 0,
																   ID = si.ID,
																   Name = si.Name,
																   TimeIndex = si.TimeIndex,
																   Title = string.Format("{2} - {0: MM/dd/yyyy} - {1: MM/dd/yyyy}", si.week.StartDate, si.week.EndDate, si.week.WeekIndex),
																   ParentID = (it != null) ? it.ParentID : 0,
																   ParentName = (it != null) ? it.ParentName : string.Empty,
																   KDVR = (it != null) ? it.KDVR : 0,
																   ChannelNo = (it != null) ? it.ChannelNo : 0
															   }).ToList();

			IEnumerable<DistributionSummary> trafficSumBySites = trafficByRegionWeek.GroupBy(x => x.ID)
				.Select(s => new DistributionSummary()
				{
					Count = s.Any() ? s.Sum(i => i.Count) : 0,
					DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					DataDetail = trafficWeeklyAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
				}).ToList();
			return trafficSumBySites;
		}

		private IEnumerable<DistributionSummary> CreateRegionData(IEnumerable<DistributionModel> regInQueue, IEnumerable<FiscalPeriod> periodSearchList)
		{
			var trafficByRegionPeriod = regInQueue.GroupBy(x => new { x.ID, x.TimeIndex })
				.Select(s => new DistributionModel()
				{
					Count = s.Any() ? s.Sum(x=>x.Count) : 0,
					Date = s.Any() ? s.FirstOrDefault().Date : DateTime.MinValue,
					DWell = s.Any() ? Convert.ToInt32(s.Average(x => x.DWell)) : 0,
					PACID = s.Any() ? s.FirstOrDefault().PACID : 0,
					ID = s.Key.ID,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					TimeIndex = s.Key.TimeIndex,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					Title = s.Key.TimeIndex.ToString(),
					KDVR = s.Any() ? s.FirstOrDefault().KDVR : 0,
					ChannelNo = s.Any() ? s.FirstOrDefault().ChannelNo : 0
				});
			var regionHasData = trafficByRegionPeriod.Select(x => new { x.ID, x.Name }).Distinct().ToList();
			var RegionWithPeriod = (from reg in regionHasData
									from p in periodSearchList
									select new { ID = reg.ID, Name = reg.Name, TimeIndex = p.Period, Period = p }).ToList();

			IEnumerable<DistributionModel> trafficWeeklyAll = (from si in RegionWithPeriod
															   join cv in trafficByRegionPeriod on new { si.ID, si.TimeIndex } equals new { cv.ID, cv.TimeIndex } into dat
															   from it in dat.DefaultIfEmpty()
															   select new DistributionModel()
															   {
																   Count = (it != null) ? it.Count : 0,
																   Date = (it != null) ? it.Date : DateTime.MinValue,
																   DWell = (it != null) ? it.DWell : 0,
																   PACID = (it != null) ? it.PACID : 0,
																   ID = si.ID,
																   Name = si.Name,
																   TimeIndex = si.TimeIndex,
																   Title = string.Empty,
																   ParentID = (it != null) ? it.ParentID : 0,
																   ParentName = (it != null) ? it.ParentName : string.Empty,
																   KDVR = (it != null) ? it.KDVR : 0,
																   ChannelNo = (it != null) ? it.ChannelNo : 0
															   }).ToList();

			IEnumerable<DistributionSummary> trafficSumBySites = trafficByRegionPeriod.GroupBy(x => x.ID)
				.Select(s => new DistributionSummary()
				{
					Count = s.Any() ? s.Sum(i => i.Count) : 0,
					DWell = s.Any() ? Convert.ToInt32(s.Average(i => i.DWell)) : 0,
					ID = s.Key,
					Name = s.Any() ? s.FirstOrDefault().Name : string.Empty,
					ParentID = s.Any() ? s.FirstOrDefault().ParentID : 0,
					ParentName = s.Any() ? s.FirstOrDefault().ParentName : string.Empty,
					DataDetail = trafficWeeklyAll.Where(w => w.ID == s.Key).OrderBy(x => x.TimeIndex)
				}).ToList();
			return trafficSumBySites;
		}

		private List<DistributionModel> QueueChartDataAll(IEnumerable<DistributionModel> dataWithQueue, int qID, DateTime curDate, int timeIdx)
		{
			var allRegionInQueue = dataWithQueue.Where(x => x.ParentID == qID).Select(x => new { x.ID, x.Name }).Distinct().ToList();
			var curRegionData = (timeIdx < 0) ? dataWithQueue.Where(x => x.ParentID == qID && x.Date == curDate) : dataWithQueue.Where(x => x.ParentID == qID && x.TimeIndex == timeIdx);

			List<DistributionModel> chartDataAll = (from r in allRegionInQueue
														   join t in curRegionData on r.ID equals t.ID into dat
														   from it in dat.DefaultIfEmpty()
														   select new DistributionModel()
														   {
															   Count = (it != null) ? it.Count : 0,
															   Date = curDate,//(it != null) ? it.Date : DateTime.MinValue,
															   DWell = (it != null) ? it.DWell : 0,
															   PACID = (it != null) ? it.PACID : 0,
															   ID = r.ID,
															   Name = r.Name,//(it != null) ? it.Name : string.Empty,
															   TimeIndex = timeIdx < 0 ? 0 : timeIdx, //(it != null) ? it.TimeIndex : 0
															   KDVR = (it != null) ? it.KDVR : 0,
															   ChannelNo = (it != null) ? it.ChannelNo : 0,
														   }).ToList();
			return chartDataAll;
		}
		private IEnumerable<DistChartData> CreateChartDetail(IEnumerable<DistributionModel> dataWithQueue, int qID, DateTime curDate, int timeIdx, List<Func_BAM_TrafficCountReportMonthly_Result> trafficCounYTD)
		{
			//var allRegionInQueue = dataWithQueue.Where(x => x.ParentID == qID).Select(x => new { x.ID, x.Name}).Distinct().ToList();
			//var curRegionData = (timeIdx < 0) ? dataWithQueue.Where(x => x.ParentID == qID && x.Date == curDate) : dataWithQueue.Where(x => x.ParentID == qID && x.TimeIndex == timeIdx);

			//IEnumerable<DistributionModel> chartDataAll = (from r in allRegionInQueue
			//											   join t in curRegionData on r.ID equals t.ID into dat
			//											   from it in dat.DefaultIfEmpty()
			//											   select new DistributionModel()
			//											   {
			//												   Count = (it != null) ? it.Count : 0,
			//												   Date = curDate,//(it != null) ? it.Date : DateTime.MinValue,
			//												   DWell = (it != null) ? it.DWell : 0,
			//												   PACID = (it != null) ? it.PACID : 0,
			//												   ID = r.ID,
			//												   Name = r.Name,//(it != null) ? it.Name : string.Empty,
			//												   TimeIndex = timeIdx < 0 ? 0 : timeIdx //(it != null) ? it.TimeIndex : 0
			//											   }).ToList();
			IEnumerable<DistributionModel> chartDataAll = QueueChartDataAll(dataWithQueue, qID, curDate, timeIdx);

			//dataWithQueue.Where(x => x.ParentID == qID && x.Date == curDate)
			IEnumerable<DistChartData> retData = chartDataAll.GroupBy(x => x.ID).Select(gr => new DistChartData() 
				{
					ID = gr.Key,
					Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
					Count = gr.Any() ? gr.Sum(x=>x.Count) : 0,
					Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x=>x.DWell)) : 0,
					KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
					ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
					DataYTD = GetDistYTDData(trafficCounYTD, gr.Key, curDate, qID)
				});
			return retData;
		}

		private IEnumerable<DistChartData> CreateChartDetail(IEnumerable<DistributionModel> dataWithQueue, int qID, DateTime curDate, int timeIdx, List<Func_BAM_TrafficCountReportHourly_Result> trafficCounYTD)
		{
			//var allRegionInQueue = dataWithQueue.Where(x => x.ParentID == qID).Select(x => new { x.ID, x.Name }).Distinct().ToList();
			//var curRegionData = (timeIdx < 0) ? dataWithQueue.Where(x => x.ParentID == qID && x.Date == curDate) : dataWithQueue.Where(x => x.ParentID == qID && x.TimeIndex == timeIdx);

			//IEnumerable<DistributionModel> chartDataAll = (from r in allRegionInQueue
			//											   join t in curRegionData on r.ID equals t.ID into dat
			//											   from it in dat.DefaultIfEmpty()
			//											   select new DistributionModel()
			//											   {
			//												   Count = (it != null) ? it.Count : 0,
			//												   Date = curDate,//(it != null) ? it.Date : DateTime.MinValue,
			//												   DWell = (it != null) ? it.DWell : 0,
			//												   PACID = (it != null) ? it.PACID : 0,
			//												   ID = r.ID,
			//												   Name = r.Name,//(it != null) ? it.Name : string.Empty,
			//												   TimeIndex = timeIdx < 0 ? 0 : timeIdx //(it != null) ? it.TimeIndex : 0
			//											   }).ToList();
			IEnumerable<DistributionModel> chartDataAll = QueueChartDataAll(dataWithQueue, qID, curDate, timeIdx);

			//dataWithQueue.Where(x => x.ParentID == qID && x.Date == curDate)
			IEnumerable<DistChartData> retData = chartDataAll.GroupBy(x => x.ID).Select(gr => new DistChartData()
			{
				ID = gr.Key,
				Name = gr.Any() ? gr.FirstOrDefault().Name : string.Empty,
				Count = gr.Any() ? gr.Sum(x => x.Count) : 0,
				Dwell = gr.Any() ? Convert.ToInt32(gr.Average(x => x.DWell)) : 0,
				KDVR = gr.Any() ? gr.FirstOrDefault().KDVR : 0,
				ChannelNo = gr.Any() ? gr.FirstOrDefault().ChannelNo : 0,
				DataYTD = GetDistYTDData(trafficCounYTD, gr.Key, timeIdx, qID)
			});
			return retData;
		}

		private DriveThroughBase GetDistYTDData(List<Func_BAM_TrafficCountReportMonthly_Result> trafficCounYTD, int regID, DateTime curDate, int qID = 0)
		{
			if (trafficCounYTD == null || !trafficCounYTD.Any())
				return new DriveThroughBase() { Count = 0, Dwell = 0 };
			DateTime endDate = EndTimeOfDate(curDate);
			IEnumerable<Func_BAM_TrafficCountReportMonthly_Result> dataYTD = null;
			DriveThroughBase retData = null;
			if (qID <= 0)
			{
				dataYTD = trafficCounYTD.Where(x => x.RegionID == regID && x.DVRDate <= endDate);
				retData = dataYTD.Any() ? new DriveThroughBase() { Count = Convert.ToInt32(dataYTD.Average(x => x.Count)), Dwell = Convert.ToInt32(dataYTD.Average(x => x.DWellTime)) } : new DriveThroughBase() { Count = 0, Dwell = 0 };
			}
			else
			{
				dataYTD = trafficCounYTD.Where(x => x.QueueID == qID && x.DVRDate <= endDate);
				var curRegion = dataYTD.Where(x => x.RegionID == regID);
				int dwell = !curRegion.Any() ? 0 : Convert.ToInt32(curRegion.Average(x => x.DWellTime));

				var countByRegion = dataYTD.GroupBy(x => x.RegionID).Select(gr => new 
					{
						Count = !gr.Any() ? 0 : Convert.ToInt32(gr.Average(x=>x.Count))
					});
				int count = !countByRegion.Any() ? 0 : countByRegion.Sum(x=>x.Count);
				retData = new DriveThroughBase() { Count = count, Dwell = dwell };
			}
			return retData;
		}

		private DriveThroughBase GetDistYTDData(List<Func_BAM_TrafficCountReportHourly_Result> trafficCounYTD, int regID, int curHour, int qID = 0)
		{
			if (trafficCounYTD == null || !trafficCounYTD.Any())
				return new DriveThroughBase() { Count = 0, Dwell = 0 };

			var dataYTD = trafficCounYTD.Where(x => x.RegionID == regID && x.DVRHour == curHour);
			//DriveThroughBase retData = dataYTD.Any() ? new DriveThroughBase() { Count = Convert.ToInt32(dataYTD.Average(x => x.Count)), Dwell = Convert.ToInt32(dataYTD.Average(x => x.DWellTime)) } : new DriveThroughBase() { Count = 0, Dwell = 0 };
			DriveThroughBase retData = null;
			if (qID <= 0)
			{
				dataYTD = trafficCounYTD.Where(x => x.RegionID == regID && x.DVRHour == curHour);
				retData = dataYTD.Any() ? new DriveThroughBase() { Count = Convert.ToInt32(dataYTD.Average(x => x.Count)), Dwell = Convert.ToInt32(dataYTD.Average(x => x.DWellTime)) } : new DriveThroughBase() { Count = 0, Dwell = 0 };
			}
			else
			{
				dataYTD = trafficCounYTD.Where(x => x.QueueID == qID && x.DVRHour == curHour);
				var curRegion = dataYTD.Where(x => x.RegionID == regID);
				int dwell = !curRegion.Any() ? 0 : Convert.ToInt32(curRegion.Average(x => x.DWellTime));

				var countByRegion = dataYTD.GroupBy(x => x.RegionID).Select(gr => new
				{
					Count = !gr.Any() ? 0 : Convert.ToInt32(gr.Average(x => x.Count))
				});
				int count = !countByRegion.Any() ? 0 : countByRegion.Sum(x => x.Count);
				retData = new DriveThroughBase() { Count = count, Dwell = dwell };
			}
			return retData;
		}

		private DateTime GetDateByPeriod(List<FiscalPeriod> fyPeriods, int idx)
		{
			FiscalPeriod per = fyPeriods.FirstOrDefault(x=>x.Period == idx);
			return per != null ? per.EndDate : DateTime.MinValue;
		}
		private DateTime GetDateByWeek(List<FiscalWeek> fyWeeks, int idx)
		{
			FiscalWeek per = fyWeeks.FirstOrDefault(x => x.WeekIndex == idx);
			return per != null ? per.EndDate : DateTime.MinValue;
		}

		#region Queue & Region
		private async Task<List<TrafficCountRegionInQueue_Result>> GetQueue(UserContext userLogin, AddQueueParam pram)
		{
			IEnumerable<int> selectedSites = string.IsNullOrEmpty(pram.siteKeys) ? null : pram.siteKeys.Split(',').Select(s => int.Parse(s));
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesBySiteIDsAsync(IUser, userLogin, selectedSites);
			string pacIds = string.Join(",", sites.Select(s => s.PACID).Distinct().ToList());

			List<TrafficCountRegionInQueue_Result> bamData = await DataService.GetTrafficCountRegionInQueue(pacIds, userLogin.ID);
			bamData = bamData.Distinct().ToList();
			return bamData;
		}
		public async Task<DistributionQueueRegion> GetArea(UserContext userLogin, AddQueueParam pram)
		{
			List<DistributionSummary> resultData = new List<DistributionSummary>();
			List<TrafficCountRegionInQueue_Result> bamData = await GetQueue(userLogin, pram);

			List<DistributionQueue> queueData = bamData.Where(x => x.QueueID != null).GroupBy(x => x.QueueID).Select(group => new DistributionQueue()
			{
				id = group.Key ?? 0,
				cid = group.Key ?? 0,
				Name = group.Any() ? group.FirstOrDefault().QueueName : string.Empty,
				areas = !group.Any() ? null : group.Select(x => new DistributionQueueBase()
				{
					id = x.RegionIndex,
					Name = x.RegionName
				})
			}).OrderBy(x => x.Name).ToList();
			List<DistributionQueueBase> AllRegion = bamData.GroupBy(x => x.RegionIndex).Select(group => new DistributionQueueBase()
			{
				id = group.Key,
				Name = group.Any() ? group.FirstOrDefault().RegionName : string.Empty
			}).OrderBy(x=>x.Name).ToList();


			DistributionQueueRegion abc = new DistributionQueueRegion();
			abc.QueueData = queueData;
			abc.AllRegions = AllRegion;
			//DataService.SelectQueue(userLogin.ID);
			return abc;

		}

		public tbl_IOPC_QueueList AddqueueNew(UserContext userLogin,DistributionQueue queue) 
		{
			tbl_IOPC_QueueList dataQueue = new tbl_IOPC_QueueList();

			dataQueue.QueueName = queue.Name;
			dataQueue.UserID = userLogin.ID;
			IEnumerable<tbl_IOPC_TrafficCountRegion> SelectedRegion = DataService.SelectRegion(queue.areas.Select(x => x.id).ToList());
			IEnumerable<tbl_IOPC_TrafficCountRegion> OldRegion = dataQueue.QueueID > 0 ? dataQueue.tbl_IOPC_TrafficCountRegion : null;
			DataService.Modifyrelation<tbl_IOPC_TrafficCountRegion, int>(dataQueue, OldRegion, SelectedRegion, uit => uit.RegionIndex, it => it.tbl_IOPC_TrafficCountRegion);

			tbl_IOPC_QueueList metricDB = DataService.AddQueue(dataQueue);
			return metricDB;
		}

		public async Task<TransactionalModel<List<DistributionQueue>>> AddQueue(UserContext userLogin, AddQueueParam QueueModel)
		{
			TransactionalModel<List<DistributionQueue>> returnmodel = new TransactionalModel<List<DistributionQueue>>();
			List<TrafficCountRegionInQueue_Result> bamData = await GetQueue(userLogin, QueueModel);
			List<DistributionQueue> QueueListDB = bamData.Where(x => x.QueueID != null).GroupBy(x => x.QueueID).Select(group => new DistributionQueue()
			{
				id = group.Key ?? 0,
			}).ToList();
			var itemRemoved = QueueListDB.Select(x => x.id).Except(QueueModel.QueueData.Select(x => x.id));
			foreach (int item in itemRemoved)
			{
				DeleteQueue(userLogin, item);
			}
			foreach (DistributionQueue data in QueueModel.QueueData)
			{
				if (data.cid == 0)
				{
					AddqueueNew(userLogin, data);
					//tbl_IOPC_QueueList dataQueue = new tbl_IOPC_QueueList();

					//dataQueue.QueueName = data.Name;
					//dataQueue.UserID = userLogin.ID;
					//IEnumerable<tbl_IOPC_TrafficCountRegion> SelectedRegion = DataService.SelectRegion(data.areas.Select(x => x.id).ToList());
					//IEnumerable<tbl_IOPC_TrafficCountRegion> OldRegion = dataQueue.QueueID > 0 ? dataQueue.tbl_IOPC_TrafficCountRegion : null;
					//DataService.Modifyrelation<tbl_IOPC_TrafficCountRegion, int>(dataQueue, OldRegion, SelectedRegion, uit => uit.RegionIndex, it => it.tbl_IOPC_TrafficCountRegion);

					//tbl_IOPC_QueueList metricDB = DataService.AddQueue(dataQueue);
				}
				else
				{

					foreach (DistributionQueue qq in QueueListDB)
					{
						if (qq.id == data.id)
						{
							//update
							var dataQueue = DataService.SelectQueue(userLogin.ID, data.id);
							

							ServiceBase.Includes<tbl_IOPC_QueueList, tbl_IOPC_TrafficCountRegion>(dataQueue, it => it.tbl_IOPC_TrafficCountRegion);

							IEnumerable<tbl_IOPC_TrafficCountRegion> SelectedRegion = DataService.SelectRegion(data.areas.Select(x => x.id).ToList());
							IEnumerable<tbl_IOPC_TrafficCountRegion> OldRegion = dataQueue.QueueID > 0 ? dataQueue.tbl_IOPC_TrafficCountRegion : null;
							DataService.Modifyrelation<tbl_IOPC_TrafficCountRegion, int>(dataQueue, OldRegion, SelectedRegion, uit => uit.RegionIndex, it => it.tbl_IOPC_TrafficCountRegion);
							dataQueue.QueueName = data.Name;
							dataQueue.UserID = userLogin.ID;
							DataService.UpdateRegion(dataQueue);
						}
					}

				}
			}


			return returnmodel;
		}

		public TransactionalModel<DistributionQueue> DeleteQueue(UserContext userLogin, int QueueID)
		{
			TransactionalModel<DistributionQueue> response = new TransactionalModel<DistributionQueue>();

			var fr = DataService.SelectQueue(userLogin.ID, QueueID);
			DataService.DeleteQueue(fr);

			response.ReturnStatus = true;
			response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());


			return response;
		}


		public async Task<TransactionalModel<List<DistributionQueue>>> applyStore(UserContext userLogin, AddQueueParam QueueModel)
		{
			TransactionalModel<List<DistributionQueue>> returnmodel = new TransactionalModel<List<DistributionQueue>>();
			List<TrafficCountRegionInQueue_Result> bamData = await GetQueue(userLogin, QueueModel);

			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesAsync(IUser, userLogin);
			List<int> pacid = sites.Select(s => s.PACID ?? 0).Distinct().ToList();
			List<int> _regionIdList = new List<int>();
			List<int> _queueList = new List<int>();
			//var queueList = QueueModel.QueueData.Select(x => x.id).ToList();
			//var regionList = QueueModel.QueueData.Select(x => x.areas);
			foreach (DistributionQueue data in QueueModel.QueueData)
			{
				_queueList.Add(data.id);
				foreach(int dt in data.areas.Select(x=>x.id))
				{
					_regionIdList.Add(dt);
				}
			}
			//var regionIdList = _regionIdList.Distinct().ToList();
			List<int> queueList = QueueModel.QueueData.Select(x => x.id).ToList();
			string regionIdList = string.Join(",", _regionIdList.Distinct().ToList());

			IEnumerable<tbl_IOPC_QueueList> queueData = DataService.SelectQueueDeleteAll(userLogin.ID, queueList);

			

			foreach(int dt in queueData.Select(x=>x.QueueID).ToList())
			{
				DeleteQueue(userLogin, dt);
			}

		

			foreach (DistributionQueue data in QueueModel.QueueData)
			{

				var OlddataQueue = DataService.SelectQueue(userLogin.ID, data.id);
				if (OlddataQueue == null)
				{
					tbl_IOPC_QueueList qData = AddqueueNew(userLogin, data);
					OlddataQueue = DataService.SelectQueue(userLogin.ID, qData.QueueID);
				}

					ServiceBase.Includes<tbl_IOPC_QueueList, tbl_IOPC_TrafficCountRegion>(OlddataQueue, it => it.tbl_IOPC_TrafficCountRegion);

					IEnumerable<tbl_IOPC_TrafficCountRegion> SelectedRegion = DataService.SelectRegion(data.areas.Select(x => x.id).ToList());
					IEnumerable<tbl_IOPC_TrafficCountRegion> aa = OlddataQueue.QueueID > 0 ? OlddataQueue.tbl_IOPC_TrafficCountRegion : null;
					DataService.Modifyrelation<tbl_IOPC_TrafficCountRegion, int>(OlddataQueue, aa, SelectedRegion, uit => uit.RegionIndex, it => it.tbl_IOPC_TrafficCountRegion);
					OlddataQueue.QueueName = data.Name;

					tbl_IOPC_QueueList dataQueuea = new tbl_IOPC_QueueList();
					IEnumerable<tbl_IOPC_TrafficCountRegion> SelectedRegiona = DataService.SelectRegion(data.areas.Select(x => x.id).ToList());

					var listIdRegiona = SelectedRegiona.Select(x => x.RegionNameID).ToList();
					IEnumerable<tbl_IOPC_TrafficCountRegion> extraRegion = DataService.SelectRegionByNameId(listIdRegiona, pacid);
					var regionNeed = extraRegion.Select(x => x.RegionIndex).Except(OlddataQueue.tbl_IOPC_TrafficCountRegion.Select(x => x.RegionIndex)).ToList();
					IEnumerable<tbl_IOPC_TrafficCountRegion> newdata = DataService.SelectRegion(regionNeed);

					IEnumerable<tbl_IOPC_TrafficCountRegion> OldRegion = dataQueuea.QueueID > 0 ? dataQueuea.tbl_IOPC_TrafficCountRegion : null;


					DataService.Modifyrelation<tbl_IOPC_TrafficCountRegion, int>(dataQueuea, OldRegion, newdata, uit => uit.RegionIndex, it => it.tbl_IOPC_TrafficCountRegion);

					dataQueuea.QueueName = data.Name;
					dataQueuea.UserID = userLogin.ID;
					tbl_IOPC_QueueList metricDB = DataService.AddQueue(dataQueuea);
			}
			return returnmodel;
		}
		#endregion

        //HEATMAP

        public async Task<HeatMapsModel> GetDataHeatMap(UserContext userLogin, BAMHeatMapParam param)
        {
            // Set param
            DateTime sDate = Convert.ToDateTime(param.sDate);
            DateTime eDate = Convert.ToDateTime(param.eDate);
            int kDVR = param.kDVR;
            int scheduleType;

            switch (param.rptDataType)
            {
                case Utils.BAMReportType.Hourly:
                    scheduleType = (int)Utils.BAMReportType.Hourly;
                    break;
                case Utils.BAMReportType.Daily:
                    scheduleType = (int)Utils.BAMReportType.Daily;
                    break;
                case Utils.BAMReportType.Weekly:
                    scheduleType = (int)Utils.BAMReportType.Weekly;
                    tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(userLogin, param.sDate);
                    if (fyInfo != null)
                    {
                        FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, sDate, fyInfo.FYDateStart.Value);
                        sDate = fyWeekInfo != null ? fyWeekInfo.StartDate : param.sDate;
                        eDate = fyWeekInfo != null ? fyWeekInfo.EndDate : param.eDate;
                    }
                    break;
                default:
                    scheduleType = 0;
                    break;
            }

            var taskChannel = DataService.GetTaskChannel(userLogin.ID, kDVR);
            var scheduleTasks = DataService.GetScheduledTasks(userLogin.ID, scheduleType);
            var images = DataService.GetImagesFromDate(StartTimeOfDate(sDate), EndTimeOfDate(eDate), userLogin.ID);
            var channels = DataService.GetListChannels(userLogin.ID, kDVR);

            //if (!taskChannel.Any() || !scheduleTasks.Any() || !images.Any()) return null;

            var process = taskChannel.Join(scheduleTasks, s => s.TaskID, e => e.TaskID, (s, e) => new { 
                ProcessID = s.ProcessID,
                TaskID = s.TaskID,
                KChannel = s.KChannel,
                scheduleTypeID = e.ScheduleType
            });

            //if(!process.Any()) return null;

            List<HeatMapsImage> process_images = process.Join(images, s => s.ProcessID, e => e.ProcessID, (s, e) => new HeatMapsImage
            {
                ImageID = e.ImgID,
                ImgName = e.ImgName,
                Title = e.ImgTitle,
                UpdatedDate = e.UploadedDate.HasValue ? e.UploadedDate.Value : DateTime.MinValue,
                paramUpdatedDate = e.UploadedDate.HasValue ? e.UploadedDate.Value.ToString(Consts.QUERY_STRING_DATE_FORMAT_HH) : string.Empty,
                Createdby = e.UploadedBy.HasValue ? e.UploadedBy.Value : 0,
                schedule = new ScheduleTypes()
                {
                    TypeID = s.scheduleTypeID.Value,
                    TypeName = DataService.GetScheduleType(s.scheduleTypeID.Value).TypeName
                },
                Channels = new Channel() 
                {
                    ChannelID = s.KChannel.Value,
                    ChannelName = channels.FirstOrDefault(i => i.KChannel == s.KChannel.Value).Name,
                    ChannelNo = channels.FirstOrDefault(i => i.KChannel == s.KChannel.Value).ChannelNo
                }
            }).ToList();

            var totalCount = process_images.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / param.PageSize);
            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            var page_process_images = process_images
                .OrderBy(x => x.ImageID)
                .Skip(param.PageSize * (param.PageNo - 1))
                .Take(param.PageSize);

            HeatMapsModel models = new HeatMapsModel()
            {
                CurrentPage = param.PageNo,
                TotalPages = paginationHeader.TotalPages,
                KDVR = kDVR,
                mapsImage = page_process_images.ToList()
            };

            return models;
        }


        public TransactionalModel<HeatMapsImage> UploadFromDialog(int KDVR, int id, HttpFileCollection filesCollection, string shedule)
        {
            try
            {
                var fileName = filesCollection[0];
                string path = Path.Combine(AppSettings.AppSettings.Instance.AppData, Consts.ImagesHeatMap_Manual, KDVR.ToString(), shedule);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string ext = Path.GetExtension(fileName.FileName);
                string newName = System.Guid.NewGuid() + ext;
                string filePath = Path.Combine(path, newName);
                fileName.SaveAs(filePath);

                HeatMapsImage map = new HeatMapsImage();
                map.ImgName = newName;
                map.ImageID = id;
                TransactionalModel<HeatMapsImage> result = new TransactionalModel<HeatMapsImage>();
                result.Data = map;
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IEnumerable<tDVRChannels> GetListChannels(UserContext userLogin, int KDVR)
        {
            return DataService.GetListChannels(userLogin.ID, KDVR);
        }

        public async Task<TransactionalModel<HeatMapsImage>> InsertImage(HeatMapsModel Models, UserContext user)
        {
            try
            {
                TransactionalModel<HeatMapsImage> result = new TransactionalModel<HeatMapsImage>();

                HeatMapsModel InsertList = new HeatMapsModel()
                {
                    KDVR = Models.KDVR,
                    mapsImage = Models.mapsImage.Where(item => item.ImageID <= 0).ToList()
                };

                HeatMapsImage Image = InsertList.mapsImage.FirstOrDefault();
                DateTime StartWeek = new DateTime();
                DateTime EndWeek = new DateTime();
                if (Image.schedule.TypeID == (int)Utils.BAMReportType.Hourly)
                {
                    Image.UpdatedDate = DateTime.ParseExact(Image.paramUpdatedDate, "yyyyMMddHH", CultureInfo.InvariantCulture);
                }
                else
                {
                    Image.UpdatedDate = DateTime.ParseExact(Image.paramUpdatedDate.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                }

                if (Image.schedule.TypeID == (int)Utils.BAMReportType.Weekly)
                {
                    tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(user, Image.UpdatedDate.Value);
                    if (fyInfo != null)
                    {
                        FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, Image.UpdatedDate.Value, fyInfo.FYDateStart.Value);
                        StartWeek = fyWeekInfo != null ? fyWeekInfo.StartDate : Image.UpdatedDate.Value;
                        EndWeek = fyWeekInfo != null ? fyWeekInfo.EndDate : Image.UpdatedDate.Value;
                    }
                }


				if (CheckExistImage(user, Models.KDVR, Image.schedule.TypeID, Image.paramUpdatedDate))//Image.Channels.ChannelID
                {
                    //Update
					var images = DataService.GetImageExist(Models.KDVR, Image.schedule.TypeID, Image.UpdatedDate.Value, StartWeek, EndWeek);//Image.Channels.ChannelID

                    string path = Path.Combine(AppSettings.AppSettings.Instance.AppData);
                    foreach(var item in images)
                    {
                        if (item.UploadedBy == null)
                        {
                            // Image of Schedule
                            path = Path.Combine(path, Consts.ImagesHeatMap_Schedules, Models.KDVR.ToString(), Image.schedule.TypeName, item.ImgName);//Image.Channels.ChannelID.ToString()
                        }
                        else 
                        { 
                            // Image of Munally
							path = Path.Combine(path, Consts.ImagesHeatMap_Manual, Models.KDVR.ToString(), Image.schedule.TypeName, item.ImgName);//Image.Channels.ChannelID.ToString()
                        }

                        if (File.Exists(path))
                        {
                            await FileManager.FileDeleteAsync(path);
                        }

                        Image.ImageID = item.ImgID;
                        item.ImgTitle = Image.Title;
                        item.ImgName = Image.ImgName;
                        item.UploadedBy = user.ID;

                        // update info
                        DataService.UpdateImage(item);

                    }

                    if (DataService.SaveImage())
                    {
                        result.ReturnMessage.Add("Success.");
                    }
                    else
                    {
                        result.ReturnMessage.Add("Fail.");
                    }

                    //InsertList.mapsImage.FirstOrDefault().ImageID = Image.ImageID;
                    result.Data = Image;
                    result.ReturnMessage = new List<string>();
                    return result;
                }
                else 
                {
                    //Insert
                    tbl_HM_TaskChannel taskchannel = DataService.AddTaskChannelNew(Image.Channels.ChannelID, Image.schedule.TypeID);

                    if (taskchannel != null)
                    {
                        tbl_HM_Images newImage = new tbl_HM_Images()
                        {
                            ImgID = Image.ImageID,
                            ProcessID = taskchannel.ProcessID,
                            ImgName = Image.ImgName,
                            ImgTitle = Image.Title,
                            UploadedDate = Image.UpdatedDate,
                            UploadedBy = user.ID
                        };

                        if (DataService.InsertImage(newImage))
                        {
                            result.ReturnMessage.Add("Success.");
                            InsertList.mapsImage.Clear();
                            InsertList.mapsImage.Add(Image);
                        }
                        else
                        {
                            result.ReturnMessage.Add("Fail.");
                        }

                        Image.ImageID = newImage.ImgID;
                        result.Data = Image;
                        result.ReturnMessage = new List<string>();
                    }
                    else
                    {
                        result.ReturnMessage.Add("Fail.");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                TransactionalModel<HeatMapsImage> result = new TransactionalModel<HeatMapsImage>();
                result.Data = null;
                result.ReturnMessage = new List<string>();
                result.ReturnMessage.Add("Exception");
                return result;
            }
        }

        private IEnumerable<tbl_HM_Images> Inserts(IEnumerable<HeatMapsImage> InsertList, int KDVR, int userID = 0)
        {
            IEnumerable<tbl_HM_Images> Maps = InsertList.Select(item => Insert(item, KDVR, userID));
            return Maps;
        }

        private tbl_HM_Images Insert(HeatMapsImage InsertItem, int KDVR, int userId = 0)
        {
            tbl_HM_Images NewMap = new tbl_HM_Images();
            NewMap.ImgName = InsertItem.ImgName;
            NewMap.UploadedDate = InsertItem.UpdatedDate;
            NewMap.UploadedBy= userId;
            
            return NewMap;
        }

        public bool CheckExistImage(UserContext usercontext, int KDVR, int schedule, string dateImage)
        {
            string format = Consts.QUERY_STRING_DATE_FORMAT;
            string valuedate = dateImage.Substring(0, 8);
            DateTime StartWeek = new DateTime();
            DateTime EndWeek = new DateTime();
            if (schedule == (int)Utils.BAMReportType.Hourly)
            {
                format = Consts.QUERY_STRING_DATE_FORMAT_HH;
                valuedate = dateImage;
            }

            DateTime ParamDate = DateTime.ParseExact(valuedate, format, CultureInfo.InvariantCulture);
            if (schedule == (int)Utils.BAMReportType.Weekly)
            {
                tCMSWeb_FiscalYear fyInfo = IFiscalYear.GetFiscalYearInfo(usercontext, ParamDate);
                if (fyInfo != null)
                {
                    FiscalWeek fyWeekInfo = IFiscalYear.GetFiscalWeek(fyInfo, ParamDate, fyInfo.FYDateStart.Value);
                    StartWeek = fyWeekInfo != null ? fyWeekInfo.StartDate : ParamDate;
                    EndWeek = fyWeekInfo != null ? fyWeekInfo.EndDate : ParamDate;
                }
            }

            return DataService.CheckExistImage(KDVR, schedule, ParamDate, StartWeek, EndWeek);
        }

        public async Task<IEnumerable<ScheduleTasks>> GetDataScheduleTasks(UserContext usercontext, int ScheduleType)
        {
            var rsTasks = DataService.GetScheduledTasksNoUpload(usercontext.ID, ScheduleType);

            var fd = rsTasks.Select(s => s.tbl_HM_TaskChannel.Select(si => si.tDVRChannels.tCMSWebSites.Select(sk => sk.siteKey).Distinct().Count()).Distinct()).ToList();

            IEnumerable<ScheduleTasks> result = rsTasks.Select(s =>
                new ScheduleTasks()
                {
                    TaskID = s.TaskID,
                    TaskName = s.TaskName,
                    paramStartTime = s.StartTime.HasValue ? s.StartTime.Value.ToString() : string.Empty,
                    paramEndTime = s.EndTime.HasValue ? s.EndTime.Value.ToString() : string.Empty,
                    StartDate = s.StartDate.HasValue ? s.StartDate.Value : DateTime.MinValue,
                    EndDate = s.EndDate.HasValue ? s.EndDate.Value : DateTime.MinValue,
                    paramStartDate = s.StartDate.HasValue ? s.StartDate.Value.ToString(Consts.QUERY_STRING_DATE_FORMAT_HH) : string.Empty,
                    paramEndDate = s.EndDate.HasValue ? s.EndDate.Value.ToString(Consts.QUERY_STRING_DATE_FORMAT_HH) : string.Empty,
                    Status = s.Status.HasValue ? s.Status.Value : -1,
                    CountDvrs = s.tbl_HM_TaskChannel.Select(di => di.tDVRChannels.KDVR).Distinct().Count(),
                    CountSites = CountSite(s.tbl_HM_TaskChannel.Select(ss => ss.tDVRChannels.tCMSWebSites.Select(sss => sss.siteKey)).Distinct()),
                    Channels = s.tbl_HM_TaskChannel.Select(sc =>
                        new Channel() { 
                            ChannelID = (int)sc.KChannel,
                            ChannelName = sc.tDVRChannels.Name,
                            ChannelNo = sc.tDVRChannels.ChannelNo
                    }).Distinct(),
                    Dvrs = s.tbl_HM_TaskChannel.Select(di => di.tDVRChannels.KDVR).Distinct(),
                    scheduleType = s.ScheduleType.HasValue ? s.ScheduleType.Value : 0
                });
            return result;
        }

        private int CountSite(IEnumerable<IEnumerable<int>> value)
        {
            List<int> arr = new List<int>();
            foreach (var a in value)
            {
                foreach (var aa in a)
                {
                    arr.Add(aa);
                }
            }

            return arr.Distinct().Count();
        }

        public async Task<TransactionalModel<ScheduleTasks>> InsertDataScheduleTasks(ScheduleTasks model, UserContext userContext)
        {
            try
            {
                TransactionalModel<ScheduleTasks> result = new TransactionalModel<ScheduleTasks>();

                tbl_HM_ScheduledTasks newscheduletasks = new tbl_HM_ScheduledTasks()
                {
                    TaskName = model.TaskName,
                    StartTime = model.scheduleType != (int)ScheduleType.WEEKLY && model.paramStartTime != string.Empty ? 
                        (TimeSpan?)TimeSpan.ParseExact(model.paramStartTime, "hhmmss", System.Globalization.CultureInfo.InvariantCulture) : null,
                    EndTime = model.scheduleType != (int)ScheduleType.WEEKLY && model.paramEndTime != string.Empty ? 
                        (TimeSpan?)TimeSpan.ParseExact(model.paramEndTime, "hhmmss", System.Globalization.CultureInfo.InvariantCulture) : null,
                    StartDate = model.paramStartDate != string.Empty ? (DateTime?)DateTime.ParseExact(model.paramStartDate, Consts.QUERY_STRING_DATE_FORMAT, CultureInfo.InvariantCulture) : null,
                    EndDate = model.paramEndDate != string.Empty ? (DateTime?)DateTime.ParseExact(model.paramEndDate, Consts.QUERY_STRING_DATE_FORMAT, CultureInfo.InvariantCulture) : null,
                    Status = (int)StatusSchedule.ACTIVE,
                    CreatedBy = userContext.ID,
                    ScheduleType = (byte)model.scheduleType,
                    tbl_HM_TaskChannel = DataService.GetChannel(model.Dvrs.ToList(), model.Channels.Select(s => s.ChannelNo).ToList()).Select(cn =>
                        new tbl_HM_TaskChannel()
                        {
                            KChannel = cn.KChannel
                        }
                    ).ToList()
                };

                if (DataService.InsertScheulde(newscheduletasks))
                {
                    result.ReturnMessage.Add("Success.");
                }
                else
                {
                    result.ReturnMessage.Add("Fail.");
                }

                model.TaskID = newscheduletasks.TaskID;
                result.Data = model;
                result.ReturnMessage = new List<string>();

                return result;
            }
            catch (Exception ex)
            {
                TransactionalModel<ScheduleTasks> result = new TransactionalModel<ScheduleTasks>();
                result.Data = null;
                result.ReturnMessage = new List<string>();
                result.ReturnMessage.Add("Exception");
                return result;
            }
        }

        public async Task<TransactionalModel<ScheduleTasks>> UpdateDataScheduleTasks(ScheduleTasks model, UserContext userContext)
        {
            try
            {
                TransactionalModel<ScheduleTasks> result = new TransactionalModel<ScheduleTasks>();

                var scheduleTasks = DataService.GetScheduledTasksNoUpload(userContext.ID, model.scheduleType);
                var updateScheduleTask = scheduleTasks.Where(w => w.TaskID == model.TaskID).FirstOrDefault();

                updateScheduleTask.TaskName = model.TaskName;
                updateScheduleTask.StartTime = model.scheduleType != (int)ScheduleType.WEEKLY && model.paramStartTime != string.Empty ? 
                    (TimeSpan?)TimeSpan.ParseExact(model.paramStartTime, "hhmmss", System.Globalization.CultureInfo.InvariantCulture) : null;
                updateScheduleTask.EndTime = model.scheduleType != (int)ScheduleType.WEEKLY && model.paramEndTime != string.Empty ? 
                    (TimeSpan?)TimeSpan.ParseExact(model.paramEndTime, "hhmmss", System.Globalization.CultureInfo.InvariantCulture) : null;
                updateScheduleTask.StartDate = model.paramStartDate != string.Empty ? (DateTime?)DateTime.ParseExact(model.paramStartDate, Consts.QUERY_STRING_DATE_FORMAT, CultureInfo.InvariantCulture) : null;
                updateScheduleTask.EndDate = model.paramEndDate != string.Empty ? (DateTime?)DateTime.ParseExact(model.paramEndDate, Consts.QUERY_STRING_DATE_FORMAT, CultureInfo.InvariantCulture) : null;
                updateScheduleTask.tbl_HM_TaskChannel = DataService.GetChannel(model.Dvrs.ToList(), model.Channels.Select(s => s.ChannelNo).ToList()).Select(cn =>
                        new tbl_HM_TaskChannel()
                        {
                            KChannel = cn.KChannel
                        }
                    ).ToList();



                if (DataService.UpdateScheulde(updateScheduleTask))
                {
                    result.ReturnMessage.Add(CMSWebError.EDIT_SUCCESS_MSG.ToString());
                }
                else
                {
                    result.ReturnMessage.Add(CMSWebError.EDIT_FAIL_MSG.ToString());
                }

                result.Data = model;

                return result;
            }
            catch (Exception ex)
            {
                TransactionalModel<ScheduleTasks> result = new TransactionalModel<ScheduleTasks>();
                result.Data = null;
                result.ReturnMessage = new List<string>();
                result.ReturnMessage.Add("Exception");
                return result;
            }
        }

        public async Task<TransactionalModel<ScheduleTasks>> DeleteDataScheduleTasks(ScheduleTasks model, UserContext userContext)
        {
            TransactionalModel<ScheduleTasks> response = new TransactionalModel<ScheduleTasks>();

            var scheduleTasks = DataService.GetScheduledTasksNoUpload(userContext.ID, model.scheduleType);
            var updateScheduleTask = scheduleTasks.Where(w => w.TaskID == model.TaskID).FirstOrDefault();

            updateScheduleTask.Status = (int)StatusSchedule.IN_ACTIVE;

            if (DataService.UpdateScheulde(updateScheduleTask))
            {
                response.ReturnMessage.Add(CMSWebError.DELETE_SUCCESS_MSG.ToString());
            }
            else
            {
                response.ReturnMessage.Add(CMSWebError.DELETE_FAIL_MSG.ToString());
            }

            response.ReturnStatus = true;

            return response;
            
        }
    }
}
