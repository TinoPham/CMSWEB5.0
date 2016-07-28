using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.DBWarehouse;
using Commons;
using PACDMModel.Model;

namespace CMSWebApi.Wrappers.Handlers
{
	public class Warehouse: Commons.SingletonClassBase<Wrappers.Handlers.Warehouse>
	{
		public object UpdateDim(IResposity PACContext, object value)
		{
			return WarehouseManager.Instance.UpdateDim(PACContext, value);
		}
		public object ModifyDim(IResposity PACContext, object value)
		{
			return WarehouseManager.Instance.ModifyDim(PACContext, value);
		}

		public Task<object> UpdateDimsync(IResposity PACContext, object value)
		{

			return Cache.BackgroundTaskManager.Instance.Run<object>( ()=>
			{
				return WarehouseManager.Instance.UpdateDim(PACContext, value);
			});
			
		}

		public object UpdateDim(object value)
		{
			return WarehouseManager.Instance.UpdateDim(value);
		}

		public Task<object> UpdateDimAsync(object value)
		{
			return Cache.BackgroundTaskManager.Instance.Run<object>(() =>
			{
				return WarehouseManager.Instance.UpdateDim(value);
			});
		}

		public Tdim GetDim<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			return WarehouseManager.Instance.GetDim<Tdim>(filter);
		}

		public Task<Tdim> GetDimAsync<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			return Cache.BackgroundTaskManager.Instance.Run<Tdim>(() =>
			{
				return WarehouseManager.Instance.GetDim<Tdim>(filter);
			});
		}

		public IEnumerable<Tdim> GetDims<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			return WarehouseManager.Instance.GetDims<Tdim>(filter);
		}

		public Task<IEnumerable<Tdim>> GetDimsAsync<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			return Cache.BackgroundTaskManager.Instance.Run<IEnumerable<Tdim>>(() =>
			{
				return WarehouseManager.Instance.GetDims<Tdim>(filter);
			});
		}

		public Tfact AddFactData<Tfact, Traw>(PACDMModel.Model.IResposity dbmodel, Traw raw)
			where Tfact : class
			where Traw : class
		{
			return WarehouseManager.Instance.AddFactData<Tfact, Traw>(dbmodel, raw);
		}

		public Task<Tfact> AddFactDataAsync<Tfact, Traw>(PACDMModel.Model.IResposity dbmodel, Traw raw)
			where Tfact : class
			where Traw : class
		{
			return Cache.BackgroundTaskManager.Instance.Run<Tfact>(() =>
			{
				return WarehouseManager.Instance.AddFactData<Tfact, Traw>(dbmodel, raw);
			});
			
		}

        public void UpdateWareHouse(tbl_LPR_Info raw, IResposity pacdb)
        {
            if (raw == null) return;
            if (raw.tbl_POS_CameraNBList == null)
                pacdb.Include<tbl_LPR_Info, tbl_POS_CameraNBList>(raw, it => it.tbl_POS_CameraNBList);
            Fact_IOPC_LPR_Info fact = AddFactData<Fact_IOPC_LPR_Info, tbl_LPR_Info>(pacdb, raw);
            return;
        }

		public void UpdateWareHouse(tbl_IOPC_Count raw, IResposity pacdb)
		{
			if (raw == null || raw.Count_ID <= 0)
				return;
			if( raw.tbl_IOPC_Count_Area == null)
				pacdb.Include<tbl_IOPC_Count, tbl_IOPC_Count_Area>( raw, it => it.tbl_IOPC_Count_Area);

			Fact_IOPC_Count fact = AddFactData<Fact_IOPC_Count, tbl_IOPC_Count>(pacdb, raw);
			if (fact != null && fact.Count_ID > 0)
			{
				if (fact.Dim_IOPC_Monitored_Location == null)
					pacdb.Include<Fact_IOPC_Count, Dim_IOPC_Monitored_Location>(fact, it => it.Dim_IOPC_Monitored_Location);
				Fact_IOPC_Periodic_Hourly_Traffic fact_period = AddFactData<Fact_IOPC_Periodic_Hourly_Traffic, Fact_IOPC_Count>(pacdb, fact);
			}
		}

        public void UpdateWareHouse(tbl_IOPC_DriveThrough raw, IResposity pacdb)
        {
            if (raw == null) return;
            if(raw.tbl_POS_CameraNBList == null )
            pacdb.Include<tbl_IOPC_DriveThrough, tbl_POS_CameraNBList>(raw, it => it.tbl_POS_CameraNBList);
            Fact_IOPC_DriveThrough fact = AddFactData<Fact_IOPC_DriveThrough, tbl_IOPC_DriveThrough>(pacdb, raw);
            return;
        }

        public void UpdateWareHouse(tbl_IOPC_TrafficCount raw, IResposity pacdb)
        {
            if (raw == null) return;
            if (raw.tbl_IOPC_TrafficCountRegion == null)
                pacdb.Include<tbl_IOPC_TrafficCount, tbl_IOPC_TrafficCountRegion>(raw, it => it.tbl_IOPC_TrafficCountRegion);
            if (raw.tbl_IOPC_TrafficCountRegion.tbl_IOPC_TrafficCountRegionName == null)
            {
                pacdb.Include<tbl_IOPC_TrafficCountRegion, tbl_IOPC_TrafficCountRegionName>(raw.tbl_IOPC_TrafficCountRegion, it => it.tbl_IOPC_TrafficCountRegionName);
            }
            Fact_IOPC_TrafficCount fact = AddFactData<Fact_IOPC_TrafficCount, tbl_IOPC_TrafficCount>(pacdb, raw);
            return;
        }

		public void UpdateWareHouse(tbl_POS_Transact transact, IResposity pacdb)
		{
			if (transact == null || transact.TransID <= 0)
				return;

			Fact_POS_Transact fact_transact = AddFactData<Fact_POS_Transact, tbl_POS_Transact>(pacdb, transact);
			if (fact_transact != null && fact_transact.TransSRID > 0)
			{
				Fact_POS_Periodic_Hourly_Transact fact_period_hourly = AddFactData<Fact_POS_Periodic_Hourly_Transact, Fact_POS_Transact>(pacdb, fact_transact);
			}
		}

		public Fact_POS_Periodic_Hourly_Transact POSTransactionSummary(IResposity pacdb, int kdvrid, DateTime TransDate, int TransHour, int? Count_Trans, decimal TotalAmount)
		{
			return WarehouseManager.Instance.POSTransactionSummaryInfo(pacdb, kdvrid, TransDate, TransHour,Count_Trans, TotalAmount);
		}
	}
}
