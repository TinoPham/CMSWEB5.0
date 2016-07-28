using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.ATM
{
	[DataContract(Namespace = "", Name= Consts.str_Transact)]
	public partial class Transact
	{
		[DataMember]
		public string TransactKey { get; set; }

		[DataMember]
		public Nullable<long> T_0TransNB { get; set; }

		[DataMember]
		public Nullable<int> T_TransCode { get; set; }

		[DataMember]
		public Nullable<decimal> T_TransAmount { get; set; }

		[DataMember]
		public Nullable<decimal> T_TransTermFee { get; set; }

		[DataMember]
		public Nullable<int> T_TransType { get; set; }

		[DataMember]
		public Nullable<decimal> T_TransTotal { get; set; }

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
		public string BusinessDate { get; set; }

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
		public Nullable<int> T_CameraNB { get; set; }

		[DataMember]
		public Nullable<int> T_CardNB { get; set; }

		[DataMember]
		public Nullable<decimal> T_AcctBalance { get; set; }

		[DataMember]
		public List<KeyValuePair<int, int>> ExStrings { get; set; }
	}
}
