using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using ConvertMessage;
using Newtonsoft.Json;

namespace CMSSVR.Infrastructure
{
	public class KDVRToken : IIdentity
	{
		public string ActiveMacAddress { get; set; }
		public int KDVR { get; set; }
		public string HASKeyID { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime EndDate { get; set; }
		public Int64 ID { get; set; }
		public string ServerID{ get; set;}


		[JsonIgnore]
		public string Name
		{
			get { return ActiveMacAddress; }
		}

		[JsonIgnore]
		public string AuthenticationType
		{
			get { return "Basic"; }
		}
		[JsonIgnore]
		public bool IsAuthenticated
		{
			get { return true; }
		}

		public static KDVRToken ToDVRToken(string encrypt, string encKey, bool useHashing = true)
		{
			string jsonString = Cryptography.MACSHA256.Decrypt(encrypt, encKey);
			return JsonConvert.DeserializeObject<KDVRToken>(jsonString);
		}

		public static string ToTokenString(KDVRToken dvrtoken, string encKey, bool useHashing = true)
		{
			string json = JsonConvert.SerializeObject(dvrtoken);
			return Cryptography.MACSHA256.Encrypt(json, encKey);
		}
		public string ToString(string encKey, bool useHashing = true)
		{
			return ToTokenString(this, encKey, useHashing);
		}

		public ConvertMessage.MessageDVRInfo SinpleDVRMessageInfo()
		{
			return new MessageDVRInfo { KDVR = this.KDVR, HASPK = this.HASKeyID, Date = this.CreateDate };
		}



	}
}