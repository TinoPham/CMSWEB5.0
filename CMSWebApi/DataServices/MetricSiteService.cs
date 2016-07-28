using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using PACDMModel;
using CMSWebApi.DataModels;
using System.Linq.Expressions;


namespace CMSWebApi.DataServices
{
	public partial class MetricSiteService : ServiceBase, IMetricSiteService
	{
		public MetricSiteService(PACDMModel.Model.IResposity model) : base(model) { }

		public MetricSiteService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public tCMSWeb_Metric_List AddMetric(tCMSWeb_Metric_List metric)
		{
			DBModel.Insert<tCMSWeb_Metric_List>(metric);
			return DBModel.Save() > 0 ? metric : null;
		}

		public tCMSWeb_Metric_List UpdateMetric(tCMSWeb_Metric_List metric)
		{
			DBModel.Update<tCMSWeb_Metric_List>(metric);
			return DBModel.Save() >= 0 ? metric : null;
		}

		public bool DeleteMetrics(List<int> metricIds)
		{
			DBModel.DeleteWhere<tCMSWeb_Metric_List>(metric => metricIds.Contains(metric.MListID));
			return DBModel.Save() > 0 ? true : false;
		}

		public IQueryable<Tout> GetMetrics<Tout>(UserContext user, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string [] includes) where Tout : class
		{
			return Query<tCMSWeb_Metric_List, Tout>(t => t.CreateBy == user.ID, selector, includes);
		}

		public IQueryable<Tout> GetMetrics<Tout>(UserContext user, bool? root, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		{
			if (root.HasValue && root.Value == true)
				return Query<tCMSWeb_Metric_List, Tout>(t => t.CreateBy == user.ID && t.ParentID == null, selector, includes);

			return Query<tCMSWeb_Metric_List, Tout>(t => t.CreateBy == user.ID, selector, includes);
		}

		public IQueryable<Tout> SelectMetricChild<Tout>(int parentID, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		{
			IQueryable<Tout> model = Query<tCMSWeb_Metric_List, Tout>(i => i.ParentID == parentID, selector, includes);
			return model;
		}

		public IQueryable<Tout> GetMetricsRoot<Tout>(int user, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWeb_Metric_List, Tout>(metric => metric.CreateBy == user && metric.ParentID == null && metric.isDefault == true, selector, includes);
		}

		public IQueryable<Tout> GetMetricByID<Tout>(UserContext user, List<int> metricIds, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		{
			return Query<tCMSWeb_Metric_List, Tout>(metric => metric.CreateBy == user.ID && metricIds.Contains(metric.MListID) , selector, includes);
		}

		public IQueryable<Tout> GetMetricByParentID<Tout>(UserContext user, int parentId, bool includeParent, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		{
			if(includeParent)
				return Query<tCMSWeb_Metric_List, Tout>(metric => metric.CreateBy == user.ID && (metric.MListID == parentId || metric.ParentID == parentId), selector, includes);
			return Query<tCMSWeb_Metric_List, Tout>(metric => metric.CreateBy == user.ID && metric.ParentID == parentId, selector, includes);
		}

		public IQueryable<Tout> GetMetricByName<Tout>(UserContext user, string metricName, Expression<Func<tCMSWeb_Metric_List, Tout>> selector, string[] includes) where Tout : class
		 {
			return Query<tCMSWeb_Metric_List, Tout>(metric => metric.CreateBy == user.ID && metric.MetricName == metricName, selector, includes);
		}

		public IQueryable<Tout> GetMetricSiteList<Tout>(List<int> metricIds, List<int> siteIds, Expression<Func<tCMSWeb_MetricSiteList, Tout>> selector, string[] includes) where Tout: class
		{
			if (metricIds != null && metricIds.Count > 0 && siteIds != null && siteIds.Count > 0)
			{
				return Query<tCMSWeb_MetricSiteList, Tout>(t => metricIds.Contains(t.MListID) && siteIds.Contains(t.SiteID), selector, includes);
			}
			else if (metricIds != null && metricIds.Count > 0)
			{
				return Query<tCMSWeb_MetricSiteList, Tout>(t => metricIds.Contains(t.MListID), selector, includes);
			}
			else if (siteIds != null && siteIds.Count > 0)
			{
				return Query<tCMSWeb_MetricSiteList, Tout>(t => siteIds.Contains(t.SiteID), selector, includes);
			}

			return Query<tCMSWeb_MetricSiteList, Tout>(null, selector, includes);
		}
	}
}
