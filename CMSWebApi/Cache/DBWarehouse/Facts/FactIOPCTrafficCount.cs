using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
    internal class FactIOPCTrafficCount : FactBase<Fact_IOPC_TrafficCount, tbl_IOPC_TrafficCount>
    {
        bool isnew = false;
        IQueryable<Fact_IOPC_TrafficCount> CurrentFact(IResposity pacdb, tbl_IOPC_TrafficCount raw)
        {
            return pacdb.QueryNoTrack<Fact_IOPC_TrafficCount>(trafficcount=>trafficcount.EventID_BK == raw.EventAutoID && trafficcount.EventGUI == raw.EventGUI); 
        }
        protected override Fact_IOPC_TrafficCount ToFactModel(IResposity pacdb, tbl_IOPC_TrafficCount raw, ref bool isnewmodel)
        {
            isnewmodel = isnew;
            Fact_IOPC_TrafficCount fact = new Fact_IOPC_TrafficCount();
            if (raw.tbl_IOPC_TrafficCountRegion != null)
                fact.DVRDateCreateRegion = raw.tbl_IOPC_TrafficCountRegion.DVRDate;
            fact.DVRDateKey = raw.DVRDate.HasValue? raw.DVRDate.Value.Date: raw.RegionEnterTime.Date.Date;
            fact.EventGUI = raw.EventGUI;
            fact.EventID = raw.EventID;
            fact.EventID_BK = raw.EventAutoID;
            

            if (raw.tbl_IOPC_TrafficCountRegion != null)
            {
                var ex_camera = GetDim<Dim_POS_CameraNB>(cameraN => cameraN.CameraNB_BK == raw.tbl_IOPC_TrafficCountRegion.ExternalChannel);
                if (ex_camera != null)
                    fact.ExternalChannel = ex_camera.CameraNB_ID;
                var in_camera = GetDim<Dim_POS_CameraNB>(cameraN => cameraN.CameraNB_BK == raw.tbl_IOPC_TrafficCountRegion.InternalChannel);
                if (in_camera != null)
                    fact.InternalChannel = in_camera.CameraNB_ID;
                if (raw.tbl_IOPC_TrafficCountRegion.tbl_IOPC_TrafficCountRegionName != null)
                {
                    var location = GetDim<Dim_IOPC_Monitored_Location>(loca => loca.LocationID_BK == raw.tbl_IOPC_TrafficCountRegion.tbl_IOPC_TrafficCountRegionName.RegionNameID);
                    fact.LocationID = location.LocationID;
                }
                fact.RegionID = raw.tbl_IOPC_TrafficCountRegion.RegionID;
                fact.RegionIndex = raw.RegionIndex;
                var pacid = GetDim<Dim_POS_PACID>(pac=>pac.KDVR == raw.tbl_IOPC_TrafficCountRegion.T_PACID );
                if(pacid!=null)
                    fact.T_PACID =  pacid.PACID_ID;
            }

            fact.PersonID = raw.PersonID;
            fact.RegionEnterTime = raw.RegionEnterTime;
            fact.RegionExitTime = raw.RegionExitTime;
            return fact;
        }

        protected override Fact_IOPC_TrafficCount UpdateFact(IResposity pacdb, tbl_IOPC_TrafficCount raw)
        {
            if (raw == null) return null;
            var facts = CurrentFact(pacdb, raw);
            if (!facts.Any())
                isnew = true;
            else
                isnew = false;

            return base.UpdateFact(pacdb, raw);
        }
    }
}
