using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Threading;
using System.Runtime.Caching;
namespace CMSWebApi.Cache.EntityCaches
{
	internal class SqlCommandDependence<T>:IDisposable
	{
#if !DEBUG
		const int ReConnect_Interval = 1000 * 60;
#else
		const int ReConnect_Interval = 1000 * 5;
#endif
		readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

		public event EventHandler<DataEventArgs<T>> OnChanged;

		DbContext _context;

		SqlCommand _Command;

		SqlCommand Command {
			get {
					return _Command;
				}
			set { _Command = value;}
		}

		bool _hasDataChanged = false;

		//volatile bool ContinueListening = true;

		readonly string _cacheKey;
		
		//BlockingCollection<Tout> Items;

		CancellationToken Schedule_Reconnect_Token;

		readonly ManualResetEvent Evt_Exit = new ManualResetEvent(false);

		bool _detectchange = false;
		readonly object _locker = new object();

		//public EntityCache(CancellationToken canceltoken, DbContext dbcontext, Expression<Func<TEntity, bool>> query, Expression<Func<TEntity, Tout>> selector, bool detectchange = true)
		public SqlCommandDependence(CancellationToken canceltoken, DbContext dbcontext, SqlCommand query, string cachekey, bool detectchange = true)
		{
			_cacheKey = cachekey;
			Schedule_Reconnect_Token = canceltoken;
			_context = dbcontext;
			this.Command = query;
			//_selector = selector;
			_detectchange = detectchange;
			if( _detectchange)
				EntityChangeNotifier.AddConnectionString(_context.Database.Connection.ConnectionString);

		}

		
		private IEnumerable<T> GetCurrent(SqlCommand command, out SqlDependency sqldepend)
		{
			IEnumerable<T> data = null; //new List<Tout>();
			_hasDataChanged = false;
			sqldepend = null;
			try
			{
				//SqlCommand command = SQLCommand();
				if( _detectchange)
				{
					command.Notification = null;
					using (SqlConnection con = new SqlConnection(_context.Database.Connection.ConnectionString))
					{
						Task T_Connect = con.OpenAsync( this.Schedule_Reconnect_Token);
						T_Connect.Wait( Schedule_Reconnect_Token);
						if( Schedule_Reconnect_Token.IsCancellationRequested || T_Connect.IsFaulted || con.State != System.Data.ConnectionState.Open)
							return data;

						command.Notification = null;
						command.Connection = con;
						sqldepend = new SqlDependency(command);
						sqldepend.OnChange += sqldepend_OnChange;
						using (SqlDataReader reader = command.ExecuteReader())
						{
						}
					
					}
				}
				List<SqlParameter> ps = new List<SqlParameter>();
				foreach(SqlParameter p in command.Parameters )
				{
					ps.Add(
							new SqlParameter{ SqlValue = p.SqlValue, Size = p.Size, ParameterName = p.ParameterName} );
				}
				
				data = _context.Database.SqlQuery<T>(command.CommandText, ps.ToArray() ).ToList<T>();
				
			}
			catch(System.InvalidOperationException){}
			catch (System.Data.SqlClient.SqlException) { }
			catch (System.Configuration.ConfigurationErrorsException) { }
			catch(Exception){}
			
			return data;
		}

		private IEnumerable<T> GetResults()
		{
			if( MemoryCache.Default.Contains(_cacheKey))
				return MemoryCache.Default [_cacheKey] as List<T>;
			//First we do a read lock to see if it already exists, this allows multiple readers at the same time.
			cacheLock.EnterReadLock();
			
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				IEnumerable<T> cached = MemoryCache.Default [_cacheKey] as List<T>;

				if (cached != null)
				{
					return cached;
				}
			}
			finally
			{
				cacheLock.ExitReadLock();
			}


			//Only one UpgradeableReadLock can exist at one time, but it can co-exist with many ReadLocks
			cacheLock.EnterUpgradeableReadLock();
			List<T> value = null;
			try
			{
				 //We need to check again to see if the string was created while we where waiting to enter the EnterUpgradeableReadLock
				object cached = MemoryCache.Default.Get(_cacheKey);

				if (cached != null)
					return cached as List<T>;
				//The entry still does not exist so we need to enter the write lock and create it, this will block till all the Readers flush.
				cacheLock.EnterWriteLock();
				try
				{
					CacheItemPolicy policy;

					var item = CacheItem(Command, out policy);
					if( policy == null)
						policy = new CacheItemPolicy{ AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration};
					if (item != null && item.Value != null)
						MemoryCache.Default.Set(item.Key, item.Value, policy);

					value = item == null? null:item.Value as List<T>;
				}
				finally
				{
					cacheLock.ExitWriteLock();
				}
			}

			finally
			{
				cacheLock.ExitUpgradeableReadLock();
			}

			return value;
		}

		CacheItem CacheItem(SqlCommand command, out CacheItemPolicy policy)
		{
			
			SqlChangeMonitor changeMonitor = null;
			SqlDependency sqldepend;
			policy = null;
			if( command == null)
				return null;
			IEnumerable<T> Data = GetCurrent(command, out sqldepend);
			if( Data == null)
				return null;

			if( _detectchange)
			{
				changeMonitor = new SqlChangeMonitor(sqldepend);

				policy = new CacheItemPolicy();
				policy.AbsoluteExpiration = DateTimeOffset.MaxValue;
				policy.ChangeMonitors.Add(changeMonitor);
				policy.UpdateCallback = new CacheEntryUpdateCallback(CacheEntryUpdateCallback);
			}
			return new CacheItem(_cacheKey, Data);
		}

		void sqldepend_OnChange(object sender, SqlNotificationEventArgs e)
		{
			switch (e.Type)
			{
				case SqlNotificationType.Change:

					if (e.Info == SqlNotificationInfo.Delete
						|| e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Truncate
						|| e.Info == SqlNotificationInfo.Update)
					{
						_hasDataChanged = true;
						(sender as SqlDependency).OnChange -= sqldepend_OnChange;
					}
					else
					{
						Task.Run(() => ReInitSchedule(Command, Schedule_Reconnect_Token), Schedule_Reconnect_Token);
					}
					break;
				case SqlNotificationType.Subscribe:

					break;
				case SqlNotificationType.Unknown:

					break;

			}

		}

		private void CacheEntryUpdateCallback(CacheEntryUpdateArguments arguments)
		{
			if(_hasDataChanged)
			{
				_hasDataChanged = false;
				CacheItemPolicy policy;
				var item = CacheItem(Command, out policy);
				arguments.UpdatedCacheItem = item;
				OnDependencyChanged( item.Value as IEnumerable<T>);
				arguments.UpdatedCacheItemPolicy = policy;
			}
		}

		private void OnDependencyChanged(IEnumerable<T> collection)
		{
			//IEnumerable<string> dependencyKeys = (IEnumerable<string>)state;
			if( OnChanged != null)
			{
				DataEventArgs<T> newdata = new EntityDataEventArgs<T>();
				newdata.Results = collection;
				OnChanged(this, newdata);

			}
		}

		private void ReInitSchedule( SqlCommand command, CancellationToken canceltoken)
		{
			CacheItemPolicy policy;
			bool stop = false;
			AutoResetEvent evt_schedule = new AutoResetEvent(false);
			Evt_Exit.Reset();
			WaitHandle[] handlers = new WaitHandle[]{ Evt_Exit, evt_schedule};
			int index = -1;

			while (!stop && !canceltoken.IsCancellationRequested)
			{
				index = WaitHandle.WaitAny(handlers, ReConnect_Interval);
				switch( index)
				{
					case 0:
						stop = true;

					break;
					default:
					{
						var item = CacheItem(command, out policy);
							if( item != null && item.Value != null)
							{
								MemoryCache.Default.Set(item.Key, item.Value, policy);
								stop = true;
							}
						break;
					}
				}
			}
			
			Evt_Exit.Set();
		}

		public IEnumerable<T> Results
		{
			get
			{
				return GetResults();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{

				if( _detectchange)
					EntityChangeNotifier.RemoveConnectionString(_context.Database.Connection.ConnectionString);
				Evt_Exit.Set();
				if(!string.IsNullOrEmpty(_cacheKey))
					MemoryCache.Default.Remove(_cacheKey);
				//if (_context != null)
				//{
				//	_context.Dispose();
				//	_context = null;
				//}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void RemoveItemCache(T value)
		{
			if (_detectchange == true || !MemoryCache.Default.Contains(this._cacheKey))
				return;
			cacheLock.EnterReadLock();
			List<T> cached = null;
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				cached = MemoryCache.Default [_cacheKey] as List<T>;

			}
			finally
			{
				cacheLock.ExitReadLock();
			}
			if (cached == null)
				return;
			lock (_locker)
			{
				if( cached.Contains(value))
				{
					cached.Remove(value);
					MemoryCache.Default.Set(_cacheKey, cached, new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration });
				}
			}

		}

		public void AddItemCache(T value)
		{
			if( _detectchange == true || ! MemoryCache.Default.Contains( this._cacheKey))
				return;

			cacheLock.EnterReadLock();
			List<T> cached = null;
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				cached = MemoryCache.Default [_cacheKey] as List<T>;

			}
			finally
			{
				cacheLock.ExitReadLock();
			}
			if( cached == null)
				return;
			lock(_locker)
			{
				if(!cached.Contains( value))
				{
					cached.Add(value);
					MemoryCache.Default.Set(_cacheKey, cached, new CacheItemPolicy{ AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration});
				}
			}

		}

		public void ReloadCache()
		{
			MemoryCache.Default.Remove( _cacheKey);
		}
	}
}
