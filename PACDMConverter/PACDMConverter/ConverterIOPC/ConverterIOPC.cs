using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;
using ConverterDB.Model;
using MSAccessObjects.IOPC;
using Access = MSAccessObjects.IOPC;
using Sql = ConvertMessage.PACDMObjects.IOPC;

namespace PACDMConverter.PACDMConverter.ConverterIOPC
{
	internal class ConverterIOPC : ConvertBase
	{
		//const string str_Alarm = "Alarm";
		const string RegionCount_Setup_FileName = "RegionCount.mdb";
		//const string str_TrafficCount = "TrafficCount";
		const string SQL_Alarm = "Select * From ALarm";
		const string SQL_TrafficCount = "Select Top 1 * from TrafficCount Where EventID >{0} ORDER BY EventID ASC";
		const string SQL_TrafficCountregion = "Select * from TrafficCountRegion Where RegionID = {0} ";
		const string SQL_Drive_Through = "Select Top 1 * from DriveThrough Where TD_ID > {0} Order by TD_ID ";
		const string SQL_Count = "Select * from [Count]";
		private Dictionary<int, int>RegionID_Index_Mapping = new  Dictionary<int,int>();
		public ConverterIOPC(ConvertDB LocalDB, Programset pset, ApiService httpclient, CancellationToken CancelToken, ConvertBase.ConvertMode cvtMode, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterIOPC).Namespace, CancelToken, cvtMode, dataformatter)
		{
		}

		public override ConvertMessage.MessageResult ConvertData()
		{
			//List<DataFileInfo> DataFiles = base.GetDataFileList(base.DataPath);
			DataFileInfo datafile = null;

			ConvertMessage.MessageResult result = base.ConvertData();

			while (!base.CancellingToken.IsCancellationRequested && DataFiles.Count > 0)
			{
				datafile = DataFiles.First();
				base.RawDB.DBFile = datafile.Fileinfo.FullName;
				#region mark
				//if (Trafficcount_Converter != null && datafile.DVRDate.Date >= Trafficcount_Converter.DvrDate.Date)
				//{
				//	//reset last key to 0  and DVR date to datafile date
				//	if (datafile.DVRDate.Date != Trafficcount_Converter.DvrDate.Date)
				//	{
				//		Trafficcount_Converter.DvrDate = datafile.DVRDate.Date;
				//		Trafficcount_Converter.LastKey = "0";
				//		base.UpdateConvertinfo(Trafficcount_Converter);
				//	}
				//	result = ConvertIOPCTrafficCount(Trafficcount_Converter, datafile);
				//	if (result.ErrorID != ERROR_CODE.OK)
				//		break;
				//	Trafficcount_Converter.DvrDate = datafile.DVRDate.Date;
				//	Trafficcount_Converter.LastKey = "0";
				//	base.UpdateConvertinfo(Trafficcount_Converter);
				//}
				#endregion

				result = ConvertTrafficCount(datafile);
				if( result.ErrorID != ERROR_CODE.OK && result.ErrorID != ERROR_CODE.DB_QUERY_NODATA && result.ErrorID != ERROR_CODE.DB_INVALID_TABLE)
					break;

				result = ConvertIOPCCount(datafile);
				if (result.ErrorID != ERROR_CODE.OK && result.ErrorID != ERROR_CODE.DB_QUERY_NODATA && result.ErrorID != ERROR_CODE.DB_INVALID_TABLE)
					break;

				#region	Mark
				//if (Alarm_Converter != null && datafile.DVRDate >= Alarm_Converter.DvrDate.Date)
				//{
				//	if (datafile.DVRDate.Date != Alarm_Converter.DvrDate.Date)
				//	{
				//		Alarm_Converter.DvrDate = datafile.DVRDate.Date;
				//		Alarm_Converter.LastKey = "0";
				//		base.UpdateConvertinfo(Alarm_Converter);
				//	}
				//	result = ConvertIOPCAlarm(Alarm_Converter, datafile);
				//	if (result.ErrorID != ERROR_CODE.OK)
				//		break;

				//	Alarm_Converter.DvrDate = datafile.DVRDate.Date;
				//	Alarm_Converter.LastKey = "0";
				//	base.UpdateConvertinfo(Alarm_Converter);
				//}
				#endregion

				result = ConvertAlarm(datafile);
				if (result.ErrorID != ERROR_CODE.OK && result.ErrorID != ERROR_CODE.DB_QUERY_NODATA && result.ErrorID != ERROR_CODE.DB_INVALID_TABLE)
					break;

				result = ConvertDrivethrough(datafile);
				if (result.ErrorID != ERROR_CODE.OK && result.ErrorID != ERROR_CODE.DB_QUERY_NODATA && result.ErrorID != ERROR_CODE.DB_INVALID_TABLE)
					break;

				


				
				if (convertmode == ConvertMode.Date)
					break;

				DataFiles.RemoveAt(0);
			}
			if( convertmode == ConvertMode.Date && DataFiles.Count > 0)
				DataFiles.RemoveAt(0);

			Done = (DataFiles == null || (DataFiles.Count == 0 && !base.CancellingToken.IsCancellationRequested)) ? true : false;

			return result;
		}
		 #region IOPC Count
		private ConvertMessage.MessageResult ConvertIOPCCount(DataFileInfo Fileinfo)
		{
			ConvertInfo covnertinfo = GetConvertInfo(MSAccessObjects.ConstEnums.Count);
			ConvertMessage.MessageResult result = new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.OK };
			if (covnertinfo != null && Fileinfo.DVRDate.Date >= covnertinfo.DvrDate.Date)
			{
				ValidateLastKey(covnertinfo, Fileinfo);

				result = ConvertIOPCCount(covnertinfo, Fileinfo);
				if (result.ErrorID != ERROR_CODE.OK)
					return result;

			}
			return result;
		}

		private ConvertMessage.MessageResult ConvertIOPCCount(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.Count).Name, true) == 0);
			if (string.IsNullOrEmpty(objConfig.Key) || objConfig.Value == null)
				return new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.DB_INVALID_CONVERT_CONFIG };

			retcode = base.IsValidtable(Fileinfo, MSAccessObjects.ConstEnums.Count);
			if (retcode != ERROR_CODE.OK)
				return new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.DB_INVALID_TABLE };

			DataTable tblCount = base.SelectDatabySQLComand(Fileinfo, SQL_Count);
			if (tblCount == null)
				return new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.DB_QUERY_EXCEPTION };

			int rowindex = 0;
			Int32.TryParse(covnertinfo.LastKey, out rowindex);
			rowindex = rowindex == 0 ? 0 : rowindex + 1;
			if (rowindex >= tblCount.Rows.Count)
				return new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.DB_QUERY_NODATA };

			tblCount.TableName = MSAccessObjects.ConstEnums.Count;
			Access.AccessTransCount ACount = null;

			ConvertMessage.MessageResult message_ret = null;
			for (int i = rowindex; i < tblCount.Rows.Count; i++)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					message_ret = new ConvertMessage.MessageResult { ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST };
					break;
				}

				ACount = Commons.ObjectUtils.RowToObject<Access.AccessTransCount>(tblCount.Rows[i]);

				//if ((retcode = base.ConvertObject<Sql.Alarm>(out SAlarm, AAlarm, objConfig.Value)) != ERROR_CODE.OK)
				//	return new ConvertMessage.MessageResult{ ErrorID = retcode};

				//message_ret = base.TransferTrans<Sql.Alarm>(SAlarm);

				message_ret = base.ConvertTransactData<Access.AccessTransCount, Sql.Count>(ACount, objConfig.Value);
				if ((retcode = message_ret.ErrorID) != ERROR_CODE.OK)
				{
					message_ret = new ConvertMessage.MessageResult { ErrorID = retcode };
					break;
				}

				covnertinfo.LastKey = i.ToString();
				base.UpdateConvertinfo(covnertinfo);
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
			}

			return message_ret;
		}
		#endregion

		#region Drive Through Table

		public ConvertMessage.MessageResult ConvertDrivethrough(DataFileInfo Fileinfo)
		{
			ConvertInfo covnertinfo =  GetConvertInfo(MSAccessObjects.ConstEnums.DriveThrough);
			ConvertMessage.MessageResult result = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.OK};
			if (covnertinfo != null && Fileinfo.DVRDate.Date >= covnertinfo.DvrDate.Date)
			{
				//reset last key to 0  and DVR date to datafile date
				//if (Fileinfo.DVRDate.Date != covnertinfo.DvrDate.Date)
				//{
				//	covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//	covnertinfo.LastKey = "0";
				//	base.UpdateConvertinfo(covnertinfo);
				//}
				ValidateLastKey(covnertinfo, Fileinfo);

				result = ConvertIOPCDriveThrough(covnertinfo, Fileinfo);
				if (result.ErrorID != ERROR_CODE.OK)
					return result;

				//covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//covnertinfo.LastKey = "0";
				//base.UpdateConvertinfo(covnertinfo);
			}
			return result;
		}

		private MSAccessObjects.IOPC.AccessTransDriveThrough GetDriveThrough( string lastkey, out ERROR_CODE ret_code)
		{
			string sql = string.Format( SQL_Drive_Through, lastkey);
			return base.SelectDatabySQLComand<Access.AccessTransDriveThrough>(sql, MSAccessObjects.ConstEnums.DriveThrough,out ret_code);
		}

		private ConvertMessage.MessageResult ConvertIOPCDriveThrough(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.DriveThrough).Name, true) == 0);
			if( string.IsNullOrEmpty( objConfig.Key) || objConfig.Value == null)
				return new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.DB_INVALID_CONVERT_CONFIG};
			ERROR_CODE err_code= ERROR_CODE.OK;
			Access.AccessTransDriveThrough access = GetDriveThrough( covnertinfo.LastKey, out err_code);
			if( access == null)
				return new ConvertMessage.MessageResult{ ErrorID = err_code};

			ConvertMessage.MessageResult msg_ret = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.OK};
			while(access != null && access.TD_ID > 0 )
			{
				if(CancellingToken.IsCancellationRequested)
				{
					msg_ret.ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}
				msg_ret = ConvertTransactData<Access.AccessTransDriveThrough, Sql.DriveThrough>(access, objConfig.Value);
				if( msg_ret.ErrorID != ERROR_CODE.OK)
					break;
				covnertinfo.LastKey = access.TD_ID.ToString();
				base.UpdateConvertinfo(covnertinfo);
				access = GetDriveThrough(covnertinfo.LastKey, out err_code);
				msg_ret.ErrorID = err_code;
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
				
			}
			
			return msg_ret;
		}

		private ConvertMessage.MessageResult ConvertIOPCDriveThrough(MSAccessObjects.IOPC.AccessTransDriveThrough ADriveThrough, KeyValuePair<string, ItemObject> objConfig)
		{
			Sql.DriveThrough sqlDT;
			ERROR_CODE ret_code = base.ConvertObject<Sql.DriveThrough>(out sqlDT, ADriveThrough, objConfig.Value);
			if( ret_code != ERROR_CODE.OK)
				return new ConvertMessage.MessageResult{ ErrorID = ret_code};
			return base.TransferTrans<Sql.DriveThrough>( sqlDT);
		}
		#endregion

		#region Alarm table

		private ConvertMessage.MessageResult ConvertAlarm(DataFileInfo Fileinfo)
		{
			ConvertInfo covnertinfo = GetConvertInfo(MSAccessObjects.ConstEnums.Alarm); //LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, MSAccessObjects.ConstEnums.Alarm, true) == 0);
			ConvertMessage.MessageResult result = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.OK};
			if (covnertinfo != null && Fileinfo.DVRDate >= covnertinfo.DvrDate.Date)
			{
				//if (Fileinfo.DVRDate.Date != covnertinfo.DvrDate.Date)
				//{
				//	covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//	covnertinfo.LastKey = "0";
				//	base.UpdateConvertinfo(covnertinfo);
				//}

				ValidateLastKey(covnertinfo, Fileinfo);

				result = ConvertIOPCAlarm(covnertinfo, Fileinfo);
				if (result.ErrorID != ERROR_CODE.OK)
					return result;

				//covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//covnertinfo.LastKey = "0";
				//base.UpdateConvertinfo(covnertinfo);
			}

			return result;

		}

		private ConvertMessage.MessageResult ConvertIOPCAlarm(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.Alarm).Name, true) == 0);
			if( string.IsNullOrEmpty( objConfig.Key) || objConfig.Value == null)
				return new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.DB_INVALID_CONVERT_CONFIG};

			retcode = base.IsValidtable(Fileinfo, MSAccessObjects.ConstEnums.Alarm);
			if (retcode != ERROR_CODE.OK)
				return new ConvertMessage.MessageResult{ ErrorID =  ERROR_CODE.DB_INVALID_TABLE};

			DataTable tbl_Alarm = base.SelectDatabySQLComand(Fileinfo, SQL_Alarm);
			if( tbl_Alarm == null)
				return new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.DB_QUERY_EXCEPTION};

			int rowindex = 0;
			Int32.TryParse(covnertinfo.LastKey, out rowindex);
			rowindex = rowindex == 0? 0 : rowindex + 1;
			if (rowindex >= tbl_Alarm.Rows.Count)
				return new ConvertMessage.MessageResult{ ErrorID =  ERROR_CODE.DB_QUERY_NODATA};

			tbl_Alarm.TableName = MSAccessObjects.ConstEnums.Alarm;
			Access.AccessTransAlarm AAlarm = null;

			ConvertMessage.MessageResult message_ret = null;
			for (int i = rowindex; i < tbl_Alarm.Rows.Count; i++)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					message_ret = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST};
					break;
				}

				AAlarm = Commons.ObjectUtils.RowToObject<Access.AccessTransAlarm>(tbl_Alarm.Rows[i]);
				
				//if ((retcode = base.ConvertObject<Sql.Alarm>(out SAlarm, AAlarm, objConfig.Value)) != ERROR_CODE.OK)
				//	return new ConvertMessage.MessageResult{ ErrorID = retcode};

				//message_ret = base.TransferTrans<Sql.Alarm>(SAlarm);

				message_ret = base.ConvertTransactData<Access.AccessTransAlarm, Sql.Alarm>(AAlarm, objConfig.Value);
				if ((retcode = message_ret.ErrorID) != ERROR_CODE.OK)
				{
					message_ret = new ConvertMessage.MessageResult{ ErrorID = retcode};
					break;
				}

				covnertinfo.LastKey = i.ToString();
				base.UpdateConvertinfo(covnertinfo);
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
			}

			return message_ret;
		}

		#endregion

		#region Traffic count table

		private ConvertMessage.MessageResult ConvertTrafficCount(DataFileInfo Fileinfo)
		{
			ConvertInfo covnertinfo = GetConvertInfo(MSAccessObjects.ConstEnums.TrafficCount); //LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, MSAccessObjects.ConstEnums.TrafficCount, true) == 0);
			ConvertMessage.MessageResult result = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.OK};
			if (covnertinfo != null && Fileinfo.DVRDate.Date >= covnertinfo.DvrDate.Date)
			{
				//reset last key to 0  and DVR date to datafile date
				//if (Fileinfo.DVRDate.Date != covnertinfo.DvrDate.Date)
				//{
				//	covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//	covnertinfo.LastKey = "0";
				//	base.UpdateConvertinfo(covnertinfo);
				//}
				ValidateLastKey(covnertinfo, Fileinfo);

				result = ConvertIOPCTrafficCount(covnertinfo, Fileinfo);
				if (result.ErrorID != ERROR_CODE.OK)
					return result;

				//covnertinfo.DvrDate = Fileinfo.DVRDate.Date;
				//covnertinfo.LastKey = "0";
				//base.UpdateConvertinfo(covnertinfo);
			}
			return result;
		}

		private ConvertMessage.MessageResult ConvertTrafficCountRegion( int regionID)
		{
			MSAccessObjects.MSAccess dbfile = new MSAccessObjects.MSAccess();
			dbfile.DBFile = Path.Combine(DVRInfos.Instance.PACDMInfo.PACDir, RegionCount_Setup_FileName);
			string sql = string.Format(SQL_TrafficCountregion, regionID);
			Access.TrafficCountRegion region = dbfile.SelectDatabySQLComand<Access.TrafficCountRegion>(sql, MSAccessObjects.ConstEnums.TrafficCountRegion);
			if( region == null)
				return new ConvertMessage.MessageResult{ ErrorID =  ERROR_CODE.DB_CONVERT_DATA_FAILED};

			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.TrafficCountRegion).Name, true) == 0);
			if( string.IsNullOrEmpty( objConfig.Key) || objConfig.Value == null)
				return new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.DB_INVALID_CONVERT_CONFIG};

			return ConvertTransactData<Access.TrafficCountRegion, Sql.TrafficCountRegion>(region, objConfig.Value);

			//Sql.TrafficCountRegion sqltrafficregion;
			//Commons.ERROR_CODE ret = base.ConvertObject<Sql.TrafficCountRegion>(out sqltrafficregion, region, objConfig.Value);
			//if( ret != ERROR_CODE.OK)
			//	return new ConvertMessage.MessageResult { ErrorID = ret};

			//return base.TransferTrans<Sql.TrafficCountRegion>(sqltrafficregion);
		}
		
		private Access.AccessTransTrafficCount GetTrafficCount(DataFileInfo Fileinfo, string lastkey, out ERROR_CODE errcode)
		{
			string sql = string.Format(SQL_TrafficCount, lastkey);
			return base.SelectDatabySQLComand<Access.AccessTransTrafficCount>(sql, MSAccessObjects.ConstEnums.TrafficCount, out errcode);
		}
		
		private ConvertMessage.MessageResult ConvertIOPCTrafficCount(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.TrafficCount).Name, true) == 0);
			if( string.IsNullOrEmpty( objConfig.Key) || objConfig.Value == null)
				return new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.DB_INVALID_CONVERT_CONFIG};


			retcode = base.IsValidtable(Fileinfo, MSAccessObjects.ConstEnums.TrafficCount);
			if (retcode != ERROR_CODE.OK)
				return new ConvertMessage.MessageResult{ ErrorID = retcode};
			ERROR_CODE errcode = ERROR_CODE.OK;
			Access.AccessTransTrafficCount ATrafficCount = GetTrafficCount(Fileinfo, covnertinfo.LastKey, out errcode);
			if( ATrafficCount == null)
				return new ConvertMessage.MessageResult { ErrorID = errcode };

			Sql.TrafficCount sqltrafficCount = null;
			ConvertMessage.MessageResult message_ret = null;
			KeyValuePair<int, int>RegionIDMapping;
			while(ATrafficCount != null && ATrafficCount.EventID > 0)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					message_ret = new ConvertMessage.MessageResult{ ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST};
					break;
				}

				RegionIDMapping = RegionID_Index_Mapping.FirstOrDefault( item => item.Key == ATrafficCount.RegionID);
				if( RegionIDMapping.Key == 0)
				{
					message_ret = ConvertTrafficCountRegion(ATrafficCount.RegionID);
					if (message_ret.ErrorID != ERROR_CODE.OK)
						return message_ret;

					RegionIDMapping = new KeyValuePair<int, int>(ATrafficCount.RegionID, Convert.ToInt32(message_ret.Data));
					RegionID_Index_Mapping.Add(RegionIDMapping.Key, RegionIDMapping.Value);
					
				}
				ATrafficCount.FileDate  = Fileinfo.DVRDate;
				retcode = base.ConvertObject<Sql.TrafficCount>(out sqltrafficCount, ATrafficCount, objConfig.Value);
				if (retcode != ERROR_CODE.OK || CancellingToken.IsCancellationRequested)
				{
					message_ret = new ConvertMessage.MessageResult{ ErrorID = CancellingToken.IsCancellationRequested? ERROR_CODE.SERVICE_TERMINAL : retcode};
					break;
				}
				sqltrafficCount.RegionIndex = RegionIDMapping.Value;
				message_ret = base.TransferTrans<Sql.TrafficCount>(sqltrafficCount);
				if( message_ret.ErrorID != ERROR_CODE.OK)
					break;

				covnertinfo.LastKey = ATrafficCount.EventID.ToString();
				base.UpdateConvertinfo(covnertinfo);
				ATrafficCount = GetTrafficCount(Fileinfo, covnertinfo.LastKey, out errcode);
				if( ATrafficCount == null)
				{
					message_ret = new ConvertMessage.MessageResult{ ErrorID = errcode};
					break;
				}
				System.Threading.Thread.Sleep( ConvertBase.Sleep_TimeOut);
			}

			//base.UpdateConvertinfo(covnertinfo);

			return message_ret;

		}
		#endregion
	}
}
