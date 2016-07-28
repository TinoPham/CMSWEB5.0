using ConverterSVR.BAL.TransformMessages;
using ConvertMessage;
using PACDMModel.Model;
using SVRDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.PACDMConverter.LABOR
{
   internal class LABORConverter:PACDMConvertBase
    {
        public LABORConverter(PACDMModel.PACDMDB pacdb, SVRManager logdb, MessageData msgdata, MessageDVRInfo dvrinfo, MediaTypeFormatter formatter)
			: base(pacdb, logdb, msgdata, dvrinfo, formatter)
		{}
		public override MessageResult ConvertData()
		{
            bool isTransact = string.Compare(base.MsgData.Mapping, typeof(tbl_POS_Transact).Name, true) == 0 || string.Compare(base.MsgData.Mapping, PACDMConverter.STR_Transact, true) == 0;
            MessageResult msg_result = SaveTransact<ConvertMessage.PACDMObjects.LABOR.Labor, tbl_POS_Labor>(base.MsgData.Data, LABORTransformMessage.Instance);
            return msg_result;
		}
		protected override void UpdateWareHouse<Tsql>(Tsql Rawdata, IResposity pacdb)
		{
            return;       
		}
        private void UpdateWareHouse(tbl_POS_Labor transact, IResposity pacdb)
        {
            return;
        }
    }
}
