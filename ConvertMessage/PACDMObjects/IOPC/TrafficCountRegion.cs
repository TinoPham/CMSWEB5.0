using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_TrafficCountRegion)]
	public partial class TrafficCountRegion
	{
		[DataMember]
		public int RegionIndex{ get; set;}

		[DataMember]
		public byte RegionID { get; set; }

		[DataMember]
		public int RegionNameID { get; set; }

		[DataMember]
		public int InternalChannel { get; set; }

		[DataMember]
		public int ExternalChannel { get; set; }

		Nullable<System.DateTime> dvrdate;
		[DataMember]
		public Nullable<System.DateTime> DVRDate
		{
			get { return dvrdate; }
			set
			{
				if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
					dvrdate = new Nullable<DateTime>(new DateTime(value.Value.Ticks, DateTimeKind.Utc));

				else
					dvrdate = value;
			}
		}

	}
}
