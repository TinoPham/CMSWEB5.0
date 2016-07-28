using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
    internal class FactIOPCLPRInfo : FactBase<Fact_IOPC_LPR_Info, tbl_LPR_Info>
    {

        bool isnew = false;
        IQueryable<Fact_IOPC_LPR_Info> CurrentFact(IResposity pacdb, tbl_LPR_Info raw)
        {
            var camNo = GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.CamNo);
            var pacid = GetDim<Dim_POS_PACID>(item => item.KDVR == raw.LPR_PACID);
    
            if (camNo == null || pacid == null ) return null;
            else
            {
                return pacdb.QueryNoTrack<Fact_IOPC_LPR_Info>(lpr => lpr.CamNo == camNo.CameraNB_ID && lpr.LPR_PACID == pacid.PACID_ID && lpr.DVRDate == raw.DVRDate && lpr.LPR_Num  == raw.LPR_Num);
            }
                 
           
        }
        protected override Fact_IOPC_LPR_Info ToFactModel(IResposity pacdb, tbl_LPR_Info raw, ref bool isnewmodel)
        {
            isnewmodel = isnew;
            Fact_IOPC_LPR_Info fact = new Fact_IOPC_LPR_Info();
            var camNo = GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.CamNo);
            var pacid = GetDim<Dim_POS_PACID>(item => item.KDVR == raw.LPR_PACID);
            //var date = GetDim<Dim_POS_Date>(item => item.dateKey == raw.DVRDate.Value.Date);


            fact.CamNo = camNo.CameraNB_ID;
            fact.DVRDate = raw.DVRDate;
            fact.DVRDateKey = raw.DVRDate.Value.Date;
            fact.LPR_ID_BK = raw.LPR_ID;
          
            fact.LPR_ImageName = raw.LPR_ImageName;
            fact.LPR_isMatch = raw.LPR_isMatch;
            fact.LPR_Num = raw.LPR_Num;
            fact.LPR_PACID = pacid.PACID_ID;
            fact.LPR_Possibility = raw.LPR_Possibility;

            return fact;
        }

        protected override Fact_IOPC_LPR_Info UpdateFact(IResposity pacdb, tbl_LPR_Info raw)
        {
            if (raw == null) return null;
            var facts = CurrentFact(pacdb, raw);
            if (facts==null || !facts.Any())
                isnew = true;
            else
                isnew = false;
            return base.UpdateFact(pacdb, raw);
        }
    }
}
