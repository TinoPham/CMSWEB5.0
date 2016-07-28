using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using CMSWebApi.Utils;
using Extensions;
using CMSWebApi.Cache.Caches;
using CMSWebApi.DataModels.DashBoardCache;
using Extensions.Linq;
using CMSWebApi.BusinessServices.ReportBusiness;
using CMSWebApi.BusinessServices.ReportBusiness.Interfaces;

namespace CMSWebApi.BusinessServices.ReportBusiness.IOPC
{
	internal class IOPCBusinessService : BusinessBase<IUsersService>, IIOPCBusinessService
	{
		public IIOPCService IIOPCService{ get ;set;}

		public IFiscalYearServices IFiscalYearServices { get; set; }

		public async Task<IEnumerable<TrafficChartModel>> ChartTraffic(DateTime sdate, DateTime edate, PeriodType ptype, IEnumerable<int> pacids, UserContext user)
		{
			//IEnumerable<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result> counts = await IIOPCService.Func_Fact_IOPC_Periodic_Hourly_Traffic_Async(sdate, edate, pacids);
			IEnumerable<TrafficChartModel> result = null;
			bool isasync = false;
			IEnumerable<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result> Daily_Channels = System.Linq.Enumerable.Empty<Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>();
			switch(ptype)
			{
				case PeriodType.Hour:
					{
						List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);
						IEnumerable<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result> counts = await IOPC_Periodic_Hourly_Traffic(sdate, edate, pacids, lsEnableChannels, ref isasync);
						IEnumerable<Proc_DashBoard_Traffic_ForeCast_Hourly_Result> forecast = await BAM_Get_DashBoard_ForeCast_Hourly(edate, edate, pacids, ref isasync);
						IEnumerable<int> distinc_pacids = counts.Select(it => it.PACID).Distinct();
						IEnumerable<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result> Channels = System.Linq.Enumerable.Empty<Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result>();
						if(distinc_pacids.Count() == 1)//single site- then support link to channel image
							Channels = await IIOPCService.Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels(distinc_pacids.First(), sdate.Date);
							
						

						var retCount = counts.GroupBy(it => it.C_Hour, it => it).OrderBy(x => x.Key).Select(git => new
						{
							key = git.Key,
							countIn = git.Sum(eit => eit.Count_IN.HasValue ? eit.Count_IN.Value : 0),
							countOut = git.Sum(eit => eit.Count_OUT.HasValue ? eit.Count_OUT.Value : 0)
						});

						var retForecastAll = forecast.GroupBy(it => it.C_Hour, it => it)
							.Select(git => new
							{
								key = git.Key,
								forecast = git.Sum(eit => eit.TotalTraffic.HasValue ? eit.TotalTraffic.Value : 0)
							});
						var retForecast = retForecastAll.Where(x => x.forecast > 0);

						var dbdata = retCount.FullOuterJoin(retForecast, cnt => cnt.key, fcs => fcs.key, (cnt, fcs, k) => new
						{
							key = (cnt == null) ? ((fcs == null) ? 0 : fcs.key) : cnt.key,
							countIn = (cnt == null) ? 0 : cnt.countIn,
							countOut = (cnt == null) ? 0 : cnt.countOut,
							forecast = (fcs == null) ? 0 : fcs.forecast
						}).OrderBy(x=>x.key);

						IEnumerable<int> lsHours = new List<int>();
						int first = -1;
						int last = -1;
						var firstItem = dbdata.FirstOrDefault();
						if (firstItem != null)
							first = firstItem.key;
						var lastItem = dbdata.LastOrDefault();
						if (lastItem != null)
							last = lastItem.key;

						if (first >= 0 && last >= first)
						{
							lsHours = ArrayUtilities.SequenceNumber(first, last - first + 1);
							//for (int h = first; h <= last; h++)
							//{
							//	lsHours.Add(h);
							//}
						}
						
						result = (from hh in lsHours
									join d in dbdata on hh equals d.key into dat
									from it in dat.DefaultIfEmpty()
									select new TrafficChartModel
									{
										Label = string.Format(Consts.CHART_LEGEND_HOUR_FORMAT, hh, hh + 1),
										countIn = (it == null) ? 0 : it.countIn,
										countOut = (it == null) ? 0 : it.countOut,
										forecast = (it == null) ? 0 : it.forecast,
										Channels = !Channels.Any() ? null : Channels.Where(itchan => itchan.C_Hour == hh).Select < Func_Fact_IOPC_Periodic_Hourly_Traffic_Channels_Result,
										DVRPACChannel>( itchan => new DVRPACChannel{ Dim_PACID = itchan.DimPACID, DVR_ChannelNo = itchan.ChannelNo, KChannel = itchan.KChannel, KDVR = itchan.KDVR
																					, Name = itchan.Name, PAC_ChannelID = itchan.DimCam, PAC_ChannelName = itchan.PCamName.HasValue?itchan.PCamName.Value: -1})
									});//.Distinct();

					}
					break;
				case PeriodType.Month:
				case PeriodType.LastMonth:
					{
						tCMSWeb_FiscalYear fyInfo = IFiscalYearServices.GetFiscalYearInfo(user, edate);
						sdate = (fyInfo != null && fyInfo.FYDateStart.HasValue) ? fyInfo.FYDateStart.Value : sdate;
						edate = (fyInfo != null && fyInfo.FYDateEnd.HasValue && fyInfo.FYDateEnd.Value < edate) ? fyInfo.FYDateEnd.Value : edate;
						//if (fyInfo != null)
						//{
						//	double numDayOfYear = (double)((fyInfo.FYNoOfWeeks.HasValue ? fyInfo.FYNoOfWeeks : 0) * 7);
						//	DateTime fyEnd = !fyInfo.FYDateEnd.HasValue ? sdate.AddDays(numDayOfYear) : fyInfo.FYDateEnd.Value;
						//	numDayOfYear = (fyEnd - sdate).TotalDays + 1;
						//	while (fyEnd < edate)
						//	{
						//		//sdate = fyEnd.AddDays(1);//sdate.AddDays(0 - numDayOfYear);
						//		fyEnd = fyEnd.AddDays(numDayOfYear);
						//	}
						//	double totalDays = (edate - sdate).TotalDays;

						//	while (totalDays > numDayOfYear)
						//	{
						//		sdate = sdate.AddDays(numDayOfYear);
						//		totalDays = (edate - sdate).TotalDays;
						//	}
						//}

						List<FiscalPeriod> lsPeriods = IFiscalYearServices.GetFiscalPeriods(fyInfo, edate, sdate);

						List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

						IEnumerable<Func_Fact_IOPC_Periodic_Daily_Traffic_Result> counts = await IOPC_Periodic_Daily_Traffic(sdate, edate, pacids, lsEnableChannels, ref isasync); //IIOPCService.Func_Fact_IOPC_Periodic_Daily_Traffic_Async(sdate, edate, pacids);

						IEnumerable<Proc_DashBoard_Traffic_ForeCast_Result> forecastAll = await BAM_Get_DashBoard_ForeCast(sdate, edate, pacids, ref isasync);

						IEnumerable<int> distinc_pacids = counts.Select(it => it.PACID).Distinct();
						IEnumerable<KeyValuePair<int,Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>> period_channel = System.Linq.Enumerable.Empty<KeyValuePair<int,Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>>();
						if (distinc_pacids.Count() == 1)//single site- then support link to channel image
						{
							Daily_Channels = await IIOPCService.Func_Fact_IOPC_Periodic_Daily_Traffic_Channels(distinc_pacids.First(), sdate.Date, edate.Date);
							period_channel = from p in lsPeriods
									from ch in Daily_Channels
									where ch.DVRDateKey >= p.StartDate && ch.DVRDateKey <= p.EndDate
									select new KeyValuePair<int,Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>(p.Period, ch);

						}

						var forecast = forecastAll.Where(x => x.TotalTraffic > 0);

						var count_forecast = counts.FullOuterJoin(forecast, cnt => new { cnt.PACID, cnt.DVRDateKey }, fcs => new { fcs.PACID, fcs.DVRDateKey }, (cnt, fcs, k) => new
						{
							PACID = (cnt != null) ? cnt.PACID : (fcs == null) ? 0: fcs.PACID,
							DVRDateKey = (cnt != null) ? cnt.DVRDateKey : (fcs == null) ? DateTime.MinValue : fcs.DVRDateKey,
							Count_IN = (cnt == null) ? 0: cnt.Count_IN,
							Count_OUT = (cnt == null) ? 0 : cnt.Count_OUT,
							Forecast = (fcs == null) ? 0 : fcs.TotalTraffic
						});

						var period_count = from c in count_forecast
										   from p in lsPeriods
							where c.DVRDateKey >= p.StartDate && c.DVRDateKey <= p.EndDate
							select new { data = c, period = p.Period };

						var dbdata = period_count.GroupBy(it => it.period, it => it).OrderBy(x => x.Key).Select(git => new
						{
							key = git.Key,
							countIn = git.Sum(eit => (eit.data != null && eit.data.Count_IN.HasValue) ? eit.data.Count_IN.Value : 0),
							countOut = git.Sum(eit => (eit.data != null && eit.data.Count_OUT.HasValue) ? eit.data.Count_OUT.Value : 0),
							forecast = git.Sum(eit => (eit.data != null && eit.data.Forecast.HasValue) ? eit.data.Forecast.Value : 0)
						}).OrderBy(x=>x.key);

						IEnumerable<int> lsPeriodIDs = new List<int>();
						int first = -1;
						int last = -1;
						var firstItem = dbdata.FirstOrDefault();
						if (firstItem != null)
							first = firstItem.key;
						var lastItem = dbdata.LastOrDefault();
						if (lastItem != null)
							last = lastItem.key;

						if (first >= 0 && last >= first)
						{
							lsPeriodIDs = ArrayUtilities.SequenceNumber(first, last - first + 1);
							//for (int h = first; h <= last; h++)
							//{
							//	lsPeriodIDs.Add(h);
							//}
						}

						result = (from pe in lsPeriodIDs
								  join d in dbdata on pe equals d.key into dat
								  from it in dat.DefaultIfEmpty()
								  select new TrafficChartModel
								  {
									  Label = string.Format(Consts.CHART_LEGEND_MONTH_FORMAT, pe),
									  countIn = (it == null) ? 0 : it.countIn,
									  countOut = (it == null) ? 0 : it.countOut,
									  forecast = (it == null) ? 0 : it.forecast,
									  Channels = !period_channel.Any()? null: period_channel.Where( ch => ch.Key == pe).Select( sit => sit.Value).GroupBy( git => git.KChannel).Select( fit =>
									  {
										var fgit = fit.First();
										return new DVRPACChannel{
										 Dim_PACID = fgit.DimPACID,
										 DVR_ChannelNo = fgit.ChannelNo,
										 KChannel = fgit.KChannel,
										 KDVR = fgit.KDVR,
										 Name = fgit.Name,
										 PAC_ChannelID = fgit.DimCam,
										 PAC_ChannelName = fgit.PCamName.HasValue? fgit.PCamName.Value: -1};
									  })
								  });//.Distinct();
					}
					break;
				case PeriodType.Day:
				case PeriodType.Now:
				case PeriodType.Today:
					{
						//Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> Tcounts = IOPC_Periodic_Daily_Traffic(sdate, edate, pacids, ref isasync); //IIOPCService.Func_Fact_IOPC_Periodic_Daily_Traffic_Async(sdate, edate, pacids);
						//Task<List<Func_BAM_Get_DashBoard_ForeCast_Result>> Tforecasts = BAM_Get_DashBoard_ForeCast(sdate, edate, pacids, ref isasync);
						List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

						IEnumerable<Func_Fact_IOPC_Periodic_Daily_Traffic_Result> counts = await IOPC_Periodic_Daily_Traffic(sdate, edate, pacids, lsEnableChannels, ref isasync);
						IEnumerable<Proc_DashBoard_Traffic_ForeCast_Result> forecast = await BAM_Get_DashBoard_ForeCast(sdate, edate, pacids, ref isasync);

						IEnumerable<int> distinc_pacids = counts.Select(it => it.PACID).Distinct();
						IEnumerable<KeyValuePair<int, Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>> period_channel = System.Linq.Enumerable.Empty<KeyValuePair<int, Func_Fact_IOPC_Periodic_Daily_Traffic_Channels_Result>>();
						if (distinc_pacids.Count() == 1)//single site- then support link to channel image
						{
							Daily_Channels = await IIOPCService.Func_Fact_IOPC_Periodic_Daily_Traffic_Channels(distinc_pacids.First(), sdate.Date, edate.Date);
						}
						var retCount = counts.GroupBy(it => it.DVRDateKey, it => it).OrderBy(x=>x.Key)
							.Select(git => new { key = git.Key, 
									countIn = git.Sum(eit => eit.Count_IN.HasValue ? eit.Count_IN.Value : 0),
									countOut = git.Sum(eit => eit.Count_OUT.HasValue ? eit.Count_OUT.Value : 0)});

						var retForecastAll = forecast.GroupBy(it => it.DVRDateKey, it => it)
							.Select(git => new { key = git.Key,
								forecast = git.Sum(eit => eit.TotalTraffic.HasValue ? eit.TotalTraffic.Value : 0)
							});
						var retForecast = retForecastAll.Where(x => x.forecast > 0);

						var dbdata = retCount.FullOuterJoin(retForecast, cnt => cnt.key, fcs => fcs.key, (cnt, fcs, k) => new
						{
							key = (cnt == null) ? ((fcs == null) ? DateTime.MinValue : fcs.key) : cnt.key,
							countIn = (cnt == null) ? 0 : cnt.countIn,
							countOut = (cnt == null) ? 0 : cnt.countOut,
							forecast = (fcs == null) ? 0 : fcs.forecast
						}).OrderBy(x=>x.key);

						DateTime first = DateTime.MinValue;//dbdata.FirstOrDefault().key;
						DateTime last = DateTime.MinValue;//dbdata.LastOrDefault().key;
						var firstItem = dbdata.FirstOrDefault();
						if (firstItem != null)
							first = firstItem.key;
						var lastItem = dbdata.LastOrDefault();
						if (lastItem != null)
							last = lastItem.key;

						IEnumerable<DateTime> lsDateTimes = new List<DateTime>();
						if(!(first == DateTime.MinValue && last == DateTime.MinValue))
						{
							int iCount = Convert.ToInt32((last - first).TotalDays);
							lsDateTimes = ArrayUtilities.SequenceDate(first, iCount);
							//for (int i = 0; i <= iCount; i++)
							//{
							//	lsDateTimes.Add(first.AddDays(i));
							//}
							
						}
						result = from day in lsDateTimes
								 join db in dbdata on day equals db.key into dat
								 from it in dat.DefaultIfEmpty()
								 select new TrafficChartModel
								 {
									 Label = day.ToString(Consts.CHART_LEGEND_DATE_FORMAT),
									 countIn = (it == null) ? 0 : it.countIn,
									 countOut = (it == null) ? 0 : it.countOut,
									 forecast = (it == null) ? 0 : it.forecast,
									 Channels = !Daily_Channels.Any()? null: Daily_Channels.Where( dch => dch.DVRDateKey == day).Select(sit=> new DVRPACChannel{
										 Dim_PACID = sit.DimPACID,
										 DVR_ChannelNo = sit.ChannelNo,
										 KChannel = sit.KChannel,
										 KDVR = sit.KDVR,
										 Name = sit.Name,
										 PAC_ChannelID = sit.DimCam,
										 PAC_ChannelName = sit.PCamName.HasValue ? sit.PCamName.Value : -1
									 })
								 };
						
					}
					break;
				case PeriodType.Week:
				case PeriodType.LastWeek:
				break;
				
			}

			return result;
		}

        public Task<List<Func_BAM_TrueTraffic_Opportunity_Result>> GetTrueTrafficOpportunity(DateTime sdate, DateTime edate, IEnumerable<int> pacids)
        {
            return IIOPCService.Func_BAM_TrueTraffic_Opportunity(sdate, edate, pacids);
        }

		public async Task<ALertCompModel> TrafficCompare(IEnumerable<int> pacids, DateTime sdate, DateTime date, DateTime scmpdate, DateTime cmpdate)
		{
			bool isasync = false;
			Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> Ttraff_raw;
			List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result> traff_raw;
			Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> Tcmp_traff_raw;
			List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result> cmp_traff_raw;
			List<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels = await DashBoard_Channel_EnableTrafficCount(pacids, ref isasync);

			Ttraff_raw = IOPC_Periodic_Daily_Traffic(sdate, date, pacids, lsEnableChannels, ref isasync);
			if (isasync)
			{
				Tcmp_traff_raw = IOPC_Periodic_Daily_Traffic(scmpdate, cmpdate, pacids, lsEnableChannels, ref isasync);
				traff_raw = await Ttraff_raw;
				cmp_traff_raw = await Tcmp_traff_raw;
			}
			else
			{
				traff_raw = await Ttraff_raw;
				Tcmp_traff_raw = IOPC_Periodic_Daily_Traffic(scmpdate, cmpdate, pacids, lsEnableChannels, ref isasync);
				cmp_traff_raw = await Tcmp_traff_raw;
			}

			decimal traffic = traff_raw.Any() ? traff_raw.Sum(it => it.Count_IN.HasValue ? it.Count_IN.Value : 0) : 0;
			decimal cmp_traffic = cmp_traff_raw.Any() ? cmp_traff_raw.Sum(it => it.Count_IN.HasValue ? it.Count_IN.Value : 0) : 0;

			return new ALertCompModel { Value = Math.Round(traffic, 2), CmpValue = traffic.toValueCompare(cmp_traffic), Increase = (traffic == cmp_traffic) ? (bool?)null : traffic > cmp_traffic };
		}

		Task<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>> IOPC_Periodic_Daily_Traffic(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync)
		{
			ICache<IOPCCountPeriodicCacheModel>	icache = ResolveCache<IOPCCountPeriodicCacheModel>(sdate, edate);
			isasync = icache != null;
			if( icache == null )
			{
				return IIOPCService.Func_Fact_IOPC_Periodic_Daily_Traffic_Async(sdate, edate, pacids);
			}
			else
			{
				return Task.Run<List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>>( () =>
				{
					if (lsEnableChannels.Any())
					{
						int stime = (int)sdate.DateToUnixTimestamp();
						int etime = (int)edate.DateToUnixTimestamp();
						IEnumerable<IOPCCountPeriodicCacheModel> items = icache.Query<IOPCCountPeriodicCacheModel>(it => it.DVRDate >= stime && it.DVRDate <= etime, it => it);
						var item_dvr = !pacids.Any() ? items : items.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it);
						var item_enchan = item_dvr.Join(lsEnableChannels, it => new { it.PACID, it.CameraID }, pid => new { pid.PACID, pid.CameraID }, (it, pid) => it);

						return item_enchan.GroupBy(it => new { it.PACID, it.DVRDate }).Select(
						it => new Func_Fact_IOPC_Periodic_Daily_Traffic_Result
						{
							DVRDateKey = it.Key.DVRDate.unixTime_ToDateTime(),
							PACID = it.Key.PACID,
							Count_IN = it.Sum(c => !c.ReportNormalize ? c.In : c.InN),
							Count_OUT = it.Sum(c => !c.ReportNormalize ? c.Out : c.OutN)
						}).ToList();
					}
					else
					{
						return new List<Func_Fact_IOPC_Periodic_Daily_Traffic_Result>();
					}
				}
				);
			}
		}

		Task<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>> IOPC_Periodic_Hourly_Traffic(DateTime sdate, DateTime edate, IEnumerable<int> pacids, IEnumerable<Proc_DashBoard_Channel_EnableTrafficCount_Result> lsEnableChannels, ref bool isasync)
		{
			ICache<IOPCCountPeriodicCacheModel> icache = ResolveCache<IOPCCountPeriodicCacheModel>(sdate, edate);
			isasync = icache != null;
			if (icache == null)
			{
				return IIOPCService.Func_Fact_IOPC_Periodic_Hourly_Traffic_Async(sdate, edate, pacids);
			}
			else
			{
				return Task.Run<List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>>(() =>
				{
					if (lsEnableChannels.Any())
					{
						int stime = (int)sdate.ToUnixTimestamp(sdate.Hour);
						int etime = (int)edate.ToUnixTimestamp(edate.Hour);
						IEnumerable<IOPCCountPeriodicCacheModel> items = icache.Query<IOPCCountPeriodicCacheModel>(it => it.DVRDateHour >= stime && it.DVRDateHour <= etime, it => it);
						var item_dvr = !pacids.Any() ? items : items.Join(pacids, it => it.PACID, pid => pid, (it, pid) => it);
						var item_enchan = item_dvr.Join(lsEnableChannels, it => new { it.PACID, it.CameraID }, pid => new { pid.PACID, pid.CameraID }, (it, pid) => it);

						return item_enchan.GroupBy(it => new { it.PACID, it.DVRDate, it.C_Hour }).Select(
						it => new Func_Fact_IOPC_Periodic_Hourly_Traffic_Result
						{
							DVRDateKey = it.Key.DVRDate.unixTime_ToDateTime(),
							PACID = it.Key.PACID,
							C_Hour = it.Key.C_Hour,
							Count_IN = it.Sum(c => !c.ReportNormalize ? c.In : c.InN),
							Count_OUT = it.Sum(c => !c.ReportNormalize ? c.Out : c.OutN)
						}).ToList();
					}
					else
					{
						return new List<Func_Fact_IOPC_Periodic_Hourly_Traffic_Result>();
					}
				}
				);
			}
		}

		Task<List<Proc_DashBoard_Traffic_ForeCast_Result>> BAM_Get_DashBoard_ForeCast(DateTime sdate, DateTime edate, IEnumerable<int> pacids, ref bool isasync)
		{
			return IIOPCService.Proc_DashBoard_Traffic_ForeCast_Async(sdate, edate, pacids, AppSettings.AppSettings.Instance.ForecastFomular, AppSettings.AppSettings.Instance.ForecastWeeks);
		}
		
		Task<List<Proc_DashBoard_Traffic_ForeCast_Hourly_Result>> BAM_Get_DashBoard_ForeCast_Hourly(DateTime sdate, DateTime edate, IEnumerable<int> pacids, ref bool isasync)
		{
			return IIOPCService.Proc_DashBoard_Traffic_ForeCast_Hourly_Async(sdate, edate, pacids, AppSettings.AppSettings.Instance.ForecastFomular, AppSettings.AppSettings.Instance.ForecastWeeks);
		}

		public Task<List<Proc_DashBoard_Channel_EnableTrafficCount_Result>> DashBoard_Channel_EnableTrafficCount(IEnumerable<int> pacids, ref bool isasync)
		{
			return IIOPCService.Proc_DashBoard_Channel_EnableTrafficCount_Async(pacids);
		}
	}
}
