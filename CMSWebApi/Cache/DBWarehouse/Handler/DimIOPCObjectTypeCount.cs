using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.Cache.EntityCaches;
using PACDMModel.Model;

namespace CMSWebApi.Cache.DBWarehouse.Handler
{
	internal class DimIOPCObjectTypeCount : Dimbase<Dim_IOPC_ObjectType, tbl_IOPC_Count_ObjectType>
	{
		const string str_Count = "Count";
		public DimIOPCObjectTypeCount(PACDMModel.Model.IResposity dbcontext, Caches.CacheManager CacheManager) : base(dbcontext, CacheManager) { }
		protected override Dim_IOPC_ObjectType ToDim(WarehouseMap map, tbl_IOPC_Count_ObjectType rawvalue)
		{
			return new Dim_IOPC_ObjectType
			{
				ObjectTypeID_BK = rawvalue.ObjectTypeID,
				SourceName = str_Count,
				ObjectType = rawvalue.ObjectType
			};
		}
		protected override Dim_IOPC_ObjectType CheckValidData(WarehouseMap map, tbl_IOPC_Count_ObjectType rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Dim_IOPC_ObjectType> icahce = CacheManager.ResolveEntity<Dim_IOPC_ObjectType>();
			if (icahce == null || icahce.Results == null)
				return null;
			IEnumerable<Dim_IOPC_ObjectType> items = icahce.Results;
			Dim_IOPC_ObjectType item = items.FirstOrDefault(it => it.ObjectTypeID_BK == rawvalue.ObjectTypeID && string.Compare(it.SourceName, str_Count, true) == 0);
			return item;
		}
	}

}
