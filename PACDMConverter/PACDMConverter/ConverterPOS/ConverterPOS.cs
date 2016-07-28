using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;
using System.Data;
using Access = MSAccessObjects.POS;
using ConverterDB.Model;
using Sql = ConvertMessage.PACDMObjects.POS;
using System.Net.Http.Formatting;
using ConverterDB;
using System.Threading;
using ConvertMessage;

namespace PACDMConverter.PACDMConverter.ConverterPOS
{
	internal class ConverterPOS : ConvertBase
	{
		const string SQL_Transact = "Select top 1 * FROM Transact Where TransactKey > '{0}' order by TransactKey";
		const string SQL_Retail = "Select  * FROM Retail Where TransactKey = '{0}'";
		const string SQL_SubRetail = "Select  * FROM SubRetail Where RetailKey = '{0}'";
		const string SQL_Sensor = "SELECT * from [Sensor] order by OT_Start ASC";
		
		const string STR_Sensor = "Sensor";


		public ConverterPOS(ConvertDB LocalDB, Programset pset, ApiService httpclient, CancellationToken CancelToken, ConvertBase.ConvertMode cvtMode, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterPOS).Namespace, CancelToken,cvtMode, dataformatter)
		{

			Done = false;
//#if DEBUG
//			ConvertInfo ConvertInfo = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.TableName == "Transact");
//			if (ConvertInfo == null)
//			{
//				LocalDb.Insert<ConvertInfo>(new ConvertInfo { DvrDate = DateTime.Now.Date, Enable = true, LastKey = "0", Programset = (byte)Programset.POS, TableName = "Transact", Order = 0, UpdateDate = DateTime.Now });
//				LocalDB.Save();
//			}

//			ConvertInfo = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.TableName == MSAccessObjects.ConstEnums.Sensor);
//			if (ConvertInfo == null)
//			{
//				LocalDb.Insert<ConvertInfo>(new ConvertInfo { DvrDate = new DateTime(2013, 05, 21), Enable = true, LastKey = "0", Programset = (byte)Programset.POS, TableName = MSAccessObjects.ConstEnums.Sensor, Order = 1, UpdateDate = DateTime.Now });
//				LocalDB.Save();
//			}
//#endif
		}

		public override MessageResult ConvertData()
		{
			//List<DataFileInfo> DataFiles = base.GetDataFileList( base.DataPath);
			DataFileInfo datafile = null;

			ConvertInfo POS_Converter = LocalDb.ConvertInfo.FirstOrDefault( item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare( item.TableName, MSAccessObjects.ConstEnums.Transact, true) == 0);
			ConvertInfo Sensor_Converter = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, MSAccessObjects.ConstEnums.Sensor, true) == 0);
			MessageResult msg_ret = base.ConvertData();
			Commons.ERROR_CODE err = ERROR_CODE.OK;
			while (!base.CancellingToken.IsCancellationRequested && DataFiles.Count > 0)
			{
				datafile = DataFiles.First();
				base.RawDB.DBFile = datafile.Fileinfo.FullName;
				if( POS_Converter != null && datafile.DVRDate.Date >= POS_Converter.DvrDate.Date)
				{
					//reset last key to 0  and DVR date to datafile date
					//if(datafile.DVRDate.Date != POS_Converter.DvrDate.Date)
					//{
					//	POS_Converter.DvrDate = datafile.DVRDate.Date;
					//	POS_Converter.LastKey = "0";
					//	base.UpdateConvertinfo(POS_Converter);
					//} 
					ValidateLastKey(POS_Converter, datafile);
					msg_ret = ConvertPOSTransact(POS_Converter, datafile);
					if (msg_ret.ErrorID != ERROR_CODE.OK && msg_ret.ErrorID != ERROR_CODE.DB_QUERY_NODATA)
						break;

				}
				
				
				if( Sensor_Converter != null &&datafile.DVRDate >= Sensor_Converter.DvrDate.Date)
				{
					//if( datafile.DVRDate.Date != Sensor_Converter.DvrDate.Date)
					//{
					//	Sensor_Converter.DvrDate = datafile.DVRDate.Date;
					//	Sensor_Converter.LastKey = "0";
					//	base.UpdateConvertinfo(Sensor_Converter);
					//}
					ValidateLastKey(Sensor_Converter, datafile);
					msg_ret = ConvertPOSSensor(Sensor_Converter, datafile);
					if (err != ERROR_CODE.OK && msg_ret.ErrorID != ERROR_CODE.DB_QUERY_NODATA && msg_ret.ErrorID != ERROR_CODE.DB_INVALID_TABLE) 
						break;

					//Sensor_Converter.DvrDate = datafile.DVRDate.Date;
					//Sensor_Converter.LastKey = "0";
					//base.UpdateConvertinfo(Sensor_Converter);
				}
				
				if( convertmode == ConvertMode.Date)
					break;

				DataFiles.RemoveAt(0);
			}

			if (convertmode == ConvertMode.Date && DataFiles.Count > 0)
				DataFiles.RemoveAt(0);
			Done = (DataFiles == null|| (DataFiles.Count == 0 && !base.CancellingToken.IsCancellationRequested))? true : false;
			return msg_ret;
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		#region Sensor data
		private MessageResult ConvertPOSSensor(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault( item => string.Compare( item.Value.Des, typeof(Sql.Sensor).Name, true) == 0);
			retcode = base.IsValidtable(Fileinfo, STR_Sensor);
			if(retcode != ERROR_CODE.OK)
				return new MessageResult{ ErrorID = ERROR_CODE.DB_INVALID_TABLE};

			DataTable tbl_Sensor = base.SelectDatabySQLComand( Fileinfo, SQL_Sensor);
			int rowindex = 0;
			Int32.TryParse(covnertinfo.LastKey, out rowindex);
			if (rowindex != 0) rowindex++;
			if( rowindex >= tbl_Sensor.Rows.Count)
				return new MessageResult{ ErrorID = ERROR_CODE.DB_QUERY_NODATA};

			tbl_Sensor.TableName = MSAccessObjects.ConstEnums.Sensor;
			Access.AccessTransSensor ASensor = null;
			Sql.Sensor SSensor = null;
			ConvertMessage.MessageResult message_ret = null;
			for(int i = rowindex; i < tbl_Sensor.Rows.Count; i++)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					message_ret.ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}
				ASensor = Commons.ObjectUtils.RowToObject<Access.AccessTransSensor>(tbl_Sensor.Rows[i]);
				ASensor.DVRDate = Fileinfo.DVRDate;
				 if( (retcode = base.ConvertObject<Sql.Sensor>( out SSensor, ASensor,objConfig.Value)) != ERROR_CODE.OK) 
				 {
					message_ret.ErrorID = retcode;
					break;
				}

				message_ret = base.TransferTrans<Sql.Sensor>(SSensor);
				if( (retcode = message_ret.ErrorID) != ERROR_CODE.OK)
				{
					message_ret.ErrorID = retcode;
					break;
				}

				covnertinfo.LastKey = i.ToString();
				base.UpdateConvertinfo(covnertinfo);
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
			}


			
			return message_ret;
		}
		#endregion

		#region POS DATA

		private MessageResult ConvertPOSTransact(ConvertInfo covnertinfo, DataFileInfo Fileinfo)
		{
			ERROR_CODE errcode= ERROR_CODE.OK;
			Access.AccessTransPOS transact = POSTransaction(covnertinfo, Fileinfo, out errcode);
			if( errcode != ERROR_CODE.OK)
				return new MessageResult{ ErrorID = errcode};

			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault( item => string.Compare( item.Value.Des, typeof(Sql.Transact).Name, true) == 0);
			Sql.Transact sqltrans = null;
			ERROR_CODE code_ret = ERROR_CODE.OK;
			MessageResult msg_ret = new MessageResult{ ErrorID = ERROR_CODE.OK};

			ConvertMessage.MessageResult message_ret = new MessageResult();

			while (transact != null && !string.IsNullOrEmpty(transact.TransactKey) && CancellingToken.IsCancellationRequested == false)
			{
				if ( (code_ret = base.ConvertObject<Sql.Transact>(out sqltrans, transact, objConfig.Value)) != ERROR_CODE.OK)
				{
					message_ret.ErrorID = code_ret;
					//return code_ret;
					break;
				}

				if ((code_ret = ConvertPOSRetail(ref sqltrans, transact)) != ERROR_CODE.OK)
				{
					message_ret.ErrorID = code_ret;
					break;
				}

				if (base.CancellingToken.IsCancellationRequested)
				{
					message_ret.ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}

				message_ret = base.TransferTrans<Sql.Transact>(sqltrans);
				if ((code_ret = message_ret.ErrorID) != ERROR_CODE.OK)
				{
					message_ret.ErrorID = code_ret;
					break;
				}

				covnertinfo.LastKey = transact.TransactKey;
				base.UpdateConvertinfo(covnertinfo);
				transact = POSTransaction(covnertinfo, Fileinfo, out errcode);
				if( transact == null || errcode != ERROR_CODE.OK)
				{
					message_ret.ErrorID = errcode;
					break;
				}
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
				
			 }
			return message_ret;
		}
		
		private ERROR_CODE ConvertPOSRetail( ref Sql.Transact sqlTrans, Access.AccessTransPOS acessTransact)
		{
			if( acessTransact.Retails == null || acessTransact.Retails.Count == 0)
				return  ERROR_CODE.OK;

				Sql.Retail sRtail = null;
				sqlTrans.Retails = new List<Sql.Retail>();
				KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Key, typeof(Sql.Retail).Name, true) == 0);
				if( string.IsNullOrEmpty(objConfig.Key))
					return  ERROR_CODE.DB_CONVERT_IGNORED;
				ERROR_CODE code_ret = ERROR_CODE.OK;
				foreach (Access.Retail aRtail in acessTransact.Retails)
				{
					if (base.CancellingToken.IsCancellationRequested)
					{
						code_ret = ERROR_CODE.CONVERTER_CANCELREQUEST;
						break;
					}

					if ( (code_ret = base.ConvertObject<Sql.Retail>(out sRtail, aRtail, objConfig.Value)) != ERROR_CODE.OK)
						return code_ret;

					if( (code_ret = ConvertPOSSRetail(ref sRtail, aRtail)) != ERROR_CODE.OK)
						return code_ret;
					sqlTrans.Retails.Add(sRtail);
				}
				return code_ret;
		}
		
		private ERROR_CODE ConvertPOSSRetail( ref Sql.Retail sqlRtail, Access.Retail assRtail)
		{
			if( assRtail.SubRetails == null || assRtail.SubRetails.Count == 0)
				return  ERROR_CODE.OK;
			Sql.SubRetail sqlSRtail = null;
			sqlRtail.SubRetails = new List<Sql.SubRetail>();
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Key, typeof(Sql.SubRetail).Name, true) == 0);
			if( string.IsNullOrEmpty(objConfig.Key))
				return  ERROR_CODE.DB_CONVERT_IGNORED;
			ERROR_CODE code_ret = ERROR_CODE.OK;
			foreach(Access.SubRetail assSRtail in assRtail.SubRetails)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					code_ret = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}
				if ((code_ret = base.ConvertObject<Sql.SubRetail>(out sqlSRtail, assSRtail, objConfig.Value)) != ERROR_CODE.OK)
					return code_ret;
				sqlRtail.SubRetails.Add(sqlSRtail);
			}
			return code_ret;
		}

		private Access.AccessTransPOS POSTransaction(ConvertInfo covnertinfo, DataFileInfo Fileinfo, out ERROR_CODE ret_code)
		{

			ret_code = ERROR_CODE.OK;
			string sqlcmd = string.Format(SQL_Transact, covnertinfo.LastKey);
			Access.AccessTransPOS Transact = base.SelectDatabySQLComand<Access.AccessTransPOS>( sqlcmd, MSAccessObjects.ConstEnums.Transact, out ret_code);
			if( Transact == null)
				return Transact;
			
			sqlcmd = string.Format( SQL_Retail, Transact.TransactKey);
			if( base.CancellingToken.IsCancellationRequested)
				return Transact;
			List<Access.Retail> Retails = base.SelectListDatabySQLComand<Access.Retail>( sqlcmd, MSAccessObjects.ConstEnums.Retail, out ret_code);
			if( Retails != null)
			{
				foreach( Access.Retail rtail in Retails)
				{
					if (base.CancellingToken.IsCancellationRequested)
						break;
				   rtail.SubRetails = GetListSubretail(Fileinfo, rtail);
				}
				//Retails.ForEach( rtail => rtail.SubRetails = GetListSubretail(Fileinfo, rtail));
			}
			Transact.Retails = Retails;
			return Transact;
		}

		List<Access.SubRetail> GetListSubretail(DataFileInfo Fileinfo, Access.Retail retail)
		{
			Commons.ERROR_CODE err_code = ERROR_CODE.OK;
			string sqlcmd = string.Format(SQL_SubRetail, retail.RetailKey);
			return base.SelectListDatabySQLComand<Access.SubRetail>( sqlcmd, MSAccessObjects.ConstEnums.SubRetail, out err_code);
		}
		#endregion
	}
}
