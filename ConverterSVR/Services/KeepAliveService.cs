using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertMessage;
using System.IO;
using System.Text.RegularExpressions;
using Commons;
using System.Xml;
namespace ConverterSVR.Services
{
	public class KeepAliveService : IDisposable
	{
		const string Full_date_FileKey = "yyyymmddHHmmssfff";
		public const string STR_X64 = "x64";
		public const string STR_X86 = "x86";
		private static readonly LazyDisposable<KeepAliveService> sInstance = new LazyDisposable<KeepAliveService>(() => new KeepAliveService());
		public static KeepAliveService Instance { get { return sInstance.Value; } }

		DVRKeepAlive KeepaliveManager = DVRKeepAlive.Instance;
		FileVersionWatcher FileVersionManager = FileVersionWatcher.Instance;
		public void Dispose()
		{
			KeepaliveManager.Dispose();
			FileVersionManager.Dispose();
		}
		private KeepAliveService()
		{

		}

		public MessageKeepAlive DVRKeepAliveMessage(int? kdvr, Int64 token, string version, bool isnewdvr)
		{
			MessageKeepAlive msg = KeepaliveManager.KeepAliveMessage(kdvr, token, isnewdvr);
			
			if(msg == null)
				msg = new MessageKeepAlive();

			msg.LastVersion = LastVersionName();
			msg.DataReset = false;

			return msg;
		}

		public void GetInstallversion( string versioname, string platform, out string installpath, out string Description)
		{
			installpath = string.Empty;
			Description = string.Empty;
			if( string.IsNullOrEmpty( versioname))
				return;
			IEnumerable<Model.VersionModel> vers = FileVersionManager.VerionList;
			Model.VersionModel ver = vers.FirstOrDefault( it => string.Compare(it.Name, versioname, true) == 0);
			if( ver == null)
				return;
			installpath = Path.Combine(FileVersionManager.Dir, ver.InstallName);
			if( string.Compare(platform, STR_X64) == 0)
			{
				installpath = Getx64Filename( installpath);
			}
			Description = Path.Combine(FileVersionManager.Dir, ver.Description);
			if(!File.Exists(installpath))
				installpath = string.Empty;
			if( !File.Exists(Description) )
				Description = string.Empty;
		}
		
		public string LastVersionName()
		{
			IEnumerable<Model.VersionModel> vers = FileVersionManager.VerionList;
			if(!vers.Any())
				return null;
			return vers.OrderByDescending(it => it.Version).First().Name;
			
		}

		public string Getx64Filename( string path)
		{
			if( string .IsNullOrEmpty(path))
				return null;
			FileInfo f = new FileInfo(path);
			string name = Path.GetFileNameWithoutExtension(f.Name);
			string x64name = name + "_" + STR_X64 + f.Extension;
			string x64path = Path.Combine(f.DirectoryName, x64name);
			if( File.Exists(x64path))
				return x64path;

			x64name = name + "." + STR_X64 + f.Extension;
			x64path = Path.Combine(f.DirectoryName, x64name);
			if (File.Exists(x64path))
				return x64path;
			x64name = name + "-" + STR_X64 + f.Extension;
			x64path = Path.Combine(f.DirectoryName, x64name);
			if(File.Exists(x64path))
				return x64path;
			x64name = name + STR_X64 + f.Extension;
			x64path = Path.Combine(f.DirectoryName, x64name);
			if (File.Exists(x64path))
				return x64path;
			return string.Empty;
		}
	}


	internal class DVRKeepAlive: IDisposable
	{
		static readonly string DefaultPath = Path.Combine(AppSettings.AppSettings.Instance.AppData, Consts.STR_Converter, Consts.STR_KeepAlive);

		private static readonly LazyDisposable<DVRKeepAlive> sInstance = new LazyDisposable<DVRKeepAlive>(() => new DVRKeepAlive(DefaultPath));
		public static DVRKeepAlive Instance { get { return sInstance.Value; } }

		protected const string KeepAlive_ConfigFile = "KeepAlive.json";

		readonly object locker = new object();

		ConvertMessage.MessageKeepAlive _DVRDefaultKeepAlive;

		private ConvertMessage.MessageKeepAlive DVRDefaultKeepAlive{ get { lock(locker){ return _DVRDefaultKeepAlive;}} set{  lock(locker){ _DVRDefaultKeepAlive = value;}}}
		protected string Dir;

		internal DVRKeepAlive(string path)
		{
			Dir = path;
			Initialize();
			
		}

		private void Initialize()
		{
			FileInfo finfo = KeepAliveFileInfo( Dir, (int?)null);
			if( finfo == null)
			{
				_DVRDefaultKeepAlive = MessageKeepAlive.Default();
				finfo = CreateDefaultKeepAlive(_DVRDefaultKeepAlive);
				if (finfo != null)
					_DVRDefaultKeepAlive.KeepAliveToken = finfo.LastWriteTimeUtc.Ticks;
			}
			else
			{
				_DVRDefaultKeepAlive = ParserKeepAlive( finfo);
				if( _DVRDefaultKeepAlive != null)
					_DVRDefaultKeepAlive.KeepAliveToken = Math.Max(finfo.LastWriteTimeUtc.Ticks,  finfo.CreationTimeUtc.Ticks );
				else
				{
					_DVRDefaultKeepAlive = MessageKeepAlive.Default();
					_DVRDefaultKeepAlive.KeepAliveToken = DateTime.UtcNow.Ticks;
				}
			}
		}

		public ConvertMessage.MessageKeepAlive KeepAliveMessage(int? kdvr, Int64 token, bool isnewdvr)
		{
			FileInfo fileinfo = KeepAliveFileInfo(Dir, kdvr);

			if( fileinfo == null)
			{
				MessageKeepAlive tmp_msg = DVRDefaultKeepAlive.Clone();
				if(token <= 0)
				{
					if(!isnewdvr)
						tmp_msg.ConvertInfo = null;//only return converter info when dvr connect @ 1st time

					return tmp_msg;
				}
				else
				{
					if(tmp_msg.KeepAliveToken == token)
						return null;

					tmp_msg.ConvertInfo = null;
					return tmp_msg;
				}
			}
			else
			{
				long lasttoken = Math.Max(fileinfo.LastWriteTimeUtc.Ticks, fileinfo.CreationTimeUtc.Ticks);
				if (token <= 0 || token != lasttoken)
				{
					ConvertMessage.MessageKeepAlive msg = ParserKeepAlive(fileinfo);
					
					if (msg == null)
					{
						msg = DVRDefaultKeepAlive.Clone();
						if(!isnewdvr)
							msg.ConvertInfo = null;
					}
					else
						msg.KeepAliveToken = lasttoken;

					return msg;
				}
			}
			

			return null;
		}

		public virtual void Dispose()
		{
			
		}

		private FileInfo KeepAliveFileInfo(string dir, int? kdvr)
		{
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
				return null;

			string filepath;

			if (kdvr.HasValue)
				filepath = Path.Combine(dir, kdvr.Value.ToString(), KeepAlive_ConfigFile);
			else
				filepath = Path.Combine(dir, Consts.STR_Default, KeepAlive_ConfigFile);
			if (!File.Exists(filepath))
				return null;
			return new FileInfo( filepath);
		}
		
		private ConvertMessage.MessageKeepAlive ParserKeepAlive( FileInfo fileinfo)
		{
			if( fileinfo == null || !fileinfo.Exists)
				return null;
			try
			{
				string buff = System.IO.File.ReadAllText(fileinfo.FullName);
				return ConvertMessage.MessageKeepAlive.FromJsonString( buff);
			}
			catch(Exception){ return null;}
			
		}

		private FileInfo CreateDefaultKeepAlive(ConvertMessage.MessageKeepAlive data)
		{
			string dirpath = Path.Combine(Dir, Consts.STR_Converter, Consts.STR_Default);
			if(!Directory.Exists(dirpath))
			{
				try
				{
					 Directory.CreateDirectory( dirpath);
				}
				catch(Exception){ return null;}
			}
			
			try
			{
				string buff = data.toJsonString(false);
				string fpath = Path.Combine(dirpath, KeepAlive_ConfigFile);
				System.IO.File.WriteAllText(fpath, buff);
				return new FileInfo(fpath);
			} catch(Exception){ return null;}

			

		}
		
		
	}
	 
	internal class FileVersionWatcher : IDisposable
	{
		static readonly string VersionPath = Path.Combine(AppSettings.AppSettings.Instance.AppData, Consts.STR_Converter, Consts.STR_Version);
		private static readonly LazyDisposable<FileVersionWatcher> sInstance = new LazyDisposable<FileVersionWatcher>(() => new FileVersionWatcher(VersionPath));
		public static FileVersionWatcher Instance { get { return sInstance.Value; } }
		const string VersionFileList = "Version.xml";
		
		public string Dir{ get;private set;}

		BlockingCollection<Model.VersionModel>Versions = new BlockingCollection<Model.VersionModel>();

		FileSystemWatcher FileWatcher;
		public IEnumerable<Model.VersionModel> VerionList { get { return Versions.AsEnumerable(); } }
		byte Change_Count = 0;
		public void Dispose()
		{
			DoDispose();
		}
	
		internal FileVersionWatcher( string path)
		{
			Dir = path;
			LoadVersions(Dir);
			//string fileverions = Path.Combine(Dir, VersionFileList);
			InitFileverionsWatcher(Dir, VersionFileList);
		}

		private void InitFileverionsWatcher( string path, string filename)
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
			if( e.ChangeType == WatcherChangeTypes.Changed)
			{
				if( Change_Count %2 == 1)
				{
					Change_Count = 0;
					RemoveAllVersions();
					LoadVersions(Dir);
				}
				else
					Change_Count ++;
			}
		}

		void FileWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if( e.ChangeType == WatcherChangeTypes.Created)
			{
				RemoveAllVersions();
				LoadVersions(Dir);
			}
		}

		void FileWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if( e.ChangeType == WatcherChangeTypes.Renamed)
			{
				RemoveAllVersions();
				LoadVersions(Dir);
			}
		}

		void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if( e.ChangeType == WatcherChangeTypes.Deleted)
			{
				RemoveAllVersions();
				
			}
		}

		private void LoadVersions( string dir)
		{
			IEnumerable<Model.VersionModel> versions = LoadFileVersions(dir);
			IEnumerable<Model.VersionModel> newversions = versions.Where(it => !Versions.Any(v => string.Compare(it.Name, v.Name, true) == 0));
			AddVersions( newversions);

		}

		private IEnumerable<Model.VersionModel> LoadFileVersions( string dir)
		{
			string fpath = Path.Combine(dir, VersionFileList);
			if( string.IsNullOrEmpty( fpath) || !File.Exists(fpath))
				goto LABEL_EMPTY;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( fpath);
				XmlNode root = doc.DocumentElement;
				Model.VersionModel model = null;
				List<Model.VersionModel> models = new List<Model.VersionModel>();
				foreach( XmlNode child in root.ChildNodes )
				{
					model = XmlNodetoModel(child);
					if(model== null)
						continue;
					models.Add(model);
				}
				return models;
			}
			catch (Exception) { goto LABEL_EMPTY; }

			LABEL_EMPTY:
				return Enumerable.Empty<Model.VersionModel>();
		}

		private Model.VersionModel XmlNodetoModel(XmlNode node)
		{
			if( node == null)
				return null;
			string name = XMLUtils.XMLAttributeValue(node, Model.VersionModel.STR_NAME);
			string installname = XMLUtils.XMLAttributeValue(node, Model.VersionModel.STR_INSTALLNAME);
			string description = XMLUtils.XMLAttributeValue(node, Model.VersionModel.STR_DESCRIPTION);
			if( string.IsNullOrEmpty( name) || string.IsNullOrEmpty(installname))
				return null;
			return new Model.VersionModel{ Name = name, Description = description, InstallName = installname};
		}
		private void AddVersions(IEnumerable<Model.VersionModel> items)
		{
			foreach(Model.VersionModel it in items)
				Versions.TryAdd( it);
				
		}
		private void RemoveAllVersions()
		{
			while(Versions.Any())
				Versions.Take();

		}
		private void RemoveVersion(Model.VersionModel item)
		{
			if( item == null)
				return;
			Versions.TryTake(out item);
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
