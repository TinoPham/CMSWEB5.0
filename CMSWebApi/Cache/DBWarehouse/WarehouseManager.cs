using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CMSWebApi.Cache.DBWarehouse;
using System.Reflection;
using System.IO;
using Commons;
using Extensions.String;
using PACDMModel.Model;
using CMSWebApi.Cache.DBWarehouse.Facts;
using CMSWebApi.Cache.EntityCaches;

namespace CMSWebApi.Cache.DBWarehouse
{
	public class WarehouseManager : Commons.SingletonClassBase<WarehouseManager>
	{
		const string FileName = "WavehouseMapping.xml";
		const string STR_Items = "Items";
		const string STR_Item = "Items";
		const string STR_Raw = "Raw";
		const string STR_Dim = "Dim";
		const string STR_value = "value";
		const string STR_key = "key";
		const string STR_detectchange = "detectchange";
		const string STR_Handler = "Handler";
		const string STR_Dimbase = "Dimbase";
		const string Method_UpdateWaveHouse = "UpdateWareHouse";
		const string Method_ModifyWareHouse = "ModifyWareHouse";
		const string Method_ValidDataWaveHouse = "ValidDataWareHouse";
		
		readonly Dictionary<string, WarehouseMap>Mappings;
		//readonly Dictionary<Type,Type>FactRawMaps;
		readonly Dictionary<string, Type> FactHandleMaps;
		readonly BackgroundTaskManager BackgroundTask;
		private WarehouseManager()
		{
			BackgroundTask = BackgroundTaskManager.Instance;
			Mappings = new Dictionary<string, WarehouseMap>();
			FactHandleMaps = new Dictionary<string,Type>();
			LoadConfig();
			InitRawMaps();
		}
		
		private void InitRawMaps()
		{
			Type facttype = typeof(Fact_POS_Periodic_Hourly_Transact);
			FactHandleMaps.Add(facttype.Name, typeof(FactPOSPeriodicHourlyTransact));

			facttype = typeof(Fact_POS_Transact);
			FactHandleMaps.Add(facttype.Name, typeof(FactPOSTransact));

			facttype = typeof(Fact_IOPC_Periodic_Hourly_Traffic);
			FactHandleMaps.Add(facttype.Name, typeof(FactIOPCPeriodicHourlyTraffic));

			facttype = typeof(Fact_IOPC_Count);
			FactHandleMaps.Add(facttype.Name, typeof(FactIOPCCount));

            facttype = typeof(Fact_IOPC_DriveThrough);
            FactHandleMaps.Add(facttype.Name, typeof(FactIOPCDriveThrough));

            facttype = typeof(Fact_IOPC_TrafficCount);
            FactHandleMaps.Add(facttype.Name, typeof(FactIOPCTrafficCount));

            facttype = typeof(Fact_IOPC_LPR_Info);
            FactHandleMaps.Add(facttype.Name, typeof(FactIOPCLPRInfo));

		}

		private FactBase<Tfact,Traw> ResolveFactHandle<Tfact,Traw>() where Tfact : class where Traw : class
		{
			FactBase<Tfact,Traw> handler = null;
			Type handletype = ResolveFactHandleType( typeof(Tfact));
			if (handletype != null)
				handler = Activator.CreateInstance(handletype) as FactBase<Tfact, Traw>;
			return handler;
		}
		private Type ResolveFactHandleType( Type facttype) 
		{
			if( facttype == null)
				return null;
			KeyValuePair<string, Type> item = FactHandleMaps.FirstOrDefault( it => string.Compare( it.Key, facttype.Name , true) == 0);
			return string.IsNullOrEmpty(item.Key)? null : item.Value;
		}

		private Stream ResourceStream()
		{
			Assembly assem = this.GetType().Assembly;
			string path = this.GetType().Namespace;
			path +=	"." + FileName;
			return assem.GetManifestResourceStream(path);
		}

		private void LoadConfig()
		{
			Stream stream = ResourceStream();
			if(stream == null)
				return;
			try
			{
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.Load(stream);
				XmlNode root = xmldoc.DocumentElement;
				XmlNodeList childs = root.ChildNodes;
				WarehouseMap map = null;
				foreach(XmlNode item in childs)
				{
					map = ElementtoMap( item);
					if( Mappings.ContainsKey(map.Raw))
						continue;
					map.Initialise();
					if( map.DimType == null || map.RawType == null || map.HandlerType == null)
					{
						if( map.DimType != null)
							BackgroundTask.CacheMgr.RegisterEntityCache(map.DimType, typeof(PACDMModel.Model.PACDMDB), map.DetectChange);
						continue;
					}
					Mappings.Add( map.Raw, map);

					BackgroundTask.CacheMgr.RegisterEntityCache(map.DimType, typeof(PACDMModel.Model.PACDMDB), map.DetectChange);
				}
			}
			finally{
				stream.Close();
				stream.Dispose();
				stream = null;
			}

		}

		private WarehouseMap ElementtoMap(XmlNode node)
		{
			if( node == null)
				return null;
			WarehouseMap itemobject = new WarehouseMap();
			string dim = Commons.XMLUtils.XMLAttributeValue(node, STR_Dim);
			string raw = Commons.XMLUtils.XMLAttributeValue(node, STR_Raw);
			string handler = Commons.XMLUtils.XMLAttributeValue(node, STR_Handler);
			string detectchange = Commons.XMLUtils.XMLAttributeValue(node, STR_detectchange);
			itemobject.Dim = dim;
			itemobject.Raw = raw;
			itemobject.Handler = handler;
			itemobject.DetectChange = string.IsNullOrEmpty(detectchange)? false : detectchange.toBool();
			XmlNodeList columns = node.ChildNodes;
			ColumnMap colmap = null;
			List<ColumnMap> cols = new List<ColumnMap>();
			foreach( XmlNode child in columns )
			{
				colmap = ElementColumnMap( child);
				cols.Add( colmap);
			}
			itemobject.Columns = cols;
			return itemobject;
		}
		
		private ColumnMap ElementColumnMap(XmlNode node)
		{
			ColumnMap item = new ColumnMap();
			item.DimColumn = Commons.XMLUtils.XMLAttributeValue(node, STR_Dim);
			item.RawColumn = Commons.XMLUtils.XMLAttributeValue(node, STR_Raw);
			item.Key = Commons.XMLUtils.XMLAttributeValue(node, STR_key).toBool();
			return item;
		}

		WarehouseMap GetMap(string name)
		{
			KeyValuePair<string, WarehouseMap> item = Mappings.FirstOrDefault(it => string.Compare(it.Key, name, true) == 0);
			return item.Value;
		}
		#region Dim table

		private object UpdateDim(PACDMModel.Model.IResposity PACContext, object value, WarehouseMap map, string method_Name)
		{
			if( map.HandlerType == null || map.RawType == null || map.DimType == null)
				return null;
			try
			{	 
				Type constructor = map.HandlerType;
				
				if( constructor == null)
					return null;

				Object dimhandler = Activator.CreateInstance( constructor, new object[]{PACContext, BackgroundTaskManager.Instance.CacheMgr });
				if( dimhandler == null )
					return null;

				//MethodInfo func = constructor.GetMethod( Method_UpdateWaveHouse);
				MethodInfo func = constructor.GetMethod(method_Name);
				if( func == null)
					return null;
				return func.Invoke( dimhandler, new object[]{ map, value});
			}
			catch(Exception)
			{
				return null;
			}
		}

		public object ModifyDim(PACDMModel.Model.IResposity PACContext, object value)
		{
			if (value == null || PACContext == null)
				return null;
			WarehouseMap map = GetMap(value.GetType().Name);
			if (map == null)
				return null;
			return UpdateDim(PACContext, value, map, Method_ModifyWareHouse);
		}

		public object UpdateDim(PACDMModel.Model.IResposity PACContext, object value)
		{
			if (value == null || PACContext == null)
				return null ;
			WarehouseMap map = GetMap(value.GetType().Name);
			if(map == null)
				return null;
			return UpdateDim(PACContext, value, map, Method_UpdateWaveHouse);
		}
		
		public object UpdateDim( object value)
		{
			PACDMModel.Model.IResposity PACDBContext = BackgroundTask.CacheMgr.IDBResolver.GetService(typeof(PACDMModel.Model.IResposity)) as PACDMModel.Model.IResposity;
			if( PACDBContext == null)
				return null;
			return UpdateDim(PACDBContext, value);
		}

		public Tdim GetDim<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			IEntityCache<Tdim> ICache = BackgroundTask.ResolveEntityCache<Tdim>();
			if (ICache == null)
				return null;
			IEnumerable<Tdim> items = ICache.Results;
			if (items == null || !items.Any())
				return null;
			Tdim item = items.FirstOrDefault( filter);
			return item;
		}

		public IEnumerable<Tdim> GetDims<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			IEntityCache<Tdim> ICache = BackgroundTask.ResolveEntityCache<Tdim>();
			if (ICache == null)
				return null;
			IEnumerable<Tdim> items = ICache.Results;
			if (items == null || !items.Any())
				return Enumerable.Empty<Tdim>();
			return filter == null? items : items.Where( filter);
		}

		#endregion

		public Tfact AddFactData<Tfact, Traw>(PACDMModel.Model.IResposity dbmodel, Traw raw) where Tfact : class where Traw : class
		{
			if( raw == null || dbmodel == null)
				return null;

			Facts.FactBase<Tfact,Traw> handler = ResolveFactHandle<Tfact,Traw>();
			if( handler == null)
				return null;
			return handler.AddFact( dbmodel, raw);
		}
		public PACDMModel.Model.Fact_POS_Periodic_Hourly_Transact POSTransactionSummaryInfo( PACDMModel.Model.IResposity DBModel, int kdvr, DateTime TransDate, int TransHour, int? Count_Trans, decimal TotalAmount)
		{
			return new FactPOSPeriodicHourlyTransact().SummaryInfo(DBModel, kdvr, TransDate, TransHour, Count_Trans, TotalAmount);
		}
		//public object  GetWareHourItem( object value)
		//{
		//	if( value == null)
		//		return null;
		//	Type type = value.GetType();
		//	return GetWareHourItem(type, value );
		//}
		//public object GetWareHourItem(Type itemtype, object value)
		//{
		//	if( itemtype == null || value == null)
		//		return null;
		//	if( string.Compare( itemtype.Name, value.GetType().Name, true) != 0)
		//		return null;
		//	WarehouseMap map = GetMap( itemtype.Name);
		//	if( map == null)
		//		return null;

		//}
	}

}
