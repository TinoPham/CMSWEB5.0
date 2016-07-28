using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.Caches;
using CMSWebApi.DataModels.DashBoardCache;
using PACDMModel.Model;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
	internal class FactIOPCCount : FactBase<Fact_IOPC_Count, tbl_IOPC_Count>
	{

		protected override Fact_IOPC_Count UpdateFact(IResposity pacdb, tbl_IOPC_Count raw)
		{
			if( raw == null || raw.tbl_IOPC_Count_Area == null || !isTrafficCount_In_Out(raw.tbl_IOPC_Count_Area.AreaName))
				return null;
			ICache<IOPCCountPeriodicCacheModel> icache = this.BackgroungTask.CacheMgr.Resolve<IOPCCountPeriodicCacheModel>();
			IOPCCountCache iopccache = icache as IOPCCountCache;
			if (iopccache == null || !iopccache.Config.Enable || !iopccache.Config.Live)
				return null;

			return base.UpdateFact(pacdb, raw);
		}

		protected override Fact_IOPC_Count ToFactModel(IResposity pacdb, tbl_IOPC_Count raw, ref bool isnewmodel)
		{
			Fact_IOPC_Count fact = new Fact_IOPC_Count();
			Dim_IOPC_Monitored_Location locID = GetDimMonitored_Location( raw.C_AreaNameID.Value);
			Dim_IOPC_ObjectType objtype = Get_Dim_IOPC_ObjectType( raw.C_ObjectTypeID.Value);
			isnewmodel = false;
			if( locID == null|| objtype == null)
				return null;
			fact.Count_ID_BK = raw.Count_ID;
			fact.DVRDateKey = raw.DVRDate.Value.Date;
			fact.DVRDate = raw.DVRDate.Value;
			fact.T_PACID = PACIDbyraw( raw.T_PACID.Value);
			fact.C_CameraNumber = CameraNBbyraw( raw.C_CameraNumber.Value);
			fact.MonitoredLocID = locID.LocationID;
			fact.C_ObjectTypeID = objtype.ObjectTypeID;
			fact.C_Count = raw.C_Count;
			isnewmodel = true;
			return fact;
		}

		Dim_IOPC_Monitored_Location GetDimMonitored_Location(int locationID)
		{
			Dim_IOPC_Monitored_Location locID = GetDim<Dim_IOPC_Monitored_Location>(it => it.LocationID_BK == locationID && string.Compare(it.SourceName, STR_Count, true) == 0);
			return locID;
		}
		Dim_IOPC_ObjectType Get_Dim_IOPC_ObjectType( int objectID)
		{
			Dim_IOPC_ObjectType locID = GetDim<Dim_IOPC_ObjectType>(it => it.ObjectTypeID_BK == objectID && string.Compare(it.SourceName, STR_Count, true) == 0);
			return locID;
		}
	}
}
