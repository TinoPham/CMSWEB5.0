using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMessage
{
	[DataContract(Namespace = Consts.Empty, Name = Consts.str_MessageData)]
	public partial class MessageData : ICloneable
	{
		/// <summary>
		/// program-set for message
		/// </summary>
		[DataMember]
		public Commons.Programset Programset { get; set;}
		/// <summary>
		/// identity value for message data. because Data is string and we don't know what's object that data contain
		/// </summary>
		[DataMember]
		public string Mapping{ get; set;}
		/// <summary>
		/// Whole object data will be convert to string before sending to server
		/// </summary>
		/// 
		[DataMember]
		public string Data{ get; set;}

		public object Clone()
		{
			return new MessageData{ Data = this.Data, Programset = this.Programset, Mapping = this.Mapping};
		}
	}
}
