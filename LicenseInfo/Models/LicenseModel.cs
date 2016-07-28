using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenseInfo.Models
{
	public class LicenseModel
	{
		static readonly string [] Default_Module_Name = { "Sites", "Bam", "ReBar", "CMSReports", "Configurations", "IncidentReports", "Dashboard" };
		public List<CMSWebModule> CMSWebModules { get; set; }
		public int DVRNumber{ get;set;}
		public static LicenseModel Default{ get { return DefaultLicenseModel();}}
		private static LicenseModel DefaultLicenseModel()
		{
			LicenseModel model = new LicenseModel();
			model.DVRNumber = 50;
			model.CMSWebModules = new List<CMSWebModule>();
			foreach (string name in Default_Module_Name)
				model.CMSWebModules.Add( new CMSWebModule{ Enable = false, Name = name, From = DateTime.Now, To = DateTime.Now});
			return model;

		}
	}

	public class CMSWebModule
	{
		public string Name{ get;set;}
		public bool Enable{ get;set;}
		public DateTime From{ get; set;}
		public DateTime To{ get; set;}
	}
}
