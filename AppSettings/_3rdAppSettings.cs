using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;

namespace AppSettings
{
	public class _3rdAppSettings :IDisposable
	{

		private static readonly LazyDisposable<_3rdAppSettings> sInstance = new LazyDisposable<_3rdAppSettings>(() => new _3rdAppSettings());

		public static _3rdAppSettings Instance { get { return sInstance.Value; } }

		ConcurrentDictionary<Int64, List<_3rdConfig>> SettingList = new ConcurrentDictionary<long,List<_3rdConfig>>();

		List<_3rdConfig> settings { get { return SettingList.Last().Value; } }

		FileLicenseWatcher LicenseWatcher = null;

		internal _3rdAppSettings()
		{
			List<_3rdConfig> model = LoadLicenseFile(Path.Combine(AppSettings.Instance.AppData, "3rd", AppSettings._3rd_Info_File)); 
			AddLicense(model);
			LicenseWatcher = new FileLicenseWatcher( Path.Combine(AppSettings.Instance.AppData, "3rd"), AppSettings._3rd_Info_File);
			LicenseWatcher.OnFileChange += LicenseWatcher_OnFileChange;
		}

		void LicenseWatcher_OnFileChange(object sender, WatcherChangeTypes changeType)
		{
			List<_3rdConfig> model = LoadLicenseFile(Path.Combine(AppSettings.Instance.AppData, "3rd", AppSettings._3rd_Info_File));
			AddLicense(model);
		}

		private void AddLicense(List<_3rdConfig> items)
		{
			if( items == null )
				return;
			Int64 key = DateTime.UtcNow.Ticks;
			SettingList.TryAdd(key, items);
			if (SettingList.Keys.Count > 1)
			{
				List<_3rdConfig> delitem = null;
				Int64 minkey = SettingList.Keys.Min();
				SettingList.TryRemove(minkey, out  delitem);
			}

			
		}

		private List<_3rdConfig> LoadLicenseFile(string filepath)
		{
			if (!File.Exists(filepath))
				return new List<_3rdConfig>();

			try
			{
				string buff = System.IO.File.ReadAllText(filepath);
				return Newtonsoft.Json.JsonConvert.DeserializeObject<List<_3rdConfig>>(buff, new Newtonsoft.Json.JsonSerializerSettings { DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc, NullValueHandling = Newtonsoft.Json.NullValueHandling.Include });
				
			}
			catch (Exception)
			{
				return new List<_3rdConfig>();
			}
			//RSA.DefaultPublicDecryption()
		}

		public _3rdConfig GetConfig(string appid)
		{
			return settings.FirstOrDefault( it => string.Compare(it.AppID, appid, true) == 0);
		}
		private void DoDispose()
		{
			if( LicenseWatcher != null)
			{
				LicenseWatcher.Dispose();
				LicenseWatcher = null;
			}
		}
		public void Dispose()
		{
			DoDispose();
		}
	}

	public class _3rdConfig
	{
		//Application ID
		public string AppID { get; set; }
		//Name of application
		public string Name { get; set; }
		//max connection allow on application id
		public int MaxConnection { get; set; }
		//Number request per minutes
		public int RequestPerMin { get; set; }
		//Alow request
		public List<string> Allow { get; set; }
		//denied request
		public List<string> Denied { get; set; }

		public int RequestAge{get;set;}

		public Nullable<int> UserID{ get;set;}
	}
}
