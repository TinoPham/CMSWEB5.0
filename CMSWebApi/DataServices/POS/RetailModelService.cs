using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.DataServices.POS
{
	public class RetailModelService : CMSWebApi.DataServices.ModelServiceBase<tbl_POS_Retail>
	{
		protected RetailModelService():base(){}

		public RetailModelService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
		public RetailModelService(ServiceBase svrbase)
			: base(svrbase.DBModel)
		{
		}
	}
}
