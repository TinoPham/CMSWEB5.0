using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;

namespace CMSWebApi.APIFilters._3rdToken
{
	
	public class _3rdInfo
	{
		//Application ID
		public string AppID { get; set; }

		public string SID { get;set;}
		//Name of application: It must be formatted: Name: version number
		public string Name { get; set; }

		public string HMAC { get; set;}

		public bool Encrypted { get; set;}

	}
	
	public class _3rdConfig
	{
		//Application ID
		public string AppID{ get ;set;}
		//Name of application
		public string Name{ get;set;}
		//max connection allow on application id
		public int MaxConnection{ get ;set;}
		//Number request per minutes
		public int RequestPerMin{ get ;set;}
		//Alow request
		public List<string> Allow{ get; private set;}
		//denied request
		public List<string>Denied{ get; private set;}
	}
}
