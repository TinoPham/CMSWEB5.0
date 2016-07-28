using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawVideoConfig : RawDVRConfig<RawVideoBody>
	{
		#region Parameter
		public const string STR_VideoConfig = "video_config";
		public const string STR_VideoInputs = "video_inputs";
		public const string STR_VideoInput = "video_input";
		public const string STR_Brightness = "brightness";
		public const string STR_Hue = "hue";
		public const string STR_Contrast = "contrast";
		public const string STR_Monochrome = "is_monochrome";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawVideoBody msgBody { get; set; }
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);

		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null)
				return await base.UpdateToDB();
			if (DVRAdressBook.tDVRChannels == null || DVRAdressBook.tDVRChannels.Count == 0)
			{
				SetDvrReconnect(msgHeader.DVRGuid, reconnectFlag: true);
				return await base.UpdateToDB();
			}

			if (msgBody.videoData == null || msgBody.videoData.VideoInputs == null)
				return await base.UpdateToDB();

			if (UpdateVideoConfig(DVRAdressBook,msgBody.videoData.VideoInputs))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_VIDEO, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateVideoConfig(tDVRAddressBook dvrAdressBook, List<VIInfo> lsInfo)
		{
			db.Include<tDVRAddressBook,tDVRVideoSource>(dvrAdressBook,item =>item.tDVRVideoSources );
			Func<tDVRVideoSource, VIInfo, bool> func_filter = (dbitem, info) => dbitem.VideoSourceNo == info.id;
			Func<tDVRVideoSource, VIInfo, bool> compare_update = null;
			Expression<Func<tDVRVideoSource, object>> updatedata = item => item.tDVRAddressBook;

			Expression<Func<tDVRVideoSource, int>> db_key = dbitem => dbitem.KVideoSource;
			Expression<Func<VIInfo, int>> info_key = info => info.id;
			return base.UpdateDBData<tDVRVideoSource, VIInfo, int, int>(dvrAdressBook.tDVRVideoSources, lsInfo, func_filter, compare_update, updatedata, dvrAdressBook, db_key, info_key);
		}


		#region unused

//public override async Task<Commons.ERROR_CODE> UpdateToDB()
		//{
		//	if(DVRAdressBook == null)
		//		return await base.UpdateToDB();

		//	SetVideoSources(DVRAdressBook.KDVR);
		//	return await base.UpdateToDB();
		//}


		//private void SetVideoSources(Int32 kDvr)
		//{
		//	List<tDVRVideoSource> lsVideoSource = db.Query<tDVRVideoSource>(x => x.KDVR == kDvr).ToList();

		//	List<VIInfo> viInfoList = GetViInfoList();

		//	foreach (var tv in lsVideoSource)
		//	{
		//		tDVRVideoSource tVi = tv;
		//		VIInfo viInfo = null;
		//		if (viInfoList != null)
		//		{
		//			viInfo = viInfoList.FirstOrDefault(t => t.id == tv.VideoSourceNo);
		//		}
		//		if (viInfo != null)
		//		{
		//			if (!CompareVideoSource(kDvr, viInfo, tVi))
		//			{
		//				SetVideoSourceInfo(kDvr, viInfo, ref tVi);
		//				db.Update<tDVRVideoSource>(tVi);
		//			}
		//			viInfoList.Remove(viInfo);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRVideoSource>(tVi);
		//		}
		//	}
		//	if (viInfoList != null)
		//	{
		//		InsertVs(kDvr, viInfoList);
		//	}
		//	db.Save();
		//}

		//private List<VIInfo> GetViInfoList()
		//{
		//	List<VIInfo> viInfoList = null;
		//	if (msgBody.videoData != null)
		//	{
		//		if (msgBody.videoData.VideoInputs != null)
		//		{
		//			viInfoList = msgBody.videoData.VideoInputs.ToList();
		//		}
		//	}
		//	return viInfoList;
		//}

		//private void InsertVs(int kDvr, List<VIInfo> viInfoList)
		//{
		//	foreach (var tv in viInfoList)
		//	{
		//		var tVi = new tDVRVideoSource();
		//		SetVideoSourceInfo(kDvr, tv, ref tVi);
		//		db.Insert<tDVRVideoSource>(tVi);
		//	}
		//}

		//private void SetVideoSourceInfo(Int32 kDvr, VIInfo viInfo, ref tDVRVideoSource vsInfo)
		//{
		//	vsInfo.KDVR = kDvr;
		//	vsInfo.VideoSourceNo = viInfo.id;
		//	vsInfo.Brightness = viInfo.Brightness;
		//	vsInfo.Contrast = viInfo.Contrast;
		//	vsInfo.Hue = viInfo.Hue;
		//	vsInfo.Monochrome = viInfo.Monochrome;
		//}

		//private bool CompareVideoSource(int kDvr, VIInfo viInfo, tDVRVideoSource vsInfo)
		//{
		//	bool result = vsInfo.KDVR == kDvr &&
		//				  vsInfo.VideoSourceNo == viInfo.id &&
		//				  vsInfo.Brightness == viInfo.Brightness &&
		//				  vsInfo.Contrast == viInfo.Contrast &&
		//				  vsInfo.Hue == viInfo.Hue &&
		//				  vsInfo.Monochrome == viInfo.Monochrome;
		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawVideoBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/
		[XmlElement(RawVideoConfig.STR_VideoConfig)]
		public VideoData videoData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawVideoConfig.STR_VideoConfig)]
	public class VideoData
	{
		[XmlArray(RawVideoConfig.STR_VideoInputs)]
		[XmlArrayItem(RawVideoConfig.STR_VideoInput)]
		public List<VIInfo> VideoInputs = new List<VIInfo>();
	}

	[XmlRoot(RawVideoConfig.STR_VideoInput)]
	public class VIInfo : IMessageEntity<tDVRVideoSource>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVideoConfig.STR_Brightness)]
		public Int32 Brightness { get; set; }

		[XmlElement(RawVideoConfig.STR_Hue)]
		public Int32 Hue { get; set; }

		[XmlElement(RawVideoConfig.STR_Contrast)]
		public Int32 Contrast { get; set; }

		[XmlElement(RawVideoConfig.STR_Monochrome)]
		public Int32 Monochrome { get; set; }

		public bool Equal(tDVRVideoSource value)
		{
			bool result =
			  value.VideoSourceNo == id &&
			  value.Brightness == Brightness &&
			  value.Contrast == Contrast &&
			  value.Hue == Hue &&
			  value.Monochrome == Monochrome;
			return result;
		}

		public void SetEntity(ref tDVRVideoSource value)
		{
			if (value == null)
				value = new tDVRVideoSource();
			value.VideoSourceNo = id;
			value.Brightness = Brightness;
			value.Contrast = Contrast;
			value.Hue = Hue;
			value.Monochrome = Monochrome;
		}
	}
#endregion
}
