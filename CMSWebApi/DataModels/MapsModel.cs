using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class MapsModel
	{
		public int siteKey { set; get; }
		public List<MapsImage> mapImage { set; get; }
	}

	public class MapsImage 
	{
		public int ImageID { set; get; }
		public string ImageURL { set; get; }
		public byte[] ImageByte { set; get; }
		public string Caption { set; get; }
		public byte Flag { set; get; }
		public List<ChannelsPosition> Channels;
		public string Title { get; set; }
		public DateTime UpdatedDate {get;set; }
		public int Createdby { get; set; }
 	}
	public class ChannelsPosition
	{
		public int ChannelID { set; get; }
		public float Leftpoint { set; get; }
		public float Toppoint { set; get; }
		public string ChannelName { set; get; }
		public byte Status { set; get; }
	}
}
