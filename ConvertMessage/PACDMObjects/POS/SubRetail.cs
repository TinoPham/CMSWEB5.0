using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.POS
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_SubRetail)]
	public partial class SubRetail
	{
		[DataMember]
		public Nullable<int> SR_2SubItemLineNb { get; set; }
		
		[DataMember]
		public Nullable<double> SR_1Qty { get; set; }
		
		[DataMember]
		public Nullable<decimal> SR_0Amount { get; set; }
		
		[DataMember]
		public Nullable<int> SR_Description { get; set; }
	}
}
