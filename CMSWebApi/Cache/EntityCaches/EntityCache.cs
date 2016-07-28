using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
namespace CMSWebApi.Cache.EntityCaches
{
	public interface IEntityCache<TEntity> where TEntity : class
	{
		IEnumerable<TEntity> Results { get; }
		void AddItemCache(TEntity value);
		void RemoveItemCache(TEntity value);
		void ReloadCache();
	}

	internal class EntityCache<TEntity> : IEntityCache< TEntity>, IDisposable where TEntity : class
	{
		#if !DEBUG
			const int ReConnect_Interval = 1000 * 60;
		#else
			const int ReConnect_Interval = 1000 * 5;
		#endif
		readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

		public event EventHandler<EntityDataEventArgs<TEntity>> OnChanged;

		DbContext _context;

		Expression<Func<TEntity, bool>> _query;

		//Expression<Func<TEntity, TEntity>> _selector;

		SqlCommand _Command;

		SqlCommand Command {
			get {
					if(_Command == null)
						_Command = SQLCommand();
					return _Command;
				}
			set { _Command = value;}
		}

		bool _hasDataChanged = false;

		//volatile bool ContinueListening = true;

		private string _cacheKey= typeof(TEntity).FullName;
		
		//BlockingCollection<Tout> Items;

		CancellationToken Schedule_Reconnect_Token;

		readonly ManualResetEvent Evt_Exit = new ManualResetEvent(false);

		bool _detectchange = false;
		readonly object _locker = new object();

		//public EntityCache(CancellationToken canceltoken, DbContext dbcontext, Expression<Func<TEntity, bool>> query, Expression<Func<TEntity, Tout>> selector, bool detectchange = true)
		public EntityCache(CancellationToken canceltoken, DbContext dbcontext, Expression<Func<TEntity, bool>> query, bool detectchange = true)
		{
			Schedule_Reconnect_Token = canceltoken;
			_context = dbcontext;
			_query = query;
			//_selector = selector;
			_detectchange = detectchange;
			if( _detectchange)
				EntityChangeNotifier.AddConnectionString(_context.Database.Connection.ConnectionString);

		}

		private SqlCommand SQLCommand()
		{
			try
			{
			var query = _query == null ? _context.Set<TEntity>().Select( it=> it): _context.Set<TEntity>().Where(_query);
			//if( _selector == null)
			return (query as DbQuery<TEntity>).ToSqlCommand();

			//return (query.Select(_selector) as DbQuery<Tout>).ToSqlCommand();
			}
			catch(Exception)
			{
				return null;
			}
		}

		private IEnumerable<TEntity> GetCurrent(SqlCommand command, out SqlDependency sqldepend)
		{
			IEnumerable<TEntity> data = null; //new List<Tout>();
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
				data = _context.Database.SqlQuery<TEntity>(command.CommandText).ToList<TEntity>();
			}
			catch(System.InvalidOperationException){}
			catch (System.Data.SqlClient.SqlException) { }
			catch (System.Configuration.ConfigurationErrorsException) { }
			catch(Exception){}
			
			return data;
		}

		private IEnumerable<TEntity> GetResults()
		{
			if( MemoryCache.Default.Contains(_cacheKey))
				return MemoryCache.Default [_cacheKey] as List<TEntity>;
			//First we do a read lock to see if it already exists, this allows multiple readers at the same time.
			cacheLock.EnterReadLock();
			
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				IEnumerable<TEntity> cached = MemoryCache.Default [_cacheKey] as List<TEntity>;

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
			List<TEntity> value = null;
			try
			{
				 //We need to check again to see if the string was created while we where waiting to enter the EnterUpgradeableReadLock
				object cached = MemoryCache.Default.Get(_cacheKey);

				if (cached != null)
					return cached as List<TEntity>;
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

					value = item == null? null:item.Value as List<TEntity>;
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
			IEnumerable<TEntity> Data = GetCurrent(command, out sqldepend);
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
				OnDependencyChanged( item.Value as IEnumerable<TEntity>);
				arguments.UpdatedCacheItemPolicy = policy;
			}
		}

		private void OnDependencyChanged(IEnumerable<TEntity> collection)
		{
			//IEnumerable<string> dependencyKeys = (IEnumerable<string>)state;
			if( OnChanged != null)
			{
				EntityDataEventArgs<TEntity> newdata = new EntityDataEventArgs<TEntity>();
				newdata.Results = collection;
				newdata.Query = null;
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

		public IEnumerable<TEntity> Results
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

		public void RemoveItemCache(TEntity value)
		{
			if (_detectchange == true || value == null || !MemoryCache.Default.Contains(this._cacheKey))
				return;
			cacheLock.EnterReadLock();
			List<TEntity> cached = null;
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				cached = MemoryCache.Default [_cacheKey] as List<TEntity>;

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

		public void AddItemCache(TEntity value)
		{
			if( _detectchange == true ||  value == null ||! MemoryCache.Default.Contains( this._cacheKey))
				return;

			cacheLock.EnterReadLock();
			List<TEntity> cached = null;
			try
			{
				//Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
				cached = MemoryCache.Default [_cacheKey] as List<TEntity>;

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
