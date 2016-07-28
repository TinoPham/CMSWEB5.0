using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataModels
{
	public class CmsMetadata<T> where T: class
	{
		private readonly long? _countline;
		private readonly IEnumerable<T> _data;

		public IEnumerable<T> DataResult {
			get { return _data; }
		}

		public long? Countline
		{
			get { return _countline; }
		}

		public CmsMetadata(long? count, IEnumerable<T> data)
		{
			_countline = count;
			_data = data;
		}
	}
}
