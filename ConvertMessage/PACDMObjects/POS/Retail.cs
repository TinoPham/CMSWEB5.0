using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage.PACDMObjects.POS
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_Retail)]
	public partial class Retail
	{
		[DataMember]
		public string RetailKey { get; set; }
		[DataMember]
		public Nullable<int> R_2ItemLineNb { get; set; }
		[DataMember]
		public Nullable<double> R_1Qty { get; set; }
		[DataMember]
		public Nullable<decimal> R_0Amount { get; set; }
		[DataMember]
		public Nullable<int> R_Description { get; set; }
		[DataMember]
		public Nullable<int> R_ItemCode { get; set; }

		Nullable<System.DateTime> date;
		[DataMember]
		public Nullable<System.DateTime> R_DVRDate 
		{ 
				get{ return date; } 
				set { 
						if( value.HasValue && value.Value.Kind != DateTimeKind.Utc)
							date = new Nullable<DateTime>( new DateTime( value.Value.Ticks, DateTimeKind.Utc));

						else date = value;
					}
		}

		[DataMember]
		public List<SubRetail> SubRetails { get; set; }
		[DataMember]
		public List<KeyValuePair<int, double>> ExNumbers { get; set; }
		[DataMember]
		public List<KeyValuePair<int, int>> ExStrings { get; set; }
		[DataMember]
		public Nullable<int> R_TOBox { get; set; }
	}
}
