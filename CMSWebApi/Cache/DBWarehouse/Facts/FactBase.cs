using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using PACDMModel.Model;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
	internal abstract class FactBase<Tfact,Traw> : IDisposable where Tfact : class where Traw : class
	{
		internal const string STR_Count = "Count";
		internal const string STR_IOPC_OUT = "OUT#";
		internal const string STR_IOPC_IN = "IN#";
		readonly Regex Rx_Traffic_InOut = new Regex(@"^(IN\#|OUT\#)");

		protected BackgroundTaskManager BackgroungTask{ get ;private set;}
		protected WarehouseManager WHManager{ get; private set;}
		internal FactBase()
		{
			BackgroungTask = BackgroundTaskManager.Instance;
			WHManager = WarehouseManager.Instance;
		}
		public void Dispose()
		{

		}

		protected virtual Tfact UpdateFact( PACDMModel.Model.IResposity pacdb, Traw raw)
		{
			bool isnewmodel = false;
			
			Tfact fact = null;
			fact = ToFactModel(pacdb, raw, ref isnewmodel);
			if( fact != null)
			{
				if( isnewmodel)
					pacdb.Insert<Tfact>( fact);
				else
					pacdb.Update<Tfact>( fact);
				 int saved = pacdb.Save();
				if(saved < 0)
					fact = null;
			}
			return fact;
		}
		
		protected virtual Tfact ToFactModel(IResposity pacdb, Traw raw, ref bool isnewmodel)
		{
			return default(Tfact);
		}
		public Tfact AddFact(PACDMModel.Model.IResposity Imodel, Traw raw)
		{
			return UpdateFact(Imodel, raw);
		}

		protected Tdim GetDim<Tdim>(Func<Tdim, bool> filter) where Tdim : class
		{
			return WHManager.GetDim<Tdim>(filter);
		}
		protected Tout GetDim<Tdim, Tout>(Func<Tdim, bool> filter, Func<Tdim, Tout> selector) where Tdim : class
		{
			Tdim search = WHManager.GetDim<Tdim>(filter);
			return search == null? default(Tout) : selector.Invoke(search);
		}

		protected Dim_POS_PACID GetPACIDbyRaw(int value)
		{
			IEnumerable<Dim_POS_PACID> all_pacs = WHManager.GetDims<Dim_POS_PACID>( it => it.KDVR == value);
			if(!all_pacs.Any())
				return null;

			return all_pacs.OrderByDescending( it => it.PACID_ID).FirstOrDefault();
		}

		protected Dim_POS_PACID GetPACIDbyDim(int value)
		{
			return GetDim<Dim_POS_PACID>(it => it.PACID_ID == value);
		}

		protected Dim_POS_CameraNB GetCamerabyDim(int dimcamnb)
		{
			return GetDim<Dim_POS_CameraNB>(it => it.CameraNB_ID == dimcamnb);
		}

		protected Dim_POS_CameraNB GetCamerabyRaw( int rawvalue)
		{
			return GetDim<Dim_POS_CameraNB>( it => it.CameraNB_BK == rawvalue);
		}
		protected int CameraNBbydim( int dim)
		{
			Dim_POS_CameraNB it = GetCamerabyDim( dim);
			return it == null ? 0 : it.CameraNB_ID;
		}

		protected int CameraNBbyraw(int raw)
		{
			Dim_POS_CameraNB it = GetCamerabyRaw(raw);
			return it == null ? 0 : it.CameraNB_ID;
		}
		protected int PACIDbyDim( int dim)
		{
		    Dim_POS_PACID it = GetPACIDbyDim(dim);
			return it == null ? 0 : it.PACID_ID;
		}
		protected int PACIDbyraw(int raw)
		{
			Dim_POS_PACID it = GetPACIDbyRaw(raw);
			return it == null ? 0 : it.PACID_ID;
		}
		protected bool isTrafficCount_In_Out(string AreaName)
		{
			if( string.IsNullOrEmpty(AreaName))
				return false;
			return Rx_Traffic_InOut.IsMatch(AreaName);
		}
	}
}
