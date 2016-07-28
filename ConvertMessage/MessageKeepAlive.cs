using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageKeepAlive)]
	public class MessageKeepAlive : System.Object
	{
		/// <summary>
		/// last converter version is available on server.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string LastVersion { get; set; }

		/// <summary>
		/// server identify
		/// </summary>
		[DataMember(IsRequired = true)]
		public string ServerID{ get;set;}

		/// <summary>
		///Time out when client send request to server
		/// </summary>
		[DataMember(IsRequired = true)]
		public int? TimeOut{ get ;set;}

		/// <summary>
		///interval to send keep alive message to server. default is 5 minutes
		/// </summary>
		[DataMember(IsRequired = true)]
		public int? KeepAliveInterval { get; set; }

		/// <summary>
		/// reset local data for pac converter
		/// </summary>
		[DataMember(IsRequired = false)]
		public bool? DataReset{ get ;set;}

		/// <summary>
		/// time interval when converting POS data. default is 10 minutes
		/// </summary>
		[DataMember(IsRequired = false)]
		public short? ConverterInterval{ get;set;}

		/// <summary>
		///Number of days of last log in local db
		/// </summary>
		[DataMember(IsRequired = false)]
		public short? LogRecyle{ get;set;}

		/// <summary>
		///Number of days of last DVR message when server/Network failed. default 2 days
		/// </summary>
		[DataMember(IsRequired = false)]
		public short? DVRMessageRecycle { get; set; }

		[DataMember(IsRequired = true)]
		public bool? DVRConvert { get; set; }
		
		[DataMember(IsRequired= false)]
		public ConvertInfoConfig[] ConvertInfo { get; set; }

		[DataMember(IsRequired = false)]
		public Int64? KeepAliveToken { get; set; }

		public static MessageKeepAlive Default()
		{
			return new MessageKeepAlive{
				LastVersion = null,
				ServerID = null,
				TimeOut = Consts.Http_Default_Time_Out,//2 mins
				KeepAliveInterval = Consts.Http_Default_KeepAlive,// 5 mins
				DataReset = false,
				ConverterInterval = Consts.Http_Default_Convert_Interval,//10 mins
				LogRecyle = Consts.Default_LogRecyle,// days
				DVRMessageRecycle = Consts.DVR_Message_Recyle,// 2days
				DVRConvert = null,
				ConvertInfo = null
			};
		}

		public static MessageKeepAlive FromJsonString(string json)
		{
			if( string.IsNullOrEmpty(json))
				return null;
			try
			{
				return Newtonsoft.Json.JsonConvert.DeserializeObject<MessageKeepAlive>( json, new Newtonsoft.Json.JsonSerializerSettings{ DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc, NullValueHandling = Newtonsoft.Json.NullValueHandling.Include});
			}
			catch(Exception){ return null;}

		}
		
		public override bool Equals(object obj)
		{
			if( obj == null || !(obj is MessageKeepAlive) )
				return false;
			MessageKeepAlive mobj = obj as MessageKeepAlive;

			if(mobj.ConverterInterval != ConverterInterval)
				return false;
			if( mobj.DataReset != DataReset)
				return false;
			if( mobj.DVRMessageRecycle != DVRMessageRecycle)
				return false;
			if( mobj.KeepAliveInterval != KeepAliveInterval)
				return false;
			if(string.Compare(mobj.LastVersion,LastVersion, true) != 0)
				return false;
			if (string.Compare(mobj.ServerID, ServerID, true) != 0)
				return false;
			if( mobj.TimeOut != TimeOut)
				return false;

			return true;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public MessageKeepAlive Clone()
		{
			return base.MemberwiseClone() as MessageKeepAlive;
		}
		public string toJsonString( bool istoken = false)
		{
			MessageKeepAlive data = this.MemberwiseClone() as MessageKeepAlive;
			if( data != null)
				data.KeepAliveToken = (Int64?)null;
			return Newtonsoft.Json.JsonConvert.SerializeObject( data == null? this : data);
		}
	}

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_ConvertInfo)]
	public class ConvertInfoConfig
	{
		[DataMember(IsRequired = true)]
		public byte Programset { get; set; }

		[DataMember(IsRequired = true)]
		public string TableName { get; set; }

		[DataMember(IsRequired = true)]
		[JsonConverter(typeof(YearMonthDayJsonDateConverter))]
		public Nullable<DateTime> DvrDate { get; set; }

		[DataMember(IsRequired = true)]
		public string LastKey { get; set; }

		[DataMember(IsRequired = true)]
		public bool Enable { get; set; }
		class YearMonthDayJsonDateConverter : IsoDateTimeConverter
		{
			public YearMonthDayJsonDateConverter()
			{
				DateTimeFormat = "yyyyMMdd";
			}
		}
	}
}
