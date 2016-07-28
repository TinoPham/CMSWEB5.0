using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;
using ConverterDB.Model;
using Sql = ConvertMessage.PACDMObjects.CA;
using Access = MSAccessObjects.CA;
using ConvertMessage;

namespace PACDMConverter.PACDMConverter.ConverterCA
{
	internal class ConverterCA : ConvertBase
	{
		const string SQL_Transact = "SELECT * from [Transact]";
		public ConverterCA(ConvertDB LocalDB, Programset pset, ApiService httpclient, CancellationToken CancelToken, ConvertBase.ConvertMode cvtMode, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterCA).Namespace, CancelToken, cvtMode, dataformatter)
		{
			Done = false;
//#if DEBUG
//            ConvertInfo ConvertInfo = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.TableName == "Transact");
//            if (ConvertInfo == null)
//            {
//                LocalDb.Insert<ConvertInfo>(new ConvertInfo { DvrDate = new DateTime(2011,12,29), Enable = true, LastKey = "0", Programset = (byte)Programset.CA, TableName = "Transact", Order = 0, UpdateDate = DateTime.Now });
//                LocalDB.Save();
//            }
//#endif
		}
		public override ConvertMessage.MessageResult ConvertData()
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
					ValidateLastKey(convertinfo, datafile);
					//if (datafile.DVRDate.Date != convertinfo.DvrDate.Date)
					//{
					//	convertinfo.DvrDate = datafile.DVRDate.Date;
					//	convertinfo.LastKey = "0";
					//	base.UpdateConvertinfo(convertinfo);
					//}
					msg_result = ConvertCATransact(convertinfo, datafile);
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

		private MessageResult ConvertCATransact(ConvertInfo CAinfo, DataFileInfo fileinfo )
		{
			ERROR_CODE retcode = ERROR_CODE.OK;
			KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Key, typeof(Sql.Transact).Name, true) == 0);

			DataTable tbl_Transact = base.SelectDatabySQLComand(fileinfo, SQL_Transact);
			if( tbl_Transact == null)
				return new MessageResult{ ErrorID =  ERROR_CODE.DB_QUERY_EXCEPTION};
				
			int rowindex = 0;
			Int32.TryParse(CAinfo.LastKey, out rowindex);
			//Int32.TryParse(covnertinfo.LastKey, out rowindex);
			if (rowindex != 0) rowindex++;
			if (rowindex >= tbl_Transact.Rows.Count)
				return new MessageResult{ ErrorID = ERROR_CODE.DB_QUERY_NODATA};
			MessageResult msg_ret = new MessageResult{ ErrorID = ERROR_CODE.OK};
			tbl_Transact.TableName = MSAccessObjects.ConstEnums.Transact;
			Access.AccessTransCA ATransact = null;
			//Sql.Transact STransact = null;
			for (int i = rowindex; i < tbl_Transact.Rows.Count; i++)
			{
				if (base.CancellingToken.IsCancellationRequested)
				{
					msg_ret.ErrorID = ERROR_CODE.CONVERTER_CANCELREQUEST;
					break;
				}
				ATransact = Commons.ObjectUtils.RowToObject<Access.AccessTransCA>(tbl_Transact.Rows[i]);

				//if ((retcode = base.ConvertObject<Sql.Transact>(out STransact, ATransact, objConfig.Value)) != ERROR_CODE.OK)
				//	return retcode;
				//message_ret = base.TransferTrans<Sql.Transact>(STransact);
				msg_ret = ConvertTransactData<Access.AccessTransCA, Sql.Transact>(ATransact, objConfig.Value);
				if ((retcode = msg_ret.ErrorID) != ERROR_CODE.OK)
					break;

				CAinfo.LastKey = i.ToString();
				base.UpdateConvertinfo(CAinfo);
				System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
			}

			
			return msg_ret;
		}
	}
}
