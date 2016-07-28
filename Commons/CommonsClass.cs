using System;
using System.Collections.Generic;
using  System.Linq;
using System.Threading;
namespace Commons
{


	/// <summary>
	///  A <see cref="Lazy"/> object that implements <see cref="IDisposable"/>.
	/// </summary>
	/// <typeparam name="T">
	///  The object being lazily created.
	/// </typeparam>

	public class LazyDisposable<T> : Lazy<T>, IDisposable where T : IDisposable
	{
		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class.
		///  When lazy initialization occurs, the default constructor is used.
		/// </summary>
		public LazyDisposable() : base() { }

		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class.
		///  When lazy initialization occurs, the default constructor of the target type
		///  and the specified initialization mode are used.
		/// </summary>
		/// <param name="isThreadSafe">
		///  true to make this instance usable concurrently by multiple threads;
		///  false to make the instance usable by only one thread at a time. 
		/// </param>
		public LazyDisposable(bool isThreadSafe) : base(isThreadSafe) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class
		///  that uses the default constructor of T and the specified thread-safety mode.
		/// </summary>
		/// <param name="mode">
		///  One of the enumeration values that specifies the thread safety mode. 
		/// </param>
		public LazyDisposable(LazyThreadSafetyMode mode) : base(mode) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class.
		///  When lazy initialization occurs, the specified initialization function is used.
		/// </summary>
		/// <param name="valueFactory">
		///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
		/// </param>
		public LazyDisposable(Func<T> valueFactory) : base(valueFactory) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class.
		///  When lazy initialization occurs, the specified initialization function
		///  and initialization mode are used.
		/// </summary>
		/// <param name="valueFactory">
		///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
		/// </param>
		/// <param name="isThreadSafe">
		///  true to make this instance usable concurrently by multiple threads;
		///  false to make this instance usable by only one thread at a time. 
		/// </param>
		public LazyDisposable(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="LazyDisposable<T>"/> class
		///  using the specified initialization function and thread-safety mode.
		/// </summary>
		/// <param name="valueFactory">
		///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
		/// </param>
		/// <param name="mode">
		///  One of the enumeration values that specifies the thread safety mode. 
		/// </param>
		public LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }

		/// <summary>
		///  Performs tasks defined in the created instance associated with freeing, releasing,
		///  or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (this.IsValueCreated)
				this.Value.Dispose();
		}
	}

	public abstract class SingletonStringTypeMappingBase<T> where T : class
	{
		#region Members

		/// <summary>
		/// Static instance. Needs to use lambda expression
		/// to construct an instance (since constructor is private).
		/// </summary>
		private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());

		Dictionary<string, Type> MappingDict = new Dictionary<string,Type>();
		#endregion

		#region Properties

		/// <summary>
		/// Gets the instance of this singleton.
		/// </summary>
		public static T Instance { get { return sInstance.Value; } }

		#endregion

		#region Methods

		/// <summary>
		/// Creates an instance of T via reflection since T's constructor is expected to be private.
		/// </summary>
		/// <returns></returns>
		private static T CreateInstanceOfT()
		{
			return Activator.CreateInstance(typeof(T), true) as T;
		}

		#endregion
		public void ClearAll()
		{
			MappingDict.Clear();
		}
		public KeyValuePair<string, Type> GetMapping(string key)
		{
			return MappingDict.FirstOrDefault(item => string.Compare(item.Key, key, false) == 0);
		}

		public void RemoveKey(string key)
		{
			MappingDict.Remove( key);
		}
		public void AddMapping( string key, Type type)
		{
			MappingDict.Add(key, type);
		}
	}
	
	public abstract class SingletonListBase<T, TType> where T : class where TType :class
	{
		#region Members

		/// <summary>
		/// Static instance. Needs to use lambda expression
		/// to construct an instance (since constructor is private).
		/// </summary>
		private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());
		protected List<TType> Items = new List<TType>();
		#endregion

		#region Properties

		/// <summary>
		/// Gets the instance of this singleton.
		/// </summary>
		public static T Instance { get { return sInstance.Value; } }

		#endregion

		#region Methods

		/// <summary>
		/// Creates an instance of T via reflection since T's constructor is expected to be private.
		/// </summary>
		/// <returns></returns>
		private static T CreateInstanceOfT()
		{
			return Activator.CreateInstance(typeof(T), true) as T;
		}

		#endregion

	}

	public abstract class SingletonClassBase<T> where T : class
	{
		#region Members

		/// <summary>
		/// Static instance. Needs to use lambda expression
		/// to construct an instance (since constructor is private).
		/// </summary>
		private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());
		#endregion

		#region Properties

		/// <summary>
		/// Gets the instance of this singleton.
		/// </summary>
		public static T Instance { get { return sInstance.Value; } }

		#endregion

		#region Methods

		/// <summary>
		/// Creates an instance of T via reflection since T's constructor is expected to be private.
		/// </summary>
		/// <returns></returns>
		private static T CreateInstanceOfT()
		{
			return Activator.CreateInstance(typeof(T), true) as T;
		}

		#endregion
	}


	public abstract class SingletonStringTypeMappingBase<T,tType> where T : class where tType: class
	{
		#region Members

		/// <summary>
		/// Static instance. Needs to use lambda expression
		/// to construct an instance (since constructor is private).
		/// </summary>
		private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());

		protected Dictionary<string, tType> MappingDict = new Dictionary<string, tType>();
		#endregion

		#region Properties

		/// <summary>
		/// Gets the instance of this singleton.
		/// </summary>
		public static T Instance { get { return sInstance.Value; } }

		#endregion

		#region Methods

		/// <summary>
		/// Creates an instance of T via reflection since T's constructor is expected to be private.
		/// </summary>
		/// <returns></returns>
		private static T CreateInstanceOfT()
		{
			return Activator.CreateInstance(typeof(T), true) as T;
		}

		#endregion
		public void ClearAll()
		{
			MappingDict.Clear();
		}

		public KeyValuePair<string, tType> GetMapping(string key)
		{
			return MappingDict.FirstOrDefault(item => string.Compare(item.Key, key, false) == 0);
		}

		public void RemoveKey(string key)
		{
			MappingDict.Remove(key);
		}
		public void AddMapping(string key, tType type)
		{
			MappingDict.Add(key, type);
		}

	}

	public class TEqualityComparer<T> : IEqualityComparer<T>
	{

		private Func<T, T, bool> cmp { get; set; }

		public  TEqualityComparer(Func<T, T, bool> func_cmp)
		{
			this.cmp = func_cmp;
		}
		public virtual bool Equals(T x, T y)
		{
			bool ret = cmp(x, y);
			return ret;
			//return x.Equals(y);
		}

		public virtual int GetHashCode(T obj)
		{
			return obj.GetType().GetHashCode();  //obj.GetHashCode();
		}
	}

	public class TPEqualityComparer<T, TKey> : IEqualityComparer<T> where T:class
	{
		Func<T, TKey> Func_Property;
		Func<TKey, TKey, bool> cmp { get; set; }
		public TPEqualityComparer(Func<TKey, TKey, bool> func_cmp, Func<T, TKey> Property)
		{
			Func_Property = Property;
			cmp = func_cmp;
		}
		public bool Equals(T x, T y)
		{
			TKey v1 = Func_Property(x);
			TKey v2 = Func_Property(y);
			return cmp(v1,v2);
		}

		public int GetHashCode(T obj)
		{
			return Func_Property(obj).GetHashCode();  //obj.GetHashCode();
		}
	}
}