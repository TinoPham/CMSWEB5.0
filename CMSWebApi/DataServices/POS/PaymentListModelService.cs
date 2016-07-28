using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.DataServices.POS
{
	public class PaymentListModelService : CMSWebApi.DataServices.ModelServiceBase< tbl_POS_PaymentList>
	{
		protected PaymentListModelService():base(){}

		public PaymentListModelService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
	}
}
