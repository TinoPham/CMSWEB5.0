using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.HardwareConfig
{
	[Serializable]
	public class HardwareMonitor : IMessageEntity<tDVRDigitalMonitorOption> //Anh Huynh, Update for new Digital Monitor structure, Sept 22, 2014
	{

		[XmlElement(RawHardwareConfig.STR_MON_Enabled)]
		public int Enable { get; set; }

		[XmlElement(RawHardwareConfig.STR_MON_ChannelMask)]
		public UInt64 ChannelMask { get; set; }

		[XmlArray(RawHardwareConfig.STR_MON_Channels)]
		[XmlArrayItem(RawHardwareConfig.STR_MON_Channel)]
		public List<int> Channels { get; set; }

		[XmlElement(RawHardwareConfig.STR_MON_DwellTime)]
		public Int32 DwellTime { get; set; }

		[XmlElement(RawHardwareConfig.STR_MON_Priority)]
		public Int32 Priority { get; set; }

		[XmlElement(RawHardwareConfig.STR_Stream)]
		public Int32 Stream { get; set; }

		[XmlElement(RawHardwareConfig.STR_DisplayLayout)]
		public Int32 DisplayLayout { get; set; }

		[XmlIgnore]
		public string OptName { get; set; }

		[XmlIgnore]
		public bool EnableMonitor { get; set; }

		[XmlIgnore]
		public int id { get; set; }

		//Anh Huynh, Update for new Digital Monitor structure, Sept 22, 2014
		public void SetEntity(ref tDVRDigitalMonitorOption dvrMonitorOpt)
		{
			if (dvrMonitorOpt == null)
				dvrMonitorOpt = new tDVRDigitalMonitorOption();

			dvrMonitorOpt.Enable = Enable;
			//dvrMonitorOpt.ChannelMask = ChannelMask;
			dvrMonitorOpt.DwellTime = DwellTime;

			dvrMonitorOpt.Priority = Priority;
			dvrMonitorOpt.Stream = Stream;
			dvrMonitorOpt.DisplayLayout = DisplayLayout;
			//dvrMonitorOpt.MonitorNo = id;
			//dvrMonitorOpt.EnableMonitor = EnableMonitor;
			dvrMonitorOpt.OptName = OptName;
		}
		public bool Equal(tDVRDigitalMonitorOption dvrMonitorOpt)
		{
			bool bRet = Enable == dvrMonitorOpt.Enable
						//&& ChannelMask == dvrHWMonitor.ChannelMask
						&& DwellTime == dvrMonitorOpt.DwellTime
						&& Priority == dvrMonitorOpt.Priority
						&& Stream == dvrMonitorOpt.Stream
						&& DisplayLayout == dvrMonitorOpt.DisplayLayout
						//&& id == dvrHWMonitor.MonitorNo
						//&& EnableMonitor == dvrHWMonitor.EnableMonitor
						&& string.Compare(OptName, dvrMonitorOpt.OptName, true) == 0;
			//for()
			return bRet;
		}
	}
}
