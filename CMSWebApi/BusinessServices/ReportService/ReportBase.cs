using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.ReportService
{
	public class ReportBase
	{
		protected PACDMModel.Model.IResposity DBModel { get; private set; }
		protected UserContext UserContext { get; private set; }
		public ReportBase(UserContext userContext, PACDMModel.Model.IResposity dbModel)
		{
			DBModel = dbModel;
			UserContext = userContext;
		}

        public string ResolveServerUrl(string url, bool forceHttps)
        {
            var serverUrl = VirtualPathUtility.ToAbsolute(url);

            if (serverUrl.IndexOf("://", System.StringComparison.Ordinal) > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = System.Web.HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                "://" + originalUri.Authority + newUrl;
            return newUrl;
        }

		public CompanyModel GetCompanyInfo()
		{
			var svc = (ICompanyService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ICompanyService));
			tCMSWeb_Company tCompany = svc.SelectCompanyInfo(UserContext.CompanyID);
			return new CompanyModel
			{
				CompanyID = tCompany.CompanyID,
				CompanyName = tCompany.CompanyName,
				CompanyLogo = tCompany.CompanyLogo,
				UserID = UserContext.ID,
				UpdateDate = tCompany.UpdateDate
			};
		}

		public  DataTable GetParamsResources(tbl_BAM_Metric_ReportUser report, NameValueCollection requestParms = null)
		{
			var lang = requestParms != null ? string.IsNullOrEmpty(requestParms["languageId"]) != true ? "." + requestParms["languageId"] : ".en" : ".en";

			var pathReport = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettings.AppSettings.Instance.ReportPath, report.tbl_BAM_Metric_ReportList.ReportLocation + lang);

			 var dataResource = ReportFactory.LoadParamResources(pathReport);

			 DataTable parmTable = ReportFactory.ToDataTable<TableParamModel>(dataResource);
			 parmTable.TableName = "ParamReport";
			return parmTable;
		}

		protected BusinessServices.BusinessBase<IType> InitBusiness<BType, IType>()
            where IType : class
            where BType : BusinessServices.BusinessBase<IType>
        {
            BusinessServices.BusinessBase<IType> BService = Resolve(typeof(BType)) as BusinessServices.BusinessBase<IType>;
            IType iservice = DependencyResolver<IType>();
            (BService as BType).DataService = iservice;

            PropertyInfo[] Pinfos = typeof(BType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in Pinfos)
            {
                if (!p.CanRead || !p.CanWrite || p.PropertyType.Equals(typeof(IType)) || !p.PropertyType.IsInterface)
                    continue;
                p.SetValue(BService, DependencyResolver(p.PropertyType));
            }

            return BService;
        }
        private object Resolve(Type type)
        {
            ConstructorInfo constructor = type.GetConstructors().Last();
            ParameterInfo[] parameters = constructor.GetParameters();

            if (!parameters.Any())
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return constructor.Invoke(ResolveParameters(parameters).ToArray());
            }
        }

        private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Select(p => Resolve(p.ParameterType)).ToList();
        }

        protected object DependencyResolver(Type isvr)
        {
            return GlobalConfiguration.Configuration.DependencyResolver.GetService(isvr);
        }
        protected Isvr DependencyResolver<Isvr>() where Isvr : class
        {
            return DependencyResolver(typeof(Isvr)) as Isvr;
        }

        protected DateTime StartTimeOfDate(DateTime date)
        {
            return date.Date;
        }
        protected DateTime EndTimeOfDate(DateTime date)
        {
            return date.Date.AddDays(1).AddMilliseconds(-10);//minus 10 for correct SQL value
        }

	}
}
