using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Cache.DBWarehouse.Facts
{
	internal class FactEventArgs<Tfact> : EventArgs where Tfact : class
	{
		public Tfact Data{ get; private set;}
		internal FactEventArgs(Tfact data )
		{
			Data = data;
		} 
	}
}
