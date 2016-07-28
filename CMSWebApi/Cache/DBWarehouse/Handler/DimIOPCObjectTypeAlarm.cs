using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACDMModel.Model;
using CMSWebApi.Cache.EntityCaches;

namespace CMSWebApi.Cache.DBWarehouse.Handler
{
	internal class DimIOPCObjectTypeAlarm : Dimbase<Dim_IOPC_ObjectType, tbl_IOPC_AlarmObjectType>
	{
		const string str_ALarm = "Alarm";
		public DimIOPCObjectTypeAlarm(PACDMModel.Model.IResposity dbcontext, Caches.CacheManager CacheManager) : base(dbcontext, CacheManager) { }
		protected override Dim_IOPC_ObjectType ToDim(WarehouseMap map, tbl_IOPC_AlarmObjectType rawvalue)
		{
			return new Dim_IOPC_ObjectType
			{
				ObjectTypeID_BK = rawvalue.ObjectTypeID,
				SourceName = str_ALarm,
				ObjectType = rawvalue.ObjectType
			};
		}
		protected override Dim_IOPC_ObjectType CheckValidData(WarehouseMap map, tbl_IOPC_AlarmObjectType rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Dim_IOPC_ObjectType> icahce = CacheManager.ResolveEntity<Dim_IOPC_ObjectType>();
			if (icahce == null || icahce.Results == null)
				return null;
			IEnumerable<Dim_IOPC_ObjectType> items = icahce.Results;
			Dim_IOPC_ObjectType item = items.FirstOrDefault(it => it.ObjectTypeID_BK == rawvalue.ObjectTypeID && string.Compare(it.SourceName, str_ALarm, true) == 0);
			return item;
		}
	}
}
