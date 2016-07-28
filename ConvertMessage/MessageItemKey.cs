using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageItemKey)]
	public partial class MessageItemKey
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public int ID { get; set;}
	}

	[KnownType(typeof(MessageItemKey))]
	[DataContract(Namespace  = Consts.Empty, Name= Consts.str_MessageItemCAFullName)]
	public partial class MessageItemCAFullName : MessageItemKey
	{
		[DataMember]
		public string LastName{ get; set;}
	}
	
}
