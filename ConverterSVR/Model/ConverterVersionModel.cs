using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace ConverterSVR.Model
{
	public class VersionModel
	{
		public const string STR_NAME = "Name";
		public const string STR_INSTALLNAME = "InstallName";
		public const string STR_DESCRIPTION = "Description";
		public const string STR_Platform = "Platform";
		
		public string Name { get ;set;}
		
		public string InstallName { get; set; }
		
		public string Description { get; set; }

		public string Platform{ get ;set;}

		public Version Version{ 
								get { 
										if(string.IsNullOrEmpty(Name))
											return new System.Version( Commons.ConstEnums.STR_NONE_VERSION);
										return Commons.Utils.String2Version(Name); }}

	}
}
