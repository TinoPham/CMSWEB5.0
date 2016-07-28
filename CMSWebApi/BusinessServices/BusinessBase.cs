using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CMSWebApi.DataModels;
using CMSWebApi.Cache.Caches;
using CMSWebApi.Cache;
using CMSWebApi.Cache.EntityCaches;
using CMSWebApi.Utils;
using System.Security.AccessControl;
using System.Security.Principal;
namespace CMSWebApi.BusinessServices
{
	public abstract class BusinessBase<T> where T: class
	{
		readonly FileManager _FileManager = new FileManager();

		protected FileManager FileManager
		{
			get { return _FileManager; }
		}

		public T DataService { get; set;}
		
		public DataServices.ServiceBase ServiceBase{ get{ return this.DataService as DataServices.ServiceBase;}}

		public CultureInfo Culture { get; set; }

		protected string LocalPath{ get; set;}

		public CMSWebApi.DataModels.UserContext Userctx{ get; set;}

		protected DateTime StartTimeOfDate(DateTime date)
		{
			return date.Date;
		}
		protected DateTime EndTimeOfDate(DateTime date)
		{
			return date.Date.AddDays(1).AddMilliseconds(-10);//minus 10 for correct SQL value
		}

		protected IEnumerable<UserSiteDvrChannel> UserSites(ServiceInterfaces.IUsersService Iuser, UserContext Userctx)
		{
			if (Iuser == null || Userctx == null)
				return new List<UserSiteDvrChannel>();

			IEnumerable<UserSiteDvrChannel> uSites = Iuser.GetDvrbyUser<UserSiteDvrChannel>(Userctx.ID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			//if( AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber > 0)
			//{
			//	return uSites.Take(AppSettings.AppSettings.Instance.Licenseinfo.DVRNumber );
			//}
			return uSites;
		}
		protected Task<IEnumerable<UserSiteDvrChannel>> UserSitesAsync(ServiceInterfaces.IUsersService Iuser, UserContext Userctx)
		{
			if( Iuser == null || Userctx == null)
				return Task.FromResult<IEnumerable<UserSiteDvrChannel>>(new List<UserSiteDvrChannel>());

			Task<IEnumerable<UserSiteDvrChannel>> uSites = Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(Userctx.ID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			return uSites;
			//return UserSitesBySiteIDsAsync(Iuser, Userctx, null);
		}
		protected Task<IEnumerable<UserSiteDvrChannel>> UserSitesAsync(int userid)
		{
			
			ServiceInterfaces.IUsersService Iuser = new DataServices.UsersService(ServiceBase);
			Task<IEnumerable<UserSiteDvrChannel>> uSites = Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(userid, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			return uSites;
			//return UserSitesBySiteIDsAsync(Iuser, Userctx, null);
		}

		#region mark
		/*
		protected bool WriteFile( string fpath, MemoryStream mem, bool overwrite = true)
		{

			if (mem == null || mem.Length == 0 || string.IsNullOrEmpty(fpath))
				return false;
			return WriteFile(fpath,mem.ToArray(), overwrite);
		}
		
		protected bool WriteFile( string fpath, byte[] mem, bool overwrite = true)
		{
		  if( mem == null || mem.Length == 0 || string.IsNullOrEmpty(fpath))
			return false;
			string dir = Path.GetDirectoryName(fpath);
			if(!CMSWebApi.Utils.Utilities.CreateDir(dir))
				return false;
			if( File.Exists( fpath) && overwrite == false)
				return true;

			FileStream fs = null;
			try
			{
				fs = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
				Task tsk = fs.WriteAsync(mem, 0, (int)mem.Length);
				Task.WaitAll(tsk);

				return fs.Length == mem.Length;
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				if( fs != null)
				{
					fs.Close();
					fs.Dispose();
					fs = null;
				}
			}
		}

		protected bool DeleteFolder(string path)
		{
			try
			{
				if (Directory.Exists(path))
				{
					DirectoryInfo dir = new DirectoryInfo(path);			
					dir.Delete(true);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}

		}
		*/
		#endregion
		protected IEnumerable<Tentity> ResolveEntityCache<Tentity>() where Tentity : class
		{
			IEntityCache<Tentity> ICache = BackgroundTaskManager.Instance.ResolveEntityCache<Tentity>();
			return ICache == null? null : ICache.Results;
		}
		
		protected ICache<Tcachemodel> ResolveCache<Tcachemodel>( DateTime sdate, DateTime edate ) where Tcachemodel : CMSWebApi.DataModels.DashBoardCache.CacheModelBase
		{
			ICache<Tcachemodel> cache = BackgroundTaskManager.Instance.GetCache<Tcachemodel>();
			if( cache == null)
				return cache;
			ICache<Tcachemodel> icache = cache.Status == CacheStatus.Ready? cache : null;
			if( icache == null)
				return icache;
			return icache.ValidData(sdate, edate)? icache : null;
		}

		protected Task<IEnumerable<UserSiteDvrChannel>> UserSitesBySiteIDsAsync(ServiceInterfaces.IUsersService Iuser, int userID, IEnumerable<int> lsSiteIDs)
		{
			if (Iuser == null || userID == 0 || lsSiteIDs == null || !lsSiteIDs.Any())
			{
				return Task.FromResult<IEnumerable<UserSiteDvrChannel>>(new List<UserSiteDvrChannel>());
			}

			return Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(userID, lsSiteIDs, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
		}
		protected Task<IEnumerable<UserSiteDvrChannel>> UserSitesBySiteIDsAsync(ServiceInterfaces.IUsersService Iuser, UserContext Userctx, IEnumerable<int> lsSiteIDs)
		{
			//if (Iuser == null || Userctx == null)
			//	return Task.FromResult<IEnumerable<UserSiteDvrChannel>>(new List<UserSiteDvrChannel>());
			if (Iuser == null || Userctx == null || lsSiteIDs == null || !lsSiteIDs.Any())
			{
				return Task.FromResult<IEnumerable<UserSiteDvrChannel>>(new List<UserSiteDvrChannel>());
				//return Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(Userctx.ID, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			}
			else
			{
				return Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(Userctx.ID, lsSiteIDs, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			}
			//Task<IEnumerable<UserSiteDvrChannel>> uSites = Iuser.GetDvrbyUserAsync<UserSiteDvrChannel>(Userctx.ID, lsSiteIDs, item => new UserSiteDvrChannel { KChannel = item.KChannel, KDVR = item.KDVR, PACID = item.PACID, siteKey = item.siteKey, UserID = item.UserID });
			//return uSites;
		}
	}

	public sealed class FileManager
	{
		const int DEFAULT_BUFF_SIZE = 4096;
		#region Directory
		public IEnumerable<String> DirGetFiles(string path, string pattern = null, bool recursive = true)
		{
			return DirGetFileInfos(path, path, recursive).Select( it => it.FullName);
		}
		
		public IEnumerable<FileInfo> DirGetFileInfos(string path, string pattern = null, bool recursive = true)
		{
			DirectoryInfo dinfo = DirInfoExist(path);
			if( dinfo == null || !dinfo.Exists)
			return Enumerable.Empty<FileInfo>();
			try
			{
				return dinfo.GetFiles(string.IsNullOrEmpty(pattern)? "*" : pattern, recursive == false ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
			}
			catch(Exception)
			{
				return Enumerable.Empty<FileInfo>();
			}
		}
		
		public Task<bool> DirDeleteAsync(string path, bool recursive = true)
		{
			return Task.Run(() => DirDelete(path, recursive));
		}
	
		public bool DirDelete( string path, bool recursive = true)
		{
			if( string.IsNullOrEmpty(path) || !Directory.Exists(path) || !Path.IsPathRooted(path))
				return true;
			try
			{
				Directory.Delete(path, recursive);
				return true;
			}
			catch(IOException){}
			catch(UnauthorizedAccessException){}
			catch(ArgumentException){}
			return false;
		}
		
		public bool DirExist( string path)
		{
			if( string.IsNullOrEmpty(path))
				return false;
			return Directory.Exists(path);
		}
		
		public DirectoryInfo DirInfoExist(string path)
		{
			return !DirExist(path)? null : new DirectoryInfo(path);
		}

		public DirectoryInfo DirCreate(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			try
			{
				DirectoryInfo dinfo = Directory.Exists(path)? new DirectoryInfo(path) : Directory.CreateDirectory(path);
				//DirGrantAccess(path);
				return dinfo.Exists? dinfo : null;
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			catch (ArgumentException) { }
			catch (NotSupportedException) { }
			return null;
			
		}

		public DirectoryInfo DirRename( string oldPath, string newpath )
		{
			if(!DirExist(oldPath))
				return null;
			if( DirCreate(newpath) == null )
				return null;
			try
			{
				Directory.Move( oldPath, newpath);
				return new System.IO.DirectoryInfo(newpath);
			}
			catch(IOException){}
			catch(UnauthorizedAccessException){}
			catch(ArgumentException){}
			return null;
		}

		//public bool DirGrantAccess(string path)
		//{
		//	DirectoryInfo dInfo = new DirectoryInfo(path);
		//	DirectorySecurity dSecurity = dInfo.GetAccessControl();
		//	dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
		//		FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, 
		//		PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
		//	dInfo.SetAccessControl(dSecurity);
		//	return true;
		//}

		#endregion
		#region Files

		private FileStream OpenReadStream( string path, bool usesync = false)
		{
			FileStream sourceStream = null;
			if( !FileExist(path) )
				return null;

			try
			{
				sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: DEFAULT_BUFF_SIZE, useAsync: usesync);
			}
			catch(Exception)
			{
				sourceStream = null;
			}
			return sourceStream;
		}
		
		private FileStream OpenWriteStream(string path, bool overwrite, bool usesync = false)
		{
			FileStream sourceStream = null;
			if (FileExist(path) && overwrite == false)
				return null;

			try
			{
				sourceStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize: DEFAULT_BUFF_SIZE, useAsync: usesync);
			}
			catch (Exception)
			{
				sourceStream = null;
			}
			return sourceStream;
		}

		public Task<bool> FileDeleteAsync(string path)
		{
			return Task.Run( () => FileDelete(path));
		}
		
		public bool FileDelete(string path)
		{
			if( string.IsNullOrEmpty(path) || !File.Exists(path))
				return true;
			try
			{
				File.Delete(path);
			}
			catch(ArgumentException){}
			
			catch(DirectoryNotFoundException){}
			catch(IOException){}
			catch(NotSupportedException){}
			catch(UnauthorizedAccessException){}
			return false;

		}

		public bool FileExist(string path)
		{
			if( string.IsNullOrEmpty(path))
				return false;
			return File.Exists(path);
		}

		public string FileRandom()
		{
			Guid guid = Guid.NewGuid();
			return Commons.Utils.ByteArrayToHexString( guid.ToByteArray()).ToUpper();
		}
		
		public FileInfo FileInfoExist(string path)
		{
			return !FileExist(path)? null : new FileInfo(path);
		}

		public string FileName(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			return Path.GetFileName(path);
		}

		public string FileNameWithoutExtension(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			return Path.GetFileNameWithoutExtension(path);
		}

		public string FileExtension(string path)
		{
			if(string.IsNullOrEmpty(path))
				return null;
			return Path.GetExtension(path);
		}

		public async Task<byte []> ReadFileAsync(string path)
		{
			FileStream sourceStream = null;
			byte[]buff = null;
			try
			{
				sourceStream = OpenReadStream(path, true);
				 //new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: DEFAULT_BUFF_SIZE, useAsync: true);
				if(sourceStream == null)
					return null;

				buff = new byte[ sourceStream.Length];
				await sourceStream.ReadAsync(buff, 0, buff.Length);
				return buff;

			}
			catch(Exception)
			{
				return buff;
			}
			finally{
				if(sourceStream != null)
				{
					sourceStream.Close();
					sourceStream.Dispose();
					sourceStream = null;
				}
			}
			
		}

		public byte[] ReadFile(string path)
		{
			if( !FileExist(path) )
				return null;
			FileStream fs = null;
			byte[]buff = null;
			try
			{
				fs = OpenReadStream(path, false); //new FileStream(path, FileMode.Open,FileAccess.Read, FileShare.Read);
				if( fs == null)
					return null;
				buff = new byte[ fs.Length];
				fs.Read(buff, 0, buff.Length);
			}
			catch(Exception){}
			finally{
				if( fs != null)
				{
					fs.Close();
					fs.Dispose();
					fs = null;
				}
			}
			return buff;
		}

		public bool FileWrite( string path, byte[] buff, bool overwrite = true)
		{
			if( buff == null || buff.Length == 0)
				return false;

			if(FileExist(path) && overwrite == false)
				return false;

			FileStream sourceStream = null;
			try
			{
				string dirpath = Path.GetDirectoryName(path);
				DirectoryInfo dinfo = DirCreate(dirpath);
				if(!dinfo.Exists)
					return false;

				//DirGrantAccess(dirpath);
				sourceStream = OpenWriteStream(path, overwrite, false); //new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: DEFAULT_BUFF_SIZE, useAsync: false);
				int index = 0;
				int count = 0;
				while(index < buff.Length)
				{
					count = Math.Min(DEFAULT_BUFF_SIZE, buff.Length - index);
					sourceStream.Write( buff, index, count);
					index += count;
				}
				return true;

			}
			catch(Exception){}
			finally{
				if( sourceStream != null)
				{
					sourceStream.Close();
					sourceStream.Dispose();
					sourceStream = null;
				}
			}
			return false;
		}

		public async Task<bool> FileWriteAsync(string path, byte [] buff, bool overwrite = true)
		{
			if(buff == null || buff.Length == 0)
				return false;

			FileStream sourceStream = OpenWriteStream(path, overwrite, false);
			if( sourceStream == null)
				return false;
			try
			{
				//if (Directory.Exists(path)) 
				//{
				//	DirGrantAccess(path);
				//}
				await sourceStream.WriteAsync(buff, 0, buff.Length);
				return true;

			}
			catch(Exception){}
			finally{
				if( sourceStream != null)
				{
					sourceStream.Close();
					sourceStream.Dispose();
					sourceStream = null;
				}
			}
			return false;
		}

		public bool FileCopy(string srcFile, string destFile, bool overwite = true)
		{
			if (string.IsNullOrEmpty(srcFile) || string.IsNullOrEmpty(destFile) || !File.Exists(srcFile))
			{
				return false;
			}

			try
			{
				File.Copy(srcFile, destFile, overwite);
				return File.Exists(destFile);
			}
			catch(Exception){}
			return false;
		}
		#endregion
	}
}
