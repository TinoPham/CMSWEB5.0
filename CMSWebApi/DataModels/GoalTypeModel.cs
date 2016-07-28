using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace CMSWebApi.DataModels
{
	public class GoalSimple
	{
		public int GoalID { get; set; }
		public string GoalName { get; set; }
	}

	public class GoalModel : GoalSimple
	{
		
		public int? GoalCreateBy { get; set; }
		public DateTime? GoalLastUpdated { get; set; }
		public string UUsername { get; set; }
		public IEnumerable<GoalMap> MapValue{ get; set;}
	}

	
	public class GoalMap
	{
		public int GoalID { get; set; }
		public int GoalTypeID { get; set; }
		public string GoalTypeName { get; set; }
		public Nullable<double> MaxValue { get; set; }
		public Nullable<double> MinValue { get; set; }
	}

	

	public class GoalTypeModel// : TransactionalInformation
	{
		public int GoalTypeID { get; set; }
		public string GoalTypeName { get; set; }
	}
}
