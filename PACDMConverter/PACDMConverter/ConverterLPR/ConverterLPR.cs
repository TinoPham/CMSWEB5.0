using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using ConverterDB;
using ConverterDB.Model;
using ConvertMessage;
using Access = MSAccessObjects.LPR;
using Sql = ConvertMessage.PACDMObjects.LPR;

namespace PACDMConverter.PACDMConverter.ConverterLPR
{
	internal class ConverterLPR : ConvertBase
	{
        const string SQL_LPR = "Select top 1 * FROM Info Where  LPR_ID > {0} order by LPR_ID";
		public ConverterLPR(ConvertDB LocalDB, Programset pset, ApiService httpclient, CancellationToken CancelToken, ConvertBase.ConvertMode cvtMode, MediaTypeFormatter dataformatter = null)
			: base(LocalDB, pset, httpclient, typeof(ConverterLPR).Namespace, CancelToken, cvtMode, dataformatter)
		{
            Done = false;
		}

        public override ConvertMessage.MessageResult ConvertData()
        {
            DataFileInfo datafile = null;
            ConvertInfo LPR_Converter = LocalDb.ConvertInfo.FirstOrDefault(item => item.Programset == (byte)this.ProgramSet && item.Enable == true && string.Compare(item.TableName, MSAccessObjects.ConstEnums.Info, true) == 0);
            MessageResult msg_ret = base.ConvertData();
            Commons.ERROR_CODE err = ERROR_CODE.OK;
            while (!base.CancellingToken.IsCancellationRequested && DataFiles.Count > 0)
            {
                datafile = DataFiles.First();
                base.RawDB.DBFile = datafile.Fileinfo.FullName;
                if (LPR_Converter != null && datafile.DVRDate.Date >= LPR_Converter.DvrDate.Date)
                {

                    ValidateLastKey(LPR_Converter, datafile);
                    msg_ret = ConvertLPR(LPR_Converter, datafile);
           
                    if (msg_ret.ErrorID != ERROR_CODE.OK && msg_ret.ErrorID != ERROR_CODE.DB_QUERY_NODATA)
                        break;

                }
                if (convertmode == ConvertMode.Date)
                    break;

                DataFiles.RemoveAt(0);
            }

            if (convertmode == ConvertMode.Date && DataFiles.Count > 0)
                DataFiles.RemoveAt(0);
            Done = (DataFiles == null || (DataFiles.Count == 0 && !base.CancellingToken.IsCancellationRequested)) ? true : false;
            return msg_ret;
         
            //return base.ConvertData();
        }

        MessageResult ConvertLPR(ConvertInfo LPR_Converter, DataFileInfo datafile)
        {
            ERROR_CODE errcode = ERROR_CODE.OK;
            Access.AccessTransInfo transact = LPR_Transact(LPR_Converter, datafile, out errcode);
            if (errcode != ERROR_CODE.OK)
                return new MessageResult { ErrorID = errcode };

            KeyValuePair<string, ItemObject> objConfig = base.ObjectMapping.FirstOrDefault(item => string.Compare(item.Value.Des, typeof(Sql.Info).Name, true) == 0);
            Sql.Info sqltrans = null;
            ERROR_CODE code_ret = ERROR_CODE.OK;
            MessageResult msg_ret = new MessageResult { ErrorID = ERROR_CODE.OK };
            while (transact != null && transact.LPR_ID!=0 && CancellingToken.IsCancellationRequested == false)
            {
                if ((code_ret = base.ConvertObject<Sql.Info>(out sqltrans, transact, objConfig.Value)) != ERROR_CODE.OK)
                {
                    msg_ret.ErrorID = code_ret;
                    //return code_ret;
                    break;
                }
               
                msg_ret = base.TransferTrans<Sql.Info>(sqltrans);
                if (msg_ret.ErrorID == ERROR_CODE.OK)
                {
                    LPR_Converter.LastKey = transact.LPR_ID.ToString();
                    if (!base.UpdateConvertinfo(LPR_Converter))
                    {
                        System.Diagnostics.Debug.WriteLine("Update Key Fail");
                    }
                    transact = LPR_Transact(LPR_Converter, datafile, out errcode);
                    msg_ret.ErrorID = errcode;
                    System.Threading.Thread.Sleep(ConvertBase.Sleep_TimeOut);
                }
                else
                {
                    break;
                }
 
            }
            return msg_ret;
        }
        
        private Access.AccessTransInfo LPR_Transact(ConvertInfo covnertinfo, DataFileInfo Fileinfo, out ERROR_CODE ret_code)
        {
            ret_code = ERROR_CODE.OK;
            string sqlcmd = string.Format(SQL_LPR, covnertinfo.LastKey);
            Access.AccessTransInfo Transact = base.SelectDatabySQLComand<Access.AccessTransInfo>(sqlcmd, MSAccessObjects.ConstEnums.Info, out ret_code);
            if (Transact == null)
                return Transact;
            return Transact;
		}
	}
}
