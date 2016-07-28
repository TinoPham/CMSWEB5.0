using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public partial class FileModel
	{
		public string Path { get; set; }
		public string ExFile { get; set; }
		public byte[] Data { get; set; }
		public string Name { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime Modified { get; set; }
		public long Size { get; set; }
		public int UserId { get; set; }
	}

	public class SiteFileModel
	{
		CmsSites Model { get; set; }
		FileModel Files { get; set; }
	}
	public class FileSiteModel
	{
		public string MAC{ get; set;}
		public int KDVR { get; set; }
		public int SiteKey{ get ;set;}
		public string Name{ get;set;}
		public byte [] Data { get; set; }
		public long Size { get; set; }
		public bool hasData{ get;set;}
	}

	public class ImageModel
	{
		public string Name { get; set; }
		public byte[] Data { get; set; }
		public bool hasData { get; set; }
	}
}
