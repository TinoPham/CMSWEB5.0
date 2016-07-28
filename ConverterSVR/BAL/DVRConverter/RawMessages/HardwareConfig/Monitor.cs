using PACDMModel.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.HardwareConfig
{
	[XmlRoot(RawHardwareConfig.STR_MON_Monitor)]
	public class Monitor
	{
		
		Int32 _id;
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 id { get { return _id; } set{ _id = value; } }

		bool _Enable;
		[XmlAttribute(RawHardwareConfig.STR_MON_Enabled)]
		public bool Enabled { get { return _Enable;} set { _Enable = value;} }

		//Anh Huynh, Update for new Digital Monitor structure, Sept 22, 2014
		bool _ShowInfo;
		[XmlAttribute(RawHardwareConfig.STR_MON_ShowInfo)]
		public bool ShowInfo { get { return _ShowInfo; } set { _ShowInfo = value; } }

		HardwareMonitor _DisplayChannels;
		[XmlElement(RawHardwareConfig.STR_MON_Display_Channels)]
		public HardwareMonitor DisplayChannels { 
												get{ return _DisplayChannels;} 
												set{ _DisplayChannels = value; 
													 _DisplayChannels.OptName = RawHardwareConfig.STR_MON_Display_Channels;
													 UpdateMonitorData(ref _DisplayChannels);
													_HWMonitors.Add(_DisplayChannels);
													} 
												}

		HardwareMonitor _SensorTriggered;
		[XmlElement(RawHardwareConfig.STR_MON_Sensor_Triggered)]
		public HardwareMonitor SensorTriggered { 
												get{ return _SensorTriggered;} 
												set{ _SensorTriggered = value;
													_SensorTriggered.OptName = RawHardwareConfig.STR_MON_Sensor_Triggered;
													UpdateMonitorData(ref _SensorTriggered);
													_HWMonitors.Add(_SensorTriggered);
													} 
												}

		HardwareMonitor _VideoLogixTriggered;
		[XmlElement(RawHardwareConfig.STR_MON_Video_Logix_Triggered)]
		public HardwareMonitor VideoLogixTriggered {
													get{return _VideoLogixTriggered; } 
													set{ _VideoLogixTriggered = value;
														_VideoLogixTriggered.OptName = RawHardwareConfig.STR_MON_Video_Logix_Triggered;
														UpdateMonitorData(ref _VideoLogixTriggered);
														_HWMonitors.Add(_VideoLogixTriggered);
														} 
													}

		HardwareMonitor _ControlPtzTriggered;
		[XmlElement(RawHardwareConfig.STR_MON_Control_Ptz_Triggered)]
		public HardwareMonitor ControlPtzTriggered { 
													get{ return _ControlPtzTriggered;} 
													set{ 
														_ControlPtzTriggered = value;
														_ControlPtzTriggered.OptName = RawHardwareConfig.STR_MON_Control_Ptz_Triggered;
														UpdateMonitorData(ref _ControlPtzTriggered);
														_HWMonitors.Add(_ControlPtzTriggered);
														} 
													}

		HardwareMonitor _MotionTriggered;
		[XmlElement(RawHardwareConfig.STR_MON_Motion_Triggered)]
		public HardwareMonitor MotionTriggered { 
												get{ return _MotionTriggered; } 
												set{ _MotionTriggered = value;
													_MotionTriggered.OptName = RawHardwareConfig.STR_MON_Motion_Triggered;
													UpdateMonitorData(ref _MotionTriggered);
													_HWMonitors.Add(_MotionTriggered);
													} 
												}

		private List<HardwareMonitor> _HWMonitors = new List<HardwareMonitor>();
		[XmlIgnore]
		public List<HardwareMonitor> HWMonitors {
			get
			{
				return _HWMonitors;
			}
			set
			{
				_HWMonitors = value;
			}
		}

		//public Monitor()
		//{
		//	UpdateHWList();
		//}
		public void UpdateHWList()
		{
			_HWMonitors = new List<HardwareMonitor>();
			if( _DisplayChannels != null)
				_HWMonitors.Add(_DisplayChannels);

			if( _SensorTriggered != null )
				_HWMonitors.Add(_SensorTriggered);

			if( _VideoLogixTriggered != null)
				_HWMonitors.Add(_VideoLogixTriggered);

			if(	_ControlPtzTriggered != null )
				_HWMonitors.Add(_ControlPtzTriggered);

			if( _MotionTriggered != null)
				_HWMonitors.Add(_MotionTriggered);
		}
		private void UpdateMonitorData( ref HardwareMonitor modata)
		{
			if( modata == null)
				return;
			modata.id = _id;
			modata.EnableMonitor = _Enable;

		}
	}
}
