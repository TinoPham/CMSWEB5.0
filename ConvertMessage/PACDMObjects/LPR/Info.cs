using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.LPR
{

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_Info)]
	public partial class Info
	{

		[DataMember]
		public long LPR_ID { get; set; }

		[DataMember]
		public Nullable<int> CamNo { get; set; }

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
		public string LPR_Num { get; set; }

		[DataMember]
		public string LPR_Possibility { get; set; }

		[DataMember]
		public string LPR_isMatch { get; set; }

		[DataMember]
		public string LPR_ImageBase64 { get; set; }

        [DataMember]
        public string LPR_PACID { get; set; }
	}
}
