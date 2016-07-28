using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CMSWebApi.Utils;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	class RawMsgHeatmapImage : RawDVRConfig<HeatmapImageBody>
	{
		#region Parameter
		public const string STR_Image = "image";
		public const string STR_Schedule = "schedule";
		public const string STR_Channel = "channel";
		public const string STR_StartTime = "start";
		public const string STR_EndTime = "end";
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			//string subFolder = HM_SCHEDULE_TYPE.Daily.ToString();
			byte[] rawData = Convert.FromBase64String(msgBody.imgBuffer);
			string sError = string.Empty;
			try
			{
				int VideoHeaderLength = Marshal.SizeOf(typeof(MessageDefines.VideoHeader));
				byte[] vhBuffer = new byte[VideoHeaderLength];
				Buffer.BlockCopy(rawData, 0, vhBuffer, 0, VideoHeaderLength);
				GCHandle hndHeader = GCHandle.Alloc(vhBuffer, GCHandleType.Pinned);
				MessageDefines.VideoHeader VideoHeader = (MessageDefines.VideoHeader)Marshal.PtrToStructure(hndHeader.AddrOfPinnedObject(), typeof(MessageDefines.VideoHeader));
				hndHeader.Free();

				int nDataLen = rawData.Length - VideoHeaderLength;
				byte[] bufVideoData = new byte[nDataLen];
				Buffer.BlockCopy(rawData, VideoHeaderLength, bufVideoData, 0, nDataLen);
				//DateTime imgTime = DateTime.ParseExact(VideoHeader.stime, MessageDefines.STR_IMG_DATE_FORMAT, CultureInfo.InvariantCulture);
				if (bufVideoData != null && bufVideoData.Length > 0)
				{
					HM_SCHEDULE_TYPE stype = (HM_SCHEDULE_TYPE)msgBody.iSchedule;
					string subFolder = String.Format(@"{0}\{1}", DVRAdressBook.KDVR, stype.ToString());

					string fileName = string.Format(MessageDefines.IMAGE_FILENAME, VideoHeader.param + 1, VideoHeader.time);
					string sPath = System.IO.Path.Combine(AppSettings.AppSettings.Instance.AppData, MessageDefines.PATH_HEATMAP_IMAGE, subFolder, fileName);

					i3Image iSnapshot = new i3Image(bufVideoData);
					sError = iSnapshot.SaveImage(sPath, false, DateTime.MinValue, 0, 0, 0);

					if (String.IsNullOrEmpty(sError))
					{
						tDVRChannels channel = db.FirstOrDefault<tDVRChannels>(x => x.KDVR == DVRAdressBook.KDVR && x.ChannelNo == VideoHeader.param);
						var includes = new string[]
						{
							//typeof (tbl_HM_ScheduledTasks).Name,
							//string.Format("{0}.{1}", typeof (tbl_HM_ScheduledTasks).Name, typeof (tbl_HM_TaskChannel).Name)
						};
						tbl_HM_TaskChannel taskChannel = db.FirstOrDefault<tbl_HM_TaskChannel>(x => x.KChannel == channel.KChannel && x.tbl_HM_ScheduledTasks.ScheduleType == (byte)stype, null);//includes

						tbl_HM_Images dbImage = new tbl_HM_Images() 
						{
							ImgName = fileName,
							ProcessID = taskChannel.ProcessID,
							UploadedBy = null,
							UploadedDate = null
						};
						db.Insert<tbl_HM_Images>(dbImage);
						db.Save();
					}
				} //if
			}
			catch(Exception)
			{}
			return await base.UpdateToDB();
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class HeatmapImageBody
	{
		//[XmlElement(MessageDefines.STR_Common)]
		//public RawMsgCommon msgCommon { get; set; }
		[XmlElement(RawKeepAlive.STR_DVRTime)]
		public DVRTimeStamp tsDVRTime { get; set; }

		[XmlElement(RawMsgHeatmapImage.STR_Schedule)]
		public int iSchedule { get; set; }

		[XmlElement(RawMsgHeatmapImage.STR_Channel)]
		public int iChannel { get; set; }

		[XmlElement(RawMsgHeatmapImage.STR_StartTime)]
		public string sStartTime { get; set; }
		public DateTime? StartTime
		{
			get
			{
				if (String.IsNullOrEmpty(sStartTime)) return null;
				return DateTime.ParseExact(sStartTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(RawMsgHeatmapImage.STR_EndTime)]
		public string sEndTime { get; set; }
		public DateTime? EndTime
		{
			get
			{
				if (String.IsNullOrEmpty(sEndTime)) return null;
				return DateTime.ParseExact(sEndTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(RawMsgHeatmapImage.STR_Image)]
		public string imgBuffer { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}
	#endregion
}
