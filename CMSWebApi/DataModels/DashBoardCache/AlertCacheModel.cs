using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Extensions;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace CMSWebApi.DataModels.DashBoardCache
{
	[ProtoContract]
	[XmlType(TypeName = "tAlertEvent")]
	[JsonObject(Title = "tAlertEvent")]
	public class AlertCacheModel : CacheModelBase
	{
		[ProtoMember(1)]
		[XmlAttribute(AttributeName = "KAlertEvent")]
		[JsonProperty(PropertyName = "KAlertEvent")]
		public int KAlertEvent{ get ;set;}

		[ProtoMember(2)]
		[XmlAttribute(AttributeName = "KAlertType")]
		[JsonProperty(PropertyName = "KAlertType")]
		public byte KAlertType {get; set;}

		[ProtoMember(3)]
		[XmlAttribute(AttributeName = "KDVR")]
		[JsonProperty(PropertyName = "KDVR")]
		public int KDVR{ get ;set;}

		[ProtoMember(4)]
		[JsonIgnore]
		[XmlIgnore]
		public int Time{ get ;set;}

		[ProtoMember(5)]
		[JsonIgnore]
		[XmlIgnore]
		public byte KAlertSeverity{ get ;set; }

		[XmlIgnore]
		[JsonIgnore]
		public override int ItemTime
		{
			get
			{
				return Time;
			}
		
		}

		/// <summary>
		/// Waring:
		///Property only for Export data purpose. Please don't use for coding
		/// </summary>
		[XmlAttribute(AttributeName = "Time")]
		[JsonProperty(PropertyName = "Time")]
		public DateTime DateTime { get { return Time.unixTime_ToDateTime(); } set{}}

	}
}
