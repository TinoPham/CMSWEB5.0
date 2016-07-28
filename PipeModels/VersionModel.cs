using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeModels
{
	public class VersionModel : Object
	{
		public string OldVersion{ get;set;}
		public string NewVersion{ get ;set;}

		public virtual string ToString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this, JsonSettings.Instance.Settings);
		}
	}
}
