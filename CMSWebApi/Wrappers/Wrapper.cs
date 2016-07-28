using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Wrappers.Handlers;
using Commons;

namespace CMSWebApi.Wrappers
{
	public class Wrapper : Commons.SingletonClassBase<Wrapper>
    {
		public Warehouse DBWareHouse{ get { return Warehouse.Instance;}}
		public DVRAlertEvent DVRAlertEvent { get { return DVRAlertEvent.Instance; } }

    }

}
