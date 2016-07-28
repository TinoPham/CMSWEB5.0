using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_TrafficCount)]
	public partial class TrafficCount
	{
		[DataMember]
		public int EventID { get; set; }

		[DataMember]
		public int RegionIndex { get; set; }

		[DataMember]
		public Nullable<long> PersonID { get; set; }

		DateTime  _RegionEnterTime;
		[DataMember]
		public DateTime RegionEnterTime { 
			get{ return _RegionEnterTime; }
			set
			{
				if (value.Kind != DateTimeKind.Utc)
					_RegionEnterTime = new DateTime(value.Ticks, DateTimeKind.Utc);

				else
					_RegionEnterTime = value;
			}
		
		}

		DateTime _RegionExitTime;
		[DataMember]
		public DateTime RegionExitTime { 
			get{ return _RegionExitTime;}
			set
			{
				if (value.Kind != DateTimeKind.Utc)
					_RegionExitTime = new DateTime(value.Ticks, DateTimeKind.Utc);

				else
					_RegionExitTime = value;
			}
		}

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
