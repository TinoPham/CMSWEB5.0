using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageAgreement)]
	public class MessageAgreement
	{
		[DataMember]
		public Customer Customer { get; set; }
		[DataMember]
		public MessageDVRInfo DVR { get; set; }
	}

	[DataContract(Namespace = Consts.Empty, Name = Consts.str_DomainAccept)]
	public class DomainResponse
	{
		[DataMember]
		public string Url{ get ;set;}
		[DataMember]
		public bool isAccept{ get ;set;}
		[DataMember]
		public long UIChange{ get ;set;}
		[DataMember]
		public long TechChange{get ;set;}
	}
}
