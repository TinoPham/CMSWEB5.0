using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.EntityCaches;
using PACDMModel.Model;

namespace CMSWebApi.Cache.DBWarehouse.Handler
{
	internal class DimIOPCMonitoredLocationAlarmArea : Dimbase<Dim_IOPC_Monitored_Location, tbl_IOPC_AlarmArea>
	{
		const string str_ALarm = "Alarm";
		public DimIOPCMonitoredLocationAlarmArea(PACDMModel.Model.IResposity dbcontext, Caches.CacheManager CacheManager) : base( dbcontext, CacheManager){}

		protected override Dim_IOPC_Monitored_Location ToDim(WarehouseMap map, tbl_IOPC_AlarmArea rawvalue)
		{
			return new Dim_IOPC_Monitored_Location{
			LocationID_BK = rawvalue.AreaID,
			SourceName = str_ALarm,
			Location = rawvalue.AreaName
			};
		}
		protected override Dim_IOPC_Monitored_Location CheckValidData(WarehouseMap map, tbl_IOPC_AlarmArea rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Dim_IOPC_Monitored_Location> icahce = CacheManager.ResolveEntity<Dim_IOPC_Monitored_Location>();
			if (icahce == null || icahce.Results == null)
				return null;
			IEnumerable<Dim_IOPC_Monitored_Location> items = icahce.Results;
			Dim_IOPC_Monitored_Location item = items.FirstOrDefault( it => it.LocationID_BK == rawvalue.AreaID &&  string.Compare(it.SourceName, str_ALarm, true) == 0);
			return item;
		}
	}
}
