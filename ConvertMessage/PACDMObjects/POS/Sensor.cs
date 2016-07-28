using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.POS
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_Sensor)]
	public partial class Sensor
	{
		[DataMember]
		public System.Guid S_ID { get; set; }
		[DataMember]
		public string OT_Start { get; set; }
		[DataMember]
		public string OT_End { get; set; }
		[DataMember]
		public string GT_Start { get; set; }
		[DataMember]
		public string GT_End { get; set; }
		[DataMember]
		public string PU_Start { get; set; }
		[DataMember]
		public string PU_End { get; set; }
		
		Nullable<System.DateTime> date;
		[DataMember]
		public Nullable<System.DateTime> DVRDate
		{
			get { return date; }
			set
			{
				if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
					date = new Nullable<DateTime>(new DateTime(value.Value.Ticks, DateTimeKind.Utc));

				else
					date = value;
			}
		}
	}
}
