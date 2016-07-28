using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.EntityCaches;
using PACDMModel.Model;

namespace CMSWebApi.Cache.DBWarehouse.Handler
{
	internal class DimIOPCMonitoredLocationTrafficCountRegionName : Dimbase<Dim_IOPC_Monitored_Location, tbl_IOPC_TrafficCountRegionName>
	{
		const string str_TrafficCount = "TrafficCount";
		public DimIOPCMonitoredLocationTrafficCountRegionName(PACDMModel.Model.IResposity dbcontext, Caches.CacheManager CacheManager) : base(dbcontext, CacheManager) { }
		protected override Dim_IOPC_Monitored_Location ToDim(WarehouseMap map, tbl_IOPC_TrafficCountRegionName rawvalue)
		{
			return new Dim_IOPC_Monitored_Location
			{
				LocationID_BK = rawvalue.RegionNameID,
				SourceName = str_TrafficCount,
				Location = rawvalue.RegionName
			};
		}
		protected override Dim_IOPC_Monitored_Location CheckValidData(WarehouseMap map, tbl_IOPC_TrafficCountRegionName rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Dim_IOPC_Monitored_Location> icahce = CacheManager.ResolveEntity<Dim_IOPC_Monitored_Location>();
			if (icahce == null || icahce.Results == null)
				return null;
			IEnumerable<Dim_IOPC_Monitored_Location> items = icahce.Results;
			Dim_IOPC_Monitored_Location item = items.FirstOrDefault(it => it.LocationID_BK == rawvalue.RegionNameID && string.Compare(it.SourceName, str_TrafficCount, true) == 0);
			return item;
		}
	}
}
