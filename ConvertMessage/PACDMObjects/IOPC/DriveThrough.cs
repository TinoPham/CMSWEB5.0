using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.IOPC
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_DriveThrough)]
	public partial class DriveThrough
	{

		Nullable<System.DateTime> startdate;
		[DataMember]
		public Nullable<System.DateTime> StartDate
		{
			get { return startdate; }
			set
			{
				if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
					startdate = new Nullable<DateTime>(new DateTime(value.Value.Ticks, DateTimeKind.Utc));

				else
					startdate = value;
			}
		}

		Nullable<System.DateTime> enddate { get; set; }
		[DataMember]
		public Nullable<System.DateTime> EndDate
		{
			get { return enddate; }
			set
			{
				if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
					enddate = new Nullable<DateTime>(new DateTime(value.Value.Ticks, DateTimeKind.Utc));

				else
					enddate = value;
			}
		}

		[DataMember]
		public Nullable<int> ExternalCamera { get; set; }

		[DataMember]
		public Nullable<int> InternalCamera { get; set; }

		[DataMember]
		public Nullable<int> Function { get; set; }
	}
}
