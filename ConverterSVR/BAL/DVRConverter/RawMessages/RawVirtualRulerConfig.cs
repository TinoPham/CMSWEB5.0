using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawVirtualRulerConfig : RawDVRConfig<RawVirtualRulerBody>
	{
		#region Param XML
		public const string STR_VirtualRuler = "virtual_ruler";
		public const string STR_Channels = "channels";
		public const string STR_Channel = "channel";
		public const string STR_Color = "color";
		public const string STR_Unit = "unit";
		public const string STR_StartX = "start_x";
		public const string STR_StartY = "start_y";
		public const string STR_EndX = "end_x";
		public const string STR_EndY = "end_y";
		public const string STR_EnLiveMode = "enable_live_mode";
		public const string STR_StartPoint = "start_point";
		public const string STR_EndPoint = "end_point";
		public const string STR_LenDivision = "length_of_division";
		#endregion

		public override void SetEvnVars(PACDMModel.PACDMDB pacDB, SVRManager Logdb, ConvertMessage.MessageDVRInfo dvrinfo)
		{
			base.SetEvnVars(pacDB, Logdb, dvrinfo);
			if (DVRAdressBook == null)
				return;
			db.Include<tDVRAddressBook, tDVRChannels>(DVRAdressBook, item => item.tDVRChannels);
			DVRAdressBook.tDVRChannels = db.Query<tDVRChannels>(item => item.KDVR == dvrinfo.KDVR).Include(t => t.tDVRVirtualRuler).ToList();
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

			if (UpdateVirtualRuler(DVRAdressBook))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_VIRTUAL_RULER, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdateVirtualRuler(tDVRAddressBook dvrAdressBook)
		{
			if (msgBody == null || msgBody.VRData == null || msgBody.VRData.Channels == null)
				return false;

			bool ret = false;
			foreach (var channel in dvrAdressBook.tDVRChannels.Where(t => t.tDVRVirtualRuler != null).ToList())
			{
				tDVRVirtualRuler vrChannel = channel.tDVRVirtualRuler;
				VRChannelInfo vrInfo = msgBody.VRData.Channels.FirstOrDefault(t => t.id == channel.ChannelNo);
				if (vrInfo != null)
				{
					if (!vrInfo.Equal(vrChannel))
					{
						vrInfo.SetEntity(ref vrChannel);
						db.Update<tDVRVirtualRuler>(vrChannel);
						ret = true;
					}
					msgBody.VRData.Channels.Remove(vrInfo);
				}
				else
				{
					db.Delete<tDVRVirtualRuler>(vrChannel);
					ret = true;
				}
			}

			ret |= InsertVcChannels(dvrAdressBook);
			return ret;
		}

		private bool InsertVcChannels(tDVRAddressBook dvrAdressBook)
		{
			bool ret = false;
			foreach (var vrInfo in msgBody.VRData.Channels)
			{
				tDVRChannels channel = dvrAdressBook.tDVRChannels.FirstOrDefault(t => t.ChannelNo == vrInfo.id);
				var vrChan = new tDVRVirtualRuler() {tDVRChannels = channel};
				vrInfo.SetEntity(ref vrChan);
				db.Insert<tDVRVirtualRuler>(vrChan);
				ret = true;
			}
			return ret;
		}
	}
	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawVirtualRulerBody
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
		[XmlElement(RawVirtualRulerConfig.STR_VirtualRuler)]
		public VirtualRulerData VRData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawVirtualRulerConfig.STR_VirtualRuler)]
	public class VirtualRulerData
	{
		[XmlArray(RawVirtualRulerConfig.STR_Channels)]
		[XmlArrayItem(RawVirtualRulerConfig.STR_Channel)]
		public List<VRChannelInfo> Channels = new List<VRChannelInfo>();
	}

	[XmlRoot(RawVirtualRulerConfig.STR_Channel)]
	public class VRChannelInfo : IMessageEntity<tDVRVirtualRuler>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_Color)]
		public Int32 Color { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_Unit)]
		public Int16 Unit { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_StartX)]
		public Int32 StartX { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_StartY)]
		public Int32 StartY { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_EndX)]
		public Int32 EndX { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_EndY)]
		public Int32 EndY { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_EnLiveMode)]
		public Int32 EnLiveMode { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_StartPoint)]
		public float StartPoint { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_EndPoint)]
		public float EndPoint { get; set; }

		[XmlElement(RawVirtualRulerConfig.STR_LenDivision)]
		public float LenDivision { get; set; }

		public bool Equal(tDVRVirtualRuler value)
		{
			bool result = value.Color == Color;
			result &= value.KUnit == Unit;
			result &= value.StartX == StartX;
			result &= value.StartY == StartY;
			result &= value.EndX == EndX;
			result &= value.EndY == EndY;
			result &= value.StartPoint == StartPoint;
			result &= value.EndPoint == EndPoint;
			result &= value.LengthOfDivision == LenDivision;
			result &= value.EnableLiveMode == (EnLiveMode != 0);
			return result;
		}

		public void SetEntity(ref tDVRVirtualRuler value)
		{
			if (value == null)
				value = new tDVRVirtualRuler();
			//value.KChannel = kChannel;
			value.Color = Color;
			value.KUnit = Unit;
			value.StartX = StartX;
			value.StartY = StartY;
			value.EndX = EndX;
			value.EndY = EndY;
			value.EnableLiveMode = EnLiveMode != 0;
			value.StartPoint = StartPoint;
			value.EndPoint = EndPoint;
			value.LengthOfDivision = LenDivision;
		}
	}
	#endregion
}
