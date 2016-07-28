using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = "", Name = Consts.str_Alarm)]
	public partial class Alarm
	{
		[DataMember]
		public Nullable<int> A_CameraNumber { get; set; }

		[DataMember]
		public Nullable<int> AreaID { get; set; }

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

		[DataMember]
		public Nullable<int> ObjectTypeID { get; set; }

		[DataMember]
		public int AlarmTypeID { get; set; }
	}
}
