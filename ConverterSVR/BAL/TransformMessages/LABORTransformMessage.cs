using ConvertMessage;
using ConvertMessage.PACDMObjects.LABOR;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.TransformMessages
{
    internal class LABORTransformMessage : Commons.SingletonClassBase<LABORTransformMessage>, ITransformMessage<Labor, tbl_POS_Labor>
    {
        public tbl_POS_Labor TransForm(Labor input, MessageDVRInfo DVRInfo)
        {
            tbl_POS_Labor output = new tbl_POS_Labor();
            output.EmployeeID = input.EmployeeID;
            output.Date = input.Date;
            output.InPunch = input.InPunch.ToString();
            output.OutPunch = input.OutPunch.ToString();
            output.StoreID = input.StoreID;
            output.T_PACID = DVRInfo.KDVR;
            return output;
        }
    }
}
