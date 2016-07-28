using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Xml.Serialization;

namespace CMSWebApi.DataModels.DashBoardCache
{
	[ProtoContract]
	[XmlType(TypeName = "Fact_IOPC_Periodic_Hourly_Traffic")]
	[JsonObject(Title="Fact_IOPC_Periodic_Hourly_Traffic")]
	public class IOPCCountPeriodicCacheModel:CacheModelBase
	{
		[ProtoMember(1)]
		[XmlAttribute(AttributeName = "PACID")]
		[JsonProperty(PropertyName = "PACID")]
		public int PACID{ get ;set;}

		int _DVRDate;
		[ProtoMember(2)]
		[JsonIgnore]
		[XmlIgnoreAttribute]
		public int DVRDate { get{ return _DVRDate;} set{ _DVRDate = value; _DVRDateHour = 0;} }

		byte _C_Hour;
		[ProtoMember(3)]
		[JsonProperty(PropertyName = "C_Hour")]
		[XmlAttribute(AttributeName = "C_Hour")]
		public byte C_Hour { get{ return _C_Hour;} set{ _C_Hour = value; _DVRDateHour = 0;} }

		[ProtoMember(4)]
		[JsonProperty(PropertyName = "CameraID")]
		[XmlAttribute(AttributeName = "CameraID")]
		public int CameraID { get; set; }

		[ProtoMember(5)]
		[JsonProperty(PropertyName = "Count_IN")]
		[XmlElement(ElementName = "Count_IN", IsNullable=true)]
		public short? In { get; set; }

		[ProtoMember(6)]
		[JsonProperty(PropertyName="Count_OUT")]
		[XmlElement(ElementName = "Count_OUT", IsNullable= true)]
		public short? Out { get; set; }

		[ProtoMember(7)]
		[JsonProperty(PropertyName="Count_IN_N")]
		[XmlElement(ElementName = "Count_IN_N", IsNullable= true)]
		public short? InN { get; set; }


		[ProtoMember(8)]
		[JsonProperty(PropertyName = "Count_OUT_N")]
		[XmlElement(ElementName = "Count_OUT_N", IsNullable= true)]
		public short? OutN { get; set; }

		[ProtoMember(9)]
		[JsonIgnore]
		[XmlIgnoreAttribute]
		public byte R_N {
			get { return _Report_Normalize; } 
			set {
					_Report_Normalize = value; 
					SetNormalize(value);
				} }

		private bool _Normalize  = false;
		[JsonProperty(PropertyName = "Normalize")]
		[XmlAttribute(AttributeName = "Normalize")]
		public bool Normalize{ get { return _Normalize;}}

		[JsonProperty(PropertyName = "ReportNormalize")]
		[XmlAttribute(AttributeName = "ReportNormalize")]
		public bool ReportNormalize{ get { return _rpt_Normalize;}}

		private bool _rpt_Normalize = false;

		private byte _Report_Normalize = 0;

		int _DVRDateHour = 0;

		[JsonIgnore]
		[XmlIgnoreAttribute]
		public int DVRDateHour { get
		{
			if(_DVRDateHour == 0)
			{
				Int64 h = DVRDate;
				_DVRDateHour = h.unixTime_ToDateTime().ToUnixTimestamp(C_Hour);
			}

			return _DVRDateHour;
		}
		 }


		private void SetNormalize( byte val)
		{
			_Normalize = IsFlagValue(val, NormalizeFlag.Normalize);
			_rpt_Normalize = IsFlagValue( val, NormalizeFlag.Report_Normalize);
		}
	
		public void SetNormalize(bool isnormailize, bool isRptNormalize)
		{
			_Normalize = isnormailize;
			_rpt_Normalize = isRptNormalize;

			_Report_Normalize = 0;
			_Report_Normalize |= isnormailize ? (byte)NormalizeFlag.Normalize : (byte)0;
			_Report_Normalize |= isRptNormalize ? (byte)NormalizeFlag.Report_Normalize : (byte)0;

		}
		[JsonIgnore]
		[XmlIgnore]
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
		[JsonProperty(PropertyName = "DVRDateKey")]
		[XmlAttribute(AttributeName = "DVRDateKey")]
		public DateTime DVRDateKey { get { return DVRDate.unixTime_ToDateTime(); } set{}}
		/// <summary>
		/// Waring:
		///Property only for Export data purpose. Please don't use for coding
		/// </summary>
		[JsonProperty(PropertyName = "DVRDateHourKey")]
		[XmlAttribute(AttributeName = "DVRDateHourKey")]
		public DateTime DVRDateHourKey { get { return DVRDate.unixTime_ToDateTime((int)C_Hour); } set{}}
		
	}
}
