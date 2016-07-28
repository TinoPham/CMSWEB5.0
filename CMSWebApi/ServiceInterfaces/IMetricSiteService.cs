using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface IMetricSiteService
	{
		tCMSWeb_Metric_List AddMetric(tCMSWeb_Metric_List metric);
		tCMSWeb_Metric_List UpdateMetric(tCMSWeb_Metric_List metric);
		bool DeleteMetrics(List<int> metricIds);

		IQueryable<Tout> GetMetrics<Tout>(UserContext user, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string [] includes) where Tout : class;

		IQueryable<Tout> GetMetrics<Tout>(UserContext user, bool? root, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> SelectMetricChild<Tout>(int parentID, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetMetricsRoot<Tout>(int user, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetMetricByID<Tout>(UserContext user, List<int> metricIds, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetMetricByParentID<Tout>(UserContext user, int parentId, bool includeParent, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetMetricByName<Tout>(UserContext user, string metricName, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class;
		IQueryable<Tout> GetMetricSiteList<Tout>(List<int> metricIds, List<int> SiteIds, Expression<Func<tCMSWeb_MetricSiteList, Tout>> selector, string[] includes) where Tout : class;
	}
}

