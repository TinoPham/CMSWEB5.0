using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageResult)]
	public partial class MessageResult
	{
		[DataMember]
		public Commons.ERROR_CODE ErrorID { get; set;}
		[DataMember]
		public string Data{ get;set;}
		[IgnoreDataMember]
		public HttpStatusCode httpStatus{ get ;set;}
		
	}
}
