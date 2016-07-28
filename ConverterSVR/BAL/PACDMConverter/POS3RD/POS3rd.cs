using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using ConvertMessage.PACDMObjects.POS;
using PACDMModel.Model;
using SVRDatabase;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.PACDMConverter.POS3RD
{
    class POS3rd : PACDMConvertBase
    {
        //private PACDMModel.PACDMDB database;
        public POS3rd(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{
          //  this.database = pacdb;
           // this.p
        }
		public override MessageResult ConvertData()
		{
			bool isTransact = string.Compare(base.MsgData.Mapping, typeof(tbl_POS_Transact).Name, true) == 0 || string.Compare(base.MsgData.Mapping, PACDMConverter.STR_Transact, true) == 0;
            Transact msg_transact = Commons.ObjectUtils.DeSerialize<Transact>(Formatter, base.MsgData.Data);
            List <tbl_POS_Transact> trans =  this.PACDB.Query<tbl_POS_Transact>(item => item.T_0TransNB == msg_transact.T_0TransNB && item.T_PACID == this.DVRInfo.KDVR && item.DVRDate == msg_transact.DVRDate && item.T_RegisterID == msg_transact.T_RegisterID && msg_transact.T_TerminalID == item.T_TerminalID && item.T_OperatorID == msg_transact.T_OperatorID).ToList(); ;
            if (trans != null && trans.Any())
            {
                DeleteTransaction(trans.First().TransID);
               // return new MessageResult() { ErrorID = Commons.ERROR_CODE.OK, Data = Commons.Resources.ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.OK), httpStatus = System.Net.HttpStatusCode.OK };
            }
			MessageResult msg_result =  SaveTransact<Transact,tbl_POS_Transact>(base.MsgData.Data, POSTransactTransformMessage.Instance);
			return msg_result;
		}

        public async void DeleteTransaction(long TransID)
        {
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@TransID", System.Data.SqlDbType.Int) { Value = TransID };
           
           string sql = string.Format("sp_DeleteTransact", param.First().ParameterName);
            await Task.Run(()=> this.PACDB.ExecuteQuery(sql,System.Data.CommandType.StoredProcedure, param));
          // return;

        }
		protected override void UpdateWareHouse<Tsql>(Tsql Rawdata, IResposity pacdb)
		{
			if (Rawdata is tbl_POS_Transact)
			{
				UpdateWareHouse( Rawdata as tbl_POS_Transact, pacdb);
				return;
			}
		}
		public void UpdateWareHouse(tbl_POS_Transact transact, IResposity pacdb)
		{
			if( transact == null || transact.TransID <= 0)
				return;
			Wrapper.DBWareHouse.UpdateWareHouse( transact, pacdb);
		}
    }
}
