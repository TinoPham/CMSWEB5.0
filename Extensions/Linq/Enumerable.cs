using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions.Linq
{
	public static class Enumerable
	{
		public static IEnumerable<TR> LeftOuterGroupJoin<TA, TB, TK, TR>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB, Func<IEnumerable<TA>, IEnumerable<TB>, TK, TR> projection, IEqualityComparer<TK> cmp = null)
		{
			cmp = cmp ?? EqualityComparer<TK>.Default;
			var alookup = a.ToLookup(selectKeyA, cmp);
			var blookup = b.ToLookup(selectKeyB, cmp);

			var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
			//keys.UnionWith(blookup.Select(p => p.Key));

			var join = from key in keys
					   let xa = alookup [key]
					   let xb = blookup [key]
					   select projection(xa, xb, key);

			return join;
		}

		public static IEnumerable<TR> FullOuterGroupJoin<TA, TB, TK, TR>( this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB, Func<IEnumerable<TA>, IEnumerable<TB>, TK, TR> projection, IEqualityComparer<TK> cmp = null)
		{
			cmp = cmp ?? EqualityComparer<TK>.Default;
			var alookup = a.ToLookup(selectKeyA, cmp);
			var blookup = b.ToLookup(selectKeyB, cmp);

			var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
			keys.UnionWith(blookup.Select(p => p.Key));

			var join = from key in keys
					   let xa = alookup [key]
					   let xb = blookup [key]
					   select projection(xa, xb, key);

			return join;
		}

		public static IEnumerable<TR> FullOuterJoin<TA, TB, TK, TR>( this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB, Func<TA, TB, TK, TR> projection, TA defaultA = default(TA), TB defaultB = default(TB), IEqualityComparer<TK> cmp = null)
		{
			cmp = cmp ?? EqualityComparer<TK>.Default;
			var alookup = a.ToLookup(selectKeyA, cmp);
			var blookup = b.ToLookup(selectKeyB, cmp);

			var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
			keys.UnionWith(blookup.Select(p => p.Key));

			var join = from key in keys.Distinct(cmp)
					   from xa in alookup [key].DefaultIfEmpty()
					   from xb in blookup [key].DefaultIfEmpty()
					   select projection(xa, xb, key);

			return join;
		}

		public static IEnumerable<TR> LeftOuterJoin<TA, TB, TK, TR>( this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB, Func<TA, TB, TK, TR> projection, TA defaultA = default(TA), TB defaultB = default(TB), IEqualityComparer<TK> cmp = null)
		{
			cmp = cmp ?? EqualityComparer<TK>.Default;
			var alookup = a.ToLookup(selectKeyA, cmp);
			var blookup = b.ToLookup(selectKeyB, cmp);

			var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
			//keys.UnionWith(blookup.Select(p => p.Key));

			var join = from key in keys.Distinct(cmp)
					   from xa in alookup [key]
					   from xb in blookup [key].DefaultIfEmpty()
					   select projection(xa, xb, key);

			return join;
		}
	}
}
