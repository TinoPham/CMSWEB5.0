using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter
{
	public class RawMsgVideo : RawDVRConfig<String>
	{
		string ImageData = string.Empty;

		public RawMsgVideo()
		{
		}

		public RawMsgVideo(string strMsg)
		{
			ImageData = strMsg;
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			byte[] rawData = Convert.FromBase64String(ImageData);

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
				DateTime imgTime = DateTime.ParseExact(VideoHeader.stime, MessageDefines.STR_IMG_DATE_FORMAT, CultureInfo.InvariantCulture);
				if (bufVideoData != null && bufVideoData.Length > 0)
				{
					string subFolder = string.Empty;
					if (DVRAdressBook != null && DVRAdressBook.KDVR > 0)
					{
						subFolder = DVRAdressBook.KDVR.ToString();
					}
					else
					{
						//DVRAdressBook = db.FirstOrDefault<tDVRAddressBook>(item => item.KDVR == dvrinfo.KDVR);
					}
					i3Image iSnapshot = new i3Image(bufVideoData);
					string fileName = string.Format("C_{0:00}.jpg", VideoHeader.param + 1);//, VideoHeader.time);AppDomain.CurrentDomain.GetData("DataDirectory").ToString()
					string sPath = System.IO.Path.Combine(AppSettings.AppSettings.Instance.AppData, MessageDefines.PATH_RAW_DVRIMAGE, subFolder, fileName);//VideoHeader.guid
					iSnapshot.SaveImage(sPath, true, DateTime.MinValue, 2);

					fileName = string.Format(MessageDefines.IMAGE_FILENAME, VideoHeader.param + 1, VideoHeader.time);//, VideoHeader.time);AppDomain.CurrentDomain.GetData("DataDirectory").ToString()
					sPath = System.IO.Path.Combine(AppSettings.AppSettings.Instance.AppData, MessageDefines.PATH_ALERT_IMAGE, subFolder, fileName);//VideoHeader.guid
					iSnapshot.SaveImage(sPath, false, imgTime, 0, 0, 0);
				}
			}
			catch (Exception) { }

			return await base.UpdateToDB();
		}
	}
}
