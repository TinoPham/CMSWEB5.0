using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageDVRInfo)]
	public partial class MessageDVRInfo
	{
		[DataMember]
		public int KDVR { get; set; }

		[DataMember]
		public List<MacInfo> MACs { get; set; }

		[DataMember]
		public PACInfo PACinfo{ get ;set;}

		[DataMember]
		public string HASPK { get; set; }

		DateTime _date;

		[DataMember]
		public DateTime Date { get { return _date ; } 
			set{ 
				if( value.Kind == DateTimeKind.Utc)
					_date = value;
				else 
					_date = new DateTime(value.Ticks, DateTimeKind.Utc);
					
			} }

		[DataMember]
		public string HostName
		{
			get; 
			set;
		}
	}

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MacInfo)]
	public partial class MacInfo
	{
		[DataMember]
		public string MAC_Address{ get; set;}

		[DataMember]
		public int MacOrder{ get; set;}

		[DataMember]
		public string IP_Address { get; set;}

		[DataMember]
		public string IP_Version { get; set;}

		[DataMember]
		public bool Active{ get; set;}

		[DataMember]
		public string Description { get; set; }
	}

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_PACInfo)]
	public partial class PACInfo
	{
		[DataMember]
		public string PACID{ get; set;}
		[DataMember]
		public string PACVersion{ get; set;}
	}

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_Customer)]
	public class Customer : IComparable
	{
		[DataMember]
		public string Name{ get;set;}
		[DataMember]
		public string Email { get ;set;}
		[DataMember]
		public string Phone{ get;set;}
		[JsonIgnore]
		public string Domain{ get ;set;}
		[JsonIgnore]
		public UInt16 ConverterPort{ get ;set;}
		[JsonIgnore]
		public bool AllowConnect{ get ;set;}
		public int CompareTo(object obj)
		{
			Customer info = obj as Customer;
			if (info == null)
				return 1;
			bool is_equal = string.Compare(Name, info.Name, false) == 0 && string.Compare(Email, info.Email, true) == 0 && string.Compare(Phone, info.Phone, true) == 0 && string.Compare(Domain,info.Domain, true) == 0 && ConverterPort == info.ConverterPort && AllowConnect == info.AllowConnect;
			return is_equal? 0: 1;
			
		}
	}

}
