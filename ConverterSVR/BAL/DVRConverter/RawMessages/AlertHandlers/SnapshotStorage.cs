using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.DVRConverter.RawMessages.AlertHandlers
{
	internal class SnapshotStorage
	{
		const int RECYCLE_WITHOUT_CONFIG = 30;
		const int RECYCLE_INTERVAL = 600; //in seconds = 10 minutes

		private static readonly Lazy<SnapshotStorage> Lazy = new Lazy<SnapshotStorage>(() => new SnapshotStorage());
		private static object _lockObj = new object();

		public static SnapshotStorage Instance
		{
			get { return Lazy.Value; }
		}
		private DateTime LastRecycle = DateTime.Now;

		public void RecycleData(string path)
		{
			lock (_lockObj)
			{
				TimeSpan delta = DateTime.Now - LastRecycle;
				if (delta.TotalSeconds <= RECYCLE_INTERVAL)
					return;

				bool hasRunning = false;
				if (AppSettings.AppSettings.Instance.ImageAlertRecycleSpace > 0)
				{
					hasRunning = true;
					DeleteFilebySpace(path, AppSettings.AppSettings.Instance.ImageAlertRecycleSpace);
				}
				if (AppSettings.AppSettings.Instance.ImageAlertRecycleDays > 0)
				{
					hasRunning = true;
					DeleteFilebyDate(path, AppSettings.AppSettings.Instance.ImageAlertRecycleDays);
				}

				if (!hasRunning)
				{
					DeleteFilebyDate(path, RECYCLE_WITHOUT_CONFIG);
				}
				LastRecycle = DateTime.Now;
			} //lock
		}

		private void DeleteFilebySpace(string path, int free_space)
		{
			long freespace = GetFreeDisk(path);
			if (freespace > (long)free_space || !Directory.Exists(path))
				return;
			DirectoryInfo dbdir = new DirectoryInfo(path);

			DirectoryInfo[] dirinfos = dbdir.GetDirectories();
			IEnumerable<FileInfo> Files = null;
			long clean_up_length = 0;
			bool stop_delete = false;
			int complete_count = 0;
			while (!stop_delete)
			{
				clean_up_length = 0;
				complete_count = 0;
				foreach (DirectoryInfo dinfo in dirinfos)
				{
					Files = dinfo.GetFiles().OrderBy(file => file.CreationTime).Take(100);
					if (Files == null)
						continue;
					clean_up_length += DeleteFiles(Files.ToList(), out complete_count);
					System.Threading.Thread.Sleep(100);
				}
				freespace += (clean_up_length / 1024 / 1024);
				stop_delete = complete_count == 0 || freespace > free_space;
				System.Threading.Thread.Sleep(100);
			}

		}

		private void DeleteFilebyDate(string path, int last_date)
		{
			if (!Directory.Exists(path))
				return;

			DateTime limit_date = DateTime.Now.AddDays(1 - Math.Abs(last_date));// ADD 1 for current date. 
			DirectoryInfo dbdir = new DirectoryInfo(path);
			DirectoryInfo[] dirinfos = dbdir.GetDirectories();
			IEnumerable<FileInfo> Files = null;
			int delete_Count = 0;
			int all_delete_Count = 0;
			bool stop_delete = false;
			while (!stop_delete)
			{
				all_delete_Count = 0;
				foreach (DirectoryInfo dinfo in dirinfos)
				{
					Files = dinfo.GetFiles().Where(file => file.CreationTime.Ticks < limit_date.Ticks);
					if (Files == null)
						continue;
					DeleteFiles(Files.ToList(), out delete_Count);
					all_delete_Count += delete_Count;
					System.Threading.Thread.Sleep(100);
				}
				stop_delete = all_delete_Count == 0;
				System.Threading.Thread.Sleep(50);
			}
		}
		private long DeleteFiles(List<FileInfo> files, out int complete_Count)
		{
			complete_Count = 0;
			long free_space = 0;
			while (files.Count > 0)
			{
				try
				{
					files[0].Delete();
					free_space += files[0].Length;
					complete_Count++;
				}
				catch (Exception) { }

				files.RemoveAt(0);
				System.Threading.Thread.Sleep(10);
			}

			return free_space;
		}
		private long GetFreeDisk(string sPath)
		{
			long lDiskFree = 999;
			try
			{
				string sRoot = Directory.GetDirectoryRoot(sPath);
				DriveInfo dIf = new DriveInfo(sRoot);
				lDiskFree = (dIf.AvailableFreeSpace / 1024) / 1024;//Mb;
			}
			catch (Exception) {}
			return lDiskFree;
		}
	}
}
