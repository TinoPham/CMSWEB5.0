using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using ConvertMessage.PACDMObjects.LPR;
using PACDMModel.Model;
using SVRDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.PACDMConverter.LPR
{
    internal class LPRConverter : PACDMConvertBase
    {
        public LPRConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}

        public override MessageResult ConvertData()
        {
            MessageResult msg_rs = SaveTransact<Info, tbl_LPR_Info>(base.MsgData.Data, LPRTransformeMessage.Instance);
            return msg_rs;
        }
        protected override MessageResult MessageDatatoMessageResult(Commons.ERROR_CODE errorID, MessageData data)
        {
            return base.MessageDatatoMessageResult(errorID, data);
        }
        protected override void UpdateWareHouse<Tsql>(Tsql Rawdata, PACDMModel.Model.IResposity pacdb)
        {
            if (Rawdata == null)
                return;
            if(Rawdata is tbl_LPR_Info)
                Wrapper.DBWareHouse.UpdateWareHouse(Rawdata as tbl_LPR_Info, pacdb);
          //  base.UpdateWareHouse<Tsql>(Rawdata, pacdb);
        }
    }
}
