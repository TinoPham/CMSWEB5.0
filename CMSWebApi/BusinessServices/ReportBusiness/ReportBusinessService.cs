using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using System.Data.Entity;
using PACDMModel.Model;
using System.Linq.Expressions;
using CMSWebApi.Cache.Caches;
using CMSWebApi.BusinessServices.ReportBusiness.Alert;
using CMSWebApi.BusinessServices.ReportBusiness.Interfaces;
using CMSWebApi.BusinessServices.ReportBusiness.IOPC;
using CMSWebApi.BusinessServices.ReportBusiness.POS;

namespace CMSWebApi.BusinessServices.ReportBusiness
{
	public partial class ReportBusinessService : BusinessBase<IUsersService>
	{
		#region properties
		public IDVRService IDVRService { get; set; }
		public IPOSService IPOSService { get; set; }
		public IAlertService IAlertService { get ;set;}
		public IIOPCService  IIOPCService{ get ;set;}
		public IGoalTypeService IGoalTypeService{ get ;set;}
		public ISiteService ISiteService{ get ;set;}
		public ICommonInfoService ICommonInfoService{ get;set;}
		public ICompanyService ICompanyService { get; set; }
		public IFiscalYearServices IFiscalYearServices { get; set; }

		private IAlertBusiness IAlertBusiness { get { return new AlertBusiness(){
																					Culture = Culture, 
																					DataService = DataService, 
																					IAlertService = IAlertService, 
																					ICompanyService = ICompanyService, 
																					IDVRService = IDVRService, 
																					IPOSService = IPOSService,
																					Userctx = base.Userctx};} }

		public IIOPCBusinessService IIOPCBusinessService
		{
															get { return new IOPCBusinessService{ 
																		Culture = Culture, 
																		DataService = DataService, 
																		IFiscalYearServices = IFiscalYearServices, 
																		IIOPCService = IIOPCService, 
																		Userctx = Userctx};}}

		public IPOSBusinessService IPOSBusinessService
		{
														get{
															return new POSBusinessService{
																							Culture = Culture, 
																							DataService = DataService, 
																							ICommonInfoService = ICommonInfoService, 
																							IGoalTypeService= IGoalTypeService, 
																							IPOSService =IPOSService, 
																							ISiteService = ISiteService, 
																							Userctx = Userctx};}
		}
		#endregion
		
		#region public methods
		/// <summary>
		///for box-widgets
		/// </summary>
		public async Task<ALertCompModel> CompareAlert(UserContext user, AlertCompareParam param, int keepaliveint)
		{
			
			//need to load UserSiteDvrChannel first
			IEnumerable<UserSiteDvrChannel> uSites = await base.UserSitesAsync(DataService, user); //base.UserSites(DataService, user);

			if (!uSites.Any())
				return null;


			DateTime date = param.Date_Period == Utils.PeriodType.Today ? EndTimeOfDate(param.Date) : param.Date;
			DateTime sdate = (param.Date_Period == Utils.PeriodType.Today || param.interval == 0) ? StartTimeOfDate(date) : date.AddHours(-param.interval);

			DateTime cmpdate = CMSWebApi.Utils.Utilities.ToPeriodDate(date, param.Period, param.PeriodValue);
			DateTime scmpdate = (param.Date_Period == Utils.PeriodType.Today || param.interval == 0) ? StartTimeOfDate(cmpdate) : cmpdate.AddHours(-param.interval);

			ALertCompModel model = null;
			IEnumerable<int> sites = uSites.Where(item => item.KDVR.HasValue && item.KDVR.Value > 0).Select(kdvr => kdvr.KDVR.Value).Distinct();
			IEnumerable<int> pacids = uSites.Where(item => item.PACID.HasValue && item.PACID.Value > 0).Select(kdvr => kdvr.PACID.Value).Distinct();
			switch(param.Cmp)
			{
				case AlertCompareParam.CompareType.AlertType:
					{
						if (param.Values.Count == 1 && param.Values[0] == (int)Utils.AlertType.DVR_Sensor_Triggered)
						{
							model = await IAlertBusiness.AlertTypeComapreByTZ(sites, param.Values, sdate, date, scmpdate, cmpdate);
						}
						else
						{
							model = await IAlertBusiness.AlertTypeComapre(sites, param.Values, sdate.ToUniversalTime(), date.ToUniversalTime(), scmpdate.ToUniversalTime(), cmpdate.ToUniversalTime());
						}
					}
					break;
				case AlertCompareParam.CompareType.DVROffline:
				model = await IAlertBusiness.DVR_On_Offline(sites, DateTime.Now, true, keepaliveint);
				break;
				case AlertCompareParam.CompareType.DVROnline:
				model = await IAlertBusiness.DVR_On_Offline(sites, DateTime.Now, false, keepaliveint);
				break;
				case AlertCompareParam.CompareType.AlertSeverity:
				{
					CMSWebApi.Utils.AlertSeverity? altserver = (param.Values == null || !param.Values.Any()) ? (CMSWebApi.Utils.AlertSeverity?)null : (CMSWebApi.Utils.AlertSeverity?)param.Values.First();
					//model = await AlertSeverityComapre(sites, altserver, sdate.ToUniversalTime(), date.ToUniversalTime(), scmpdate.ToUniversalTime(), cmpdate.ToUniversalTime());

					List<int> altypes = IAlertService.GetAlertTypes<int>(x => x.KAlertSeverity == (byte)altserver, x => (int)x.KAlertType, null).ToList();
					model = await IAlertBusiness.AlertTypeComapre(sites, altypes, sdate.ToUniversalTime(), date.ToUniversalTime(), scmpdate.ToUniversalTime(), cmpdate.ToUniversalTime());
				}
				break;
				case AlertCompareParam.CompareType.Conversion:
					model = await IPOSBusinessService.POSConversionCompare(uSites, sdate, date, scmpdate, cmpdate);
				break;
				case AlertCompareParam.CompareType.Exception:
					IEnumerable<int> extypes = param.Values;
				model = await IPOSBusinessService.POSExceptionCompare(pacids, extypes, sdate, date, scmpdate, cmpdate);
				break;
				case AlertCompareParam.CompareType.Traffic:
				model = await IIOPCBusinessService.TrafficCompare(pacids, sdate, date, scmpdate, cmpdate);
				break;
				case AlertCompareParam.CompareType.Transaction:
				model = await IPOSBusinessService.TransactionCompare(pacids, sdate, date, scmpdate, cmpdate);
				break;
				case AlertCompareParam.CompareType.Sales:
				model = await IPOSBusinessService.TotalSaleCompare(pacids, sdate, date, scmpdate, cmpdate);
				break;
				case AlertCompareParam.CompareType.RecordLess:
					model = new ALertCompModel();
					tCMSSystemConfig sysConfig = user.CompanyID > 0 ? ICompanyService.SelectRecordingDay(user.CompanyID) : null;
					int recodingday = sysConfig != null ? sysConfig.Value : AppSettings.AppSettings.Instance.RecordDayExpected;
					//int recodingday = ICompanyService.SelectRecordingDay(user.CompanyID)!= null? ICompanyService.SelectRecordingDay(user.CompanyID).Value: 0;
					//int recodingday = (param.Values == null ||!param.Values.Any())? AppSettings.AppSettings.Instance.RecordDayExpected : param.Values.First();
					model.Value = await IAlertBusiness.CountDVRRecordingLess(recodingday, uSites.Where(item => item.KDVR.HasValue && item.KDVR.Value > 0).Select(kdvr => kdvr.KDVR.Value));
					model.CmpValue = recodingday;
				break;
				
			}
			 return (model);
		}

		public async Task<IQueryable<ChartWithImageModel>> DashBoardColumnCharts(UserContext user, RptChartParam pram, int keepaliveint)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesAsync(DataService,user);
			if( !sites.Any())
				return new List<ChartWithImageModel>().AsQueryable();

			DateTime edate = pram.Date_Period == Utils.PeriodType.Today? EndTimeOfDate(pram.Date): pram.Date;
			DateTime sdate = Utils.Utilities.ToPeriodDate(edate, pram.Period, pram.PeriodValue, true);
			IQueryable<ChartWithImageModel> result = null;
			List<byte> alttypes = pram.Values == null || pram.Values.Count == 0? null : pram.Values.Select(it => (byte)it).ToList();
			IEnumerable<int> kdvrs = sites.Where(it => it.KDVR.HasValue && it.KDVR.Value > 0).Select(kdvr => kdvr.KDVR.Value).Distinct();
			sdate = sdate.ToUniversalTime();
			edate = edate.ToUniversalTime();
			switch(pram.rptDataType)
			{
				case RptChartParam.DataType.AlertCount:
					//result = await AlertChartbyAlerttype(sdate, edate, alttypes, kdvrs);
					result = await IAlertBusiness.AlertChartbyServerity(sdate, edate, alttypes, kdvrs);
				break;
				case RptChartParam.DataType.DVRCount:
					//result = await AlertChartbyDVR( sdate, edate, kdvrs);
				result = await IAlertBusiness.OverallStatisticChart(sdate, edate, alttypes, kdvrs, user, keepaliveint);
				break;
				case RptChartParam.DataType.DVRMostAlert:
				result = await IAlertBusiness.AlertChartMostDVR(sdate, edate, kdvrs, (pram.Values == null || pram.Values.Count == 0) ? 10 : pram.Values.First());
				break;
			}
			return result;
		}

		public async Task<IEnumerable<TrafficChartModel>>DashboardTraffic(UserContext user, RptChartParam pram)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesAsync(DataService, user);
			if(!sites.Any())
				return new List<TrafficChartModel>();

			DateTime edate = pram.Date_Period == Utils.PeriodType.Today ? EndTimeOfDate(pram.Date) : pram.Date;
			DateTime sdate = Utils.Utilities.ToPeriodDate(edate, pram.Period, pram.PeriodValue, true);
			IEnumerable<int> pacids = sites.Where(it => it.PACID.HasValue && it.PACID.Value > 0).Select(kdvr => kdvr.PACID.Value).Distinct();
			return await IIOPCBusinessService.ChartTraffic(sdate, edate, pram.Period, pacids, user);
		}
		
		public async Task<ConversionChartModel> DashBoardConversionCharts(UserContext user, RptChartParam pram)
		{
			IEnumerable<UserSiteDvrChannel> sites = await base.UserSitesAsync(DataService,user);
			DateTime edate = pram.Date_Period == Utils.PeriodType.Today ? EndTimeOfDate(pram.Date) : pram.Date;//EndTimeOfDate(pram.Date);//
			DateTime sdate = (pram.PeriodValue == 0) ? edate.Date : Utils.Utilities.ToPeriodDate(edate, pram.Period, pram.PeriodValue);
			return await IPOSBusinessService.ConversionChartModel(sdate, edate, sites);
		}

		public async Task<IEnumerable<ConvMapChartModel>> DashBoardConverionMap(UserContext user, RptChartParam pram)
		{
			IEnumerable<UserSiteDvrChannel> sites = null;
			List<int> selectedSites = pram.Values;
			if (selectedSites != null && selectedSites.Count > 0)
			{
				IEnumerable<UserSiteDvrChannel> allsites = await base.UserSitesAsync(DataService, user);
				sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
			}
			else
			{
				sites = await base.UserSitesAsync(DataService, user);
			}
			List<KeyValuePair<string, string>> var = pram.Extend == null ? null : pram.Extend.ToList();
			DateTime edate = pram.Date_Period == Utils.PeriodType.Today ? EndTimeOfDate(pram.Date) : pram.Date;
			DateTime sdate = Utils.Utilities.ToPeriodDate(edate, pram.Period, pram.PeriodValue, true);

			return await IPOSBusinessService.ConversionMapchart(sdate, edate, sites);
		}
		
		public async Task<IEnumerable<ConvSitesChartModel>> DashBoardConverionSites(UserContext user, RptChartParam pram)
		{
			IEnumerable<UserSiteDvrChannel> sites = null;// await base.UserSitesAsync(DataService, user);
			List<int> selectedSites = pram.Values;
			if (selectedSites != null && selectedSites.Count > 0)
			{
				IEnumerable<UserSiteDvrChannel> allsites = await base.UserSitesAsync(DataService, user);
				sites = allsites.Join(selectedSites, si => si.siteKey, se => se, (si, se) => si);
			}
			else
			{
				sites = await base.UserSitesAsync(DataService, user);
			}
			int StateID = 0;
			int TopSelect = 0;
			List<KeyValuePair<string, string>> lsParams = pram.Extend == null ? null : pram.Extend.ToList();
			if (lsParams != null)
			{
				KeyValuePair<string, string> parValue = lsParams.FirstOrDefault(x => x.Key.CompareTo(Utils.Consts.STATE_ID_PARAM) == 0);
				if (!String.IsNullOrEmpty(parValue.Value))
				{
					StateID = Convert.ToInt32(parValue.Value);
				}
				KeyValuePair<string, string> topValue = lsParams.FirstOrDefault(x => x.Key.CompareTo(Utils.Consts.NUM_CONV_SITE_PARAM) == 0);
				if (!String.IsNullOrEmpty(topValue.Value))
				{
					TopSelect = Convert.ToInt32(topValue.Value);
				}
			}
			DateTime edate = pram.Date_Period == Utils.PeriodType.Today ? EndTimeOfDate(pram.Date) : pram.Date;
			DateTime sdate = Utils.Utilities.ToPeriodDate(edate, pram.Period, pram.PeriodValue, true);

			return await IPOSBusinessService.ConversionChartBySites(sdate, edate, sites, StateID, TopSelect == 0 ? Utils.Consts.NUM_CONV_SITE_DEF : TopSelect);
		}
		#endregion

		#region private methods
		private decimal toValueCompare(decimal current, decimal compare)
		{
			if (compare == 0)
				return (current == 0) ? 0 : 100; //#896

			decimal value = (Math.Abs(current - compare) / compare) * 100;
			return Math.Round(value, 2);
		}
		

		
		#endregion
		 
	}
}
