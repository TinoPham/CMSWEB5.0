using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;

namespace CMSWebApi.BusinessServices.POSPayment
{
	public class POSPayments : BusinessBase<DataServices.POS.PaymentListModelService>
	{

		public IQueryable<PaymentModel> PaymentList()
		{
			IQueryable<PaymentModel> ret = DataService.Gets<PaymentModel>(null, null, it => new PaymentModel { ID = it.PaymentID, Name = it.PaymentName });
				//.Select(s => new PaymentListModel()
				//{
				//	ID = s.PaymentID,
				//	Name = s.PaymentName
				//});
			return ret;
		}

		public PaymentModel Payment(string name)
		{
			PaymentModel ret = DataService.Get<PaymentModel>(it => string.Compare(it.PaymentName, name, true) == 0, null, it => new PaymentModel { ID = it.PaymentID, Name = it.PaymentName });
			return ret;
		}

		public PaymentModel Payment(int id)
		{
			PaymentModel ret = DataService.Get<PaymentModel>(it => it.PaymentID == id, null, it => new PaymentModel { ID = it.PaymentID, Name = it.PaymentName });
			return ret;
		}
	}
}
