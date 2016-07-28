using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.DataServices.POS
{
	public class TransactModelService : CMSWebApi.DataServices.ModelServiceBase<tbl_POS_Transact>
	{
		protected TransactModelService():base(){}

		public TransactModelService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
		public TransactModelService(ServiceBase svrbase)
			: base(svrbase.DBModel)
		{
		}
	}
}
