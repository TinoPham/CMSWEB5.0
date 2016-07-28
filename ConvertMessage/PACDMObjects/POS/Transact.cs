using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.POS
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_Transact) ]
	public partial class Transact
	{
		[DataMember]
		public string TransactKey { get; set; }
		[DataMember]
		public Nullable<long> T_0TransNB { get; set; }
		[DataMember]
		public Nullable<decimal> T_6TotalAmount { get; set; }
		[DataMember]
		public Nullable<decimal> T_1SubTotal { get; set; }
		[DataMember]
		public Nullable<decimal> T_8ChangeAmount { get; set; }

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
		public Nullable<int> T_9RecItemCount { get; set; }
		[DataMember]
		public Nullable<int> T_CameraNB { get; set; }
		[DataMember]
		public Nullable<int> T_OperatorID { get; set; }
		[DataMember]
		public Nullable<int> T_StoreID { get; set; }
		[DataMember]
		public Nullable<int> T_TerminalID { get; set; }
		[DataMember]
		public Nullable<int> T_RegisterID { get; set; }
		[DataMember]
		public Nullable<int> T_ShiftID { get; set; }
		[DataMember]
		public Nullable<int> T_CheckID { get; set; }
		[DataMember]
		public Nullable<int> T_CardID { get; set; }
		[DataMember]
		public Nullable<int> T_TOBox { get; set; }
		[DataMember]
		public string T_00TransNBText { get; set; }

		[DataMember]
		public List<KeyValuePair<int,double>> Payments { get; set; }
		[DataMember]
		public List<KeyValuePair<int, double>> Taxes { get; set; }
		[DataMember]
		public List<KeyValuePair<int, double>> ExNumbers { get; set; }
		[DataMember]
		public List<KeyValuePair<int, int>> ExStrings { get; set; }
		[DataMember]
		public List<Retail> Retails { get; set; }
		
	}
}
