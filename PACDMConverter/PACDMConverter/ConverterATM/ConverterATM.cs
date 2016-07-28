using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;
using System.Data;
using Access = MSAccessObjects.ATM;
using ConverterDB.Model;
using Sql = ConvertMessage.PACDMObjects.ATM;
using System.Net.Http.Formatting;
using ConverterDB;
using System.Threading;
using ConvertMessage;

namespace PACDMConverter.PACDMConverter.ConverterATM
{
	internal class ConverterATM : ConvertBase
	{
		const string SQL_Transact = "Select top 1 * FROM Transact Where TransactKey > '{0}' order by TransactKey";

		public ConverterATM(ConvertDB LocalDB, Programset pset, ApiService httpclient, CancellationToken CancelToken, ConvertBase.ConvertMode cvtMode, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterATM).Namespace, CancelToken, cvtMode, dataformatter)
		{
			Done = false;
		}

		public override MessageResult ConvertData()
		{
			//List<DataFileInfo> DataFiles = base.GetDataFileList(base.DataPath);
			DataFileInfo datafile = null;
			ConvertInfo convertinfo = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, MSAccessObjects.ConstEnums.Transact, true) == 0);
			//Commons.ERROR_CODE err = ERROR_CODE.OK;
			MessageResult msg_result = base.ConvertData();
			while (!base.CancellingToken.IsCancellationRequested && DataFiles.Count > 0)
			{
				datafile = DataFiles.First();
				base.RawDB.DBFile = datafile.Fileinfo.FullName;
				if (convertinfo != null && datafile.DVRDate.Date >= convertinfo.DvrDate.Date)
				{
					//reset last key to 0  and DVR date to datafile date
					//if (datafile.DVRDate.Date != convertinfo.DvrDate.Date)
					//{
					//    convertinfo.DvrDate = datafile.DVRDate.Date;
					//    convertinfo.LastKey = "0";
					//    base.UpdateConvertinfo(convertinfo);
					//}
					ValidateLastKey(convertinfo, datafile);
					msg_result = ConvertATMTransact(convertinfo, datafile);
					if (msg_result.ErrorID != ERROR_CODE.OK)
						break;
					//convertinfo.DvrDate = datafile.DVRDate.Date;
					//convertinfo.LastKey = "0";
					//base.UpdateConvertinfo(convertinfo);
				}


				if (convertmode == ConvertMode.Date)
					break;

				DataFiles.RemoveAt(0);
			}

			if (convertmode == ConvertMode.Date && DataFiles.Count > 0)
				DataFiles.RemoveAt(0);

			Done = (DataFiles == null || (DataFiles.Count == 0 && !base.CancellingToken.IsCancellationRequested)) ? true : false;
			return msg_result;

		}

		private MessageResult ConvertATMTransact(ConvertInfo ATMinfo, DataFileInfo fileinfo)
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Key, typeof(Sql.Transact).Name, true) == 0);
			string sqlcmd = string.Format(SQL_Transact, ATMinfo.LastKey);
			DataTable tbl_Transact = base.SelectDatabySQLComand(fileinfo, sqlcmd);
			if (tbl_Transact == null)
				return new MessageResult { ErrorID = ERROR_CODE.DB_QUERY_EXCEPTION };

			
			if (tbl_Transact.Rows.Count == 0)
				return new MessageResult { ErrorID = ERROR_CODE.DB_QUERY_NODATA };

			MessageResult msg_ret = new MessageResult { ErrorID = ERROR_CODE.OK };
			tbl_Transact.TableName = MSAccessObjects.ConstEnums.Transact;
			Access.AccessTransATM ATransact = null;
			//Sql.Transact STransact = null;
			ATransact = Commons.ObjectUtils.RowToObject<Access.AccessTransATM>( tbl_Transact.Rows[0]);
			Commons.ERROR_CODE errcode = ERROR_CODE.OK;
			while(ATransact != null)
			{
				if( CancellingToken.IsCancellationRequested)
				{
					msg_ret.ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}

				msg_ret = ConvertTransactData<Access.AccessTransATM, Sql.Transact>(ATransact, objConfig.Value);
				if ((retcode = msg_ret.ErrorID) != ERROR_CODE.OK)
					break;

				ATMinfo.LastKey = ATransact.TransactKey;
				base.UpdateConvertinfo(ATMinfo);
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
				sqlcmd = string.Format(SQL_Transact, ATMinfo.LastKey);
				ATransact = base.SelectDatabySQLComand<Access.AccessTransATM>(sqlcmd, MSAccessObjects.ConstEnums.Transact, out errcode);
				if( errcode != ERROR_CODE.OK)
				{
					ATransact = null;
					msg_ret.ErrorID = errcode;
				}
			}

			return msg_ret;
		}

		public override void Dispose()
		{
			base.Dispose();
		}

	}
}
