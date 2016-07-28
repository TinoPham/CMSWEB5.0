using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Utils
{
	public class TEqualityComparer<T> : IEqualityComparer<T>
	{
		public TEqualityComparer(Func<T, T, bool> cmp)
		{
			this.cmp = cmp;
		}
		public bool Equals(T x, T y)
		{
			return cmp(x, y);
		}

		public int GetHashCode(T obj)
		{
			if( obj == null)
				return 0;

			return obj.GetType().GetHashCode();
		}

		public Func<T, T, bool> cmp { get; set; }
	}
}
