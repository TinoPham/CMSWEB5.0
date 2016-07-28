using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels.DashBoardCache;
using CMSWebApi.Configurations;
using System.Collections.Concurrent;
using CMSWebApi.ServiceInterfaces;
using System.Threading;
using System.Web.Http.Dependencies;
using System.IO;
using System.Reflection;
using CMSWebApi.Cache.EntityCaches;
using PACDMModel.Model;
using System.Data.Entity;
using System.Data.SqlClient;
namespace CMSWebApi.Cache.Caches
{
	internal enum SqlJobStatus : int
	{
		Job_was_not_Found  = -2,
		Job_is_Disabled  = -1,
		Job_Failed  = 0, 
		Job_Succeeded = 1,
		Job_Retry =2,
		Job_Canceled  = 3,
		Job_In_progress = 4,
		Job_Disabled =5,
		Job_Idle = 6
		}

	internal class CacheManager:IDisposable
	{
		readonly IEnumerable<DashboardCacheConfig> Configs;
		readonly IEnumerable<DashboardCacheConfig> UnusedConfigs;
		string sql_job_monitor = "select dbo.Func_JobStatus(@name)";
		readonly string DataDir;

		public IDependencyResolver IDBResolver{ get ;set;}

		private readonly Dictionary<String, Type> NameModelMaps = new Dictionary<string,Type>();

		private readonly Dictionary<Type, Type> ModelCacheTypeMaps = new Dictionary<Type, Type>();

		readonly object ModelCacheMaps_locker = new object();
		readonly string sql_dbJobname;
		SqlCommandDependence<int>JobMonitor;

		private Dictionary<Type, object> ModelCacheMaps = new Dictionary<Type,object>();
		private readonly Dictionary<Type,object>EntityCaches = new Dictionary<Type,object>();
		private readonly Dictionary<Type, DbContext> DBContextMaps = new Dictionary<Type, DbContext>();
		CancellationToken CancelToken;
		#region Public methods

		private void RegisterCacheModel( string name, Type modelType)
		{
			if (NameModelMaps.ContainsKey(name))
				throw new Exception( String.Format("Cache will name + '{0}' already existed.", name));
			if (NameModelMaps.ContainsValue(modelType))
				throw new Exception(String.Format("Cannot add duplicate model data type '{0}'", modelType.FullName));
			if (!ModelCacheTypeMaps.ContainsKey(modelType))
				throw new Exception(String.Format("Cache is not working for model type '{0}'", modelType.FullName));

			NameModelMaps.Add(name, modelType);
			
		}
		private void InitJobMonitor( DbContext dbcontext, string jobname, CancellationToken CancelToken)
		{
			SqlCommand cmd = new SqlCommand(sql_job_monitor);
			cmd.Parameters.Add( new SqlParameter{
				ParameterName = "@name",
				SqlDbType = System.Data.SqlDbType.NVarChar,
				Size = 128,
				SqlValue = jobname});

			JobMonitor = new SqlCommandDependence<int>(CancelToken, dbcontext, cmd, jobname, true);
		}
		private SqlJobStatus JobStatus()
		{
			if( JobMonitor == null)
				return SqlJobStatus.Job_was_not_Found;
			IEnumerable<int> ret = JobMonitor.Results;
			if(ret == null || !ret.Any())
				return SqlJobStatus.Job_was_not_Found;
			int val = ret.First();
			return Commons.Utils.GetEnum<SqlJobStatus>(val) ;
		}
		internal CacheManager(IEnumerable<DashboardCacheConfig> configs, string datadir, IDependencyResolver dbResolver, CancellationToken canceltoken, string sqldbjob)
		{
			this.sql_dbJobname = sqldbjob;
			Configs = configs.Where( it => it.Enable == true);
			UnusedConfigs = configs.Where(it => it.Enable == false || it.Save == false);
			DataDir = datadir;
			IDBResolver = dbResolver;
			CancelToken = canceltoken;

			ModelCacheTypeMaps.Add(typeof(AlertCacheModel), typeof(AlertsCache));
			ModelCacheTypeMaps.Add(typeof(POSPeriodicCacheModel), typeof(POSCache));
			ModelCacheTypeMaps.Add(typeof(IOPCCountPeriodicCacheModel), typeof(IOPCCountCache));

			RegisterCacheModel(CMSWebApi.Cache.Defines.Alert_Cache_Name, typeof(CMSWebApi.DataModels.DashBoardCache.AlertCacheModel));
			RegisterCacheModel(CMSWebApi.Cache.Defines.POS_Cache_Name, typeof(CMSWebApi.DataModels.DashBoardCache.POSPeriodicCacheModel));
			RegisterCacheModel(CMSWebApi.Cache.Defines.IOPCCount_Cache_Name, typeof(CMSWebApi.DataModels.DashBoardCache.IOPCCountPeriodicCacheModel));

		}
		
		internal void RegisterDBContext<TContext>( TContext dbcontext) where TContext : DbContext
		{
			if(!DBContextMaps.ContainsKey(typeof(TContext)))
			DBContextMaps.Add( typeof(TContext), dbcontext);
		}
		
		internal DbContext ResolveContext( Type contextType)
		{
			return DBContextMaps.ContainsKey(contextType) ? DBContextMaps [contextType] : null;
		}
		
		internal DbContext ResolveContext<TContext>() where TContext : DbContext
		{
		   return ResolveContext( typeof(TContext));
		}
		
		internal void RegisterEntityCache<TEntity, TContext>(bool detectchange) where TEntity : class where TContext : DbContext
		{
			Type dbctxType = typeof(TContext);
			Type entityType = typeof(TEntity);
			if(!EntityCaches.ContainsKey(entityType) && DBContextMaps.ContainsKey( dbctxType))
			{
				DbContext dbcontext = DBContextMaps [dbctxType];
				EntityCaches.Add(entityType, new EntityCache<TEntity>(CancelToken, dbcontext, null, detectchange));
			}
		}

		internal void RegisterEntityCache( Type TEntityType, Type contexttype,  bool detectchange)
		{
			if (!EntityCaches.ContainsKey(TEntityType) && DBContextMaps.ContainsKey(contexttype))
			{
				DbContext dbcontext = DBContextMaps [contexttype];
				object entitycache = Commons.ObjectUtils.InitObject(typeof(EntityCache<>), new Type [] { TEntityType }, new object [] { CancelToken, dbcontext, null, detectchange });
				if( entitycache != null)
					EntityCaches.Add(TEntityType, entitycache);
			}
		}
		
		internal ICache<T> Resolve<T>() where T : CMSWebApi.DataModels.DashBoardCache.CacheModelBase
		{
			if (!ModelCacheMaps.Any() || !ModelCacheMaps.ContainsKey(typeof(T)))
				return null;
			SqlJobStatus status = JobStatus();
			Type cachemodeltype = typeof(T);
			if (cachemodeltype.Equals(typeof(CMSWebApi.DataModels.DashBoardCache.AlertCacheModel)))
				return ModelCacheMaps [typeof(T)] as ICache<T>;

			if((status == SqlJobStatus.Job_In_progress || status == SqlJobStatus.Job_Retry) )
				return null;

			return ModelCacheMaps[typeof(T)] as ICache<T>;
		}

		internal object ResolveEntity( Type entitytype)
		{
			return !EntityCaches.ContainsKey(entitytype) ? null : EntityCaches [entitytype];
		}
	
		internal IEntityCache<TEntity> ResolveEntity<TEntity>() where TEntity: class
		{
			Type entype = typeof(TEntity);
			object cache = ResolveEntity( entype);
			return cache == null ? null : cache as IEntityCache<TEntity>;
		}

		internal void SaveAllCache()
		{
			BlockingCollection<Task> tasks = new BlockingCollection<Task>();
			IEnumerable<DashboardCacheConfig> _save = Configs.Where( it => it.Enable == true && it.Save == true);
			foreach (DashboardCacheConfig item in _save)
			{
				Type modeltype = ResolveModelType( item.Name);
				ICachelocal icache = ResolveCache(modeltype) as ICachelocal;
				if( icache == null)
					continue;

				tasks.Add(SaveCache(icache));
			}
			if( tasks.Any())
				Task.WaitAll( tasks.ToArray());
		}

		internal void LoadAllCache(CancellationToken canceltokensource)
		{
			if( !Configs.Any() || NameModelMaps.Count == 0 || ModelCacheTypeMaps.Count == 0)
				return;
			if( canceltokensource == null)
				canceltokensource = CancelToken;
			bool already_register = false;
			if( Configs.Any())
				InitJobMonitor( ResolveContext<PACDMDB>(), sql_dbJobname, canceltokensource);
			foreach (DashboardCacheConfig cfig in Configs)
			{
				//if( cfig.Enable == false)
				//	continue;
				if(canceltokensource.IsCancellationRequested)
					break;
				switch(cfig.Name.ToLower())
				{
					case CMSWebApi.Cache.Defines.POS_Cache_Name:
					case CMSWebApi.Cache.Defines.IOPCCount_Cache_Name:
						
						if( already_register)
						{
							RegisterEntityCache<tCMSWeb_Caches, PACDMDB>(true);
							(ResolveEntity<tCMSWeb_Caches>() as EntityCache<tCMSWeb_Caches>).OnChanged += CacheManager_OnChanged;
							already_register = true;
						}
					break; 
				}
				LoadCache(cfig, canceltokensource);
			}
		}

		internal void RefreshCache(DashboardCacheConfig config)
		{
			if( config == null)
				return;
			if( config.Enable == false)
				return;
			Object Cache_Object = ResolveCache( config.Name);
			if( Cache_Object == null)
				return;
			(Cache_Object as ICachelocal).Refresh();
			
		}
		void CacheManager_OnChanged(object sender, EntityDataEventArgs<tCMSWeb_Caches> e)
		{
			tCMSWeb_Caches lastconfig = null;
			IEnumerable<tCMSWeb_Caches> items = e.Results;
			ICachelocal cache = null;
			foreach (DashboardCacheConfig cfg in this.Configs)
			{
				
				lastconfig = items.FirstOrDefault( it => string.Compare( cfg.Name, it.CacheName, true) == 0);
				if( lastconfig == null)
					continue;

				cache = ResolveCache( cfg.Name)as ICachelocal;
				if( cache == null || !lastconfig.UpdateTimeInt.HasValue ||  lastconfig.UpdateTimeInt <= cache.LastCacheTime)
					continue;
				cache.UpdateCacheFromDB( lastconfig.UpdateTimeInt.Value);
			}
		}

		public void Dispose()
		{
			EntityCache<tCMSWeb_Caches> web_cache_event = (ResolveEntity<tCMSWeb_Caches>() as EntityCache<tCMSWeb_Caches>);
			if( web_cache_event != null )
				web_cache_event.OnChanged -= CacheManager_OnChanged;

			while(EntityCaches.Any())
			{
				var it = EntityCaches.First();
				EntityCaches.Remove(it.Key);
				
				(it.Value as IDisposable).Dispose();
			}

			while(DBContextMaps.Any())
			{
				var it = DBContextMaps.First();
				DBContextMaps.Remove(it.Key);
				it.Value.Dispose();
			}

			while(ModelCacheMaps.Any())
			{
				var it = ModelCacheMaps.First();
				ModelCacheMaps.Remove(it.Key);
				(it.Value as IDisposable).Dispose();
			}
			ModelCacheTypeMaps.Clear();
			NameModelMaps.Clear();

			
		}

		#endregion

		#region Entity Caches
		
		#endregion

		private Type ResolveCacheType( Type modeltype)
		{
			
			return ModelCacheTypeMaps == null ? null : ModelCacheTypeMaps.FirstOrDefault( it => it.Key.Equals(modeltype)).Value;// ModelCacheTypeMaps [modeltype];
		}
		
		private Type ResolveCacheType( string cahenname)
		{
			Type modeltype = ResolveModelType(cahenname);
			return ModelCacheTypeMaps== null || modeltype == null? null :ModelCacheTypeMaps.FirstOrDefault(it => it.Key.Equals(modeltype)).Value;//ModelCacheTypeMaps [modeltype];
		}
		
		private Type ResolveModelType(string name)
		{
			return NameModelMaps == null? null:NameModelMaps.FirstOrDefault( it => string.Compare(it.Key,name, true) == 0).Value;
		}

		private object ResolveCache( Type modelType)
		{
			if( modelType == null )
				return null;

			return ModelCacheMaps == null? null : ModelCacheMaps.FirstOrDefault( it => it.Key.Equals(modelType)).Value;// ModelCacheMaps [modelType];
		}

		private object ResolveCache<T>() where T:class
		{
			Type cachetype = typeof(T);
			return ResolveCache ( cachetype);
		}
		
		private object ResolveCache( string name)
		{
			Type ctype = ResolveModelType( name);
			return ResolveCache( ctype);
		}

		internal void DeleteCache(CancellationToken canceltoken)
		{
			foreach (DashboardCacheConfig config in UnusedConfigs)
			{
				if( canceltoken.IsCancellationRequested)
					break;
				DeleteCache( config, canceltoken);
			}
		}
		private void DeleteCache(DashboardCacheConfig config, CancellationToken canceltoken)
		{
			if( config == null || (config.Enable == true && config.Save == true) )
				return;
			string path = Path.Combine(DataDir, CacheBase<AlertCacheModel, IResposity>.CACHES, config.Name);
			if( !Directory.Exists(path))
				return;
			String[] files = Directory.GetFiles(path);
			try
			{
				foreach( string file in files)
				{
					if( canceltoken.IsCancellationRequested)
						break;

					File.Delete( file);
				}
			}
			catch(Exception){}

		}
		private Task LoadCache(DashboardCacheConfig config, CancellationToken canceltoken)
		{
			return Task.Run(() =>
			{
				//ct.ThrowIfCancellationRequested();
				Type ModelType = ResolveModelType(config.Name);//NameModelMaps [config.Name];
				Type CacheType = ResolveCacheType(ModelType);
				
				 object cachedata = Commons.ObjectUtils.InitObject(CacheType, new object [] {canceltoken, config, DataDir });
				
				if (cachedata == null)
					return;
				(cachedata as ICachelocal).PreLoad();
				(cachedata as ICachelocal).Load( config.Save );
				(cachedata as ICachelocal).PostLoad();
				lock (ModelCacheMaps_locker)
				{
					ModelCacheMaps.Add(ModelType, cachedata);
				}
			}, canceltoken);
		}

		private Task SaveCache(ICachelocal cache)
		{
			return Task.Run(() =>
			{
				cache.Save();
			});
		}
	}
}
