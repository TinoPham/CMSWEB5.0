using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Extensions;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace CMSWebApi.DataModels.DashBoardCache
{
	[ProtoContract]
	[XmlType(TypeName = "Fact_POS_Periodic_Hourly_Transact")]
	[JsonObject(Title = "Fact_POS_Periodic_Hourly_Transact")]
	public class POSPeriodicCacheModel : CacheModelBase
	{
		[ProtoMember(1)]
		[XmlAttribute(AttributeName = "PACID")]
		[JsonProperty(PropertyName = "PACID")]
		public int PACID { get; set; }

		[ProtoMember(2)]
		[XmlElement(ElementName = "Count_Trans", IsNullable= true)]
		[JsonProperty(PropertyName = "Count_Trans")]
		public ushort? TotalTrans { get; set; }

		[ProtoMember(3)]
		[XmlElement(ElementName = "TotalAmount", IsNullable= true)]
		[JsonProperty(PropertyName = "TotalAmount")]
		public decimal? TotalAmount { get; set; }

		[ProtoMember(4)]
		[XmlElement(ElementName = "Count_Trans_N", IsNullable=true)]
		[JsonProperty(PropertyName = "Count_Trans_N")]
		public ushort? NTotalTrans { get; set; }

		[ProtoMember(5)]
		[XmlElement(ElementName = "TotalAmount_N", IsNullable=true)]
		[JsonProperty(PropertyName = "TotalAmount_N")]
		public decimal? NTotalAmount { get; set; }

		byte _Hour;
		[ProtoMember(6)]
		[XmlAttribute(AttributeName = "TransHour")]
		[JsonProperty(PropertyName = "TransHour")]
		public byte Hour { get{ return _Hour;} set{ _Hour = value; _DVRDateHour = 0;} }

		int _DVRDate;
		[ProtoMember(7)]
		[JsonIgnore]
		[XmlIgnore]
		public int DVRDate { get{ return _DVRDate;} set{ _DVRDate = value; _DVRDateHour = 0;} }

		volatile int _DVRDateHour = 0;
		[JsonIgnore]
		[XmlIgnore]
		public int DVRDateHour
		{
			get
			{
				if (_DVRDateHour == 0)
				{
					Int64 h = DVRDate;
					_DVRDateHour = h.unixTime_ToDateTime().ToUnixTimestamp(Hour);
				}

				return _DVRDateHour;
			}
		}

		[ProtoMember(8)]
		[JsonIgnore]
		[XmlIgnore]
		public byte R_N
		{
			get { return _Report_Normalize; }
			set
			{
				_Report_Normalize = value;
				SetNormalize(value);
			}
		}

		private bool _Normalize = false;
		[XmlAttribute(AttributeName = "Normalize")]
		[JsonProperty(PropertyName = "Normalize")]
		public bool Normalize{ get { return _Normalize;}}

		[XmlAttribute(AttributeName = "ReportNormalize")]
		[JsonProperty(PropertyName = "ReportNormalize")]
		public bool ReportNormalize{ get { return _rpt_Normalize;}}

		private bool _rpt_Normalize = false;

		private byte _Report_Normalize = 0;


		private void SetNormalize(byte val)
		{
			_Normalize = IsFlagValue(val, NormalizeFlag.Normalize);
			_rpt_Normalize = IsFlagValue(val, NormalizeFlag.Report_Normalize);
		}

		public void SetNormalize(bool isnormailize, bool isRptNormalize)
		{
			_Normalize = isnormailize;
			_rpt_Normalize = isRptNormalize;

			_Report_Normalize = 0;
			_Report_Normalize |= isnormailize ? (byte)NormalizeFlag.Normalize : (byte)0;
			_Report_Normalize |= isRptNormalize ? (byte)NormalizeFlag.Report_Normalize : (byte)0;

		}
		[XmlIgnore]
		[JsonIgnore]
		public override int ItemTime
		{
			get
			{
				return DVRDate;
			}
		}

		/// <summary>
		/// Waring:
		///Property only for Export data purpose. Please don't use for coding
		/// </summary>
		[XmlAttribute(AttributeName = "DVRDateKey")]
		[JsonProperty(PropertyName = "DVRDateKey")]
		public DateTime DVRDateKey { get { return DVRDate.unixTime_ToDateTime(); } set{}}
		/// <summary>
		/// Waring:
		///Property only for Export data purpose. Please don't use for coding
		/// </summary>
		[XmlAttribute(AttributeName = "DVRDateHourKey")]
		[JsonProperty(PropertyName = "DVRDateHourKey")]
		public DateTime DVRDateHourKey { get { return DVRDate.unixTime_ToDateTime((int)Hour); } set{ }}
	}
	
}
