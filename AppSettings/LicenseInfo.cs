using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons;
using Cryptography;
using LicenseInfo.Models;
using LicenseInfolib = LicenseInfo;


namespace AppSettings
{
	internal class LicenseInfo : IDisposable
	{
		private static readonly LazyDisposable<LicenseInfo> sInstance = new LazyDisposable<LicenseInfo>(() => new LicenseInfo());

		public static LicenseInfo Instance { get { return sInstance.Value; } }
		
		ConcurrentDictionary<Int64, LicenseModel> LicensList = new ConcurrentDictionary<long,LicenseModel>();

		public LicenseModel License{ get{ return LicensList.Last().Value; }}

		FileLicenseWatcher LicenseWatcher = null;

		internal LicenseInfo()
		{
			LicenseModel model = LoadLicenseFile(Path.Combine(AppSettings.Instance.AppData, AppSettings.License_Info_File)); 
			AddLicense(model);
			LicenseWatcher = new FileLicenseWatcher(AppSettings.Instance.AppData, AppSettings.License_Info_File);
			LicenseWatcher.OnFileChange += LicenseWatcher_OnFileChange;
		}

		void LicenseWatcher_OnFileChange(object sender, WatcherChangeTypes changeType)
		{
			LicenseModel model = LoadLicenseFile(Path.Combine(AppSettings.Instance.AppData, AppSettings.License_Info_File));
			AddLicense(model);
		}

		private void AddLicense(LicenseModel item)
		{
			if( item == null )
				return;
			Int64 key = DateTime.UtcNow.Ticks;
			LicensList.TryAdd(key, item);
			if( LicensList.Keys.Count > 1)
			{
				LicenseModel delitem = null;
				Int64 minkey = LicensList.Keys.Min();
				LicensList.TryRemove( minkey,out  delitem);
			}

			
		}

		private LicenseModel LoadLicenseFile(string filepath)
		{
			if (!File.Exists(filepath))
				return new LicenseModel();

			try
			{
				byte [] raw_buff = null;
				using (BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open)))
				{
					raw_buff = new byte [reader.BaseStream.Length];
					reader.Read(raw_buff, 0, raw_buff.Length);
				}

				string strbuff = Cryptography.Rijndael.DefaultDecryptStringFromBytes(raw_buff);
				if (strbuff == null)
					return new LicenseModel();
				LicenseModel model = LicenseInfolib.LicenseInfo.Instance.ParserModel(strbuff); //LicenseInfo.LicenseInfo.Instance.ParserModel(Encoding.UTF8.GetString(buff));
				return model == null ? new  LicenseModel() : model;
			}
			catch (Exception)
			{
				return new LicenseModel();
			}
			//RSA.DefaultPublicDecryption()
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

	internal class FileLicenseWatcher : IDisposable
	{
		internal delegate void FileChnage(object sender, WatcherChangeTypes changeType);

		internal event FileChnage OnFileChange;
		//private static readonly LazyDisposable<FileLicenseWatcher> sInstance = new LazyDisposable<FileLicenseWatcher>(() => new FileLicenseWatcher(AppSettings.Instance.AppData));
		//public static FileLicenseWatcher Instance { get { return sInstance.Value; } }

		FileSystemWatcher FileWatcher;

		private readonly string File_Path;

		byte Change_Count = 0;

		public void Dispose()
		{
			DoDispose();
		}

		internal FileLicenseWatcher(string path, string filename)
		{
			File_Path = Path.Combine(path, filename);

			InitFileverionsWatcher(path, filename);
		}

		private void InitFileverionsWatcher(string path, string filename)
		{
			FileWatcher = new FileSystemWatcher(path, filename);
			FileWatcher.Deleted += FileWatcher_Deleted;
			FileWatcher.Renamed += FileWatcher_Renamed;
			FileWatcher.Created += FileWatcher_Created;
			FileWatcher.Changed += FileWatcher_Changed;
			FileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;
			FileWatcher.IncludeSubdirectories = false;
			FileWatcher.EnableRaisingEvents = true;
		}

		void FileWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Changed)
			{
				if (Change_Count % 2 == 1)
				{
					Change_Count = 0;
					OnChangeEvent(e.ChangeType);
				}
				else
					Change_Count++;
			}
		}

		void FileWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Created)
			{
				OnChangeEvent(e.ChangeType);
			}
		}

		void FileWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Renamed)
			{
				OnChangeEvent(e.ChangeType);
			}
		}

		void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Deleted)
			{
				OnChangeEvent(e.ChangeType);

			}
		}

		private void OnChangeEvent(WatcherChangeTypes changeType)
		{
			if (OnFileChange != null)
				OnFileChange(this, changeType);

		}

		private void DoDispose()
		{
			if (FileWatcher != null)
			{
				FileWatcher.EnableRaisingEvents = false;
				FileWatcher.Deleted -= FileWatcher_Deleted;
				FileWatcher.Renamed -= FileWatcher_Renamed;
				FileWatcher.Created -= FileWatcher_Created;
				FileWatcher.Dispose();
				FileWatcher = null;
			}
		}
	}
}
