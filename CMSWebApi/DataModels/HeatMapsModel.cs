using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Utils;

namespace CMSWebApi.DataModels
{
	public class HeatMapsModel
	{
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
		public int KDVR { set; get; }
        public List<HeatMapsImage> mapsImage { set; get; }
	}

	public class HeatMapsImage
	{
		public long ImageID { set; get; }
		public string ImageURL { set; get; }
		public byte[] ImageByte { set; get; }
		public string ImgName { set; get; }
        public Channel Channels;
		public string Title { get; set; }
		public DateTime? UpdatedDate {get;set; }
        public string paramUpdatedDate { get; set; }
		public int? Createdby { get; set; }
        public ScheduleTypes schedule { get; set; }
 	}
	public class Channel
	{
		public int ChannelID { set; get; }
		public string ChannelName { set; get; }
        public int ChannelNo { set; get; }
	}

    public class ScheduleTypes
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
    }

    public class ScheduleTasks
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string paramStartTime { get; set; }
        public string paramEndTime { get; set; }
        public string paramStartDate { get; set; }
        public string paramEndDate { get; set; }
        public int Status { get; set; }
        public int CountSites { get; set; }
        public int CountDvrs { get; set; }
        public IEnumerable<Channel> Channels { get; set; }
        public IEnumerable<int> Dvrs { get; set; }
        public int scheduleType { get; set; }
    }

}
