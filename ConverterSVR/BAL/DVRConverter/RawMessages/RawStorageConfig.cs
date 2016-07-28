using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PACDMModel.Model;

namespace ConverterSVR.BAL.DVRConverter
{
	[XmlRoot(MessageDefines.STR_Message)]
	public class RawStorageConfig : RawDVRConfig<RawStorageBody>
	{
		#region parameter
		public const string STR_Storage = "storage";
		public const string STR_OverWrite = "over_write";
		public const string STR_RecordMode = "record_mode";
		public const string STR_Drivers = "drivers";
		public const string STR_Driver = "driver";
		//public const string STR_Name = "name";
		public const string STR_VolumeName = "volume_name";
		public const string STR_DriveType = "drive_type";
		public const string STR_UseRecording = "use_for_recording";
		public const string STR_TotalSpace = "total_space";
		public const string STR_FreeSpace = "free_space";
		public const string STR_ChannelMask = "channel_mask";
		public const string STR_NumRecordDays = "num_record_days";
		public const string STR_RecordedTime = "recorded_time";
		public const string STR_Begin = "begin";
		public const string STR_End = "end";
		#endregion

		public override async Task<Commons.ERROR_CODE> UpdateToDB()
		{
			if (DVRAdressBook == null || msgBody.storageData == null)
				return await base.UpdateToDB();
		
			if (UpdateStorageConfig(DVRAdressBook, msgBody.storageData))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			if (UpdateChecksum((int)DVR_CONFIGURATION.EMS_CFG_STORAGE, msgBody.msgCommon))
			{
				int ret = 0;
				if ((ret = db.Save()) == -1)
					return await Task.FromResult<Commons.ERROR_CODE>(ret == -1 ? Commons.ERROR_CODE.DB_UPDATE_DATA_FAILED : Commons.ERROR_CODE.OK);
			}

			return await base.UpdateToDB();
		}

		private bool UpdateStorageConfig(tDVRAddressBook dvrAdressBook, StorageData storageData)
		{
			bool ret = SetStorageConfig(dvrAdressBook, storageData);
			if (storageData.Drivers != null)
				ret |= UpdateDrivers(DVRAdressBook, storageData.Drivers);
			return ret;
		}

		private bool UpdateDrivers(tDVRAddressBook dvrAdressBook, List<DriverInfo> drivers)
		{
			db.Include<tDVRAddressBook, tDVRStorages>(dvrAdressBook, item => item.tDVRStorages);
			Func<tDVRStorages, DriverInfo, bool> func_filter = (dbitem, info) => dbitem.Drive.Trim() == info.Name.Trim();
			Func<tDVRStorages, DriverInfo, bool> compare_update = null;
			Expression<Func<tDVRStorages, object>> updatedata = item => item.tDVRAddressBook;

			Expression<Func<tDVRStorages, string>> db_key = dbitem => dbitem.Drive;
			Expression<Func<DriverInfo, int>> info_key = info => info.ID;
			return base.UpdateDBData<tDVRStorages, DriverInfo, string, int>(dvrAdressBook.tDVRStorages, drivers, func_filter, compare_update, updatedata, dvrAdressBook, db_key, info_key);
		}

		private bool SetStorageConfig(tDVRAddressBook dvrAdressBook, StorageData storageData)
		{
			bool ret = false;
			var storageCfg = db.FirstOrDefault<tDVRStorageConfig>(item => item.KDVR == dvrAdressBook.KDVR);
			if (storageCfg == null)
			{
				storageCfg = new tDVRStorageConfig
				{
					KDVR = dvrAdressBook.KDVR,
					OverWrite = storageData.OverWrite,
					RecordMode = storageData.RecordMode
				};
				db.Insert<tDVRStorageConfig>(storageCfg);
				ret = true;
			}
			else
			{
				if (storageCfg.OverWrite != storageData.OverWrite || storageCfg.RecordMode != storageData.RecordMode)
				{
					storageCfg.OverWrite = storageData.OverWrite;
					storageCfg.RecordMode = storageData.RecordMode;
					db.Update<tDVRStorageConfig>(storageCfg);
					ret = true;
				}
			}
			return ret;
		}


		#region unused

		//private bool IsBodyData()
		//{
		//	return msgBody != null;
		//}

		//private bool IstorageData()
		//{
		//	if (!IsBodyData()) return false;
		//	return msgBody.storageData != null;
		//}

		//private bool IsDrivers()
		//{
		//	if (!IstorageData()) return false;
		//	return msgBody.storageData.Drivers != null;
		//}

		//private List<DriverInfo> GetDriverList()
		//{
		//	List<DriverInfo> driverInfolst = IsDrivers() ? msgBody.storageData.Drivers : new List<DriverInfo>();
		//	return driverInfolst;
		//}

		//private void SetDrivers(Int32 kDvr)
		//{
		//	List<tDVRStorage> lsStorages = db.Query<tDVRStorage>(x => x.KDVR == kDvr).ToList();
		//	List<DriverInfo> driverInfolst = GetDriverList();

		//	foreach (var st in lsStorages)
		//	{
		//		tDVRStorage storage = st;
		//		DriverInfo dv = driverInfolst.FirstOrDefault(x => x.Name.Trim().ToUpper() == storage.Drive.Trim().ToUpper());
		//		if (dv != null)
		//		{
		//			if (!CompareDriverInfo(kDvr, dv, storage))
		//			{
		//				SetDriverInfo(kDvr, dv, ref storage);
		//				db.Update<tDVRStorage>(storage);
		//			}
		//			driverInfolst.Remove(dv);
		//		}
		//		else
		//		{
		//			db.Delete<tDVRStorage>(storage);
		//		}
		//	}

		//	foreach (var st in driverInfolst)
		//	{
		//		var stDriver = new tDVRStorage();
		//		SetDriverInfo(kDvr, st, ref stDriver);
		//		db.Insert<tDVRStorage>(stDriver);
		//	}
		//	//db.Save();
		//}

		//private void SetDriverInfo(Int32 kDvr, DriverInfo dv, ref tDVRStorage stDriver)
		//{
		//	stDriver.KDVR = kDvr;
		//	stDriver.Drive = dv.Name;
		//	stDriver.TotalSpace = dv.TotalSpace;
		//	stDriver.FreeSpace = dv.FreeSpace;
		//	stDriver.VolumeName = dv.VolumeName;
		//	stDriver.Use4Rec = dv.UseRecording;
		//	stDriver.ChannelMask = dv.ChannelMask;
		//	stDriver.DriveType = dv.DriveType;
		//	stDriver.RecordDays = dv.NumRecordDays;

		//	if (dv.RecordedTime.BeginDate != null && dv.RecordedTime.BeginDate.Year >= 1970)
		//		stDriver.StartRecDate = dv.RecordedTime.BeginDate.Value.ToString();
		//	if (dv.RecordedTime.EndDate != null && dv.RecordedTime.EndDate.Year >= 1970)
		//		stDriver.EndRecDate = dv.RecordedTime.EndDate.Value.ToString();
		//}

		//private bool CompareDriverInfo(Int32 kDvr, DriverInfo dv, tDVRStorage stDriver)
		//{
		//	bool result = stDriver.KDVR == kDvr &&
		//				  stDriver.Drive == dv.Name &&
		//				  stDriver.TotalSpace == dv.TotalSpace &&
		//				  stDriver.FreeSpace == dv.FreeSpace &&
		//				  stDriver.VolumeName == dv.VolumeName &&
		//				  stDriver.Use4Rec == dv.UseRecording &&
		//				  stDriver.ChannelMask == dv.ChannelMask &&
		//				  stDriver.DriveType == dv.DriveType &&
		//				  stDriver.RecordDays == dv.NumRecordDays;

		//	if (dv.RecordedTime.BeginDate != null && dv.RecordedTime.BeginDate.Year >= 1970)
		//	{
		//		result = result &&
		//				 stDriver.StartRecDate == dv.RecordedTime.BeginDate.Value.ToString();
		//	}
		//	if (dv.RecordedTime.EndDate != null && dv.RecordedTime.EndDate.Year >= 1970)
		//	{
		//		result = result &&
		//				 stDriver.EndRecDate == dv.RecordedTime.EndDate.Value.ToString();
		//	}

		//	return result;
		//}

		#endregion

	}

	#region XML Classes
	[XmlRoot(MessageDefines.STR_Body)]
	public class RawStorageBody
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
		[XmlElement(RawStorageConfig.STR_Storage)]
		public StorageData storageData { get; set; }

		[XmlElement(MessageDefines.STR_Whois)]
		public RawMsgWhois msgWhois { get; set; }
	}

	[XmlRoot(RawStorageConfig.STR_Storage)]
	public class StorageData
	{
		[XmlElement(RawStorageConfig.STR_OverWrite)]
		public Int32 OverWrite { get; set; }

		[XmlElement(RawStorageConfig.STR_RecordMode)]
		public Int32 RecordMode { get; set; }

		[XmlArray(RawStorageConfig.STR_Drivers)]
		[XmlArrayItem(RawStorageConfig.STR_Driver)]
		public List<DriverInfo> Drivers = new List<DriverInfo>();
	}

	[XmlRoot(RawStorageConfig.STR_Driver)]
	public class DriverInfo : IMessageEntity<tDVRStorages>
	{
		[XmlAttribute(MessageDefines.STR_id)]
		public Int32 ID { get; set; }

		[XmlElement(MessageDefines.STR_Name)]
		public string Name { get; set; }

		[XmlElement(RawStorageConfig.STR_VolumeName)]
		public string VolumeName { get; set; }

		[XmlElement(RawStorageConfig.STR_DriveType)]
		public int DriveType { get; set; }

		[XmlElement(RawStorageConfig.STR_UseRecording)]
		public Int32 UseRecording { get; set; }

		[XmlElement(RawStorageConfig.STR_TotalSpace)]
		public Int32 TotalSpace { get; set; }

		[XmlElement(RawStorageConfig.STR_FreeSpace)]
		public Int32 FreeSpace { get; set; }

		[XmlElement(RawStorageConfig.STR_ChannelMask)]
		public UInt64 ChannelMask { get; set; }

		[XmlElement(RawStorageConfig.STR_NumRecordDays)]
		public int NumRecordDays { get; set; }

		[XmlElement(RawStorageConfig.STR_RecordedTime)]
		public RecordedTimeInfo RecordedTime { get; set; }

		public bool Equal(tDVRStorages value)
		{
			bool result = value.Drive == Name &&
						  value.TotalSpace == TotalSpace &&
						  value.FreeSpace == FreeSpace &&
						  value.VolumeName == VolumeName &&
						  value.Use4Rec == UseRecording &&
						  value.ChannelMask == ChannelMask &&
						  value.DriveType == DriveType &&
						  value.RecordDays == NumRecordDays;

			if (RecordedTime.BeginDate != null && RecordedTime.BeginDate.Year >= 1970)
			{
				result = result && (value.StartRecDate.HasValue && value.StartRecDate.Value == RecordedTime.BeginDate.Value.Date);
			}
			else
			{
				if (value.StartRecDate.HasValue)
					result = false;
			}
			if (RecordedTime.EndDate != null && RecordedTime.EndDate.Year >= 1970)
			{
				result = result && (value.EndRecDate.HasValue && value.EndRecDate.Value == RecordedTime.EndDate.Value.Date);
			}
			else
			{
				if(value.EndRecDate.HasValue)
					result = false;
			}

			return result;
		}

		public void SetEntity(ref tDVRStorages value)
		{
			if(value == null)
				value = new tDVRStorages();
			value.Drive = Name;
			value.TotalSpace = TotalSpace;
			value.FreeSpace = FreeSpace;
			value.VolumeName = VolumeName;
			value.Use4Rec = UseRecording;
			value.ChannelMask = ChannelMask;
			value.DriveType = DriveType;
			value.RecordDays = NumRecordDays;

			if (RecordedTime.BeginDate != null && RecordedTime.BeginDate.Year >= 1970)
				value.StartRecDate = RecordedTime.BeginDate.Value.Date;
			else
				value.StartRecDate = null;
			if (RecordedTime.EndDate != null && RecordedTime.EndDate.Year >= 1970)
				value.EndRecDate = RecordedTime.EndDate.Value.Date;
			else
				value.EndRecDate = null;
		}
	}

	[XmlRoot(RawStorageConfig.STR_RecordedTime)]
	public class RecordedTimeInfo
	{
		[XmlElement(RawStorageConfig.STR_Begin)]
		public DateInfo BeginDate { get; set; }

		[XmlElement(RawStorageConfig.STR_End)]
		public DateInfo EndDate { get; set; }
	}
	#endregion
}
