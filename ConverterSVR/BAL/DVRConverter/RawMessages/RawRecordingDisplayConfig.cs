using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawRecordingDisplayConfig : RawDVRConfig<RawRecordingDisplayBody>
	{
		#region Parameter
		public const string STR_RecordingDisplay = "recording_display";
		public const string STR_AD4016Encode = "ad4016_encode";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_RecordFrame = "record_frame";
		public const string STR_ResolutionWidth = "resolution_width";
		public const string STR_ResolutionHeight = "resolution_height";
		public const string STR_Emergency = "emergency";
		public const string STR_Resolutions = "resolutions";
		public const string STR_Resolution = "resolution";
		public const string STR_TotalFrame = "total_frame";
		public const string STR_Width = "width";
		public const string STR_Height = "height";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawRecordingDisplayBody msgBody { get; set; }
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{

			if (DVRAdressBook == null || msgBody.rdData== null)
				return await base.UpdateToDB();

			if (UpdateRecordDisplayConfigs(DVRAdressBook, msgBody.rdData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			//UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_RECORDING, msgBody.msgCommon.Checksum, msgBody.msgCommon.TimeChange, msgBody.msgCommon.dtDVRTime); //AAAA
			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_RECORDING, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateRecordDisplayConfigs(tDVRAddressBook dvrAdressBook, RecordingDisplayData rdData)
		{
			bool ret = false;
			if (msgBody.rdData.AD4016Encode >= 0)
			{
				db.Include<tDVRAddressBook, tDVRHardware>(dvrAdressBook, item => item.tDVRHardware);
				tDVRHardware hwDvr = db.FirstOrDefault<tDVRHardware>(item => item.KDVR == DVRAdressBook.KDVR);
				if (hwDvr != null)
				{
					if (hwDvr.CCEncode != rdData.AD4016Encode)
					{
						hwDvr.CCEncode = rdData.AD4016Encode;
						db.Update<tDVRHardware>(hwDvr);
						ret = true;
					}
				}
			}

			ret |= SetDvrResolutions(DVRAdressBook, rdData);

			ret |= SetDvrVideoSource(DVRAdressBook, rdData);

			return ret;
		}

		private bool SetDvrVideoSource(tDVRAddressBook dvr, RecordingDisplayData rdData)
		{
			if (msgBody.rdData.RDChannels == null) return false;
			db.Include<tDVRAddressBook, tDVRVideoSource>(dvr, item => item.tDVRVideoSources);
			bool ret = false;
			bool highVersion = HighVersion();

			if (highVersion)
			{
				ret |= SetHightVersionVideoSource(dvr, rdData.RDChannels);
			}
			else
			{
				ret |= SetLowVersionVideoSource(dvr, rdData.RDChannels);
			}
			return ret;
		}

		private bool SetLowVersionVideoSource(tDVRAddressBook dvrAddressBook, List<RDChannelInfo> rdChannelInfoList)
		{
			bool ret = false;
			var kVideoFormat = db.Query<tDVRHardware>(t => t.KDVR == dvrAddressBook.KDVR).Select(t => t.KVideoFormat).FirstOrDefault();
			foreach (var vi in rdChannelInfoList)
			{
				var kresolution =
					db.Query<tResolutionDef>(t => t.KVideoFormat == kVideoFormat && t.ResolutionNo == vi.id)
						.Select(t => t.KResolution)
						.FirstOrDefault();
				tDVRVideoSource videoSource = db.Query<tDVRVideoSource>(t => t.KDVR == dvrAddressBook.KDVR && t.VideoSourceNo == vi.id).FirstOrDefault();
				if (videoSource == null) continue;
				if (videoSource.Emergency != vi.Emergency || videoSource.KResolution != kresolution)
				{
					videoSource.Emergency = vi.Emergency;
					videoSource.KResolution = kresolution;
					db.Update<tDVRVideoSource>(videoSource);
					ret = true;
				}
			}
			return ret;
		}

		private bool SetHightVersionVideoSource(tDVRAddressBook dvrAddressBook, List<RDChannelInfo> rdChannelInfoList)
		{
			bool ret = false;
			foreach (var vi in rdChannelInfoList)
			{
				tDVRVideoSource videoSource = db.Query<tDVRVideoSource>(t => t.KDVR == dvrAddressBook.KDVR && t.VideoSourceNo == vi.id).FirstOrDefault();
				if (videoSource == null) continue;
				if (videoSource.Emergency != vi.Emergency ||videoSource.ResolutionWidth != vi.ResolutionWidth ||videoSource.ResolutionHeight != vi.ResolutionHeight || videoSource.FPS != vi.RecordFrame)
				{
					videoSource.FPS = vi.RecordFrame;
					videoSource.Emergency = vi.Emergency;
					videoSource.ResolutionWidth = vi.ResolutionWidth;
					videoSource.ResolutionHeight = vi.ResolutionHeight;
					db.Update<tDVRVideoSource>(videoSource);
					ret = true;
				}
			}
			return ret;
		}

		private bool SetDvrResolutions(tDVRAddressBook dvr, RecordingDisplayData rdData)
		{
			if (rdData.RDResolutions == null) return false;
			bool ret = false;
			db.Include<tDVRAddressBook, tDVRAnalogRes>(dvr, item => item.tDVRAnalogRes);
			db.Include<tDVRAddressBook, tDVRResolutions>(dvr, item => item.tDVRResolutions);
			List<tDVRAnalogRes> analogResList = db.Query<tDVRAnalogRes>(x => x.KDVR == dvr.KDVR).ToList();
			bool highVersion = HighVersion();
			
			if (highVersion)
			{
				ret |= SetAnalogRes(dvr, rdData, analogResList);
			}
			else
			{
				ret = SetResolutions(dvr, rdData);
			}
			return ret;
		}

		private bool SetResolutions(tDVRAddressBook dvr, RecordingDisplayData rdData)
		{
			bool ret = false;
			List<tDVRResolutions> reDbsolutionsList = db.Query<tDVRResolutions>(x => x.KDVR == dvr.KDVR).ToList();
			var kVideoFormat = db.Query<tDVRHardware>(t => t.KDVR == dvr.KDVR).Select(t => t.KVideoFormat).FirstOrDefault();
			foreach (var vi in reDbsolutionsList)
			{
				var resolution = db.FirstOrDefault<tResolutionDef>(t => t.KResolution == vi.KResolution);
				RDResolutionInfo anRe =
					rdData.RDResolutions.FirstOrDefault(t => t.id == resolution.ResolutionNo && t.TotalFrame == vi.TotalFrame);
				if (anRe != null)
				{
					rdData.RDResolutions.Remove(anRe);
				}
				else
				{
					db.Delete<tDVRResolutions>(vi);
					ret = true;
				}
			}

			foreach (var vi in rdData.RDResolutions)
			{
				var resolution = db.FirstOrDefault<tResolutionDef>(t => t.KVideoFormat == kVideoFormat && t.ResolutionNo == vi.id);
				var tvi = new tDVRResolutions()
				{
					KDVR = dvr.KDVR,
					KResolution = resolution.KResolution,
					TotalFrame = vi.TotalFrame
				};
				db.Insert<tDVRResolutions>(tvi);
				ret = true;
			}
			return ret;
		}

		private bool SetAnalogRes(tDVRAddressBook dvr, RecordingDisplayData rdData, List<tDVRAnalogRes> analogResList)
		{
			bool ret = false;
			foreach (var vi in analogResList)
			{
			
				RDResolutionInfo viInfo =
					rdData.RDResolutions.FirstOrDefault(
						t => t.Height == vi.Height && t.Width == vi.Width && t.TotalFrame == vi.TotalFrame);
				if (viInfo != null)
				{
					rdData.RDResolutions.Remove(viInfo);
				}
				else
				{
					db.Delete<tDVRAnalogRes>(vi);
					ret = true;
				}
			}

			foreach (var vi in rdData.RDResolutions)
			{
				var tVi = new tDVRAnalogRes() {KDVR = dvr.KDVR};
				vi.SetEntity(ref tVi);
				db.Insert<tDVRAnalogRes>(tVi);
				ret = true;
			}
			return ret;
		}

		private bool HighVersion()
		{
			db.Include<tDVRAddressBook,tDVRVersion>(DVRAdressBook,t=>t.tDVRVersion);
			bool highVersion = false;

			if (DVRAdressBook.tDVRVersion != null)
			{
				highVersion = DVRAdressBook.tDVRVersion.Version >= (int) PRO_VERSION.PRO_2000;
			}
			return highVersion;
		}


		//private List<tDVRResolution> GetDvrResolutionsList(int kDvr, List<RDResolutionInfo> rdResolutionInfoList)
		//{
		//	var reSolutionList = new List<tDVRResolution>();
		//	var kVideoFormat = db.Query<tDVRHardware>(t => t.KDVR == kDvr).Select(t => t.KVideoFormat).FirstOrDefault();
		//	foreach (var vi in rdResolutionInfoList)
		//	{
		//		var tVi = new tDVRResolution();
		//		RDResolutionInfo vi1 = vi;
		//		var resolutionNo =
		//			db.Query<tResolutionDef>(t => t.KVideoFormat == kVideoFormat && t.ResolutionNo == vi1.id)
		//				.Select(t => t.KResolution)
		//				.FirstOrDefault();
		//		SetResolutionInfo(kDvr, resolutionNo, vi, ref tVi);
		//		reSolutionList.Add(tVi);
		//	}
		//	return reSolutionList;
		//}

		//private void SetAnalogRes(Int32 kDvr, RDResolutionInfo resInfo, ref tDVRAnalogRe alogRes)
		//{
		//	alogRes.KDVR = kDvr;
		//	alogRes.Width = resInfo.Width;
		//	alogRes.Height = resInfo.Height;
		//	alogRes.TotalFrame = resInfo.TotalFrame;
		//}

		//private bool CompareAnalogRes(Int32 kDvr, RDResolutionInfo resInfo, tDVRAnalogRe alogRes)
		//{
		//	bool result = alogRes.KDVR == kDvr &&
		//				  alogRes.Width == resInfo.Width &&
		//				  alogRes.Height == resInfo.Height &&
		//				  alogRes.TotalFrame == resInfo.TotalFrame;
		//	return result;
		//}

		//private void SetResolutionInfo(Int32 kDvr, short kResoluion, RDResolutionInfo resInfo, ref tDVRResolution alogRes)
		//{
		//	alogRes.KDVR = kDvr;
		//	alogRes.KResolution = kResoluion;
		//	alogRes.TotalFrame = resInfo.TotalFrame;
		//}
	}
	
	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawRecordingDisplayBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }

		[XmlElement(RawRecordingDisplayConfig.STR_RecordingDisplay)]
		public RecordingDisplayData rdData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }

		[XmlElement(MessageDefines.STR_TimeResult)]
		public Int64 TimeResult { get; set; }

		[XmlElement(MessageDefines.STR_DVRGuid)]
		public string dvrGuid { get; set; }

		[XmlElement(MessageDefines.STR_DVRVersion)]
		public string dvrVersion { get; set; }

		[XmlElement(MessageDefines.STR_DVRProduct)]
		public string dvrProduct { get; set; }
	}

	[XmlRoot(RawRecordingDisplayConfig.STR_RecordingDisplay)]
	public class RecordingDisplayData
	{
		[XmlElement(RawRecordingDisplayConfig.STR_AD4016Encode)]
		public Int32 AD4016Encode { get; set; }

		[XmlArray(RawRecordingDisplayConfig.STR_Channels)]
		[XmlArrayItem(RawRecordingDisplayConfig.STR_Channel)]
		public List<RDChannelInfo> RDChannels = new List<RDChannelInfo>();

		[XmlArray(RawRecordingDisplayConfig.STR_Resolutions)]
		[XmlArrayItem(RawRecordingDisplayConfig.STR_Resolution)]
		public List<RDResolutionInfo> RDResolutions = new List<RDResolutionInfo>();
	}

	[XmlRoot(RawRecordingDisplayConfig.STR_Channel)]
	public class RDChannelInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawRecordingDisplayConfig.STR_RecordFrame)]
		public Int32 RecordFrame { get; set; }

		[XmlElement(RawRecordingDisplayConfig.STR_ResolutionWidth)]
		public Int32 ResolutionWidth { get; set; }

		[XmlElement(RawRecordingDisplayConfig.STR_ResolutionHeight)]
		public Int32 ResolutionHeight { get; set; }

		[XmlElement(RawRecordingDisplayConfig.STR_Emergency)]
		public Int32 Emergency { get; set; }
	}

	[XmlRoot(RawRecordingDisplayConfig.STR_Resolution)]
	public class RDResolutionInfo : IMessageEntity<tDVRAnalogRes>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawRecordingDisplayConfig.STR_TotalFrame)]
		public Int32 TotalFrame { get; set; }

		[XmlAttribute(RawRecordingDisplayConfig.STR_Width)]
		public Int32 Width { get; set; }

		[XmlAttribute(RawRecordingDisplayConfig.STR_Height)]
		public Int32 Height { get; set; }

		//[XmlText]
		//public string Name { get; set; }
		public bool Equal(tDVRAnalogRes value)
		{
			bool result = //value.KDVR == kDvr &&
				  value.Width == Width &&
				  value.Height == Height &&
				  value.TotalFrame == TotalFrame;
			return result;
		}

		public void SetEntity(ref tDVRAnalogRes value)
		{
			if (value == null)
				value = new tDVRAnalogRes();
			value.Width = Width;
			value.Height = Height;
			value.TotalFrame = TotalFrame;
		}
	}
	#endregion
}
