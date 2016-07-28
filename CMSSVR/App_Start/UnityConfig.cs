using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;
using CMSSVR.Controllers;
using ConverterSVR.IServices;
using ConverterSVR.Services;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.DataServices;
using CMSWebApi.Controllers;
using CMSWebApi.Configurations;
using CMSWebApi.Cache;
using System.Configuration;
using System.Web.Http.Dependencies;
using System.Data.Entity.Core.EntityClient;
using PACDMModel.Model;



namespace CMSSVR
{
	public static class UnityConfig
	{
		public static void RegisterComponents()
		{
			var container = new UnityContainer();

			// register all your components with the container here
			// it is NOT necessary to register your controllers

			// e.g. container.RegisterType<ITestService, TestService>();
			RegisterDataBaseTypes(container);
			RegisterConverterTypes(container);
			RegisterCMSWebApiTypes(container);
			GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
			InitDashboardCaches(GlobalConfiguration.Configuration.DependencyResolver);

			InitTableCache(GlobalConfiguration.Configuration.DependencyResolver);

		}

		private static void RegisterDataBaseTypes(IUnityContainer container)
		{
			container.RegisterType<PACDMModel.Model.IResposity, PACDMModel.PACDMDB>(new TransientLifetimeManager(), new InjectionConstructor(new object[] { AppSettings.AppSettings.Instance.PACDM_Model }));
		}

		private static void RegisterConverterTypes(IUnityContainer container)
		{
			//container.RegisterType<SVRDatabase.SVRManager>(new ContainerControlledLifetimeManager(), new InjectionConstructor(new object[] { "LogContext" }));
			//container.RegisterType<PACDMModel.PACDMDB>(new ContainerControlledLifetimeManager(), new InjectionConstructor(new object[] { "PACDMDB" }));

			container.RegisterType<SVRDatabase.SVRManager>(new TransientLifetimeManager(), new InjectionConstructor(new object[] { AppSettings.AppSettings.Instance.LogDB_Model }));

			container.RegisterType<IConvertService, ConvertService>(new TransientLifetimeManager());
			container.RegisterType<IConvertSummaryService, ConvertSummaryService>(new TransientLifetimeManager());
		}

		private static void RegisterCMSWebApiTypes(IUnityContainer container)
		{
			container.RegisterType<IAccountService, AccountService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IUserGroupService, UserGroupService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ICalendarService, CalendarService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IGoalTypeService, GoalTypeService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IMetricSiteService, MetricSiteService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IIncidentService, IncidentService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IJobTitleService, JobTitleService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ISynUserService, SynUserService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ISiteService, SiteService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IRecipientService, RecipientService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IFiscalYearServices, FiscalYearService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ICommonInfoService, CommonInfoService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IUsersService, UsersService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ICompanyService, CompanyService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			//container.RegisterType<IChartService, ChartService>(GetLifeTimeManager());
			container.RegisterType<IDashboardService, DashboardService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IAlertService, AlertService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IDVRService, DVRService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IPOSService, POSService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<INoteService, NoteService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ITodoService, TodoService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IIOPCService, IOPCService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IDvrChanelService, DvrChanelService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IActivityLogService, ActivityLogService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IMapsService, MapsService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ISaleReportsService, SaleReportsService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IBamHeaderService, BamHeaderService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IBamMetricService, BamMetricService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IDistributionService, DistributionService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IExportService, ExportService>(GetLifeTimeManager());
			container.RegisterType<IRebarDataService, RebarDataService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IAdhocDataService, AdhocDataService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IQuickSearchService, QuickSearchService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<ICannedService, CannedService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
			container.RegisterType<IEmailSettingService, EmailSettingService>(GetLifeTimeManager(), new InjectionConstructor(typeof(PACDMModel.Model.IResposity)));
            container.RegisterType<ILicenseService,LisenceService>(GetLifeTimeManager());
		}

		private static TransientLifetimeManager GetLifeTimeManager()
		{
			return new TransientLifetimeManager(); //new PerThreadLifetimeManager();
		}

		private static string DBConnection(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			ConnectionStringSettings con_Setting = ConfigurationManager.ConnectionStrings[name];
			if (con_Setting == null || string.IsNullOrEmpty(con_Setting.ConnectionString))
				return null;
			EntityConnectionStringBuilder en_conbuilder = new EntityConnectionStringBuilder(con_Setting.ConnectionString);
			return en_conbuilder.ProviderConnectionString;

		}

		private static void InitCacheContext()
		{
			PACDMDB DBContext = new PACDMDB(AppSettings.AppSettings.Instance.PACDM_Model, false, false);
			//register context
			BackgroundTaskManager.Instance.RegisterDBContext<PACDMDB>(DBContext);
		}

		private static void InitDashboardCaches(IDependencyResolver IDependencyResolver)
		{
			var config = AppSettings.AppSettings.Instance.DashboardCaches;//  ConfigurationManager.GetSection(DashBoardsSection.DashBoardsSection_Name) as DashBoardsSection;

			//init cache
			BackgroundTaskManager.Instance.InitializeCaches(AppSettings.AppSettings.Instance.AppData, config, IDependencyResolver, AppSettings.AppSettings.Instance.JobName);
			InitCacheContext();
			//register caches
			BackgroundTaskManager.Instance.RegisterEntityCache<tAlertType, PACDMDB>(false);
			//BackgroundTaskManager.Instance.RegisterEntityCache<Dim_POS_CameraNB, PACDMDB>();
			// dashboard caches
			BackgroundTaskManager.Instance.LoadDashboadCaches();
		}
		private static void InitTableCache(IDependencyResolver IDependencyResolver)
		{
			BackgroundTaskManager.Instance.RegisterEntityCache(AppSettings.AppSettings.Instance.TableCaches);
		}
	}
}