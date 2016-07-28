using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.POS
{
	public class POSListModelService<T> : CMSWebApi.DataServices.ModelServiceBase<T> where T : class
	{
		protected POSListModelService():base(){}

		internal POSListModelService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
		public POSListModelService(ServiceBase svrbase)
			: base(svrbase.DBModel)
		{
		}
	}
}
