using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
     internal class  FactIOPCDriveThrough : FactBase<Fact_IOPC_DriveThrough, tbl_IOPC_DriveThrough>
    {
         bool isnew = false;
         IQueryable<Fact_IOPC_DriveThrough> CurrentFact(IResposity pacdb, tbl_IOPC_DriveThrough raw) 
         {
             var e_camNo = GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.ExternalCamera);
             var i_camNo = GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.InternalCamera);
             var pacid = GetDim<Dim_POS_PACID>(item => item.KDVR == raw.T_PACID);
            // var date = GetDim<Dim_POS_Date>(item => item.dateKey == raw.StartDate.Value.Date);
             if (e_camNo == null || i_camNo == null || pacid == null) return null;

             return pacdb.QueryNoTrack<Fact_IOPC_DriveThrough>(item => item.T_PACID == pacid.PACID_ID && item.StartDate == raw.StartDate && item.EndDate == raw.EndDate && item.ExternalCamera == e_camNo.CameraNB_ID && item.InternalCamera == i_camNo.CameraNB_ID);
         }


         protected override Fact_IOPC_DriveThrough UpdateFact(IResposity pacdb, tbl_IOPC_DriveThrough raw)
         {
             if (raw == null) return null;
             var facts = CurrentFact(pacdb, raw);
             if (facts == null || !facts.Any())
                 isnew = true;
             else
                 isnew = false;
             return base.UpdateFact(pacdb, raw);
         }

         protected override Fact_IOPC_DriveThrough ToFactModel(IResposity pacdb, tbl_IOPC_DriveThrough raw, ref bool isnewmodel)
         {
             isnewmodel = isnew;
             Fact_IOPC_DriveThrough fact = new Fact_IOPC_DriveThrough();

             var pacid = GetDim<Dim_POS_PACID>(pac => pac.KDVR == raw.T_PACID);
             if(pacid!=null) fact.T_PACID = pacid.PACID_ID;
             fact.TD_ID_BK = raw.TD_ID;
             fact.StartDate = raw.StartDate;
             fact.EndDate = raw.EndDate;
             fact.Function = raw.Function;
             DateTime start = new DateTime(raw.StartDate.Value.Date.Year, raw.StartDate.Value.Date.Month, raw.StartDate.Value.Date.Day);
             DateTime end = new DateTime(raw.EndDate.Value.Date.Year, raw.EndDate.Value.Date.Month, raw.EndDate.Value.Date.Day);
             fact.StartDateKey = raw.StartDate.Value;
             fact.EndDateKey = raw.StartDate.Value;
             
             var ex_camera = raw.tbl_POS_CameraNBList != null ? GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.tbl_POS_CameraNBList.CameraNB_ID) :null;
             var int_camera = GetDim<Dim_POS_CameraNB>(item => item.CameraNB_BK == raw.InternalCamera);
             if (ex_camera != null)
             {
                 fact.ExternalCamera = ex_camera.CameraNB_ID;
             }
             if (int_camera != null)
             {
                 fact.InternalCamera = int_camera.CameraNB_ID;
             }

            return fact;
         }


        
    }
}
