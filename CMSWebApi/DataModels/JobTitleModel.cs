using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{

	public class JobTitleModel
	{
		public int PositionID { get; set; }
		public string PositionName { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Description { get; set; }
		public string CreatedName { get; set; }
		public Nullable<int> Color { get; set; }
	}

	//public class JobTitleData : TransactionalInformation
	//{
	//	public JobTitleModel JobTitle { get; set; }
	//}

}
