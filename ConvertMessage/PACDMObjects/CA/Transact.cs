using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.CA
{
	[DataContract(Namespace = "", Name = Consts.str_Transact)]
	public partial class Transact
	{
		
		[DataMember]
		public long Transact_Key { get; set; }

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

		Nullable<System.DateTime> transdate;
		[DataMember]
		public Nullable<System.DateTime> TransDate
		{
			get { return transdate; }
			set
			{
				if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
					transdate = new Nullable<DateTime>(new DateTime(value.Value.Ticks, DateTimeKind.Utc));

				else
					transdate = value;
			}
		}

		[DataMember]
		public Nullable<int> T_CameraNB { get; set; }

		[DataMember]
		public Nullable<int> T_TranType { get; set; }

		[DataMember]
		public Nullable<int> T_UnitID { get; set; }

		[DataMember]
		public Nullable<int> T_SiteID { get; set; }

		[DataMember]
		public Nullable<int> T_DevName { get; set; }

		[DataMember]
		public Nullable<int> T_Batch { get; set; }

		[DataMember]
		public Nullable<int> T_Card { get; set; }

		[DataMember]
		public Nullable<int> T_FullName { get; set; }

		[DataMember]
		public List<KeyValuePair<int, int>> ExStrings { get; set; }
	}
}
