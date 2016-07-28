using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Data.Entity;
using PACDMModel.Model;
using SVRDatabase;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawDVRPerformance : RawDVRConfig<RawPerformanceBody>
	{
		#region Parameter

		public const string STR_Performance = "dvr_performance";
		public const string STR_PhysicalMemory = "physical_memory";
		public const string STR_VirtualMemory = "virtual_memory";
		public const string STR_CPUUsage = "cpu_usage";
		public const string STR_SysMemory = "system_memory";
		public const string STR_SysCPUUsage = "system_cpu_usage";
		public const string STR_DateTime = "date_time";
		public const string STR_GMTTime = "gmt_time";
		public const string STR_TimeZoneName = "time_zone_name";
		public const string STR_TimeZoneBias = "time_zone_bias";

		private const int SYSTEM_TYPE_DVR = 1;
		private const string DATE_FORMAT = "MM/dd/yyyy HH:mm:ss";
		#endregion

		public RawDVRPerformance() { }

		public RawDVRPerformance(string strMsg)
		{
			RawDVRPerformance rw = Commons.ObjectUtils.DeSerialize(typeof(RawDVRPerformance), strMsg) as RawDVRPerformance;
			this.msgHeader = rw.msgHeader;
			this.msgBody = rw.msgBody;
		}

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.pfData == null)
				return await base.UpdateToDB();

			if (UpdatePerformance(DVRAdressBook, msgBody.pfData))
			{
				int result;
				if ((result = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(result == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}
			return await base.UpdateToDB();
		}

		private bool UpdatePerformance(tDVRAddressBook dvrAddressBook, PerformanceData pData)
		{
			var dvrStatus = new tCMSSystemStatus();
			SetPerformanceData(dvrAddressBook.KDVR, ref dvrStatus, pData);
			db.Insert<tCMSSystemStatus>(dvrStatus);
			tTimeZone tzone = db.FirstOrDefault<tTimeZone>(x => x.KSystem == DVRAdressBook.KDVR && x.SystemType == SYSTEM_TYPE_DVR);
			if (tzone == null)
			{
				tzone = new tTimeZone() {KSystem =  dvrAddressBook.KDVR};
				pData.SetEntity(ref tzone);
				db.Insert<tTimeZone>(tzone);
			}
			else
			{
				if (!pData.Equal(tzone))
				{
					pData.SetEntity(ref tzone);
					db.Update<tTimeZone>(tzone);
				}
			}
			return true;
		}


		private void SetPerformanceData(Int32 kDVR, ref tCMSSystemStatus dvrPerf, PerformanceData pData)
		{
			dvrPerf.KSystem = kDVR;
			dvrPerf.PhysicalMem = pData.PhysicalMemory;
			dvrPerf.VirtualMem = pData.VirtualMemory;
			dvrPerf.CPU = pData.CPUUsage;
			dvrPerf.TotalSystemMem = pData.SysMemory;
			dvrPerf.TotalSystemCPU = pData.SysCPUUsage;
			dvrPerf.TimeStamp = ToDateTime(pData.GMTTime, DATE_FORMAT);
			dvrPerf.SystemType = SYSTEM_TYPE_DVR;
		}
	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawPerformanceBody
	{
		[XmlElement(RawDVRPerformance.STR_Performance)]
		public PerformanceData pfData { get; set; }
	}

	[XmlRoot(RawDVRPerformance.STR_Performance)]
	public class PerformanceData : IMessageEntity<tTimeZone>
	{
		private const int SYSTEM_TYPE_DVR = 1;

		[XmlElement(RawDVRPerformance.STR_PhysicalMemory)]
		public Int32 PhysicalMemory { get; set; }

		[XmlElement(RawDVRPerformance.STR_VirtualMemory)]
		public Int32 VirtualMemory { get; set; }

		[XmlElement(RawDVRPerformance.STR_CPUUsage)]
		public Int32 CPUUsage { get; set; }

		[XmlElement(RawDVRPerformance.STR_SysMemory)]
		public Int32 SysMemory { get; set; }

		[XmlElement(RawDVRPerformance.STR_SysCPUUsage)]
		public Int32 SysCPUUsage { get; set; }

		[XmlElement(RawDVRPerformance.STR_DateTime)]
		public Int64 DateTime { get; set; }

		[XmlElement(RawDVRPerformance.STR_GMTTime)]
		public string GMTTime { get; set; }

		[XmlElement(RawDVRPerformance.STR_TimeZoneName)]
		public string TimeZoneName { get; set; }

		[XmlElement(RawDVRPerformance.STR_TimeZoneBias)]
		public Int32 TimeZoneBias { get; set; }

		public bool Equal(tTimeZone value)
		{
			bool result = 
						  value.TimeZoneName == TimeZoneName &&
						  value.TimeZoneOffset == TimeZoneBias &&
						  value.SystemType == SYSTEM_TYPE_DVR;
			return result;
		}

		public void SetEntity(ref tTimeZone value)
		{
			if (value == null)
				value = new tTimeZone();
			value.TimeZoneName = TimeZoneName;
			value.TimeZoneOffset = TimeZoneBias;
			value.SystemType = SYSTEM_TYPE_DVR;
		}
	}
	#endregion
}
