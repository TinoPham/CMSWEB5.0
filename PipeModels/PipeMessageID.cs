using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
namespace PipeModels
{

	
	public class MessageModel: Object
	{
		
		public string ClassModel{ get;set;}

		public string Data{ get;set;}
		public override string ToString()		{			return Newtonsoft.Json.JsonConvert.SerializeObject(this, JsonSettings.Instance.Settings);		}
	}

	public sealed class JsonSettings
	{
		private static readonly Lazy<JsonSettings> sInstance = new Lazy<JsonSettings>( () =>new JsonSettings());
		public static JsonSettings Instance { get { return sInstance.Value; } }
		public JsonSerializerSettings Settings{ get; private set;}
		private JsonSettings()
		{
			Settings = new JsonSerializerSettings{ DateTimeZoneHandling = DateTimeZoneHandling.Utc};
		}

	}
}
