using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Cache.EntityCaches
{
	internal class DataEventArgs<T> : EventArgs
	{
		public IEnumerable<T> Results { get; set; }
	}
	internal class EntityDataEventArgs<T> : DataEventArgs<T>//EventArgs
	{
		//public IEnumerable<T> Results { get; set; }
		public Expression<Func<T, bool>> Query{ get ;set;}
	}

	//internal class EntityChangeEventArgs<T> : EntityDataEventArgs<T>//;//EventArgs
	//{
	//	public IEnumerable<T> Results { get; set; }
	//	public bool ContinueListening { get; set; }
	//}

	internal static class EntityChangeNotifier
	{
		private static readonly List<string> _connectionStrings;
		private static object _lockObj = new object();

		static EntityChangeNotifier()
		{
			_connectionStrings = new List<string>();

			AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
			{
				foreach (var cs in _connectionStrings)
					SqlDependency.Stop(cs);
			};
		}

		public static void RemoveConnectionString(string cs)
		{
				lock (_lockObj)
				{
					if (_connectionStrings.Contains(cs))
					{
						StopDependency(cs);
						_connectionStrings.Remove(cs);
					}
				}
		}

		public static void AddConnectionString(string cs)
		{
				lock (_lockObj)
				{
					if (!_connectionStrings.Contains(cs))
					{
						if( StartDependency(cs))
							_connectionStrings.Add(cs);
					}
				}
		}

		private static bool StartDependency(string cs)
		{
			if( string.IsNullOrEmpty(cs))
				return false;
			try
			{
				return SqlDependency.Start( cs);

			}
			//catch(System.ArgumentNullException){}
			catch(System.InvalidOperationException){}
			catch(System.Security.SecurityException){}
			catch (System.Data.SqlClient.SqlException) { }

			return false;
		}
		private static bool StopDependency(string cs)
		{
			if (string.IsNullOrEmpty(cs))
				return false;
			try
			{
				return SqlDependency.Stop(cs);

			}
			//catch(System.ArgumentNullException){}
			catch (System.InvalidOperationException) { }
			catch (System.Security.SecurityException) { }
			catch (System.Data.SqlClient.SqlException) { }

			return false;
		}
	}
}
