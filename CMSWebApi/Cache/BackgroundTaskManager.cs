using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;
using CMSWebApi.Configurations;
using System.Web.Hosting;
using CMSWebApi.Cache.Caches;
using System.Threading;
using System.Web.Http.Dependencies;
using PACDMModel.Model;
using CMSWebApi.DataModels.DashBoardCache;
using System.Data.Entity;
using CMSWebApi.Cache.EntityCaches;
namespace CMSWebApi.Cache
{
	public enum CacheStatus:byte
	{
		Ready = 0,
		Rebuild,
		Not_ready,
		Loading,
		//No_cacheFile

	}
	public sealed class Defines
	{
		public const bool Delete_cache_Loaded = true;
		public const string Alert_Cache_Name = "alert";
		public const string POS_Cache_Name = "pos";
		public const string IOPCCount_Cache_Name = "iopccount";

	}
	public class BackgroundTaskManager : SingletonClassBase<BackgroundTaskManager>,IRegisteredObject
	{
		/// <summary>
		/// A cancellation token that is set when ASP.NET is shutting down the app domain.
		/// </summary>
		private readonly CancellationTokenSource _shutdown;

		/// <summary>
		/// A countdown event that is incremented each time a task is registered and decremented each time it completes. When it reaches zero, we are ready to shut down the app domain. 
		/// </summary>
		private readonly AsyncCountdownEvent _count;

		/// <summary>
		/// A task that completes after <see cref="count"/> reaches zero and the object has been unregistered.
		/// </summary>
		private readonly Task _done;


		/// <summary>
        /// Creates an instance that is registered with the ASP.NET runtime.
        /// </summary>
		private BackgroundTaskManager()
        {
            // Start the count at 1 and decrement it when ASP.NET notifies us we're shutting down.
            _shutdown = new CancellationTokenSource();
            _count = new AsyncCountdownEvent(1);
            _shutdown.Token.Register(() => _count.Signal(), useSynchronizationContext: false);

            // Register the object.
            HostingEnvironment.RegisterObject(this);

            // When the count reaches zero (all tasks have completed and ASP.NET has notified us we are shutting down),
            //  then unregister this object, and then the _done task is completed.
            _done = _count.WaitAsync().ContinueWith(
                _ => HostingEnvironment.UnregisterObject(this),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

		void IRegisteredObject.Stop(bool immediate)
		{
			_shutdown.Cancel();

			if(CacheMgr != null)
			{
				CacheMgr.SaveAllCache();
				CacheMgr.Dispose();
			}

			if (immediate)
				_done.Wait();
		}

		/// <summary>
		/// Registers a task with the ASP.NET runtime. The task is unregistered when it completes.
		/// </summary>
		/// <param name="task">The task to register.</param>
		private void Register(Task task)
		{
			_count.AddCount();
			//task.ContinueWith(_ => _count.Signal(), TaskContinuationOptions.ExecuteSynchronously);

			task.ContinueWith(_ => _count.Signal(), _shutdown.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
		}

		private Task<T> Register<T>(Task<T> task)
		{
			_count.AddCount();
			//task.ContinueWith(_ => _count.Signal(), TaskContinuationOptions.ExecuteSynchronously);

			 return task.ContinueWith<T>(_ => {
				_count.Signal();
				if( _.IsCompleted && !_.IsFaulted)
					return _.Result;
				return default(T);
			}, _shutdown.Token,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
		}

		public Task<T> Run<T>(Func<T> operation)
		{
			return Register<T>(Task.Run<T>(operation));
		}
		/// <summary>
		/// Executes an asynchronous background operation, registering it with ASP.NET.
		/// </summary>
		/// <param name="operation">The background operation.</param>
		public void Run(Func<Task> operation)
		{
			Register(Task.Run(operation));
		}
		/// <summary>
		/// Executes a background operation, registering it with ASP.NET.
		/// </summary>
		/// <param name="operation">The background operation.</param>
		public void Run(Action operation)
		{
			Register(Task.Run(operation));
		}

		#region Cache data
		internal CacheManager CacheMgr = null;

		public bool InitializeCaches(string appDataDir, DashboardCacheConfigCollection configs, IDependencyResolver Dbresolver, string sqlDBJob)
		{
			if( string.IsNullOrEmpty( appDataDir) ||  configs == null || configs.Count == 0 || Dbresolver == null)
				return false;
			IEnumerable<DashboardCacheConfig> enable_configs =  configs.Cast<DashboardCacheConfig>(); //configs.Cast<DashboardCacheConfig>().Where(item => item.Enable == true);

			//if( ! enable_configs.Any())
			//	return false;
			if(CacheMgr == null)
			{
				CacheMgr = new CacheManager(enable_configs, appDataDir, Dbresolver, this._shutdown.Token, sqlDBJob);
				
			}
			return true;
		}

		public void LoadDashboadCaches()
		{
			if( CacheMgr != null)
			{
				CacheMgr.LoadAllCache(this._shutdown.Token);
				Run( () => CacheMgr.DeleteCache(this._shutdown.Token));
			}
		
		}

		public void RegisterDBContext<TContext>( TContext dbcontext) where TContext : DbContext
		{
			if( CacheMgr != null)
				CacheMgr.RegisterDBContext<TContext>( dbcontext);
		}

		public void RegisterEntityCache(TableCacheConfigCollection configs)
		{
			if (configs == null || CacheMgr == null)
				return;

			Type dbtype = typeof(PACDMDB);
			string ass_name = dbtype.Assembly.FullName;
			string str_ns = dbtype.Namespace;
			int count = configs.Count;
			TableCacheConfig config = null;
			string typepath = null;
			Type ttype = null;
			for( int i = 0; i < count; i++)
			{
				config = configs[i];
				if( config.Enable == false || string.IsNullOrEmpty(config.Name))
					continue;
				typepath = string.Format("{0}.{1},{2}", str_ns, config.Name, ass_name);
				ttype = Type.GetType(typepath, false, true);
				if( ttype == null)
					continue;
				CacheMgr.RegisterEntityCache(ttype, typeof(PACDMDB), config.AutoUpdate);
			}
		}
		
		public void RegisterEntityCache<TEntity, TContext>(bool detectchange)
			where TEntity : class
			where TContext : DbContext
		{
			if( CacheMgr != null)
				CacheMgr.RegisterEntityCache<TEntity, TContext>(detectchange);
		}

		public IEntityCache<TEntity> ResolveEntityCache<TEntity>()
			where TEntity : class
		{
			return CacheMgr == null? null : CacheMgr.ResolveEntity<TEntity>();
		}

		public ICache<T> GetCache<T>() where T : CMSWebApi.DataModels.DashBoardCache.CacheModelBase 
		{
			return CacheMgr == null? null  : CacheMgr.Resolve<T>();
		}

		public bool Add(IList<tAlertEvent> alerts)
		{
			if( alerts == null || !alerts.Any() || CacheMgr == null )
				return false ;
			ICache<AlertCacheModel>Cache = CacheMgr.Resolve<AlertCacheModel>();
			if(Cache == null)
				return false;
			return (Cache as AlertsCache).Add(alerts);
			
		}

		public bool Add(tAlertEvent alert)
		{
			if (alert == null || CacheMgr == null)
				return  false;
			ICache<AlertCacheModel> Cache = CacheMgr.Resolve<AlertCacheModel>();
			if (Cache == null)
				return false ;
			return (Cache as AlertsCache).Add(alert);
		}
		
		#endregion
	}

}
