using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Wrappers.Handlers
{
	public class DVRAlertEvent : Commons.SingletonClassBase<DVRAlertEvent>
	{
		public void Add(PACDMModel.Model.tAlertEvent alert)
		{
			CMSWebApi.Cache.BackgroundTaskManager.Instance.Add(alert);
		}
		public void Add(IList<PACDMModel.Model.tAlertEvent> alerts)
		{
			CMSWebApi.Cache.BackgroundTaskManager.Instance.Add(alerts);
		}
	}
}
