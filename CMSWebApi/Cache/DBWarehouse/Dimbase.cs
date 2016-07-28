using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;
using CMSWebApi.Cache;
using System.Xml.Serialization;
using PACDMModel.Model;
using CMSWebApi.Cache.EntityCaches;
using Extensions.Linq;


namespace CMSWebApi.Cache.DBWarehouse
{
	internal class Dimbase<Tdim,Traw>: IDisposable where Tdim: class, new() where Traw: class
	{
		protected Caches.CacheManager CacheManager;
		protected PACDMModel.Model.IResposity PACDMContext;
		public Dimbase(PACDMModel.Model.IResposity dbcontext, Caches.CacheManager CacheManager)
		{
			this.CacheManager = CacheManager;
			PACDMContext = dbcontext;
		}

		protected string Filter(WarehouseMap map, Traw rawvalue, out object [] param)
		{
			List<ColumnMap> columns = map.Columns.Where( it => it.Key == true).ToList();
			int count = columns.Count();
			param = new object[count];
			
			string[]sql = new string[count];
			ColumnMap col = null;
			for( int index = 0; index < count; index++)
			{
				col = columns[index];
				param[ index] = Commons.ObjectUtils.GetPropertyValue(rawvalue, col.RawColumn);
				sql[index] = string.Format("{0} == @{1}",  col.DimColumn, index);
			}
			string fullsql = string.Join(" and ", sql);
			return fullsql;
		}

		protected virtual Tdim ToDim(WarehouseMap map, Traw rawvalue)
		{
			Tdim result = new Tdim();
			ToModifyDim(map, result, rawvalue);
			//foreach(ColumnMap col in map.Columns)
			//{
			//	Commons.ObjectUtils.SetPropertyValue( result, col.DimColumn, Commons.ObjectUtils.GetPropertyValue(rawvalue, col.RawColumn));
			//}
			return result;
		}
		protected virtual void ToModifyDim(WarehouseMap map, Tdim tdim, Traw rawvalue)
		{
			if( tdim == null || rawvalue == null)
				return;

			foreach (ColumnMap col in map.Columns)
			{
				try
				{
					Commons.ObjectUtils.SetPropertyValue(tdim, col.DimColumn, Commons.ObjectUtils.GetPropertyValue(rawvalue, col.RawColumn));
				}catch(Exception){}
			}
		}

		protected virtual Tdim CheckValidData(WarehouseMap map, Traw rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Tdim> icahce = CacheManager.ResolveEntity<Tdim>();
			if (icahce == null || icahce.Results == null)
				return null;
			IQueryable<Tdim> items = icahce.Results.AsQueryable();
			object [] param = null;
			string sql = Filter(map, rawvalue, out param);
			Tdim valid = items.Where(sql, param).FirstOrDefault();
			return valid;
		}

		public virtual bool ValidDataWareHouse(WarehouseMap map, Traw rawvalue)
		{
			return CheckValidData(map, rawvalue) != null;
		}

		public virtual Tdim ModifyWareHouse(WarehouseMap map, Traw rawvalue)
		{
			if (rawvalue == null)
				return null;
			IEntityCache<Tdim> icahce = CacheManager.ResolveEntity<Tdim>();
			if (icahce == null || icahce.Results == null)
				return null;
			Tdim valid = CheckValidData(map, rawvalue);
			if( valid == null)
				return null;
			
			try
			{
				ToModifyDim(map, valid, rawvalue);
				PACDMContext.Update<Tdim>( valid);
				return PACDMContext.Save() >= 0? valid : null;
			}
			catch (Exception) { valid = null; }
			return valid;

		}
		public virtual Tdim UpdateWareHouse(WarehouseMap map, Traw rawvalue)
		{
			if( rawvalue == null)
				return null;
			IEntityCache<Tdim> icahce = CacheManager.ResolveEntity<Tdim>();
			if( icahce == null || icahce.Results == null)
				return null;
			Tdim valid = CheckValidData(map, rawvalue);

			if( valid != null)
				return valid;
			try
			{
				valid = ToDim(map, rawvalue);
				PACDMContext.Insert<Tdim>( valid);
				PACDMContext.Save();
				icahce.AddItemCache(valid);
			}
			catch(Exception){ valid = null; }
			return valid;
		}

		public virtual void Dispose()
		{
			
		}
	}

	//internal class Mappings
	//{
	//	public List<Mapping> Items { get; set; }
	//}
	
	internal class WarehouseMap
	{
		const string STR_Handler = "Handler";
		const string STR_Dimbase = "Dimbase";
		string handler_namespace;
		string dim_raw_name_space;
		readonly string PACDM_Assembly = typeof(PACDMDB).Assembly.FullName; 

		public WarehouseMap()
		{
			handler_namespace = string.Format("{0}.{1}", this.GetType().Namespace ,STR_Handler);
			dim_raw_name_space = typeof(PACDMModel.Model.PACDMDB).Namespace;
		}

		string _HandlerTypePath = null;
		public string HandlerTypePath { get { return _HandlerTypePath; } }

		
		public string DimTypePath{ get{ return string.Format("{0}.{1}", dim_raw_name_space, Dim);} }

		public string RawTypePath { get { return string.Format("{0}.{1}", dim_raw_name_space, Raw); } }

		public Type	DimType{ get { return _DimType;}}
		public Type RawType { get { return _RawType; } }
		public Type HandlerType { get { return _HandlerType; } }
		Type _DimType;
		Type _RawType;
		Type _HandlerType;
		public bool DetectChange{ get ;set;}
		public void Initialise()
		{


			_RawType = Type.GetType(RawTypePath + "," + PACDM_Assembly, false, true);
			_DimType = Type.GetType(DimTypePath + "," + PACDM_Assembly, false, true);
			if( _DimType == null || _RawType == null )
				return;

			if( string.IsNullOrEmpty( Handler))
			{
				handler_namespace = this.GetType().Namespace;
				Handler = STR_Dimbase;
				_HandlerTypePath = string.Format("{0}.{1}`2", handler_namespace, Handler);

				Type hwndType = Type.GetType(HandlerTypePath);
				if (hwndType != null)
				{
					_HandlerType = hwndType.MakeGenericType(new Type [] { _DimType, _RawType });
				}
			}
			else
			{
				_HandlerTypePath = string.Format("{0}.{1}", handler_namespace, Handler);
				_HandlerType = Type.GetType(HandlerTypePath);
			}
		}
		public string Raw { get; set; }
		public string Dim { get; set; }
		public string Handler{ get ;set;}
		public IEnumerable<ColumnMap> Columns { get; set; }
	}
	
	internal class ColumnMap
	{
		public string RawColumn { get; set; }
		public string DimColumn { get; set; }
		public bool Key { get; set; }
	}
}
