using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;
using System.Runtime.Serialization;

namespace CMSWebApi.DataModels
{
	
	public class ApplicationMenu 
	{
		
		public int ID{ get ;set;}
		/// <summary>
		/// Name of html view
		/// </summary>
		public string Name{ get;set;}
		/// <summary>
		/// angular ui state name
		/// </summary>
		public string State{ get;set;}
		/// <summary>
		/// url path to view. resolve on client by syntax: Url/Name/name.html
		/// </summary>
		private string _url;

		public string Url{ get{ return _url; }set { _url = value == null? null : value.ToLowerInvariant();}}
		/// <summary>
		/// view template <div ng-view=''></div>
		/// </summary>
		public string UrlTemplate{get;set;}
		//title translate string
		public string Translate{get;set;}
		/// <summary>
		///is root view
		/// </summary>
		public bool Abstract{ get;set;}

		/// <summary>
		/// group key
		/// </summary>
		public string Groupkey { get; set; }

		public string Params { get; set; }

		public string OptionId { get; set; }

		public string Classstyle { get; set; }

		public bool? Menu{ get ;set;}
		public bool isResource { get; set;}
		public bool isAdmin { get; set; }
		/// <summary>
		/// List all childs view 
		/// </summary>
		public List<ApplicationMenu>childs{ get ;set;}

		//public ApplicationMenu Clone()
		//{
		//	ApplicationMenu obj = new ApplicationMenu{ Abstract = this.Abstract, ID = this.ID, Menulevel = this.Menulevel, Name = this.Name, Sate = this.Sate, Translate = this.Translate, Url = this.Url, UrlTemplate = this.UrlTemplate};
		//	obj.childs = new List<ApplicationMenu>();
		//	if (this.childs != null && this.childs.Count > 0)
		//		this.childs.ForEach(item => obj.childs.Add(item.Clone()));
		//	return obj;
		//}
	}
	
}
