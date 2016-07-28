using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_QTime)]
	public partial class QTime
	{
		[DataMember]
		public Nullable<long> PersonID { get; set; }

		[DataMember]
		public string ServiceEnterTime { get; set; }

		[DataMember]
		public string RegisterEnterTime { get; set; }

		[DataMember]
		public string RegisterExitTime { get; set; }

		[DataMember]
		public string PickupEnterTime { get; set; }

		[DataMember]
		public string PickupExitTime { get; set; }

		[DataMember]
		public string ServiceExitTime { get; set; }

		[DataMember]
		public Nullable<int> ExternalChannel { get; set; }

		[DataMember]
		public Nullable<int> InternalChannel { get; set; }

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
