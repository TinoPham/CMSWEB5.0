using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = "", Name= Consts.str_Count)]
	public partial class Count
	{
		[DataMember]
		public Nullable<int> C_CameraNumber { get; set; }

		[DataMember]
		public Nullable<int> C_AreaNameID { get; set; }

		[DataMember]
		public Nullable<int> C_ObjectTypeID { get; set; }

		[DataMember]
		public Nullable<long> C_Count { get; set; }

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
