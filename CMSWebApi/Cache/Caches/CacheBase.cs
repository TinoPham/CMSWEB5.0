using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CMSWebApi.Configurations;
using System.IO;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Web.Http;
using Extensions;
using CMSWebApi.Cache.EntityCaches;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace CMSWebApi.Cache.Caches
{
	internal class CacheBase<T,IDbSvr> : IDisposable, ICachelocal, ICache<T> where T : CMSWebApi.DataModels.DashBoardCache.CacheModelBase where IDbSvr : class
	{
		#region constant
		protected const int Interval_Clean_Cache = 300; //5 * 60;//5 minutes
		protected const int Segment = 100000;//limit item to perform TPL
		protected const int Partitionner_Range = 10 * Segment;// apply Partition for collection
		protected const int TimeOut_AddItem = 1000;
		protected const string Cache_extension = ".cach";
		internal const string CACHES = "Caches";
		protected const string Cache_File_Pattern = "????????-????-????-????-????????????" + Cache_extension;
		#endregion

		#region Properties
		protected volatile byte _status = (byte)CacheStatus.Not_ready;

		public CacheStatus Status { get { return (CacheStatus)_status;}}

		internal IDbSvr dbService{ get { return ResolveDBService<IDbSvr>();}}

		protected BlockingCollection<T> CacheData { get; set; }

		protected CancellationToken CancelToken { get; set; }

		protected readonly ParallelOptions PO;

		private readonly CancellationTokenSource _AddDeleteTokenSource;

		internal DashboardCacheConfig Config { get; private set; }

		protected string CacheDir { get; set; }

		private volatile int min_time;
		private volatile int max_time;
		readonly object lock_min_time = new object();
		readonly object lock_max_time = new object();
		/// <summary>
		/// minimum time value in cache
		/// </summary>
		protected int MinTime{ get{ lock(lock_min_time){ return min_time;}} set{ lock(lock_min_time){ min_time = value;}}}
		/// <summary>
		/// maximun time value in cache
		/// </summary>
		protected int MaxTime{ get{ lock(lock_max_time){ return max_time;}} set{ lock(lock_max_time){ max_time = value;}}}

		volatile int _LastCacheTime;
		/// <summary>
		///Last time was cached
		/// </summary>
		public  int LastCacheTime { get{ return _LastCacheTime; } protected set{ _LastCacheTime = value;} }

		//volatile int _LastClean;
		//readonly object clean_locker = new object();
		///// <summary>
		///// last time when cleanup cache
		///// </summary>
		//protected int LastClean{ get{ lock(clean_locker){ return _LastClean; } } set { lock(clean_locker){ _LastClean = value;}}}
		#endregion

		#region Local

		private readonly AsyncCountdownEvent _count;
		//private readonly Task _done;
		#endregion

		public CacheBase(CancellationToken canceltoken, DashboardCacheConfig config, string datadir)
			: this()
		{
			Config = config;
			CancelToken = canceltoken;
			CacheDir =  Path.Combine( datadir, CACHES , config.Name);
			PO = new ParallelOptions{ CancellationToken = CancelToken, MaxDegreeOfParallelism = config.Parallelism };


		}

		private CacheBase()
		{
			CacheData = new BlockingCollection<T>();
			_AddDeleteTokenSource = new CancellationTokenSource();
			_count = new AsyncCountdownEvent(1);

			_AddDeleteTokenSource.Token.Register(() => _count.Signal(), useSynchronizationContext: false);
			//dbService = isvr;
			

		}

		#region Context, Entity

		protected Tmodel Get_Dim_Data<Tmodel, TContext>(Tmodel value, Func<Tmodel, bool> filter) where Tmodel: class where TContext : DbContext
		{
			IEnumerable<Tmodel> items = EntityCache<Tmodel>();
			if(items == null )
				return value;
			Tmodel item = items.FirstOrDefault(filter);
			if( item != null)
				return item;
			value = EntityInsert<Tmodel,TContext>(value);
			return value;
		}
		
		protected IEnumerable<Tmodel> EntityCache<Tmodel>() where Tmodel : class
		{
			IEntityCache<Tmodel> ientity = BackgroundTaskManager.Instance.ResolveEntityCache<Tmodel>();
			return ientity == null? new List<Tmodel>() : ientity.Results;
		}

		protected Tmodel EntityInsert<Tmodel, DBContext>( Tmodel value) where Tmodel : class where DBContext : DbContext
		{
			DbContext context = BackgroundTaskManager.Instance.CacheMgr.ResolveContext<DbContext>();
			if( context == null )
				return value;
			DbContext new_context = Commons.ObjectUtils.InitObject(typeof( DBContext)) as DbContext;
			try
			{
				new_context = Commons.ObjectUtils.InitObject(typeof( DBContext)) as DbContext;
				new_context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString;
				new_context.Configuration.AutoDetectChangesEnabled = false;
				new_context.Configuration.LazyLoadingEnabled = true;
				new_context.Configuration.ProxyCreationEnabled = false;
				new_context.Configuration.EnsureTransactionsForFunctionsAndCommands = true;

				new_context.Set<Tmodel>().Add( value);
				int ret = new_context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException) { }
			catch (DbUpdateException) { }
			catch (DbEntityValidationException) { }
			catch (NotSupportedException) { }
			catch (ObjectDisposedException) { }
			catch (InvalidOperationException) { }
			finally{ if( new_context != null){  new_context.Dispose(); new_context = null;}}

			return value;
		}
		
		protected void ResigterEntityCache<TModel, TContext>( bool detectchange) where TModel : class where TContext: DbContext
		{
			BackgroundTaskManager.Instance.RegisterEntityCache<TModel, TContext>(detectchange);
		}
		
		protected IDBService ResolveDBService<IDBService> ()where IDBService :class
		{
			return (BackgroundTaskManager.Instance.CacheMgr == null || BackgroundTaskManager.Instance.CacheMgr.IDBResolver == null )? null : BackgroundTaskManager.Instance.CacheMgr.IDBResolver.GetService( typeof(IDBService)) as IDBService;
		}

		#endregion

		#region Task
		private Task Register(Task task, CancellationToken canceltoken)
		{
			_count.AddCount();

			return task.ContinueWith(
			_ => _count.Signal(),
			canceltoken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
		
		protected Task AddItemTask(Func<Task> operation)
		{
			return Register(Task.Run(operation), _AddDeleteTokenSource.Token);
		}

		/// <summary>
		/// Executes a background operation, registering it with ASP.NET.
		/// </summary>
		/// <param name="operation">The background operation.</param>
		protected Task AddItemTask(Action operation)
		{
			return Register(Task.Run(operation),_AddDeleteTokenSource.Token);
		}

		#endregion

		protected string GetFileName()
		{
			return Guid.NewGuid().ToString() + Cache_extension;
		}

		protected bool DeleteFile(string path)
		{
			if( string.IsNullOrEmpty(path) || !File.Exists(path))
				return true;
			try
			{
				File.Delete( path);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		protected List<string> CacheFiles( string cachedir)
		{
			if( string.IsNullOrEmpty( cachedir))
				return new List<string>();
			try
			{
				string [] path = Directory.GetFiles(CacheDir, Cache_File_Pattern);
				return new List<string>( path);
			}
			catch(Exception)
			{
				return new List<string>();
			}

		}

		protected virtual T toCachemodel<Tin>( Tin model) where Tin :class
		{
			return Commons.ObjectUtils.InitObject<T>();
		}

		
		protected virtual void Update_Min_Max_Time()
		{
		}

		protected virtual void CleanCache(bool force = false)
		{
		}
		protected virtual DateTime MinConfigDate( DateTime datetime)
		{
			return DateTime.MinValue;
		}
		protected virtual DateTime MaxConfigDate()
		{
			return DateTime.MaxValue;
		}
		//protected void UpdateLastClean(bool isdatecompare = true)
		//{
		//	LastClean = isdatecompare?(int)DateTime.UtcNow.DateToUnixTimestamp() : (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();
		//}

		#region Serialize
		private void WriteTimeupdate(int value, Stream stream)
		{
			byte[] bytes = BitConverter.GetBytes( value);
			stream.Write(bytes, 0, bytes.Length);
		}
		private bool Serialized(IEnumerable<T> items, string filename, int updatetime)
		{
			Stream stream = null;
			bool ret = true;
			try
			{
				string dir =Path.GetDirectoryName(filename);//  new FileInfo(filename);
				if (!CMSWebApi.Utils.Utilities.CreateDir(dir))
					return false;

				stream = File.Open(filename, FileMode.Create);
				WriteTimeupdate( updatetime, stream);
				Serializer.Serialize<IEnumerable<T>>(stream, items);
			}
			catch( Exception)
			{
				ret = false;
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
					stream.Dispose();
				}

			}
			return ret;
		}

		private IEnumerable<IEnumerable<T>> Chunked(List<T> source, int chunkSize)
		{
			var offset = 0;

			while (offset < source.Count)
			{
				yield return source.GetRange(offset, Math.Min(source.Count - offset, chunkSize));
				offset += chunkSize;
			}
		}

		private bool Serialized(List<T> items, int updatetime)
		{
			IEnumerable<IEnumerable<T>> chunks = Chunked(items, Config.ChunkSize);
			ConcurrentBag<bool> result = new ConcurrentBag<bool>();
			if( Directory.Exists(CacheDir))
			{
				string[]del_files =  Directory.GetFiles( CacheDir);
				foreach(string del in del_files)
				{
					DeleteFile( del);
				}
			}
			Parallel.ForEach(chunks, new ParallelOptions { MaxDegreeOfParallelism = Config.Parallelism }, item =>
			{
				string filename = Path.Combine(CacheDir, GetFileName());
				bool ret = Serialized(item, filename, updatetime);
				result.Add(ret);

			});
			return result.Count(item => item == false) == 0;
			//chunks.AsParallel().WithDegreeOfParallelism(Config.Parallelism).ForAll(item =>
			//{
			//	string filename = Path.Combine(CacheDir, GetFileName());
			//	bool ret = Serialized(item, filename);
			//	result.Add(ret);

			//}
			//);
			
		}

		#endregion

		#region De-serialize

		private void DeSerialized(Stream filestream, FileResult fresult)
		{
			Stream stream = filestream;
			List<T> result = null;
			
			try
			{
				result = Serializer.Deserialize<List<T>>(stream);
				AddItems(result);


			}
			catch (Exception ex)
			{
				fresult.StringResult = ex.Message;
				fresult.Result = false;
				result = new List<T>();
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
			}

		}
		
		private int ReadTimeupdate(Stream stream)
		{
			byte[] buff = new byte[ sizeof(int)];
			stream.Read( buff,0, buff.Length);
			return BitConverter.ToInt32( buff,0);

		}
		
		private FileResult DeSerialized(string filename)

		{
			FileStream stream = null;
			List<T> result = null;
			FileResult fresult = new FileResult{ Path = filename, Result = true, StringResult = null};
			try
			{
				stream = File.Open(filename, FileMode.Open);
				if( stream.Length < sizeof(int))
				{
					fresult.Result = false;
					fresult.Update = -1;
				}
				fresult.Update = ReadTimeupdate(stream);
				if( fresult.Update < 0)
					stream.Seek(0, SeekOrigin.Begin);
				MemoryStream realstream = new MemoryStream();
				stream.CopyTo( realstream, (int)(stream.Length - stream.Position));
				realstream.Seek(0, SeekOrigin.Begin);
				DeSerialized(realstream, fresult);
				
				
			}
			catch(Exception ex)
			{
				fresult.StringResult = ex.Message;
				fresult.Result = false;
				result = new List<T>();
			}
			finally
			{
				if(stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
				CMSWebApi.Utils.Utilities.DeleteFile(filename);
			}

			return fresult;
		}

		private BlockingCollection<FileResult> PralalelDeSerialized(List<string> Files)
		{
			BlockingCollection<FileResult> allresult = new BlockingCollection<FileResult>();
			Parallel.ForEach( Files, PO, file => allresult.Add(DeSerialized(file)));
			return allresult;

			//ParallelQuery<FileResult> alldata = Files.AsParallel<string>().WithDegreeOfParallelism(Config.Parallelism).Select(file =>
			//{
			//	return DeSerialized(file);

			//}
			//);
			//return alldata;
		}

		private IEnumerable<FileResult> DeSerialized(List<string> Files)
		{
			BlockingCollection<FileResult> alldata = PralalelDeSerialized(Files);
			return  alldata.AsEnumerable();
		}
		
		#endregion

		#region query/add/delete
		protected IEnumerable<Tout> PQuery<Tout>( Func<T, bool> filter, Func<T, Tout> selector)
		{
			if (Status != CacheStatus.Ready || CacheData == null)
				return new List<Tout>();
			if(CacheData.Count > Segment)
			{
				OrderablePartitioner<T>partition = Partitioner.Create<T>(CacheData);
				ParallelQuery<T> pquery = filter == null? partition.AsParallel().WithDegreeOfParallelism(Config.Parallelism) : partition.AsParallel().WithDegreeOfParallelism(Config.Parallelism).Where(filter);
				//ParallelQuery<T> pquery = CacheData.AsParallel<T>().WithDegreeOfParallelism(Config.Parallelism).Where(filter);
				return pquery.Select( selector).AsEnumerable();
			}
			else
			{
				IEnumerable<T> ie_result = filter == null? CacheData : CacheData.Where(filter);
				return ie_result.Select( selector);
			}
			
		}
		
		protected bool CheckValid(Func<T, bool> filter)
		{
			if (Status != CacheStatus.Ready || CacheData == null)
				return false;
			return CacheData.Any( filter);

		}
	
		protected bool RemoveItem(T value, int timeout = TimeOut_AddItem)
		{
			try
			{
				if( Status != CacheStatus.Ready)
					return false;

				T out_value = null;
				return CacheData.TryTake(out out_value, timeout, this.CancelToken);
			}
			catch(System.OperationCanceledException){}
			catch (System.ObjectDisposedException) { }
			catch(System.ArgumentOutOfRangeException){}
			catch (System.InvalidOperationException) { }
			return false;
		}

		protected virtual bool RemoveItems(Func<T, bool> filter)
		{
			IEnumerable<T> removeItems = PQuery<T>(filter, item => { return item; });
			bool ret = true;
			foreach (T it in removeItems)
			{
				if(CancelToken.IsCancellationRequested || Status != CacheStatus.Ready)
					break;
				ret &= RemoveItem(it);
			}
			return ret;
		}

		protected virtual bool AddItem<Tin>(Tin model, Func< Tin, T> toTFunc, int timeout = TimeOut_AddItem) where Tin: class
		{
			T it = toTFunc(model);
			if( it == null)
				return false;
			if( it.ItemTime < MinTime)
				return true;

			return AddItem( it, timeout);
		}
		
		protected virtual bool AddItem(T value, int timeout = TimeOut_AddItem)
		{
			try
			{
				if( Status == CacheStatus.Rebuild)
					return false;

				if( value.ItemTime < MinTime )
					return true;
				return CacheData.TryAdd(value, TimeOut_AddItem, this.CancelToken);
			}
			catch(System.OperationCanceledException){}
			catch(System.ObjectDisposedException){}
			catch(System.InvalidOperationException){}
			catch(System.ArgumentOutOfRangeException){}

			return false;
		}

		protected virtual bool AddItems<Tin>(IList<Tin> dbcollection, Func<Tin, T> toTFunc) where Tin : class
		{
			Func<IEnumerable<Tin>, bool> add_items = items =>
				{
					bool ret = true;
					foreach (Tin it in items)
					{
						if( CancelToken.IsCancellationRequested || Status == CacheStatus.Rebuild)
							break;
					
						ret &= AddItem<Tin>(it, toTFunc);
					}
					return ret;
			};
			int item_Count =  dbcollection == null?  0 : dbcollection.Count();
			if ( item_Count> Segment)
			{
				try
				{
					//ParallelOptions po = new ParallelOptions{ CancellationToken = this.CancelToken, MaxDegreeOfParallelism = Config.Parallelism};
					if (item_Count > Partitionner_Range)
					{
						OrderablePartitioner<Tin> partitionner = Partitioner.Create<Tin>(dbcollection, true);
						Parallel.ForEach(partitionner, this.PO, it => AddItem<Tin>(it, toTFunc));
					}
					else
						Parallel.ForEach(dbcollection, this.PO, it => AddItem<Tin>(it, toTFunc));
					return true;
				}
				catch(OperationCanceledException){return false;}
				catch(ArgumentNullException){return false;}
				catch(AggregateException){return false;}
				catch (ObjectDisposedException) { return false; }
				
			}
			else
				return add_items(dbcollection);

			//return CacheData.IsAddingCompleted;
		}

		protected bool AddItems(IList<T> collection)
		{
			if( collection == null || !collection.Any())
				return true;
			IEnumerable<T> validItems = collection.Where( it => it.ItemTime >= MinTime);
			if (validItems == null || !validItems.Any())
				return true;
			Action<IEnumerable<T>> add_items = items =>
															{
																
																foreach( T it in items) 
																{
																	if( this.CancelToken.IsCancellationRequested )
																		break;

																	AddItem(it);
																}
															
															};
			int it_count = validItems.Count();
			if( it_count > Segment)
			{
				try
				{
					ParallelOptions po = new ParallelOptions{ CancellationToken = this.CancelToken, MaxDegreeOfParallelism = Config.Parallelism};
					if( it_count > Partitionner_Range)
					{
						OrderablePartitioner<T> partitioner = Partitioner.Create<T>(validItems.ToList(), true);
						Parallel.ForEach(partitioner,po, it=> AddItem(it));
					}
					else
						Parallel.ForEach( validItems, po, it=> AddItem(it));
					return true;
				}
				catch(OperationCanceledException){ return false;}
				catch(ArgumentNullException){ return false;}
				catch(AggregateException){ return false;}
				catch(ObjectDisposedException){ return false;}
			}
			else
				add_items(validItems);

			return CacheData.IsAddingCompleted;
		}
		#endregion

		#region save/load
		public virtual bool PreLoad()
		{

			return true;
		}

		public virtual bool PostLoad()
		{
			return true;
		}
		
		protected virtual CacheStatus DBLoad()
		{
			return CacheStatus.Not_ready;
		}
	
		protected bool SaveCacheFile()
		{
			return Serialized(CacheData.ToList(), LastCacheTime);
		}

		protected virtual CacheStatus LoadCacheFile()
		{
			List<string> Files = CacheFiles(CacheDir);
			if( Files == null || Files.Count == 0)
				return  CacheStatus.Not_ready;

			IEnumerable<FileResult> allresult = PralalelDeSerialized(Files);

			CacheStatus status = allresult.FirstOrDefault(rfile => rfile.Result == false) != null ? CacheStatus.Not_ready : CacheStatus.Loading;
			LastCacheTime = allresult.Max( it => it.Update);
			return status;
		}

		#endregion

		#region ICache
		public virtual bool ValidData(DateTime sdate, DateTime edate)
		{
			return false;
		}
		public virtual IEnumerable<Tout> Query<Tout>(Func<T, bool> filter, Func<T, Tout> selector)
		{
		  return PQuery<Tout>( filter, selector);
		}
		
		public virtual bool Check(Func<T, bool> filter)
		{
			return CheckValid( filter);
		}
		
		public virtual void Add(T value)
		{
			AddItemTask( () => AddItem(value));
			//return AddItem(value);
		}
	
		public virtual void Add(IList<T> collection)
		{
			AddItemTask(() => AddItems(collection));
			//return AddItems(collection);
		}
		
		public virtual bool Delete(T value)
		{
			return RemoveItem(value);
		}
		
		public virtual bool Delete(Func<T, bool> filter)
		{
			return RemoveItems(filter);
		}

		//public virtual Task Add<Tdata>(Tdata value)
		//{
		//	return null;
		//}
		//public virtual Task Add<Tdata>(IList<Tdata> collection)
		//{
		//	return null;
		//}
		#endregion

		#region ICachelocal
		public virtual CacheStatus Refresh()
		{
			_status = (byte)CacheStatus.Rebuild;
			CacheStatus c_status = DBLoad();
			_status = (byte)c_status;
			return  c_status;
		}
		public virtual bool UpdateCacheFromWareHouse<Tfact>(Tfact fact) where Tfact : class
		{
			return true;
		}
		public virtual bool UpdateCacheFromDB(int timeUpdate)
		{
			LastCacheTime = timeUpdate;
			return true;
		}
		public virtual bool Load(bool filesupport)
		{
			_status = (byte)CacheStatus.Not_ready;
			//LastClean = (int)DateTime.UtcNow.FullDateTimeToUnixTimestamp();
			CacheData = new BlockingCollection<T>();
			return true;
		}
		public virtual bool Save()
		{
			_AddDeleteTokenSource.Cancel();
			_count.WaitAsync().Wait();
			return SaveCacheFile();
		}
		public virtual void Dispose()
		{
			_status = (byte)CacheStatus.Not_ready;
			CacheData.Dispose();
			CacheData = null;
			CacheDir = null;
		}
		#endregion

	}

	internal class FileResult
	{
		public string Path{get; set;}
		public bool Result{ get ;set;}
		public string StringResult{ get ;set;}
		public int Update{get ;set;}
	}

	internal interface ICachelocal
	{
		
		int LastCacheTime{get;}
		CacheStatus Refresh();
		bool PreLoad();
		bool PostLoad();
		bool Load( bool filesupport);
		bool Save();
		bool UpdateCacheFromDB(int timeUpdate);
		bool UpdateCacheFromWareHouse<Tfact>( Tfact fact) where Tfact: class;
	}

	public interface ICache<Tin> where Tin: class
	{
		
		CacheStatus Status { get; }
		bool ValidData( DateTime sdate, DateTime edate);
		IEnumerable<Tout> Query<Tout>(Func<Tin, bool> filter, Func<Tin, Tout> selector);
		bool Check( Func<Tin, bool> filter);
		void Add(Tin value);
		void Add(IList<Tin> collection);

		//Task Add<Tdata>(Tdata value);
		//Task Add<Tdata>(IList<Tdata> collection);
		
		bool Delete(Tin value);
		bool Delete(Func<Tin, bool> filter);
		void Dispose();
		
	}


}
