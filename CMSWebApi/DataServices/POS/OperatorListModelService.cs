using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace CMSWebApi.DataServices.POS
{
	public class OperatorListModelService : CMSWebApi.DataServices.ModelServiceBase<tbl_POS_OperatorList>
	{
		protected OperatorListModelService():base(){}

		public OperatorListModelService(PACDMModel.Model.IResposity dbModel)
			: base(dbModel)
		{
		}
	}
}
