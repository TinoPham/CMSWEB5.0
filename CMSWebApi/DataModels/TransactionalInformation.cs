using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	
	public class TransactionalModel<T>: TransactionalInformation where T:class
	{
		public T Data{ get ;set;}

		public static TransactionalModel<T> CreateModel(T value)
		{
			TransactionalModel<T> ret = new TransactionalModel<T>();
			ret.Data = value;
			return ret;
		}
	}
	public class TransactionalInformation
    {
		 /// <summary>
		 /// true: Transaction complete with no error. The messages can return to client in ReturnMessage
		 /// false: Transaction had an error.
		 /// </summary>
		 [Newtonsoft.Json.JsonProperty(Required= Newtonsoft.Json.Required.AllowNull)]
		 public bool ReturnStatus { get; set; }
		  /// <summary>
		  /// List all messages that return to client
		  /// </summary>
		  [Newtonsoft.Json.JsonProperty(Required= Newtonsoft.Json.Required.AllowNull)]
		  public List<String> ReturnMessage { get; set; }
		  /// <summary>
		  /// List all key pair value error. 
		  /// </summary>
		  [Newtonsoft.Json.JsonProperty(Required= Newtonsoft.Json.Required.AllowNull)]
		  public Hashtable ValidationErrors;
		  [Newtonsoft.Json.JsonProperty(Required= Newtonsoft.Json.Required.AllowNull)]
		  public Boolean IsAuthenicated{ get; set;}

		  public TransactionalInformation()
		  {
			   ReturnMessage = new List<String>();
			   ReturnStatus = false;
			   ValidationErrors = new Hashtable();
			   IsAuthenicated = false;
		  }  
	}
}
