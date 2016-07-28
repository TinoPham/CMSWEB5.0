using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Data.Entity;
using PACDMModel.Model;
using Commons;
using SVRDatabase;
using ConverterSVR.BAL.DVRConverter.RawMessages.HardwareConfig;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawHardwareConfig : RawDVRConfig<RawHardwareBody>
	{
		#region ParamXML
		public const string STR_Hardware = "hardware";
		public const string STR_NextConfigs = "next_configs";
		public const string STR_VideoFormat = "video_format";
		public const string STR_CCInfo = "capture_card_info";
		public const string STR_Model = "model";
		public const string STR_NumOfBoards = "num_of_boards";
		public const string STR_NumOfChips = "num_of_chips";
		public const string STR_COMPorts = "com_ports";
		public const string STR_COMPort = "com_port";
		public const string STR_HaspKey = "hasp_key";
		public const string STR_HPKCode = "hpk_code";
		public const string STR_PAC = "pac";
		public const string STR_VideoFPS = "video_fps";
		public const string STR_Audio = "audio";
		public const string STR_IPCamera = "ip_camera";
		public const string STR_VideoLogixBasic = "video_logix_basic";
		public const string STR_VisionCountBasic = "vision_count_basic";
		public const string STR_VideoLogix = "video_logix";
		public const string STR_VisionCount = "vision_count";
		public const string STR_LPR = "lpr";
		public const string STR_CMSMode = "cms_mode";
		public const string STR_ControlCount = "control_count";
		public const string STR_SensorCount = "sensor_count";
		public const string STR_HPK_Analog = "analog";
		public const string STR_HPK_Monitor = "monitor";
		public const string STR_HPK_FaceBlur = "faceblur";
		public const string STR_HPK_ISearch = "isearch";
		public const string STR_HPK_MaxConnection = "max_connection";
		public const string STR_HPK_Version = "version";
		public const string STR_HPK_Upgradable = "upgradable";
		public const string STR_HPK_Heatmap = "heatmap";

		public const string STR_IPCameras = "ip_cameras";
		public const string STR_VideoSource = "video_source";
		public const string STR_VSInfo = "video_source";
		public const string STR_ExtMonitor = "ext_monitor";
		public const string STR_DWell = "dwell";
		public const string STR_InputMask = "input_mask";
		public const string STR_EnExtMonitor = "en_ext";
		public const string STR_Channels = "channels";
		//public const string STR_Size = "size";
		public const string STR_NumVideoInput = "num_video_input";
		public const string STR_NumPTZType = "num_ptz_type";
		public const string STR_Channel = "channel";
		//public const string STR_Name = "name";
		public const string STR_Enable = "enable";
		public const string STR_CameraID = "camera_id";
		public const string STR_DWellTime = "dwell_time";
		public const string STR_AP = "ap";
		public const string STR_ptzType = "ptz_type";
		public const string STR_AudioSource = "audio_source";
		public const string STR_VideoCompQuality = "video_compress_quality";
		public const string STR_EnableiSearch = "en_isearch";

		public const string STR_IOCardInfo = "iocard_info";
		public const string STR_IOCard = "iocard";
		public const string STR_CardType = "card_type";
		public const string STR_NumberControl = "number_control";
		public const string STR_NumberSensor = "number_sensor";
		public const string STR_MonitorSupport = "monitor_support";
		public const string STR_PTZSupport = "ptz_support";
		public const string STR_WatchdogSupport = "watchdog_support";
		public const string STR_ComPort = "com_port";
		public const string STR_ip = "ip";
		public const string STR_MACAddress = "mac_address";
		public const string STR_IOName = "io_name";

		public const string STR_Controls = "controls";
		public const string STR_Control = "control";
		public const string STR_No = "no";
		//public const string STR_Name = "name";
		//public const string STR_Enable = "enable";
		public const string STR_WorkSeconds = "work_seconds";
		public const string STR_BeginTime = "begin_time";
		public const string STR_EndTime = "end_time";
		public const string STR_ChannelMask = "channel_mask";
		public const string STR_MotionMask = "motion_mask";
		public const string STR_RealIndex = "real_index";

		public const string STR_Sensors = "sensors";
		public const string STR_Alarm = "alarm";
		public const string STR_PreRecTime = "pre_record_time";
		public const string STR_PostRecTime = "post_record_time";
		//public const string STR_Size = "size";
		public const string STR_Sensor = "sensor";
		//public const string STR_Name = "name";
		//public const string STR_Enable = "enable";
		public const string STR_NormalOpen = "normal_open";
		//public const string STR_ChannelMask = "channel_mask";
		public const string STR_ControlMask = "control_mask";
		//public const string STR_RealIndex = "real_index";

		public const string STR_DigiMonitors = "digital_monitors";
		public const string STR_Monitor = "monitor";
		public const string STR_DisplayChannels = "display_channels";
		public const string STR_SensorTrigger = "sensor_triggered";
		public const string STR_VLTrigger = "video_logix_triggered";
		public const string STR_ControlPTZTrigger = "control_ptz_triggered";
		public const string STR_MotionTrigger = "motion_triggered";
		public const string STR_Enabled = "enabled";
		//public const string STR_ChannelMask = "channelMask";
		public const string STR_DwellTime = "dwellTime";
		public const string STR_Priority = "priority";
		public const string STR_Stream = "stream";
		public const string STR_MonitorSensor = "monitor_sensor";

		public const string STR_ControlType = "con_type";
		public const string STR_SensorType = "sen_type";
		public const string STR_DisplayLayout = "displayLayout";
		
		#endregion

		#region ParamXML Monitor
		public const string STR_MON_Digital_monitors = "digital_monitors";
		public const string STR_MON_Monitor = "monitor";
		public const string STR_MON_Display_Channels = "display_channels";
		public const string STR_MON_Sensor_Triggered = "sensor_triggered";
		public const string STR_MON_Video_Logix_Triggered = "video_logix_triggered";
		public const string STR_MON_Control_Ptz_Triggered = "control_ptz_triggered";
		public const string STR_MON_Motion_Triggered = "motion_triggered";
		public const string STR_MON_Enabled = "enabled";
		public const string STR_MON_ChannelMask = "channelMask";
		public const string STR_MON_Channels = "channels";
		public const string STR_MON_Channel = "channel";
		public const string STR_MON_DwellTime = "dwellTime";
		public const string STR_MON_Priority = "priority";
		public const string STR_MON_Stream = "stream";
		public const string STR_MON_ShowInfo = "showInfo";

		//[XmlElement(STR_Header)]
		//public RawMsgHeader msgHeader { get; set; }

		//[XmlElement(STR_Body)]
		//public RawHardwareBody msgBody { get; set; }

		//private readonly int[] _notSend = { 3, 19, 20, 21 };
		private const int _numMsgReq = (int)DVR_CONFIGURATION.EMS_CFG_END;
		//List<tDVRIOCard> _lsIoCards;
		//List<tDVRControl> _lsControl;
		//List<tDVRSensor> _lsSensor;
		#endregion

		
		public override async Task<string> GetResponseMsg()
		{
			if (msgHeader.MsgID == (int)CMSMsg.MSG_DVR_GET_CONFIG_RESPONSE)// 10006
			{
				IEnumerable<int> msgResponse = null;
				if (DVRAdressBook.KDVRVersion < VersionPro3_3)
				{
					IEnumerable<int> msgRanges = Enum.GetValues(typeof(DVR_CONFIGURATION)).Cast<int>().Where( item => item > 0 && item < _numMsgReq);
					msgResponse = msgRanges.Except( _notSend);
				}
				else if (msgBody.lsNextConfigs != null && msgBody.lsNextConfigs.Count > 0)
				{
					msgResponse = msgBody.lsNextConfigs.Except(_notSend);
				}
				if (msgResponse != null && msgResponse.Count() > 0)
				{
					List<string> seqMessage = new List<string>();
					foreach (int msgID in msgResponse)
					{
						seqMessage.Add(Utils.String2Base64(GetRequestConfigMsg(msgHeader.DVRGuid, msgID)));
					}
					string combined = string.Join(", ", seqMessage);

					return await Task.FromResult<string>(combined);
				}
			}
			return await Task.FromResult<string>( null);
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.hwData == null)
				return await base.UpdateToDB();

			if (msgBody == null || msgBody.hwData == null)
				return await base.UpdateToDB();

			RawHardwareData HWData = msgBody.hwData;

			bool ret = false;

			//Anh Huynh, If no channel info, this is IO data message only, Aug 14, 2014
			if (HWData.Channels != null)
			{
				ret = UpdateHWConfig(DVRAdressBook, msgBody.hwData);
				ret |= UpdateHWPort(DVRAdressBook, HWData.ComPorts);
				if (ret && db.Save() == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED);

				ret = false;
				ret |= UpdateHWChanel(DVRAdressBook, HWData.Channels.Channels);

				if (ret && db.Save() == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED);

				ret = UpdateMonitors(DVRAdressBook, HWData.Monitors);
				if (ret && db.Save() == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED);
			}

			db.Include<tDVRAddressBook, tDVRIOCard>(DVRAdressBook, item => item.tDVRIOCards);

			PreProcessIoCards();

			ret = UpdateIOCards(DVRAdressBook, HWData.IOCards, DVRAdressBook.tDVRIOCards);

			if (ret && db.Save() == -1)
				return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED);

			bool re_updck = UpdateChecksum((int) DVR_CONFIGURATION.EMS_CFG_HARDWARE, msgBody.msgCommon);

			if (HWData.Channels != null && re_updck)
			{
				int re = 0;
				if ((re = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(re == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			return await Task.FromResult<Commons.ERROR_CODE>(Commons.ERROR_CODE.OK);

			//if (IsChannelInfoData())
			//{
			//	SetHwConfig(iVersion, iProduct);

			//	SetHwPort(DVRAdressBook.KDVR);

			//	//Update channel info
			//	SetHwChannels(DVRAdressBook.KDVR);
			//	SetMonitorConfig(DVRAdressBook.KDVR);
			//}

			//SetHwioCards(DVRAdressBook.KDVR);

		}

		private void PreProcessIoCards()
		{
			CreateVirtualIoCard();
			CreateKeyForUsbIoCards();

		}

		private void CreateKeyForUsbIoCards()
		{
			//if (msgBody.hwData.IOCards.Count(item => item.CardType == 3) > 1)
			//{
			foreach (var iocard in msgBody.hwData.IOCards.Where(item => item.CardType == 3))
			{
				iocard.MACAddress += iocard.id.ToString();
			}
			//}
		}

		private void CreateVirtualIoCard()
		{
			if (msgBody.hwData.Controls != null && msgBody.hwData.IOCards.Count == 0 && msgBody.hwData.Controls.Count > 0)
			{
				var ioCard = new IOCardInfo()
				{
					CardType = -1,
					ComPort = null,
					IOName = "Virtual IOCard",
					IP = null,
					MACAddress = "i3VirtualIOCard",
					MonitorSupport = 0,
					NumberControl = msgBody.hwData.Controls.Count,
					NumberSensor = msgBody.hwData.Sensors != null && msgBody.hwData.Sensors.Sensors != null ? msgBody.hwData.Sensors.Sensors.Count : 0 ,
					PTZSupport = 0,
					WatchdogSupport = 0,
					id = 0
				};
				msgBody.hwData.IOCards.Add(ioCard);
			}
		}

		private bool UpdateHWConfig( tDVRAddressBook dvradd, RawHardwareData rawHW)
		{
			if (rawHW == null)// || rawHW.Channels== null)
				return false;
			//db.Include<tDVRAddressBook, tDVRVersion>(dvradd, item => item.tDVRVersion);
			if(dvradd.KDVRVersion == null)
				return false;
			rawHW.IgnoredRecodingTime = dvradd.KDVRVersion >= VersionProSchedule;
			tDVRHardware dvrHW = db.FirstOrDefault<tDVRHardware>(item => item.KDVR == dvradd.KDVR);
			if( dvrHW == null)
			{
				dvrHW = new tDVRHardware(){KDVR = dvradd.KDVR};
				rawHW.SetEntity( ref dvrHW);
				db.Insert<tDVRHardware>(dvrHW);
				return true;
			}
			else
			{
				if(rawHW.Equal(dvrHW))
					return false;

				rawHW.SetEntity(ref dvrHW);
				db.Update<tDVRHardware>(dvrHW);
				return true;
			}
		}
		
		private bool UpdateHWPort( tDVRAddressBook dvradd, List<COMPort> ports)
		{
			db.Include<tDVRAddressBook, tDVRRS232Ports>(dvradd, item => item.tDVRRS232Ports);
			db.Include<tDVRAddressBook, tDVRSystemInfo>(dvradd, item => item.tDVRSystemInfo);

			IEnumerable<string> Portmatch =  from dvrport in dvradd.tDVRRS232Ports
						from port in ports
						where string.Compare( dvrport.PortName, port.Name, true) == 0
						select port.Name;

			bool ret = false;
			IEqualityComparer<string> icompare = new StringEqualityComparer(true);
			//new ports
			IEnumerable<COMPort> addPorts = ports.Where(item => !Portmatch.Contains(item.Name, icompare));
			tDVRRS232Ports new232 = null;
			foreach(COMPort comport in addPorts)
			{
				new232 = new tDVRRS232Ports{ PortName = comport.Name.ToUpper(), tDVRAddressBook = dvradd};
				db.Insert<tDVRRS232Ports>(new232);
				ret = true;
			}

			//delete unused port
			IEnumerable<tDVRRS232Ports> deleteports = dvradd.tDVRRS232Ports.Where(item => !Portmatch.Contains(item.PortName, icompare)).ToList();
			foreach (tDVRRS232Ports rs232 in deleteports)
			{
				//check & delete Kport in systeminfo table
				if (dvradd.tDVRSystemInfo != null && dvradd.tDVRSystemInfo.KPort == rs232.KPort)
				{
					dvradd.tDVRSystemInfo.KPort = null;
					db.Update<tDVRSystemInfo>(dvradd.tDVRSystemInfo);
				}

				db.Delete<tDVRRS232Ports>(rs232);
				ret = true;
			}

			return ret;

		}
		
		private bool UpdateHWChanel(tDVRAddressBook dvradd, List<ChannelInfo> channels )
		{
			if( channels == null|| channels.Count == 0)
				return false;

			db.Include<tDVRAddressBook, tDVRChannels>( dvradd, item => item.tDVRChannels);
			/*
			Func<tDVRChannels, ChannelInfo, bool> func_filter = (dbitem, info) => dbitem.ChannelNo == info.id;
			Func<tDVRChannels, ChannelInfo, bool> compare_update = null;
			Expression<Func<tDVRChannels, object>> updatedata = dbitem => dbitem.tDVRAddressBook;
			Expression<Func<tDVRChannels, int>> db_key = dbitem => dbitem.ChannelNo;
			Expression<Func<ChannelInfo, int>> info_key = info => info.id;
			return base.UpdateDBData<tDVRChannels, ChannelInfo, int, int>(dvradd.tDVRChannels, channels, func_filter, compare_update, updatedata, dvradd, db_key, info_key);
			*/
			# region unused
			var update = from dvrchan in dvradd.tDVRChannels
						 from chan in channels
						 where dvrchan.ChannelNo == chan.id
						 select new { DVRChan = dvrchan, ChanInfo = chan };
			bool ret = false;

			tDVRChannels updatechannel = null;
			List<int> channelNo = new List<int>();
			foreach (var item in update)
			{
				db.Include<tDVRChannels, tCMSWebSites>(item.DVRChan, it => it.tCMSWebSites);
				channelNo.Add(item.DVRChan.ChannelNo);
				if (item.ChanInfo.Equal(item.DVRChan))
					continue;
				ret = true;
				updatechannel = item.DVRChan;
				item.ChanInfo.SetEntity(ref updatechannel);
				db.Update<tDVRChannels>(item.DVRChan);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}

			//delete unused channel
			List<tDVRChannels> deletes = dvradd.tDVRChannels.Where(item => !channelNo.Contains(item.ChannelNo)).ToList();
			foreach (tDVRChannels delitem in deletes)
			{
				db.Include<tDVRChannels, tCMSWebSites>(delitem, item => item.tCMSWebSites);
				if (delitem.tCMSWebSites.Any())
				{
					foreach (tCMSWebSites site in delitem.tCMSWebSites)
					{
						db.Include<tCMSWebSites, tDVRChannels>(site, item => item.tDVRChannels);
						site.tDVRChannels.Remove(delitem);
					}
					delitem.tCMSWebSites.Clear();
				}
				db.Delete<tDVRChannels>(delitem);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			//add new channel
			IEnumerable<ChannelInfo> newchannels = channels.Where(item => !channelNo.Contains(item.id));
			if (newchannels.Any())
			{
				tCMSWebSites defSite = null;
				tDVRChannels chan = dvradd.tDVRChannels.FirstOrDefault(x=>x.tCMSWebSites.Any());
				if (chan != null)
				{
					defSite = chan.tCMSWebSites.FirstOrDefault();
				}
				foreach (ChannelInfo add_item in newchannels)
				{
					updatechannel = null;
					add_item.SetEntity(ref updatechannel);
					updatechannel.tDVRAddressBook = dvradd;
					if (defSite != null)
					{
						if (updatechannel.tCMSWebSites == null)
						{
							updatechannel.tCMSWebSites = new HashSet<tCMSWebSites>();
						}
						updatechannel.tCMSWebSites.Add(defSite);
					}
					db.Insert<tDVRChannels>(updatechannel);
					ret = true;

				}
			}
			return ret;
			#endregion
		}

		//Anh Huynh, Update for new Digital Monitor structure, Sept 22, 2014, begin
		private bool UpdateMonitors(tDVRAddressBook dvradd, List<Monitor> monitors )
		{
			if (monitors == null) return false;
			/*
			dvradd.tDVRHWDigitalMonitors = db.Query<tDVRHWDigitalMonitor>( item => item.KDVR == dvradd.KDVR).ToList();
			db.Include<tDVRAddressBook, tDVRHWDigitalMonitor>( dvradd, item => item.tDVRHWDigitalMonitors);

			bool ret = false;

			List<HardwareMonitor> hwMonitors = new List<HardwareMonitor>();
			foreach (Monitor mo in monitors)
			{
				hwMonitors.AddRange(mo.HWMonitors);
			}
			ret |= UpdateMonitors(dvradd, hwMonitors, dvradd.tDVRHWDigitalMonitors);
			*/
			bool ret = false;
			dvradd.tDVRDigitalMonitors = db.Query<tDVRDigitalMonitor>(item => item.KDVR == dvradd.KDVR).ToList();
			db.Include<tDVRAddressBook, tDVRDigitalMonitor>(dvradd, item => item.tDVRDigitalMonitors);
			ret |= UpdateMonitors(dvradd, monitors, dvradd.tDVRDigitalMonitors);

			return ret;
		}
		private bool UpdateMonitors(tDVRAddressBook dvrAdd, List<Monitor> hwMonitors, ICollection<tDVRDigitalMonitor> dvrMonitors)
		{
			hwMonitors.RemoveAll(item => item == null);
			bool ret = false;
			//DVR 3.3
			var monUpdates = from dbmon in dvrAdd.tDVRDigitalMonitors
							 from mon in hwMonitors
							 where dbmon.MonitorNo == mon.id
							 select new { DBMon = dbmon, Info = mon };

			tDVRDigitalMonitor dvrMon = null;
			foreach (var item in monUpdates)
			{
				db.Include<tDVRDigitalMonitor, tDVRDigitalMonitorOption>(item.DBMon, i => i.tDVRDigitalMonitorOptions);
				dvrMon = item.DBMon;
				dvrMon.Enable = item.Info.Enabled;
				dvrMon.ShowInfo = item.Info.ShowInfo;
				db.Update<tDVRDigitalMonitor>(dvrMon);
				UpdateMonitorOption(item.Info, item.DBMon);
			}

			IEnumerable<tDVRDigitalMonitor> monDeletes = dvrMonitors.Except(monUpdates.Select(item => item.DBMon)).ToList();
			foreach (tDVRDigitalMonitor delIt in monDeletes)
			{
				DeleteMonitor(delIt);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				ret = true;
			}

			List<Monitor> monInserts = hwMonitors.Except(monUpdates.Select(it => it.Info)).ToList();
			foreach (Monitor newIt in monInserts)
			{
				dvrMon = new tDVRDigitalMonitor();
				dvrMon.MonitorNo = newIt.id;
				dvrMon.Enable = newIt.Enabled;
				dvrMon.ShowInfo = newIt.ShowInfo;
				//dvrMon.KDVR = dvrAdd.KDVR;
				dvrMon.tDVRAddressBook = dvrAdd;

				db.Insert<tDVRDigitalMonitor>(dvrMon);

				InsertMonitorOption(newIt, dvrMon);
			}
			ret = monUpdates.Count() > 0 || monDeletes.Count() > 0 || monInserts.Count > 0;
			return ret;
		}
		#region unused
		/*
		private bool UpdateMonitors(tDVRAddressBook dvradd, List<HardwareMonitor> HWmonitors, ICollection<tDVRHWDigitalMonitor> dvrMonitors)
		{
			HWmonitors.RemoveAll( item => item == null);

			//Func<tDVRHWDigitalMonitor, HardwareMonitor, bool> func_filter = (dbitem, info) => dbitem.MonitorNo == info.id && string.Compare(dbitem.OptName, info.OptName, true) == 0;
			//Func<tDVRHWDigitalMonitor, HardwareMonitor, bool> compare_update = null;
			//Expression<Func<tDVRHWDigitalMonitor, object>> updatedata = dbitem => dbitem.tDVRAddressBook;
			//return base.UpdateDBData<tDVRHWDigitalMonitor, HardwareMonitor>(dvradd.tDVRHWDigitalMonitor, HWmonitors, func_filter, compare_update, updatedata, dvradd);

			#region unused
			var updates = from dvrmo in dvrMonitors
						  from infomo in HWmonitors
						  where dvrmo.MonitorNo == infomo.id && System.String.Compare(dvrmo.OptName, infomo.OptName, System.StringComparison.OrdinalIgnoreCase) == 0
						  select new { DVRMo = dvrmo, Info = infomo };
			tDVRHWDigitalMonitor dvrHWUpdate = null;
			bool ret = false;

			foreach (var update in updates)
			{
				dvrHWUpdate = update.DVRMo;
				if (update.Info.Equal(dvrHWUpdate))
					continue;
				update.Info.SetEntity(ref dvrHWUpdate);
				db.Update<tDVRHWDigitalMonitor>(dvrHWUpdate);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}

			var deletes = dvrMonitors.Except(updates.Select(item => item.DVRMo)).ToList();
			foreach (var delete in deletes)
			{
				db.Delete<tDVRHWDigitalMonitor>(delete);
				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			var adds = HWmonitors.Except(updates.Select(item => item.Info));
			foreach (var add in adds)
			{
				dvrHWUpdate = new tDVRHWDigitalMonitor(){KDVR = dvradd.KDVR};
				add.SetEntity(ref dvrHWUpdate);
				db.Insert<tDVRHWDigitalMonitor>(dvrHWUpdate);
				ret = true;
			}
			return ret;
			#endregion
		}
		*/
		#endregion
		private void SetMonitorChannels(HardwareMonitor hwOpt, tDVRDigitalMonitorOption dbOpt)
		{
			if (hwOpt.Channels != null && hwOpt.Channels.Count > 0)
			{
				int iPos = 0;
				for (int i = 0; i < hwOpt.Channels.Count; i++)
				{
					if (hwOpt.Channels[i] < 0)
						continue;
					tDVRDigitalMonitorChannel dmChan = new tDVRDigitalMonitorChannel();
					dmChan.ChannelNo = hwOpt.Channels[i];
					dmChan.Position = iPos;
					//dmChan.KOption = dbOpt.KOption;
					dmChan.tDVRDigitalMonitorOption = dbOpt;

					db.Insert<tDVRDigitalMonitorChannel>(dmChan);
					iPos++;
				}
			}
			else if (hwOpt.ChannelMask > 0)
			{
				int iPos = 0;
				for (int ich = 0; ich < MAX_CHANNEL; ich++)
				{
					if ((hwOpt.ChannelMask & ((UInt64)1 << ich)) == 0)
						continue;
					tDVRDigitalMonitorChannel dmChan = new tDVRDigitalMonitorChannel();
					dmChan.ChannelNo = ich;
					dmChan.Position = iPos;
					//dmChan.KOption = dbOpt.KOption;
					dmChan.tDVRDigitalMonitorOption = dbOpt;

					db.Insert<tDVRDigitalMonitorChannel>(dmChan);
					iPos++;
				}
			}
		}
		private bool UpdateMonitorOption(Monitor hwMonitor,  tDVRDigitalMonitor dbMonitor)
		{
			bool ret = false;
			List<tDVRDigitalMonitorOption> lsDBMonitors = dbMonitor.tDVRDigitalMonitorOptions.ToList();
			foreach (tDVRDigitalMonitorOption opt in lsDBMonitors)
			{
				db.DeleteWhere<tDVRDigitalMonitorChannel>(x => x.KOption == opt.KOption);
			}

			tDVRDigitalMonitorOption newOpt = null;
			hwMonitor.UpdateHWList();
			foreach (HardwareMonitor hwOpt in hwMonitor.HWMonitors)
			{
				newOpt = lsDBMonitors.FirstOrDefault(x=>String.Compare(x.OptName, hwOpt.OptName) == 0); //new tDVRDigitalMonitorOption();
				if (newOpt == null)
				{
					newOpt = new tDVRDigitalMonitorOption();
					hwOpt.SetEntity(ref newOpt);
					db.Insert<tDVRDigitalMonitorOption>(newOpt);
				}
				else
				{
					hwOpt.SetEntity(ref newOpt);
					db.Update<tDVRDigitalMonitorOption>(newOpt);
					lsDBMonitors.Remove(newOpt);
				}

				SetMonitorChannels(hwOpt, newOpt);
			}

			return ret;
		}
		private bool DeleteMonitor(tDVRDigitalMonitor dvrMonitor)
		{
			bool ret = false;
			foreach (tDVRDigitalMonitorOption opt in dvrMonitor.tDVRDigitalMonitorOptions)
			{
				db.DeleteWhere<tDVRDigitalMonitorChannel>(x=>x.KOption == opt.KOption);
				db.Delete<tDVRDigitalMonitorOption>(opt);
			}
			//db.DeleteWhere<tDVRDigitalMonitorOption>(x => dvrMonitor.tDVRDigitalMonitorOptions.Contains(x));
			//db.DeleteWhere<tDVRDigitalMonitorChannel>(x=>dvrMonitor.
			return ret;
		}
		private bool InsertMonitorOption(Monitor hwMonitor, tDVRDigitalMonitor dbMonitor)
		{
			bool ret = false;
			//db.Include<tDVRDigitalMonitor, tDVRDigitalMonitorOption>(dbMonitor, item => item.tDVRDigitalMonitorOptions);
			tDVRDigitalMonitorOption newOpt = null;
			hwMonitor.UpdateHWList();

			//int iPos = 0;
			foreach (HardwareMonitor hwOpt in hwMonitor.HWMonitors)
			{
				newOpt = new tDVRDigitalMonitorOption();
				hwOpt.SetEntity(ref newOpt);
				newOpt.tDVRDigitalMonitor = dbMonitor;

				db.Insert<tDVRDigitalMonitorOption>(newOpt);

				SetMonitorChannels(hwOpt, newOpt);
				/*
				if (hwOpt.Channels != null && hwOpt.Channels.Count > 0)
				{
					iPos = 0;
					for (int i = 0; i < hwOpt.Channels.Count; i++)
					{
						if (hwOpt.Channels[i] < 0)
							continue;
						tDVRDigitalMonitorChannel dmChan = new tDVRDigitalMonitorChannel();
						dmChan.ChannelNo = hwOpt.Channels[i];
						dmChan.Position = iPos;
						dmChan.KOption = newOpt.KOption;
						db.Insert<tDVRDigitalMonitorChannel>(dmChan);
						iPos++;
					}
				}
				else if(hwOpt.ChannelMask > 0)
				{
					iPos = 0;
					for (int ich = 0; ich < MAX_CHANNEL; ich++)
					{
						if ((hwOpt.ChannelMask & ((UInt64)1 << ich)) == 0)
							continue;
						tDVRDigitalMonitorChannel dmChan = new tDVRDigitalMonitorChannel();
						dmChan.ChannelNo = ich;
						dmChan.Position = iPos;
						dmChan.KOption = newOpt.KOption;
						db.Insert<tDVRDigitalMonitorChannel>(dmChan);
						iPos++;
					}
				}*/
			} //foreach
			return ret;
		}
		//Anh Huynh, Update for new Digital Monitor structure, Sept 22, 2014, end

		private bool UpdateIOCards(tDVRAddressBook dvradd, List<IOCardInfo> ioCardinfo, ICollection<tDVRIOCard> dvrIOCards)
		{
			db.Include<tDVRAddressBook, tDVRSensors>( dvradd, item => item.tDVRSensors);
			db.Include<tDVRAddressBook, tDVRControl>(dvradd, item => item.tDVRControls);
			bool ret = false;
			var updates = from dvrio in dvrIOCards
						from ioinfo in ioCardinfo
						where string.Compare( dvrio.MacAddress, ioinfo.MACAddress, true) == 0
						select new {DVRItem = dvrio, InfoItem = ioinfo};
			//update existed data
			tDVRIOCard dvriocard = null;
			List<string>macs = new List<string>();
			IEnumerable<tDVRControl> dvr_ctrls;
			IEnumerable<tDVRSensors> dvr_sens;
			IEnumerable<ControlInfo> info_ctrls = null;
			IEnumerable<SensorInfo> info_sens = null;
			foreach(var item in updates )
			{
				macs.Add( item.DVRItem.MacAddress);
				dvriocard = item.DVRItem;
				if(!item.InfoItem.Equal( dvriocard))
				{
					item.InfoItem.SetEntity(ref dvriocard);
					db.Update<tDVRIOCard>(dvriocard);
					ret |= true;
				}
				dvr_ctrls = dvradd.tDVRControls.Where( ctrl => ctrl.KIOCard == dvriocard.KIOCard);
				dvr_sens = dvradd.tDVRSensors.Where(sen => sen.KIOCard == dvriocard.KIOCard);
				info_ctrls = msgBody.hwData.ControlInfobyCardinfo( item.InfoItem);
				info_sens = msgBody.hwData.SensorInfobyCardinfo(item.InfoItem);
				ret |= UpdateControls(dvradd, dvriocard, dvr_ctrls.ToList(), info_ctrls.ToList());
				ret |= UpdateSensors(dvradd, dvriocard, dvr_sens.ToList(), info_sens.ToList());
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}

			IEqualityComparer<string> icomparer = new StringEqualityComparer(true);
			//delete unused
			IEnumerable<tDVRIOCard> deletes = dvrIOCards.Except(updates.Select(item => item.DVRItem)).ToList();//.Where(io => !macs.Contains(io.MacAddress, icomparer)).ToList();
			foreach(tDVRIOCard delete in deletes)
			{
				DeleteIOCard( dvradd, delete);
				System.Threading.Thread.Sleep(Time_Loop_Delay);
				ret = true;
			}
			//add new IOCard
			IEnumerable<IOCardInfo> newitems = ioCardinfo.Except(updates.Select(item => item.InfoItem));//.Where(ioinfo => !macs.Contains(ioinfo.MACAddress, icomparer));
			foreach( IOCardInfo newitem in newitems)
			{
				dvriocard = null;
				newitem.SetEntity(ref dvriocard);
				dvriocard.tDVRAddressBook = dvradd;
				db.Insert<tDVRIOCard>(dvriocard);

				info_ctrls = msgBody.hwData.ControlInfobyCardinfo(newitem);
				info_sens = msgBody.hwData.SensorInfobyCardinfo(newitem);
				ret |= UpdateControls(dvradd, dvriocard, null, info_ctrls.ToList());
				ret |= UpdateSensors(dvradd, dvriocard, null, info_sens.ToList());

				ret = true;
				System.Threading.Thread.Sleep(Time_Loop_Delay);
			}
			return ret;
		}
		
		private void DeleteIOCard(tDVRAddressBook dvradd, tDVRIOCard dvriocard)
		{
			db.DeleteWhere<tDVRSensors>(item => item.KDVR == dvradd.KDVR && item.KIOCard == dvriocard.KIOCard);
			db.DeleteWhere<tDVRControl>(item => item.KDVR == dvradd.KDVR && item.KIOCard == dvriocard.KIOCard);
			db.Delete<tDVRIOCard>(dvriocard);
		}
		
		private bool UpdateSensors(tDVRAddressBook dvradd, tDVRIOCard dvriocard, ICollection<tDVRSensors>dvrsens, List<SensorInfo> sensors)
		{
			Func<tDVRSensors, SensorInfo, bool> func_filter = (dbitem, info) => dbitem.RealIndex == info.RealIndex;

			Func<tDVRSensors, SensorInfo, bool> compare_update = null;// (dbitem, info) => dbitem.KIOCard == dvriocard.KIOCard;

			List<Expression<Func<tDVRSensors, object>>> update_expr = new List<Expression<Func<tDVRSensors,object>>>();
			List<object> update_data = new List<object>();
			update_expr.Add(dbitem => dbitem.tDVRIOCard);
			update_data.Add(dvriocard);

			update_expr.Add(dbitem => dbitem.KDVR);
			update_data.Add(dvradd.KDVR);

			Expression<Func<tDVRSensors, int>> db_key = dbitem => dbitem.RealIndex.Value;
			Expression<Func<SensorInfo, int>> info_key = info => info.RealIndex;
			return base.UpdateDBData<tDVRSensors, SensorInfo, int, int>(dvrsens, sensors, func_filter, compare_update, update_expr, update_data , db_key, info_key);
		}
		
		private bool UpdateControls(tDVRAddressBook dvradd, tDVRIOCard dvriocard, ICollection<tDVRControl> dvrctrls, List<ControlInfo> controls)
		{
			Func<tDVRControl, ControlInfo, bool> func_filter = (dbitem, info) => dbitem.RealIndex == info.RealIndex;
			Func<tDVRControl, ControlInfo, bool> compare_update = null;// (dbitem, info) =>  dbitem.KIOCard == dvriocard.KIOCard;

			List<Expression<Func<tDVRControl, object>>> update_expr = new List<Expression<Func<tDVRControl, object>>>();
			List<object> update_data = new List<object>();
			update_expr.Add(dbitem => dbitem.tDVRIOCard);
			update_data.Add(dvriocard);

			update_expr.Add(dbitem => dbitem.KDVR);
			update_data.Add(dvradd.KDVR);

			
			Expression<Func<tDVRControl, int>> db_key = dbitem => dbitem.RealIndex.Value;
			Expression<Func<ControlInfo, int>> info_key = info => info.RealIndex;
			return base.UpdateDBData<tDVRControl, ControlInfo, int, int>(dvrctrls, controls, func_filter, compare_update, update_expr, update_data, db_key, info_key);
		}

		#region unused
		/*
		private void SetHwConfig(int iVersion, int iProduct)
		{
			tDVRVersion verInfo = db.FirstOrDefault<tDVRVersion>(x => x.Version == iVersion && x.Product == iProduct);

			tDVRHardware hw = db.FirstOrDefault<tDVRHardware>(item => item.KDVR == DVRAdressBook.KDVR);
			if (hw == null) //insert new
			{
				hw = new tDVRHardware();
				SetHwConfigInfo(DVRAdressBook.KDVR, verInfo, ref hw);
				db.Insert<tDVRHardware>(hw);
			}
			else
			{
				tDVRHardware compare = new tDVRHardware();
				CopyPropertyValues(hw, compare);
				SetHwConfigInfo(DVRAdressBook.KDVR, verInfo, ref hw);
				if (!Compare2Object<tDVRHardware>(hw, compare))
				{
					db.Update<tDVRHardware>(hw);
				}
			}
		}

		private bool IsMonitorData()
		{
			if (!IsHardWareData()) return false;
			return msgBody.hwData.Monitors != null;
		}

		private List<Monitor> GetMonitorDvrList()
		{
			List<Monitor> monitors = IsMonitorData() ? msgBody.hwData.Monitors.ToList() : new List<Monitor>();
			return monitors;
		}

		private void SetMonitorConfig(Int32 kDvr)
		{
			List<tDVRHWDigitalMonitor> lsmonitors = db.Query<tDVRHWDigitalMonitor>(x => x.KDVR == kDvr).ToList();
			List<Monitor> monitors = GetMonitorDvrList();

			List<tDVRHWDigitalMonitor> hwMonitorLst = GetNewDigitalMonitorList(monitors);

			foreach (var m in lsmonitors)
			{
				tDVRHWDigitalMonitor tmonitor = m;
				tDVRHWDigitalMonitor monitorInfo = hwMonitorLst.FirstOrDefault(x => x.KDVR == m.KDVR
				                                                                    && x.MonitorNo == m.MonitorNo
				                                                                    && x.OptName.Trim().ToUpper() ==m.OptName.Trim().ToUpper());
				if (monitorInfo != null)
				{
					if (!CompareDiMonitor(monitorInfo, tmonitor))
					{
						tmonitor= CopyDiMonitor(monitorInfo, tmonitor);
						db.Update<tDVRHWDigitalMonitor>(tmonitor);
					}
					hwMonitorLst.Remove(monitorInfo);
				}
				else
				{
					db.Delete<tDVRHWDigitalMonitor>(tmonitor);
				}
			}

			InsertNewDigitalMonitors(hwMonitorLst);

			//db.Save();
		}

		private List<tDVRHWDigitalMonitor> GetNewDigitalMonitorList(List<Monitor> monitors)
		{
			var hwMonitorLst = new List<tDVRHWDigitalMonitor>();
			foreach (var mo in monitors)
			{
				hwMonitorLst.AddRange(GetHwMonitorList(mo));
			}
			return hwMonitorLst;
		}

		private void InsertNewDigitalMonitors(List<tDVRHWDigitalMonitor> hwMonitorLst)
		{
			foreach (var m in hwMonitorLst)
			{
				var tmonitor = new tDVRHWDigitalMonitor();
				CopyPropertyValues(m, tmonitor);
				db.Insert<tDVRHWDigitalMonitor>(tmonitor);
			}
		}

		private tDVRHWDigitalMonitor CopyDiMonitor(tDVRHWDigitalMonitor soureMonitor, tDVRHWDigitalMonitor denstinationMonitor)
		{
			denstinationMonitor.ChannelMask = soureMonitor.ChannelMask;
			denstinationMonitor.DwellTime = soureMonitor.DwellTime;
			denstinationMonitor.EnableMonitor = soureMonitor.EnableMonitor;
			denstinationMonitor.EnableOpt = soureMonitor.EnableOpt;
			denstinationMonitor.KDVR = soureMonitor.KDVR;
			denstinationMonitor.MonitorNo = soureMonitor.MonitorNo;
			denstinationMonitor.OptName = soureMonitor.OptName;
			denstinationMonitor.Priority = soureMonitor.Priority;
			denstinationMonitor.Stream = soureMonitor.Stream;
			return denstinationMonitor;
		}

		private bool CompareDiMonitor(tDVRHWDigitalMonitor mo1, tDVRHWDigitalMonitor mo2)
		{
			bool result = mo2.ChannelMask == mo1.ChannelMask &&
			              mo2.DwellTime == mo1.DwellTime &&
			              mo2.EnableMonitor == mo1.EnableMonitor &&
			              mo2.EnableOpt == mo1.EnableOpt &&
			              mo2.KDVR == mo1.KDVR &&
			              mo2.MonitorNo == mo1.MonitorNo &&
			              mo2.OptName == mo1.OptName &&
			              mo2.Priority == mo1.Priority &&
			              mo2.Stream == mo1.Stream;
			return result;
		}

		public List<tDVRHWDigitalMonitor> GetHwMonitorList(Monitor monitor)
		{
			var result = new List<tDVRHWDigitalMonitor>();

			tDVRHWDigitalMonitor mo;

			if (monitor.DisplayChannels != null)
			{
				mo = new tDVRHWDigitalMonitor()
				{
					KDVR = DVRAdressBook.KDVR,
					MonitorNo = monitor.id,
					OptName = RawHardwareConfig.STR_MON_Display_Channels,
					EnableOpt = monitor.DisplayChannels.Enable,
					DwellTime = monitor.DisplayChannels.DwellTime,
					Priority = monitor.DisplayChannels.Priority,
					Stream = monitor.DisplayChannels.Stream,
					ChannelMask = monitor.DisplayChannels.ChannelMask,
					EnableMonitor = monitor.Enabled
				};
				result.Add(mo);
			}

			if (monitor.SensorTriggered != null)
			{				
				mo = new tDVRHWDigitalMonitor()
				{
					KDVR = DVRAdressBook.KDVR,
					MonitorNo = monitor.id,
					OptName = RawHardwareConfig.STR_MON_Sensor_Triggered,
					EnableOpt = monitor.SensorTriggered.Enable,
					DwellTime = monitor.SensorTriggered.DwellTime,
					Priority = monitor.SensorTriggered.Priority,
					Stream = monitor.SensorTriggered.Stream,
					ChannelMask = monitor.SensorTriggered.ChannelMask,
					EnableMonitor = monitor.Enabled
				};
				result.Add(mo);
			}

			if (monitor.VideoLogixTriggered != null)
			{
				mo = new tDVRHWDigitalMonitor()
				{
					KDVR = DVRAdressBook.KDVR,
					MonitorNo = monitor.id,
					OptName = RawHardwareConfig.STR_MON_Video_Logix_Triggered,
					EnableOpt = monitor.VideoLogixTriggered.Enable,
					DwellTime = monitor.VideoLogixTriggered.DwellTime,
					Priority = monitor.VideoLogixTriggered.Priority,
					Stream = monitor.VideoLogixTriggered.Stream,
					ChannelMask = monitor.VideoLogixTriggered.ChannelMask,
					EnableMonitor = monitor.Enabled
				};
				result.Add(mo);
			}

			if (monitor.ControlPtzTriggered != null)
			{
				mo = new tDVRHWDigitalMonitor()
				{
					KDVR = DVRAdressBook.KDVR,
					MonitorNo = monitor.id,
					OptName = STR_MON_Control_Ptz_Triggered,
					EnableOpt = monitor.ControlPtzTriggered.Enable,
					DwellTime = monitor.ControlPtzTriggered.DwellTime,
					Priority = monitor.ControlPtzTriggered.Priority,
					Stream = monitor.ControlPtzTriggered.Stream,
					ChannelMask = monitor.ControlPtzTriggered.ChannelMask,
					EnableMonitor = monitor.Enabled
				};
				result.Add(mo);
			}


			if (monitor.MotionTriggered != null)
			{
				mo = new tDVRHWDigitalMonitor()
				{
					KDVR = DVRAdressBook.KDVR,
					MonitorNo = monitor.id,
					OptName = STR_MON_Motion_Triggered,
					EnableOpt = monitor.MotionTriggered.Enable,
					DwellTime = monitor.MotionTriggered.DwellTime,
					Priority = monitor.MotionTriggered.Priority,
					Stream = monitor.MotionTriggered.Stream,
					ChannelMask = monitor.MotionTriggered.ChannelMask,
					EnableMonitor = monitor.Enabled
				};
				result.Add(mo);
			}

			return result;
		}

		private void SetHwConfigInfo(Int32 kDvr, tDVRVersion ver, ref tDVRHardware hw)
		{
			hw.KDVR = kDvr;
			int extmonitor = 0;
			int ioCardscount = 0;
			if (msgBody.hwData.VideoSource != null)
			{
				hw.ExtMonitor = msgBody.hwData.VideoSource.ExtMonitor;
				hw.Dwell = msgBody.hwData.VideoSource.DWell;
				hw.InputMask = msgBody.hwData.VideoSource.InputMask;
				hw.MonitorSensor = msgBody.hwData.VideoSource.MonitorSensor;
				extmonitor = msgBody.hwData.VideoSource.EnExtMonitor;
			}

			if (msgBody.hwData.IOCards != null)
			{
				ioCardscount = msgBody.hwData.IOCards.Count;
			}

			if (msgBody.hwData.HaspInfo != null)
			{
				hw.HPKCode = msgBody.hwData.HaspInfo.HPKCode;
				hw.HPKPAC = msgBody.hwData.HaspInfo.PAC;
				hw.HPKVideoFPS = msgBody.hwData.HaspInfo.VideoFPS;
				hw.HPKAudio = msgBody.hwData.HaspInfo.Audio;
				hw.HPKIPCamera = msgBody.hwData.HaspInfo.IPCamera;
				hw.HPKVideoLogix = msgBody.hwData.HaspInfo.VideoLogix;
				hw.HPKVisionCount = msgBody.hwData.HaspInfo.VisionCount;
				hw.HPKLPR = msgBody.hwData.HaspInfo.LPR;
				hw.HPKVideoLogixBasic = msgBody.hwData.HaspInfo.VideoLogixBasic;
				hw.HPKVisionCountBasic = msgBody.hwData.HaspInfo.VisionCountBasic;
				hw.HPKCMSMode = msgBody.hwData.HaspInfo.CMSMode;
				hw.HPKControlCount = msgBody.hwData.HaspInfo.ControlCount;
				hw.HPKSensorCount = msgBody.hwData.HaspInfo.SensorCount;
				hw.HPKAnalog = msgBody.hwData.HaspInfo.HPKAnalog;
				hw.HPKMonitor = msgBody.hwData.HaspInfo.HPKMonitor;
			}
			if (msgBody.hwData.Sensors != null)
			{
				hw.PreRecordingTime = msgBody.hwData.Sensors.PreRecTime;
				hw.PostRecordingTime = msgBody.hwData.Sensors.PostRecTime;
				hw.EnableAlarm = msgBody.hwData.Sensors.Alarm;
			}

			if (msgBody.hwData.CCInfo != null)
			{
				hw.CCBoardNum = msgBody.hwData.CCInfo.NumOfBoards;
				hw.CCChipNum = msgBody.hwData.CCInfo.NumOfChips;
				hw.CardType = msgBody.hwData.CCInfo.Model;
			}

			if (msgBody.hwData.Channels != null)
			{
				hw.NumPtzType = msgBody.hwData.Channels.NumPTZType;
			}

			hw.KVideoFormat = msgBody.hwData.VideoFormat;
			hw.EnableExtMonitor = (ioCardscount == 0) ? 0 : extmonitor;

		}

		private bool IsComPortData()
		{
			if (!IsHardWareData()) return false;
			return msgBody.hwData.ComPorts != null;
		}

		private List<COMPort> GetComPortDvrList()
		{
			List<COMPort> comPortlst = IsComPortData() ? msgBody.hwData.ComPorts.ToList() : new List<COMPort>();
			return comPortlst;
		}

		private void SetHwPort(Int32 kDvr)
		{
			List<tDVRRS232Ports> lsPorts = db.Query<tDVRRS232Ports>(x => x.KDVR == kDvr).ToList();
			List<COMPort> comPortlst = GetComPortDvrList();

			foreach (var p in lsPorts)
			{
				tDVRRS232Ports port = p;
				COMPort com = comPortlst.FirstOrDefault(x => x.Name.Trim().ToUpper() == port.PortName.Trim().ToUpper());
				if (com != null)
				{
					if (port.KDVR != kDvr ||
					    port.PortName != com.Name)
					{
						port.KDVR = kDvr;
						port.PortName = com.Name;
						db.Update<tDVRRS232Ports>(port);
					}
					comPortlst.Remove(com);
				}
				else
				{
					tDVRSystemInfo sysInfo = db.FirstOrDefault<tDVRSystemInfo>(item => item.KDVR == DVRAdressBook.KDVR);
					if (sysInfo != null)
					{
						sysInfo.KPort = null;
						db.Update<tDVRSystemInfo>(sysInfo);
						db.Delete<tDVRRS232Ports>(port);
					}
				}
			}

			foreach (var pi in comPortlst)
			{
				var port = new tDVRRS232Ports {KDVR = kDvr, PortName = pi.Name};
				db.Insert<tDVRRS232Ports>(port);
			}
			//db.Save();
		}

		private List<SensorInfo> GetSensorList()
		{
			List<SensorInfo> sensorInfoList = IsSensors() ? msgBody.hwData.Sensors.Sensors : new List<SensorInfo>();

			return sensorInfoList;
		}

		private void UpdateHwSensors(Int32 kDvr, Int32 kIo, int ioNo)
		{
			List<tDVRSensor> lsSensors = _lsSensor.Where(x => x.KDVR == kDvr && x.KIOCard == kIo).ToList();
			List<SensorInfo> sensorInfoList = GetSensorList().Where(t => t.IoCardNo == ioNo).ToList();

			foreach (tDVRSensor ss in lsSensors)
			{
				tDVRSensor sensor = ss;
				SensorInfo ssInfo = sensorInfoList.FirstOrDefault(x => x.No == sensor.SensorNo);
				if (ssInfo != null)
				{
					if (!CompareHwSensorInfo(kDvr, kIo, ssInfo, sensor))
					{
						SetHwSensorInfo(kDvr, kIo, ssInfo, ref sensor);
						db.Update<tDVRSensor>(sensor);
					}
					sensorInfoList.Remove(ssInfo);
				}
				else
				{
					db.Delete<tDVRSensor>(sensor);
				}
			}

			foreach (var ssInfo in sensorInfoList)
			{
				var sensor = new tDVRSensor();
				SetHwSensorInfo(kDvr, kIo, ssInfo, ref sensor);
				db.Insert<tDVRSensor>(sensor);
			}
			//db.Save();
		}
		
		private void DeleteHwSensors(Int32 kDvr, Int32 kIo)
		{
			List<tDVRSensor> lsSensors = _lsSensor.Where(x => x.KDVR == kDvr && x.KIOCard == kIo).ToList();
			foreach (var sensor in lsSensors)
			{
				db.Delete<tDVRSensor>(sensor);
			}
			//db.Save();
		}

		#region Check Data from  XML
		private void UpdateHwControls(Int32 kDvr, Int32 kIo, int ioNo)
		{
			List<tDVRControl> lsControls = _lsControl.Where(x => x.KDVR == kDvr && x.KIOCard == kIo).ToList();
			if (msgBody.hwData.Controls == null)
			{
				foreach (var ct in lsControls)
				{
					db.Delete<tDVRControl>(ct);
				}
				//db.Save();
				return;
			}
			List<ControlInfo> controlInfoList = msgBody.hwData.Controls.Where(t=>t.IoCardNo == ioNo).ToList();
			foreach (var ct in lsControls)
			{
				tDVRControl control = ct;
				ControlInfo ctInfo = controlInfoList.FirstOrDefault(x => x.No == control.ControlNo);
				if (ctInfo != null)
				{
					if (!CompareHwControlInfo(kDvr, kIo, ctInfo, control))
					{
						SetHwControlInfo(kDvr, kIo, ctInfo, ref control);
						db.Update<tDVRControl>(control);
					}
					controlInfoList.Remove(ctInfo);
				}
				else
				{
					db.Delete<tDVRControl>(control);
				}
			}

			foreach (var ctInfo in controlInfoList)
			{
				var control = new tDVRControl();
				SetHwControlInfo(kDvr, kIo, ctInfo, ref control);
				db.Insert<tDVRControl>(control);
			}
			//db.Save();
		}

		private void DeleteHwControls(Int32 kDvr, Int32 kIo)
		{
			//Find Control from database
			List<tDVRControl> lsControls = _lsControl.Where(x => x.KDVR == kDvr && x.KIOCard == kIo).ToList();
			foreach (var con in lsControls)
			{
				db.Delete<tDVRControl>(con);
			}
			//db.Save();
		}

		private bool IsHardWareChannel()
		{
			if (!IsHardWareData())
				return false;
			return msgBody.hwData.Channels != null;
		}

		private bool IsChannelInfoData()
		{
			if (!IsHardWareChannel())
				return false;
			return msgBody.hwData.Channels.Channels != null;
		}


		private bool IsBodyData()
		{
			return msgBody != null;
		}

		private bool IsHardWareData()
		{
			if (!IsBodyData()) return false;
			return msgBody.hwData != null;
		}

		private bool IsIoCardData()
		{
			if (!IsHardWareData()) return false;
			return msgBody.hwData.IOCards != null;
		}

		private bool IsSensorCard()
		{
			if (!IsHardWareData()) return false;
			return msgBody.hwData.Sensors != null;
		}

		private bool IsSensors()
		{
			if (IsSensorCard())
			{
				return msgBody.hwData.Sensors.Sensors != null;
			}
			return false;
		}

		private bool IsControls()
		{
			if (IsHardWareData())
			{
				return msgBody.hwData.Controls != null;
			}
			return false;
		}

		private List<IOCardInfo> GetIoCardList()
		{
			List<IOCardInfo> ioCardList = IsIoCardData() ? msgBody.hwData.IOCards.ToList() : new List<IOCardInfo>();
			return ioCardList;
		}

		private void MarkIdControl(IOCardInfo ioCardInfo, ref int idControl)
		{
			if (IsControls())
			{
				List<ControlInfo> controlInfoList = msgBody.hwData.Controls;
				foreach (var ct in controlInfoList)
				{
					if (ct.No >= idControl && ct.No < ioCardInfo.NumberControl + idControl)
						msgBody.hwData.Controls.Where(t => t.No == ct.No).FirstOrDefault().IoCardNo = ioCardInfo.id;
				}
				idControl += ioCardInfo.NumberControl;
			}
		}

		private void MarkIdSensors(IOCardInfo ioCardInfo, ref int idSensor)
		{
			if (IsSensors())
			{
				List<SensorInfo> sensorInfoList = msgBody.hwData.Sensors.Sensors;
				foreach (var ss in sensorInfoList)
				{
					if (ss.No >= idSensor && ss.No < ioCardInfo.NumberSensor + idSensor)
						msgBody.hwData.Sensors.Sensors.Where(t => t.No == ss.No).FirstOrDefault().IoCardNo = ioCardInfo.id;
				}
				idSensor += ioCardInfo.NumberSensor;
			}
		}

		private void MarkIdMessageSensorandControl()
		{
			List<IOCardInfo> iioCardfolst = GetIoCardList();
			int idxSen = 0;
			int idxCon = 0;
			foreach (var ioCard in iioCardfolst.OrderBy(t => t.id))
			{
				MarkIdControl(ioCard, ref idxCon);
				MarkIdSensors(ioCard, ref idxSen);
			}
		}
		#endregion

		private void SetHwioCards(Int32 kDvr)
		{
			_lsIoCards = db.Query<tDVRIOCard>(x => x.KDVR == kDvr).ToList();
			_lsControl = db.Query<tDVRControl>(x => x.KDVR == kDvr).ToList();
			_lsSensor = db.Query<tDVRSensor>(x => x.KDVR == kDvr).ToList();

			MarkIdMessageSensorandControl();
			List<IOCardInfo> iioCardfolst = GetIoCardList();
			foreach (var io in _lsIoCards)
			{
				tDVRIOCard ioCard = io;
				IOCardInfo ioInfo = iioCardfolst.FirstOrDefault(x => x.MACAddress.Trim().ToUpper() == io.MacAddress.Trim().ToUpper());
				if (ioInfo != null)
				{
					if (!CompareIoCardInfo(kDvr, ioInfo, ioCard))
					{
						SetIoCardInfo(kDvr, ioInfo, ref ioCard);
						db.Update<tDVRIOCard>(ioCard);
					}

					UpdateHwControls(kDvr, ioCard.KIOCard, ioInfo.id);
					UpdateHwSensors(kDvr, ioCard.KIOCard, ioInfo.id);
					iioCardfolst.Remove(ioInfo);
				}
				else
				{
					DeleteHwSensors(kDvr, io.KIOCard);
					DeleteHwControls(kDvr, io.KIOCard);
					db.Delete<tDVRIOCard>(ioCard);
				}
			}

			foreach (var ioInfo in iioCardfolst)
			{
				var ioCard = new tDVRIOCard();
				SetIoCardInfo(kDvr, ioInfo, ref ioCard);
				db.Insert<tDVRIOCard>(ioCard);
				db.Save();
				//ioCard = db.Query<tDVRIOCard>(t => t.KDVR == kDvr && String.Equals(t.MacAddress.Trim(), ioInfo.MACAddress.Trim(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
				UpdateHwControls(kDvr, ioCard.KIOCard, ioInfo.id);
				UpdateHwSensors(kDvr, ioCard.KIOCard, ioInfo.id);
			}

		}

		private void SetIoCardInfo(Int32 kDvr,IOCardInfo ioInfo, ref tDVRIOCard iocard)
		{
			iocard.KDVR = kDvr;
			iocard.CardType = ioInfo.CardType;
			iocard.NumberSensors = ioInfo.NumberSensor;
			iocard.NumberControls = ioInfo.NumberControl;
			iocard.MonitorSupport = ioInfo.MonitorSupport != 0;
			iocard.PTZSupport = ioInfo.PTZSupport != 0;
			iocard.WatchDogSupport = ioInfo.WatchdogSupport != 0;
			iocard.COMPortName = ioInfo.ComPort;
			iocard.IPAddress = ioInfo.IP;
			iocard.MacAddress = ioInfo.MACAddress;
			iocard.IOname = ioInfo.IOName;
		}

		private bool CompareIoCardInfo(Int32 kDvr, IOCardInfo ioInfo, tDVRIOCard iocard)
		{
			bool result = iocard.KDVR == kDvr &&
			              iocard.CardType == ioInfo.CardType &&
			              iocard.NumberSensors == ioInfo.NumberSensor &&
			              iocard.NumberControls == ioInfo.NumberControl &&
			              iocard.MonitorSupport == (ioInfo.MonitorSupport != 0) &&
			              iocard.PTZSupport == (ioInfo.PTZSupport != 0) &&
			              iocard.WatchDogSupport == (ioInfo.WatchdogSupport != 0) &&
			              iocard.COMPortName == ioInfo.ComPort &&
			              iocard.IPAddress == ioInfo.IP &&
			              iocard.MacAddress == ioInfo.MACAddress &&
			              iocard.IOname == ioInfo.IOName;
			return result;
		}

		private void SetHwChannels(Int32 kDvr)
		{
			List<tDVRChannel> lsChannels = db.Query<tDVRChannel>(x => x.KDVR == kDvr).ToList();

			if (!IsChannelInfoData()) return;

			List<ChannelInfo> channelinfolst = msgBody.hwData.Channels.Channels;

			foreach (var cn in lsChannels)
			{
				tDVRChannel channel = cn;
				ChannelInfo chan = channelinfolst.FirstOrDefault(x => x.id == channel.ChannelNo);
				if (chan != null)
				{
					if (!CompareHwChannelInfo(kDvr, chan, channel))
					{
						SetHwChannelInfo(kDvr, chan, ref channel);
						db.Update<tDVRChannel>(channel);
					}
					channelinfolst.Remove(chan);
				}
				else
				{
					db.Delete<tDVRChannel>(channel);
				}
			}

			foreach (var cni in channelinfolst)
			{
				var channel = new tDVRChannel();
				SetHwChannelInfo(kDvr, cni, ref channel);
				db.Insert<tDVRChannel>(channel);
			}
			//db.Save();
		}

		private void SetHwChannelInfo(Int32 kDvr, ChannelInfo chaninfo, ref tDVRChannel chan)
		{
			chan.ChannelNo = chaninfo.id;
			chan.KDVR = kDvr;
			chan.VideoSource = chaninfo.VideoSource;
			chan.KAudioSource = chaninfo.AudioSource;
			chan.KPTZ = chaninfo.ptzType;
			chan.Name = chaninfo.Name;
			chan.Enable = chaninfo.Enable;
			chan.DwellTime = chaninfo.DWellTime;
			chan.AP = chaninfo.AP;
			chan.CameraID = chaninfo.CameraID;
			chan.VideoCompressQual = chaninfo.VideoCompQuality;
		}

		private bool CompareHwChannelInfo(Int32 kDvr, ChannelInfo chaninfo, tDVRChannel chan)
		{
			bool result = chan.ChannelNo == chaninfo.id &&
			              chan.KDVR == kDvr &&
			              chan.VideoSource == chaninfo.VideoSource &&
			              chan.KAudioSource == chaninfo.AudioSource &&
			              chan.KPTZ == chaninfo.ptzType &&
			              chan.Name == chaninfo.Name &&
			              chan.Enable == chaninfo.Enable &&
			              chan.DwellTime == chaninfo.DWellTime &&
			              chan.AP == chaninfo.AP &&
			              chan.CameraID == chaninfo.CameraID &&
			              chan.VideoCompressQual == chaninfo.VideoCompQuality;
			return result;
		}

		private void SetHwSensorInfo(Int32 kDvr, int kIoCard, SensorInfo sensor, ref tDVRSensor sen)
		{
			sen.SensorNo = sensor.No;
			sen.KDVR = kDvr;
			sen.KIOCard = kIoCard;
			sen.Enable = sensor.Enable;
			sen.Name = sensor.Name;
			sen.NoNc = sensor.NormalOpen;
			sen.LinkWithChannel = sensor.ChannelMask;
			sen.LinkWithControl = sensor.ControlMask;
			sen.RealIndex = sensor.RealIndex;
		}

		private bool CompareHwSensorInfo(Int32 kDvr, int kIoCard, SensorInfo sensor, tDVRSensor sen)
		{
			bool result = sen.SensorNo == sensor.No &&
			              sen.KDVR == kDvr &&
			              sen.KIOCard == kIoCard &&
			              sen.Enable == sensor.Enable &&
			              sen.Name == sensor.Name &&
			              sen.NoNc == sensor.NormalOpen &&
			              sen.LinkWithChannel == sensor.ChannelMask &&
			              sen.LinkWithControl == sensor.ControlMask &&
			              sen.RealIndex == sensor.RealIndex;
			return result;
		}

		private void SetHwControlInfo(Int32 kDvr, int kIoCard, ControlInfo control, ref tDVRControl con)
		{
			con.KDVR = kDvr;
			con.KIOCard = kIoCard;
			con.EnableControl = control.Enable;
			con.Name = control.Name;
			con.BeginTime = control.BeginTime;
			con.EndTime = control.EndTime;
			con.ControlNo = control.No;
			con.WorkingSec = control.WorkSeconds;
			con.LinkWithChannel = control.ChannelMask;
			con.MotionMask = control.MotionMask;
			con.RealIndex = control.RealIndex;
		}

		private bool CompareHwControlInfo(Int32 kDvr, int kIoCard, ControlInfo control, tDVRControl con)
		{
			bool result = con.KDVR == kDvr &&
			              con.KIOCard == kIoCard &&
			              con.EnableControl == control.Enable &&
			              con.Name == control.Name &&
			              con.BeginTime == control.BeginTime &&
			              con.EndTime == control.EndTime &&
			              con.ControlNo == control.No &&
			              con.WorkingSec == control.WorkSeconds &&
			              con.LinkWithChannel == control.ChannelMask &&
			              con.MotionMask == control.MotionMask &&
			              con.RealIndex == control.RealIndex;
			return result;
		}
		*/
		#endregion unused
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawHardwareBody
	{
		[XmlElement(MessageDefines.STR_Common)]
		public RawMsgCommon msgCommon { get; set; }
		/*
		[XmlElement(MessageDefines.STR_TimeChange)]
		public Int64 TimeChange { get; set; }

		[XmlElement(MessageDefines.STR_Checksum)]
		public Int64 Checksum { get; set; }

		[XmlElement(MessageDefines.STR_DVRTime)]
		public string DVRTime { get; set; }
		public DateTime dtDVRTime
		{
			get
			{
				return DateTime.ParseExact(DVRTime, MessageDefines.STR_DVR_DATETIME_FORMAT, CultureInfo.InvariantCulture);
			}
		}

		[XmlElement(MessageDefines.STR_ConfigID)]
		public Int32 ConfigID { get; set; }

		[XmlElement(MessageDefines.STR_ConfigName)]
		public string ConfigName { get; set; }
		*/

		//Anh Huynh, Update checksum for Pro 3.3, Sept 22, 2014
		[XmlElement(RawHardwareConfig.STR_NextConfigs)]
		public string NextConfigs { get; set; }
		public List<int> lsNextConfigs
		{
			get
			{
				if (String.IsNullOrEmpty(NextConfigs))
					return null;

				string[] arrCfgs = NextConfigs.Split(';');
				List<int> lsCfgs = new List<int>();
				foreach (string cfg in arrCfgs)
				{
					if (!String.IsNullOrEmpty(cfg))
					{
						lsCfgs.Add(Convert.ToInt32(cfg));
					}
				}
				return lsCfgs;
			}
		}

		[XmlElement(RawHardwareConfig.STR_Hardware)]
		public RawHardwareData hwData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }

	}

	[XmlRoot(RawHardwareConfig.STR_Hardware)]
	public class RawHardwareData : IMessageEntity<tDVRHardware>
	{
		[XmlElement(RawHardwareConfig.STR_VideoFormat)]
		public int VideoFormat { get; set; }

		[XmlElement(RawHardwareConfig.STR_CCInfo)]
		public CaptureCardInfo CCInfo { get; set; }

		[XmlArray(RawHardwareConfig.STR_COMPorts)]
		[XmlArrayItem(RawHardwareConfig.STR_COMPort)]
		public List<COMPort> ComPorts = new List<COMPort>();

		[XmlElement(RawHardwareConfig.STR_HaspKey)]
		public HASPKeyInfo HaspInfo { get; set; }

		[XmlArray(RawHardwareConfig.STR_IPCameras)]
		[XmlArrayItem(RawHardwareConfig.STR_IPCamera)]
		public List<IPCameraInfo> IPCameras = new List<IPCameraInfo>();

		[XmlElement(RawHardwareConfig.STR_VSInfo)]
		public VideoSourceInfo VideoSource { get; set; }

		[XmlElement(RawHardwareConfig.STR_Channels)]
		public HWChannels Channels { get; set; }

		[XmlArray(RawHardwareConfig.STR_IOCardInfo)]
		[XmlArrayItem(RawHardwareConfig.STR_IOCard)]
		public List<IOCardInfo> IOCards = new List<IOCardInfo>();

		[XmlArray(RawHardwareConfig.STR_Controls)]
		[XmlArrayItem(RawHardwareConfig.STR_Control)]
		public List<ControlInfo> Controls = new List<ControlInfo>();

		[XmlElement(RawHardwareConfig.STR_Sensors)]
		public HWSensors Sensors { get; set; }

		[XmlArray(RawHardwareConfig.STR_MON_Digital_monitors)]
		[XmlArrayItem(RawHardwareConfig.STR_MON_Monitor)]
		public List<Monitor> Monitors { get; set; }

		[XmlIgnore]
		public bool IgnoredRecodingTime{ get ;set;}
		public IEnumerable<ControlInfo> ControlInfobyCardinfo(IOCardInfo iocardinfo)
		{
			if( IOCards == null || iocardinfo == null || Controls == null)
				return new List<ControlInfo>();
			int skip = 0;

			IEnumerable<IOCardInfo> counts = IOCards.Where( item => item.id < iocardinfo.id);
			counts.ToList().ForEach( item => skip += item.NumberControl);
			return Controls.Skip(skip).Take(iocardinfo.NumberControl);
		}

		public IEnumerable<SensorInfo> SensorInfobyCardinfo(IOCardInfo iocardinfo)
		{
			if (IOCards == null || iocardinfo == null)
				return new List<SensorInfo>();
			int skip = 0;

			IEnumerable<IOCardInfo> counts = IOCards.Where(item => item.id < iocardinfo.id);
			counts.ToList().ForEach(item => skip += item.NumberControl);
			if( Sensors == null || Sensors.Sensors == null)
				return new List<SensorInfo>();

			return Sensors.Sensors.Skip(skip).Take(iocardinfo.NumberControl);
		}

		public bool Equal(tDVRHardware dvrHW)
		{
			if( dvrHW == null)
				return false;

			bool ret = true;
			if (VideoSource != null && Channels != null)
			{
				ret &= dvrHW.ExtMonitor == VideoSource.ExtMonitor;
				ret &= dvrHW.Dwell == VideoSource.DWell;
				ret &= dvrHW.InputMask == VideoSource.InputMask;
				ret &= dvrHW.MonitorSensor == VideoSource.MonitorSensor;
			}

			if (ret == true && HaspInfo != null && Channels != null)
			{
				ret &= dvrHW.HPKCode == HaspInfo.HPKCode;
				ret &= dvrHW.HPKPAC == HaspInfo.PAC;
				ret &= dvrHW.HPKVideoFPS == HaspInfo.VideoFPS;
				ret &= dvrHW.HPKAudio == HaspInfo.Audio;
				ret &= dvrHW.HPKIPCamera == HaspInfo.IPCamera;
				ret &= dvrHW.HPKVideoLogix == HaspInfo.VideoLogix;
				ret &= dvrHW.HPKVisionCount == HaspInfo.VisionCount;
				ret &= dvrHW.HPKLPR == HaspInfo.LPR;
				ret &= dvrHW.HPKVideoLogixBasic == HaspInfo.VideoLogixBasic;
				ret &= dvrHW.HPKVisionCountBasic == HaspInfo.VisionCountBasic;
				ret &= dvrHW.HPKCMSMode == HaspInfo.CMSMode;
				ret &= dvrHW.HPKControlCount == HaspInfo.ControlCount;
				ret &= dvrHW.HPKSensorCount == HaspInfo.SensorCount;
				ret &= dvrHW.HPKAnalog == HaspInfo.HPKAnalog;
				ret &= dvrHW.HPKMonitor == HaspInfo.HPKMonitor;
				ret &= dvrHW.HPKFaceBlur == HaspInfo.HPKFaceBlur;
				ret &= dvrHW.HPKISearch == HaspInfo.HPKISearch;
				ret &= dvrHW.HPKMaxConnection == HaspInfo.HPKMaxConnection;
				ret &= dvrHW.HPKVersion == HaspInfo.HPKVersion;
				ret &= dvrHW.HPKUpgradable == HaspInfo.HPKUpgradable;
				//ret &= dvrHW. == HaspInfo.HPKHeatmap;
			}

			if ( ret == true && Sensors != null)
			{
				if( !IgnoredRecodingTime)
				{
					ret &= dvrHW.PreRecordingTime == Sensors.PreRecTime;
					ret &= dvrHW.PostRecordingTime == Sensors.PostRecTime;
				}
				ret &= dvrHW.EnableAlarm == Sensors.Alarm;
			}

			if (ret == true && CCInfo != null && Channels != null)
			{
				ret &= dvrHW.CCBoardNum == CCInfo.NumOfBoards;
				ret &= dvrHW.CCChipNum == CCInfo.NumOfChips;
				ret &= dvrHW.CardType == CCInfo.Model;
			}

			if (ret == true && Channels != null)
			{
				ret &= dvrHW.NumPtzType == Channels.NumPTZType;
				ret &= dvrHW.KVideoFormat == VideoFormat;
				//int IOCount = IOCards == null ? 0 : IOCards.Count;
				ret &= dvrHW.EnableExtMonitor == (IOCards.Count == 0 ? 0 : (VideoSource == null ? 0 : VideoSource.EnExtMonitor));
			}
			return ret;
		}
		
		public void SetEntity(ref tDVRHardware dvrHW)
		{
			if (VideoSource != null && Channels != null)
			{
				dvrHW.ExtMonitor = VideoSource.ExtMonitor;
				dvrHW.Dwell = VideoSource.DWell;
				dvrHW.InputMask = VideoSource.InputMask;
				dvrHW.MonitorSensor = VideoSource.MonitorSensor;
			}

			if (HaspInfo != null && Channels != null)
			{
				dvrHW.HPKCode = HaspInfo.HPKCode;
				dvrHW.HPKPAC = HaspInfo.PAC;
				dvrHW.HPKVideoFPS = HaspInfo.VideoFPS;
				dvrHW.HPKAudio = HaspInfo.Audio;
				dvrHW.HPKIPCamera = HaspInfo.IPCamera;
				dvrHW.HPKVideoLogix = HaspInfo.VideoLogix;
				dvrHW.HPKVisionCount = HaspInfo.VisionCount;
				dvrHW.HPKLPR = HaspInfo.LPR;
				dvrHW.HPKVideoLogixBasic = HaspInfo.VideoLogixBasic;
				dvrHW.HPKVisionCountBasic = HaspInfo.VisionCountBasic;
				dvrHW.HPKCMSMode = HaspInfo.CMSMode;
				dvrHW.HPKControlCount = HaspInfo.ControlCount;
				dvrHW.HPKSensorCount = HaspInfo.SensorCount;
				dvrHW.HPKAnalog = HaspInfo.HPKAnalog;
				dvrHW.HPKMonitor = HaspInfo.HPKMonitor;
				dvrHW.HPKFaceBlur = HaspInfo.HPKFaceBlur;
				dvrHW.HPKISearch = HaspInfo.HPKISearch;
				dvrHW.HPKMaxConnection = HaspInfo.HPKMaxConnection;
				dvrHW.HPKVersion = HaspInfo.HPKVersion;
				dvrHW.HPKUpgradable = HaspInfo.HPKUpgradable;
				//dvrHW. = HaspInfo.HPKHeatmap;
			}

			if (Sensors != null)
			{
				if (!IgnoredRecodingTime)
				{
					dvrHW.PreRecordingTime = Sensors.PreRecTime;
					dvrHW.PostRecordingTime = Sensors.PostRecTime;
				}
				dvrHW.EnableAlarm = Sensors.Alarm;
			}

			if (CCInfo != null && Channels != null)
			{
				dvrHW.CCBoardNum = CCInfo.NumOfBoards;
				dvrHW.CCChipNum = CCInfo.NumOfChips;
				dvrHW.CardType = CCInfo.Model;
			}

			if (Channels != null)
			{
				dvrHW.NumPtzType = Channels.NumPTZType;
				dvrHW.KVideoFormat = VideoFormat;
				//int IOCount = IOCards == null ? 0 : IOCards.Count;

				dvrHW.EnableExtMonitor = IOCards.Count == 0 ? 0 : (VideoSource == null ? 0 : VideoSource.EnExtMonitor); //(ioCardscount == 0) ? 0 : extmonitor;
			}
		}
	}

	[XmlRoot(RawHardwareConfig.STR_CCInfo)]
	public class CaptureCardInfo
	{
		[XmlElement(RawHardwareConfig.STR_Model)]
		public int Model { get; set; }

		[XmlElement(RawHardwareConfig.STR_NumOfBoards)]
		public int NumOfBoards { get; set; }

		[XmlElement(RawHardwareConfig.STR_NumOfChips)]
		public int NumOfChips { get; set; }
	}

	[XmlRoot(RawHardwareConfig.STR_COMPort)]
	public class COMPort
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlText]
		public string Name { get; set; }
	}

	[XmlRoot(RawHardwareConfig.STR_HaspKey)]
	public class HASPKeyInfo
	{
		[XmlElement(RawHardwareConfig.STR_HPKCode)]
		public string _HPKCode { get; set; }

		public Int32 HPKCode {
			get{
				if(String.IsNullOrEmpty(_HPKCode))
				{
					return 0;
				}
				else return Int32.Parse(_HPKCode);

			}
			set { _HPKCode = value.ToString(); }
		}

		[XmlElement(RawHardwareConfig.STR_PAC)]
		public Int32 PAC { get; set; }

		[XmlElement(RawHardwareConfig.STR_VideoFPS)]
		public Int32 VideoFPS { get; set; }

		[XmlElement(RawHardwareConfig.STR_Audio)]
		public Int32 Audio { get; set; }

		[XmlElement(RawHardwareConfig.STR_IPCamera)]
		public Int32 IPCamera { get; set; }

		[XmlElement(RawHardwareConfig.STR_VideoLogixBasic)]
		public Int32 VideoLogixBasic { get; set; }

		[XmlElement(RawHardwareConfig.STR_VisionCountBasic)]
		public Int32 VisionCountBasic { get; set; }

		[XmlElement(RawHardwareConfig.STR_VideoLogix)]
		public Int32 VideoLogix { get; set; }

		[XmlElement(RawHardwareConfig.STR_VisionCount)]
		public Int32 VisionCount { get; set; }

		[XmlElement(RawHardwareConfig.STR_LPR)]
		public Int32 LPR { get; set; }

		[XmlElement(RawHardwareConfig.STR_CMSMode)]
		public Int32 CMSMode { get; set; }

		[XmlElement(RawHardwareConfig.STR_ControlCount)]
		public Int32 ControlCount { get; set; }

		[XmlElement(RawHardwareConfig.STR_SensorCount)]
		public Int32 SensorCount { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_Analog)]
		public Int32 HPKAnalog { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_Monitor)]
		public Int32 HPKMonitor { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_FaceBlur)]
		public Int32 HPKFaceBlur { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_ISearch)]
		public Int32 HPKISearch { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_MaxConnection)]
		public Int32 HPKMaxConnection { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_Version)]
		public Int32 HPKVersion { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_Upgradable)]
		public Int32 HPKUpgradable { get; set; }

		[XmlElement(RawHardwareConfig.STR_HPK_Heatmap)]
		public Int32 HPKHeatmap { get; set; }
	}

	[XmlRoot(RawHardwareConfig.STR_IPCamera)]
	public class IPCameraInfo
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlText]
		public string Name { get; set; }
	}

	[XmlRoot(RawHardwareConfig.STR_VSInfo)]
	public class VideoSourceInfo
	{
		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlElement(RawHardwareConfig.STR_ExtMonitor)]
		public Int32 ExtMonitor { get; set; }

		[XmlElement(RawHardwareConfig.STR_DWell)]
		public Int32 DWell { get; set; }

		[XmlElement(RawHardwareConfig.STR_InputMask)]
		public Int32 InputMask { get; set; }

		[XmlElement(RawHardwareConfig.STR_EnExtMonitor)]
		public Int32 EnExtMonitor { get; set; }

		[XmlElement(RawHardwareConfig.STR_MonitorSensor)]
		public Int32 MonitorSensor { get; set; }
	}

	[XmlRoot(RawHardwareConfig.STR_Channels)]
	public class HWChannels
	{
		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_NumVideoInput)]
		public Int32 NumVideoInput { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_NumPTZType)]
		public Int32 NumPTZType { get; set; }

		[XmlElement(RawHardwareConfig.STR_Channel)]
		public List<ChannelInfo> Channels = new List<ChannelInfo>();
	}

	[XmlRoot(RawHardwareConfig.STR_Channel)]
	public class ChannelInfo : IMessageEntity<tDVRChannels> 
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawHardwareConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawHardwareConfig.STR_CameraID)]
		public Int32 CameraID { get; set; }

		[XmlElement(RawHardwareConfig.STR_DWellTime)]
		public Int32 DWellTime { get; set; }

		[XmlElement(RawHardwareConfig.STR_AP)]
		public Int32 AP { get; set; }

		[XmlElement(RawHardwareConfig.STR_ptzType)]
		public Int32 ptzType { get; set; }

		[XmlElement(RawHardwareConfig.STR_VideoSource)]
		public Int32 VideoSource { get; set; }

		[XmlElement(RawHardwareConfig.STR_AudioSource)]
		public Int32 AudioSource { get; set; }

		[XmlElement(RawHardwareConfig.STR_VideoCompQuality)]
		public Int32 VideoCompQuality { get; set; }

		[XmlElement(RawHardwareConfig.STR_EnableiSearch)]
		public bool EnableiSearch { get; set; }

		public bool Equal(tDVRChannels dvrchanel)
		{
			if( dvrchanel == null)
				return false;
			return dvrchanel.ChannelNo == id &&
							dvrchanel.VideoSource == VideoSource 
							&& dvrchanel.KAudioSource == AudioSource 
							&& dvrchanel.KPTZ == ptzType 
							&& string.Compare( dvrchanel.Name,Name, false ) == 0
							&& dvrchanel.Enable == Enable
							&& dvrchanel.DwellTime == DWellTime
							&& dvrchanel.AP == AP
							&& dvrchanel.CameraID == CameraID
							&& dvrchanel.VideoCompressQual == VideoCompQuality
							&& dvrchanel.EnableiSearch == EnableiSearch;
		}
		public void SetEntity( ref tDVRChannels channel)
		{
			if(channel == null)
				channel = new tDVRChannels();
			channel.ChannelNo = id;
			channel.VideoSource = VideoSource;
			channel.KAudioSource = AudioSource;
			channel.KPTZ = ptzType;
			channel.Name = Name;
			channel.Enable = Enable;
			channel.DwellTime = DWellTime;
			channel.AP = AP;
			channel.CameraID = CameraID;
			channel.VideoCompressQual = VideoCompQuality;
			channel.EnableiSearch = EnableiSearch;
		}
	}

	[XmlRoot(RawHardwareConfig.STR_IOCard)]
	public class IOCardInfo : IMessageEntity<tDVRIOCard>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlElement(RawHardwareConfig.STR_CardType)]
		public Int32 CardType { get; set; }

		[XmlElement(RawHardwareConfig.STR_NumberControl)]
		public Int32 NumberControl { get; set; }

		[XmlElement(RawHardwareConfig.STR_NumberSensor)]
		public Int32 NumberSensor { get; set; }

		[XmlElement(RawHardwareConfig.STR_MonitorSupport)]
		public Int32 MonitorSupport { get; set; }

		[XmlElement(RawHardwareConfig.STR_PTZSupport)]
		public Int32 PTZSupport { get; set; }

		[XmlElement(RawHardwareConfig.STR_WatchdogSupport)]
		public Int32 WatchdogSupport { get; set; }

		[XmlElement(RawHardwareConfig.STR_ComPort)]
		public string ComPort { get; set; }

		[XmlElement(RawHardwareConfig.STR_ip)]
		public string IP { get; set; }

		[XmlElement(RawHardwareConfig.STR_MACAddress)]
		public string MACAddress { get; set; }

		[XmlElement(RawHardwareConfig.STR_IOName)]
		public string IOName { get; set; }

		public void SetEntity(ref tDVRIOCard IOCard)
		{
			if (IOCard == null)
				IOCard = new tDVRIOCard();

			IOCard.CardType = CardType;
			IOCard.NumberSensors = NumberSensor;
			IOCard.NumberControls = NumberControl;
			IOCard.MonitorSupport = MonitorSupport != 0;
			IOCard.PTZSupport = PTZSupport != 0;
			IOCard.WatchDogSupport = WatchdogSupport != 0;
			IOCard.COMPortName = ComPort;
			IOCard.IPAddress = IP;
			IOCard.MacAddress = MACAddress;
			IOCard.IOname = IOName;
		}

		public bool Equal(tDVRIOCard IOCard)
		{
			if(IOCard == null )
				return false;

			return IOCard.CardType == CardType
					&& IOCard.NumberSensors == NumberSensor
					&& IOCard.NumberControls == NumberControl
					&& IOCard.MonitorSupport == (MonitorSupport != 0)
					&& IOCard.PTZSupport == (PTZSupport != 0)
					&& IOCard.WatchDogSupport == (WatchdogSupport != 0)
					&& IOCard.COMPortName == ComPort
					&& IOCard.IPAddress == IP
					&& string.Compare(IOCard.MacAddress,MACAddress, true) == 0
					&& string.Compare(IOCard.IOname,IOName, false) == 0;
		}

	}

	[XmlRoot(RawHardwareConfig.STR_Control)]
	public class ControlInfo: IMessageEntity<tDVRControl> 
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_No)]
		public Int32 No { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawHardwareConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawHardwareConfig.STR_WorkSeconds)]
		public Int32 WorkSeconds { get; set; }

		[XmlElement(RawHardwareConfig.STR_BeginTime)]
		public Int32 BeginTime { get; set; }

		[XmlElement(RawHardwareConfig.STR_EndTime)]
		public Int32 EndTime { get; set; }

		[XmlElement(RawHardwareConfig.STR_ChannelMask)]
		public UInt64 ChannelMask { get; set; }

		[XmlElement(RawHardwareConfig.STR_MotionMask)]
		public UInt64 MotionMask { get; set; }

		[XmlElement(RawHardwareConfig.STR_RealIndex)]
		public Int32 RealIndex { get; set; }

		[XmlElement(RawHardwareConfig.STR_ControlType)]
		public Int32 ControlType { get; set; }

		[XmlIgnore]
		public Int32 IoCardNo { get; set; }

		public bool Equal(tDVRControl dvrcontrol)
		{
			if( dvrcontrol == null)
				return false;

			return
					dvrcontrol.EnableControl == Enable
					&&  string.Compare( dvrcontrol.Name, Name, false) == 0
					&& dvrcontrol.BeginTime == BeginTime
					&& dvrcontrol.EndTime == EndTime
					&& dvrcontrol.ControlNo == No
					&& dvrcontrol.WorkingSec == WorkSeconds
					&& dvrcontrol.LinkWithChannel == ChannelMask
					&& dvrcontrol.MotionMask == MotionMask
					&& dvrcontrol.RealIndex == RealIndex
					&& dvrcontrol.ControlType == ControlType;
		}
		
		public void SetEntity(ref tDVRControl dvrcontrol)
		{
			if (dvrcontrol == null)
				dvrcontrol = new tDVRControl();

			dvrcontrol.EnableControl = Enable;
			dvrcontrol.Name = Name;
			dvrcontrol.BeginTime = BeginTime;
			dvrcontrol.EndTime = EndTime;
			dvrcontrol.ControlNo = No;
			dvrcontrol.WorkingSec = WorkSeconds;
			dvrcontrol.LinkWithChannel = ChannelMask;
			dvrcontrol.MotionMask = MotionMask;
			dvrcontrol.RealIndex = RealIndex;
			dvrcontrol.ControlType = ControlType;
		}
	}

	[XmlRoot(RawHardwareConfig.STR_Sensors)]
	public class HWSensors
	{
		[XmlAttribute(RawHardwareConfig.STR_Alarm)]
		public Int32 Alarm { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_PreRecTime)]
		public Int32 PreRecTime { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_PostRecTime)]
		public Int32 PostRecTime { get; set; }

		[XmlAttribute(MessageDefines.STR_Size)]
		public Int32 Size { get; set; }

		[XmlElement(RawHardwareConfig.STR_Sensor)]
		public List<SensorInfo> Sensors = new List<SensorInfo>();
	}

	[XmlRoot(RawHardwareConfig.STR_Sensor)]
	public class SensorInfo : IMessageEntity<tDVRSensors>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get; set; }

		[XmlAttribute(RawHardwareConfig.STR_No)]
		public Int32 No { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawHardwareConfig.STR_Enable)]
		public Int32 Enable { get; set; }

		[XmlElement(RawHardwareConfig.STR_NormalOpen)]
		public Int32 NormalOpen { get; set; }

		[XmlElement(RawHardwareConfig.STR_ChannelMask)]
		public UInt64 ChannelMask { get; set; }

		[XmlElement(RawHardwareConfig.STR_ControlMask)]
		public UInt64 ControlMask { get; set; }

		[XmlElement(RawHardwareConfig.STR_RealIndex)]
		public string RealId { get; set; }

		public Int32 RealIndex {
			get
			{
				if (!string.IsNullOrEmpty(RealId))
					return Convert.ToInt32(RealId);
				else
				{
					return No;
				}
			}
			set { RealId = value.ToString(); }
		}

		[XmlElement(RawHardwareConfig.STR_SensorType)]
		public Int32 SensorType { get; set; }

		[XmlIgnore]
		public Int32 IoCardNo { get; set; }

		public void SetEntity(ref tDVRSensors sensor)
		{
			if( sensor == null)
				sensor = new tDVRSensors();

			sensor.SensorNo = No;
			sensor.Enable = Enable;
			sensor.Name = Name;
			sensor.NoNc = NormalOpen;
			sensor.LinkWithChannel = ChannelMask;
			sensor.LinkWithControl = ControlMask;
			sensor.RealIndex = RealIndex;
			sensor.SensorType = SensorType;
		}

		public bool Equal(tDVRSensors sensor)
		{
			if( sensor == null)
				return false;

			return sensor.SensorNo == No 
					&& sensor.Enable == Enable 
					&& string.Compare(sensor.Name, Name, false) == 0 
					&& sensor.NoNc == NormalOpen 
					&& sensor.LinkWithChannel == ChannelMask
					&& sensor.LinkWithControl == ControlMask
					&& sensor.RealIndex == RealIndex
					&& sensor.SensorType == SensorType;
		}
	}
	#endregion
}
